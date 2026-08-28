using System.Collections.Generic;
using UnityEngine;

public class ReserveLightIndicator : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [Header("References")]
    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Light[] targetLights;

    [Header("Lit State")]
    [SerializeField] private Color litEmissionColor = Color.green;
    [SerializeField] private float litEmissionIntensity = 2f;
    [SerializeField] private float litLightIntensity = 1f;

    [Header("Off State")]
    [SerializeField] private Color offEmissionColor = Color.black;
    [SerializeField] private float offEmissionIntensity;
    [SerializeField] private float offLightIntensity;

    [Header("Transition")]
    [SerializeField] private bool startLit;
    [SerializeField] private float transitionSpeed = 10f;

    private Material[] materialInstances;
    private Color currentEmissionColor;
    private Color targetEmissionColor;
    private Color targetLightColor;
    private float currentLightIntensity;
    private float targetLightIntensity;

    // Prepara materiales propios para no modificar el material compartido del proyecto.
    private void Awake()
    {
        CacheMaterialInstances();
        SetLit(startLit, true);
    }

    // Suaviza el cambio de brillo para que el bombillo se encienda gradualmente.
    private void Update()
    {
        float lerpAmount = Time.deltaTime * transitionSpeed;
        currentEmissionColor = Color.Lerp(currentEmissionColor, targetEmissionColor, lerpAmount);
        currentLightIntensity = Mathf.Lerp(currentLightIntensity, targetLightIntensity, lerpAmount);
        ApplyEmissionToMaterials();
        ApplyLightValues();
    }

    // Enciende o apaga este bombillo sin desactivar el objeto.
    public void SetLit(bool isLit)
    {
        SetLit(isLit, false);
    }

    // Cambia el estado del bombillo y permite aplicarlo de inmediato al iniciar.
    public void SetLit(bool isLit, bool instant)
    {
        Color emissionColor = isLit ? litEmissionColor : offEmissionColor;
        float emissionIntensity = isLit ? litEmissionIntensity : offEmissionIntensity;

        targetEmissionColor = emissionColor * Mathf.Max(0f, emissionIntensity);
        targetLightColor = emissionColor;
        targetLightIntensity = isLit ? litLightIntensity : offLightIntensity;

        if (!instant)
        {
            return;
        }

        currentEmissionColor = targetEmissionColor;
        currentLightIntensity = targetLightIntensity;
        ApplyEmissionToMaterials();
        ApplyLightValues();
    }

    // Busca los renderers asignados o, si no hay, usa los hijos del objeto.
    private void CacheMaterialInstances()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>();
        }

        List<Material> materials = new List<Material>();

        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            Material[] rendererMaterials = targetRenderer.materials;

            foreach (Material material in rendererMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                material.EnableKeyword("_EMISSION");
                materials.Add(material);
            }
        }

        materialInstances = materials.ToArray();
    }

    // Aplica el color de emission a todos los materiales controlados.
    private void ApplyEmissionToMaterials()
    {
        if (materialInstances == null)
        {
            return;
        }

        foreach (Material material in materialInstances)
        {
            if (material == null || !material.HasProperty(EmissionColorId))
            {
                continue;
            }

            material.SetColor(EmissionColorId, currentEmissionColor);
        }
    }

    // Aplica color e intensidad a las luces reales opcionales.
    private void ApplyLightValues()
    {
        if (targetLights == null)
        {
            return;
        }

        foreach (Light targetLight in targetLights)
        {
            if (targetLight == null)
            {
                continue;
            }

            targetLight.color = targetLightColor;
            targetLight.intensity = currentLightIntensity;
        }
    }
}