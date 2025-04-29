using UnityEngine;

namespace Addifex.Kinematics
{
    public static class Functions
    {
        public static Vector3 RelativeInput(Transform transform, Vector3 input)
        {
            return transform.TransformDirection(input).normalized;
        }
        
        public static (Vector3 bottom, Vector3 top) CreateCapsuleCastPoints(Vector3 position, float radius, float height)
        {
            Vector3 bottom = position + new Vector3(0, radius);
            Vector3 top = position + new Vector3(0, height - radius);
        
            return (bottom, top);
        }

        public static float GetStepHeight(Vector3 position, RaycastHit hit)
        {
            return hit.point.y - position.y;
        }

        public static bool IsOverlapping(RaycastHit hit)
        {
            return hit.distance == 0 && hit.point == Vector3.zero;
        }

        public static RaycastHit GetClosestHit(RaycastHit[] hits, int hitCount)
        {
            if(hits.Length == 0 || hitCount == 0)
                return new RaycastHit();
            
            RaycastHit closest = new()
            {
                distance = float.MaxValue,
            };

            for (int i = 0; i < hitCount; i++)
            {
                if(hits[i].distance < closest.distance)
                    closest = hits[i];
            }
            
            return closest;
        }

        public static RaycastHit GetFurthestHit(RaycastHit[] hits, int hitCount)
        {
            RaycastHit furthest = new()
            {
                distance = float.MinValue
            };

            for (int i = 0; i < hitCount; i++)
            {
                if(hits[i].distance > furthest.distance)
                    furthest = hits[i];
            }
            
            return furthest;
        }

        /// <summary>
        /// Add up all the normals in the collision.
        /// </summary>
        /// <param name="collisions">Array of RaycastHit data.</param>
        /// <param name="count">The number of collisions.</param>
        /// <returns>Normalized sum of all normals in collisions. Returns zero if empty.</returns>
        public static Vector3 GetCastNormal(RaycastHit[] collisions, int count)
        {
            Vector3 normal = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                if(!IsOverlapping(collisions[i]))
                    normal += collisions[i].normal;
            }
            normal.Normalize();

            return normal;
        }

        public static Vector3 Average(Vector3 a, Vector3 b)
        {
            return (a + b) / 2f;
        }
    }
}
