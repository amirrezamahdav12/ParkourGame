using UnityEngine;

/// <summary>
/// فقط مسئول نگهداری آخرین Checkpoint فعال.
/// </summary>
public sealed class CheckpointManager : MonoBehaviour
{
    [Header("Checkpoints (Element 0 = Start Point)")]
    [SerializeField]
    private Checkpoint[] checkpoints;

    public Transform CurrentCheckpoint { get; private set; }

    private void Awake()
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogError($"{nameof(CheckpointManager)} : No checkpoints assigned.");
            enabled = false;
            return;
        }

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (checkpoint != null)
                checkpoint.Initialize(this);
        }

        CurrentCheckpoint = checkpoints[0].transform;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
            return;

        if (CurrentCheckpoint == checkpoint)
            return;

        CurrentCheckpoint = checkpoint;

        Debug.Log($"Checkpoint Saved : {checkpoint.name}");
    }
}
