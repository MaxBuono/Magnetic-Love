using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HintHandler : MonoBehaviour
{
    public GameObject hintPanel;
    public GameObject hintButton;
    public Image hintLight;
    public TextMeshProUGUI hintText;
    
    private int _level;
    private Color activeColor = new Color(1.0f, 0.93f, 0.83f);
    private Color notActiveColor = new Color(0.8f, 0.8f, 0.8f);

    public void ShowHint()
    {
        Color color = hintLight.color; 

        if (hintPanel.activeSelf)
        {
            hintPanel.gameObject.SetActive(false);
            color = notActiveColor;
        }
        else
        {
            hintPanel.gameObject.SetActive(true);
            color = activeColor;
        }

        hintLight.color = color;
    }

    private void OnEnable()
    {
        //handle first enable of pauseMenu
        try
        {
            _level = Convert.ToInt32(SceneManager.GetActiveScene().name.Split('-')[0].Trim('L'));
        }
        catch (FormatException e)
        {}

        hintPanel.gameObject.SetActive(false);

        //check if an hint exists
        string hint = GameManager.Instance.levelProperties[_level - 1].hint;
        if (hint == "")
        {
            hintButton.SetActive(false);
        }
        else
        {
            hintButton.SetActive(true);
            hintText.text = hint;
        }
        
    }
}
