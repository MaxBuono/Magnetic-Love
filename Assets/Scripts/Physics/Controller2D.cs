using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to handle a platform-like custom physics controller for a AABB collider (like a non-rotated box collider)
// Inherit from RaycastController for the basic rays physics

public class Controller2D : RaycastController
{

    // Public
    [Tooltip("Max angle to climb or descend a slope without sliding.")]
    public float maxSlopeAngle = 75.0f;
    public CollisionInfo collisionInfo;
    public IEnumerator tillBelow;

    // Internals
    private Vector2 _playerInput;
    private PlayerInput _playerInputScript;
    private MagneticObject _magneticObject;


    // store info about collision direction
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        // the direction we are facing
        public int faceDir;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;

        public Vector2 inputVelocity;
        // collider of the platform that you can jump through
        public Collider2D fallingThroughPlatform;

        public void Reset()
        {
            above = below = false;
            left = right = false;

            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;

            // the slope angle of the previous frame
            slopeAngleOld = slopeAngle;
            slopeAngle = 0.0f;
            slopeNormal = Vector2.zero;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        _playerInputScript = GetComponent<PlayerInput>();
        _magneticObject = GetComponent<MagneticObject>();
    }

    protected override void Start()
    {
        base.Start();

        // Register this controller to the GameManager
        GameManager.Instance.RegisterController2D(_collider2D.GetInstanceID(), this);

        collisionInfo.faceDir = 1;  // facing right
    }

    // Overload so that if you don't need the player input you don't have to specify it equal to zero
    public void Move(Vector2 deltaMove, bool standingOnPlatform = false)
    {
        Move(deltaMove, Vector2.zero, standingOnPlatform);
    }

    // main movement function called by PlayerInput script, the argument is the resulting input
    public void Move(Vector2 deltaMove, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collisionInfo.Reset();
        collisionInfo.inputVelocity = deltaMove;
        _playerInput = input;


        if (deltaMove.y < 0)
        {
            DescendSlope(ref deltaMove);
        }
        
        // this has to go after DescendSlope since it can modify the deltamove.x sign
        if (deltaMove.x != 0)
        {
            collisionInfo.faceDir = (int)Mathf.Sign(deltaMove.x);
        }
        
        // we want to detect horizontal collisions also when standind still
        HorizontalCollisions(ref deltaMove);
        
        if (deltaMove.y != 0)
        {
            VerticalCollisions(ref deltaMove);
        }
        
        transform.Translate(deltaMove);

        // this allows us to jump while standing on a platform moving upward
        if (standingOnPlatform)
        {
            collisionInfo.below = true;
        }
    }



