using UnityEngine;

public class codigoExamen : MonoBehaviour
{
    public float velocidad = 2f; 

    [Header("Balanceo")]
    public float inclinacionMaxima = 7f;
    public float velocidadBalanceo = 3f;
    private float tiempoBalanceo = 0f;
    public float distanciaRecorrida = 0f; // distancia total recorrida por el examen

    void Update()
    {
        // Calculamos cuánto se va a mover este frame
        Vector3 movimiento = Vector3.right * velocidad * Time.deltaTime;
        
        // Lo movemos
        transform.Translate(movimiento, Space.World);
        
        // Sumamos lo que se ha movido al progreso total
        distanciaRecorrida += movimiento.magnitude;
    }

    void LateUpdate()
    {
        tiempoBalanceo += Time.deltaTime;
        float anguloActual = inclinacionMaxima * Mathf.Sin(tiempoBalanceo * velocidadBalanceo);
        transform.rotation = Quaternion.Euler(0, 0, anguloActual);
    }
}