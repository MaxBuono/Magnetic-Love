using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPlatform : MonoBehaviour
{
    //Externals
    public CharColor color;
    public GameObject heart;

    //Internals
    private bool isOnGoal = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<MagneticField>())
        {
            return;
        }
        if (other.GetComponent<CharacterData>().color == color)
        {
            heart.GetComponent<HeartManager>().setBooleans(color,true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<MagneticField>())
        {
            return;
        }
        if (other.GetComponent<CharacterData>().color == color)
        {
            heart.GetComponent<HeartManager>().setBooleans(color,false);
        }
    }
}
