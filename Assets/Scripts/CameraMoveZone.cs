using UnityEngine;

public class CameraMoveZone : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private Transform destinationPoint;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private bool useDestinationYaw = true;
    [SerializeField] private bool overrideCameraPitch;
    [SerializeField] private float targetCameraPitch;

    [Header("Settings")]
    [SerializeField] private bool canUse = true;
    [SerializeField] private string interactionText = "Move";
    public string InteractionText => interactionText;
    [SerializeField] private float customMoveDuration = -1f;

    public Transform DestinationPoint => destinationPoint;
    public Transform LookAtTarget => lookAtTarget;
    public bool UseDestinationYaw => useDestinationYaw;
    public bool OverrideCameraPitch => overrideCameraPitch;
    public float TargetCameraPitch => targetCameraPitch;
    public bool CanUse => canUse;
    public bool HasCustomMoveDuration => customMoveDuration >= 0f;
    public float CustomMoveDuration => customMoveDuration;

    public void SetCanUse(bool value)
    {
        canUse = value;
    }

    private void OnDrawGizmosSelected()
    {
        if (destinationPoint == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, destinationPoint.position);
        Gizmos.DrawWireSphere(destinationPoint.position, 0.12f);

        if (lookAtTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(destinationPoint.position, lookAtTarget.position);
            Gizmos.DrawWireSphere(lookAtTarget.position, 0.08f);
        }
    }
}
