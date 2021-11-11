using System.Collections;
using System.Collections.Generic;
using MenuManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// NOTE: the game manager is supposed to handle everything that requires
// a higher level management, usually aspects not directly related
// to the game objects themselves (e.g. game loop, registering colliders IDs and so on). 

public class GameManager : MonoBehaviour
{   
    [SerializeField] private TransitionFader endTransitionPrefab;     // transition screen for ending
    // Singleton
    private GameManager() { }
    private static GameManager _instance = null;

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
    }


    // Public


    // Internals
    private float _gravity = 0.0f;
    private bool _isGameOver = false;
    // Dictionaries used to save performances by caching components avoiding getting them at runtime
    private Dictionary<int, MagneticObject> _magneticObjects = new Dictionary<int, MagneticObject>();
    private Dictionary<int, Controller2D> _controllers2D = new Dictionary<int, Controller2D>();

    // Properties
    public Dictionary<int, MagneticObject> MagneticObjects { get { return _magneticObjects; } }
    public Dictionary<int, Controller2D> Controllers2D { get { return _controllers2D; } }
    public float Gravity { get { return _gravity; } set { _gravity = value; } }

    private void Start()
    {
        StartCoroutine(GameLoop());
    }

    private void Update()
    {

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
        LevelManager.LoadNextLevel();
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
