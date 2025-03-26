using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class CharacterControllerPrototype : MonoBehaviour
{
    [SerializeField]
    private float speed = 1.0f;
    [SerializeField]
    private LayerMask collide;
    [SerializeField]
    private float maxSlopeAngle = 45;

    private new CapsuleCollider collider;
    
    private float radius = 0.5f;
    private float height = 2;
    
    private Vector3 velocity = Vector3.zero;
    
    private static RaycastHit[] collisions = new RaycastHit[8];
    private static Collider[] overlaps = new Collider[8];

    private const float MAX_STEP_HEIGHT = .25f;

    private void Awake()
    {
        collider = GetComponent<CapsuleCollider>();
        
        radius = collider.radius;
        height = collider.height;
    }

    private void Start()
    {
        CheckForOverlaps();
    }

    private void CheckForOverlaps()
    {
        (Vector3 bottom, Vector3 top) = Functions.CreateCapsuleCastPoints(transform.position, radius, height);
        int overlapCount = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, overlaps, collide);

        for (int i = 0; i < overlapCount; i++)
        {
            transform.position = ResolveOverlap(overlaps[i], transform.position);
        }
    }

    private void Update()
    {
        Vector3 input = GetMoveInput();
        Vector3 direction = transform.TransformDirection(input).normalized;
        Vector3 movement = direction * (speed * Time.deltaTime);
        
        bool foundGround = IsGrounded(transform.position, out RaycastHit groundHit);
        
        float angle = Vector3.Angle(Vector3.up, groundHit.normal);
        
        if(foundGround && angle <= maxSlopeAngle)
            velocity = Vector3.ProjectOnPlane(movement, groundHit.normal);
        else
            velocity = movement + Physics.gravity * Time.deltaTime;
        
        transform.position = Move(transform.position, velocity);
    }
    
    public Vector3 Move(Vector3 position, Vector3 direction)
    {
        (Vector3 bottom, Vector3 top) = Functions.CreateCapsuleCastPoints(position, radius, height);
        
        int hitCount = Physics.CapsuleCastNonAlloc(bottom, top, radius, direction.normalized, collisions, direction.magnitude, collide, QueryTriggerInteraction.Ignore);
        
        if (hitCount == 0)
            return position + direction;

        RaycastHit closestHit = new()
        {
            distance = float.PositiveInfinity,
        };
        
        for (int i = 0; i < hitCount; i++)
        {
            if(collisions[i].distance < closestHit.distance)
                closestHit = collisions[i];
        }
        
        if(closestHit.distance == 0 && closestHit.point == Vector3.zero)
            return ResolveOverlap(closestHit.collider, position + direction);

        Vector3 projection = Vector3.ProjectOnPlane(direction, closestHit.normal);
        Vector3 newPosition = position + projection;
        
        return newPosition;
    }
    
    private bool IsGrounded(Vector3 position, out RaycastHit ground)
    {
        Vector3 castOrigin = position + new Vector3(0, radius);
        float distance = Constants.GROUND_CHECK;

        int count = Physics.SphereCastNonAlloc(castOrigin, radius, Vector3.down, collisions, distance, collide, QueryTriggerInteraction.Ignore);

        ground = new()
        {
            distance = float.PositiveInfinity,
            normal = Vector3.up
        };
        
        for (int i = 0; i < count; i++)
        {
            if(collisions[i].distance < ground.distance)
                ground = collisions[i];
        }
        
        return count > 0;
    }

    private Vector3 ResolveOverlap(Collider overlap, Vector3 position)
    {
        bool didPenetrate = Physics.ComputePenetration(
            collider, position, transform.rotation,
            overlap, overlap.transform.position, overlap.transform.rotation,
            out Vector3 direction,
            out float distance
        );
        
        return didPenetrate ? 
            transform.position + direction * distance
            :
            FindNonOverlappingPosition(position);
    }
    
    private Vector3 FindNonOverlappingPosition(Vector3 position)
    {
        float searchRadius = radius + Constants.COLLISION_OFFSET;
        int maxAttempts = 4;
        float stepDistance = 0.01f;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            foreach (Vector3 direction in Constants.Directions)
            {
                Vector3 testPosition = position + direction * (i * stepDistance);
                
                (Vector3 bottom, Vector3 top) = Functions.CreateCapsuleCastPoints(testPosition, radius, height);

                if (Physics.OverlapCapsuleNonAlloc(bottom, top, searchRadius, overlaps, collide) == 0)
                {
                    return testPosition;
                }
            }
        }
        
        return position;
    }

    private static Vector3 GetMoveInput()
    {
        float forward = Input.GetKey(KeyCode.W) ? 1 : 0;
        float back = Input.GetKey(KeyCode.S) ? 1 : 0;
        float left = Input.GetKey(KeyCode.A) ? 1 : 0;
        float right = Input.GetKey(KeyCode.D) ? 1 : 0;

        float moveX = right - left;
        float moveY = forward - back;
        
        return new Vector3(moveX, 0.0f, moveY);
    }
}
