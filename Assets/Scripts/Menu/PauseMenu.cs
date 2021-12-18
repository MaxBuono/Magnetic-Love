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
            if (AudioManager.Instance != null)
            {
                float volume = AudioManager.Instance.MusicSource.volume;
                AudioManager.Instance.SetMusicVolume(volume * 0.4f);
            }

        }

        private void OnDisable()
        {
            if (AudioManager.Instance != null)
            {
                float volume = AudioManager.Instance.MusicSource.volume;
                AudioManager.Instance.SetMusicVolume(volume * 2.5f);
            }
        }
    }
}
