using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ButtonDoor : MonoBehaviour
{
    //Externals
    public CharColor color;
    public bool needContinuosPressure = false;
    public Door door;
    
    //Internals
    private bool _isClicked = false;
    private Rigidbody2D parentRB;
    private Transform parentTransform;
    private string _tagToCompare;

    private void Start()
    {
        parentRB = GetComponentInParent<Rigidbody2D>();
        parentTransform = GetComponentInParent<Transform>();
        _tagToCompare = "Player" + color.ToString();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(_tagToCompare))
        {
            _isClicked = true;
            //Click button
            //Something here
            //Open door
            door.openDoor();    
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (needContinuosPressure)
        {
            if (other.CompareTag(_tagToCompare))
            {
                _isClicked = false;
                //Release Button
                    //Something here
                //Close door
                door.closeDoor();
            }
        }
    }
}
