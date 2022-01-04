using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenuManagement
{
    public class SettingsMenuTabs : Menu<SettingsMenuTabs>
    {
        public GameObject[] objectsToSwap;
        public GameObject firstButton;

        private void OnEnable()
        {
            SetActivePage(0);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }

        public void SetActivePage(int pageNumber)
        {
            for (int i = 0; i < objectsToSwap.Length; i++)
            {
                if (i == pageNumber)
                {
                    objectsToSwap[i].SetActive(true);
                }
                else
                {
                    objectsToSwap[i].SetActive(false);
                }
            }
        }
    }
}
