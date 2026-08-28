using UnityEngine;
using UnityEngine.InputSystem;

public class TelefonoRojo : MonoBehaviour
{
    public AudioSource audioEstatica;
    public Animator anim;

    private NIS inputActions;

    [SerializeField] private TaskManager taskManager;

    private void Awake()
    {
        inputActions = new NIS();
    }


    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Interactuar.started += AbrirTelefono;
        inputActions.Player.Interactuar.canceled += CerrarTelefono;
    }

    private void OnDisable()
    {
        inputActions.Player.Interactuar.started -= AbrirTelefono;
        inputActions.Player.Interactuar.canceled -= CerrarTelefono;
        inputActions.Disable();
    }

    private void AbrirTelefono(InputAction.CallbackContext context)
    {
        anim.SetBool("isUp", true);
    }


    private void CerrarTelefono(InputAction.CallbackContext context)
    {
        anim.SetBool("isUp", false);
    }

    private void Update()
    {
        if (taskManager == null || audioEstatica == null)
        {
            return;
        }

        TaskData currentTask = taskManager.CurrentTask;

        if (currentTask != null && currentTask.Type == TaskType.Legitimate && anim.GetBool("isUp"))
        {
            if (!audioEstatica.isPlaying)
            {
                audioEstatica.Play();
            }
        }
        else if (audioEstatica.isPlaying)
        {
            audioEstatica.Stop();
        }
    }
    
}
