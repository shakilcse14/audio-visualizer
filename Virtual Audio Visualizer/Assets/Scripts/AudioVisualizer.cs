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
	public enum DrawShape
	{
		Linear,
		Circular,
		RandomizeFloat
	};
	[HideInInspector]
	public DrawShape shape = DrawShape.Linear;
	[HideInInspector]
	public float radiusCircular = 5.0f;
	[HideInInspector]
	public float distanceBetween = 1.0f;
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
	[Range(0.1f,5.0f)]
	public float smoothScaleDuration = 0.3f;
	[Range(10.0f,100.0f)]
	public float multiplierDB = 10.0f;
	public AudioSource audioSource;
	[HideInInspector]
	public float timerClip = 0.0f;
	[HideInInspector]
	public float audioTime = 0.0f;

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
					soundBars [i].transform.localPosition = GetPosition (i, transform);
				}
			}
		}
		if (soundBars != null) {
			totalDividationBars = soundBars.Length;
		}
		if (audioSource == null) {
			var temp = AudioListener.FindObjectOfType<AudioSource> ();
			if (temp != null) {
				audioSource = temp;
				audioTime = audioSource.clip.length;
			}
		}
		else
		{
			audioTime = audioSource.clip.length;
		}
	}

	Vector3 GetPosition(int index, Transform trns)
	{
		Vector3 tempPosition = Vector3.zero;
		if (shape == DrawShape.Linear) {
			tempPosition = new Vector3 ((index - (divideBarCount / 2)) * distanceBetween, 0.0f, 0.0f);
		} else if (shape == DrawShape.Circular) {
			tempPosition = new Vector3 (
				Mathf.Cos ((index == 0 ? 0.0f : 360.0f / (float)divideBarCount) * index * Mathf.PI / 180.0f) * radiusCircular,
				0.0f,
				Mathf.Sin ((index == 0 ? 0.0f : 360.0f / (float)divideBarCount) * index * Mathf.PI / 180.0f) * radiusCircular);
		} else if (shape == DrawShape.RandomizeFloat) {
			
		}
		return tempPosition;
	}

    void Update()
    {
        if (totalDividationBars > 0)
		{
			if (soundBars != null) {
				if (audioSource == null) {
					AudioListener.GetSpectrumData (spectrum, 0, window);
				} else {
					audioSource.GetSpectrumData (spectrum, 0, window);
					timerClip = audioSource.time;
				}
				for (int i = 0; i < totalDividationBars; i++) {
					soundBars [i].transform.localScale = Vector3.Lerp (soundBars [i].transform.localScale, new Vector3 (soundBars [i].transform.localScale.x,
						spectrum [i] * 10.0f > 1.0f ? spectrum [i] * multiplierDB : 1.0f, soundBars [i].transform.localScale.z), Time.deltaTime * smoothScaleDuration);
				}
			}
        }
    }
}
