using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Button : MonoBehaviour
{
    //Externals
    public CharColor color;
    public bool needContinuosPressure = false;
    public Door door;
    
    //Internals
    private bool _isClicked = false;
    private Rigidbody2D parentRB;
    private Transform parentTransform;

    private void Start()
    {
        parentRB = GetComponentInParent<Rigidbody2D>();
        parentTransform = GetComponentInParent<Transform>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<MagneticField>())
        {
            return;
        }
        if (other.GetComponent<CharacterData>().color == color)
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
            if (other.GetComponent<MagneticField>())
            {
                return;
            }

            if (other.GetComponent<CharacterData>().color == color)
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
