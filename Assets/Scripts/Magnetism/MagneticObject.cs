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
    [Tooltip("A value of one means that the object receives full strenght force while zero means no force received at all.")]
    [Range(0, 1)]
    public float forceReceived = 1.0f;

    // Internals
    private Rigidbody2D _rb2d;
    private Collider2D _coll;
    private float _diagonalLength;

    // Forces
    // the int represent the collider id of the object causing that force
    private Dictionary<int, Vector2> _forces = new Dictionary<int, Vector2>();

    // Properties
    public float DiagonalLength { get { return _diagonalLength; } }

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

        // calculate the (square) length of this collider diagonal
        _diagonalLength = new Vector3(_coll.bounds.extents.x, _coll.bounds.extents.y, 0).magnitude;
    }

    // returns the resulting magnetic force (aka acceleration because we aren't considering any mass)
    public Vector2 GetMagneticForce()
    {
        Vector2 acceleration = Vector2.zero;

        foreach (var force in _forces)
        {
            acceleration += force.Value * forceReceived;
        }

        return acceleration;
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
