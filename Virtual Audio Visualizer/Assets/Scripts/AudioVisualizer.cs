using UnityEngine;
using System.Collections;

public class AudioVisualizer : MonoBehaviour
{

    float[] spectrum;
    public int spectrumSize = 256;
    public int totalDividationBars = 10;
    private float sumOfNeighbours = 0;

    void Start()
    {
        spectrum = new float[spectrumSize];
    }

    void Update()
    {
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        //for (int i = 1; i < spectrum.Length - 1; i++)
        //{
        //    Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
        //}
        for (int i = 0; i < spectrum.Length - (spectrum.Length / totalDividationBars); i += (spectrum.Length / totalDividationBars))
        {
            sumOfNeighbours = 0.0f;
            for (int j = i; j < i + (spectrum.Length / totalDividationBars); j++)
            {
                sumOfNeighbours += spectrum[j];
            }
            Debug.LogWarning(i + ": " + sumOfNeighbours);
        }
        if ((spectrum.Length - (spectrum.Length / totalDividationBars)) > 0)
        {
            sumOfNeighbours = 0.0f;
            for (int i = (spectrum.Length - (spectrum.Length / totalDividationBars)); i < spectrum.Length; i++)
            {
                sumOfNeighbours += spectrum[i];
            }
            Debug.LogWarning(totalDividationBars + ": " + sumOfNeighbours);
        }
    }
}