using UnityEngine;
using System.Collections;

public class AudioVisualizer : MonoBehaviour
{
	public enum Mode
	{
		Auto,
		Manual
	};
	public Mode mode = Mode.Auto;
	public enum CreationType{
		Primitive,
		Prefab
	};
	[HideInInspector]
	public CreationType type = CreationType.Primitive;
	[HideInInspector]
	public PrimitiveType primitiveType = PrimitiveType.Cube;
    float[] spectrum;
    public FFTWindow window = FFTWindow.Rectangular;
    public int spectrumSize = 256;
	[HideInInspector]
    public GameObject soundBarsParent;
    public GameObject[] soundBars;
	[HideInInspector]
	public int divideBarCount = 10;
	[HideInInspector]
	public GameObject barPrefab;
    private int totalDividationBars = 10;

    void Start()
    {
        spectrum = new float[spectrumSize];
        int count = 0;
		if (mode == Mode.Manual) {
			soundBars = new GameObject[soundBarsParent.transform.childCount];
			if (soundBarsParent != null) {
				foreach (Transform gme in soundBarsParent.transform) {
					soundBars [count] = gme.gameObject;
					count++;
				}
			}
		} else if (mode == Mode.Auto) {
			soundBars = new GameObject[divideBarCount];
			for (int i = 0; i < divideBarCount; i++) {
				if (type == CreationType.Primitive) {
					soundBars [i] = GameObject.CreatePrimitive (primitiveType);
					DestroyImmediate (soundBars [i].GetComponent<Collider> ());
				}
				else if (type == CreationType.Prefab) {
					if (barPrefab != null) {
						soundBars [i] = (GameObject)Instantiate (barPrefab, Vector3.zero, Quaternion.identity);
					}
					else {
						soundBars = null;
						return;
					}
				}
				if (soundBars [i] != null) {
					soundBars [i].transform.parent = transform;
					soundBars [i].transform.localScale = Vector3.one;
					soundBars [i].transform.localPosition = new Vector3 (i - divideBarCount / 2, 0.0f, 0.0f);
				}
			}
		}
		if (soundBars != null) {
			totalDividationBars = soundBars.Length;
		}
	}

    void Update()
    {
        if (totalDividationBars > 0)
		{
			if (soundBars != null) {
				
				AudioListener.GetSpectrumData (spectrum, 0, window);

				for (int i = 0; i < totalDividationBars; i++) {
					soundBars [i].transform.localScale =
                    new Vector3 (1.0f, spectrum [i] * 10.0f > 1.0f ? spectrum [i] * 10.0f : 1.0f, 1.0f);
				}
			}
        }
    }
}