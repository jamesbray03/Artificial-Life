using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowthPlot : MonoBehaviour
{
    // plot output
    public LineRenderer plot;

    // plot parameters
    public Slider growthRate;
    public Slider growthMu;
    public Slider growthSigma;

    // called on start
    void Start() { PlotGrowth(); }

    // update line renderer
    public void PlotGrowth()
    {
        for (int i = 0; i < 360; i++)
        {
            float value = growthRate.value * (Mathf.Exp(-Mathf.Pow(i / 360f - growthMu.value, 2) / (2 * Mathf.Pow(growthSigma.value, 2))) - 0.5f) + 0.5f;
            value = Mathf.Clamp(value, 0f, 1f);

            plot.SetPosition(i, new Vector3(i, value * 140 - 140, 0));
        }
    }
}
