using UnityEngine;

/// <summary>
/// روی هر Checkpoint قرار می‌گیرد.
///
/// Requirements:
/// - BoxCollider
/// - Is Trigger = true
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public sealed class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private bool activateOnlyOnce = true;

    private bool activated;
    private CheckpointManager checkpointManager;

    public void Initialize(CheckpointManager manager)
    {
        checkpointManager = manager;
    }

    private void Reset()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activateOnlyOnce && activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager reference is missing.");
            return;
        }

        checkpointManager.SetCheckpoint(transform);

        activated = true;

        Debug.Log($"Checkpoint Activated : {name}");
    }
}
