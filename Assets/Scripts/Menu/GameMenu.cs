using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuManagement
{
    public class GameMenu : Menu<GameMenu>
    {
        //Not used, the button is disabled in the GameMenu in prefabs
        public void OnPausePressed()
        {
            Time.timeScale = 0f;
            PauseMenu.Open();
        }
        
    }
}
