using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

// This script allows the characters to move as a single entity when they stick 
// to each other (due to magnetic forces) by controlling and overriding their movements

public class PlayerMovementOverride : MonoBehaviour
{
    // Public
    public float timeToUnplug = 2.0f;
    public float unplugForce = 1.0f;
    public float dragWhileUnplugging = 0.2f;

    [HideInInspector] public bool unplugging = false;
    [HideInInspector] public static bool startUnplug = false;
    [HideInInspector] public bool waitingForSecondJump;

    // Internals
    private float _redVelX;
    private float _redVelY;
    private float _blueVelX;
    private float _blueVelY;
    private float _blueToRed;
    private float _timeToWaitForSecondJumpInput = 0.1f;
    private float _desiredStickDistance = 0.0f;
    private float _currentDistance;

    private PlayerMovement _movementRed;
    private PlayerMovement _movementBlue;
    private IEnumerator _unplugCoroutine = null;
    private IEnumerator _waitForJumpInput = null;

    // audio parameter
    private ulong _delayedID;


    // Singleton
    private PlayerMovementOverride() { }
    private static PlayerMovementOverride _instance = null;

    public static PlayerMovementOverride Instance { get { return _instance; } }

    private void Awake()
    {
        //Singleton
        if (_instance == null)
        {
            _instance = FindObjectOfType<PlayerMovementOverride>();

            if (_instance == null)
            {
                GameObject sceneManager = new GameObject("Game Manager");
                _instance = sceneManager.AddComponent<PlayerMovementOverride>();
            }
        }

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // avoid to process anything if you are not inside a level scene
        if (_movementRed == null || _movementBlue == null) return;
    }

    // put here only what's related to the movement
    private void FixedUpdate()
    {
        // avoid to process anything if you are not inside a level scene
        if (_movementRed == null || _movementBlue == null) return;

        if (_movementRed.isStickToAlly || _movementBlue.isStickToAlly)
        {
            // be sure that the characters are really attached
            _currentDistance = Mathf.Abs(_movementBlue.transform.position.x - _movementRed.transform.position.x);
            // but do it only if they are not too far from each other (ideally, never)
            if (!unplugging && _currentDistance < _desiredStickDistance + 0.5f)
            {
                CheckSticknessValidity();
            }

            CalculateResultantVelocity();

            // apply the final velocities to the characters
            Vector2 finalRedVel = new Vector2(_redVelX, _redVelY);
            Vector2 finalBlueVel = new Vector2(_blueVelX, _blueVelY);
            ApplyMovement(finalRedVel, finalBlueVel);

            // reset the resultant y axis if at least one of them is colliding verically
            if (_movementRed.Controller.collisionInfo.below || _movementRed.Controller.collisionInfo.above ||
                _movementBlue.Controller.collisionInfo.below || _movementBlue.Controller.collisionInfo.above)
            {
                _movementRed.resultingVelY = 0.0f;
                _movementBlue.resultingVelY = 0.0f;

                _redVelY = 0.0f;
                _blueVelY = 0.0f;
            }
        }

        // always reset the resultant x axis unless they are just unplugging
        if (!unplugging)
        {
            _redVelX = 0.0f;
            _blueVelX = 0.0f;
        }
        // add some drag while they are unplugging so that it gives more a magnetic feeling
        else
        {
            _redVelX -= dragWhileUnplugging * GetBlueToRedDir();
            _blueVelX += dragWhileUnplugging * GetBlueToRedDir();
        }

        // if you are above the other character, his X movement will influence yours
        if (_movementRed.isAboveCharacter)
        {
            _movementRed.Controller.Move(Vector2.right * _movementBlue.Velocity.x * GameManager.Instance.gameVelocityMultiplier, _movementBlue.DirectionalInput);
        }
        else if (_movementBlue.isAboveCharacter)
        {
            _movementBlue.Controller.Move(Vector2.right * _movementRed.Velocity.x * GameManager.Instance.gameVelocityMultiplier, _movementRed.DirectionalInput);
        }
    }

    private void OnEnable()
    {
        // Cache the characters movements
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // if I'm in a level, cache the characters movements
        string pattern = @"^L\d+.*";
        Regex rg = new Regex(pattern);

        if (rg.IsMatch(scene.name))
        {
            _movementRed = GameObject.FindGameObjectWithTag("PlayerRed").GetComponent<PlayerMovement>();
            _movementBlue = GameObject.FindGameObjectWithTag("PlayerBlue").GetComponent<PlayerMovement>();

            _desiredStickDistance = _movementBlue.Collider.bounds.extents.x * 2;
        }
        else
        {
            _movementRed = null;
            _movementBlue = null;
        }
    }

