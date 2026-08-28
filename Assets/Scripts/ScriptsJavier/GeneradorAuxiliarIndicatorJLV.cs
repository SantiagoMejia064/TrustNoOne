using UnityEngine;


public class GeneradorAuxiliarIndicatorJLV : MonoBehaviour
{
    [SerializeField] private BunkerStateManager bunkerStateManager;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color colorDisponible = Color.green;
    [SerializeField] private Color colorAveriado = Color.red;

    private Material runtimeMaterial;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer != null)
        {
            runtimeMaterial = targetRenderer.material;
        }
    }

    private void OnEnable()
    {
        if (bunkerStateManager != null)
        {
            bunkerStateManager.onAuxiliaryGeneratorAvailabilityChanged.AddListener(UpdateIndicatorState);
        }

        UpdateIndicatorState();
    }

    private void OnDisable()
    {
        if (bunkerStateManager != null)
        {
            bunkerStateManager.onAuxiliaryGeneratorAvailabilityChanged.RemoveListener(UpdateIndicatorState);
        }
    }

    private void UpdateIndicatorState()
    {
        if (runtimeMaterial == null || bunkerStateManager == null)
        {
            return;
        }

        runtimeMaterial.color = bunkerStateManager.IsAuxiliaryGeneratorAvailable()
            ? colorDisponible
            : colorAveriado;
    }
}