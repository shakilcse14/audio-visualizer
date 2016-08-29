using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        ScaleAll_Position,
		ScaleAll_RandomPosition
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
	[HideInInspector]
    public GameObject[] soundBars;
    [HideInInspector]
    public int divideBarCount = 10;
    [HideInInspector]
    public GameObject barPrefab;
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
	[HideInInspector]
	public Vector3[] oldPositions;
	public float[] perlinNoise;
    public bool isRotate = false;
    public float rotateAmount = 15.0f;
    public Material reflectionMat;
    public Transform originParent;
    public Light pointLight;
    public Light directionalLight;
    public int channel = 0;
	private Color targetColor;
	private float timeToColorFade = 0.0f;
	public float totalTimeColorFade = 1.0f;
	private int seed;
    [HideInInspector]
    public bool isMeshGenerate;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private GameObject gmeMesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    Vector2[] uvs;
    public float spaceVertices = 0.5f;
	public int increment = 0;

    void Start()
	{
		seed = (int)Network.time * 10;
		timeToColorFade = totalTimeColorFade;
		spectrum = new float[spectrumSize];
		int count = 0;
        uvs = new Vector2[divideBarCount * divideBarCount];

		if (type == CreationType.Primitive) {
			if (originParent != null) {
				originParent.transform.position = new Vector3 (originParent.transform.position.x,
					originParent.transform.position.y + 0.5f, originParent.transform.position.z);
			}
		}
        if (!isMeshGenerate)
        {
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
                if (shape == DrawShape.BoxLinear || shape == DrawShape.PerlinNoise)
                {
                    divideBarCount = Row * Column;
                    if (divideBarCount >= spectrum.Length)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                    soundBars = new GameObject[divideBarCount];
                    oldPositions = new Vector3[divideBarCount];
                    perlinNoise = new float[divideBarCount];
                    GenerateBox();
                }
                else
                {
                    soundBars = new GameObject[divideBarCount];
                    oldPositions = new Vector3[divideBarCount];
                    Generate();
                }
            }
            if (soundBars != null)
            {
                divideBarCount = soundBars.Length;
            }
        }
        else
        {
            perlinNoise = new float[divideBarCount * divideBarCount];
            for (int i = 0; i < divideBarCount; i++)
            {
                for (int j = 0; j < divideBarCount; j++)
                {
                    var index = i * Column + j;
                    perlinNoise[index] = Mathf.PerlinNoise(j + seed + 0.01f, i);
                }
            }
            MeshGenerate();
        }
		if (audioSource == null) {
			var temp = AudioListener.FindObjectOfType<AudioSource> ();
			if (temp != null) {
				audioSource = temp;
				audioTime = audioSource.clip.length;
			}
		} else {
			audioTime = audioSource.clip.length;
		}
	}

    void MeshGenerate()
    {
        gmeMesh = new GameObject("Wave");
        gmeMesh.transform.position = new Vector3(-divideBarCount / 2.0f * spaceVertices, 0.0f, -divideBarCount / 2.0f * spaceVertices);
        meshFilter = gmeMesh.AddComponent<MeshFilter>();
        mesh = new Mesh();
        int indexUvs = 0;
        for (int i = 0; i < divideBarCount; i++)
        {
            for (int j = 0; j < divideBarCount; j++)
            {
                Vector3 position = new Vector3(j * spaceVertices, 0.0f, i * spaceVertices);
                vertices.Add(position);

                uvs[indexUvs] = new Vector2(j / (float)divideBarCount, i / (float)divideBarCount);
                indexUvs++;
            }
        }
        for (int i = 0; i < divideBarCount - 1; i++)
        {
            for (int j = 0; j < divideBarCount - 1; j++)
            {
                triangles.Add(j + (i * divideBarCount));
                triangles.Add((j + (i * divideBarCount)) + divideBarCount);
                triangles.Add((j + (i * divideBarCount)) + divideBarCount + 1);

                triangles.Add(j + (i * divideBarCount));
                triangles.Add((j + (i * divideBarCount)) + divideBarCount + 1);
                triangles.Add((j + (i * divideBarCount)) + 1);
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.name = "Custom Plane";
        meshFilter.mesh = mesh;

        gmeMesh.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
    }

    void GenerateBox()
    {
        for (int i = 0; i < Row; i++)
        {
            for (int j = 0; j < Column; j++)
            {
                var index = i * Column + j;
				if (shape == DrawShape.PerlinNoise) {
					perlinNoise [index] = Mathf.PerlinNoise (j + seed + 0.01f, i);
				}
                if (type == CreationType.Primitive)
                {
                    soundBars[index] = GameObject.CreatePrimitive(primitiveType);
                    if (reflectionMat != null)
                    {
                        soundBars[index].GetComponent<Renderer>().sharedMaterial = reflectionMat;
                    }
                    soundBars[index].GetComponent<MeshRenderer>().receiveShadows = false;
                    DestroyImmediate(soundBars[index].GetComponent<Collider>());
                }
                else if (type == CreationType.Prefab)
                {
                    if (barPrefab != null)
                    {
                        soundBars[index] = (GameObject)Instantiate(barPrefab, Vector3.zero, Quaternion.identity);
                    }
                    else
                    {
                        soundBars = null;
                        return;
                    }
                }
                if (soundBars[index] != null)
                {
                    if (originParent != null)
                    {
                        soundBars[index].transform.parent = originParent;
                    }
                    else
                    {
                        soundBars[index].transform.parent = transform;
                    }
                    if (randomEffect == Effect.ScaleAll || randomEffect == Effect.ScaleAll_Position)
                    {
                        soundBars[index].transform.localScale = Vector3.one * 0.25f;
                    }
                    else
                    {
                        soundBars[index].transform.localScale = Vector3.one;
                    }
                    soundBars[index].transform.localPosition = GetPosition(index,
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
                if (reflectionMat != null)
                {
                    soundBars[i].GetComponent<Renderer>().sharedMaterial = reflectionMat;
                }
                soundBars[i].GetComponent<MeshRenderer>().receiveShadows = false;
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
                if (originParent != null)
                {
                    soundBars[i].transform.parent = originParent;
                }
                else
                {
                    soundBars[i].transform.parent = transform;
                }
                if (randomEffect == Effect.ScaleAll || randomEffect == Effect.ScaleAll_Position)
                {
                    soundBars[i].transform.localScale = Vector3.one * 0.25f;
                }
                else
                {
                    soundBars[i].transform.localScale = Vector3.one;
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
		else if (shape == DrawShape.BoxLinear || shape == DrawShape.PerlinNoise)
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
        if (isRotate)
        {
            if (originParent != null)
            {
                originParent.Rotate(0.0f, Time.deltaTime * rotateAmount, 0.0f);
            }
            else
            {
                transform.Rotate(0.0f, Time.deltaTime * rotateAmount, 0.0f);
            }
        }

        if (divideBarCount > 0)
        {
            if (soundBars != null)
            {
                if (audioSource == null)
                {
                    AudioListener.GetSpectrumData(spectrum, channel, window);
                }
                else
                {
                    audioSource.GetSpectrumData(spectrum, channel, window);
                    timerClip = audioSource.time;
                }
                if (shape == DrawShape.PerlinNoise)
                {
                    if (isMeshGenerate)
					{
						var index = 0;
						float val = 0.0f;
						var indice = 0;
						increment = (int)((float)(divideBarCount * divideBarCount) / (float)spectrumSize) + 1;
						for (int k = 0; k < divideBarCount; k += increment) {
							for (int j = 0; j < divideBarCount; j += increment) {
								val = 0.0f;
								indice = k * divideBarCount + j;
								if (spectrumSize > index) {
									val = Mathf.Clamp (spectrum [index] * multiplierDB * (index + 1), 0.0f, 3.0f);
									if (shape == DrawShape.PerlinNoise) {
										val = val - perlinNoise [indice];
										val = Mathf.Clamp (val, 0.0f, 100.0f);
									}
								} else {
									val = Mathf.Clamp (perlinNoise [indice] * multiplierDB * Random.value * (index + 1), 0.0f, 3.0f);
								}
								vertices [indice] = Vector3.Lerp (mesh.vertices [indice],
									new Vector3 (mesh.vertices [indice].x, val, mesh.vertices [indice].z),
									Time.deltaTime * smoothScaleDuration);
								index++;
							}
						}
                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uvs;
                        mesh.triangles = triangles.ToArray();

                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        meshFilter.mesh = mesh;
                    }
                    else
					{
						var index = 0;
                        for (int i = 0; i < Row; i++)
                        {
                            for (int j = 0; j < Column; j++)
							{
								index = i * Column + j;
                                if (randomEffect == Effect.Position)
                                {
                                    Position(index);
                                }
                                else if (randomEffect == Effect.ScaleY)
                                {
                                    ScaleY(index);
                                }
                                else if (randomEffect == Effect.ScaleAll)
                                {
                                    ScaleAll(index);
                                }
                                else if (randomEffect == Effect.ScaleY_Position)
                                {
                                    Position(index);
                                    ScaleY(index);
                                }
                                else if (randomEffect == Effect.ScaleAll_Position)
                                {
                                    Position(index);
                                    ScaleAll(index);
                                }
                                else if (randomEffect == Effect.ScaleAll_RandomPosition)
                                {
                                    RandomPosition(index);
                                    ScaleAll(index);
                                }
                            }
                            if (directionalLight != null && pointLight != null)
                            {
                                if (spectrum[i] * 10.0f >= 0.5f)
                                {
                                    if (timeToColorFade <= 0.0f)
                                    {
                                        targetColor = new Color(Random.value, Random.value, Random.value, 1.0f);
                                        timeToColorFade = totalTimeColorFade;
                                    }
                                    else
                                    {
                                        var color = Color.Lerp(pointLight.color, targetColor,
                                            Time.deltaTime / timeToColorFade);
                                        color = new Color(color.r, color.g, color.b, 1.0f);
                                        if (color.r <= 0.35f && color.g <= 0.35f && color.b <= 0.35f)
                                        {
                                            color = new Color(Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), 1.0f);
                                        }
                                        pointLight.color = color;
                                        directionalLight.color = color;
                                        timeToColorFade -= Time.deltaTime;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < divideBarCount; i++)
                    {
                        if (randomEffect == Effect.Position)
                        {
                            Position(i);
                        }
                        else if (randomEffect == Effect.ScaleY)
                        {
                            ScaleY(i);
                        }
                        else if (randomEffect == Effect.ScaleAll)
                        {
                            ScaleAll(i);
                        }
                        else if (randomEffect == Effect.ScaleY_Position)
                        {
                            Position(i);
                            ScaleY(i);
                        }
                        else if (randomEffect == Effect.ScaleAll_Position)
                        {
                            Position(i);
                            ScaleAll(i);
                        }
                        else if (randomEffect == Effect.ScaleAll_RandomPosition)
                        {
                            RandomPosition(i);
                            ScaleAll(i);
                        }
                        if (directionalLight != null && pointLight != null)
                        {
                            if (spectrum[i] * 10.0f >= 0.5f)
                            {
                                if (timeToColorFade <= 0.0f)
                                {
                                    targetColor = new Color(Random.value, Random.value, Random.value, 1.0f);
                                    timeToColorFade = totalTimeColorFade;
                                }
                                else
                                {
                                    var color = Color.Lerp(pointLight.color, targetColor,
                                        Time.deltaTime / timeToColorFade);
                                    color = new Color(color.r, color.g, color.b, 1.0f);
                                    if (color.r <= 0.35f && color.g <= 0.35f && color.b <= 0.35f)
                                    {
                                        color = new Color(Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), 1.0f);
                                    }
                                    pointLight.color = color;
                                    directionalLight.color = color;
                                    timeToColorFade -= Time.deltaTime;
                                }
                            }
                        }
                    }
                }

            }
        }

    }

	void ScaleAll(int index)
	{
		var val = spectrum [index] * multiplierDB >= 1.0f ? 
			Mathf.Clamp (spectrum [index] * multiplierDB / 2.0f * (index + 1), 0.25f, 5.0f) : 
			Mathf.Clamp (0.25f + spectrum [index] * multiplierDB / 2.0f * (index + 1), 0.25f, 5.0f);
		if (shape == DrawShape.PerlinNoise) {
			val = perlinNoise [index] - val;
			val = Mathf.Clamp (val, 0.0f, 100.0f);
		}
		soundBars [index].transform.localScale = Vector3.Lerp (soundBars [index].transform.localScale,
			new Vector3 (val, val, val), Time.deltaTime * smoothScaleDuration);
	}

	void Position(int index)
	{
		var val = spectrum [index] * multiplierDB >= 1.0f ? 
			Mathf.Clamp (spectrum [index] * multiplierDB * (index + 1), 0.0f, 10.0f) : 
			Mathf.Clamp (initialLocalPosition.y + spectrum [index] * multiplierDB * (index + 1), 0.0f, 10.0f);
		if (shape == DrawShape.PerlinNoise) {
			val = val - perlinNoise [index];
			val = Mathf.Clamp (val, 0.0f, 100.0f);
		}
		soundBars [index].transform.localPosition = Vector3.Lerp (soundBars [index].transform.localPosition,
			new Vector3 (soundBars [index].transform.localPosition.x,
				val, soundBars [index].transform.localPosition.z),
			Time.deltaTime * smoothScaleDuration);
	}

	void RandomPosition(int index)
	{
		var valy = spectrum [index] * multiplierDB >= 1.0f ? 
			Mathf.Clamp (spectrum [index] * multiplierDB * (index + 1), 0.0f, 10.0f) : 
			Mathf.Clamp (initialLocalPosition.y + spectrum [index] * multiplierDB * (index + 1), 0.0f, 10.0f);
		var val = spectrum [index] * multiplierDB >= 1.0f ? 
			Mathf.Clamp (spectrum [index] * multiplierDB * (index + 1), 0.0f, 10.0f) : 
			Mathf.Clamp (spectrum [index] * multiplierDB * (index + 1), 0.0f, 10.0f);
		if (shape == DrawShape.PerlinNoise) {
			valy = valy - perlinNoise [index];
			val = val - perlinNoise [index];
			val = Mathf.Clamp (val, 0.0f, 100.0f);
			valy = Mathf.Clamp (valy, 0.0f, 100.0f);
		}

		soundBars [index].transform.localPosition = Vector3.Lerp (soundBars [index].transform.localPosition,
			new Vector3 (val * Random.Range (-1.0f, 1.0f),
				valy, val * Random.Range (-1.0f, 1.0f)),
			Time.deltaTime * smoothScaleDuration);
	}

	void ScaleY(int index)
	{
		var val = spectrum [index] * multiplierDB >= 1.0f ? 
			Mathf.Clamp (spectrum [index] * multiplierDB * (index + 1), 1.0f, 10.0f) : 
			Mathf.Clamp (1.0f + spectrum [index] * multiplierDB * (index + 1), 1.0f, 10.0f);
		if (shape == DrawShape.PerlinNoise) {
			val = val - perlinNoise [index];
			//Debug.LogWarning ("Before: " + val);
			val = Mathf.Clamp (val, 0.0f, 100.0f);
			//Debug.LogWarning ("After: " + val);
		}
		soundBars [index].transform.localScale = Vector3.Lerp (soundBars [index].transform.localScale,
			new Vector3 (soundBars [index].transform.localScale.x, val,
				soundBars [index].transform.localScale.z),
			Time.deltaTime * smoothScaleDuration);
	}

}
