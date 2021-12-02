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

    public void Play()
    {
        StartCoroutine(PlayRoutine());
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
