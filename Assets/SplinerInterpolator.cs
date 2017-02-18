using UnityEngine;
using System.Collections;
namespace Softdrink{
	public class SplinerInterpolator : MonoBehaviour {

		enum SplinerInterpolationMode{
			ForwardOnly,
			PingPong,
			Count,
			Cycle,

			Manual,
		}

		[HeaderAttribute("Target Spline")]

		// Reference to the target Spline
		[TooltipAttribute("The Spline to follow.")]
		public SplinerBase targetSpline = null;

		[HeaderAttribute("Behavior")]

		[SerializeField]
		[TooltipAttribute("How does this Interpolator behave? \nForward Only - stops when reaching the end of the Spline. \nPingPong - Never stops; reverses direction when it reaches an endpoint. \nCount - increases by one each time it travels to an endpoint, and stops at a certain count. \nCycle - travels forward along the Spline until it reaches the end, then returns to the start point instantly.\nManual - Interpolation progress is controlled by another Script, using the Evaulate() method.")]
		private SplinerInterpolationMode mode = SplinerInterpolationMode.ForwardOnly;

		[SerializeField]
		[TooltipAttribute("If the Interpolator is in 'Count' mode, how many times should it reach an endpoint before stopping?")]
		private int desiredCount = 1;
		private int currentCount = 0;

		[SerializeField]
		[TooltipAttribute("Does the Interpolator move 'backwards' relative to the Spline's natural direction?")]
		private bool reverseDirection = false;

		[HeaderAttribute("Movement")]

		[SerializeField]
		[TooltipAttribute("How fast does the Interpolator move? \nThis is measured in seconds, a value of 1 means it will take one second to traverse the entire Spline.")]
		private float moveSpeed = 1.0f;

		[SerializeField]
		[TooltipAttribute("How fast foes the Interpolator move in frame-dependent mode? \nThis is measured in frames; a value of 60 means it will take 60 frames to traverse the entire Spline.")]
		private int moveSpeedFrames = 60;

		[SerializeField]
		[TooltipAttribute("Is the Interpolator movement unaffected by framerate, or framerate dependent?")]
		private bool frameBasedMovement = true;

		[SerializeField]
		[TooltipAttribute("Should the Interpolator align with the spline's direction?")]
		private bool alignRotation = true;

		[SerializeField]
		[Range(0.001f,0.999f)]
		[TooltipAttribute("How far ahead (in 0...1 range of t) should the 'look point' be for aligning rotation?")]
		private float alignLookDistance = 0.1f;

		[HeaderAttribute("Bounds and Precision")]

		[SerializeField]
		[Range(0f, 0.99f)]
		[TooltipAttribute("At what point along the Spline does the Interpolator start?")]
		private float startPoint = 0.0f;

		[SerializeField]
		[Range(0.01f, 1f)]
		[TooltipAttribute("At what point along the Spline does the Interpolator end?")]
		private float endPoint = 1.0f;

		[SerializeField]
		[Range(0.00001f, 0.001f)]
		[TooltipAttribute("At what distance from an end point is the Interpolator considered to have reached it, and it can turn around if desired?")]
		private float endpointPrecision = 0.001f;

		

		// Reference to the Transform of the attached GameObject
		private Transform self;
		// Internal bool
		private bool isMovingForwards = true;

		[HeaderAttribute("Preview")]

		[SerializeField]
		[Range(0f, 1f)]
		[TooltipAttribute("A preview value, used to check the movement of the Interpolator along the Spline.")]
		private float previewT = 0f;

		// The actual progress of the curve traversal
		private float progress = 0f;
		// How much to increase the progress
		private float step = 0f;


		void Awake () {
			// Assign self Transform
			self = gameObject.GetComponent<Transform>() as Transform;
			if(self == null) Debug.LogError("ERROR! The SplinerInterpolator was unable to associate a reference to the attached Transform!", this);

			// Set the step value per frame if we are in frame based mode
			if(frameBasedMovement) step = 1.0f / (float)moveSpeedFrames;
			// Otherwise, set it to the reciprocal of the movement duration
			if(!frameBasedMovement) step = 1.0f/moveSpeed;
		}
		
		void Update () {
			// Immediatly exit if the mode is set to Manual
			if(mode == SplinerInterpolationMode.Manual) return;

			// Update progress, factoring in Time.deltaTime in framerate-independent mode
			if(frameBasedMovement) progress += step;
			else progress += step * Time.deltaTime;


			// Apply progress to the movement
			if(!reverseDirection) Evaluate(progress);
			else Evaluate(1.0f - progress);

			// If we are in a mode other than ForwardOnly, denote that we have reached the end if progress is approximately 1.0
			if(mode != SplinerInterpolationMode.ForwardOnly){
				if(isMovingForwards){
					if(progress > 1.0f - endpointPrecision) TurnAround();
				}
				else{
					if(progress < endpointPrecision) TurnAround();
				}

				
			}
		
		}

		void TurnAround(){
			// If we aren't in Cycles mode and Count is less than desired, reverse the direction of movement
			if(mode != SplinerInterpolationMode.Cycle){
				if(mode == SplinerInterpolationMode.Count && currentCount >= desiredCount) return;
				else{
					step *= -1f;			// Reverse direction
					if(isMovingForwards) progress = 1.0f - endpointPrecision;
					else progress = endpointPrecision;
				}
			}

			// Increment the count
			currentCount++;

			// If we are in Cycles mode
			if(mode == SplinerInterpolationMode.Cycle) progress = 0f;
		}

		void OnValidate(){
			if(targetSpline == null) return;
			//else if(reverseDirection == false) transform.position = targetSpline.Evaluate(0f);
			//else transform.position = targetSpline.Evaluate(1f);

			if(self == null) self = gameObject.GetComponent<Transform>() as Transform;

			if(!reverseDirection) Evaluate(previewT);
			else Evaluate(1.0f - previewT);
		}

		// Internal value tracking the previous value of T, to check if we are moving forawrd or backwards
		private float pT = 0f;

		public void Evaluate(float t){

			if(pT <= t) isMovingForwards = true;
			else isMovingForwards = false;

			pT = t;

			// Bail out if there is no assigned reference to the target spline
			if(targetSpline == null){
				Debug.LogError("There is no SplinerBase set as a target in this SplinerInterpolator!", this);
				return;
			}

			// Evaluate the position and set it, but offset t slightly if we are aligning to a rotation target
			if(alignRotation){
				if(isMovingForwards) t -= 0.001f;
				else t += 0.001f;
			}
			//t = Map(t, 0f, 1f, startPoint, endPoint);

			// Set the position, and bail out immediately if we aren't aligning to rotation
			self.position = targetSpline.Evaluate(Map(t, 0f, 1f, startPoint, endPoint));
			if(!alignRotation) return;

			// Set the sampleT for the rotation to account for the offset
			if(alignRotation){
				if(isMovingForwards) t += alignLookDistance;
				else t -= alignLookDistance;
				if(t <= 0f) t = 0f;
				if(t >= 1f) t = 1f;

				t = Map(t, 0f, 1f, startPoint, endPoint);
				// Apply the rotation
				self.LookAt(targetSpline.Evaluate(t));
			}
		}

		// Adapted from https://forum.unity3d.com/threads/mapping-or-scaling-values-to-a-new-range.180090/
		public float Map(float value, float from2, float to2, float from, float to){
	        if(value <= from2){
	            return from;
	        }else if(value >= to2){
	            return to;
	        }else{
	            return (to - from) * ((value - from2) / (to2 - from2)) + from;
	        }
	    }

	}
}
