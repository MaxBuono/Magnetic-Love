using System;
using System.Collections;
using System.Collections.Generic;
using MenuManagement;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HintHandler : MonoBehaviour
{
    public GameObject hint;
    public GameObject hintButton;
    public TextMeshProUGUI hintText;
    
    private int _level;
    //key -> level, value -> hint
    private Dictionary<int, string> _hints = new Dictionary<int, string>
    {
        {10, "Mag e Net vengono attratti dai piccoli anche attraverso i muri"},
        {12, "I piccoli possono essere usati come delle piattaforme per saltare"},
        {14, "A volte le piattaforme verticali possono essere prese solo in un determinato modo..."},
        {17, "L'attrazione tra Mag e Net va ben oltre i muri"}
    };

    public void ShowHint()
    {
        if (hint.activeSelf)
        {
            hint.gameObject.SetActive(false);
        }
        else
        {
            hint.gameObject.SetActive(true);
        }
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

        hint.gameObject.SetActive(false);
        
        //check if an hint exists
        if (!_hints.ContainsKey(_level))
        {
            hintButton.SetActive(false);
        }
        else
        {
            hintButton.SetActive(true);
            hintText.text = _hints[_level];
        }
        
    }
}
