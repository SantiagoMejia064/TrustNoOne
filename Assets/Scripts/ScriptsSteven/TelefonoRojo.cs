using UnityEngine;
using UnityEngine.InputSystem;

public class TelefonoRojo : MonoBehaviour
{
    public AudioSource audioEstatica;
    public Animator anim;

    private NIS inputActions;

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
        //inputActions.Player.Interactuar.started -= AbrirTelefono;
        inputActions.Player.Interactuar.canceled -= CerrarTelefono;
        inputActions.Disable();
    }

    private void AbrirTelefono(InputAction.CallbackContext context)
    {
        anim.SetBool("isUp", true);
        audioEstatica.Play();
    }

    private void CerrarTelefono(InputAction.CallbackContext context)
    {
        anim.SetBool("isUp", false);
        audioEstatica.Stop();
    }

    
}
