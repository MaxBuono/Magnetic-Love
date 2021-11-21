using System;
using System.Collections;
using System.Collections.Generic;
using MenuManagement;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
   public void Select(int level)
   {
      LevelManager.LoadLevel(level);
      GameMenu.Open();
   }
}
