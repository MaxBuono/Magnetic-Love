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
    [Tooltip("Maximum height at which the character will arrive when jumping. Modifies gravity.")]
    public float maxJumpHeight = 4.0f;
    [Tooltip("Minimum height at which the character will arrive when jumping.")]
    public float minJumpHeight = 1.0f;
    [Tooltip("Time to get to the maximum height. Modifies gravity.")]
    public float timeToJumpApex = 0.5f;
    [Tooltip("Jump speed multiplier on the x axis (a value of 0.5 means that the character is jumping with half jump speed on the x axis.")]
    public float jumpWidth = 0.5f;
    [Tooltip("Amount of vertical force you get when jumping (keeping it pressed) on the other character")]
    public float jumpPushForce = 1.7f;
    // Wall jumping stuff
    public float wallSlideMaxSpeed = 3.0f;
    [Tooltip("Amount of time that you will stay sticked to the wall before actually jumping off if you are moving in the opposite direction from the wall")]
    // this allows to have a bit of time to perform a leap jump
    public float wallStickTime = 0.25f;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    // sticked characters variables
    [HideInInspector] public bool isJumping;
    [HideInInspector] public bool isStickToAlly;
    [HideInInspector] public bool isAboveCharacter;
    [HideInInspector] public float resultingVelX;
    [HideInInspector] public float resultingVelY;

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
    private bool _wasInAir;
    private Vector2 _velocity;
    private Controller2D _controller2D;
    private Collider2D _collider;
    private MagneticObject _magneticObject;
    private PlayerMovement _allyMovement;
    private MagneticField _allyField;
    private Animator _animator;
    private Vector2 _directionalInput;
    private IEnumerator _pushUpCoroutine = null;

    // animators hashes
    private int _moveSpeedHash;
    private int _isGroundedHash;
    private int _startJumpHash;
    private int _endJumpHash;


    public Vector2 Velocity { get { return _velocity; } set { _velocity = value; } }
    public Controller2D Controller { get { return _controller2D; } }
    public Vector2 DirectionalInput { get { return _directionalInput; } }
    public float MaxJumpSpeed { get { return _maxJumpSpeed; } }
    public MagneticObject MagneticObject { get { return _magneticObject; } }
    public MagneticField AllyField { get { return _allyField; } }

    private void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _collider = GetComponent<Collider2D>();
        _magneticObject = GetComponent<MagneticObject>();

        string allyTag = gameObject.tag == "PlayerRed" ? "PlayerBlue" : "PlayerRed";
        _allyMovement = GameObject.FindGameObjectWithTag(allyTag).GetComponent<PlayerMovement>();
        _allyField = _allyMovement.GetComponentInChildren<MagneticField>();
        _animator = GetComponent<Animator>();

        // Custom gravity given by dx = v0*t + a*t^2 / 2 with dx = maxJumpHeight, v0 = 0 
        // acceleration = gravity and time = timetoJumpApex
        _gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
    }

    private void Start()
    {
        GameManager.Instance.Gravity = _gravity;

        // cache animator's parameters hashes
        _moveSpeedHash = Animator.StringToHash("moveSpeed");
        _isGroundedHash = Animator.StringToHash("isGrounded");
        _startJumpHash = Animator.StringToHash("startJump");
        _endJumpHash = Animator.StringToHash("endJump");

        // v1 = v0 + a*t (v0 = 0)
        _maxJumpSpeed = Mathf.Abs(_gravity * timeToJumpApex);
        // v1^2 = v0^2 + 2 * a * dx (v0 = 0)
        _minJumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);
    }

    private void Update()
    {
        // check if sticked here

        // Handle the "vertical unplug"
        bool isAbove = CheckIfAboveCharacter();
        if (isAbove && DirectionalInput.y == 1 && _pushUpCoroutine == null)
        {
            _pushUpCoroutine = PushMeUp();
            StartCoroutine(_pushUpCoroutine);
        }

        // Calculate Velocity was here

        // Update the animator
        UpdateAnimator(_velocity.x);
    }

    // put here only what's related to the movement
    void FixedUpdate()
    {
        // set the stick bool if characters are (almost) attached and then ignore the ally field
        if (CheckIfSticked())
        {
            // if they are sticked each character should ignore the ally field
            _magneticObject.UnregisterForce(_allyField.ID);
        }

        if (!isStickToAlly)
        {
            //Debug.Log(LayerMask.LayerToName(gameObject.layer) + ": " + isStickToAlly);
        }

        CalculateVelocity();
        HandleWallSliding();

        // if you characters are stick together let the Override script handle the Move function for both
        if (isStickToAlly)
        {
            if (_controller2D.collisionInfo.below || _controller2D.collisionInfo.above ||
                _allyMovement.Controller.collisionInfo.below || _allyMovement.Controller.collisionInfo.above)
            {
                _velocity.y = 0.0f;
            }

            return;
        }

        // The frame independent multiplication with deltaTime is done here
        _controller2D.Move(_velocity * GameManager.Instance.gameVelocityMultiplier, _directionalInput);

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

    // *************** INPUT FUNCTIONS ***************
    // Used by PlayerInput script to set directional inputs from there
    public void SetDirectionalInput(Vector2 input)
    {
        _directionalInput = input;
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

        // side jump from a magnetic object (that is not a character)
        HashSet <Collider2D> rightColliders = _controller2D.RaycastHorizontally(Vector2.right);
        HashSet<Collider2D> leftColliders = _controller2D.RaycastHorizontally(Vector2.left);
        foreach (Collider2D coll in rightColliders)
        {
            string collLayer = LayerMask.LayerToName(coll.gameObject.layer);
            if (collLayer != "PlayerRed" && collLayer != "PlayerBlue")
            {
                MagneticObject magneticObj = coll.GetComponent<MagneticObject>();
                if (magneticObj != null)
                {
                    _velocity.x = _maxJumpSpeed * jumpWidth * Vector2.left.x;
                }
            }
        }
        foreach (Collider2D coll in leftColliders)
        {
            string collLayer = LayerMask.LayerToName(coll.gameObject.layer);
            if (collLayer != "PlayerRed" && collLayer != "PlayerBlue")
            {
                MagneticObject magneticObj = coll.GetComponent<MagneticObject>();
                if (magneticObj != null)
                {
                    _velocity.x = _maxJumpSpeed * jumpWidth * Vector2.right.x;
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
        float playerInput = _directionalInput.x * moveSpeed;
        float velX = playerInput + CalculateHorizontalForce();
        float velY = CalculateVerticalForce();  
        resultingVelX = velX;
        resultingVelY = velY;

        // smooth the x velocity 
        _velocity.x = Mathf.SmoothDamp(_velocity.x, velX, ref _smoothedVelocityX,
                                        _controller2D.collisionInfo.below ? _accelerationTimeGrounded : _accelerationTimeAirborne);
        _velocity.y += velY;
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
        return _gravity * GameManager.Instance.gameVelocityMultiplier + _magneticObject.GetMagneticForce().y;
    }

    // Set the isStickToAlly bool and returns it
    private bool CheckIfSticked()
    {
        // fake that they are sticked together when they are unplugging 
        // so that you can handle everything from the Override script
        if (PlayerMovementOverride.Instance.unplugging) return true;

        // cast rays towards your ally to check if you should stick together
        // Note that this is totally independent from the rays used for collision in the Controller2D script
        Vector2 fromMeToAlly = _allyMovement.transform.position - transform.position;
        Vector3 toAllyDir = new Vector2(Mathf.Sign(fromMeToAlly.x), 0);
        // the length has to be always higher than the _skinWidth
        float rayLength;
        if (isStickToAlly || _allyMovement.isStickToAlly)
        {
            // when they are already sticked use a longer ray to be super sure that they stay sticked
            rayLength = 0.5f;
        }
        else
        {
            rayLength = 0.025f;
        }

        isStickToAlly = false;
        HashSet<Collider2D> colliders = _controller2D.RaycastHorizontally(toAllyDir, rayLength);

        foreach (Collider2D coll in colliders)
        {
            string layer = LayerMask.LayerToName(coll.gameObject.layer);
            if (layer == "PlayerRed" || layer == "PlayerBlue")
            {
                // check also that they are close enough on the y axis
                float extentY = _collider.bounds.extents.y;
                Vector3 allyPos = _allyMovement.transform.position;
                if (allyPos.y < (transform.position + Vector3.up * extentY).y &&
                    allyPos.y > (transform.position - Vector3.up * extentY).y)
                {
                    isStickToAlly = true;
                    return isStickToAlly;
                }
            }
        }

        return isStickToAlly;
    }

    // check if this character is above the other character
    private bool CheckIfAboveCharacter()
    {
        isAboveCharacter = false;
        // if you are in the air or sticked, you are not above the other character 
        if (!_controller2D.collisionInfo.below || isStickToAlly || _allyMovement.isStickToAlly) return false;

        // This can be implemented in a much more light way in the Controller2D script
        // but raycasting vertically again here gives a lot more control
        HashSet<Collider2D> colliders = _controller2D.RaycastVertically(Vector2.down, 0.0f);

        foreach (Collider2D coll in colliders)
        {
            string layer = LayerMask.LayerToName(coll.gameObject.layer);
            if (layer == _allyMovement.gameObject.tag)
            {
                isAboveCharacter = true;
            }
        }

        return isAboveCharacter;
    }

    // a sort of unplug but in vertical when you are above the other character
    private IEnumerator PushMeUp()
    {
        float timer = 0.0f;

        while ( isAboveCharacter && DirectionalInput.y == 1 && timer < PlayerMovementOverride.Instance.timeToUnplug)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (timer > PlayerMovementOverride.Instance.timeToUnplug)
        {
            // push up
            _velocity.y += _maxJumpSpeed * jumpPushForce;
        }

        _pushUpCoroutine = null;
    }

    private void UpdateAnimator(float velX)
    {
        _animator.SetFloat(_moveSpeedHash, Mathf.Abs(velX));
        _animator.SetBool(_isGroundedHash, _controller2D.collisionInfo.below);
        _animator.SetBool(_startJumpHash, isJumping);
        _animator.SetBool(_endJumpHash, _controller2D.collisionInfo.below && _wasInAir);

        // store the grounded state of the previous frame
        _wasInAir = !_controller2D.collisionInfo.below;
    }
}
