using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle the different kind of player movements

[RequireComponent(typeof(Controller2D))]
public class PlayerMovement : MonoBehaviour
{
    // Public
    public float moveSpeed = 6.0f;
    // we use this instead of a "jump speed" variable because they are more intuitive
    public float maxJumpHeight = 4.0f;
    public float minJumpHeight = 1.0f;
    public float timeToJumpApex = 0.5f;
    [Tooltip("Jump speed multiplier on the x axis (a value of 0.5 means that the character is jumping with half jump speed on the x axis.")]
    public float jumpWidth = 0.5f;
    // Wall jumping stuff
    public float wallSlideMaxSpeed = 3.0f;
    [Tooltip("Amount of time that you will stay sticked to the wall before actually jumping off if you are moving in the opposite direction from the wall")]
    // this allows to have a bit of time to perform a leap jump
    public float wallStickTime = 0.25f;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    // Internals
    private float _gravity;
    private float _maxJumpSpeed;
    private float _minJumpSpeed;
    private float _smoothedVelocityX;
    private float _accelerationTimeAirborne = 0.2f;
    private float _accelerationTimeGrounded = 0.1f;
    private float _timeToWallUnstick;
    private int _wallDirX;
    private bool _wallSliding;
    private Vector2 _velocity;
    private Controller2D _controller2D;
    private MagneticObject _magneticObject;
    private Vector2 _directionalInput;

    public Vector2 Velocity { get { return _velocity; } }

    private void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _magneticObject = GetComponent<MagneticObject>();

        // Custom gravity given by dx = v0*t + a*t^2 / 2 with dx = maxJumpHeight, v0 = 0 
        // acceleration = gravity and time = timetoJumpApex
        _gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
    }

    private void Start()
    {
        GameManager.Instance.Gravity = _gravity;

        // v1 = v0 + a*t (v0 = 0)
        _maxJumpSpeed = Mathf.Abs(_gravity * timeToJumpApex);
        // v1^2 = v0^2 + 2 * a * dx (v0 = 0)
        _minJumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);
    }

    void Update()
    {
        CalculateVelocity();
        HandleWallSliding();

        // The frame independent multiplication with deltaTime is done here
        _controller2D.Move(_velocity * Time.deltaTime, _directionalInput);

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
                    

                //_velocity.y += _controller2D.collisionInfo.slopeNormal.y * -_gravity * Time.deltaTime;
            }
            else
            {
                // this avoids gravity (or other constant forces) to some up to some crazy value
                _velocity.y = 0.0f;
            }
        }
    }

    // *************** INPUT FUNCTIONS ***************
    // Used by PlayerInput script to set directional inputs from there
    public void SetDirectionalInput(Vector2 input)
    {
        _directionalInput = input;
    }

    public void OnJumpOnCharacter()
    {
        
    }

    public void OnJumpInputDown()
    {
        // and I'm sliding on a wall
        if (_wallSliding)
        {
            // and my _directionalInput is going toward that wall
            if (_wallDirX == _directionalInput.x)
            {
                // then go to the opposite direction and up (to climb it)
                _velocity.x = -_wallDirX * wallJumpClimb.x;
                _velocity.y = wallJumpClimb.y;

                // modify the airborne acceleration to get different climbing "curves"
                //_accelerationTimeAirborne = _accelerationTimeAirborne * 0.5f;
            }
            // jump while not giving any horizontal _directionalInput
            else if (_directionalInput.x == 0)
            {
                _velocity.x = -_wallDirX * wallJumpOff.x;
                _velocity.y = wallJumpOff.y;
            }
            // _directionalInput going in the opposite direction from the wall
            else
            {
                _velocity.x = -_wallDirX * wallLeap.x;
                _velocity.y = wallLeap.y;
            }
        }


        // Are we jumping while side colliding the other character?
        //raycast to check collision with the other character since you may move in the opposite direction
        // (hence not seeing the collision) but still be attached thanks to magnetism
        bool collidingLeft = false;
        bool collidingRight = false;
        HashSet<Collider2D> colliders = new HashSet<Collider2D>();
        if (_directionalInput.x == 1)
        {
            colliders = _controller2D.RaycastHorizontally(Vector2.left);
            collidingLeft = colliders.Count != 0;
        }
        else if (_directionalInput.x == -1)
        {
            colliders = _controller2D.RaycastHorizontally(Vector2.right);
            collidingRight = colliders.Count != 0;
        }

        if (collidingLeft || collidingRight)
        {
            foreach (Collider2D coll in colliders)
            {
                PlayerMovement allyMovement = coll.GetComponent<PlayerMovement>();
                if (allyMovement != null)
                {
                    // in that case perform a side jump to unstick
                    _velocity.x = _maxJumpSpeed * jumpWidth * _directionalInput.x;
                    break;
                }
            }
        }


        //and I'm grounded (hitting below) while not pressing the downward _directionalInput
        if (_controller2D.collisionInfo.below && _directionalInput.y != -1)
        {
            if (_controller2D.collisionInfo.slidingDownMaxSlope)
            {
                // if not jumping against a max slope
                if (_directionalInput.x != -Mathf.Sign(_controller2D.collisionInfo.slopeNormal.x))
                {
                    _velocity.y = _maxJumpSpeed * _controller2D.collisionInfo.slopeNormal.y;
                    _velocity.x = _maxJumpSpeed * _controller2D.collisionInfo.slopeNormal.x;
                }
            }
            //jump normally
            else
            {
                _velocity.y = _maxJumpSpeed;
            } 
        }
    }

    public void OnJumpInputUp()
    {
        // if I'm going up, stop jumping up
        if (_velocity.y > _minJumpSpeed)
        {
            _velocity.y = _minJumpSpeed;
        }
    }
    // ************************************************************


    private void CalculateVelocity()
    {
        float targetVelocityX = _directionalInput.x * moveSpeed + CalculateHorizontalForce() * Time.deltaTime;

        // smooth the x velocity 
        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _smoothedVelocityX,
                                        _controller2D.collisionInfo.below ? _accelerationTimeGrounded : _accelerationTimeAirborne);
        _velocity.y += CalculateVerticalForce() * Time.deltaTime;
    }

    private void HandleWallSliding()
    {
        _wallSliding = false;
        _wallDirX = _controller2D.collisionInfo.left ? -1 : 1;

        if ((_controller2D.collisionInfo.left || _controller2D.collisionInfo.right)
            && !_controller2D.collisionInfo.below && _velocity.y < 0)
        {
            //Apply the Wall Sliding only on specific walls with the right tag
            // this also prevents the magnetic objects to slide in the air if they are near each other 
            HashSet<Collider2D> colliders = _controller2D.RaycastHorizontally(new Vector2(_wallDirX, 0));
            bool isSlidingWall = false;
            foreach (Collider2D coll in colliders)
            {
                if (coll.CompareTag("SlidingWall"))
                {
                    isSlidingWall = true;
                    break;
                }
            }
            if (!isSlidingWall) return;


            _wallSliding = true;

            // limit our sliding speed along walls
            if (_velocity.y < -wallSlideMaxSpeed)
            {
                _velocity.y = -wallSlideMaxSpeed;
            }

            if (_timeToWallUnstick > 0.0f)
            {
                _smoothedVelocityX = 0.0f;
                _velocity.x = 0.0f;

                // if _directionalInput is going in the opposite direction from the wall
                if (_directionalInput.x != _wallDirX && _directionalInput.x != 0)
                {
                    _timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    _timeToWallUnstick = wallStickTime;
                }
            }
            // reset the value so that the above if can start
            else
            {
                _timeToWallUnstick = wallStickTime;
            }
        }
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
