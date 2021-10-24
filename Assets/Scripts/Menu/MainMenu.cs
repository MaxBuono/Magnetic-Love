using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuManagement
{
    public class MainMenu : Menu<MainMenu>
    {
        public void OnPlayPressed()
        {
            
        }

        public void OnSettingsPressed()
        {
            if (MenuManager.Instance != null && SettingsMenu.Instance != null)
            {
                MenuManager.Instance.OpenMenu(SettingsMenu.Instance);
            }
        }
        
        public void OnCreditsPressed()
        {
            if (MenuManager.Instance != null && CreditsScreen.Instance != null)
            {
                MenuManager.Instance.OpenMenu(CreditsScreen.Instance);
            }
        }

        public override void OnBackPressed()
        {
            Application.Quit();
        }
       
    }
}
