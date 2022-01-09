using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Note: the "PlaySomethingHandleSound" functions should be refactored in one using a delegate and referenced argumensts

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Text _masterValue;
    [SerializeField] private Text _musicValue;
    [SerializeField] private Text _sfxValue;

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

        PlayMasterHandleSound();
    }
    public void OnMusicChange(System.Single value)
    {
        AudioManager.Instance.Mixer.SetFloat("MusicVolume", NormalizeVolume(value));
        _musicValue.text = value.ToString();

        PlayMusicHandleSound();
    }
    public void OnSFXChange(System.Single value)
    {
        AudioManager.Instance.Mixer.SetFloat("SFXVolume", NormalizeVolume(value));
        _sfxValue.text = value.ToString();

        PlaySfxHandleSound();
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
    private void PlayMasterHandleSound()
    {
        // if another sound is already playing stop it
        if (_masterCoroutine != null)
        {
            AudioManager.Instance.StopOneShotSound(_masterSound);
            StopCoroutine(_masterCoroutine);
            _masterCoroutine = null;
        }
        else
        {
            _masterSound = AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.buttonExitPressed);
            _masterCoroutine = WaitForMasterSoundToEnd(AudioManager.Instance.buttonExitPressed.length); //AudioManager.Instance.buttonExitPressed.length
            StartCoroutine(_masterCoroutine);
        }
    }

    // play a sound whenever the slider value change and stop it before doing it if it's already playing
    private void PlayMusicHandleSound()
    {
        // if another sound is already playing stop it
        if (_musicCoroutine != null)
        {
            AudioManager.Instance.StopOneShotSound(_musicSound);
            StopCoroutine(_musicCoroutine);
            _musicCoroutine = null;
        }
        else
        {
            _musicSound = AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.buttonExitPressed);
            _musicCoroutine = WaitForMasterSoundToEnd(AudioManager.Instance.buttonExitPressed.length); //AudioManager.Instance.buttonExitPressed.length
            StartCoroutine(_musicCoroutine);
        }
    }

    // play a sound whenever the slider value change and stop it before doing it if it's already playing
    private void PlaySfxHandleSound()
    {
        // if another sound is already playing stop it
        if (_sfxCoroutine != null)
        {
            AudioManager.Instance.StopOneShotSound(_sfxSound);
            StopCoroutine(_sfxCoroutine);
            _sfxCoroutine = null;
        }
        else
        {
            _sfxSound = AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.buttonExitPressed);
            _sfxCoroutine = WaitForMasterSoundToEnd(AudioManager.Instance.buttonExitPressed.length); //AudioManager.Instance.buttonExitPressed.length
            StartCoroutine(_sfxCoroutine);
        }
    }

    private IEnumerator WaitForMasterSoundToEnd(float time)
    {
        yield return new WaitForSeconds(time);
        _masterCoroutine = null;
    }

    private IEnumerator WaitForMusicSoundToEnd(float time)
    {
        yield return new WaitForSeconds(time);
        _musicCoroutine = null;
    }

    private IEnumerator WaitForSfxSoundToEnd(float time)
    {
        yield return new WaitForSeconds(time);
        _sfxCoroutine = null;
    }
}
