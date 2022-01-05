using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuManagement;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static int _nextLevel = 1;
    private static int _maxLevel = SceneManager.sceneCountInBuildSettings - 1;
    private static int _levelPlayed = -1;
    
    private static string SceneName(int level)
    {
        return GameManager.Instance.Levels[level - 1];
    }

    private static void TransitionMusicBetweenLevels(int level)
    {
        // MUSIC TRANSITION
        float originalVolume = AudioManager.Instance.MusicSource.volume;
        LevelProperties properties = AudioManager.Instance.levelProperties[level - 1];
        // changing clip
        if (properties.music != AudioManager.Instance.MusicSource.clip)
        {
            AudioManager.Instance.TransitionToMusic(properties.music, originalVolume, 0.0f, properties.timeToGoToZero, 0.0f, properties.timeToGetBackToMax);
        }
    }

    public static void LoadLevel(int level)
    {
        if (level < _maxLevel)
        {
            _levelPlayed = level;
            _nextLevel = level + 1;
            Debug.Log(SceneName(_levelPlayed));
            SceneManager.LoadScene(SceneName(_levelPlayed));

            TransitionMusicBetweenLevels(level);
        }
    }

    public static void LoadFirstLevel()
    {
        _levelPlayed = 1;
        _nextLevel = 2;
        Debug.Log("first level -> " + _levelPlayed);
        SceneManager.LoadScene(SceneName(_levelPlayed));

        TransitionMusicBetweenLevels(_levelPlayed);
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
        MenuManager.Instance.ClearStack();
        SceneManager.LoadScene("Scenes/MainMenu_2");
        MainMenu.Open();

        // transition to main menu music
        AudioManager.Instance.TransitionToMusic(AudioManager.Instance.mainMenuClips.AudioClip, AudioManager.Instance.MusicSource.volume, 
                                                0.0f, 0.2f, 0.0f, 0.6f);
    }

    public static int GetLevelPlayed()
    {
        return _levelPlayed;
    }
}
