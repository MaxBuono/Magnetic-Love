using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TransitionFader : ScreenFader
{
    public float lifetime = 1f;
    public float delay = 0.3f;
    public TMP_Text transitionText;
    public TMP_Text secondText;

    private void Awake()
    {
        lifetime = Mathf.Clamp(lifetime, FadeOnDuration + FadeOffDuration + delay, 10f);
    }

    public void SetText(string text="")
    {
        if (transitionText != null)
        {
            transitionText.text = text;
        }
    }
    
    private IEnumerator PlayRoutine(float time = 0.0f)
    {
        GameManager.Instance.PlayerControlsBlocked = true;
        SetAlpha(clearAlpha);
        yield return new WaitForSeconds(delay - time);

        FadeOn();

        float onTime = lifetime - (FadeOffDuration + delay);
        yield return new WaitForSeconds(onTime);

        FadeOff();
        GameManager.Instance.PlayerControlsBlocked = false;

        print("FADER DESTROYED");
        
        Destroy(gameObject, FadeOffDuration);
    }

    private IEnumerator PlayStringToStringRoutine(string tutorialEndString, string levelNameString)
    {
        GameManager.Instance.PlayerControlsBlocked = true;
        SetText(tutorialEndString);
        SetAlpha(clearAlpha);
        yield return new WaitForSeconds(delay);
        
        FirstStringFadeOn();
        
        float onTime = lifetime - (FadeOffDuration + delay);
        yield return new WaitForSeconds(onTime + 2f);
        
        //StringToStringFadeOn();
        FirstStringFadeOff();
        yield return new WaitForSeconds(onTime);
        
        secondText.SetText(levelNameString);
        SecondStringFadeOn();
        
        yield return new WaitForSeconds(onTime);
        
        SecondStringFadeOff();
        GameManager.Instance.PlayerControlsBlocked = false;

        print("FADER DESTROYED");
        
        Destroy(gameObject, FadeOffDuration);
    }
    
    public void Play()
    {
        StartCoroutine(PlayRoutine());
    }

    public void PlayStringToString(string tutorialEndString, string levelNameString)
    {
        StartCoroutine(PlayStringToStringRoutine(tutorialEndString, levelNameString));
    }

    public static void PlayStringToStringTransition(TransitionFader transitionPrefab, string tutorialEndString, string levelNameString)
    {
        if (transitionPrefab != null)
        {
            TransitionFader instance = Instantiate(transitionPrefab, Vector3.zero, Quaternion.identity);
            instance.PlayStringToString(tutorialEndString, levelNameString);
        }
    }

    public void FirstPlay()
    {
        StartCoroutine(PlayRoutine(0.3f));
    }

    public static void PlayTransition(TransitionFader transitionPrefab, string text = "")
    {
        if (transitionPrefab != null)
        {
            print("Transition Fader Created");
            TransitionFader instance = Instantiate(transitionPrefab, Vector3.zero, Quaternion.identity);

            // handle special case for the transition from main menu to the first level
            //if (text == "Menu")
            //{
            //    instance.SetText("");
            //    instance.FirstPlay();
            //    return;
            //}

            instance.SetText(text);
            print("Transition Fader Created <" + instance.gameObject.name + ">");
            instance.Play();
        }
    }
    
    
}
