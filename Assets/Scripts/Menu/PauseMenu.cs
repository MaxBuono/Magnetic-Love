using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace MenuManagement
{
    public class PauseMenu : Menu<PauseMenu>
    {
        public void OnResumePressed()
        {
            MenuManager.Instance.PauseMenuOpen = false;
            Time.timeScale = 1f;
            base.OnBackPressed();
        }

        public void OnRestartPressed()
        {
            MenuManager.Instance.PauseMenuOpen = false;
            Time.timeScale = 1f;
            base.OnBackPressed();
            LevelManager.ReloadLevel();
        }
        
        public void OnMainMenuPressed()
        {
            MenuManager.Instance.PauseMenuOpen = false;
            MenuManager.Instance.CloseMenu();
            MenuManager.Instance.ClearStack();
            Time.timeScale = 1f;
            LevelManager.LoadMainMenuLevel();
        }
        
        public void OnQuitPressed()
        {
            Application.Quit();
        }

        // lower the background music when pausing
        private void OnEnable()
        {
            float volume = AudioManager.Instance.MusicSource.volume;
            AudioManager.Instance.MusicSource.volume = volume * 0.5f;
        }

        private void OnDisable()
        {
            float volume = AudioManager.Instance.MusicSource.volume;
            AudioManager.Instance.MusicSource.volume = volume * 2.0f;
        }
    }
}
