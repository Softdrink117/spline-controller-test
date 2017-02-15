using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace Softdrink{
	[CustomEditor(typeof(SplinerBase))]
	public class SplinerEditor : Editor {

		public override void OnInspectorGUI(){
			DrawDefaultInspector();

			SplinerBase t = target as SplinerBase;
			if(GUILayout.Button("Reset All Segments")) t.ResetAllSegments();
		}

		void OnSceneGUI(){
			SplinerBase t = target as SplinerBase;
			Undo.RecordObject(t, "Spline Segment Edit");

			// Index through and create handles for startPoint, startTangent, endPoint, endTangent
			for(int i = 0; i < t.splineSegments.Length; i++){
				

				// Create handle for startPoint
				if(t.splineSegments[i].prev == null || !t.enforceContinuousSpline)
					t.splineSegments[i].startPoint = Handles.PositionHandle(t.splineSegments[i].startPoint, Quaternion.identity);
				else if( i >= 1) t.splineSegments[i].startPoint = t.splineSegments[i-1].endPoint;
				// Create handle for startTangent
				if(t.splineSegments[i].prev == null || !t.autoAlignTangents)
					t.splineSegments[i].startTangent = Handles.PositionHandle(t.splineSegments[i].startTangent, Quaternion.identity);
				else if(i >= 1) t.splineSegments[i].startTangent = t.splineSegments[i-1].endTangent;

				// Create handle for endPoint
				t.splineSegments[i].endPoint = Handles.PositionHandle(t.splineSegments[i].endPoint, Quaternion.identity);
				// Create handle for endTangent
				t.splineSegments[i].endTangent = Handles.PositionHandle(t.splineSegments[i].endTangent, Quaternion.identity);

				
			}
			
			EditorUtility.SetDirty(t);
		}

	}
}