    private int GetBlueToRedDir()
    {
        return _movementRed.transform.position.x > _movementBlue.transform.position.x ? 1 : -1;
    }

    private IEnumerator UnplugCharacters()
    {
        float timer = 0.0f;

        while ((_movementRed.Input.isPressingUnplug || _movementBlue.Input.isPressingUnplug) && timer < timeToUnplug)
        {
            timer += Time.deltaTime;
            startUnplug = true;
            yield return null;
        }

        if (timer > timeToUnplug)
        {
            unplugging = true;

            // Get the direction of the unplug force
            int redDir = GetBlueToRedDir();
            int blueDir = -redDir;
            // unplug them
            _redVelX += unplugForce * redDir;
            _blueVelX += unplugForce * blueDir;

            AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.unplug);

            yield return new WaitForSeconds(0.2f);

            unplugging = false;

            // register their fields again if the unplug force is not strong enough to make you exit
            Collider2D redField = _movementBlue.AllyField.Field;
            // we only need to check one field since they are symmetrical
            if (redField.OverlapPoint(_movementBlue.transform.position))
            {
                _movementRed.MagneticObject.RegisterForce(_movementRed.AllyField.ID, Vector2.zero);
                _movementBlue.MagneticObject.RegisterForce(_movementBlue.AllyField.ID, Vector2.zero);
            }
        }

