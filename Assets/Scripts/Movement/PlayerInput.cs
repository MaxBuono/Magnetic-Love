using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle all the inputs events here

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour
{
    // Public
    // INPUTS
    public string horizontal;
    public string jump;

    // Hidden
    [HideInInspector] public bool isPressingJump;
    [HideInInspector] public bool isPressingUnplug;

    // Internals
    private PlayerMovement _playerMovement;


    void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        isPressingJump = false;
        isPressingUnplug = false;
    }

    void Update()
    {
        // Take player (movement) inputs only if they are not blocked
        if (GameManager.Instance.PlayerControlsBlocked) return;

        // Horizontal/Vertical Input
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw(horizontal), Input.GetAxisRaw(jump));
        _playerMovement.SetDirectionalInput(directionalInput);

        // Jump
        // this is for the animator
        if (Input.GetButtonDown(jump) && !isPressingJump)
        {
            _playerMovement.isJumping = true;
        }

        // actual jump
        if (Input.GetButton(jump) && !isPressingJump)
        {
            if (_playerMovement.isStickToAlly)
            {
                PlayerMovementOverride.Instance.OnJumpInputDown(_playerMovement);
            }
            else
            {
                _playerMovement.OnJumpInputDown();
            }

            isPressingJump = true;
        }

        if (Input.GetButtonUp(jump))
        {
            _playerMovement.OnJumpInputUp();
            isPressingJump = false;
            _playerMovement.isJumping = false;
        }

        // unplug
        if (Input.GetButtonDown("Unplug"))
        {
            PlayerMovementOverride.Instance.OnUnplugInput();
            isPressingUnplug = true;
        }

        if (Input.GetButtonUp("Unplug"))
        {
            isPressingUnplug = false;
        }
    }
}
