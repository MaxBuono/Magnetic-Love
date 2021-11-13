using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script emulates a magnetic field using a 2d collider of any shape.

public class MagneticField : MonoBehaviour
{
    // Public
    // A positive strength means repulsive force while negative an attractive force (given an object with positive polarization)
    public float strengthX;
    public float strengthY;

    // Internals
    private int _myID;
    private Collider2D _field;

    // Properties
    public int ID { get { return _myID; } }
    public Collider2D Field { get { return _field; } }

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

            //if (!isValidId)
            //{
            //    Debug.LogError(collision.gameObject.name + " ID was already registered!");
            //}
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
            Controller2D controller;
            if (GameManager.Instance.Controllers2D.TryGetValue(collisionID, out controller))
            {
                // if the collision object unregistered and started the coroutine RegisterForceWhenBelow
                // and then it exits from this field I have to stop it, otherwise I will unregister the id here
                // and then register it again when the coroutine finishes and the object is outside the field!
                if (controller.tillBelow != null)
                    controller.StopTillBelow();
            }

            // unregister this force on colission object
            bool isValidId = magneticObject.UnregisterForce(_myID);

            //if (!isValidId)
            //{
            //    Debug.LogError(collision.gameObject.name + " ID has not been found!");
            //}
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
            float squareDist = distanceVec.sqrMagnitude;
            Vector2 normalizedDist = distanceVec.normalized;
            float forceX = (normalizedDist * strengthX / squareDist).x * polarization;
            float forceY = (normalizedDist * strengthY / squareDist).y * polarization;

            return (magneticObject, new Vector2(forceX, forceY));
        }

        return (null, Vector2.zero);
    }
}
