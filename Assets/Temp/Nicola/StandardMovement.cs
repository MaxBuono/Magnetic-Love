using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle the different kind of a (magnetic) object movements

[RequireComponent(typeof(Controller2D))]
public class StandardMovement : MonoBehaviour
{
    // Internals
    private float _gravity;
    private float _smoothedVelocityX;
    private float _accelerationTimeAirborne = 0.2f;
    private Vector2 _velocity;
    private Controller2D _controller2D;
    private Collider2D _coll;

    private Dictionary<int, PlayerMovement> _playerMovements = new Dictionary<int, PlayerMovement>();


    private void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _coll = GetComponent<Collider2D>();
    }

    private void Start()
    {
        _gravity = GameManager.Instance.Gravity;
        _velocity.x = 0;
    }

    void Update()
    {
        if (_gravity == 0)
            _gravity = GameManager.Instance.Gravity;

        CalculateVelocity();

        // The frame independent multiplication with deltaTime is done here
        _controller2D.Move(_velocity * Time.deltaTime);

        // we call this at the end because if for instance we are on a moving platform, it's going
        // to call Move too, potentially altering the below/above values.
        if (_controller2D.collisionInfo.below || _controller2D.collisionInfo.above)
        {
            if (_controller2D.collisionInfo.slidingDownMaxSlope)
            {
                // slow down the fall only if the vertical force is negative
                float verticalForce = CalculateVerticalForce();
                if (Mathf.Sign(verticalForce) < 0)
                {
                    // the steeper the slope, the smaller will be the slopeNormal.y countering vertical forces
                    _velocity.y += _controller2D.collisionInfo.slopeNormal.y * -verticalForce * Time.deltaTime;
                }
            }
            else
            {
                // this avoids gravity (or other constant forces) to some up to some crazy value
                _velocity.y = 0.0f;
            }
        }
    }


    private void CalculateVelocity()
    {
        float targetVelocityX = CalculateHorizontalForce();

        // smooth the x velocity 
        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _smoothedVelocityX, _accelerationTimeAirborne);
        _velocity.y += CalculateVerticalForce() * Time.deltaTime;
    }

    // Returns the sum of all the external forces applied to the player on the x axis
    private float CalculateHorizontalForce()
    {
        return _velocity.x;
    }

    // Returns the sum of all the external forces applied to the player on the y axis
    private float CalculateVerticalForce()
    {
        return _gravity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerMovement movement = collision.transform.GetComponent<PlayerMovement>();
        Vector2 velocity;
        if (movement != null)
        {
            _playerMovements.Add(collision.collider.GetInstanceID(), movement);

            Vector2 topLeft = new Vector2(_coll.bounds.min.x, _coll.bounds.max.y);
            Vector2 topCenter = new Vector2(_coll.bounds.center.x, _coll.bounds.max.y);
            Vector2 topRight = new Vector2(_coll.bounds.max.x, _coll.bounds.max.y);

            RaycastHit2D hitLeft = Physics2D.Raycast(topLeft, Vector2.up, 0.1f, _controller2D.collisionMask);
            RaycastHit2D hitCenter = Physics2D.Raycast(topCenter, Vector2.up, 0.1f, _controller2D.collisionMask);
            RaycastHit2D hitRight = Physics2D.Raycast(topRight, Vector2.up, 0.1f, _controller2D.collisionMask);

            if (hitLeft || hitCenter || hitRight)
            {
                _velocity.x = movement.Velocity.x;
            }
        }
            
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        PlayerMovement movement;
        if (_playerMovements.TryGetValue(collision.collider.GetInstanceID(), out movement))
        {
            Vector2 topLeft = new Vector2(_coll.bounds.min.x, _coll.bounds.max.y);
            Vector2 topCenter = new Vector2(_coll.bounds.center.x, _coll.bounds.max.y);
            Vector2 topRight = new Vector2(_coll.bounds.max.x, _coll.bounds.max.y);

            RaycastHit2D hitLeft = Physics2D.Raycast(topLeft, Vector2.up, 0.015f, _controller2D.collisionMask);
            RaycastHit2D hitCenter = Physics2D.Raycast(topCenter, Vector2.up, 0.015f, _controller2D.collisionMask);
            RaycastHit2D hitRight = Physics2D.Raycast(topRight, Vector2.up, 0.015f, _controller2D.collisionMask);

            if (hitLeft || hitCenter || hitRight)
            {
                _velocity.x = movement.Velocity.x;
            }
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        int id = collision.collider.GetInstanceID();
        if (_playerMovements.ContainsKey(id))
        {
            _velocity.x = 0.0f;
            _playerMovements.Remove(id);
        }
    }
}
