using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Necesario para la corrutina del retroceso

public class CodigoAlumno : MonoBehaviour
{
    [Header("Estadísticas del Alumno")]
    public float rangoAtaque = 3f;
    public float velocidadGiro = 6f;

    [Header("Disparo")]
    public GameObject proyectilPrefab;
    public float cadenciaFuego = 1.5f;

    private float fuerzaRetroceso = 0.1f;
    private float tiempoAtras = 0.1f;
    private float tiempoRecuperacion = 0.2f;

    [HideInInspector] public bool estaColocado = false; 
    [HideInInspector] public bool seleccionado = false;

    private bool listoParaSeleccionar = false;
    private bool inicializado = false; // Guarda la posición original al colocarlo

    private Transform objetivoActual;
    private float temporizadorDisparo;
    private Coroutine rutinaRetroceso;
    private Vector3 posicionOriginal;

    void Start() { }

    void Update()
    {
        if (!estaColocado) return;

        // Guardamos su posición final en el mapa nada más soltarlo para el retroceso
        if (!inicializado)
        {
            posicionOriginal = transform.position;
            temporizadorDisparo = cadenciaFuego; // Listo para disparar al instante
            inicializado = true;
        }

        if (!listoParaSeleccionar)
        {
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

            temporizadorDisparo += Time.deltaTime;
            if (temporizadorDisparo >= cadenciaFuego)
            {
                // 1. Calculamos el ángulo hacia el objetivo igual que al apuntar
                Vector3 direccion = objetivoActual.position - transform.position;
                float anguloObjetivo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
                
                // 2. Comprobamos la diferencia absoluta con nuestro ángulo actual
                float diferencia = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, anguloObjetivo));

                // 3. ¡Condición nueva! Solo dispara si el desfase es menor o igual a 15 grados
                if (diferencia <= 15f)
                {
                    Disparar();
                    temporizadorDisparo = 0f;
                }
            }
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
            
            // Ignora a los enemigos que ya se están desvaneciendo
            if (enemigo != null && enemigo.vida > 0)
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

    void Disparar()
    {
        // Guardamos el lápiz recién creado en una variable
        GameObject lapizDisparado = Instantiate(proyectilPrefab, transform.position, transform.rotation);

        // Le pasamos el objetivo actual al script del lápiz
        lapizDisparado.GetComponent<CodigoLapiz>().AsignarObjetivo(objetivoActual);

        if (rutinaRetroceso != null) StopCoroutine(rutinaRetroceso);
        rutinaRetroceso = StartCoroutine(EfectoRetrocesoVisual());
    }

    IEnumerator EfectoRetrocesoVisual()
    {
        float tiempo = 0f;
        // Calcula el retroceso en la dirección contraria a donde está mirando
        Vector3 posAtras = posicionOriginal - (transform.right * fuerzaRetroceso);

        while (tiempo < tiempoAtras)
        {
            tiempo += Time.deltaTime;
            transform.position = Vector3.Lerp(posicionOriginal, posAtras, tiempo / tiempoAtras);
            yield return null;
        }

        tiempo = 0f;

        while (tiempo < tiempoRecuperacion)
        {
            tiempo += Time.deltaTime;
            transform.position = Vector3.Lerp(posAtras, posicionOriginal, tiempo / tiempoRecuperacion);
            yield return null;
        }

        transform.position = posicionOriginal;
        rutinaRetroceso = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}