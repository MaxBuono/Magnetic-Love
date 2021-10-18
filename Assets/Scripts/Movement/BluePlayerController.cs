using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluePlayerController : PhysicsObject {

    public float jumpTakeOffSpeed = 7;
    public float maxSpeed = 7;
    
    private Transform _transform;
    private bool _facingRight;
    
    void Awake() {
        _facingRight = true;
        _transform = GetComponent<Transform>();
    }
    
    protected override void ComputeVelocity() {
        Vector2 move = Vector2.zero;
        

        move.x = Input.GetAxis("HorizontalBlue");

        if (Input.GetButtonDown("Jump") && _grounded) {
            _velocity.y = jumpTakeOffSpeed;
            //stop the jump    
        } else if (Input.GetButtonUp("Jump")) {
            if (_velocity.y > 0) {
                _velocity.y *= 0.5f;
            }
        }

        Flip(move.x);
        _targetVelocity = move * maxSpeed;
    }

    private void Flip(float horizontalMove) {
        Vector3 characterScale = _transform.localScale;

        if (horizontalMove > 0.01f && !_facingRight || horizontalMove < 0 && _facingRight) {
            _facingRight = !_facingRight;

            characterScale.x *= -1;
            _transform.localScale = characterScale;
        }
    }
}