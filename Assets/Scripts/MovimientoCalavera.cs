using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoCalavera : MonoBehaviour
{
    [SerializeField] private float movimientoHorizontal = 0.12f;
    [SerializeField] private float movimientoVertical = 0.08f;
    [SerializeField] private float inclinacion = 3f;
    [SerializeField] private float suavizado = 5f;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private void Awake()
    {
        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;
    }

    private void LateUpdate()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 posicionMouse = Mouse.current.position.ReadValue();
        Vector2 cursorNormalizado = new Vector2(
            posicionMouse.x / Screen.width,
            posicionMouse.y / Screen.height);
        cursorNormalizado = cursorNormalizado * 2f - Vector2.one;

        Vector3 desplazamiento = new Vector3(
            cursorNormalizado.x * movimientoHorizontal,
            cursorNormalizado.y * movimientoVertical,
            0f);
        Vector3 posicionObjetivo = posicionInicial + desplazamiento;

        Quaternion rotacionObjetivo = rotacionInicial * Quaternion.Euler(
            -cursorNormalizado.y * inclinacion,
            cursorNormalizado.x * inclinacion,
            -cursorNormalizado.x * inclinacion);

        float pasoSuave = 1f - Mathf.Exp(-suavizado * Time.unscaledDeltaTime);
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            posicionObjetivo,
            pasoSuave);
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            rotacionObjetivo,
            pasoSuave);
    }
}
