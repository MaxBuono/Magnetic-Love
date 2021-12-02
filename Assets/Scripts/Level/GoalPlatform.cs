using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharColor { Red, Blue}

public class GoalPlatform : MonoBehaviour
{
    //Externals
    public CharColor color;

    //Internals
    private bool isOnGoal = false;
    private HeartManagerSplit _heartManager;
    private string _tagToCompare;

    private void Awake()
    {
        _heartManager = GameObject.FindGameObjectWithTag("Heart").GetComponent<HeartManagerSplit>();
    }

    private void Start()
    {
        _tagToCompare = "Player" + color.ToString();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(_tagToCompare))
        {
            _heartManager.SetIsOnGoal(color,true); 
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(_tagToCompare))
        {
            _heartManager.SetIsOnGoal(color,false); 
        }
    }
}
