using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Softdrink{

	// Enum for segment types
	public enum SplineSectionSegmentType{
		Isolated,
		StartSegment,
		EndSegment,
		Continuous,
	};

	[System.Serializable]
	public class SplineSection{

		// What value to return if there is an error getting the position
		private Vector3 nullReturn = new Vector3(-99f,-99f,-99f);

		[TooltipAttribute("Reference positions for the start position of the Spline Section. ")]
		public Vector3 startPoint = new Vector3(0f,0f,0f);
		[TooltipAttribute("Reference positions for the start tangent of the Spline Section. ")]
		public Vector3 startTangent = new Vector3(0f,0f,0f);
		[TooltipAttribute("Reference positions for the end position of the Spline Section. ")]
		public Vector3 endPoint = new Vector3(0f,0f,0f);
		[TooltipAttribute("Reference positions for the end tangent of the Spline Section. ")]
		public Vector3 endTangent = new Vector3(0f,0f,0f);

		[HideInInspector]
		public SplineSection prev, next;

		// [HideInInspector]
		private bool forceContinuous = true;

		// [HideInInspector]
		private bool forceTangentAlignment = true;

		private int gizmoDetail = 8;
		private float gizmoSphereSize = 0.025f;

		[SerializeField]
		[TooltipAttribute("What type of Segment is this? \nIsolated - no continuity (solitary Segment). \nStart Segment - continuity after, but not before, this Segment. \nEnd Segment - continuity before, but not after, this Segment. \nContinuous - continuity on both sides of this Segment.")]
		private SplineSectionSegmentType segmentType = SplineSectionSegmentType.Isolated;

		public SplineSection(SplineSection input){
			startPoint = input.startPoint;
			startTangent = input.startTangent;
			endPoint = input.endPoint;
			endTangent = input.endTangent;
		}

		public Vector3 GetPositionAt(float t) {
			if ((t < 0f) && (prev!=null) && prev.IsValid()) {
				return prev.GetPositionAt(t+1.0f);
			} else if ((t > 1f) && (next!=null) && next.IsValid()) {
				return next.GetPositionAt(t-1.0f);
			} else if (IsValid()) {
				float t2 = t*t;
				float t3 = t2*t;
				Vector3 p0 = startPoint, p1 = endPoint, m0 = startTangent - p0, m1 = endTangent - p1;
				return (p0 * ((2.0f*t3) - (3.0f*t2) + 1.0f))
					+ (m0 * (t3 + (-2.0f*t2) + t))
					+ (p1 * ((-2.0f*t3) + (3.0f*t2)))
					+ (m1 * (t3 - t2));
			} else {
				return nullReturn;
			}
		}

		public bool IsValid() {
			return (startPoint != nullReturn) && (startTangent != nullReturn) && (endPoint != nullReturn) && (endTangent != nullReturn);
		}



		public void DrawGizmos(){
			SetSegmentStatus();

			if(IsValid()) {
				Gizmos.color = Color.grey;
				Gizmos.DrawLine(startPoint, startTangent);
				Gizmos.DrawLine(endPoint, endTangent);
				for(int i = 0; i < gizmoDetail; i++) {
					Gizmos.DrawLine(GetPositionAt(((float)i)/(float)gizmoDetail),
					                GetPositionAt(((float)(i+1))/(float)gizmoDetail));
				}
			}

			//Debug.Log(forceContinuous);
		}

		public void DrawGizmosSelected(){
			SetSegmentStatus();

			if(IsValid()) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(startPoint, startTangent);
				Gizmos.DrawLine(endPoint, endTangent);
				Gizmos.color = Color.white;
				int gizmoDetailTemp = gizmoDetail*2;
				for(int i = 0; i < gizmoDetailTemp; i++) {
					Gizmos.DrawLine(GetPositionAt(((float)i)/(float)gizmoDetailTemp),GetPositionAt(((float)i+1)/(float)gizmoDetailTemp));
				}
			}

			// If this is a start point, draw a colored wire sphere at the start point, otherwise use white
			if(prev == null){
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(startPoint, gizmoSphereSize);
			}else{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(startPoint, gizmoSphereSize);
			}

			// If this is a end point, draw a colored wire sphere at the end point, otherwise use white
			if(next == null){
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(endPoint, gizmoSphereSize);
			}else{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(endPoint, gizmoSphereSize);
			}

			// Draw spheres at tangent handles
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(startTangent, gizmoSphereSize/1.5f);
			Gizmos.DrawSphere(endTangent, gizmoSphereSize/1.5f);
		}

		public void SetSegmentStatus(){
			if(prev != null && next != null){
				segmentType = SplineSectionSegmentType.Continuous;
				return;
			}
			if(prev == null && next != null){
				segmentType = SplineSectionSegmentType.StartSegment;
				return;
			}
			if(prev != null && next == null){
				segmentType = SplineSectionSegmentType.EndSegment;
				return;
			}
			segmentType = SplineSectionSegmentType.Isolated;

			// If the segment type is not Isolated, set start/end points automatically
			if(forceContinuous){
				if(prev != null) startPoint = new Vector3(prev.endPoint.x, prev.endPoint.y, prev.endPoint.z);
				//if(next != null) endPoint = new Vector3(next.startPoint.x, next.startPoint.y, next.startPoint.z);
			}

			// If the segment type is not Isolated and automatic tangent alignment is enabled, set tangents automatically
			if(forceTangentAlignment){
				if(prev != null) startTangent = new Vector3(prev.endTangent.x, prev.endTangent.y, prev.endTangent.z);
			}
		}

		public SplineSectionSegmentType GetSegmentStatus(){
			return segmentType;
		}

		public void ResetSegment(){
			startPoint = new Vector3(0f,0f,0f);
			startTangent = new Vector3(0f,0f,0f);
			endPoint = new Vector3(0f,0f,0f);
			endTangent = new Vector3(0f,0f,0f);
		}

		public void SetContinuity(bool input){
			forceContinuous = input;
		}

		public void SetTangentAlignment(bool input){
			forceTangentAlignment = input;
		}

		public void SetGizmoDetail(int input){
			gizmoDetail = input;
		}

		public void setGizmoSphereSize(float input){
			gizmoSphereSize = input;
		}

	}

	public class SplinerBase : MonoBehaviour {

		[HeaderAttribute("Continuity and Self-Alignment")]
		
		[TooltipAttribute("99% of the time, you should leave this enabled. \nIf enabled, the Spline will be fully continuous. Otherwise, the start and end points of each segment in the Spline do not necessarily need to match up. The result will be a discontinuous set of Spline Segments that are still treated as one continuous Spline for Interpolation.")]
		public bool enforceContinuousSpline = true;

		[TooltipAttribute("If enabled, tangents between adjacent segments will automatically align. This makes smoother curves with less effort, but the output is slightly less customizable.")]
		public bool autoAlignTangents = true;

		[HeaderAttribute("Spline Segments")]

		[SerializeField]
		[TooltipAttribute("Array containing all Spline Segments. These are used in indexed order as the default 'forward' direction of the Spline.")]
		public SplineSection[] splineSegments;

		[HeaderAttribute("Miscellaneous")]

		[SerializeField]
		[Range(1,24)]
		[TooltipAttribute("The number of straight line segments that will be used to render each Spline Segment Gizmo. A higher number will draw a smoother curve in the Editor, at the cost of some performance.\nThe spline is drawn at double this precision when selected.\nThis DOES NOT AFFECT SAMPLING ACCURACY FOR MOVEMENT!")]
		private int gizmoDetail = 16;

		[SerializeField]
		[Range(0.001f, 0.25f)]
		[TooltipAttribute("The size of spheres to draw at start and end points of segments.")]
		private float gizmoSphereSize = 0.025f;

		void OnDrawGizmos() {
			for(int i = 0; i < splineSegments.Length; i++){
				splineSegments[i].DrawGizmos();
			}
		}

		void OnDrawGizmosSelected() {
			for(int i = 0; i < splineSegments.Length; i++){
				splineSegments[i].DrawGizmosSelected();
			}
		}

		// Validate changes to the editor setup
		void OnValidate(){
			// If the array doesn't exist bail out
			if(splineSegments == null) return;

			// Otherwise index through and assign the prev and next instances automatically, update positions, set status, etc.
			for(int i = 0; i < splineSegments.Length; i++){
				// Assign prev references if not at beginning of list
				if(i < 1) splineSegments[i].prev = null;
				else splineSegments[i].prev = splineSegments[i-1];

				// Assign next refernces if not at end of list
				if(i + 1 >= splineSegments.Length) splineSegments[i].next = null;
				else splineSegments[i].next = splineSegments[i+1];

				// Set whether or not to enforce continuity
				splineSegments[i].SetContinuity(enforceContinuousSpline);
				// Set automatic tangent alignment
				splineSegments[i].SetTangentAlignment(autoAlignTangents);

				// Set the segment status and enforce continuity if necessary
				splineSegments[i].SetSegmentStatus();

				// Set the gizmo detail 
				splineSegments[i].SetGizmoDetail(gizmoDetail);
				// Set the gizmo sphere size
				splineSegments[i].setGizmoSphereSize(gizmoSphereSize);
			}
		}

		public void ResetAllSegments(){
			for(int i = 0; i < splineSegments.Length; i++){
				splineSegments[i].ResetSegment();
				splineSegments[i].SetContinuity(enforceContinuousSpline);
				splineSegments[i].SetSegmentStatus();
			}

			splineSegments = null;

			splineSegments = new SplineSection[1]; 
		}

	}
}