using System;
using System.Collections;
using System.Collections.Generic;
using GameManagement.Data;
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

        private DataManager _dataManager;

        protected override void Awake()
        {
            base.Awake();
            _dataManager = FindObjectOfType<DataManager>();
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
            LoadData();
        }
        
        public void LoadData()
        {
            if (_dataManager == null )
                return;
            
            _dataManager.Load();
            
            //Level of the player
            int levelAt = _dataManager.LevelAt;
            Debug.Log("Level of the player" + levelAt);
            
            for (int i = 0; i< levelButtons.Length; i++)
            {
                if (i > levelAt)
                {
                    levelButtons[i].interactable = false;
                }
                else
                {
                    levelButtons[i].interactable = true;
                }
            }
        }
    }
    
}
