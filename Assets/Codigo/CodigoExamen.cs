using UnityEngine;

public class codigoExamen : MonoBehaviour
{
    public float velocidad = 2f; // la velocidad a la que van los exámenes

    [Header("Balanceo")]
    public float inclinacionMaxima = 7f;
    public float velocidadBalanceo = 3f;

    private float tiempoBalanceo = 0f;

    void Update()
    {
        transform.Translate(Vector3.right * velocidad * Time.deltaTime, Space.World);
    }

    void LateUpdate()
    {
        tiempoBalanceo += Time.deltaTime;
        float anguloActual = inclinacionMaxima * Mathf.Sin(tiempoBalanceo * velocidadBalanceo);
        transform.rotation = Quaternion.Euler(0, 0, anguloActual);
    }
}