using UnityEngine;

public class FiltroAuxiliar : MonoBehaviour
{
    [SerializeField] private string interactionText = "E - Auxiliar filter";
    [SerializeField] private string interactableName = "Generator button";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

    public GameObject Ox1;
    public GameObject Ox2;

    void Start()
    {
        if (Ox1 != null)
        {
            Ox1.SetActive(false);
        }

        if (Ox2 != null)
        {
            Ox2.SetActive(false);
        }
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



        if (botonAutorizar != null)
        {
            botonAutorizar.Activar();
        }

        if (botonRechazar != null)
        {
            botonRechazar.Desactivar();
        }
    }

    public void ActivarGases()
    {
        Ox2.SetActive(true);
        Ox1.SetActive(true);
    }
}
