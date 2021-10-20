using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script emulates a magnetic field using a 2d collider of any shape.

public class MagneticField : MonoBehaviour
{
    // Public
    [Tooltip("A positive strength means repulsive force while negative an attractive force (given an object with positive polarization).")]
    public float magneticStrength;

    // Internals
    private int _myID;
    private Collider2D _field;


    private void Awake()
    {
        // Cache components
        _field = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Cache your own id (representing the force this object applies to other magnetic objects)
        _myID = _field.GetInstanceID();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        (MagneticObject, Vector2) tuple = ComputeForce(collision);
        if (tuple.Item1 != null)
        {
            bool isValidId = tuple.Item1.RegisterForce(_myID, tuple.Item2);

            if (!isValidId)
                Debug.LogError(collision.gameObject.name + " ID was already registered!");
        }
            
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        (MagneticObject, Vector2) tuple = ComputeForce(collision);
        if (tuple.Item1 != null)
            tuple.Item1.UpdateForce(_myID, tuple.Item2);
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        int collisionID = collision.GetInstanceID();
        MagneticObject magneticObject;
        if (GameManager.Instance.MagneticObjects.TryGetValue(collisionID, out magneticObject))
        {
            // unregister this force on colission object
            bool isValidId = magneticObject.UnregisterForce(_myID);

            if (!isValidId)
                Debug.LogError(collision.gameObject.name + " ID has not been found!");
        }
    }

    private (MagneticObject, Vector2) ComputeForce(Collider2D coll)
    {
        // if the object that just entered is a magnetic object...
        int collisionID = coll.GetInstanceID();
        MagneticObject magneticObject;
        if (GameManager.Instance.MagneticObjects.TryGetValue(collisionID, out magneticObject))
        {
            // check for polarization
            int polarization = magneticObject.polarization == Polarization.positive ? 1 : -1;

            //...then compute the force applied to it
            Vector2 distanceVec = (coll.transform.position - transform.position);
            Vector2 force = (distanceVec.normalized * magneticStrength / distanceVec.sqrMagnitude) * polarization;

            return (magneticObject, force);
        }

        return (null, Vector2.zero);
    }
}