    // Detect horizontal collisions and change deltaMove accordingly
    private void HorizontalCollisions(ref Vector2 deltaMove)
    {
        float directionX = collisionInfo.faceDir;

        // we are casting from inside the actual box so we have to sum the skinWidth
        // to check if the actual box is going to collide
        float rayLength = Mathf.Abs(deltaMove.x) + _skinWidth;

        // if I'm moving this slow I still want to have a length long enough to potentially hit something
        if (Mathf.Abs(deltaMove.x) < _skinWidth)
        {
            rayLength = 2 * _skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
            // we sum deltaMove.x to cast the ray from the point we will be 
            rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (showRays)
                Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit)
            {
                // if we are stuck inside something (e.g. a platform) we'll check for collision with another ray
                if (hit.distance == 0.0f)
                {
                    continue;
                }

                // if we hit a slope, find its angle with the ground
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                // if the bottom ray hits a climbable slope
                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    // if we are descending but suddenly we hit a climbable slope
                    if (collisionInfo.descendingSlope)
                    {
                        // then we are not descending anymore (we start to climb!)
                        collisionInfo.descendingSlope = false;
                        // being on a descending slope your x deltaMove is reduced, hence when
                        // hitting the climbable slope it becomes even more reduced and you slow down
                        // dramatically. To solve it, when we collide, we reset the deltaMove to the starting/input one
                        deltaMove = collisionInfo.inputVelocity;
                    }

                    float distanceToSlopeStart = 0.0f;
                    // if I'm going to climb a new slope (so also ground->first slope)
                    if (slopeAngle != collisionInfo.slopeAngleOld)
                    {
                        // cache the distance from you and the slope
                        distanceToSlopeStart = hit.distance - _skinWidth;
                        // and remove it from the deltaMove so the next ClimbSlope won't do anything
                        // this way you won't start to climb (move diagonally) before actually hitting the slope
                        deltaMove.x -= distanceToSlopeStart * directionX;
                    }

                    ClimbSlope(ref deltaMove, slopeAngle, hit.normal);

                    // if you just hit a new slope be sure to stick to the slope in this frame before moving diagonally
                    deltaMove.x += distanceToSlopeStart * directionX;
                }

                // if I'm not climbing or I just hit an obstacle (while climbing or not)
                if ((!collisionInfo.climbingSlope || slopeAngle > maxSlopeAngle) && !collisionInfo.slidingDownMaxSlope)
                {
                    // Note that you consider the Mathf.Min because if you are climbing a slope and you are here it means
                    // that you found an obstacle. In that case you have to check that the actual hit distance is < than the reduced x deltaMove
                    // (due to the slope angle projection). Otherwise you are not hitting anything in this frame (you'll do in the next).

                    // stick to the surface you just hit instead of compenetrate it
                    deltaMove.x = Mathf.Min(Mathf.Abs(deltaMove.x), (hit.distance - _skinWidth)) * directionX;
                    // if you are only partially colliding it could be that the next ray
                    // will collide with a lower surface making you compenetrate the first one
                    rayLength = Mathf.Min(Mathf.Abs(deltaMove.x) + _skinWidth, hit.distance);

                    // if I'm climbing (and a top ray just hit an obstacle, that is a slope with not climbable angle)
                    if (collisionInfo.climbingSlope)
                    {
                        // set a fixed y deltaMove based on the x deltaMove (y = x*tg(angle)). In this way when the top ray 
                        // collides it sticks to the obstacle and the y deltaMove will be fixed too, instead of moving up and down
                        deltaMove.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaMove.x);
                    }

                    // set the collision direction in the struct
                    collisionInfo.left = directionX == -1;
                    collisionInfo.right = directionX == 1;
                }               
            }
        }

    }

    // Detect vertical collisions and change deltaMove accordingly
    private void VerticalCollisions(ref Vector2 deltaMove)
    {
        float directionY = Mathf.Sign(deltaMove.y);
        // we are casting from inside the actual box so we have to sum the skinWidth
        // to check if the actual box is going to collide
        float rayLength = Mathf.Abs(deltaMove.y) + _skinWidth;

        // used to check that you are hitting only one object at a time
        List<Collider2D> hitsId = new List<Collider2D>();

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
            // we sum deltaMove.x to cast the ray from the point we will be
            // (in the move function we first change the x deltaMove)
            rayOrigin += Vector2.right * (_verticalRaySpacing * i + deltaMove.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            if (showRays)
                Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);
            
            if (hit)
            {
                //test
                hitsId.Add(hit.collider);

                // if we want to go through an obstacle... 
                // e.g. to jump on a platform from below
                if (hit.collider.tag == "Passable")
                {
                    //... moving up and not being stuck in it
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }

                    // this avoids jumping off through more than one platform while going down
                    if (hit.collider == collisionInfo.fallingThroughPlatform)
                    {
                        continue;
                    }

                    //... or to jump off through that obstacle
                    if (_playerInput.y == -1 && _playerInputScript != null && _playerInputScript.isPressingJump) 
                    {
                        collisionInfo.fallingThroughPlatform = hit.collider;
                        continue;
                    }
                }
                else
                {
                    collisionInfo.fallingThroughPlatform = null;
                }

                // stick to the surface you just hit instead of compenetrate it
                deltaMove.y = (hit.distance - _skinWidth) * directionY;
                // if you are only partially colliding it could be that the next ray
                // will collide with a lower surface making you compenetrate the first one
                rayLength = hit.distance;

                // if I'm climbing and a vertical ray hit an obstacle I should also stop my horizontal deltaMove
                if (collisionInfo.climbingSlope)
                {
                    // x = y / tg(angle). This way you don't start going right and left when the vertical ray hits the obstacle
                    // the sign is just to keep the same direction that I was moving before doing this
                    deltaMove.x = deltaMove.y / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(deltaMove.x);
                }

                // set the collision direction in the struct
                collisionInfo.below = directionY == -1;
                collisionInfo.above = directionY == 1;
            }
        }

        // this block avoids the "flying effect" in which characters attract each other but they are 
        // in air hence flying (because the one on the top sees the one below as "ground").
        if (!collisionInfo.below && directionY == 1 && hitsId.Count != 0)
        {
            bool isTheSame = true;
            int id = 0;
            for (int i = 0; i < hitsId.Count; i++)
            {
                int thisID = hitsId[i].GetInstanceID();

                if (i > 0)
                {
                    // check that you collided only with a single object 
                    isTheSame = (thisID == id);
                }

                id = thisID;
            }

            if (isTheSame)
            {
                MagneticField otherField = hitsId[0].GetComponentInChildren<MagneticField>();
                // if it has a magnetic field
                if (otherField != null)
                {
                    // unregister...
                    _magneticObject.UnregisterForce(otherField.ID);
                    //...till below is true. We can use zero as force since once registered
                    // the onStay will update it anyway (it will miss the force only in the first frame)
                    tillBelow = RegisterForceWhenBelow(otherField.ID, Vector2.zero);
                    StartCoroutine(tillBelow);
                }
            }
        }

        // if we start climbing a new steeper slope it could happen that for the first frame we compenetrate it
        // causing the player to be stuck for a couple of frames (which is visible). To check this we cast a ray
        // from a specific point so that I'm sure that I hit something and then correct it.
        if (collisionInfo.climbingSlope)
        {
            float directionX = collisionInfo.faceDir;
            rayLength = Mathf.Abs(deltaMove.x) + _skinWidth;
            // this is the point where I should have been if I wasn't compenetrating the new slope
            // from this point I should hit the new slope (except in some triple slope cases).
            Vector2 rayOrigin = ((directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight) + Vector2.up * deltaMove.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // if it's indeed a new (steeper) slope correct the x deltaMove
                if (slopeAngle != collisionInfo.slopeAngle)
                {
                    // I just need to move the x deltaMove (to avoid it from being close to zero)
                    // the y deltaMove (pushing away from the compenetration) is already an emerging behaviour
                    deltaMove.x = (hit.distance - _skinWidth) * directionX;
                    collisionInfo.slopeAngle = slopeAngle;
                    collisionInfo.slopeNormal = hit.normal;
                }
            }
        }
    }

    // Set a new deltaMove (x and y components) based on the slope angle
    private void ClimbSlope(ref Vector2 deltaMove, float slopeAngle, Vector2 slopeNormal)
    {
        // the amount of distance that I'd move without the slope on the x 
        // considered as it was along the slope direction and then projected on x and y
        float moveDistance = Mathf.Abs(deltaMove.x);

        // movement on the slope equal to the one on the ground in terms of units (arcade style)
        // projection of the movement on the y axis
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        // if we are NOT jumping while climbing
        if (deltaMove.y <= climbVelocityY)
        {
            deltaMove.y = climbVelocityY;
            // projection of the movement on the x axis
            deltaMove.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * collisionInfo.faceDir;
            collisionInfo.below = true; // so that we can jump while climbing
            collisionInfo.climbingSlope = true;
            collisionInfo.slopeAngle = slopeAngle;
            collisionInfo.slopeNormal = slopeNormal;
        }

    }

    // Check that I'm on a walkable descending slope and change the x and y deltaMove components accordingly
    private void DescendSlope(ref Vector2 deltaMove)
    {
        // used for unwalkable slopes (too steep)
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(_raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(deltaMove.y) + _skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(_raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(deltaMove.y) + _skinWidth, collisionMask);

        // if only one of the raycast hit something
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref deltaMove);
            SlideDownMaxSlope(maxSlopeHitRight, ref deltaMove);
        }


        if (!collisionInfo.slidingDownMaxSlope)
        {
            float directionX = collisionInfo.faceDir;
            // the corner touching the descending slope 
            Vector2 rayOrigin = directionX == -1 ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // if this is actually a descending slope (and not the ground) and I can descend it
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    // if I'm moving in the slope direction (therefore descending direction)
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        // if my y distance is less than the desired y (tan*x), then it means that for this frame I'm stuck in
                        // the descending slope, hence moving only on the x axis causing a "downstair" kind of movement.
                        // HOWEVER the distance should also not be higher otherwise it means that instead of descending we are
                        // actually jumping off from the descending slope when immediately changing direction (if steeper enough).
                        // So if we want this last behaviour put the following lines inside the if below, otherwise not.
                        //if ((hit.distance - _skinWidth) <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaMove.x)

                        // To avoid jumping off we cast horizontally to see if we hit something, otherwise we could block mid-air after jumping.
                        // We also have to check that we are not jumping aside a wall by checking that the normal is the same as the slope one
                        RaycastHit2D horizontalHit = Physics2D.Raycast(rayOrigin, Vector2.right * -directionX, _skinWidth * 2, collisionMask);

                        if ((hit.distance - _skinWidth) <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaMove.x) || (horizontalHit && hit.normal == horizontalHit.normal))
                        {
                            // the amount of distance that I'd move without the slope on the x 
                            // considered as it was along the slope direction and then projected on x and y
                            float moveDistance = Mathf.Abs(deltaMove.x);

                            // projections of the diagonal deltaMove to x and y (like in ClimbSlope)
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

                            deltaMove.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * deltaMove.x;
                            // avoid the y deltaMove to stop becoming stuck
                            deltaMove.y -= descendVelocityY;

                            collisionInfo.slopeAngle = slopeAngle;
                            collisionInfo.slopeNormal = hit.normal;
                            collisionInfo.descendingSlope = true;
                            collisionInfo.below = true;
                        }


                    }
                }
            }
        }
        
    }

    // Calculate the correct x deltaMove when sliding (falling) down a steep slope
    private void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 deltaMove)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                // how much I should move along the slope on the x axis
                deltaMove.x = (Mathf.Abs(deltaMove.y) - hit.distance + _skinWidth) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
                deltaMove.x *= Mathf.Sign(hit.normal.x);

                collisionInfo.slopeAngle = slopeAngle;
                collisionInfo.slopeNormal = hit.normal;
                collisionInfo.slidingDownMaxSlope = true;
            }
        }
    }


    // Register the force on this magnetic object only when touching something below (fixing "flying effect")
    private IEnumerator RegisterForceWhenBelow(int id, Vector2 force)
    {
        while (!collisionInfo.below)
        {
            yield return null;
        }

        _magneticObject.RegisterForce(id, force);
    }

    // called by the field of the object above you, to avoid potential bugs
    public void StopTillBelow()
    {
        StopCoroutine(tillBelow);
    }

    // utility functions to cast rays manually in a given horizontal direction returning the colliders hit
    public HashSet<Collider2D> RaycastHorizontally(Vector2 direction)
    {
        float rayLength = 2 * _skinWidth;
        Vector2 rayOrigin = (direction.x == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
        HashSet<Collider2D> colliders = new HashSet<Collider2D>();

        for (int i = 0; i < horizontalRayCount; i++)
        {
            rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, rayLength, collisionMask);
            if (hit)
            {
                // add the collider to the hashset (so only unique elements, no check needed)
                colliders.Add(hit.collider);
            }
        }

        return colliders;
    }
}
