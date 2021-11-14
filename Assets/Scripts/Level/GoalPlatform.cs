using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharColor { Red, Blue}

public class GoalPlatform : MonoBehaviour
{
    //Externals
    public CharColor color;
    public GameObject heart;

    //Internals
    private bool isOnGoal = false;
    private HeartManager _heartManager;
    private string _tagToCompare;

    private void Start()
    {
        _heartManager = heart.GetComponent<HeartManager>();
        _tagToCompare = "Player" + color.ToString();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(_tagToCompare))
        {
            _heartManager.setIsOnGoal(color,true); 
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(_tagToCompare))
        {
            _heartManager.setIsOnGoal(color,false); 
        }
    }
}
