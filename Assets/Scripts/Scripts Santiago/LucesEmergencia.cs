using UnityEngine;
using System.Collections;

public class LucesEmergencia : MonoBehaviour, IInteractable
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string taskId = "Tarea2";
    [SerializeField] private string interactionText = "E - Emergency lights";
    [SerializeField] private string interactableName = "Emergency lights button";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;
    [SerializeField] private Light[] lucesParaPrender;
    [SerializeField] private Light[] lucesParaApagar;

    private Coroutine parpadeoCoroutine;
    private bool[] lucesOriginalmenteEncendidas;

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.AddListener(OnNewTaskShown);
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.RemoveListener(OnNewTaskShown);
        }

        DetenerParpadeo();
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

        RestaurarLucesParpadeando();

        if (botonAutorizar != null)
        {
            botonAutorizar.Activar();
        }

        if (botonRechazar != null)
        {
            botonRechazar.Desactivar();
        }
    }

    private void OnNewTaskShown(TaskData task)
    {
        if (task == null || task.Id != taskId)
        {
            DetenerParpadeo();
            return;
        }

        IniciarParpadeo();
    }

    private void IniciarParpadeo()
    {
        if (lucesParaApagar == null || lucesParaApagar.Length == 0)
        {
            return;
        }

        lucesOriginalmenteEncendidas = new bool[lucesParaApagar.Length];

        for (int i = 0; i < lucesParaApagar.Length; i++)
        {
            Light luz = lucesParaApagar[i];
            lucesOriginalmenteEncendidas[i] = luz != null
                && luz.gameObject.activeSelf
                && luz.enabled;
        }

        DetenerParpadeo();
        parpadeoCoroutine = StartCoroutine(ParpadearLuces());
    }

    private IEnumerator ParpadearLuces()
    {
        while (true)
        {
            CambiarEstadoLucesParpadeo(false);
            yield return new WaitForSeconds(0.4f);
            CambiarEstadoLucesParpadeo(true);
            yield return new WaitForSeconds(0.4f);
        }
    }

    private void CambiarEstadoLucesParpadeo(bool encender)
    {
        if (lucesParaApagar == null || lucesOriginalmenteEncendidas == null)
        {
            return;
        }

        for (int i = 0; i < lucesParaApagar.Length; i++)
        {
            Light luz = lucesParaApagar[i];

            if (luz == null || !lucesOriginalmenteEncendidas[i])
            {
                continue;
            }

            luz.gameObject.SetActive(encender);
            luz.enabled = encender;
        }
    }

    private void RestaurarLucesParpadeando()
    {
        DetenerParpadeo();
        foreach (Light luz in lucesParaPrender)
        {
            if (luz != null)
            {
                luz.gameObject.SetActive(true);
                luz.enabled = true;
            }
        }
        if (lucesParaApagar == null || lucesOriginalmenteEncendidas == null)
        {
            return;
        }

        for (int i = 0; i < lucesParaApagar.Length; i++)
        {
            Light luz = lucesParaApagar[i];

            if (luz == null)
            {
                continue;
            }

            bool estabaEncendida = lucesOriginalmenteEncendidas[i];
            luz.gameObject.SetActive(estabaEncendida);
            luz.enabled = estabaEncendida;
        }
    }

    private void DetenerParpadeo()
    {
        if (parpadeoCoroutine != null)
        {
            StopCoroutine(parpadeoCoroutine);
            parpadeoCoroutine = null;
        }
    }

    public void consecuencias()
    {
        foreach (Light luz in lucesParaApagar)
        {
            if (luz != null)
            {
                luz.enabled = false;
                luz.gameObject.SetActive(false);
            }
        }

        RenderSettings.ambientIntensity = 0f;
    }
}
