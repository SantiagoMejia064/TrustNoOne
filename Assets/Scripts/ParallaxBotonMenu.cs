using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ParallaxBotonMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float movimiento = 28f;
    [SerializeField] private float inclinacion = 8f;
    [SerializeField] private float escalaHover = 1.08f;
    [SerializeField] private float velocidad = 12f;
    [SerializeField] private float velocidadFlotacion = 2f;
    [SerializeField] private float amplitudFlotacion = 2f;

    private RectTransform rectTransform;
    private Vector2 posicionInicial;
    private Quaternion rotacionInicial;
    private Vector3 escalaInicial;
    private bool cursorEncima;
    private float faseFlotacion;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        posicionInicial = rectTransform.anchoredPosition;
        rotacionInicial = rectTransform.localRotation;
        escalaInicial = rectTransform.localScale;
        faseFlotacion = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (Mouse.current == null || Screen.width == 0 || Screen.height == 0)
        {
            return;
        }

        Vector2 posicionMouse = Mouse.current.position.ReadValue();
        Vector2 cursorNormalizado = new Vector2(
            posicionMouse.x / Screen.width,
            posicionMouse.y / Screen.height);
        cursorNormalizado = cursorNormalizado * 2f - Vector2.one;

        float flotacion = Mathf.Sin(Time.unscaledTime * velocidadFlotacion + faseFlotacion) * amplitudFlotacion;
        Vector2 objetivoPosicion = posicionInicial + cursorNormalizado * movimiento;
        objetivoPosicion.y += flotacion;

        Quaternion objetivoRotacion = rotacionInicial * Quaternion.Euler(
            -cursorNormalizado.y * inclinacion,
            cursorNormalizado.x * inclinacion,
            -cursorNormalizado.x * inclinacion);
        Vector3 objetivoEscala = escalaInicial * (cursorEncima ? escalaHover : 1f);
        float paso = 1f - Mathf.Exp(-velocidad * Time.unscaledDeltaTime);

        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, objetivoPosicion, paso);
        rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, objetivoRotacion, paso);
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, objetivoEscala, paso);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cursorEncima = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cursorEncima = false;
    }
}
