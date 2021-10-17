using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script offers a public function (CheckForCollision) to detect collisions and return the fixed next position

public class DetectCollision : MonoBehaviour
{
    // Public
    [Tooltip("The maximum angle on which this object can climb over (e.g. an horizontal floor and a vertical wall would get a 90 degree angle.")]
    public float maxAngleDegree = 45.0f;

    // Internals
    private Rigidbody2D _rb2d;
    private Collider2D _coll;
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private ContactFilter2D _contactFilter;
    // the smaller the shell the higher the detection precision
    private const float _shellRadius = 0.01f;
    private const float _minMoveDistance = 0.0001f;
    private Vector2 _surfaceParallel = Vector2.zero;
    private Vector2 _hitNormal = Vector2.zero;


    private void Awake()
    {
        // Cache components
        _rb2d = GetComponent<Rigidbody2D>();
        _coll = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // You don't want to collide with triggers
        _contactFilter.useTriggers = false;
    }

    // returns a corrected pNex if a collision changes it
    public Vector2 CheckForCollision(Vector2 pNow, Vector2 pNext, Collider2D coll)
    {
        // how much I'm going to move in this step
        float deltaMove = (pNext - pNow).magnitude;

        // Avoid micro movements for better accuracy
        if (deltaMove < _minMoveDistance) return pNow;

        // my movement direction
        Vector2 moveDir = (pNext - pNow).normalized;

        // check for collisions casting my collider
        int count = 0;
        if (_hitNormal == Vector2.zero || Vector2.Dot(moveDir, _hitNormal) > 0)
        {
            count = coll.Cast(moveDir, _contactFilter, _hitBuffer, deltaMove + _shellRadius);
        }
        // if I'm moving along a surface use that direction for the cast to be sure to hit the next collider
        else
        {
            count = coll.Cast(_surfaceParallel, _contactFilter, _hitBuffer, deltaMove + _shellRadius);
        }


        // if colliding with something
        if (count > 0)
        {
            // *** this is needed to avoid "invisible" collisions
            Vector2 closest = _coll.ClosestPoint(_hitBuffer[0].point);
            float dist = (_hitBuffer[0].point - closest).magnitude;

            // if between me and the collision there is more distance than the shell radius
            if ((dist) > _shellRadius)
            {
                // then I should move myself of that distance minus the radius
                return pNow + moveDir * (dist - _shellRadius);
            }
            // ***

            // how much I'm going to move along the surface
            Vector2 surfaceDelta = Vector2.zero;
            // when hitting multiple surfaces, cache their parallels
            List<Vector2> parallels = new List<Vector2>();
            // positive if I'm moving away from the collision, negative otherwise
            float moveDotNormal = 0.0f;

            // loop through hits
            for (int i = 0; i < count; i++)
            {
                _hitNormal = _hitBuffer[i].normal;
                moveDotNormal = Vector2.Dot(moveDir, _hitNormal);
                if (moveDotNormal > 0)
                    return pNext;

                // direction along the hit surface
                //Vector2 surfaceParallel = moveDir - _hitNormal * moveDotNormal;
                Vector2 surfaceParallel = new Vector2(_hitNormal.y, -_hitNormal.x);
                surfaceParallel = surfaceParallel.normalized;
                parallels.Add(surfaceParallel);

                // angle between the direction you want to move and the surface parallel
                float projectionAngle;

                // if I'm hitting at least two surfaces (e.g. a corner)
                if (i > 0)
                {
                    float surfacesAngle = Vector2.Angle(parallels[i - 1], parallels[i]);
                    // check that you can actually move on the new surface
                    if (surfacesAngle > maxAngleDegree)
                    {
                        return pNow;
                    }
                }

                // Visual debugging
                //Debug.DrawLine(transform.position, (Vector2)transform.position + moveDir * 10000, Color.magenta);
                //Debug.DrawLine(transform.position, (Vector2)transform.position + surfaceParallel * 10000, Color.cyan);

                // how much you should slide (moveDir component along the parallel direction)
                projectionAngle = Vector2.Angle(moveDir, surfaceParallel);
                surfaceDelta = surfaceParallel * (deltaMove * Mathf.Cos(Mathf.Deg2Rad * projectionAngle));

                // cache the latest parallel direction
                _surfaceParallel = surfaceDelta.normalized;
            }


            return pNow + surfaceDelta;
        }

        return pNext;
    }
}
