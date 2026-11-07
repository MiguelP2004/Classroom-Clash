using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public class CodigoProfe : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject proyectilPrefab;
    public float cadenciaFuego = 0.5f;

    [Header("Mejora x2")]
    public Sprite spriteMejorado; 
    public GameObject flechaMejora; // Arrastra aquí el objeto FlechaMejora
    public int costeMejora = 70;
    public AudioClip sonidoMejora;

    [HideInInspector] public bool bloqueadoPorTutorial = false;

    private bool tieneDisparoDoble = false;
    private SpriteRenderer miSpriteRenderer;
    private Vector3 escalaOriginal;
    private Coroutine rutinaMejora;

    private float fuerzaRetroceso = 0.1f;
    private float tiempoAtras = 0.1f;
    private float tiempoRecuperacion = 0.2f;

    private float temporizadorDisparo;
    private Coroutine rutinaRetroceso;
    private Vector3 posicionOriginal;

    private static bool esperandoSoltarClick = false;

    public static void BloquearDisparoHastaSoltar()
    {
        esperandoSoltarClick = true;
    }

    void Start()
    {
        posicionOriginal = transform.position;
        escalaOriginal = transform.localScale;
        miSpriteRenderer = GetComponent<SpriteRenderer>();
        esperandoSoltarClick = false;

        // Guardamos su posición inicial limpia y ocultamos la flecha hasta comprar la mejora
        if (flechaMejora != null)
        {
            codigoFlechitaMejora scriptFlecha = flechaMejora.GetComponent<codigoFlechitaMejora>();
            if (scriptFlecha != null) scriptFlecha.InicializarPosicionFija();
            flechaMejora.SetActive(false);
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (ControladorJuego.instancia != null && ControladorJuego.instancia.esGameOver) return;

        if (Mouse.current == null || Camera.main == null) return;

        Vector3 posicionRaton = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direccion = posicionRaton - transform.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angulo);

        temporizadorDisparo += Time.deltaTime;

        bool ratonSobreUI = (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
        bool gestorOcupado = (GestorColocacion.instancia != null && GestorColocacion.instancia.EstaOcupado());

        if (ratonSobreUI || gestorOcupado) return;

        if (esperandoSoltarClick)
        {
            if (!Mouse.current.leftButton.isPressed)
            {
                esperandoSoltarClick = false;
            }
            else
            {
                return;
            }
        }

        if (!bloqueadoPorTutorial && Mouse.current.leftButton.isPressed && temporizadorDisparo >= cadenciaFuego)
        {
            Disparar();
            temporizadorDisparo = 0f;
        }
    }

    void Disparar()
    {
        if (proyectilPrefab != null)
        {
            if (!tieneDisparoDoble)
            {
                Instantiate(proyectilPrefab, transform.position, transform.rotation);
            }
            else
            {
                Quaternion rotacion1 = transform.rotation * Quaternion.Euler(0f, 0f, 2f);
                Quaternion rotacion2 = transform.rotation * Quaternion.Euler(0f, 0f, -2f);

                Instantiate(proyectilPrefab, transform.position, rotacion1);
                Instantiate(proyectilPrefab, transform.position, rotacion2);
            }
        }

        if (GestorTutorial.instancia != null) GestorTutorial.instancia.RegistrarDisparoProfe();

        if (rutinaRetroceso != null) StopCoroutine(rutinaRetroceso);
        rutinaRetroceso = StartCoroutine(EfectoRetrocesoVisual());
    }

    public void ComprarMejora(EfectoBoton botonQueLlama)
    {
        BloquearDisparoHastaSoltar();

        if (tieneDisparoDoble) return;
        if (ControladorJuego.instancia == null || ControladorJuego.instancia.esGameOver) return;
        if (!ControladorJuego.instancia.PuedeComprar(costeMejora)) return;

        ControladorJuego.instancia.GastarEnergia(costeMejora);
        tieneDisparoDoble = true;

        if (sonidoMejora != null)
        {
            GameObject audioTemporal = new GameObject("Sonido_Mejora");
            AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();

            fuenteAudio.clip = sonidoMejora;
            fuenteAudio.spatialBlend = 0f;
            fuenteAudio.volume = ControladorJuego.volumenSFXGuardado;

            fuenteAudio.Play();
            Destroy(audioTemporal, sonidoMejora.length + 0.1f);
        }

        if (botonQueLlama != null)
        {
            botonQueLlama.forzadoActivo = false;
            botonQueLlama.IniciarCooldown();
            
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            StartCoroutine(RutinaBloqueoPermanente(botonQueLlama));
        }

        if (miSpriteRenderer != null && spriteMejorado != null)
        {
            miSpriteRenderer.sprite = spriteMejorado;
        }

        // Activamos la flechita de mejora antes de iniciar el escalado del Profe
        if (flechaMejora != null)
        {
            flechaMejora.SetActive(true);
        }

        if (rutinaMejora != null) StopCoroutine(rutinaMejora);
        rutinaMejora = StartCoroutine(EfectoVisualMejora());

        if (GestorTutorial.instancia != null)
        {
            GestorTutorial.instancia.RegistrarMejoraProfe();
        }
    }

    IEnumerator RutinaBloqueoPermanente(EfectoBoton boton)
    {
        yield return new WaitForSeconds(boton.tiempoCooldown);
        if (boton != null)
        {
            boton.compradoPermanente = true;
        }
    }

    IEnumerator EfectoVisualMejora()
    {
        float duracionSubida = 0.2f;
        float duracionBajada = 0.5f;

        Vector3 escalaMax = escalaOriginal * 1.15f;
        Color colorAzul = new Color(0.3f, 0.85f, 1f, 1f); 

        float tiempo = 0f;
        while (tiempo < duracionSubida)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracionSubida);
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaMax, t);
            if (miSpriteRenderer != null) miSpriteRenderer.color = Color.Lerp(Color.white, colorAzul, t);
            yield return null;
        }

        tiempo = 0f;
        while (tiempo < duracionBajada)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracionBajada);
            transform.localScale = Vector3.Lerp(escalaMax, escalaOriginal, t);
            if (miSpriteRenderer != null) miSpriteRenderer.color = Color.Lerp(colorAzul, Color.white, t);
            yield return null;
        }

        transform.localScale = escalaOriginal;
        if (miSpriteRenderer != null) miSpriteRenderer.color = Color.white;
        rutinaMejora = null;
    }

    IEnumerator EfectoRetrocesoVisual()
    {
        float tiempo = 0f;
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
}