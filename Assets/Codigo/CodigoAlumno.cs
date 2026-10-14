using UnityEngine;
using UnityEngine.InputSystem; 

public class CodigoAlumno : MonoBehaviour
{
    [Header("Estadísticas del Alumno")]
    public float rangoAtaque = 3f;
    public float velocidadGiro = 6f;

    [HideInInspector] public bool estaColocado = false; 
    [HideInInspector] public bool seleccionado = false;

    private bool listoParaSeleccionar = false;

    private Transform objetivoActual;

    void Start() { }

    void Update()
    {
        // Si no está colocado, no hacemos nada de esto
        if (!estaColocado) return;

        if (!listoParaSeleccionar)
        {
            // Esperamos a que el jugador suelte el clic de colocar
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                listoParaSeleccionar = true;
            }
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 posicionRaton = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Collider2D tocado = Physics2D.OverlapPoint(posicionRaton);

                if (tocado != null && tocado.transform == this.transform)
                {
                    seleccionado = true;
                }
                else
                {
                    seleccionado = false;
                }
            }
        }

        BuscarObjetivo();

        if (objetivoActual != null)
        {
            ApuntarSmooth();
        }
    }

    void BuscarObjetivo()
    {
        Collider2D[] detectados = Physics2D.OverlapCircleAll(transform.position, rangoAtaque);
        
        float mayorProgreso = -1f;
        Transform mejorObjetivo = null;

        foreach (Collider2D col in detectados)
        {
            codigoExamen enemigo = col.GetComponent<codigoExamen>();
            
            if (enemigo != null)
            {
                if (enemigo.distanciaRecorrida > mayorProgreso)
                {
                    mayorProgreso = enemigo.distanciaRecorrida;
                    mejorObjetivo = enemigo.transform;
                }
            }
        }
        objetivoActual = mejorObjetivo;
    }

    void ApuntarSmooth()
    {
        Vector3 direccion = objetivoActual.position - transform.position;
        float anguloObjetivo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        float anguloActual = transform.eulerAngles.z;

        float diferencia = Mathf.DeltaAngle(anguloActual, anguloObjetivo);

        float nuevoAngulo = anguloActual + (diferencia * velocidadGiro * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, 0, nuevoAngulo);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}