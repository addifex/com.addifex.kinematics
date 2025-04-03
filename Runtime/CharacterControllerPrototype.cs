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
            
            velocity = inputDirection * SpeedVector();
            
            bool foundGround = IsGrounded(transform.position, out Vector3 groundNormal);
            velocity = Vector3.ProjectOnPlane(velocity, groundNormal);
            
            if (foundGround && CheckStep(transform.position, velocity, out Vector3 stepDirection))
                velocity = Vector3.ProjectOnPlane(velocity, stepDirection.normalized);
            
            float angle = Vector3.Angle(Vector3.up, velocity);
            if(!foundGround || angle > maxSlopeAngle)
                velocity += Physics.gravity * Time.deltaTime;
            
            // apply this last so depenetration vector takes precedence 
            if (CheckOverlaps(out Vector3 depenetration))
            {
                // if player isn't moving use the depenetration vector for movement
                if(velocity.sqrMagnitude == 0)
                    velocity = depenetration * SpeedVector();
                // if the player is not moving away from the overlap, project their input into the depenetration vector
                if (Vector3.Dot(velocity, depenetration) <= 0)
                    velocity = Vector3.ProjectOnPlane(velocity, depenetration.normalized);
            }
            
            if(CheckMove(transform.position, velocity, out Vector3 hitNormal))
                velocity = Vector3.ProjectOnPlane(velocity, hitNormal);
            
            Vector3 newPosition = transform.position + velocity;
            transform.position = newPosition;
        }
        
        /// <summary>
        /// IsGround is used for checking if there is ground beneath the player
        /// as well as returning the normal of the ground beneath (if any)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
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
            Vector3 castOrigin = position + direction + new Vector3(0, radius);
            float distance = direction.magnitude;
            
            int count = Physics.SphereCastNonAlloc(castOrigin, radius, direction.normalized, collisions, distance, collide, QueryTriggerInteraction.Ignore);

            Vector3 positionCheck = position + new Vector3(0, skinWidth); 
            stepDirection = direction;
            bool foundStep = false;
            for (int i = 0; i < count; i++)
            {
                bool isOverlapping = Functions.IsOverlapping(collisions[i]);
                if (isOverlapping)
                    continue;
                
                float collisionHeight = Functions.GetStepHeight(positionCheck, collisions[i]);
                if(collisionHeight > skinWidth && collisionHeight < stepHeight)
                {
                    stepDirection = collisions[i].point - position;
                    foundStep = true;
                }
            }
            
            return foundStep;
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

        public bool CheckMove(Vector3 position, Vector3 direction, out Vector3 normal)
        {
            (Vector3 bottom, Vector3 top) = Functions.CreateCapsuleCastPoints(position, CastRadius(), height);
        
            int hitCount = Physics.CapsuleCastNonAlloc(bottom, top, CastRadius(), direction.normalized, collisions, direction.magnitude, collide, QueryTriggerInteraction.Ignore);
            
            normal = Vector3.zero;

            for (int i = 0; i < hitCount; i++)
            {
                if(!Functions.IsOverlapping(collisions[i]))
                    normal += collisions[i].normal;
            }
            
            normal.Normalize();

            return hitCount > 0;
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
