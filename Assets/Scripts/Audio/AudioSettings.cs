using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _masterValue;
    [SerializeField] private TextMeshProUGUI _musicValue;
    [SerializeField] private TextMeshProUGUI _sfxValue;

    // Feedback sounds when the sliders handle are used
    private ulong _masterSound = 0;
    private ulong _musicSound = 0;
    private ulong _sfxSound = 0;
    private IEnumerator _masterCoroutine = null;
    private IEnumerator _musicCoroutine = null;
    private IEnumerator _sfxCoroutine = null;

    // used by volume sliders (going from 0 to 100, whole numbers)
    public void OnMasterChange(System.Single value)
    {
        // change the volume of the mixer (child)
        AudioManager.Instance.Mixer.SetFloat("MasterVolume", NormalizeVolume(value));
        // then set the text
        _masterValue.text = value.ToString();

        PlaySliderHandleSound(_masterCoroutine, _masterSound);
    }
    public void OnMusicChange(System.Single value)
    {
        AudioManager.Instance.Mixer.SetFloat("MusicVolume", NormalizeVolume(value));
        _musicValue.text = value.ToString();

        PlaySliderHandleSound(_musicCoroutine, _musicSound);
    }
    public void OnSFXChange(System.Single value)
    {
        AudioManager.Instance.Mixer.SetFloat("SFXVolume", NormalizeVolume(value));
        _sfxValue.text = value.ToString();

        PlaySliderHandleSound(_sfxCoroutine, _sfxSound);
    }

    // mixers volume should go from -80 dB to 0 dB
    private float NormalizeVolume(float value)
    {
        // conversion from float to dB, avoiding to take log(0) value (-infinity)
        float dB = Mathf.Max(0.0f, 10 * Mathf.Log10(value));    // [0,20]
        // scale it to [-80, 0] : NewValue = (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin
        return ((dB * 80.0f) / 20.0f) - 80.0f;
    }

    // play a sound whenever the slider value change and stop it before doing it if it's already playing
    private void PlaySliderHandleSound(IEnumerator coroutine, ulong soundID)
    {
        // if another sound is already playing stop it
        if (coroutine != null)
        {
            AudioManager.Instance.StopOneShotSound(soundID);
            StopCoroutine(coroutine);
            coroutine = null;
        }
        soundID = AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.buttonExitPressed);
        coroutine = WaitForSoundToEnd(AudioManager.Instance.buttonExitPressed.length);
        StartCoroutine(coroutine);
    }

    private IEnumerator WaitForSoundToEnd(float time)
    {
        yield return new WaitForSeconds(time);
        _masterCoroutine = null;
    }
}
