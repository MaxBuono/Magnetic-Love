using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuManagement
{
    public class SelectLevelMenu : Menu<SelectLevelMenu>
    {
        public bool fadeToPlay = true;
        public Button[] levelButtons;
        [SerializeField] private float playDelay = 0.5f;
        [SerializeField] private TransitionFader transitionFaderPrefab;
        
        public void OnLevelButtonPressed()
        {
            if (fadeToPlay)
            {
                StartCoroutine(OnPlayPressedRoutine());
            }
            else
            {
                LevelManager.LoadLevel(1);
                GameMenu.Open();
            }
        }
        
        private IEnumerator OnPlayPressedRoutine()
        {
            print("ACTIVATE THE TRANSITION FADER");
            TransitionFader.PlayTransition(transitionFaderPrefab);
            LevelManager.LoadLevel(1);
            yield return new WaitForSeconds(playDelay);
            GameMenu.Open();
        }

        private void Start()
        {
            //Level of the player
            int levelAt = 2;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                if (i > levelAt)
                {
                    levelButtons[i].interactable = false;
                }
            }
        }
    }
    
}
