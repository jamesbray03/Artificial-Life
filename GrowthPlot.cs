using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowthPlot : MonoBehaviour
{
    public Slider growthRate;
    public Slider growthMu;
    public Slider growthSigma;

    public LineRenderer plot;

    public void PlotGrowth()
    {
        try
        {
            for (int i = 0; i < 360; i++ )
            {
                float value = growthRate.value * (Mathf.Exp(-Mathf.Pow(i/360f - growthMu.value, 2) / (2 * Mathf.Pow(growthSigma.value, 2))) - 0.5f) + 0.5f;
                value = Mathf.Clamp(value, 0f, 1f);

                plot.SetPosition(i, new Vector3(i, value * 140 - 140, 0));
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    void Start()
    {
        PlotGrowth();
    }
}
