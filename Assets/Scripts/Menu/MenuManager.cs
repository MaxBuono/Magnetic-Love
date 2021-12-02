using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MenuManagement
{
    public class MenuManager : MonoBehaviour
    {
        public bool enableAllLevels = false;

        [Header("Menu Prefabs")]
        public MainMenu mainMenuPrefab;

        public SelectLevelMenu selectLevelMenu;
        
        public SettingsMenu settingsMenuPrefab;

        public CreditsScreen creditsScreenPrefab;

        public GameMenu gameMenuPrefab;

        public PauseMenu pauseMenuPrefab;

        public LevelCompletedScreen levelCompletedScreen;

        public GameCompletedScreen gameCompletedScreen;
        
        
        [SerializeField] private Transform _menuParent;

        private Stack<Menu> _menuStack = new Stack<Menu>();

        // Handle pause menu specifically
        private bool _isPauseMenuOpen = false;
        public bool PauseMenuOpen { get { return _isPauseMenuOpen; } set { _isPauseMenuOpen = value; } }


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
                InitializeMenu();
                DontDestroyOnLoad(gameObject);
            }
            // base.Awake();
            
            // InitializeMenu();
        }

        private void Update()
        {
            // ****** ESC key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // if we are inside a level scene, open/close pause menu
                if (GameManager.Instance.IsLevelPlaying())
                {
                    if (!_isPauseMenuOpen)    // pause
                    {
                        Time.timeScale = 0f;
                        _isPauseMenuOpen = true;
                        PauseMenu.Open();
                    }
                    else    // resume
                    {
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
            
            Menu[] menuObjects = {mainMenuPrefab,selectLevelMenu,settingsMenuPrefab,creditsScreenPrefab, 
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
    }
}