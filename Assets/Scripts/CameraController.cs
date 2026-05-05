using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform[] targets;
    public float smoothSpeed = 0.125f;

    public float fixedY = 1.5f;
    public float fixedZ = -5f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (targets == null || targets.Length == 0)
            return;

        Vector3 center = GetCenterPoint();

        Vector3 desiredPosition = new Vector3(center.x, fixedY, fixedZ);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Length == 1)
            return targets[0].position;

        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        foreach (Transform target in targets)
        {
            if (target.gameObject.activeInHierarchy)
                bounds.Encapsulate(target.position);
        }

        return bounds.center;
    }
}
