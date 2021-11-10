using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script allows the characters to move as a single entity when they stick 
// to each other (due to magnetic forces) by controlling and overriding their movements

public class PlayerMovementOverride : MonoBehaviour
{
    public PlayerMovement movementRed;
    public PlayerMovement movementBlue;

    [HideInInspector] public bool unplugging = false;

    private float _redVelX;
    private float _redVelY;
    private float _blueVelX;
    private float _blueVelY;

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
        if (movementRed.isStickToAlly || movementBlue.isStickToAlly)
        {
            // NOTE that below I'm just taking the frame resultant and then
            // applying the sum and the reset on the y axis here.
            // It's fundamental to keep the two different movements (sticked or not) completely separated.

            // resulting x axis velocity 
            _redVelX = movementRed.resultingVelX + movementBlue.resultingVelX;
            _blueVelX = movementRed.resultingVelX + movementBlue.resultingVelX;

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

            // apply the final velocities to the characters
            Vector2 finalRedVel = new Vector2(_redVelX, _redVelY);
            Vector2 finalBlueVel = new Vector2(_blueVelX, _blueVelY);

            // Apply movement in a specific order based on the x direction, otherwise one character
            // will collide with the other countering it's movement
            float blueToRed = Mathf.Sign(movementRed.transform.position.x - movementBlue.transform.position.x);
            if (blueToRed == 1) // red on the right
            {
                if (_redVelX > 0) // and moving to the right
                {
                    movementRed.Controller.Move(finalRedVel * Time.deltaTime, movementRed.DirectionalInput);
                    movementBlue.Controller.Move(finalBlueVel * Time.deltaTime, movementBlue.DirectionalInput);
                }
                else // moving to the left
                {
                    movementBlue.Controller.Move(finalBlueVel * Time.deltaTime, movementBlue.DirectionalInput);
                    movementRed.Controller.Move(finalRedVel * Time.deltaTime, movementRed.DirectionalInput);
                }
            }
            else //red on the left
            {
                if (_redVelX > 0) // and moving to the right
                {
                    movementBlue.Controller.Move(finalBlueVel * Time.deltaTime, movementBlue.DirectionalInput);
                    movementRed.Controller.Move(finalRedVel * Time.deltaTime, movementRed.DirectionalInput);
                }
                else // moving to the left
                {
                    movementRed.Controller.Move(finalRedVel * Time.deltaTime, movementRed.DirectionalInput);
                    movementBlue.Controller.Move(finalBlueVel * Time.deltaTime, movementBlue.DirectionalInput);
                }
            }


            // reset the resultant
            if (movementRed.Controller.collisionInfo.below || movementRed.Controller.collisionInfo.above ||
                movementBlue.Controller.collisionInfo.below || movementBlue.Controller.collisionInfo.above)
            {
                movementRed.resultingVelY = 0.0f;
                movementBlue.resultingVelY = 0.0f;

                _redVelY = 0.0f;
                _blueVelY = 0.0f;
            }
        }
    }

    // if a character jump and they are sticked together
    public void OnJumpInputDown(PlayerMovement movement)
    {
        //find in which direction you have to jump to unplug
        bool isMovementRed = movement.GetInstanceID() == movementRed.GetInstanceID();
        PlayerMovement otherMovement = isMovementRed ? movementBlue : movementRed;
        // opposite direction from the other character
        float oppositeDir = Mathf.Sign(movement.transform.position.x - otherMovement.transform.position.x);

        // unplug
        if (movement.DirectionalInput.x == oppositeDir)
        {
            movement.isStickToAlly = false;
            otherMovement.isStickToAlly = false;
            unplugging = true;

            movement.OnJumpInputDown();
        }
        // jump as a whole
        else
        {
            // the 0.5 factor is used since if both characters are jumping their jump forces will sum up
            _redVelY += movementRed.MaxJumpSpeed * 0.5f;
            _blueVelY += movementBlue.MaxJumpSpeed * 0.5f;
        }
    }
}
