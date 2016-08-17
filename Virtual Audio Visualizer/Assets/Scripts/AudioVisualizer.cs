using UnityEngine;
using System.Collections;

public class AudioVisualizer : MonoBehaviour
{

    float[] spectrum;
    public FFTWindow window = FFTWindow.Rectangular;
    public int spectrumSize = 256;
    public GameObject soundBarsParent;
    public GameObject[] soundBars;
    private int totalDividationBars = 10;

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
            AudioListener.GetSpectrumData(spectrum, 0, window);

            for (int i = 0; i < totalDividationBars; i ++)
            {
                Debug.LogWarning(i + ": " + spectrum[i] * 10.0f);
                soundBars[i].transform.localScale =
                    new Vector3(1.0f, spectrum[i] * 10.0f > 1.0f ? spectrum[i] * 10.0f : 1.0f, 1.0f);
            }
        }
    }
}