using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// custom physics for platforms

public class PlatformController : RaycastController
{
    // Public
    public LayerMask passengerMask;
    public Vector3[] localWaypoints;
    public bool isCyclic;
    public float speed;
    [Tooltip("How much time the platform is going to stop on the next waypoint before start moving again.")]
    public float waitTime;
    public AnimationCurve easeCurve;

    // Internals
    private List<PassengerMovement> _passengersMovements;
    private Vector3[] globalWaypoints;
    private int _fromWaypointIndex;
    private float _percentBetweenWaypoints; // between 0 and 1
    private float _nextMoveTime;

    // Contains all the info about a passenger of the platform
    private struct PassengerMovement
    {
        public Transform transform;
        public Collider2D collider;
        public Vector3 velocity;
        public bool isStandingOnPlatform;
        // Note that when the platform moves toward the passenger you have to first move the passenger
        // and only after the platform, otherwise the platform collides, it blocks itself and then the passenger
        // moves and there will be a gap. Viceversa when the platform goes in the opposite direction in respect of
        // the passenger, you have to move the platform first and then the passenger.
        public bool moveBeforePlatform;

        public PassengerMovement(Transform transform_, Collider2D collider_, Vector3 velocity_, bool standingOnPlatform_, bool moveBeforePlatform_)
        {
            transform = transform_;
            collider = collider_;
            velocity = velocity_;
            isStandingOnPlatform = standingOnPlatform_;
            moveBeforePlatform = moveBeforePlatform_;
        }
    }

    protected override void Start()
    {
        base.Start();

        // have two separate vector to visualize waypoints differently if the game is running
        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    private void Update()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = CalculatePlatformMovement();
        CalculatePassengerMovement(velocity);

        // apply movements to passengers that have to move before the platform
        MovePassengers(true);
        transform.Translate(velocity);
        // apply movements to passengers that have to move after the platform
        MovePassengers(false);
    }

    // Ease platform movements by using a curve (or a direct formula if you prefer)
    private float Ease(float x)
    {
        // the following formula allows to get ease without using a curve, just by changing "a"
        //float a = 1;
        //return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));

        return easeCurve.Evaluate(x);
    }

    // how much you have to move between waypoints thanks to your speed (using lerp)
    private Vector3 CalculatePlatformMovement()
    {
        if (Time.time < _nextMoveTime)
        {
            return Vector3.zero;
        }

        // modulo operator is used to cycle through waypoints
        _fromWaypointIndex %= globalWaypoints.Length;

        int toWaypointIndex = (_fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[_fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        _percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        _percentBetweenWaypoints = Mathf.Clamp01(_percentBetweenWaypoints);
        // apply the ease
        float easedPercent = Ease(_percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[_fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercent);

        // we reached the target waypoint
        if (_percentBetweenWaypoints >= 1)
        {
            _percentBetweenWaypoints = 0.0f;
            _fromWaypointIndex ++;

            if (!isCyclic)
            {
                // if we reached the last waypoint
                if (_fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    _fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }

            _nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    // apply the movements to the passengers of the platform
    private void MovePassengers(bool moveBeforePlatform)
    {
        foreach (PassengerMovement passenger in _passengersMovements)
        {
            if (passenger.moveBeforePlatform == moveBeforePlatform)
            {
                // use the game manager to retrieve the controller component and call its Move function
                Controller2D controller = GameManager.Instance.Controllers2D[passenger.collider.GetInstanceID()];
                controller.Move(passenger.velocity, passenger.isStandingOnPlatform);
            }
        }
    }

    // calculate the movements of the passengers of the platform
    private void CalculatePassengerMovement(Vector3 velocity)
    {
        // Register all the passengers we already moved this frame
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        // add to this list all the passenger movements and at the end process them
        _passengersMovements = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0)
        {
            // we are casting from inside the actual box so we have to sum the skinWidth
            // to check if the actual box is going to collide
            float rayLength = Mathf.Abs(velocity.y) + _skinWidth;

            // cast your vertical rays
            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (_verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                
                if (hit && hit.distance != 0)    // we are hitting a passenger vertically
                {
                    // we have to check that we didn't already hit the passenger to avoid moving it more than once
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);

                        // if the platforms hit the passenger while descending, it shouldn't push him on the x axis
                        float pushX = directionY == 1 ? velocity.x : 0;
                        // push the passenger where the platform will be
                        float pushY = velocity.y - (hit.distance - _skinWidth) * directionY;

                        _passengersMovements.Add(new PassengerMovement(hit.transform, hit.collider, new Vector2(pushX, pushY), directionY == 1, true));
                    }
   
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + _skinWidth;

            // cast your vertical rays
            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit && hit.distance != 0)    // we are hitting a passenger on the side
                {
                    // we have to check that we didn't already hit the passenger to avoid moving it more than once
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);

                        float pushX = velocity.x - (hit.distance - _skinWidth) * directionX;
                        // side collision with the passenger won't push him along the y axis, however we use a small amount
                        // just so that collisionInfo.below is true so that we can still jump while being side-pushed by the platform
                        float pushY = -Mathf.Epsilon;

                        _passengersMovements.Add(new PassengerMovement(hit.transform, hit.collider, new Vector2(pushX, pushY), false, true));
                    }

                }
            }
        }

        // Passenger on top of a horizontally or downward moving platform
        if (directionY == -1 || (velocity.y == 0 && velocity.x != 0))
        {
            // we only want to check if a passenger is on the platform while moving
            float rayLength = _skinWidth * 2;

            // cast your vertical rays
            for (int i = 0; i < verticalRayCount; i++)
            {
                // we only want to cast up in this case
                Vector2 rayOrigin = _raycastOrigins.topLeft + Vector2.right * (_verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && hit.distance != 0)    // we are hitting a passenger vertically
                {
                    // we have to check that we didn't already hit the passenger to avoid moving it more than once
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);

                        // push the passenger of the same movement that the platform does
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        _passengersMovements.Add(new PassengerMovement(hit.transform, hit.collider, new Vector2(pushX, pushY), true, false));
                    }

                }
            }
        }
    }

    // Visual reference for platform waypoints
    private void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = 0.3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                // if simulations is running use global waypoints (local ones would move with the platform)
                Vector3 globalWaypointPos = Application.isPlaying ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
