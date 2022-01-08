using System;
using System.Collections;
using System.Collections.Generic;
using GameManagement.Data;
using MenuManagement;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public Button[] levelButtons;
    public Sprite notInteractableButtonSprite;
    public Sprite interactableButtonSprite;
    public Sprite levelCompletedButtonSprite;
    // Bonus Level
    public Sprite notInteractableBonus;
    public Sprite interactableBonus;

    private int _numOfTutorialLevels = 6;

    private DataManager _dataManager;
    public void Select(int level)
    {
        LevelManager.LoadLevel(level);
        GameMenu.Open();
    }

    private void Awake()
    {
        _dataManager = FindObjectOfType<DataManager>();
    }

    public void FromJson()
    {
        if (_dataManager == null)
            return;

        _dataManager.Load();

        //Level of the player
        int levelAt = _dataManager.LevelAt;
        bool[] levelCompleted = _dataManager.LevelCompleted;
        bool allLevelsCompleted = true;
        Debug.Log("Level of the player" + levelAt);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            // cheat to be able to select all levels
            if (MenuManager.Instance.enableAllLevels)
            {
                _numOfTutorialLevels = 0;
            }

            //Tutorial levels
            if (levelAt < _numOfTutorialLevels)
            {
                if (i > levelAt)
                {
                    levelButtons[i].interactable = false;
                    levelButtons[i].image.sprite = notInteractableButtonSprite;
                }
                else
                {
                    levelButtons[i].interactable = true;
                    levelButtons[i].image.sprite = interactableButtonSprite;
                }
            }
            else
            {
                levelButtons[i].interactable = true;
                levelButtons[i].image.sprite = interactableButtonSprite;
            }
            
            if (levelCompleted[i])
            {
                levelButtons[i].interactable = true;
                levelButtons[i].transform.localScale = new Vector3(1.2f, 1.2f, 1);
                levelButtons[i].image.sprite = levelCompletedButtonSprite;
            }
            else if (i != levelButtons.Length - 1)
            {
                allLevelsCompleted = false;
            }
            
            if (i == levelButtons.Length - 1 && !levelCompleted[i])
            {
                if (allLevelsCompleted)
                {
                    levelButtons[i].interactable = true;
                    levelButtons[i].image.sprite = interactableBonus;
                }
                else
                {
                    levelButtons[i].interactable = false;
                    levelButtons[i].image.sprite = notInteractableBonus;
                }
            }
        }
    }
}
