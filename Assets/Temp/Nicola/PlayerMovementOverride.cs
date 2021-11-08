using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementOverride : MonoBehaviour
{
    public PlayerMovement movementRed;
    public PlayerMovement movementBlue;

    private float _redVelX;
    private float _redVelY;
    private float _blueVelX;
    private float _blueVelY;

    void Update()
    {
        if (movementRed.isStickToAlly || movementBlue.isStickToAlly)
        {
            // resulting x axis velocity computation
            _redVelX = movementRed.resultingVelX + movementBlue.resultingVelX;
            _blueVelX = movementRed.resultingVelX + movementBlue.resultingVelX;


            // resulting x axis velocity computation
            //_redVelY = 0.0f;
            //_blueVelY = 0.0f;
            float redY = movementRed.resultingVelY;
            float blueY = movementBlue.resultingVelY;
            // if the resulting velocities on the y axis have the same direction
            if (Mathf.Sign(redY) == Mathf.Sign(blueY))
            {
                // take the (absolute) higher 
                _redVelY = Mathf.Abs(redY) > Mathf.Abs(blueY) ? redY : blueY;
                _blueVelY = Mathf.Abs(redY) > Mathf.Abs(blueY) ? redY : blueY;
            }
            else
            {
                // otherwise just sum
                _redVelY = redY + blueY;
                _blueVelY = redY + blueY;
            }

            // apply the final velocities to the characters
            Vector2 finalRedVel = new Vector2(_redVelX, _redVelY);
            Vector2 finalBlueVel = new Vector2(_blueVelX, _blueVelY);

            //todo directionalinput study?

            movementRed.Controller.Move(finalRedVel * Time.deltaTime, movementRed.DirectionalInput);
            movementBlue.Controller.Move(finalBlueVel * Time.deltaTime, movementBlue.DirectionalInput);

            // reset the resultant
            if ((movementRed.Controller.collisionInfo.below || movementRed.Controller.collisionInfo.above ||
                movementBlue.Controller.collisionInfo.below || movementBlue.Controller.collisionInfo.above))
            {
                movementRed.resultingVelY = 0.0f;
                movementBlue.resultingVelY = 0.0f;

                //_redVelY = 0.0f;
                //_blueVelY = 0.0f;
            }
        }
    }

    // if a character jump and they are sticked together
    public void OnJumpInputDown(PlayerMovement movement)
    {
        // find in which direction you have to jump to unplug
        bool isMovementRed = movement.GetInstanceID() == movementRed.GetInstanceID();
        PlayerMovement otherMovement = isMovementRed ? movementBlue : movementRed;
        // jump in the opposite direction from the other character
        float jumpDir = Mathf.Sign(movement.transform.position.x - otherMovement.transform.position.x);

        // unplug from the other character
        if (isMovementRed)
        {
            _redVelX = movementRed.MaxJumpSpeed * movementRed.jumpWidth * jumpDir;
            _redVelY = movementRed.MaxJumpSpeed;
        }
        else
        {
            _blueVelX = movementBlue.MaxJumpSpeed * movementBlue.jumpWidth * jumpDir;
            _blueVelY = movementBlue.MaxJumpSpeed;
        }
    }
}
