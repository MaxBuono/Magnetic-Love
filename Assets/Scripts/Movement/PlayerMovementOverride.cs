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

    [HideInInspector] public bool unplugging = false;
    [HideInInspector] public bool waitingForSecondJump;

    // Internals
    private float _redVelX;
    private float _redVelY;
    private float _blueVelX;
    private float _blueVelY;

    private PlayerMovement movementRed;
    private PlayerMovement movementBlue;
    private IEnumerator _unplugCoroutine = null;


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
        if (movementRed == null || movementBlue == null) return;

        if (movementRed.isStickToAlly || movementBlue.isStickToAlly)
        {
            // Handle unplug situation when both characters are moving 
            // in opposite directions from each other on the x axis
            if (((movementRed.DirectionalInput.x == 1 && movementBlue.DirectionalInput.x == -1) ||
                (movementRed.DirectionalInput.x == -1 && movementBlue.DirectionalInput.x == 1)) && _unplugCoroutine == null)
            {
                _unplugCoroutine = UnplugCharacters();
                StartCoroutine(_unplugCoroutine);
            }
        }
    }

    // put here only what's related to the movement
    private void FixedUpdate()
    {
        // avoid to process anything if you are not inside a level scene
        if (movementRed == null || movementBlue == null) return;

        if (movementRed.isStickToAlly || movementBlue.isStickToAlly)
        {
            CalculateResultantVelocity();
            // apply the final velocities to the characters
            Vector2 finalRedVel = new Vector2(_redVelX, _redVelY);
            Vector2 finalBlueVel = new Vector2(_blueVelX, _blueVelY);
            ApplyMovement(finalRedVel, finalBlueVel);

            // reset the resultant y axis if at least one of them is colliding verically
            if (movementRed.Controller.collisionInfo.below || movementRed.Controller.collisionInfo.above ||
                movementBlue.Controller.collisionInfo.below || movementBlue.Controller.collisionInfo.above)
            {
                movementRed.resultingVelY = 0.0f;
                movementBlue.resultingVelY = 0.0f;

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

        // if you are above the other character, his X movement will influence yours
        if (movementRed.isAboveCharacter)
        {
            movementRed.Controller.Move(Vector2.right * movementBlue.Velocity.x * GameManager.Instance.gameVelocityMultiplier, movementBlue.DirectionalInput);
        }
        else if (movementBlue.isAboveCharacter)
        {
            movementBlue.Controller.Move(Vector2.right * movementRed.Velocity.x * GameManager.Instance.gameVelocityMultiplier, movementRed.DirectionalInput);
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
            movementRed = GameObject.FindGameObjectWithTag("PlayerRed").GetComponent<PlayerMovement>();
            movementBlue = GameObject.FindGameObjectWithTag("PlayerBlue").GetComponent<PlayerMovement>();
        }
        else
        {
            movementRed = null;
            movementBlue = null;
        }
    }

    private IEnumerator UnplugCharacters()
    {
        float timer = 0.0f;

        while ( ((movementRed.DirectionalInput.x == 1 && movementBlue.DirectionalInput.x == -1) || 
                (movementRed.DirectionalInput.x == -1 && movementBlue.DirectionalInput.x == 1)) && timer < timeToUnplug)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (timer > timeToUnplug)
        {
            unplugging = true;

            // unplug them
            _redVelX += unplugForce * movementRed.DirectionalInput.x;
            _blueVelX += unplugForce * movementBlue.DirectionalInput.x;

            yield return new WaitForSeconds(0.2f);

            unplugging = false;

            // register their fields again if the unplug force is not strong enough to make you exit
            Collider2D redField = movementBlue.AllyField.Field;
            // we only need to check one field since they are symmetrical
            if (redField.OverlapPoint(movementBlue.transform.position))
            {
                movementRed.MagneticObject.RegisterForce(movementRed.AllyField.ID, Vector2.zero);
                movementBlue.MagneticObject.RegisterForce(movementBlue.AllyField.ID, Vector2.zero);
            }
        }

        _unplugCoroutine = null;
    }

    private void CalculateResultantVelocity()
    {
        // NOTE that below I'm just taking the frame resultant and then
        // applying the sum and the reset on the y axis here.
        // It's fundamental to keep the two different movements (sticked or not) completely separated.

        // resulting x axis velocity 
        _redVelX += movementRed.resultingVelX + movementBlue.resultingVelX;
        _blueVelX += movementRed.resultingVelX + movementBlue.resultingVelX;

        // avoid to go at double the speed on the x axis when both move in the same direction
        if (movementRed.DirectionalInput.x == movementBlue.DirectionalInput.x &&
            movementRed.DirectionalInput.x != 0 && movementBlue.DirectionalInput.x != 0)
        {
            _redVelX -= movementRed.moveSpeed * movementRed.DirectionalInput.x;
            _blueVelX -= movementBlue.moveSpeed * movementBlue.DirectionalInput.x;
        }

        // resulting y axis velocity 
        float redY = movementRed.resultingVelY;
        float blueY = movementBlue.resultingVelY;

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

        // if only one character is touching the ground the other should remain sticked
        if ((movementRed.Controller.collisionInfo.below || movementBlue.Controller.collisionInfo.below)
            && (_redVelY < 0 || _blueVelY < 0))
        {
            _redVelY = 0.0f;
            _blueVelY = 0.0f;

            // and also set their y position to be the same as the one character on the ground
            if (movementRed.Controller.collisionInfo.below)
            {
                movementBlue.transform.position = new Vector2(movementBlue.transform.position.x, movementRed.transform.position.y);
            }
            if (movementBlue.Controller.collisionInfo.below)
            {
                movementRed.transform.position = new Vector2(movementRed.transform.position.x, movementBlue.transform.position.y);
            }
        }
    }

    private void ApplyMovement(Vector2 finalRedVel, Vector2 finalBlueVel)
    {
        // Apply movement in a specific order based on the x direction, otherwise one character
        // will collide with the other countering it's movement
        float blueToRed = Mathf.Sign(movementRed.transform.position.x - movementBlue.transform.position.x);
        finalBlueVel *= GameManager.Instance.gameVelocityMultiplier;
        finalRedVel *= GameManager.Instance.gameVelocityMultiplier;
        if (blueToRed == 1) // red on the right
        {
            if (_redVelX > 0 || _blueVelX > 0) // and moving to the right
            {
                movementRed.Controller.Move(finalRedVel, movementRed.DirectionalInput);
                movementBlue.Controller.Move(finalBlueVel, movementBlue.DirectionalInput);
            }
            else if (_redVelX < 0 || _blueVelX < 0) // moving to the left
            {
                movementBlue.Controller.Move(finalBlueVel, movementBlue.DirectionalInput);
                movementRed.Controller.Move(finalRedVel, movementRed.DirectionalInput);
            }
            else // in this case it doesn't matter which one you move first
            {
                movementRed.Controller.Move(finalRedVel, movementRed.DirectionalInput);
                movementBlue.Controller.Move(finalBlueVel, movementBlue.DirectionalInput);
            }
        }
        else //red on the left
        {
            if (_redVelX > 0 || _blueVelX > 0) // and moving to the right
            {
                movementBlue.Controller.Move(finalBlueVel, movementBlue.DirectionalInput);
                movementRed.Controller.Move(finalRedVel, movementRed.DirectionalInput);
            }
            else if (_redVelX < 0 || _blueVelX < 0) // moving to the left
            {
                movementRed.Controller.Move(finalRedVel, movementRed.DirectionalInput);
                movementBlue.Controller.Move(finalBlueVel, movementBlue.DirectionalInput);
            }
            else // in this case it doesn't matter which one you move first
            {
                movementBlue.Controller.Move(finalBlueVel, movementBlue.DirectionalInput);
                movementRed.Controller.Move(finalRedVel, movementRed.DirectionalInput);
            }
        }
    }

    // used to enlarge the double jump window for the player
    private IEnumerator WaitForSecondJump()
    {
        waitingForSecondJump = true;

        yield return new WaitForSeconds(0.05f);

        waitingForSecondJump = false;
    }

    // if a character jump and they are sticked together
    public void OnJumpInputDown(PlayerMovement movement)
    {
        // second jump in a short span of time
        if (waitingForSecondJump)
        {
            _redVelY += movementRed.MaxJumpSpeed * 0.5f;
            _blueVelY += movementBlue.MaxJumpSpeed * 0.5f;
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
            // the 0.5 factor is used since if both characters are jumping their jump forces will sum up
            _redVelY += movementRed.MaxJumpSpeed * 0.7f;
            _blueVelY += movementBlue.MaxJumpSpeed * 0.7f;

            StartCoroutine(WaitForSecondJump());
        }
    }
}
