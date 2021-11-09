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
    public string vertical;
    public string jump;

    [HideInInspector]
    public bool isPressingJump;

    // Internals
    private PlayerMovement _playerMovement;


    void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Horizontal/Vertical Input
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw(horizontal), Input.GetAxisRaw(vertical));
        _playerMovement.SetDirectionalInput(directionalInput);

        // Jump
        if (Input.GetButtonDown(jump))
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
        }

    }
}
