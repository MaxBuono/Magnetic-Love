using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] protected float solidAlpha = 1.0f;

    [SerializeField] protected float clearAlpha = 0.0f;

    [SerializeField] protected float fadeOnDuration = 2f;

    [SerializeField] protected float fadeOffDuration = 2f;

    [SerializeField] private MaskableGraphic[] graphicsToFade;

    public float FadeOnDuration
    {
        get { return fadeOnDuration; }
    }

    public float FadeOffDuration
    {
        get { return fadeOffDuration; }
    }
    
    protected void SetAlpha(float alpha)
    {
        for (int i = 0; i < graphicsToFade.Length; i++)
        {
            if (graphicsToFade[i] != null)
            {
                graphicsToFade[i].canvasRenderer.SetAlpha(alpha); 
            }
        }
    }

    private void Fade(float targetAlpha, float duration)
    {
        for (int i = 0; i < graphicsToFade.Length; i++)
        {
            if (graphicsToFade[i] != null)
            {
                graphicsToFade[i].CrossFadeAlpha(targetAlpha,duration,true);
            }
        }
    }

    public void FadeOff()
    {
        SetAlpha(solidAlpha);
        Fade(clearAlpha,fadeOffDuration);
    }
    
    public void FadeOn()
    {
        SetAlpha(clearAlpha);
        Fade(solidAlpha,fadeOnDuration);
    }
    
    public void FirstStringFadeOn()
    {
        graphicsToFade[0].canvasRenderer.SetAlpha(clearAlpha);
        graphicsToFade[1].canvasRenderer.SetAlpha(clearAlpha);
        graphicsToFade[0].CrossFadeAlpha(solidAlpha, fadeOnDuration, true);
        graphicsToFade[1].CrossFadeAlpha(solidAlpha, fadeOnDuration, true);
    }

    public void FirstStringFadeOff()
    {
        graphicsToFade[0].canvasRenderer.SetAlpha(solidAlpha);
        graphicsToFade[1].canvasRenderer.SetAlpha(solidAlpha);
        graphicsToFade[1].CrossFadeAlpha(clearAlpha, fadeOffDuration, true);
    }
    
    public void SecondStringFadeOn()
    {
        graphicsToFade[0].canvasRenderer.SetAlpha(solidAlpha);
        graphicsToFade[2].canvasRenderer.SetAlpha(clearAlpha);
        graphicsToFade[2].CrossFadeAlpha(solidAlpha, fadeOnDuration, true);
    }
    
    public void SecondStringFadeOff()
    {
        graphicsToFade[0].canvasRenderer.SetAlpha(solidAlpha);
        graphicsToFade[2].canvasRenderer.SetAlpha(solidAlpha);
        graphicsToFade[0].CrossFadeAlpha(clearAlpha, fadeOffDuration, true);
        graphicsToFade[2].CrossFadeAlpha(clearAlpha, fadeOffDuration, true);
    }
    
}
