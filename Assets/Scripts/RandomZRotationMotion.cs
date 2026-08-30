using UnityEngine;

public class RandomZRotationMotion : MonoBehaviour
{
    [Header("Rotation Range")]
    [SerializeField] private float fixedYAngle = 90f;
    [SerializeField] private float minZAngle = 0f;
    [SerializeField] private float maxZAngle = 90f;

    [Header("Motion")]
    [SerializeField] private float minSpeed = 0.6f;
    [SerializeField] private float maxSpeed = 1.4f;
    [SerializeField] private bool startAtRandomPoint = true;

    private float currentSpeed;
    private float motionTime;
    private int lastDirection;

    // Prepara un punto inicial y una velocidad aleatoria.
    private void Awake()
    {
        currentSpeed = GetRandomSpeed();

        if (startAtRandomPoint)
        {
            motionTime = Random.Range(0f, 1f);
        }
    }

    // Mueve constantemente la rotacion local en Z y mantiene Y fijo.
    private void Update()
    {
        motionTime += Time.deltaTime * currentSpeed;

        float pingPong = Mathf.PingPong(motionTime, 1f);
        float zAngle = Mathf.Lerp(minZAngle, maxZAngle, pingPong);

        transform.localRotation = Quaternion.Euler(0f, fixedYAngle, zAngle);
        ChangeSpeedWhenDirectionChanges(pingPong);
    }

    // Cambia la velocidad cuando el movimiento llega a un extremo y se devuelve.
    private void ChangeSpeedWhenDirectionChanges(float pingPong)
    {
        int direction = pingPong >= 0.99f ? 1 : pingPong <= 0.01f ? -1 : lastDirection;

        if (direction != lastDirection)
        {
            lastDirection = direction;
            currentSpeed = GetRandomSpeed();
        }
    }

    // Devuelve una velocidad aleatoria dentro del rango configurado.
    private float GetRandomSpeed()
    {
        return Random.Range(minSpeed, maxSpeed);
    }
}