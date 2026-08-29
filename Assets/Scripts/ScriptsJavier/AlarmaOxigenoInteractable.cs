using System.Collections;
using UnityEngine;

/// <summary>
/// Objeto interactuable de la tarea legitima "Desactivar alarma de oxigeno".
/// Este objeto puede estar visible en la escena desde el inicio, pero NO
/// debe parpadear ni sonar hasta que TareaAlarmaOxigenoSetup llame a
/// Activar() al aparecer esta tarea.
///
/// Mientras esta "armada": parpadea (cambiando el color de su propio
/// material entre rojo oscuro y rojo claro) y suena. Al interactuar:
/// - Deja de parpadear y sonar, y el color queda fijo en rojo oscuro.
/// - Reproduce una pequena animacion de "pulsado".
/// - Habilita el boton Autorizar (antes desactivado por defecto).
/// - Deshabilita el boton Rechazar.
///
/// Si el jugador RECHAZA sin interactuar con esto, el parpadeo y el
/// sonido siguen para siempre, cumpliendo la regla de que la consecuencia
/// negativa se mantiene toda la partida.
/// </summary>
public class AlarmaOxigenoInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "E - Desactivar alarma";
    [SerializeField] private string interactableName = "Alarma de oxigeno";
    [SerializeField] private bool canInteract = false;

    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

    [Header("Parpadeo (color del propio material)")]
    [SerializeField] private Renderer alarmaRenderer;
    [SerializeField] private Color colorOscuro = new Color(0.25f, 0f, 0f);
    [SerializeField] private Color colorClaro = new Color(1f, 0.15f, 0.15f);
    [SerializeField] private string colorPropertyName = "_Color";
    [SerializeField] private float intervaloParpadeo = 0.4f;

    [Header("Sonido (opcional, se puede dejar vacio por ahora)")]
    [SerializeField] private AudioSource alarmaAudio;

    [Header("Animacion de pulsado")]
    [SerializeField] private Transform visualBoton;
    [SerializeField] private float escalaPulsado = 0.85f;
    [SerializeField] private float duracionPulsado = 0.15f;

    [SerializeField] private AudioSource UiBoton;

    private Coroutine parpadeoRoutine;
    private Vector3 escalaOriginal;
    private MaterialPropertyBlock propBlock;
    private bool colorEncendido;
    private bool activada;

    private void Awake()
    {
        if (visualBoton != null)
        {
            escalaOriginal = visualBoton.localScale;
        }

        propBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        // Empieza "desarmada": sin parpadear, sin sonar, sin poder interactuar.
        canInteract = false;
        AplicarColor(colorOscuro);
    }

    private void OnDisable()
    {
        DetenerParpadeo();
    }

    /// <summary>
    /// Llamado por TareaAlarmaOxigenoSetup cuando aparece esta tarea.
    /// Arma la alarma: empieza a parpadear, sonar, y ya se puede interactuar.
    /// </summary>
    public void Activar()
    {
        Debug.Log("AlarmaOxigenoInteractable.Activar() llamado");
        if (activada)
        {
            Debug.Log("Ya estaba activada, se ignora.");
            return;
        }

        activada = true;
        canInteract = true;
        IniciarParpadeo();
    }

    public void Bloquear()
    {
        canInteract = false;
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
        if (!activada)
        {
            return;
        }

        Debug.Log("Interactuando con: " + interactableName);

        DetenerParpadeo();

        if (visualBoton != null)
        {
            StartCoroutine(AnimarPulsado());
        }

        UiBoton.Play();

        if (botonAutorizar != null)
        {
            botonAutorizar.Activar();
        }

        if (botonRechazar != null)
        {
            botonRechazar.Desactivar();
        }

        // Ya se apago, no tiene sentido volver a interactuar en esta ronda.
        canInteract = false;
    }

    private void IniciarParpadeo()
    {
        DetenerParpadeo();
        parpadeoRoutine = StartCoroutine(Parpadear());

        if (alarmaAudio != null && !alarmaAudio.isPlaying)
        {
            alarmaAudio.loop = true;
            alarmaAudio.Play();
        }
    }

    private void DetenerParpadeo()
    {
        if (parpadeoRoutine != null)
        {
            StopCoroutine(parpadeoRoutine);
            parpadeoRoutine = null;
        }

        AplicarColor(colorOscuro);

        if (alarmaAudio != null)
        {
            alarmaAudio.Stop();
        }
    }

    private IEnumerator Parpadear()
    {
        while (true)
        {
            colorEncendido = !colorEncendido;
            AplicarColor(colorEncendido ? colorClaro : colorOscuro);

            yield return new WaitForSeconds(intervaloParpadeo);
        }
    }

    private void AplicarColor(Color color)
    {
        if (alarmaRenderer == null)
        {
            return;
        }

        alarmaRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(colorPropertyName, color);
        alarmaRenderer.SetPropertyBlock(propBlock);
    }

    private IEnumerator AnimarPulsado()
    {
        Vector3 objetivoPulsado = escalaOriginal * escalaPulsado;
        float mitad = duracionPulsado * 0.5f;
        float elapsed = 0f;

        while (elapsed < mitad)
        {
            elapsed += Time.deltaTime;
            visualBoton.localScale = Vector3.Lerp(escalaOriginal, objetivoPulsado, elapsed / mitad);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < mitad)
        {
            elapsed += Time.deltaTime;
            visualBoton.localScale = Vector3.Lerp(objetivoPulsado, escalaOriginal, elapsed / mitad);
            yield return null;
        }

        visualBoton.localScale = escalaOriginal;
    }

    
}