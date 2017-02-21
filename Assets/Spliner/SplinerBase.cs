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
		// What is the Transform that this SplineSection is relative to?
		[HideInInspector]
		public Transform parent;

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
		private bool drawTangentsUnselected = true;

		[SerializeField]
		[TooltipAttribute("What type of Segment is this? \nIsolated - no continuity (solitary Segment). \nStart Segment - continuity after, but not before, this Segment. \nEnd Segment - continuity before, but not after, this Segment. \nContinuous - continuity on both sides of this Segment.")]
		private SplineSectionSegmentType segmentType = SplineSectionSegmentType.Isolated;

		public SplineSection(SplineSection input){
			startPoint = input.startPoint;
			startTangent = input.startTangent;
			endPoint = input.endPoint;
			endTangent = input.endTangent;
			parent = input.parent;
		}

		public SplineSection(Transform parentIn){
			parent = parentIn;
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
				return Transformed((p0 * ((2.0f*t3) - (3.0f*t2) + 1.0f))
					+ (m0 * (t3 + (-2.0f*t2) + t))
					+ (p1 * ((-2.0f*t3) + (3.0f*t2)))
					+ (m1 * (t3 - t2)));
			} else {
				return nullReturn;
			}
		}

		public bool IsValid() {
			return (startPoint != nullReturn) && (startTangent != nullReturn) && (endPoint != nullReturn) && (endTangent != nullReturn);
		}

		public Vector3 Transformed(Vector3 input){
			// Vector3 output = new Vector3(input.x, input.y, input.z);
			// output += parent.position;
			// output = parent.rotation * output;
			// output = new Vector3(output.x * parent.lossyScale.x, output.y * parent.lossyScale.y, output.z * parent.lossyScale.z);

			

			// return output;
			return parent.TransformPoint(input);

		}

		public Vector3 InverseTransformed(Vector3 input){
			//Vector3 output = new Vector3(input.x, input.y, input.z);
			// output = new Vector3(output.x / parent.lossyScale.x, output.y / parent.lossyScale.y, output.z/parent.lossyScale.z);
			// //output = new Vector3(output.x / parent.rotation.x, output.y / parent.rotation.y, output.z / parent.rotation.z);
			// output = Quaternion.Inverse(parent.rotation) * output;
			// output -= parent.position;

			

			// return output;
			return parent.InverseTransformPoint(input);
		}

		public void DrawGizmos(){
			SetSegmentStatus();

			if(IsValid()) {
				Gizmos.color = Color.grey;
				if(drawTangentsUnselected){
					Gizmos.DrawLine(Transformed(startPoint), Transformed(startTangent));
					Gizmos.DrawLine(Transformed(endPoint), Transformed(endTangent));
				}
				for(int i = 0; i < gizmoDetail; i++) {
					Gizmos.DrawLine(GetPositionAt(((float)i)/(float)gizmoDetail),
					                GetPositionAt(((float)(i+1))/(float)gizmoDetail));
				}
			}

			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(Transformed(endPoint), gizmoSphereSize);
			if(prev == null) Gizmos.DrawWireSphere(Transformed(startPoint), gizmoSphereSize);

			//Debug.Log(forceContinuous);
		}

		public void DrawGizmosSelected(){
			SetSegmentStatus();

			if(IsValid()) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(Transformed(startPoint), Transformed(startTangent));
				Gizmos.DrawLine(Transformed(endPoint), Transformed(endTangent));
				Gizmos.color = Color.white;
				int gizmoDetailTemp = gizmoDetail*2;
				for(int i = 0; i < gizmoDetailTemp; i++) {
					Gizmos.DrawLine(GetPositionAt(((float)i)/(float)gizmoDetailTemp),GetPositionAt(((float)i+1)/(float)gizmoDetailTemp));
				}
			}

			// If this is a start point, draw a colored wire sphere at the start point, otherwise use white
			if(prev == null){
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(Transformed(startPoint), gizmoSphereSize);
			}else{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(Transformed(startPoint), gizmoSphereSize);
			}

			// If this is a end point, draw a colored wire sphere at the end point, otherwise use white
			if(next == null){
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(Transformed(endPoint), gizmoSphereSize);
			}else{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(Transformed(endPoint), gizmoSphereSize);
			}

			// Draw spheres at tangent handles
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Transformed(startTangent), gizmoSphereSize/1.5f);
			Gizmos.DrawWireSphere(Transformed(endTangent), gizmoSphereSize/1.5f);
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

		public void setDrawTangents(bool input){
			drawTangentsUnselected = input;
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

		[SerializeField]
		[TooltipAttribute("Should Tangent lines be drawn in the Scene View when the Spline is not selected?")]
		private bool drawUnselectedTangents = true;

		void OnDrawGizmos() {
			// Draw a larger sphere for the main Transform parent
			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(transform.position, gizmoSphereSize * 3f);
			Gizmos.DrawWireSphere(transform.position, gizmoSphereSize * 2f);


			for(int i = 0; i < splineSegments.Length; i++){
				splineSegments[i].DrawGizmos();
			}
		}

		void OnDrawGizmosSelected() {
			// Draw a larger sphere for the main Transform parent
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, gizmoSphereSize * 3f);
			Gizmos.DrawWireSphere(transform.position, gizmoSphereSize * 2f);

			for(int i = 0; i < splineSegments.Length; i++){
				splineSegments[i].DrawGizmosSelected();
			}
		}

		// Validate changes to the editor setup
		void OnValidate(){
			// If the array doesn't exist bail out
			if(splineSegments == null) return;

			UpdateSegments();
		}

		public void UpdateSegments(){
			// Otherwise index through and assign the prev and next instances automatically, update positions, set status, etc.
			for(int i = 0; i < splineSegments.Length; i++){
				// Set the parent
				splineSegments[i].parent = transform;

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
				// Set drawing of unselected tangents
				splineSegments[i].setDrawTangents(drawUnselectedTangents);
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
			splineSegments[0] = new SplineSection(transform);
			splineSegments[0].endTangent = new Vector3(1f,0f,1f);
			splineSegments[0].endPoint = new Vector3(0.5f, 0f, 0.5f);
			splineSegments[0].startPoint = new Vector3(-0.5f, 0f, -0.5f);
			splineSegments[0].startTangent = new Vector3(-1f, 0f, -1f);
		}

		// Extend the length of the spline by one in the Forward direction
		public void AddNewEnd(){
			// Create a temp array of Spline Sections and populate it with the current content of the spline Segments array
			SplineSection[] temp = new SplineSection[splineSegments.Length];
			for(int i = 0; i < temp.Length; i++){
				temp[i] = new SplineSection(splineSegments[i]);
			}

			// Unset and reinitialize the current array
			splineSegments = null;
			splineSegments = new SplineSection[temp.Length + 1];

			// Get the old array contents from the temp buffer
			for(int i = 0; i < temp.Length; i++){
				splineSegments[i] = new SplineSection(temp[i]);
			}

			int leng = splineSegments.Length;

			// Create a new SplineSection and set it based on the current EndPoint
			splineSegments[leng - 1] = new SplineSection(splineSegments[leng - 2]);
			splineSegments[leng - 1].startPoint = new Vector3(splineSegments[leng - 2].endPoint.x, splineSegments[leng - 2].endPoint.y, splineSegments[leng - 2].endPoint.z);
			splineSegments[leng - 1].startTangent = new Vector3(splineSegments[leng - 2].endTangent.x, splineSegments[leng - 2].endTangent.y, splineSegments[leng - 2].endTangent.z);
			// The direction and endTangent of the new segment should be based off of the normalized local space endTangent of the previous segment
			Vector3 normtan = new Vector3(splineSegments[leng - 2].endPoint.x - -splineSegments[leng - 2].endTangent.x, splineSegments[leng - 2].endPoint.y - -splineSegments[leng - 2].endTangent.y, splineSegments[leng - 2].endPoint.z - -splineSegments[leng - 2].endTangent.z);
			normtan.Normalize();
			normtan *= Vector3.Distance(splineSegments[leng - 2].startPoint, splineSegments[leng-2].endPoint);

			splineSegments[leng - 1].endPoint = new Vector3(splineSegments[leng - 1].startPoint.x + normtan.x, splineSegments[leng - 1].startPoint.y + normtan.y, splineSegments[leng - 1].startPoint.z + normtan.z);
			splineSegments[leng - 1].endTangent = new Vector3(splineSegments[leng - 1].endPoint.x + normtan.x, splineSegments[leng - 1].endPoint.y + normtan.y, splineSegments[leng - 1].endPoint.z + normtan.z);

			// Set status of all spline segments
			UpdateSegments();
		}

		// Extend the length of the spline by one in the Backward direction
		public void AddNewStart(){
			// Create a temp array of Spline Sections and populate it with the current content of the spline Segments array
			SplineSection[] temp = new SplineSection[splineSegments.Length];
			for(int i = 0; i < temp.Length; i++){
				temp[i] = new SplineSection(splineSegments[i]);
			}

			// Unset and reinitialize the current array
			splineSegments = null;
			splineSegments = new SplineSection[temp.Length + 1];

			// Get the old array contents from the temp buffer
			for(int i = 0; i < temp.Length; i++){
				splineSegments[i+1] = new SplineSection(temp[i]);
			}

			//int leng = 0;

			// Create a new SplineSection and set it based on the current StartPoint
			splineSegments[0] = new SplineSection(splineSegments[1]);
			splineSegments[0].endPoint = new Vector3(splineSegments[1].startPoint.x, splineSegments[1].startPoint.y, splineSegments[1].startPoint.z);
			splineSegments[0].endTangent = new Vector3(splineSegments[1].startTangent.x, splineSegments[1].startTangent.y, splineSegments[1].startTangent.z);
			// The direction and startTangent of the new segment should be based off of the normalized local space startTangent of the next segment
			Vector3 normtan = new Vector3(splineSegments[1].startPoint.x - -splineSegments[1].startTangent.x, splineSegments[1].startPoint.y - -splineSegments[1].startTangent.y, splineSegments[1].startPoint.z - -splineSegments[1].startTangent.z);
			normtan.Normalize();
			normtan *= Vector3.Distance(splineSegments[1].startPoint, splineSegments[1].endPoint);

			splineSegments[0].startPoint = new Vector3(splineSegments[0].endPoint.x + normtan.x, splineSegments[0].endPoint.y + normtan.y, splineSegments[0].endPoint.z + normtan.z);
			normtan *= 0.25f;
			splineSegments[0].startTangent = new Vector3(splineSegments[0].startPoint.x + normtan.x, splineSegments[0].startPoint.y + normtan.y, splineSegments[0].startPoint.z + normtan.z);

			// Set status of all spline segments
			UpdateSegments();
		}

		// Flatten a specified axis
		public void Flatten(string axis){
			if(axis == "X"){
				for(int i = 0; i < splineSegments.Length; i++){
					SplineSection temp = splineSegments[i];
					temp.startPoint = new Vector3(0f, temp.startPoint.y, temp.startPoint.z);
					temp.startTangent = new Vector3(0f, temp.startTangent.y, temp.startTangent.z);
					temp.endPoint = new Vector3(0f, temp.endPoint.y, temp.endPoint.z);
					temp.endTangent = new Vector3(0f, temp.endTangent.y, temp.endTangent.z);
					splineSegments[i] = new SplineSection(temp);
				}
			}
			if(axis == "Y"){
				for(int i = 0; i < splineSegments.Length; i++){
					SplineSection temp = splineSegments[i];
					temp.startPoint = new Vector3(temp.startPoint.x, 0f, temp.startPoint.z);
					temp.startTangent = new Vector3(temp.startPoint.x, 0f, temp.startTangent.z);
					temp.endPoint = new Vector3(temp.endPoint.x, 0f, temp.endPoint.z);
					temp.endTangent = new Vector3(temp.endTangent.x, 0f, temp.endTangent.z);
					splineSegments[i] = new SplineSection(temp);
				}
			}
			if(axis == "Z"){
				for(int i = 0; i < splineSegments.Length; i++){
					SplineSection temp = splineSegments[i];
					temp.startPoint = new Vector3(temp.startPoint.x, temp.startPoint.y, 0f);
					temp.startTangent = new Vector3(temp.startPoint.x, temp.startTangent.y, 0f);
					temp.endPoint = new Vector3(temp.endPoint.x, temp.endPoint.y, 0f);
					temp.endTangent = new Vector3(temp.endTangent.x, temp.endTangent.y, 0f);
					splineSegments[i] = new SplineSection(temp);
				}
			}


		}

		// Evalute a 0...1 position along the total spline
		public Vector3 Evaluate(float t){
			if(t >= 1.0f) t = 1.0f;
			if(t <= 0.0f) t = 0.0f;

			t *= splineSegments.Length;
			return splineSegments[0].GetPositionAt(t);

		}

	}
}