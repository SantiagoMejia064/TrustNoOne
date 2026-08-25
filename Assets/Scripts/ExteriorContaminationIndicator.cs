using UnityEngine;

public class ExteriorContaminationIndicator : MonoBehaviour
{
    [SerializeField] private BunkerStateManager bunkerStateManager;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private float blinkSpeed = 4f;

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
            runtimeMaterial.color = inactiveColor;
        }
    }

    private void OnEnable()
    {
        if (bunkerStateManager != null)
        {
            bunkerStateManager.onExteriorContaminationChanged.AddListener(UpdateIndicatorState);
        }

        UpdateIndicatorState();
    }

    private void OnDisable()
    {
        if (bunkerStateManager != null)
        {
            bunkerStateManager.onExteriorContaminationChanged.RemoveListener(UpdateIndicatorState);
        }
    }

    private void Update()
    {
        if (runtimeMaterial == null || bunkerStateManager == null)
        {
            return;
        }

        if (!bunkerStateManager.IsExteriorContaminationActive())
        {
            runtimeMaterial.color = inactiveColor;
            return;
        }

        float blink = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        runtimeMaterial.color = Color.Lerp(inactiveColor, alertColor, blink);
    }

    private void UpdateIndicatorState()
    {
        if (runtimeMaterial == null || bunkerStateManager == null)
        {
            return;
        }

        runtimeMaterial.color = bunkerStateManager.IsExteriorContaminationActive() ? alertColor : inactiveColor;
    }
}
