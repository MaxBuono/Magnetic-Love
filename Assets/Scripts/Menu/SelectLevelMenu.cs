using System.Collections;
using GameManagement.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MenuManagement
{
    public class SelectLevelMenu : Menu<SelectLevelMenu>
    {
        public static bool isTransitioning = false;
        public LevelSelector LevelSelector;

        public void Select(int level)
        {
            StartCoroutine(WaitBeforeLoad(level));
        }

        private void OnEnable()
        {
            LevelSelector.FromJson();
        }

        private IEnumerator WaitBeforeLoad(int level)
        {
            isTransitioning = true;
            TransitionFader.PlayTransition(GameManager.Instance.fromMainToFirstLevel, GameManager.Instance.LevelNames[level - 1]);
            yield return new WaitForSeconds(GameManager.Instance.fromMainToFirstLevel.FadeOnDuration + GameManager.Instance.fromMainToFirstLevel.delay);
            LevelManager.LoadLevel(level);
            GameMenu.Open();
            isTransitioning = false;
        }
    }
}
