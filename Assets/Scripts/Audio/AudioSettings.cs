using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _masterValue;
    [SerializeField] private TextMeshProUGUI _musicValue;
    [SerializeField] private TextMeshProUGUI _sfxValue;

    // used by volume sliders (going from 0 to 100, whole numbers)
    public void OnMasterChange(System.Single value)
    {
        // change the volume of the mixer (child)
        AudioManager.Instance.Mixer.SetFloat("MasterVolume", NormalizeVolume(value));
        // then set the text
        _masterValue.text = value.ToString();
    }
    public void OnMusicChange(System.Single value)
    {
        AudioManager.Instance.Mixer.SetFloat("MusicVolume", NormalizeVolume(value));
        _musicValue.text = value.ToString();
    }
    public void OnSFXChange(System.Single value)
    {
        AudioManager.Instance.Mixer.SetFloat("SFXVolume", NormalizeVolume(value));
        _sfxValue.text = value.ToString();
    }

    // mixers volume should go from -80 dB to 0 dB
    private float NormalizeVolume(float value)
    {
        // conversion from float to dB, avoiding to take log(0) value (-infinity)
        float dB = Mathf.Max(0.0f, 10 * Mathf.Log10(value));    // [0,20]
        // scale it to [-80, 0] : NewValue = (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin
        return ((dB * 80.0f) / 20.0f) - 80.0f;
    }
}
