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
    private Collider2D _coll;
    private Controller2D _controller2D;
    private MagneticObject _magneticObject;
    private Animator _animator;

    // animator hashes
    private int _isMovedHash;
    private int _isFallingHash;
    // used to correctly set the animator parameters
    private Vector3 currentPos;
    private Vector3 previousPos;


    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
        _controller2D = GetComponent<Controller2D>();
        _magneticObject = GetComponent<MagneticObject>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Register this component in the Game Manager
        GameManager.Instance.RegisterObjectMovement(_coll.GetInstanceID(), this);

        _gravity = GameManager.Instance.Gravity;

        // cache animator's parameters hashes
        _isMovedHash = Animator.StringToHash("isMoved");
        _isFallingHash = Animator.StringToHash("isFalling");
    }

    private void Update()
    {
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        previousPos = currentPos;
        currentPos = transform.position;

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
                    _velocity.y += _controller2D.collisionInfo.slopeNormal.y * -verticalForce;
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
        return _gravity * GameManager.Instance.gameVelocityMultiplier + _magneticObject.GetMagneticForce().y;
    }

    private void UpdateAnimator()
    {
        bool isMoved = Mathf.Abs(currentPos.x - previousPos.x) > 0.002f || Mathf.Abs(currentPos.y - previousPos.y) > 0.002f;
        bool isFalling = (previousPos.y - currentPos.y) > 0.01f;
        _animator.SetBool(_isMovedHash, isMoved);
        _animator.SetBool(_isFallingHash, isFalling);
    }

    public void AddVelocity(float velX, float velY = 0.0f)
    {
        _velocity.x = velX;
        _velocity.y += velY;
    }
}
