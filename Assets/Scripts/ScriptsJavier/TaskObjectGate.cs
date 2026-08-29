using UnityEngine;

/// <summary>
/// Componente generico y no invasivo: bloquea el/los Collider(s) de un
/// objeto de tarea mientras esa tarea NO sea la actual en el TaskManager,
/// y los reactiva cuando le toca. Al resolverse la tarea, se vuelven a
/// bloquear permanentemente para esa ronda.
///
/// No requiere modificar el script de interaccion existente del objeto
/// (BotonGeneradorInteractable, o cualquier otro que ya tengan hecho):
/// solo se agrega como componente adicional en el mismo GameObject (o
/// en el que tenga el Collider relevante).
/// </summary>
public class TaskObjectGate : MonoBehaviour
{
    [Header("Debe coincidir EXACTO con el Id de esta tarea en el TaskManager")]
    [SerializeField] private string taskId;

    [SerializeField] private TaskManager taskManager;

    [Header("Colliders a bloquear/desbloquear")]
    [SerializeField] private Collider[] collidersAGatear;

    private void Awake()
    {
        // Empieza bloqueado: nadie puede interactuar hasta que le toque.
        SetCollidersHabilitados(false);
    }

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.AddListener(OnNewTaskShown);
            taskManager.onTaskResolved.AddListener(OnTaskResolved);
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.RemoveListener(OnNewTaskShown);
            taskManager.onTaskResolved.RemoveListener(OnTaskResolved);
        }
    }

    private void OnNewTaskShown(TaskData task)
    {
        SetCollidersHabilitados(task.Id == taskId);
    }

    private void OnTaskResolved(TaskData task, DecisionType decision)
    {
        if (task.Id == taskId)
        {
            // Se resolvio esta tarea (autorizada o rechazada): se bloquea
            // de nuevo, sin importar la decision tomada.
            SetCollidersHabilitados(false);
        }
    }

    private void SetCollidersHabilitados(bool habilitados)
    {
        if (collidersAGatear == null)
        {
            return;
        }

        foreach (Collider collider in collidersAGatear)
        {
            if (collider != null)
            {
                collider.enabled = habilitados;
            }
        }
    }
}