using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AudioVisualizer))]
public class VisualizerEditor : Editor {

	public override void OnInspectorGUI ()
	{
		var visualizer = target as AudioVisualizer;

		visualizer.timerClip = EditorGUILayout.Slider ("Clip Timer", visualizer.timerClip,
			0.0f, visualizer.audioTime);
		if (visualizer.audioSource != null) {
			visualizer.audioSource.time = visualizer.timerClip;
		}

        visualizer.randomEffect = (AudioVisualizer.Effect)EditorGUILayout.EnumPopup("Random Effect",
            visualizer.randomEffect);

		if (visualizer.mode == AudioVisualizer.Mode.Auto) {

			visualizer.divideBarCount = EditorGUILayout.IntSlider ("Total Bar", visualizer.divideBarCount,
				1, visualizer.spectrumSize);
			visualizer.type = (AudioVisualizer.CreationType)EditorGUILayout.EnumPopup ("Creation Type", visualizer.type);
			visualizer.shape = (AudioVisualizer.DrawShape)EditorGUILayout.EnumPopup ("Draw Shape", visualizer.shape);

			if (visualizer.type == AudioVisualizer.CreationType.Primitive) {
				visualizer.primitiveType = (PrimitiveType)EditorGUILayout.EnumPopup ("Primitive Type", visualizer.primitiveType);
			}
			else if (visualizer.type == AudioVisualizer.CreationType.Prefab) {
				visualizer.barPrefab = (GameObject) EditorGUILayout.ObjectField("Bar Prefab", visualizer.barPrefab,
					typeof (GameObject), true);
			}
			if (visualizer.shape == AudioVisualizer.DrawShape.Linear) {
				visualizer.distanceBetween = EditorGUILayout.Slider ("Distance Between", visualizer.distanceBetween,
					1.0f, 10.0f);
			} else if (visualizer.shape == AudioVisualizer.DrawShape.BoxLinear) {
				visualizer.Row = EditorGUILayout.IntField ("Rows", visualizer.Row);
				visualizer.Column = EditorGUILayout.IntField ("Columns", visualizer.Column);
			}
			else if (visualizer.shape == AudioVisualizer.DrawShape.Circular) {
				visualizer.radiusCircular = EditorGUILayout.Slider ("Radius Between", visualizer.radiusCircular,
					1.0f, 50.0f);
            }
            else if (visualizer.shape == AudioVisualizer.DrawShape.Randomize_Float)
            {
                visualizer.initialLocalPosition = (Vector3)EditorGUILayout.Vector3Field("Initial Position",
                    visualizer.initialLocalPosition, null);
                visualizer.radiusCircle = EditorGUILayout.Slider("Radius Random", visualizer.radiusCircle,
                    1.0f, 50.0f);
            }
		}
		else if (visualizer.mode == AudioVisualizer.Mode.Manual) {
			visualizer.soundBarsParent = (GameObject)EditorGUILayout.ObjectField ("Bars Parent", visualizer.soundBarsParent,
				typeof (GameObject), true);
		}

		EditorUtility.SetDirty(target);
		base.OnInspectorGUI ();
	}
}
