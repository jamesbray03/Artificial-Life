using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderText : MonoBehaviour
{
    // slider
    public Slider slider;

    // called on start
    void Start() { UpdateText(); }

    // update text to show slider value
    public void UpdateText() { GetComponent<TMP_Text>().text = slider.value.ToString(); }
}
