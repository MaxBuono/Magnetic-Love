using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle all the inputs here

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour
{
    // Internals
    private PlayerMovement _playerMovement;


    void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Horizontal/Vertical Input
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _playerMovement.SetDirectionalInput(directionalInput);

        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            _playerMovement.OnJumpInputDown();
        }
        if (Input.GetButtonUp("Jump"))
        {
            _playerMovement.OnJumpInputUp();
        }
    }
}
