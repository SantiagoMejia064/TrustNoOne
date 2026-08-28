using UnityEngine;


public class TelefonoJLV : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Sonido de estatica que representa 'linea conectada'.")]
    [SerializeField] private AudioSource audioEstatica;

    [Header("Estado")]
    [SerializeField] private bool lineaActiva = false;

    public bool LineaActiva => lineaActiva;

    private void Start()
    {
        AplicarEstadoAudio();
    }

    public void SetLineaActiva(bool activa)
    {
        if (lineaActiva == activa)
        {
            return;
        }

        lineaActiva = activa;
        AplicarEstadoAudio();

        Debug.Log("[Telefono] Linea segura: " + (lineaActiva ? "ACTIVA (estatica)" : "PERDIDA (silencio)"));
    }

    private void AplicarEstadoAudio()
    {
        if (audioEstatica == null)
        {
            return;
        }

        if (lineaActiva)
        {
            audioEstatica.loop = true;
            audioEstatica.Play();
        }
        else
        {
            audioEstatica.Stop();
        }
    }
}