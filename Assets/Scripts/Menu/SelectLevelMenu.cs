using GameManagement.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MenuManagement
{
    public class SelectLevelMenu : Menu<SelectLevelMenu>
    {
        public LevelSelector LevelSelector;

        public void Select(int level)
        {
            LevelManager.LoadLevel(level);
            GameMenu.Open();
        }

        private void OnEnable()
        {
            LevelSelector.FromJson();
        }

    }
}
