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
      if (_dataManager == null )
         return;
      
      _dataManager.Load();
            
      //Level of the player
      int levelAt = _dataManager.LevelAt;
      Debug.Log("Level of the player" + levelAt);
      
      for (int i = 0; i< levelButtons.Length; i++)
      {
         
         //Tutorial levels
         if (levelAt < 8)
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
         
      }
   }
}
