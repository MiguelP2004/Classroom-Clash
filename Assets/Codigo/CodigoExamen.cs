using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class codigoExamen : MonoBehaviour
{
    [Header("Estadísticas")]
    public float vida = 3f; 
    private float vidaMaxima;
    public float velocidad = 2f; 
    private bool estaMuriendo = false;

    [Header("Interfaz de Vida")]
    public GameObject canvasBarraVida;
    public Image imagenRellenoVida;
    public Color colorVidaLleno = Color.green;
    public Color colorVidaVacio = Color.red;

    [Header("Balanceo")]
    public float inclinacionMaxima = 7f;
    public float velocidadBalanceo = 3f;
    private float tiempoBalanceo = 0f;
    
    [Header("Efecto Daño y Muerte")]
    public float duracionGolpe = 0.15f;
    public float tiempoDesvanecer = 0.5f;

    [HideInInspector]
    public float distanciaRecorrida = 0f; 

    private EdgeCollider2D ruta;
    private int indicePuntoActual = 0;
    private Vector3 desvioCamino;

    private SpriteRenderer[] todosLosSprites;
    private Color[] coloresBase;
    private Coroutine rutinaGolpe;

    void Awake()
    {
        vidaMaxima = vida;

        // ¡NUEVO!: Desvío más amplio en la Y (-0.2 a 0.2) para ocupar todo el ancho del camino
        desvioCamino = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.2f, 0.2f), 0f);

        todosLosSprites = GetComponentsInChildren<SpriteRenderer>();
        coloresBase = new Color[todosLosSprites.Length];
        
        for (int i = 0; i < todosLosSprites.Length; i++)
        {
            coloresBase[i] = todosLosSprites[i].color;
        }

        if (canvasBarraVida != null) canvasBarraVida.SetActive(false);
    }

    public void AsignarRuta(EdgeCollider2D nuevaRuta)
    {
        ruta = nuevaRuta;
        indicePuntoActual = 0;
    }

    void Update()
    {
        if (estaMuriendo) return;

        // Si tiene una línea asignada, sigue sus puntos uno por uno
        if (ruta != null && ruta.pointCount > 0 && indicePuntoActual < ruta.pointCount)
        {
            Vector3 puntoDestino = ruta.transform.TransformPoint(ruta.points[indicePuntoActual]) + desvioCamino;
            puntoDestino.z = 0f;

            Vector3 posAnterior = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, puntoDestino, velocidad * Time.deltaTime);
            distanciaRecorrida += Vector3.Distance(posAnterior, transform.position);

            if (Vector2.Distance(transform.position, puntoDestino) < 0.05f)
            {
                indicePuntoActual++;
            }
        }
        else
        {
            // Movimiento hacia la derecha al terminar la ruta (o si no hay ruta asignada)
            Vector3 movimiento = Vector3.right * velocidad * Time.deltaTime;
            transform.Translate(movimiento, Space.World);
            distanciaRecorrida += movimiento.magnitude;
        }
    }

    void LateUpdate()
    {
        if (estaMuriendo) return;

        tiempoBalanceo += Time.deltaTime;
        float anguloActual = inclinacionMaxima * Mathf.Sin(tiempoBalanceo * velocidadBalanceo);
        transform.rotation = Quaternion.Euler(0, 0, anguloActual);

        if (canvasBarraVida != null && canvasBarraVida.activeSelf)
        {
            canvasBarraVida.transform.rotation = Quaternion.identity;
        }
    }

    public void RecibirImpacto(float danoRecibido)
    {
        if (estaMuriendo) return;

        vida -= danoRecibido;
        ActualizarBarraVida();

        if (vida > 0)
        {
            if (rutinaGolpe != null) StopCoroutine(rutinaGolpe);
            rutinaGolpe = StartCoroutine(EfectoGolpeBrillo());
        }
        else
        {
            estaMuriendo = true;
            gameObject.tag = "Untagged"; 
            
            if (canvasBarraVida != null) canvasBarraVida.SetActive(false);
            if (rutinaGolpe != null) StopCoroutine(rutinaGolpe); 
            
            if (ControladorJuego.instancia != null)
            {
                ControladorJuego.instancia.SumarEnergia(3);
            }
            if (GestorTutorial.instancia != null)
            {
                GestorTutorial.instancia.RegistrarMuerteExamen();
            }
            
            StartCoroutine(EfectoDesvanecer());
        }
    }

    void ActualizarBarraVida()
    {
        if (canvasBarraVida != null && !canvasBarraVida.activeSelf && vida > 0) 
        {
            canvasBarraVida.SetActive(true);
        }

        if (imagenRellenoVida != null)
        {
            float porcentaje = Mathf.Clamp01(vida / vidaMaxima);
            imagenRellenoVida.fillAmount = porcentaje;
            imagenRellenoVida.color = Color.Lerp(colorVidaVacio, colorVidaLleno, porcentaje);
        }
    }

    IEnumerator EfectoGolpeBrillo()
    {
        float tiempo = 0f;
        Color colorDanio = Color.red;

        while (tiempo < duracionGolpe)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionGolpe;

            for (int i = 0; i < todosLosSprites.Length; i++)
            {
                if (todosLosSprites[i] != null)
                {
                    todosLosSprites[i].color = Color.Lerp(colorDanio, coloresBase[i], t);
                }
            }
            yield return null;
        }

        for (int i = 0; i < todosLosSprites.Length; i++)
        {
            if (todosLosSprites[i] != null)
            {
                todosLosSprites[i].color = coloresBase[i];
            }
        }
        
        rutinaGolpe = null;
    }

    IEnumerator EfectoDesvanecer()
    {
        float tiempo = 0f;

        while (tiempo < tiempoDesvanecer)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoDesvanecer;

            for (int i = 0; i < todosLosSprites.Length; i++)
            {
                if (todosLosSprites[i] != null)
                {
                    Color c = coloresBase[i]; 
                    float alphaActual = Mathf.Lerp(c.a, 0f, t);
                    todosLosSprites[i].color = new Color(c.r, c.g, c.b, alphaActual);
                }
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}