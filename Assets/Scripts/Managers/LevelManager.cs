using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuManagement;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static int _nextLevel = 1;
    private static int _maxLevel = 8;
    private static int _levelPlayed = -1;

    private static string SceneName(int level)
    {
        return GameManager.Instance.Levels[level - 1];
    }
    
    public static int GetLevel()
    {
        return _levelPlayed;
    }

    public static void LoadLevel(int level)
    {
        if (level < _maxLevel)
        {
            _levelPlayed = level;
            _nextLevel = level + 1;
            SceneManager.LoadScene(SceneName(_levelPlayed));        
        }
    }

    public static void LoadFirstLevel()
    {
        _levelPlayed = 1;
        _nextLevel = 2;
        Debug.Log("first level -> " + _levelPlayed);
        SceneManager.LoadScene(SceneName(_levelPlayed));
    }

    public static bool CompletedAllLevels()
    {
        return (_nextLevel == _maxLevel);
    }
    
    public static void LoadNextLevel()
    {
        if (_nextLevel < _maxLevel)
        {
            _levelPlayed = _nextLevel;
            _nextLevel += 1;
            Debug.Log("level " + _levelPlayed);
            SceneManager.LoadScene(SceneName(_levelPlayed));
        }
    }
    
    public static void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void LoadMainMenuLevel()
    {
        SceneManager.LoadScene("Scenes/MainMenu");
        MainMenu.Open();
    }

    public static int GetLevelPlayed()
    {
        return _levelPlayed;
    }
}
