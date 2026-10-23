using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class codigoExamen : MonoBehaviour
{
    [Header("Estadísticas")]
    public float vida = 3f; 
    private float vidaMaxima;
    public float velocidad = 2f; 
    private bool estaMuriendo = false; // Nueva variable de estado

    [Header("Interfaz de Vida")]
    public GameObject canvasBarraVida;
    public Image imagenRellenoVida;
    public Color colorVidaLleno = Color.green; // Color para vida alta
    public Color colorVidaVacio = Color.red;   // Color para vida baja

    [Header("Balanceo")]
    public float inclinacionMaxima = 7f;
    public float velocidadBalanceo = 3f;
    private float tiempoBalanceo = 0f;
    
    [Header("Efecto Daño y Muerte")]
    public float duracionGolpe = 0.15f;
    public float tiempoDesvanecer = 0.5f;

    [HideInInspector]
    public float distanciaRecorrida = 0f; 

    private SpriteRenderer[] todosLosSprites;
    private Color[] coloresBase;
    private Coroutine rutinaGolpe;

    void Awake()
    {
        vidaMaxima = vida;

        todosLosSprites = GetComponentsInChildren<SpriteRenderer>();
        coloresBase = new Color[todosLosSprites.Length];
        
        for (int i = 0; i < todosLosSprites.Length; i++)
        {
            coloresBase[i] = todosLosSprites[i].color;
        }

        // Ocultamos la barra de vida al nacer para que no aparezca al 100%
        if (canvasBarraVida != null) canvasBarraVida.SetActive(false);
    }

    void Update()
    {
        if (estaMuriendo) return;

        Vector3 movimiento = Vector3.right * velocidad * Time.deltaTime;
        transform.Translate(movimiento, Space.World);
        distanciaRecorrida += movimiento.magnitude;
    }

    void LateUpdate()
    {
        if (estaMuriendo) return;

        // 1. Aplicamos el balanceo al examen
        tiempoBalanceo += Time.deltaTime;
        float anguloActual = inclinacionMaxima * Mathf.Sin(tiempoBalanceo * velocidadBalanceo);
        transform.rotation = Quaternion.Euler(0, 0, anguloActual);

        // 2. Contrarrestamos el balanceo en la barra de vida para que siempre esté recta
        if (canvasBarraVida != null && canvasBarraVida.activeSelf)
        {
            canvasBarraVida.transform.rotation = Quaternion.identity;
        }
    }

    public void RecibirImpacto(float danoRecibido)
    {
        if (estaMuriendo) return;

        vida -= danoRecibido;
        ActualizarBarraVida(); // Llamamos al nuevo método de la UI

        if (vida > 0)
        {
            if (rutinaGolpe != null) StopCoroutine(rutinaGolpe);
            rutinaGolpe = StartCoroutine(EfectoGolpeBrillo());
        }
        else
        {
            estaMuriendo = true;
            gameObject.tag = "Untagged"; // Para que no le sigan disparando mientras muere
            
            if (canvasBarraVida != null) canvasBarraVida.SetActive(false);
            
            // Si estaba parpadeando de rojo, lo paramos antes de desvanecer
            if (rutinaGolpe != null) StopCoroutine(rutinaGolpe); 
            
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
            // Calculamos el porcentaje (0 a 1)
            float porcentaje = Mathf.Clamp01(vida / vidaMaxima);
            imagenRellenoVida.fillAmount = porcentaje;
            
            // Lerp mezcla los colores suavemente (0 = vacío/rojo, 1 = lleno/verde)
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
                    // Usa siempre el color original (sin tono rojo) y solo modifica su Alpha
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