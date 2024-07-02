using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderManager : MonoBehaviour
{
    // sliders
    public List<Slider> sliders;

    // sets parameters to random values
    public void Randomise()
    {
        foreach (Slider slider in sliders)
        {
            slider.value = Random.Range(slider.minValue, slider.maxValue);
        }
    }

    // nudges parameters by a random amount
    public void Nudge()
    {
        foreach (Slider slider in sliders)
        {
            float sliderRange = slider.maxValue - slider.minValue;
            slider.value += Random.Range(-0.1f * sliderRange, 0.1f * sliderRange);
        }
    }
}
