using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;

namespace MenuManagement
{
    public class MenuManager : MonoBehaviour
    {
        // Public
        public bool enableAllLevels = false;
        public bool enableStartingMenu = false;

        [Header("Menu Prefabs")]
        public MainMenu mainMenuPrefab;

        public SelectLevelMenu selectLevelMenu;
        
        //public SettingsMenu settingsMenuPrefab;

        public CreditsScreen creditsScreenPrefab;

        //public ControlsMenu controlsMenuPrefab;

        public SettingsMenuTabs settingsMenuTabs;

        public GameMenu gameMenuPrefab;

        public PauseMenu pauseMenuPrefab;

        public LevelCompletedScreen levelCompletedScreen;

        public GameCompletedScreen gameCompletedScreen;
        
        
        [SerializeField] private Transform _menuParent;

        // Internals
        private Stack<Menu> _menuStack = new Stack<Menu>();
        // used to handle nested menus inside the pause menu (e.g. setings)
        private int _pauseMenuStackCout;
        // Handle pause menu specifically
        private bool _isPauseMenuOpen = false;
        // Keyboard events
        private GameObject _mainButton;
        private GameObject _lastSelectedButton;

        // Properties
        public bool PauseMenuOpen { get { return _isPauseMenuOpen; } set { _isPauseMenuOpen = value; } }

        // Pseudo-Singleton
        private static MenuManager _instance;
        public static MenuManager Instance
        {
            get
            {
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                if (enableStartingMenu)
                {
                    InitializeMenu();
                }
            }
            // base.Awake();
            
            // InitializeMenu();
        }

        private void Start()
        {
            
            _mainButton = GameObject.FindGameObjectWithTag("MainInteractable");
            _lastSelectedButton = _mainButton;
        }

        private void Update()
        {
            // keep the selection if I click somewhere on the screen
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(_lastSelectedButton);
            }
            else
            {
                _lastSelectedButton = EventSystem.current.currentSelectedGameObject;
            }

            // ****** ESC key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // if we are inside a level scene
                if (GameManager.Instance.IsLevelPlaying())
                {
                    // open pause menu
                    if (!_isPauseMenuOpen)
                    {
                        Time.timeScale = 0f;
                        _isPauseMenuOpen = true;
                        PauseMenu.Open();
                        _pauseMenuStackCout = _menuStack.Count;
                    }
                    // closing a nested menu while game is still paused
                    else if (_menuStack.Count > _pauseMenuStackCout)
                    {
                        CloseMenu();
                    }
                    // close pause menu resuming the game
                    else
                    {
                        _pauseMenuStackCout = 0;
                        Time.timeScale = 1f;
                        _isPauseMenuOpen = false;
                        CloseMenu();
                    }
                }
                else    // main menu
                {
                    CloseMenu();
                }
            }
            // ******
        }

        public void OpenMenu(Menu menuInstance)
        {
            
            if (menuInstance == null)
            {
                Debug.LogWarning("MENU MANAGER: opening invalid menu");
                return;
            }
            
            if (_menuStack.Count > 0)
            {
                if (!(menuInstance is GameMenu))
                {
                     AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.buttonPressed);
                }
                foreach (Menu menu in _menuStack)
                {
                    menu.gameObject.SetActive(false);
                    
                }
            }

            menuInstance.gameObject.SetActive(true);
            _menuStack.Push(menuInstance);

            // select the main button if any
            SelectMainButton();

            print("STACK CONTAINS "+_menuStack.Count);
        }

        public void CloseMenu()
        {
            AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.buttonExitPressed);
            if (_menuStack.Count <= 1)
            {
                Debug.LogWarning("MENU MANAGER: no menu to close");
                return;
            }

            Menu topMenu = _menuStack.Pop();
            topMenu.gameObject.SetActive(false);

            if (_menuStack.Count > 0)
            {
                Menu nextMenu = _menuStack.Peek();
                nextMenu.gameObject.SetActive(true);
            }

            // select the main button if any
            SelectMainButton();
        }

        public void ClearStack()
        {
            while (_menuStack.Count > 1)
            {
                _menuStack.Pop();
            }
        }

        
        private void InitializeMenu()
        {
            
            if (_menuParent == null)
            {
                print("CREA MENUS GAME OBJECT");
                GameObject menuParentObject = new GameObject("Menus");
                menuParentObject.transform.position = Vector3.zero;
                _menuParent = menuParentObject.transform;
                DontDestroyOnLoad(_menuParent.gameObject);
            }

            // keeps all the menu active during the scene switch
            
            Menu[] menuObjects = {mainMenuPrefab,selectLevelMenu,settingsMenuTabs, creditsScreenPrefab,
                gameMenuPrefab,pauseMenuPrefab,levelCompletedScreen,gameCompletedScreen};

            for (int menu = 0; menu < menuObjects.Length; menu++)
            {
                Menu menuInstance = Instantiate(menuObjects[menu],_menuParent);
                
                if (menuObjects[menu] != mainMenuPrefab)
                {
                    menuInstance.gameObject.SetActive(false);
                }
                else
                {
                    OpenMenu(menuInstance);
                }
            }
            
        }

        private void SelectMainButton()
        {
            GameObject button = GameObject.FindGameObjectWithTag("MainInteractable");
            if (button != null)
            {
                EventSystem.current.SetSelectedGameObject(button);
            }
        }
    }
}