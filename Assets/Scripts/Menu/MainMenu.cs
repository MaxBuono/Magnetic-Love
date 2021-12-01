using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace MenuManagement
{
    public class MainMenu : Menu<MainMenu>
    {
        public bool fadeToPlay = true;   // should it use the fading transition?
        
        public void OnSettingsPressed()
        {
            print("SETTINGS");
            SettingsMenu.Open();
        }

        public void OnCreditPressed()
        {
            print("CREDITS");
            CreditsScreen.Open();
        }
        
        public void OnSelectLevelPressed()
        {
            print("LEVEL SELECT");
            SelectLevelMenu.Open();
        }
        
        public void OnPlayPressed()
        {
            if (fadeToPlay)
            {
                StartCoroutine(OnPlayPressedRoutine());
            }
            else
            {
                LevelManager.LoadNextLevel();
                GameMenu.Open();
            }
        }

        private IEnumerator OnPlayPressedRoutine()
        {
            TransitionFader.PlayTransition(GameManager.Instance.fromMainToFirstLevel, GameManager.Instance.LevelNames[0]);
            yield return new WaitForSeconds(GameManager.Instance.fromMainToFirstLevel.FadeOnDuration + GameManager.Instance.fromMainToFirstLevel.delay);
            LevelManager.LoadFirstLevel();
            GameMenu.Open();
        }


        public override void OnBackPressed()
        {
            Application.Quit();
        }
    }
}