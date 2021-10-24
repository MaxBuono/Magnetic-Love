using System.Collections;
using System.Collections.Generic;
using MenuManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace MenuManagement
{
    public class MenuManager : Singleton<MenuManager>
    {
        public MainMenu mainMenuPrefab;

        public SettingsMenu settingsMenuPrefab;

        public CreditsScreen creditsScreenPrefab;

        private Stack<Menu> _menusStack = new Stack<Menu>();

        private Transform _menuParent;

        private void InitializeMenu()
        {
            Menu[] menus = new Menu[] {mainMenuPrefab, settingsMenuPrefab, creditsScreenPrefab};

            if (_menuParent == null)
            {
                GameObject parentMenu = new GameObject("Menu");
                parentMenu.transform.position = Vector3.zero;
                _menuParent = parentMenu.transform;
                DontDestroyOnLoad(_menuParent.gameObject);
            }

            foreach (var menu in menus)
            {
                Menu menuInstance = Instantiate(menu, _menuParent);
                
                if (menu != mainMenuPrefab)
                {
                    menuInstance.gameObject.SetActive(false);
                }
                else
                {
                    OpenMenu(menu);
                }
            }
        }

        public void OpenMenu(Menu menuInstance)
        {
            if (menuInstance != null)
            {
                foreach (var menu in _menusStack)
                {
                    menu.gameObject.SetActive(false);
                }

                menuInstance.gameObject.SetActive(true);
                _menusStack.Push(menuInstance);
            }
        }

        public void CloseMenu()
        {
            if (_menusStack.Count > 0)
            {
                Menu topMenu = _menusStack.Pop();
                topMenu.gameObject.SetActive(false);

                Menu currentMenu = _menusStack.Peek();
                currentMenu.gameObject.SetActive(true);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            InitializeMenu();
        }

        // Update is called once per frame
        void Update()
        { }
    }
}
