using UnityEngine;

namespace Addifex.Kinematics
{
    /// <summary>
    /// Will be deprecated. Only used for prototyping.
    /// </summary>
    public static class LegacyInput
    {
        public static Vector3 GetMoveInput()
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