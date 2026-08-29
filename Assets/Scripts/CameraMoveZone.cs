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
    [SerializeField] private bool disableAfterUse;
    [SerializeField] private string interactionText = "Left click - Move";
    public string InteractionText => interactionText;
    [SerializeField] private float customMoveDuration = -1f;

    [SerializeField] private GameObject[] moveZones;
    private Collider[] cachedColliders;
    private Renderer[] cachedRenderers;

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

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        ApplyUsableState(canUse);
    }

    public void ActivateZone()
    {
        canUse = true;
        ApplyUsableState(true);
        gameObject.SetActive(true);
    }

    public void OnPlayerArrived()
    {
        canUse = false;
        ApplyUsableState(false);

        if (moveZones != null)
        {
            foreach (GameObject moveZone in moveZones)
            {
                if (moveZone == null)
                {
                    continue;
                }

                moveZone.SetActive(true);

                CameraMoveZone nextZone = moveZone.GetComponent<CameraMoveZone>();
                if (nextZone == null)
                {
                    nextZone = moveZone.GetComponentInChildren<CameraMoveZone>(true);
                }

                if (nextZone != null)
                {
                    nextZone.ActivateZone();
                }
            }
        }

        if (disableAfterUse)
        {
            gameObject.SetActive(false);
        }
    }

    private void ApplyUsableState(bool isUsable)
    {
        if (cachedColliders == null || cachedColliders.Length == 0)
        {
            cachedColliders = GetComponentsInChildren<Collider>(true);
        }

        if (cachedRenderers == null || cachedRenderers.Length == 0)
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
        }

        foreach (Collider zoneCollider in cachedColliders)
        {
            if (zoneCollider == null)
            {
                continue;
            }

            zoneCollider.enabled = isUsable;
        }

        foreach (Renderer zoneRenderer in cachedRenderers)
        {
            if (zoneRenderer == null)
            {
                continue;
            }

            zoneRenderer.enabled = isUsable;
        }
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
