using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class AcercarHoja : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactionText = "E - Abrir hoja";
    [SerializeField] private string interactableName = "Hoja";
    [SerializeField] private bool canInteract = true;

    [Header("Canvas")]
    [SerializeField] private GameObject hojaCanvas;
    [SerializeField] private TMP_Text taskInfoText;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private bool startHidden = true;

    [SerializeField] private AudioSource HojaAudioSource;

    private bool canvasVisible;

    private void Awake()
    {
        if (taskInfoText == null && hojaCanvas != null)
        {
            taskInfoText = hojaCanvas.GetComponentInChildren<TMP_Text>(true);
        }

        if (startHidden && hojaCanvas != null)
        {
            hojaCanvas.SetActive(false);
        }

        canvasVisible = hojaCanvas != null && hojaCanvas.activeSelf;
        RefreshTaskInfo();
    }

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.AddListener(HandleNewTaskShown);
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.RemoveListener(HandleNewTaskShown);
        }
    }

    private void Update()
    {
        if (!canvasVisible || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

        HideCanvas();
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
        if (!canInteract)
        {
            return;
        }

        Debug.Log("Interactuando con: " + interactableName);

        if (HojaAudioSource != null)
        {
            HojaAudioSource.Play();
        }

        if (canvasVisible)
        {
            HideCanvas();
        }
        else
        {
            ShowCanvas();
        }
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;

        if (!canInteract)
        {
            HideCanvas();
        }
    }

    private void ShowCanvas()
    {
        if (hojaCanvas == null)
        {
            Debug.LogWarning("AcercarHoja: no hay canvas asignado.");
            return;
        }

        RefreshTaskInfo();
        hojaCanvas.SetActive(true);
        canvasVisible = true;
    }

    private void HideCanvas()
    {
        if (hojaCanvas != null)
        {
            hojaCanvas.SetActive(false);
        }

        canvasVisible = false;
    }

    private void RefreshTaskInfo()
    {
        if (taskInfoText == null)
        {
            return;
        }

        if (taskManager == null || taskManager.CurrentTask == null)
        {
            taskInfoText.text = "No hay tarea actual.";
            return;
        }

        taskInfoText.text = taskManager.CurrentTask.Message;
    }

    private void HandleNewTaskShown(TaskData task)
    {
        if (!canvasVisible)
        {
            return;
        }

        RefreshTaskInfo();
    }
}
