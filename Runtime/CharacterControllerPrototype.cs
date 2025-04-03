using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Addifex.Kinematics
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterControllerPrototype : MonoBehaviour
    {
        [SerializeField]
        private float speed = 1.0f;
        [SerializeField]
        private LayerMask collide;
        [SerializeField]
        private float maxSlopeAngle = 45;
        [SerializeField]
        private float skinWidth = 0.05f;
        [SerializeField]
        private float stepHeight = 0.25f;
        
        private new CapsuleCollider collider;
    
        private float radius = 0.5f;
        private float height = 2;
    
        private Vector3 velocity = Vector3.zero;
    
        private static RaycastHit[] collisions = new RaycastHit[8];
        private static Collider[] overlaps = new Collider[8];

        private void Awake()
        {
            collider = GetComponent<CapsuleCollider>();
        
            radius = collider.radius;
            height = collider.height;
        }
        
        public float CastRadius() => radius - skinWidth;
        
        private float SpeedVector() => speed * Time.deltaTime;

        private void Update()
        {
            Vector3 input = GetMoveInput();
            Vector3 inputDirection = transform.TransformDirection(input).normalized;

            Vector3 movement = inputDirection * SpeedVector();
        
            // check for ground first because we want to keep the players input as a projection
            // on ground before we check to see if they are overlapping with something
            bool foundGround = IsGrounded(transform.position, out Vector3 groundNormal);
            velocity = Vector3.ProjectOnPlane(movement, groundNormal);
            
            if (CheckOverlaps(out Vector3 depenetration))
            {
                // if player isn't moving use the depenetration vector for movement
                velocity = velocity == Vector3.zero ? depenetration * SpeedVector() : velocity;
                // if the player is not moving away from the overlap, project their input into the depenetration vector
                if (Vector3.Dot(velocity, depenetration) <= 0)
                    velocity = Vector3.ProjectOnPlane(velocity, depenetration.normalized);
            }
            
            float angle = Vector3.Angle(Vector3.up, groundNormal);
            if(!foundGround || angle > maxSlopeAngle)
            {
                velocity += Physics.gravity * Time.deltaTime;
            }

            if (foundGround && CheckStep(transform.position, velocity, out Vector3 stepDirection))
            {
                //velocity += stepDirection.normalized * SpeedVector();
            }
            
            transform.position = Move(transform.position, velocity);
        }

        public bool CheckOverlaps(out Vector3 outDirection)
        {
            (Vector3 bottom, Vector3 top) = Functions.CreateCapsuleCastPoints(transform.position, radius, height);
            int count = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, overlaps, collide);

            outDirection = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                Collider overlapCollider = overlaps[i];
                
                Physics.ComputePenetration(
                    collider, transform.position, transform.rotation,
                    overlapCollider, overlapCollider.transform.position, overlapCollider.transform.rotation,
                    out Vector3 direction, out float distance
                );
                
                outDirection += direction.normalized * distance;
            }

            return count > 0;
        }
    
        public Vector3 Move(Vector3 position, Vector3 direction)
        {
            (Vector3 bottom, Vector3 top) = Functions.CreateCapsuleCastPoints(position, CastRadius(), height);
        
            int hitCount = Physics.CapsuleCastNonAlloc(bottom, top, CastRadius(), direction.normalized, collisions, direction.magnitude, collide, QueryTriggerInteraction.Ignore);
            
            Vector3 normal = Vector3.zero;

            for (int i = 0; i < hitCount; i++)
            {
                if(!Functions.IsOverlapping(collisions[i]))
                    normal += collisions[i].normal;
            }
            
            normal.Normalize();
            
            Vector3 projection = Vector3.ProjectOnPlane(direction, normal);
            Vector3 newPosition = position + projection;
        
            return newPosition;
        }
    
        // IsGround is used for checking if there is ground beneath the player
        // as well as returning the normal of the ground beneath (if any)
        // TODO: Test if filtering ground collisions by only collisions lower than skin width helps jitter
        private bool IsGrounded(Vector3 position, out Vector3 normal)
        {
            Vector3 castOrigin = position + new Vector3(0, CastRadius());
            float distance = skinWidth;

            int count = Physics.SphereCastNonAlloc(castOrigin, CastRadius(), Vector3.down, collisions, distance, collide, QueryTriggerInteraction.Ignore);
            
            normal = Vector3.zero;
        
            for (int i = 0; i < count; i++)
            {
                if (Functions.IsOverlapping(collisions[i]))
                    continue;
                
                normal += collisions[i].normal;
            }
            
            normal.Normalize();
        
            return count > 0;
        }

        private bool CheckStep(Vector3 position, Vector3 direction, out Vector3 stepDirection)
        {
            Vector3 castOrigin = position + direction + new Vector3(0, radius - skinWidth + stepHeight);
            float distance = radius;
            
            int count = Physics.SphereCastNonAlloc(castOrigin, radius, Vector3.down, collisions, distance, collide, QueryTriggerInteraction.Ignore);

            stepDirection = position;
            bool foundStep = false;
            for (int i = 0; i < count; i++)
            {
                if (Functions.IsOverlapping(collisions[i]))
                    continue;
                
                float collisionHeight = Functions.GetStepHeight(position, collisions[i]);
                if(collisionHeight > skinWidth && collisionHeight < stepHeight)
                {
                    stepDirection = collisions[i].point - position;
                    foundStep = true;
                }
            }
            
            return foundStep;
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
}
