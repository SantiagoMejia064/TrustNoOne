using UnityEngine;


public class RadioJLV : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private AudioSource audioRadio; // opcional: estatica/mensajes

    [Header("Estado")]
    [SerializeField] private bool conectada = true;
    [SerializeField] private int tareasRestantesApagada = 0;

    public bool Conectada => conectada;

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.TaskResolved += HandleTaskResolved;
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskResolved -= HandleTaskResolved;
        }
    }

    // Llamado por RadioSwitchJLV cuando el jugador desconecta
    // fisicamente el receptor (caso 11).
    public void Desconectar(int duracionEnTareas)
    {
        conectada = false;
        tareasRestantesApagada = Mathf.Max(1, duracionEnTareas);

        if (audioRadio != null)
        {
            audioRadio.Stop();
        }

        Debug.Log("[Radio] Desconectada durante " + tareasRestantesApagada + " tarea(s).");
    }

    // Cada vez que se RESUELVE una tarea (cualquiera, no solo la
    // del caso 11), se descuenta una del contador. Cuando llega a
    // 0, la radio se reconecta sola.
    private void HandleTaskResolved(TaskData task, bool wasCorrect)
    {
        if (conectada || tareasRestantesApagada <= 0)
        {
            return;
        }

        tareasRestantesApagada--;

        if (tareasRestantesApagada <= 0)
        {
            Reconectar();
        }
    }

    private void Reconectar()
    {
        conectada = true;
        Debug.Log("[Radio] Reconectada automaticamente.");
        // TODO: aqui, quien construya el resto del sistema de radio
        // puede reanudar la reproduccion normal de mensajes.
    }
}