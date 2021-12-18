using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameManagement.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

// NOTE: the game manager is supposed to handle everything that requires
// a higher level management, usually aspects not directly related
// to the game objects themselves (e.g. game loop, registering colliders IDs and so on). 

public class GameManager : MonoBehaviour
{
    // Singleton
    private GameManager() { }
    private static GameManager _instance = null;
    private DataManager _dataManager;

    public static GameManager Instance { get { return _instance; } }

    private void Awake()
    {
        //Singleton
        if (_instance == null)
        {
            _instance = FindObjectOfType<GameManager>();

            if (_instance == null)
            {
                GameObject sceneManager = new GameObject("Game Manager");
                _instance = sceneManager.AddComponent<GameManager>();
            }
        }

        DontDestroyOnLoad(gameObject);

        // cache all the level names to use in the LevelManager script
        string nameFilter = @"(\w|\s|'|,)+$";
        Regex filter = new Regex(nameFilter);
        foreach (string str in _levels)
        {
            Match nameMatch = filter.Match(str);
            _levelNames.Add(nameMatch.Value);
        }

        // test different frame rates
        //QualitySettings.vSyncCount = 0;  // VSync must be disabled
        //Application.targetFrameRate = 1000;

        // Cache Objects
        _dataManager = FindObjectOfType<DataManager>();
    }


    // Public/Serialized
    public TransitionFader fromMainToFirstLevel;
    public TransitionFader fromLevelToLevel;
    public TransitionFader fromLastToMain;
    public float gameVelocityMultiplier = 0.04f;

    [SerializeField] private List<string> _levels = new List<string>();

    // Internals
    private float _gravity = 0.0f;
    private bool _isGameOver = false;
    private bool _playerControlsBlocked = false;
    private float _cameraSizeRatio;
    // Dictionaries used to save performances by caching components avoiding getting them at runtime
    private Dictionary<int, MagneticObject> _magneticObjects = new Dictionary<int, MagneticObject>();
    private Dictionary<int, Controller2D> _controllers2D = new Dictionary<int, Controller2D>();
    private List<string> _levelNames = new List<string>();

    // Properties
    public Dictionary<int, MagneticObject> MagneticObjects { get { return _magneticObjects; } }
    public Dictionary<int, Controller2D> Controllers2D { get { return _controllers2D; } }
    public float Gravity { get { return _gravity; } set { _gravity = value; } }
    public bool PlayerControlsBlocked { get { return _playerControlsBlocked; } set { _playerControlsBlocked = value; } }
    public List<string> Levels { get { return _levels; } }
    public List<string> LevelNames { get { return _levelNames; } }

    private void Start()
    {
        // game loop
        StartCoroutine(GameLoop());
    }

    private void Update()
    {

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // On scene loaded adjust camera size
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AdjustCameraSize();
    }

    // Manage the whole match cycle
    private IEnumerator GameLoop()
    {
        // Play
        while (!_isGameOver)
        {
            yield return null;
        }

        // Handle the game over conditions
        // ... do something ...
        ExitMatch();
    }

    private void ExitMatch()
    {
        SceneManager.LoadScene(0);
    }

    private IEnumerator LoadNextLevelRoutine(string str = "")
    {
        TransitionFader.PlayTransition(fromLevelToLevel, str);

        float fadeDelay = (fromLevelToLevel != null) ?
            fromLevelToLevel.delay + fromLevelToLevel.FadeOnDuration : 0f;

        yield return new WaitForSeconds(fadeDelay);
        LevelManager.LoadNextLevel();
    }

    private IEnumerator GameCompletedRoutine()
    {
        TransitionFader.PlayTransition(fromLastToMain, "Congratulation, you completed the game!\n\n" +
                                                            "Thank you for playing :)");

        float fadeDelay = (fromLastToMain != null) ?
            fromLastToMain.delay + fromLastToMain.FadeOnDuration : 0f;

        yield return new WaitForSeconds(fadeDelay);
        //GameCompletedScreen.Open();

        LevelManager.LoadMainMenuLevel();
    }

    // used to dynamically adjust the camera size based on player's resolution
    private void AdjustCameraSize()
    {
        Camera mainCamera = Camera.main;
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        // in the first run we tune the sizeRatio for a 16:9 ratio with a specific size
        if (_cameraSizeRatio == 0)
        {
            _cameraSizeRatio = mainCamera.orthographicSize * (16.0f / 9.0f);
        }
        mainCamera.orthographicSize = _cameraSizeRatio / aspectRatio;
    }

    // Public Methods

    public bool RegisterMagneticObject(int id, MagneticObject magneticObject)
    {
        if (!_magneticObjects.ContainsKey(id))
        {
            _magneticObjects.Add(id, magneticObject);
            return true;
        }

        return false;
    }

    public bool RegisterController2D(int id, Controller2D controller2D)
    {
        if (!_controllers2D.ContainsKey(id))
        {
            _controllers2D.Add(id, controller2D);
            return true;
        }

        return false;
    }
    
    public void LevelCompleted()
    {
        int levelAt = LevelManager.GetLevelPlayed();
        
        if (_dataManager != null)
        {
            if (levelAt > _dataManager.LevelAt)
            {
                _dataManager.LevelAt = levelAt;
                _dataManager.Save();
            }
            
        }
        
        if (!LevelManager.CompletedAllLevels())
        {
            StartCoroutine(LoadNextLevelRoutine(_levelNames[LevelManager.GetLevelPlayed()]));
            AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.completedLevel);
        }
        else
        {
            print("GAME COMPLETED ROUTINE???");
            StartCoroutine(GameCompletedRoutine());
        }
    }

    // returns false when you are not in a level scene
    public bool IsLevelPlaying(string sceneName = "")
    {
        string pattern = @"L\d+-\w*";
        Regex rg = new Regex(pattern);
        string name = sceneName == "" ? SceneManager.GetActiveScene().name : sceneName;
        return rg.IsMatch(name);        
    }
       
}
