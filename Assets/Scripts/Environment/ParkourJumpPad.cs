using UnityEngine;

public class ParkourJumpPad : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float apexHeight = 5f;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            return;

        rb.linearVelocity = CalculateLaunchVelocity(
            rb.position,
            target.position,
            apexHeight
        );
    }

    private Vector3 CalculateLaunchVelocity(
        Vector3 start,
        Vector3 target,
        float height)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 displacementXZ =
            new Vector3(
                target.x - start.x,
                0,
                target.z - start.z);

        Vector3 velocityY =
            Vector3.up * Mathf.Sqrt(2f * gravity * height);

        float timeUp =
            Mathf.Sqrt(2f * height / gravity);

        float timeDown =
            Mathf.Sqrt(
                2f * Mathf.Max(0.1f,
                target.y - start.y + height)
                / gravity);

        float totalTime = timeUp + timeDown;

        Vector3 velocityXZ =
            displacementXZ / totalTime;

        return velocityXZ + velocityY;
    }
}