using System.Collections;
using System.Collections.Generic;
using MenuManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimpleGame
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private TransitionFader endTransitionPrefab;     // transition screen for ending
        
        public void LevelCompleted()
        {
            // - disable level controls
            //   here all the active components of the level should be blocked while
            //   the next screen level goes on
            // - save the data
            //   are there any statistics that need to be saved?

            if (!LevelManager.CompletedAllLevels())
            {
                StartCoroutine(LevelCompletedRoutine());
            }
            else
            {
                print("GAME COMPLETED ROUTINE???");
                StartCoroutine(GameCompletedRoutine());
            }
        }
        
        private IEnumerator LevelCompletedRoutine()
        {
            TransitionFader.PlayTransition(endTransitionPrefab);

            float fadeDelay  = (endTransitionPrefab != null) ?
                endTransitionPrefab.Delay + endTransitionPrefab.FadeOnDuration : 0f;
            
            yield return new WaitForSeconds(fadeDelay);
            LevelCompletedScreen.Open();
        }
        
        private IEnumerator GameCompletedRoutine()
        {
            TransitionFader.PlayTransition(endTransitionPrefab);

            float fadeDelay  = (endTransitionPrefab != null) ?
                endTransitionPrefab.Delay + endTransitionPrefab.FadeOnDuration : 0f;
            
            yield return new WaitForSeconds(fadeDelay);
            GameCompletedScreen.Open();
        }

    }
}