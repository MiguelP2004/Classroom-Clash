using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class codigoExamen : MonoBehaviour
{
    [Header("Estadísticas")]
    public float vida = 4f; 
    private float vidaMaxima;
    public float velocidad = 1.2f; 
    private float velocidadOriginal;
    private bool estaMuriendo = false;
    public float energiaRecibida = 3f;

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

    [Header("Efecto Hielo")]
    public ParticleSystem particulasHielo;
    public AudioClip sonidoCongelacion;
    private bool congelado = false;
    private float tiempoRestanteCongelacion = 0f;
    private Color colorCongelado = new Color(0f, 171f / 255f, 1f, 1f);
    private static int ultimoFrameSonidoCongelacion = -1;

    // Aturdimiento (Basket)
    private float tiempoRestanteAturdimiento = 0f;

    [Header("Sombra")]
    public GameObject prefabSombra;

    [HideInInspector]
    public float distanciaRecorrida = 0f; 

    private EdgeCollider2D ruta;
    private int indicePuntoActual = 0;
    private Vector3 desvioCamino;

    private SpriteRenderer[] todosLosSprites;
    private Color[] coloresBase;
    private Coroutine rutinaGolpe;

    private generacionExamenes generador;

    void Awake()
    {
        vidaMaxima = vida;
        velocidadOriginal = velocidad;

        desvioCamino = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.2f, 0.2f), 0f);

        todosLosSprites = GetComponentsInChildren<SpriteRenderer>();
        coloresBase = new Color[todosLosSprites.Length];
        
        for (int i = 0; i < todosLosSprites.Length; i++)
        {
            coloresBase[i] = todosLosSprites[i].color;
        }

        if (canvasBarraVida != null) canvasBarraVida.SetActive(false);
        if (particulasHielo != null) particulasHielo.Stop();
    }

    void Start()
    {
        if (prefabSombra != null && todosLosSprites.Length > 0)
        {
            GameObject miSombra = Instantiate(prefabSombra, transform.position, Quaternion.identity);
            SombraExamen scriptSombra = miSombra.GetComponent<SombraExamen>();
            
            if (scriptSombra != null)
            {
                scriptSombra.Inicializar(this.transform, todosLosSprites[0]);
            }
        }
    }

    public void AsignarGenerador(generacionExamenes g)
    {
        generador = g;
    }

    public void AsignarRuta(EdgeCollider2D nuevaRuta)
    {
        ruta = nuevaRuta;
        indicePuntoActual = 0;
    }

    void Update()
    {
        if (estaMuriendo) return;

        // Gestión del Hielo
        if (congelado)
        {
            tiempoRestanteCongelacion -= Time.deltaTime;
            if (tiempoRestanteCongelacion <= 0f)
            {
                Descongelar();
            }
        }

        // Gestión del Aturdimiento: se queda totalmente quieto
        if (tiempoRestanteAturdimiento > 0f)
        {
            tiempoRestanteAturdimiento -= Time.deltaTime;
            return;
        }

        // Movimiento
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
            Vector3 movimiento = Vector3.right * velocidad * Time.deltaTime;
            transform.Translate(movimiento, Space.World);
            distanciaRecorrida += movimiento.magnitude;
        }

        if (transform.position.x > 9.3f)
        {
            estaMuriendo = true; 
            if (ControladorJuego.instancia != null)
            {
                ControladorJuego.instancia.RestarVida(1);
            }
            if (GestorTutorial.instancia != null)
            {
                GestorTutorial.instancia.RegistrarMuerteExamen();
            }
            if (generador != null)
            {
                generador.EnemigoEliminado();
            }
            Destroy(gameObject);
            return;
        }
    }

    void LateUpdate()
    {
        if (estaMuriendo) return;

        // Mientras está aturdido no avanza el balanceo y mantiene el último ángulo exacto
        if (tiempoRestanteAturdimiento <= 0f)
        {
            tiempoBalanceo += Time.deltaTime;
            float anguloActual = inclinacionMaxima * Mathf.Sin(tiempoBalanceo * velocidadBalanceo);
            transform.rotation = Quaternion.Euler(0, 0, anguloActual);
        }

        if (canvasBarraVida != null && canvasBarraVida.activeSelf)
        {
            canvasBarraVida.transform.rotation = Quaternion.identity;
        }
    }

    public void AplicarAturdimiento(float duracion)
    {
        if (estaMuriendo) return;

        if (duracion > tiempoRestanteAturdimiento)
        {
            tiempoRestanteAturdimiento = duracion;
        }
    }

    public void AplicarCongelacion()
    {
        if (estaMuriendo) return;

        tiempoRestanteCongelacion = 8f; 

        if (!congelado)
        {
            congelado = true;
            velocidad = velocidadOriginal * 0.5f; 

            if (particulasHielo != null) particulasHielo.Play();

            if (MenuPrincipal.instancia == null && sonidoCongelacion != null && Time.frameCount != ultimoFrameSonidoCongelacion)
            {
                ultimoFrameSonidoCongelacion = Time.frameCount;
                GameObject audioTemporal = new GameObject("Sonido_Congelacion");
                AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();
                fuenteAudio.clip = sonidoCongelacion;
                fuenteAudio.spatialBlend = 0f;
                fuenteAudio.volume = ControladorJuego.volumenSFXGuardado * 0.5f;
                fuenteAudio.Play();
                Destroy(audioTemporal, sonidoCongelacion.length + 0.1f);
            }

            if (rutinaGolpe == null)
            {
                for (int i = 0; i < todosLosSprites.Length; i++)
                {
                    if (todosLosSprites[i] != null)
                    {
                        todosLosSprites[i].color = Color.Lerp(coloresBase[i], colorCongelado, 0.65f);
                    }
                }
            }
        }
    }

    private void Descongelar()
    {
        congelado = false;
        velocidad = velocidadOriginal;

        if (particulasHielo != null) particulasHielo.Stop();

        if (rutinaGolpe == null)
        {
            for (int i = 0; i < todosLosSprites.Length; i++)
            {
                if (todosLosSprites[i] != null)
                {
                    todosLosSprites[i].color = coloresBase[i];
                }
            }
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
            
            CodigoTrofeo.posicionUltimaBaja = transform.position;

            if (canvasBarraVida != null) canvasBarraVida.SetActive(false);
            if (particulasHielo != null) particulasHielo.Stop();
            if (rutinaGolpe != null) StopCoroutine(rutinaGolpe); 
            
            if (ControladorJuego.instancia != null)
            {
                float reduccion = (generador != null) ? Mathf.Clamp01(generador.reduccionEnergia) : 0f;
                float energiaFinal = energiaRecibida * (1f - reduccion);
                ControladorJuego.instancia.SumarEnergia(energiaFinal);
                ControladorJuego.instancia.SumarBaja();
            }
            if (GestorTutorial.instancia != null)
            {
                GestorTutorial.instancia.RegistrarMuerteExamen();
            }
            if (generador != null)
            {
                generador.EnemigoEliminado();
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
                    Color colorDestino = congelado ? Color.Lerp(coloresBase[i], colorCongelado, 0.7f) : coloresBase[i];
                    todosLosSprites[i].color = Color.Lerp(colorDanio, colorDestino, t);
                }
            }
            yield return null;
        }

        for (int i = 0; i < todosLosSprites.Length; i++)
        {
            if (todosLosSprites[i] != null)
            {
                Color colorDestino = congelado ? Color.Lerp(coloresBase[i], colorCongelado, 0.7f) : coloresBase[i];
                todosLosSprites[i].color = colorDestino;
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
                    Color colorDestino = congelado ? Color.Lerp(coloresBase[i], colorCongelado, 0.7f) : coloresBase[i];
                    float alphaActual = Mathf.Lerp(colorDestino.a, 0f, t);
                    todosLosSprites[i].color = new Color(colorDestino.r, colorDestino.g, colorDestino.b, alphaActual);
                }
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}