using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle the different kind of a (magnetic) object movements

[RequireComponent(typeof(Controller2D))]
public class ObjectMovement : MonoBehaviour
{
    // Internals
    private float _gravity;
    private float _smoothedVelocityX;
    private float _accelerationTimeAirborne = 0.2f;
    private Vector2 _velocity;
    private Controller2D _controller2D;
    private MagneticObject _magneticObject;


    private void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _magneticObject = GetComponent<MagneticObject>();
    }

    private void Start()
    {
        _gravity = GameManager.Instance.Gravity;
    }

    void FixedUpdate()
    {
        if (_gravity == 0)
        {
            _gravity = GameManager.Instance.Gravity;
        }

        CalculateVelocity();
 
        // The frame independent multiplication with deltaTime is done here
        _controller2D.Move(_velocity * GameManager.Instance.gameVelocityMultiplier);

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
        _velocity.y += CalculateVerticalForce();
    }

    // Returns the sum of all the external forces applied to the player on the x axis
    private float CalculateHorizontalForce()
    {
        return _magneticObject.GetMagneticForce().x;
    }

    // Returns the sum of all the external forces applied to the player on the y axis
    private float CalculateVerticalForce()
    {
        return _gravity + _magneticObject.GetMagneticForce().y;
    }
}
