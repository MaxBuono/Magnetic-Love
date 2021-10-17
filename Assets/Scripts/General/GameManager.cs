using System.Collections;
using System.Collections.Generic;
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
    public (float, Vector2) gravity = (9.81f, Vector2.down);

    // Internals
    private bool _isGameOver = false;
    private Dictionary<int, MagneticObject> _magneticObjects = new Dictionary<int, MagneticObject>();
    private List<(float, Vector2)> _externalForces = new List<(float, Vector2)>();

    // Properties
    public Dictionary<int, MagneticObject> MagneticObjects { get { return _magneticObjects; } }
    public List<(float, Vector2)> ExternalForces { get { return _externalForces; } }

    private void Start()
    {
        _externalForces.Add(gravity);

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
}
