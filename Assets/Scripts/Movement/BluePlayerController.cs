using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluePlayerController : PhysicsObject {

    public float jumpTakeOffSpeed = 7;
    public float maxSpeed = 7;

    private SpriteRenderer _spriteRenderer;
    // Start is called before the first frame update
    void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
        
        bool flipSprite = _spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < 0.01f);
        if (flipSprite) {
            _spriteRenderer.flipX = !_spriteRenderer.flipX;
        }
        
        _targetVelocity = move * maxSpeed;
    }
}