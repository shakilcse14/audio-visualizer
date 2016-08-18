using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AudioVisualizer))]
public class VisualizerEditor : Editor {

	public override void OnInspectorGUI ()
	{
		var visualizer = target as AudioVisualizer;

		if (visualizer.mode == AudioVisualizer.Mode.Auto) {
			visualizer.divideBarCount = EditorGUILayout.IntSlider ("Total Bar", visualizer.divideBarCount,
				1, visualizer.spectrumSize);
			visualizer.type = (AudioVisualizer.CreationType)EditorGUILayout.EnumPopup ("Creation Type", visualizer.type);
			if (visualizer.type == AudioVisualizer.CreationType.Primitive) {
				visualizer.primitiveType = (PrimitiveType)EditorGUILayout.EnumPopup ("Primitive Type", visualizer.primitiveType);
			}
			else if (visualizer.type == AudioVisualizer.CreationType.Prefab) {
				visualizer.barPrefab = (GameObject) EditorGUILayout.ObjectField("Bar Prefab", visualizer.barPrefab,
					typeof (GameObject), true);
			}
		}
		else if (visualizer.mode == AudioVisualizer.Mode.Manual) {
			visualizer.soundBarsParent = (GameObject)EditorGUILayout.ObjectField ("Bars Parent", visualizer.soundBarsParent,
				typeof (GameObject), true);
		}

		base.OnInspectorGUI ();
	}
}
