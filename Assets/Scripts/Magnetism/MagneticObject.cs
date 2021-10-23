using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based on this polarization the same magnet will affect this object with an attractive or a repulsive force
public enum Polarization { positive, negative}


[RequireComponent(typeof(Rigidbody2D))]
public class MagneticObject : MonoBehaviour
{
    // Public
    public Polarization polarization;
    public float drag = 0.95f;

    // Internals
    private Vector2 _pOld;
    private Vector2 _pNow;
    private Rigidbody2D _rb2d;
    private Collider2D _coll;

    // Forces
    // the int represent the collider id of the object causing that force
    private Dictionary<int, Vector2> _forces = new Dictionary<int, Vector2>();


    private void Awake()
    {
        // Cache components
        _rb2d = GetComponent<Rigidbody2D>();
        _coll = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Register this component in the Game Manager
        GameManager.Instance.RegisterMagneticObject(_coll.GetInstanceID(), this);

        // Be sure that the rigidbody is setup correctly
        _rb2d.isKinematic = true;

        // Set starting positions
        _pNow = _rb2d.position;
        _pOld = _pNow;    // starts still
    }

    //void FixedUpdate()
    //{
    //    //_pNow = _rb2d.position;

    //    //// Get the acceleration by summing all the external forces and the dump
    //    //Vector2 accel = Vector2.zero;
    //    //foreach (var force in _forces)
    //    //{
    //    //    accel += force.Value;
    //    //}
    //    //float dump = 1 - drag * Time.fixedDeltaTime;

    //    //// Verlet integration method used to get the next position
    //    //Vector2 pNext = (1 + dump) * _pNow - dump * _pOld + accel * Mathf.Pow(Time.fixedDeltaTime, 2);

    //    //// Check that the next position is a valid one (if it should detect collisions)
    //    ////if (_detectCollision != null)
    //    //    //pNext = _detectCollision.CheckForCollision(_pNow, pNext, _coll);

    //    //// Update the positions
    //    //_pOld = _pNow;
    //    //_pNow = pNext;

    //    // Finally move the object
    //    //_rb2d.MovePosition(pNext);
    //}

    public Vector2 GetNextPos()
    {
        _pNow = _rb2d.position;
        Vector2 accel = Vector2.zero;

        foreach (var force in _forces)
        {
            accel += force.Value;
        }
        float dump = 1 - drag * Time.fixedDeltaTime;

        // Verlet integration method used to get the next position
        Vector2 pNext = (1 + dump) * _pNow - dump * _pOld + accel * Mathf.Pow(Time.fixedDeltaTime, 2);

        _pOld = _pNow;
        _pNow = pNext;

        return _pNow - _pOld;
    }
    

    // When you enter in a magnetic field, register that new force
    public bool RegisterForce(int id, Vector2 force)
    {
        if (!_forces.ContainsKey(id))
        {
            _forces.Add(id, force);
            return true;
        }

        return false;
    }

    // Update the current force applied to this rigidbody
    public Vector2 UpdateForce(int id, Vector2 force)
    {
        if (_forces.ContainsKey(id))
        {  
            _forces[id] = force;
            return force;
        }

        return Vector2.zero;
    }

    // Unregister a force
    public bool UnregisterForce(int id)
    {
        if (_forces.ContainsKey(id))
        {
            _forces.Remove(id);
            return true;
        }

        return false;
    }

}
