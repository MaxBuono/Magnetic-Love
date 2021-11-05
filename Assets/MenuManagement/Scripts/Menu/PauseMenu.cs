using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleGame;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace MenuManagement
{
    public class PauseMenu : Menu<PauseMenu>
    {
        public void OnResumePressed()
        {
            Time.timeScale = 1f;
            base.OnBackPressed();
        }

        public void OnRestartPressed()
        {
            Time.timeScale = 1f;
            base.OnBackPressed();
            LevelManager.ReloadLevel();
        }
        
        public void OnMainMenuPressed()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MenuManagement/Scenes/MainMenu");
            MainMenu.Open();
        }
        
        public void OnQuitPressed()
        {
            Application.Quit();
        }
    }
}
