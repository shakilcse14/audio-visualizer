using UnityEngine;
using System.Collections;

public class AudioVisualizer : MonoBehaviour
{

    float[] spectrum;
    public int spectrumSize = 256;
    public GameObject soundBarsParent;
    public GameObject[] soundBars;
    private int totalDividationBars = 10;
    private float sumOfNeighbours = 0;

    void Start()
    {
        spectrum = new float[spectrumSize];
        int i = 0;
        soundBars = new GameObject[soundBarsParent.transform.childCount];
        if (soundBarsParent != null)
        {
            foreach (Transform gme in soundBarsParent.transform)
            {
                soundBars[i] = gme.gameObject;
                i++;
            }
        }
        totalDividationBars = soundBars.Length;
    }

    void Update()
    {
        if (totalDividationBars > 0)
        {
            AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);

            for (int i = 0; i < spectrum.Length - (spectrum.Length / totalDividationBars); i += (spectrum.Length / totalDividationBars))
            {
                sumOfNeighbours = 0.0f;
                for (int j = i; j < i + (spectrum.Length / totalDividationBars); j++)
                {
                    sumOfNeighbours += spectrum[j] * 10.0f;
                }
                Debug.LogWarning((i / (spectrum.Length / totalDividationBars)) + ": " + sumOfNeighbours * 10.0f);
                soundBars[(i / (spectrum.Length / totalDividationBars))].transform.localScale =
                    new Vector3(1.0f, sumOfNeighbours > 1.0f ? sumOfNeighbours : 1.0f, 1.0f);
            }
            if ((spectrum.Length - (spectrum.Length / totalDividationBars)) > 0)
            {
                sumOfNeighbours = 0.0f;
                for (int i = (spectrum.Length - (spectrum.Length / totalDividationBars)); i < spectrum.Length; i++)
                {
                    sumOfNeighbours += spectrum[i] * 10.0f;
                }
                soundBars[totalDividationBars - 1].transform.localScale =
                    new Vector3(1.0f, sumOfNeighbours > 1.0f ? sumOfNeighbours : 1.0f, 1.0f);
                Debug.LogWarning(totalDividationBars + ": " + sumOfNeighbours * 10.0f);
            }
        }
    }
}