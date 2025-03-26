using UnityEngine;

public static class Constants
{
    public const float GROUND_CHECK = .01f;
    public const float COLLISION_OFFSET = .001f;
        
    public static readonly Vector3[] Directions = 
    {
        Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
        Vector3.up, Vector3.down,
        new Vector3(1, 0, 1).normalized, new Vector3(-1, 0, -1).normalized,
        new Vector3(-1, 0, 1).normalized, new Vector3(1, 0, -1).normalized
    };
}