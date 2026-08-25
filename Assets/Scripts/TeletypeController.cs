using TMPro;
using UnityEngine;

public class TeletypeController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI verifyText;

    public void ShowTask(TaskData task)
    {
        if (task == null)
        {
            Debug.LogWarning("No se puede mostrar una tarea nula en el teletipo.");
            Clear();
            return;
        }

        if (titleText != null)
        {
            titleText.text = task.title;
        }
        else
        {
            Debug.LogWarning("Falta asignar titleText en TeletypeController.");
        }

        if (messageText != null)
        {
            messageText.text = task.message;
        }
        else
        {
            Debug.LogWarning("Falta asignar messageText en TeletypeController.");
        }

        if (verifyText != null)
        {
            verifyText.text = task.howToVerify;
        }
        else
        {
            Debug.LogWarning("Falta asignar verifyText en TeletypeController.");
        }
    }

    public void Clear()
    {
        if (titleText != null)
        {
            titleText.text = string.Empty;
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
        }

        if (verifyText != null)
        {
            verifyText.text = string.Empty;
        }
    }
}
