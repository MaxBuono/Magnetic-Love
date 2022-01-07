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
    public Image hintButton;
    public TextMeshProUGUI hintText;
    
    private int _level;
    private Color activeColor = new Color(1.0f, 0.93f, 0.83f);
    private Color notActiveColor = new Color(0.8f, 0.8f, 0.8f);

    public void ShowHint()
    {
        Color color = hintButton.color; 

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

        hintButton.color = color;
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
            hintButton.gameObject.SetActive(false);
        }
        else
        {
            hintButton.gameObject.SetActive(true);
            hintText.text = hint;
        }
        
    }
}
