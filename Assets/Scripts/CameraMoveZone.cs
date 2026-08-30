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

    [Header("Highlight")]
    [SerializeField] private bool highlightOnLook = true;
    [SerializeField] private GameObject highlightTargetObject;
    [SerializeField] private Material highlightMaterial;

    [SerializeField] private GameObject[] moveZones;
    private Collider[] cachedColliders;
    private Renderer[] cachedRenderers;
    private Renderer[] highlightRenderers;
    private Material[][] originalHighlightMaterials;
    private bool isHovered;

    public Transform DestinationPoint => destinationPoint;
    public Transform LookAtTarget => lookAtTarget;
    public bool UseDestinationYaw => useDestinationYaw;
    public bool OverrideCameraPitch => overrideCameraPitch;
    public float TargetCameraPitch => targetCameraPitch;
    public bool CanUse => canUse;
    public bool HasCustomMoveDuration => customMoveDuration >= 0f;
    public float CustomMoveDuration => customMoveDuration;

    public void SetHovered(bool hovered)
    {
        if (isHovered == hovered)
        {
            return;
        }

        isHovered = hovered;
        ApplyHighlightState();
    }

    public void SetCanUse(bool value)
    {
        canUse = value;

        if (!value)
        {
            SetHovered(false);
        }
    }

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        CacheHighlightRenderers();
        ApplyUsableState(canUse);
        ApplyHighlightState();
    }

    public void ActivateZone()
    {
        canUse = true;
        ApplyUsableState(true);
        gameObject.SetActive(true);
    }

    public void OnPlayerArrived()
    {
        SetHovered(false);
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

    private void OnDisable()
    {
        SetHovered(false);
    }

    private void CacheHighlightRenderers()
    {
        GameObject target = highlightTargetObject != null ? highlightTargetObject : gameObject;
        highlightRenderers = target.GetComponentsInChildren<Renderer>(true);

        if (highlightRenderers == null || highlightRenderers.Length == 0)
        {
            originalHighlightMaterials = null;
            return;
        }

        originalHighlightMaterials = new Material[highlightRenderers.Length][];

        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            Renderer zoneRenderer = highlightRenderers[i];
            if (zoneRenderer == null)
            {
                continue;
            }

            originalHighlightMaterials[i] = zoneRenderer.sharedMaterials;
        }
    }

    private void ApplyHighlightState()
    {
        if (highlightRenderers == null || highlightRenderers.Length == 0)
        {
            CacheHighlightRenderers();
        }

        if (highlightRenderers == null || highlightRenderers.Length == 0)
        {
            return;
        }

        if (!highlightOnLook || !canUse || highlightMaterial == null)
        {
            RestoreHighlightMaterials();
            return;
        }

        if (!isHovered)
        {
            RestoreHighlightMaterials();
            return;
        }

        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            Renderer zoneRenderer = highlightRenderers[i];
            if (zoneRenderer == null)
            {
                continue;
            }

            Material[] originalMaterials = originalHighlightMaterials != null && i < originalHighlightMaterials.Length
                ? originalHighlightMaterials[i]
                : null;

            if (originalMaterials == null || originalMaterials.Length == 0)
            {
                continue;
            }

            Material[] highlightedMaterials = new Material[originalMaterials.Length];
            for (int m = 0; m < highlightedMaterials.Length; m++)
            {
                highlightedMaterials[m] = highlightMaterial;
            }

            zoneRenderer.sharedMaterials = highlightedMaterials;
        }
    }

    private void RestoreHighlightMaterials()
    {
        if (highlightRenderers == null || originalHighlightMaterials == null)
        {
            return;
        }

        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            Renderer zoneRenderer = highlightRenderers[i];
            Material[] originalMaterials = i < originalHighlightMaterials.Length
                ? originalHighlightMaterials[i]
                : null;

            if (zoneRenderer == null || originalMaterials == null)
            {
                continue;
            }

            zoneRenderer.sharedMaterials = originalMaterials;
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
