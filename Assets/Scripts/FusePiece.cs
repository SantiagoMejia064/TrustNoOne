using System.Collections;
using UnityEngine;

public enum FusePieceType
{
    OldFuse,
    ReplacementFuse
}

public class FusePiece : MonoBehaviour
{
    [SerializeField] private FusePieceType pieceType;
    [SerializeField] private Rigidbody fuseRigidbody;
    [SerializeField] private bool disablePhysicsOnStart = true;

    private Coroutine disappearRoutine;

    public FusePieceType PieceType => pieceType;

    // Busca el Rigidbody y deja el fusible fijo si debe empezar sin fisicas.
    private void Awake()
    {
        if (fuseRigidbody == null)
        {
            TryGetComponent(out fuseRigidbody);
        }

        if (disablePhysicsOnStart)
        {
            SetPhysicsEnabled(false);
        }
    }

    // Restaura el fusible para repetir la tarea desde su posicion inicial.
    public void ResetPiece()
    {
        StopDisappearRoutine();
        gameObject.SetActive(true);
        SetPhysicsEnabled(false);
    }

    // Activa o desactiva la fisica segun si el fusible debe caer o quedar fijo.
    public void SetPhysicsEnabled(bool enabled)
    {
        Rigidbody rigidbodyToUse = GetOrCreateRigidbody();

        if (rigidbodyToUse == null)
        {
            return;
        }

        if (enabled)
        {
            rigidbodyToUse.isKinematic = false;
            rigidbodyToUse.useGravity = true;
            return;
        }

        if (!rigidbodyToUse.isKinematic)
        {
            rigidbodyToUse.linearVelocity = Vector3.zero;
            rigidbodyToUse.angularVelocity = Vector3.zero;
        }

        rigidbodyToUse.useGravity = false;
        rigidbodyToUse.isKinematic = true;
    }

    // Lanza el fusible con fisica y programa que desaparezca despues.
    public void Throw(Vector3 velocity, float torque, float disappearDelay)
    {
        StopDisappearRoutine();
        gameObject.SetActive(true);
        SetPhysicsEnabled(true);

        Rigidbody rigidbodyToUse = GetOrCreateRigidbody();

        if (rigidbodyToUse != null)
        {
            rigidbodyToUse.linearVelocity = velocity;
            rigidbodyToUse.angularVelocity = Random.onUnitSphere * torque;
        }

        if (disappearDelay > 0f)
        {
            disappearRoutine = StartCoroutine(DisappearAfterDelay(disappearDelay));
        }
    }

    // Devuelve un Rigidbody existente o crea uno para que el fusible pueda usar fisicas.
    private Rigidbody GetOrCreateRigidbody()
    {
        if (fuseRigidbody != null)
        {
            return fuseRigidbody;
        }

        if (!TryGetComponent(out fuseRigidbody))
        {
            fuseRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        return fuseRigidbody;
    }

    // Espera unos segundos y oculta el fusible viejo ya retirado.
    private IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
        disappearRoutine = null;
    }

    // Cancela la desaparicion si la tarea se reinicia o el fusible se restaura.
    private void StopDisappearRoutine()
    {
        if (disappearRoutine == null)
        {
            return;
        }

        StopCoroutine(disappearRoutine);
        disappearRoutine = null;
    }
}
