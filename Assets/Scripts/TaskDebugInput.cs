using UnityEngine;
using UnityEngine.InputSystem;

public class TaskDebugInput : MonoBehaviour
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private KeyCode startRunKey = KeyCode.R;
    [SerializeField] private KeyCode nextTaskKey = KeyCode.N;
    [SerializeField] private KeyCode authorizeKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode rejectKey = KeyCode.Alpha2;

    private void Update()
    {
        if (taskManager == null)
        {
            return;
        }

        if (WasPressed(startRunKey))
        {
            taskManager.StartRun();
        }

        if (WasPressed(nextTaskKey))
        {
            taskManager.ShowNextTask();
        }

        if (WasPressed(authorizeKey))
        {
            taskManager.ResolveCurrentTask(DecisionType.Authorize);
        }

        if (WasPressed(rejectKey))
        {
            taskManager.ResolveCurrentTask(DecisionType.Reject);
        }
    }

    private bool WasPressed(KeyCode keyCode)
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        switch (keyCode)
        {
            case KeyCode.R:
                return Keyboard.current.rKey.wasPressedThisFrame;
            case KeyCode.N:
                return Keyboard.current.nKey.wasPressedThisFrame;
            case KeyCode.Alpha1:
                return Keyboard.current.digit1Key.wasPressedThisFrame;
            case KeyCode.Alpha2:
                return Keyboard.current.digit2Key.wasPressedThisFrame;
            default:
                Debug.LogWarning("TaskDebugInput solo soporta R, N, Alpha1 y Alpha2 con el Input System actual.");
                return false;
        }
    }
}
