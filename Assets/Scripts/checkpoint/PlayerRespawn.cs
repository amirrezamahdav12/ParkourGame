using UnityEngine;

/// <summary>
/// مسئول تشخیص مرگ و Respawn.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerRespawn : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CheckpointManager checkpointManager;

    [Header("Death Settings")]
    [SerializeField]
    private float deathY = -10f;

    private Rigidbody rb;
    private int deathLayer;
    private bool isRespawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager reference is missing.");
            enabled = false;
            return;
        }

        deathLayer = LayerMask.NameToLayer("DeathZone");

        if (deathLayer == -1)
        {
            Debug.LogError("Layer 'DeathZone' does not exist.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!isRespawning && transform.position.y < deathY)
        {
            Respawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isRespawning && other.gameObject.layer == deathLayer)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        if (isRespawning)
            return;

        Transform checkpoint = checkpointManager.CurrentCheckpoint;

        if (checkpoint == null)
            return;

        isRespawning = true;

        rb.isKinematic = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);

        rb.position = checkpoint.position;
        rb.rotation = checkpoint.rotation;

        rb.isKinematic = false;

        isRespawning = false;
    }
}
