using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider horizontalMouseSensitivitySlider;
    public Slider verticalMouseSensitivitySlider;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("HorizontalMouseSensitivity"))
        {
            if (horizontalMouseSensitivitySlider!=null)
            {
                horizontalMouseSensitivitySlider.value = PlayerPrefs.GetFloat("HorizontalMouseSensitivity");
            }
            
        }
        else
        {
            PlayerPrefs.SetFloat("HorizontalMouseSensitivity", horizontalMouseSensitivitySlider.value);
        }

        if (PlayerPrefs.HasKey("VerticalMouseSensitivity"))
        {
            if (verticalMouseSensitivitySlider != null)
            {
                verticalMouseSensitivitySlider.value = PlayerPrefs.GetFloat("VerticalMouseSensitivity");
            }
        }
        else
        {
            PlayerPrefs.SetFloat("VerticalMouseSensitivity", verticalMouseSensitivitySlider.value);
        }
    }

    public void ChangeHorizontalMouseSensitivity()
    {
        PlayerPrefs.SetFloat("HorizontalMouseSensitivity", horizontalMouseSensitivitySlider.value);
    }

    public void ChangeVerticalMouseSensitivity()
    {
        PlayerPrefs.SetFloat("VerticalMouseSensitivity", verticalMouseSensitivitySlider.value);
    }
}
