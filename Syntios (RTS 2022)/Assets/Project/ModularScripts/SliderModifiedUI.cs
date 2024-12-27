using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderModifiedUI : MonoBehaviour
{

    public Slider slider;
    public Text label;

    protected void Update()
    {

        if (label != null) label.text = slider.value.ToString();
    }
}
