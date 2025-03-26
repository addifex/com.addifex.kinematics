using UnityEngine;

public class CameraController3P : MonoBehaviour
{
    [SerializeField]
    private Transform lookAtPoint;
    public Vector3 offset = new Vector3(0, 2, -4);
        
    [Header("Rotation Settings")]
    public float sensitivity = 3f;
    public float minYAngle = -20f;
    public float maxYAngle = 70f; 

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoom = 2f, maxZoom = 10f;

    [Header("Collision Settings")]
    public LayerMask collisionMask;
    public float collisionSmoothTime = 0.1f;

    private float currentZoom;
    private float yaw, pitch;
    private Vector3 smoothPosition;

    void Start()
    {
        currentZoom = offset.magnitude;
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        HandleRotation();
        HandleZoom();
        Vector3 desiredPosition = CalculateCameraPosition();
        transform.position = ResolveCollisions(desiredPosition);
        transform.LookAt(lookAtPoint.position + Vector3.up * offset.y);
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    private Vector3 CalculateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        return lookAtPoint.position + rotation * new Vector3(0, offset.y, -currentZoom);
    }

    private Vector3 ResolveCollisions(Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - lookAtPoint.position;
        float distance = direction.magnitude;
        if (Physics.Raycast(lookAtPoint.position, direction.normalized, out RaycastHit hit, distance, collisionMask))
        {
            return hit.point + hit.normal * 0.2f;
        }
        return desiredPosition;
    }
}