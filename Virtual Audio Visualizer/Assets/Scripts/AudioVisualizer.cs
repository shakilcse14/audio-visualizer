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
    public enum CreationType
    {
        Primitive,
        Prefab
    };
    public enum DrawShape
    {
        Linear,
		BoxLinear,
        Circular,
        PerlinNoise,
        Randomize_Float
    };
    public enum Effect
    {
        ScaleY,
        ScaleAll,
        Position,
        ScaleY_Position,
        ScaleAll_Position
    };
    [HideInInspector]
    public DrawShape shape = DrawShape.Linear;
    [HideInInspector]
    public Effect randomEffect = Effect.ScaleY;
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
    [Range(0.1f, 5.0f)]
    public float smoothScaleDuration = 0.3f;
    [Range(10.0f, 100.0f)]
    public float multiplierDB = 10.0f;
    public AudioSource audioSource;
    [HideInInspector]
    public float timerClip = 0.0f;
    [HideInInspector]
    public float audioTime = 0.0f;
    [HideInInspector]
    public Vector3 initialLocalPosition = Vector3.zero;
    [HideInInspector]
	public float radiusCircle = 10.0f;
	[HideInInspector]
	public int Row = 5;
	[HideInInspector]
	public int Column = 5;
    public Vector3[] oldPositions;
    public bool isRotate = false;
    public float rotateAmount = 15.0f;
	public Material reflectionMat;
	public Transform originParent;
	public Light pointLight;
	public Light directionalLight;

    void Start()
    {
        spectrum = new float[spectrumSize];
        int count = 0;

		if (type == CreationType.Primitive) {
			if (originParent != null) {
				originParent.transform.position = new Vector3 (originParent.transform.position.x,
					originParent.transform.position.y + 0.5f, originParent.transform.position.z);
			}
		}

        if (mode == Mode.Manual)
        {
            soundBars = new GameObject[soundBarsParent.transform.childCount];
            if (soundBarsParent != null)
            {
                foreach (Transform gme in soundBarsParent.transform)
                {
                    soundBars[count] = gme.gameObject;
                    count++;
                }
            }
        }
        else if (mode == Mode.Auto)
        {
			if (shape == DrawShape.BoxLinear) {
				divideBarCount = Row * Column;
				if (divideBarCount >= spectrum.Length) {
					gameObject.SetActive (false);
					return;
				}
				soundBars = new GameObject[divideBarCount];
				oldPositions = new Vector3[divideBarCount];
				GenerateBox ();
			} else {
				soundBars = new GameObject[divideBarCount];
				oldPositions = new Vector3[divideBarCount];
				Generate ();
			}
        }
        if (soundBars != null)
        {
            totalDividationBars = soundBars.Length;
        }
        if (audioSource == null)
        {
            var temp = AudioListener.FindObjectOfType<AudioSource>();
            if (temp != null)
            {
                audioSource = temp;
                audioTime = audioSource.clip.length;
            }
        }
        else
        {
            audioTime = audioSource.clip.length;
        }
    }

	void GenerateBox()
	{
		for (int i = 0; i < Row; i++) {
			for (int j = 0; j < Column; j++) {
				var index = i * Column + j;
				if (type == CreationType.Primitive) {
					soundBars [index] = GameObject.CreatePrimitive (primitiveType);
					if (reflectionMat != null) {
						soundBars [index].GetComponent<Renderer> ().sharedMaterial = reflectionMat;
					}
					soundBars [index].GetComponent<MeshRenderer> ().receiveShadows = false;
					DestroyImmediate (soundBars [index].GetComponent<Collider> ());
				} else if (type == CreationType.Prefab) {
					if (barPrefab != null) {
						soundBars [index] = (GameObject)Instantiate (barPrefab, Vector3.zero, Quaternion.identity);
					} else {
						soundBars = null;
						return;
					}
				}
				if (soundBars [index] != null) {
					if (originParent != null) {
						soundBars [index].transform.parent = originParent;
					} else {
						soundBars [index].transform.parent = transform;
					}
					if (randomEffect == Effect.ScaleAll || randomEffect == Effect.ScaleAll_Position) {
						soundBars [index].transform.localScale = Vector3.one * 0.25f;
					} else {
						soundBars [index].transform.localScale = Vector3.one;
					}
					soundBars [index].transform.localPosition = GetPosition (index,
						originParent != null ? originParent.transform : transform);
				}
			}
		}
	}

	void Generate()
	{
		for (int i = 0; i < divideBarCount; i++)
		{
			if (type == CreationType.Primitive)
			{
				soundBars[i] = GameObject.CreatePrimitive(primitiveType);
				if (reflectionMat != null) {
					soundBars [i].GetComponent<Renderer> ().sharedMaterial = reflectionMat;
				}
				soundBars [i].GetComponent<MeshRenderer> ().receiveShadows = false;
				DestroyImmediate(soundBars[i].GetComponent<Collider>());
			}
			else if (type == CreationType.Prefab)
			{
				if (barPrefab != null)
				{
					soundBars[i] = (GameObject)Instantiate(barPrefab, Vector3.zero, Quaternion.identity);
				}
				else
				{
					soundBars = null;
					return;
				}
			}
			if (soundBars[i] != null)
			{
				if (originParent != null) {
					soundBars [i].transform.parent = originParent;
				} else {
					soundBars[i].transform.parent = transform;
				}
				if (randomEffect == Effect.ScaleAll || randomEffect == Effect.ScaleAll_Position) {
					soundBars [i].transform.localScale = Vector3.one * 0.25f;
				} else {
					soundBars [i].transform.localScale = Vector3.one;
				}
				soundBars[i].transform.localPosition = GetPosition(i,
					originParent != null ? originParent.transform : transform);
			}
		}
	}

    Vector3 GetPosition(int index, Transform trns)
    {
        Vector3 tempPosition = Vector3.zero;
        if (shape == DrawShape.Linear)
        {
            tempPosition = new Vector3((index - (divideBarCount / 2)) * distanceBetween, 0.5f, 0.0f);
        }
		else if (shape == DrawShape.BoxLinear)
		{
//			tempPosition = new Vector3(((index % Column) - (Column / 2)) * distanceBetween,
//				0.0f, ((index / Row) - (Row / 2)) * distanceBetween);
			tempPosition = new Vector3(((index % Column) - (Column / 2)) * distanceBetween,
				0.0f, ((index / Column) - (Row / 2)) * distanceBetween);
		}
        else if (shape == DrawShape.Circular)
        {
            tempPosition = new Vector3(
                Mathf.Cos((index == 0 ? 0.0f : 360.0f / (float)divideBarCount) * index * Mathf.PI / 180.0f) * radiusCircular,
                0.0f,
                Mathf.Sin((index == 0 ? 0.0f : 360.0f / (float)divideBarCount) * index * Mathf.PI / 180.0f) * radiusCircular);
        }
        else if (shape == DrawShape.Randomize_Float)
        {
            tempPosition = new Vector3(initialLocalPosition.x + Random.insideUnitCircle.x *
                Mathf.Sin((index == 0 ? 0.0f : 360.0f / (float)divideBarCount) * index * Mathf.PI / 180.0f) * radiusCircle,
                initialLocalPosition.y + Mathf.Abs(Random.insideUnitCircle.y * radiusCircle / 2.0f), initialLocalPosition.z + Random.insideUnitCircle.y *
                Mathf.Cos((index == 0 ? 0.0f : 360.0f / (float)divideBarCount) * index * Mathf.PI / 180.0f) * radiusCircle);
        }
		oldPositions[index] = tempPosition;
		return tempPosition;
    }

    void Update()
    {
		if (isRotate) {
			if (originParent != null) {
				originParent.Rotate (0.0f, Time.deltaTime * rotateAmount, 0.0f);
			} else {
				transform.Rotate (0.0f, Time.deltaTime * rotateAmount, 0.0f);
			}
		}
        if (totalDividationBars > 0)
        {
            if (soundBars != null)
            {
                if (audioSource == null)
                {
                    AudioListener.GetSpectrumData(spectrum, 0, window);
                }
                else
                {
                    audioSource.GetSpectrumData(spectrum, 0, window);
                    timerClip = audioSource.time;
                }
                for (int i = 0; i < totalDividationBars; i++)
                {
                    if (randomEffect == Effect.Position)
                    {
                        soundBars[i].transform.localPosition = Vector3.Lerp(soundBars[i].transform.localPosition,
                            new Vector3(soundBars[i].transform.localPosition.x,
                            spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : oldPositions[i].y,
                            soundBars[i].transform.localPosition.z),
                            Time.deltaTime * smoothScaleDuration);
                    }
                    else if (randomEffect == Effect.ScaleY)
                    {
                        soundBars[i].transform.localScale = Vector3.Lerp(soundBars[i].transform.localScale,
                            new Vector3(soundBars[i].transform.localScale.x,
                            spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB : 1.0f,
                            soundBars[i].transform.localScale.z),
                            Time.deltaTime * smoothScaleDuration);
                    }
                    else if (randomEffect == Effect.ScaleAll)
                    {
                        soundBars[i].transform.localScale = Vector3.Lerp(soundBars[i].transform.localScale,
							new Vector3(spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : 0.25f,
								spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : 0.25f,
								spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : 0.25f),
                            Time.deltaTime * smoothScaleDuration);
                    }
                    else if (randomEffect == Effect.ScaleY_Position)
                    {
                        soundBars[i].transform.localPosition = Vector3.Lerp(soundBars[i].transform.localPosition,
                            new Vector3(soundBars[i].transform.localPosition.x,
                            spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : oldPositions[i].y,
                            soundBars[i].transform.localPosition.z),
                            Time.deltaTime * smoothScaleDuration);
                        soundBars[i].transform.localScale = Vector3.Lerp(soundBars[i].transform.localScale,
                            new Vector3(soundBars[i].transform.localScale.x,
                            spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB : 1.0f,
                            soundBars[i].transform.localScale.z),
                            Time.deltaTime * smoothScaleDuration);
                    }
                    else if (randomEffect == Effect.ScaleAll_Position)
                    {
                        soundBars[i].transform.localPosition = Vector3.Lerp(soundBars[i].transform.localPosition,
							new Vector3(spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : oldPositions[i].x,
                            spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : oldPositions[i].y,
								spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : oldPositions[i].z),
							Time.deltaTime * smoothScaleDuration);
						soundBars[i].transform.localScale = Vector3.Lerp(soundBars[i].transform.localScale,
							new Vector3(spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : 0.25f,
								spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : 0.25f,
								spectrum[i] * 10.0f > 1.0f ? spectrum[i] * multiplierDB / 2.0f : 0.25f),
							Time.deltaTime * smoothScaleDuration);
                    }
					if (directionalLight != null && pointLight != null) {
						if (spectrum [i] * 10.0f > 2.0f) {
							pointLight.color = Color.Lerp (pointLight.color, new Color(Random.value,Random.value,Random.value,1.0f),
								Time.deltaTime * smoothScaleDuration);
							directionalLight.color = Color.Lerp (directionalLight.color, new Color(Random.value,Random.value,Random.value,1.0f),
								Time.deltaTime * smoothScaleDuration);
						}
					}
                }
            }
        }
    }
}
