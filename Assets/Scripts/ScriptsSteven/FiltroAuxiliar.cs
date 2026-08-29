using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FiltroAuxiliar : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "E - Auxiliar filter";
    [SerializeField] private string interactableName = "Generator button";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;
    [Header("Audio tos")]
    [SerializeField] private AudioSource tosAudioSource;
    [SerializeField, Min(0f)] private float delayInicialTos = 2f;
    [SerializeField, Min(0f)] private float esperaMinimaEntreTos = 3f;
    [SerializeField, Min(0f)] private float esperaMaximaEntreTos = 6f;
    [Header("Lens Distortion")]
    [SerializeField] private Volume globalVolume;
    [SerializeField, Range(0f, 0.3f)] private float intensidadMareo = 0.12f;
    [SerializeField, Range(1f, 1.15f)] private float escalaMareo = 1.03f;
    [SerializeField, Min(0f)] private float desplazamientoCentroX = 0.035f;
    [SerializeField, Min(0f)] private float desplazamientoCentroY = 0.03f;
    [SerializeField, Min(0f)] private float derivaCentro = 0.01f;
    [SerializeField, Min(0f)] private float velocidadMareo = 1.5f;

    [SerializeField] private AudioSource UIButton;

    public GameObject Ox1;
    public GameObject Ox2;

    private Coroutine tosRoutine;
    private VolumeProfile globalVolumeProfile;
    private LensDistortion lensDistortion;
    private bool distortionSnapshotTomado;
    private bool distortionOriginalActive;
    private bool distortionOriginalCenterOverride;
    private bool distortionOriginalIntensityOverride;
    private bool distortionOriginalScaleOverride;
    private bool distortionOriginalXOverride;
    private bool distortionOriginalYOverride;
    private Vector2 distortionOriginalCenter;
    private float distortionOriginalIntensity;
    private float distortionOriginalScale;
    private float distortionOriginalXMultiplier;
    private float distortionOriginalYMultiplier;
    private float noiseSeedX;
    private float noiseSeedY;
    private float noiseSeedIntensidad;

    private void Awake()
    {
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);
        noiseSeedIntensidad = Random.Range(0f, 1000f);

        if (globalVolume == null)
        {
            globalVolume = FindFirstObjectByType<Volume>();
        }
    }

    private void Start()
    {
        if (Ox1 != null)
        {
            Ox1.SetActive(false);
        }

        if (Ox2 != null)
        {
            Ox2.SetActive(false);
        }

        if (tosAudioSource != null)
        {
            tosAudioSource.playOnAwake = false;
            tosAudioSource.loop = false;
        }
    }

    private void Update()
    {
        if (EstanActivosOxigeno())
        {
            IniciarTosSiCorresponde();
            ActualizarDistorsion();
        }
        else
        {
            DetenerTos();
            RestaurarDistorsion();
        }
    }

    private void OnDisable()
    {
        DetenerTos();
        RestaurarDistorsion();
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void Interact()
    {
        Debug.Log("Interactuando con: " + interactableName);



        if (botonAutorizar != null)
        {
            botonAutorizar.Activar();
        }

        UIButton.Play();

        if (botonRechazar != null)
        {
            botonRechazar.Desactivar();
        }
    }

    public void ActivarGases()
    {
        if (Ox2 != null)
        {
            Ox2.SetActive(true);
        }

        if (Ox1 != null)
        {
            Ox1.SetActive(true);
        }

        IniciarTosSiCorresponde();
        ActualizarDistorsion();
    }

    private bool EstanActivosOxigeno()
    {
        return Ox1 != null && Ox2 != null && Ox1.activeSelf && Ox2.activeSelf;
    }

    private void IniciarTosSiCorresponde()
    {
        if (tosRoutine != null || !EstanActivosOxigeno() || tosAudioSource == null || tosAudioSource.clip == null)
        {
            return;
        }

        tosRoutine = StartCoroutine(ReproducirTos());
    }

    private void DetenerTos()
    {
        if (tosRoutine != null)
        {
            StopCoroutine(tosRoutine);
            tosRoutine = null;
        }

        if (tosAudioSource != null && tosAudioSource.isPlaying)
        {
            tosAudioSource.Stop();
        }
    }

    private IEnumerator ReproducirTos()
    {
        yield return new WaitForSeconds(delayInicialTos);

        while (EstanActivosOxigeno())
        {
            if (tosAudioSource == null || tosAudioSource.clip == null)
            {
                break;
            }

            tosAudioSource.Stop();
            tosAudioSource.Play();
            yield return new WaitForSeconds(tosAudioSource.clip.length);

            if (!EstanActivosOxigeno())
            {
                break;
            }

            float esperaRandom = Random.Range(
                Mathf.Min(esperaMinimaEntreTos, esperaMaximaEntreTos),
                Mathf.Max(esperaMinimaEntreTos, esperaMaximaEntreTos));
            yield return new WaitForSeconds(esperaRandom);
        }

        tosRoutine = null;
    }

    private bool PrepararDistorsion()
    {
        if (globalVolume == null)
        {
            return false;
        }

        globalVolumeProfile = globalVolume.profile;
        if (globalVolumeProfile == null)
        {
            return false;
        }

        if (!globalVolumeProfile.TryGet(out lensDistortion))
        {
            lensDistortion = globalVolumeProfile.Add<LensDistortion>(false);
        }

        if (!distortionSnapshotTomado && lensDistortion != null)
        {
            distortionSnapshotTomado = true;
            distortionOriginalActive = lensDistortion.active;
            distortionOriginalCenterOverride = lensDistortion.center.overrideState;
            distortionOriginalIntensityOverride = lensDistortion.intensity.overrideState;
            distortionOriginalScaleOverride = lensDistortion.scale.overrideState;
            distortionOriginalXOverride = lensDistortion.xMultiplier.overrideState;
            distortionOriginalYOverride = lensDistortion.yMultiplier.overrideState;
            distortionOriginalCenter = lensDistortion.center.value;
            distortionOriginalIntensity = lensDistortion.intensity.value;
            distortionOriginalScale = lensDistortion.scale.value;
            distortionOriginalXMultiplier = lensDistortion.xMultiplier.value;
            distortionOriginalYMultiplier = lensDistortion.yMultiplier.value;
        }

        return lensDistortion != null;
    }

    private void ActualizarDistorsion()
    {
        if (!PrepararDistorsion())
        {
            return;
        }

        lensDistortion.active = true;
        lensDistortion.center.overrideState = true;
        lensDistortion.intensity.overrideState = true;
        lensDistortion.scale.overrideState = true;
        lensDistortion.xMultiplier.overrideState = true;
        lensDistortion.yMultiplier.overrideState = true;

        float t = Time.time * velocidadMareo;
        float driftX = Mathf.Sin(t * 0.85f) * desplazamientoCentroX;
        float driftY = Mathf.Cos(t * 0.72f + 1.3f) * desplazamientoCentroY;
        float noiseX = (Mathf.PerlinNoise(noiseSeedX, t) - 0.5f) * 2f * derivaCentro;
        float noiseY = (Mathf.PerlinNoise(noiseSeedY, t * 0.93f) - 0.5f) * 2f * derivaCentro;
        float pulsoIntensidad = Mathf.Lerp(0.9f, 1.12f, Mathf.PerlinNoise(noiseSeedIntensidad, t * 1.35f));

        lensDistortion.center.value = new Vector2(
            Mathf.Clamp01(distortionOriginalCenter.x + driftX + noiseX),
            Mathf.Clamp01(distortionOriginalCenter.y + driftY + noiseY));
        lensDistortion.intensity.value = Mathf.Max(distortionOriginalIntensity, intensidadMareo) * pulsoIntensidad;
        lensDistortion.scale.value = Mathf.Max(distortionOriginalScale, escalaMareo);
        lensDistortion.xMultiplier.value = distortionOriginalXMultiplier;
        lensDistortion.yMultiplier.value = distortionOriginalYMultiplier;
    }

    private void RestaurarDistorsion()
    {
        if (!distortionSnapshotTomado || lensDistortion == null)
        {
            return;
        }

        lensDistortion.active = distortionOriginalActive;
        lensDistortion.center.overrideState = distortionOriginalCenterOverride;
        lensDistortion.intensity.overrideState = distortionOriginalIntensityOverride;
        lensDistortion.scale.overrideState = distortionOriginalScaleOverride;
        lensDistortion.xMultiplier.overrideState = distortionOriginalXOverride;
        lensDistortion.yMultiplier.overrideState = distortionOriginalYOverride;
        lensDistortion.center.value = distortionOriginalCenter;
        lensDistortion.intensity.value = distortionOriginalIntensity;
        lensDistortion.scale.value = distortionOriginalScale;
        lensDistortion.xMultiplier.value = distortionOriginalXMultiplier;
        lensDistortion.yMultiplier.value = distortionOriginalYMultiplier;
    }
}
