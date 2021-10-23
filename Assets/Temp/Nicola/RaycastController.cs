using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Utility script to handle physics collision by using raycasts

[RequireComponent(typeof(Collider2D))]
public class RaycastController : MonoBehaviour
{
    // Constant
    // increase a bit this value to increase the "sensitivity" for collision checking
    protected const float _skinWidth = 0.015f;

    // Public
    public LayerMask collisionMask;
    public bool showRays;


    // Internals
    [Tooltip("how far each ray should be from the next one")]
    [SerializeField]
    private float distBetweenRays = 0.25f;
    //distance between the rays that you are casting (in units) 
    protected float _horizontalRaySpacing;
    protected float _verticalRaySpacing;
    // how many rays are being casted horizontally/vertically
    protected int horizontalRayCount;
    protected int verticalRayCount;
    protected Collider2D _collider2D;
    protected RaycastOrigins _raycastOrigins;

    // store your AABB corners
    protected struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        CalculateRaySpacing();
    }

    // Update the struct based on the four corners of the collider's AABB 
    protected void UpdateRaycastOrigins()
    {
        // this is the AABB structure containing the collider
        // Note that for a box collider the AABB coincides with it as long as the box is not rotated at least...
        Bounds bounds = _collider2D.bounds;
        // by using a negative factor we can shrink the collider bounds
        // Expand(amount) applies amount/2 to every extent, that's why we multiply by 2
        bounds.Expand(_skinWidth * -2);

        // get the four bound corners and set them
        _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    // Calculate the distance between the rays that you are casting 
    protected void CalculateRaySpacing()
    {
        Bounds bounds = _collider2D.bounds;
        bounds.Expand(_skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        // we can resize the collider and the number of rays will adjust automatically when this function is called 
        horizontalRayCount =  Mathf.RoundToInt(boundsHeight / distBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / distBetweenRays);

        // be sure that at least two rays are casting both on horizontal and vertical direction
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // distance between the rays I'm casting in units
        _horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
}
