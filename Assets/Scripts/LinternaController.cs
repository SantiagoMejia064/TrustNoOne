using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LinternaController : MonoBehaviour
{
    [SerializeField] private Light linterna;
    [SerializeField] private bool iniciarEncendida = false;
    [SerializeField] private float duracionTransicion = 0.12f;
    [SerializeField] private Animator animatorLinterna;
    [SerializeField] private string parametroAnimator = "isOn";
    [SerializeField] private TextMeshProUGUI textoLinterna;
    [SerializeField] private string textoEncender = "F - activar linterna";
    [SerializeField] private string textoApagar = "F - desactivar linterna";

    private NIS inputActions;
    private InputAction accionLinterna;
    private bool estaEncendida;
    private float intensidadObjetivo;
    private Coroutine transicionCoroutine;

    private void Awake()
    {
        inputActions = new NIS();
        accionLinterna = inputActions.FindAction("Linterna");

        if (linterna == null)
        {
            linterna = GetComponentInChildren<Light>();
        }

        if (linterna != null)
        {
            intensidadObjetivo = linterna.intensity;
        }
    }

    private void Start()
    {
        AplicarEstadoInicial();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        if (accionLinterna != null)
        {
            accionLinterna.performed += OnLinterna;
        }
    }

    private void OnDisable()
    {
        if (accionLinterna != null)
        {
            accionLinterna.performed -= OnLinterna;
        }

        inputActions.Disable();
    }

    public void AlternarLinterna()
    {
        CambiarEstado(!estaEncendida);
    }

    public void EncenderLinterna()
    {
        CambiarEstado(true);
    }

    public void ApagarLinterna()
    {
        CambiarEstado(false);
    }

    private void AplicarEstadoInicial()
    {
        estaEncendida = iniciarEncendida;
        ActualizarTextoLinterna();

        if (animatorLinterna != null)
        {
            animatorLinterna.SetBool(parametroAnimator, estaEncendida);
        }

        if (linterna == null)
        {
            return;
        }

        linterna.gameObject.SetActive(true);
        linterna.enabled = estaEncendida;
        linterna.intensity = estaEncendida ? intensidadObjetivo : 0f;
    }

    private void CambiarEstado(bool encender)
    {
        estaEncendida = encender;
        ActualizarTextoLinterna();

        if (animatorLinterna != null)
        {
            animatorLinterna.SetBool(parametroAnimator, estaEncendida);
        }

        if (linterna == null)
        {
            return;
        }

        if (transicionCoroutine != null)
        {
            StopCoroutine(transicionCoroutine);
        }

        transicionCoroutine = StartCoroutine(AnimarLinterna(encender));
    }

    private IEnumerator AnimarLinterna(bool encender)
    {
        linterna.gameObject.SetActive(true);

        if (encender)
        {
            linterna.enabled = true;
        }

        float intensidadInicial = linterna.intensity;
        float intensidadFinal = encender ? intensidadObjetivo : 0f;
        float elapsed = 0f;

        while (elapsed < duracionTransicion)
        {
            elapsed += Time.deltaTime;
            float t = duracionTransicion <= 0f ? 1f : Mathf.Clamp01(elapsed / duracionTransicion);
            linterna.intensity = Mathf.Lerp(intensidadInicial, intensidadFinal, t);
            yield return null;
        }

        linterna.intensity = intensidadFinal;
        linterna.enabled = encender;
        transicionCoroutine = null;
    }

    private void ActualizarTextoLinterna()
    {
        if (textoLinterna == null)
        {
            return;
        }

        textoLinterna.text = estaEncendida ? textoApagar : textoEncender;
    }

    private void OnLinterna(InputAction.CallbackContext context)
    {
        AlternarLinterna();
    }
}
