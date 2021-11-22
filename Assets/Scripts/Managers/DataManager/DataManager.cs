using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameManagement.Data
{
        
    public class DataManager : MonoBehaviour
    {
        private SaveData _saveData;
        private JsonSaver _jsonSaver;

        public int LevelAt
        {
            get { return _saveData.LevelAt;  }
            set { _saveData.LevelAt = value; }
        }

        private void Awake()
        {
            _saveData = new SaveData();
            _jsonSaver = new JsonSaver();
        }

        public void Save()
        {
            _jsonSaver.Save(_saveData);
            Debug.Log("Save on json");
        }

        public void Load()
        {
            _jsonSaver.Load(_saveData);
            Debug.Log(_saveData);
        }
        
        private void OnApplicationQuit()
        {
            _jsonSaver.Save(_saveData);
        }
    }

}
