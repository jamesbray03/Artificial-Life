using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderText : MonoBehaviour
{
    public Slider slider;
    void Start()
    {
        UpdateText();
    }
    public void UpdateText()
    {
        GetComponent<TMP_Text>().text = slider.value.ToString();
    }
}
