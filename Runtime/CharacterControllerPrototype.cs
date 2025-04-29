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
    
        private static RaycastHit[] collisions = new RaycastHit[8];
        private static Collider[] overlaps = new Collider[8];

        private const int MAX_PROJECTIONS = 4;

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
            Vector3 input = LegacyInput.GetMoveInput();
            Vector3 inputDirection = transform.TransformDirection(input).normalized;
            Vector3 inputVector = inputDirection * SpeedVector();
            
            Vector3 moveDirection = inputVector;
            
            bool isGrounded = IsGrounded(transform.position, out Vector3 groundNormal);
            if(isGrounded)
                moveDirection = Vector3.ProjectOnPlane(inputVector, groundNormal);
            
            float angle = Vector3.Angle(groundNormal, Vector3.up);
            bool canClimb = angle <= maxSlopeAngle;
            if(!isGrounded || !canClimb)
                moveDirection += Physics.gravity * Time.deltaTime;
            
            if (CheckOverlaps(out Vector3 depenetration))
            {
                // if player isn't moving use the depenetration vector for movement
                if(inputVector.sqrMagnitude == 0)
                    moveDirection = depenetration * SpeedVector();
                
                // if the player is not moving in the same direction as the depenetration vector, project their input into the depenetration vector
                float moveDot = Vector3.Dot(moveDirection, depenetration);
                if (moveDot < 0)
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, depenetration);
                
                // if the depenetration is pushing body into ground we need to project onto ground
                // but if the depenetration is moving up and out then we don't want to project onto ground
                float groundDot = Vector3.Dot(depenetration, groundNormal);
                if(groundDot < 0)
                    moveDirection = Vector3.ProjectOnPlane(depenetration, groundNormal);

                if (moveDirection.magnitude > SpeedVector())
                    moveDirection = Vector3.ClampMagnitude(moveDirection, SpeedVector());
            }
            
            bool foundStep = CheckStep(transform.position, moveDirection, out Vector3 stepDirection);
            if (isGrounded && foundStep)
                moveDirection = stepDirection.normalized * SpeedVector();
            
            moveDirection = Move(transform.position, moveDirection);
            
            transform.position += moveDirection;
        }

        public Vector3 Move(Vector3 position, Vector3 direction)
        {
            Vector3 velocity = direction;
            Vector3 normal = Vector3.zero;
            
            (Vector3 bottom, Vector3 top) = Functions.CreateCapsuleCastPoints(position, CastRadius(), height);

            for (int i = 0; i < MAX_PROJECTIONS; i++)
            {
                int hitCount = Physics.CapsuleCastNonAlloc(bottom, top, CastRadius(), velocity.normalized, collisions, velocity.magnitude, collide, QueryTriggerInteraction.Ignore);

                if (hitCount == 0)
                    break;
                
                Vector3 nextNormal = Functions.GetCastNormal(collisions, hitCount);

                float nextNormalsDot = Vector3.Dot(nextNormal, normal);
                
                if(nextNormalsDot < 0)
                {
                   velocity = Vector3.zero;
                   break;
                }
                
                normal += nextNormal;
                normal.Normalize();
                
                velocity = Vector3.ProjectOnPlane(velocity, normal);
                
                float velocityProjection = Vector3.Dot(velocity, direction);
                if (velocityProjection < 0)
                {
                    velocity = Vector3.zero;
                    break;
                }

                if (velocity.sqrMagnitude < Mathf.Epsilon)
                    break;
            }
            
            return velocity;
        }
        
        private bool IsGrounded(Vector3 position, out Vector3 normal)
        {
            Vector3 castOrigin = position + new Vector3(0, radius + skinWidth);
            float distance = skinWidth * 2;

            int count = Physics.SphereCastNonAlloc(castOrigin, radius, Vector3.down, collisions, distance, collide, QueryTriggerInteraction.Ignore);
            
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
            Vector3 castOrigin = position + new Vector3(0, radius);
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
                if(collisionHeight > skinWidth && collisionHeight <= stepHeight)
                {
                    stepDirection = collisions[i].point - position;
                    foundStep = true;
                }
            }
            
            return foundStep;
        }

        public bool CheckOverlaps(out Vector3 depenetrateNormal)
        {
            (Vector3 bottom, Vector3 top) = Functions.CreateCapsuleCastPoints(transform.position, CastRadius(), height);
            int count = Physics.OverlapCapsuleNonAlloc(bottom, top, CastRadius(), overlaps, collide);

            bool didAnyCompute = false;
            depenetrateNormal = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                Collider overlapCollider = overlaps[i];
                
                didAnyCompute |= Physics.ComputePenetration(
                    collider, transform.position, transform.rotation,
                    overlapCollider, overlapCollider.transform.position, overlapCollider.transform.rotation,
                    out Vector3 direction, out float distance
                );
                
                depenetrateNormal += direction.normalized;
            }
            
            depenetrateNormal.Normalize();

            return didAnyCompute;
        }
    }
}