        startUnplug = false;
        _unplugCoroutine = null;
    }

    private void CalculateResultantVelocity()
    {
        // NOTE that below I'm just taking the frame resultant and then
        // applying the sum and the reset on the y axis here.
        // It's fundamental to keep the two different movements (sticked or not) completely separated.

        // resulting x axis velocity 
        _redVelX += _movementRed.resultingVelX + _movementBlue.resultingVelX;
        _blueVelX += _movementRed.resultingVelX + _movementBlue.resultingVelX;

        // avoid to go at double the speed on the x axis when both move in the same direction
        if (_movementRed.DirectionalInput.x == _movementBlue.DirectionalInput.x &&
            _movementRed.DirectionalInput.x != 0 && _movementBlue.DirectionalInput.x != 0)
        {
            _redVelX -= _movementRed.moveSpeed * _movementRed.DirectionalInput.x;
            _blueVelX -= _movementBlue.moveSpeed * _movementBlue.DirectionalInput.x;
        }

        // Clamp the X velocity when sticked to avoid weird behaviors when connecting
        if (!unplugging)
        {
            _redVelX = Mathf.Clamp(_redVelX, -_movementRed.moveSpeed, _movementRed.moveSpeed);
            _blueVelX = Mathf.Clamp(_blueVelX, -_movementBlue.moveSpeed, _movementBlue.moveSpeed);
        }

        // resulting y axis velocity 
        float redY = _movementRed.resultingVelY;
        float blueY = _movementBlue.resultingVelY;

        // if the resulting velocities on the y axis have the same direction
        if (Mathf.Sign(redY) == Mathf.Sign(blueY))
        {
            // take the (absolute) higher 
            _redVelY += Mathf.Abs(redY) > Mathf.Abs(blueY) ? redY : blueY;
            _blueVelY += Mathf.Abs(redY) > Mathf.Abs(blueY) ? redY : blueY;
        }
        else
        {
            // otherwise just sum
            _redVelY += redY + blueY;
            _blueVelY += redY + blueY;
        }
    }

    private void ApplyMovement(Vector2 finalRedVel, Vector2 finalBlueVel)
    {
        // Apply movement in a specific order based on the x direction, otherwise one character
        // will collide with the other countering it's movement
        _blueToRed = Mathf.Sign(_movementRed.transform.position.x - _movementBlue.transform.position.x);
        finalBlueVel *= GameManager.Instance.gameVelocityMultiplier;
        finalRedVel *= GameManager.Instance.gameVelocityMultiplier;
        if (_blueToRed == 1) // red on the right
        {
            if (_redVelX > 0 || _blueVelX > 0) // and moving to the right
            {
                _movementRed.Controller.Move(finalRedVel, _movementRed.DirectionalInput);
                _movementBlue.Controller.Move(finalBlueVel, _movementBlue.DirectionalInput);
            }
            else if (_redVelX < 0 || _blueVelX < 0) // moving to the left
            {
                _movementBlue.Controller.Move(finalBlueVel, _movementBlue.DirectionalInput);
                _movementRed.Controller.Move(finalRedVel, _movementRed.DirectionalInput);
            }
            else // in this case it doesn't matter which one you move first
            {
                _movementRed.Controller.Move(finalRedVel, _movementRed.DirectionalInput);
                _movementBlue.Controller.Move(finalBlueVel, _movementBlue.DirectionalInput);
            }
        }
        else //red on the left
        {
            if (_redVelX > 0 || _blueVelX > 0) // and moving to the right
            {
                _movementBlue.Controller.Move(finalBlueVel, _movementBlue.DirectionalInput);
                _movementRed.Controller.Move(finalRedVel, _movementRed.DirectionalInput);
            }
            else if (_redVelX < 0 || _blueVelX < 0) // moving to the left
            {
                _movementRed.Controller.Move(finalRedVel, _movementRed.DirectionalInput);
                _movementBlue.Controller.Move(finalBlueVel, _movementBlue.DirectionalInput);
            }
            else // in this case it doesn't matter which one you move first
            {
                _movementBlue.Controller.Move(finalBlueVel, _movementBlue.DirectionalInput);
                _movementRed.Controller.Move(finalRedVel, _movementRed.DirectionalInput);
            }
        }

        // if only one character is touching the ground the other should remain sticked
        // This has to go after I called Move to properly set the below boolean
        if ((_movementRed.Controller.collisionInfo.below || _movementBlue.Controller.collisionInfo.below)
            && (_redVelY < 0 || _blueVelY < 0))
        {
            _redVelY = 0.0f;
            _blueVelY = 0.0f;

            // and also set their y position to be the same as the one character on the ground
            if (_movementRed.Controller.collisionInfo.below)
            {
                _movementBlue.transform.position = new Vector2(_movementBlue.transform.position.x, _movementRed.transform.position.y);
            }
            if (_movementBlue.Controller.collisionInfo.below)
            {
                _movementRed.transform.position = new Vector2(_movementRed.transform.position.x, _movementBlue.transform.position.y);
            }
        }
    }

    private IEnumerator WaitBeforePlay(float time)
    {
        yield return new WaitForSeconds(time);
        _delayedID = AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.jump.AudioClip);
        _waitForJumpInput = null;
    }

    // used to enlarge the double jump window for the player
    private IEnumerator WaitForSecondJump()
    {
        waitingForSecondJump = true;

        yield return new WaitForSeconds(_timeToWaitForSecondJumpInput);

        waitingForSecondJump = false;
    }

    // check that characters are stick for real, if not, stick them manually
    private void CheckSticknessValidity()
    {
        float tolerance = 0.01f;

        // if they are not really attached as they should be 
        if (_currentDistance > _desiredStickDistance + tolerance)
        {
            // then attach them
            float offset = (_currentDistance - _desiredStickDistance - tolerance) * 0.5f;
            float blueOffset = _movementBlue.transform.position.x + offset * _blueToRed;
            float redOffset = _movementRed.transform.position.x - offset * _blueToRed;
            _movementBlue.transform.position = new Vector3(blueOffset, _movementBlue.transform.position.y, 0.0f);
            _movementRed.transform.position = new Vector3(redOffset, _movementRed.transform.position.y, 0.0f);
        }
    }

    // if a character jump and they are sticked together
    public void OnJumpInputDown(PlayerMovement movement)
    {
        // second jump in a short span of time
        if (waitingForSecondJump)
        {
            _redVelY += _movementRed.MaxJumpSpeed * 0.5f;
            _blueVelY += _movementBlue.MaxJumpSpeed * 0.5f;

            StopCoroutine(_waitForJumpInput);
            _waitForJumpInput = null;
            AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.jump[1]);

            return;
        }

        HashSet<Collider2D> colliders = movement.Controller.RaycastVertically(Vector2.down, 0.0f, 0.02f);

        int hits = 0;
        foreach (Collider2D coll in colliders)
        {
            string layerName = LayerMask.LayerToName(coll.gameObject.layer);
            if (layerName == "PlayerRed" || layerName == "PlayerBlue")
            {
                continue;
            }

            hits++;
        }

        // jump only if you are touching or close enough to the ground
        if (hits != 0)
        {
            // if both characters are jumping their jump forces will sum up
            _redVelY += _movementRed.MaxJumpSpeed * 0.7f;
            _blueVelY += _movementBlue.MaxJumpSpeed * 0.7f;

            if (_waitForJumpInput == null)
            {
                _waitForJumpInput = WaitBeforePlay(_timeToWaitForSecondJumpInput);
                StartCoroutine(_waitForJumpInput);
            }

            StartCoroutine(WaitForSecondJump());
        }
    }

    // when the player presses the unplug button while the characters are sticked
    public void OnUnplugInput()
    {
        if ((_movementRed.isStickToAlly || _movementBlue.isStickToAlly) && _unplugCoroutine == null)
        {
            _unplugCoroutine = UnplugCharacters();
            StartCoroutine(_unplugCoroutine);
        }
    }
}
