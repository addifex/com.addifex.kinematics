using UnityEngine;

namespace Addifex.Kinematics
{
    public static class Functions
    {
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
    }
}
