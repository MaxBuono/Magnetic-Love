using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingTabs : MonoBehaviour
{
    public Button[] tabs;

    private Color activeColor = Color.white;
    private Color notActiveColor = new Color(0.78f, 0.72f, 0.62f);

    private void Start()
    {
        // assign all the onClick events automatically
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].onClick.AddListener(() => ChangeColor());
        }

        // set initial colors
        for (int i = 0; i < tabs.Length; i++)
        {
            ColorBlock colors = tabs[i].colors;

            if (i == 0)
            {
                colors.normalColor = activeColor;
            }
            else
            {
                colors.normalColor = notActiveColor;
            }

            tabs[i].colors = colors;
        }
    }

    public void ChangeColor()
    {
        int buttonPressed = 0;

        // find out which tab has been pressed
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].gameObject.name == EventSystem.current.currentSelectedGameObject.name)
            {
                buttonPressed = i;
                break;
            }
        }

        // adjust the colors
        for (int i = 0; i < tabs.Length; i++)
        {
            ColorBlock colors = tabs[i].colors;

            if (i == buttonPressed)
            {
                colors.normalColor = activeColor;
            }
            else
            {
                colors.normalColor = notActiveColor;
            }

            tabs[i].colors = colors;
        }
    }
}
