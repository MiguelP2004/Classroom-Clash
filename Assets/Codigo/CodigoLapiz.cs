using UnityEngine;

public class CodigoLapiz : MonoBehaviour
{
    [Header("Estadísticas del Lápiz")]
    public float velocidad = 10f;
    public float daño = 1f; 
    private float tiempoDeVida = 3f;
    public GameObject prefabImpacto;
    
    private Transform objetivo;
    private codigoExamen scriptObjetivo;

    void Start()
    {
        transform.Rotate(0, 0, -90f);
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        if (objetivo != null && scriptObjetivo != null)
        {
            if (scriptObjetivo.vida > 0)
            {
                Vector3 direccion = objetivo.position - transform.position;
                float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
                
                transform.rotation = Quaternion.Euler(0, 0, angulo - 90f);
            }
            else
            {
                objetivo = null;
            }
        }
        
        transform.Translate(Vector3.up * velocidad * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Examen"))
        {
            codigoExamen enemigo = otro.GetComponent<codigoExamen>();
            
            if (enemigo != null && enemigo.vida > 0)
            {
                enemigo.RecibirImpacto(daño); 
                
                if (prefabImpacto != null)
                {
                    Instantiate(prefabImpacto, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }

    public void AsignarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        
        if (objetivo != null)
        {
            scriptObjetivo = objetivo.GetComponent<codigoExamen>();
        }
    }
}