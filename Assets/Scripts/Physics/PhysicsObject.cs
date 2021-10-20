using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    public float minGroundNormalY = 0.65f;
    public float gravityModifier = 1f;
    protected Vector2 _velocity;
    private Rigidbody2D _rb;
    private ContactFilter2D _contactFilter;
    //information back from raycast
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);
    protected bool _grounded;
    private Vector2 _groundNormal;
    protected Vector2 _targetVelocity;

    private const float MINMoveDistance = 0.001f;
    //make sure we are not stucked inside another collider
    private const float _ShellRadius = 0.01f;

    //test
    private MagneticObject _magneticObject;

    private void OnEnable() {
        _rb = GetComponent<Rigidbody2D>();
        _magneticObject = GetComponent<MagneticObject>();
    }
    
    void Start() {
        //Not check collision against triggers
        _contactFilter.useTriggers = false;
        //use physics2d settings to determine the collision layers
        //edit -> project settings -> physics2D -> check the collision graph
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        _contactFilter.useLayerMask = true;
    }
    
    void Update()
    {
        _targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity() {
        
    }

    private void FixedUpdate() {
        //Physics2D.gravity is the Vector2(0, -9,8)
        //v(t) = v(0) + a * dt
        _velocity += gravityModifier * Physics2D.gravity;
        _velocity.x = _targetVelocity.x;

        //test
        _velocity += _magneticObject.GetNextPos();

        _grounded = false;

        //ds = v*dt
        Vector2 deltaPosition = _velocity * Time.deltaTime;
        Vector2 moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);
        Vector2 moveX = moveAlongGround * deltaPosition.x;
        
        Movement(moveX, false);
        
        //
        Vector2 moveY = Vector2.up * deltaPosition.y;

        Movement(moveY, true);
    }

    void Movement(Vector2 move, bool yMovement) {
        //magnitude of vector move
        float distance = move.magnitude;

        if (distance > MINMoveDistance) {
            //Count the number of collision along one direction (move)
            //contact filter choose collision with layers
            //shellradius to be sure to cast the collision
            int count = _rb.Cast(move, _contactFilter, _hitBuffer, distance + _ShellRadius);
            _hitBufferList.Clear();
            
            //hitbuffer it's the list of objects that overlap our collider
            for (int i = 0; i < count; i++) {
                _hitBufferList.Add(_hitBuffer[i]);
            }

            foreach (var hitObject in _hitBufferList) {
                Vector2 currentNormal = hitObject.normal;
                //Check normal vector of the object (currentNormal = 1 means floor)
                if (currentNormal.y > minGroundNormalY) {
                    _grounded = true;
                    if (yMovement) {
                        _groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }
                
                //product of vector velocity and currentNormal
                float projection = Vector2.Dot(_velocity, currentNormal);
                if (projection < 0) {
                    _velocity -= projection * currentNormal;
                }

                float modifiedDistance = hitObject.distance - _ShellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
            
        }
        _rb.position += move.normalized * distance;
    }
}
