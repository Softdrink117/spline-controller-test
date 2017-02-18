using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace Softdrink{
	[CustomEditor(typeof(SplinerBase))]
	public class SplinerEditor : Editor {

		public override void OnInspectorGUI(){
			DrawDefaultInspector();

			SplinerBase t = target as SplinerBase;
			Undo.RecordObject(t, "Spliner Edit");

			GUILayout.Space(10);

			EditorGUILayout.LabelField("Add Segments", EditorStyles.boldLabel);
			
			if(GUILayout.Button("Add End Segment")) t.AddNewEnd();
			if(GUILayout.Button("Add Start Segment")) t.AddNewStart();
			GUILayout.Space(10);

			EditorGUILayout.LabelField("Flatten", EditorStyles.boldLabel);
			if(GUILayout.Button("Flatten X Axis")) t.Flatten("X");
			if(GUILayout.Button("Flatten Y Axis")) t.Flatten("Y");
			if(GUILayout.Button("Flatten Z Axis")) t.Flatten("Z");
			GUILayout.Space(10);

			EditorGUILayout.LabelField("Clear", EditorStyles.boldLabel);
			if(GUILayout.Button("Clear All Segments")) t.ResetAllSegments();

		}

		private Transform handleTransform;
		private Quaternion handleRotation;
		private Vector3 handlePos = new Vector3(0f,0f,0f);

		private int index = 0;

		void OnSceneGUI(){
			SplinerBase t = target as SplinerBase;
			// Adapted from http://catlikecoding.com/unity/tutorials/curves-and-splines/
			handleTransform = t.transform;
			handleRotation = handleTransform.rotation;

			serializedObject.Update();
			//Undo.RecordObject(t, "Spline Segment Edit");
			EditorGUI.BeginChangeCheck();

			index = 0;

			// Index through and create handles for startPoint, startTangent, endPoint, endTangent
			for(int i = 0; i < t.splineSegments.Length; i++){

				// Create handle for startPoint
				if(t.splineSegments[i].prev == null || !t.enforceContinuousSpline){

					handlePos = t.splineSegments[i].Transformed(t.splineSegments[i].startPoint);
					//handlePos = Handles.PositionHandle(handlePos, Quaternion.identity);
					handlePos = DrawPoint(handlePos, index);
					t.splineSegments[i].startPoint = t.splineSegments[i].InverseTransformed(handlePos);

					//t.splineSegments[i].startPoint = Handles.PositionHandle(t.splineSegments[i].startPoint, Quaternion.identity);
				}
				else if( i >= 1) t.splineSegments[i].startPoint = t.splineSegments[i-1].endPoint;

				index++;
			}

			for(int i = 0; i < t.splineSegments.Length; i++){

				// Create handle for startTangent
				if(t.splineSegments[i].prev == null || !t.autoAlignTangents){

					//handlePos = t.splineSegments[i].Transformed(t.splineSegments[i].startTangent);
					//handlePos = Handles.PositionHandle(handlePos, Quaternion.identity);
					handlePos = DrawPoint(handlePos, index);
					//t.splineSegments[i].startTangent = t.splineSegments[i].InverseTransformed(handlePos);

					//t.splineSegments[i].startTangent = Handles.PositionHandle(t.splineSegments[i].startTangent, Quaternion.identity);
				}
				else if(i >= 1) t.splineSegments[i].startTangent = t.splineSegments[i-1].endTangent;

				index++;
			}

			for(int i = 0; i < t.splineSegments.Length; i++){

				// Create handle for endPoint
				handlePos = t.splineSegments[i].Transformed(t.splineSegments[i].endPoint);
				//handlePos = Handles.PositionHandle(handlePos, Quaternion.identity);
				handlePos = DrawPoint(handlePos, index);
				t.splineSegments[i].endPoint = t.splineSegments[i].InverseTransformed(handlePos);

				index++;

			}

			for(int i = 0; i < t.splineSegments.Length; i++){

				//t.splineSegments[i].endPoint = Handles.PositionHandle(t.splineSegments[i].endPoint, Quaternion.identity);

				// Create handle for endTangent
				handlePos = t.splineSegments[i].Transformed(t.splineSegments[i].endTangent);
				//handlePos = Handles.PositionHandle(handlePos, Quaternion.identity);
				handlePos = DrawPoint(handlePos, index);
				t.splineSegments[i].endTangent = t.splineSegments[i].InverseTransformed(handlePos);

				//t.splineSegments[i].endTangent = Handles.PositionHandle(t.splineSegments[i].endTangent, Quaternion.identity);

				index++;

			}

			//EditorUtility.SetDirty(t);
			serializedObject.ApplyModifiedProperties();
			if(EditorGUI.EndChangeCheck()) Undo.RecordObject(t, "Spline Segment Edit");
		}

		// Adapted from http://catlikecoding.com/unity/tutorials/curves-and-splines/
		private int selectedIndex = -1;
		private const float handleSize = 0.04f;
		private const float pickSize = 0.06f;

		Vector3 DrawPoint(Vector3 input, int index){

			handlePos = input;
			Handles.color = Color.white;

			if(Handles.Button(handlePos, handleRotation, handleSize * HandleUtility.GetHandleSize(handlePos), pickSize * HandleUtility.GetHandleSize(handlePos), Handles.DotCap)) selectedIndex = index;

			if(selectedIndex == index){
				//EditorGUI.BeginChangeCheck();
				handlePos = Handles.PositionHandle(handlePos, handleRotation);
				//if(EditorGUI.EndChangeCheck()) Undo.RecordObject(t, "Spline Segment Edit");
			}

			return handlePos;
		}

	}
}
