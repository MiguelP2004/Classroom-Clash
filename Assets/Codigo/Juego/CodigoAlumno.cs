using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public class CodigoAlumno : MonoBehaviour
{
    public enum TipoAlumno { Basico, Hielo, Boomerang, Basket }

    public GameObject proyectilPrefab;
    public GameObject prefabFlechaMejora;
    public AudioClip sonidoMejora;
    private float velocidadGiro = 6f;

    [Header("Disparo")]
    public float rangoAtaque = 3f;
    public float cadenciaFuego = 1.2f;
    public float danoProyectil = 1f;
    public float zonaImpactoHielo = 0f;
    public int maxImpactosBoomerang = 0;
    public float radioZonaBasket = 0f;
    public float danoAreaBasket = 0f;
    public float duracionAturdimientoBasket = 0f;

    [Header("Mejora Nivel 2")]
    public float rangoMejorado = 3.5f;
    public float cadenciaMejorada = 1.0f;
    public float danoMejorado = 1.2f;
    public float zonaImpactoHieloMejorada = 0f;
    public int maxImpactosBoomerangMejorado = 0;
    public float radioZonaBasketMejorado = 0f;
    public float danoAreaBasketMejorado = 0f;
    public float duracionAturdimientoBasketMejorada = 0f;
    public int costeMejora = 30;

    [Header("Mejora Nivel 3")]
    public float rangoMejoradoNivel3 = 4.0f;
    public float cadenciaMejoradaNivel3 = 0.8f;
    public float danoMejoradoNivel3 = 1.5f;
    public float zonaImpactoHieloMejoradaNivel3 = 0f;
    public int maxImpactosBoomerangNivel3 = 0;
    public float radioZonaBasketNivel3 = 0f;
    public float danoAreaBasketNivel3 = 0f;
    public float duracionAturdimientoBasketNivel3 = 0f;
    public int costeMejoraNivel3 = 50;

    [Header("Brillo Selección y Mejora")]
    public float velocidadBrilloSeleccion = 5f;
    public float intensidadBrilloSeleccion = 2.1f;
    public float intensidadBrilloMejora = 2.4f;

    private float fuerzaRetroceso = 0.1f;
    private float tiempoAtras = 0.1f;
    private float tiempoRecuperacion = 0.2f;

    [HideInInspector] public bool estaColocado = false;
    [HideInInspector] public bool seleccionado = false;
    [HideInInspector] public int nivelActual = 1;

    private bool listoParaSeleccionar = false;
    private bool inicializado = false;
    private bool estabaSeleccionado = false;
    private float tiempoCosenoSeleccion = 0f;

    private Transform objetivoActual;
    private float temporizadorDisparo;
    private Coroutine rutinaRetroceso;
    private Coroutine rutinaMejora;
    private Vector3 posicionOriginal;
    private Vector3 escalaOriginal;
    private SpriteRenderer miSpriteRenderer;
    private MaterialPropertyBlock propBlock;
    private static readonly int idColor = Shader.PropertyToID("_Color");
    private codigoFlechitaMejora flechaInstanciada;

    public TipoAlumno ObtenerTipoAlumno()
    {
        if (proyectilPrefab != null)
        {
            CodigoLapiz lapiz = proyectilPrefab.GetComponent<CodigoLapiz>();
            if (lapiz != null)
            {
                if (lapiz.esHielo) return TipoAlumno.Hielo;
                if (lapiz.esBoomerang) return TipoAlumno.Boomerang;
                if (lapiz.esBasket) return TipoAlumno.Basket;
            }
        }
        return TipoAlumno.Basico;
    }

    public bool EstaAlMaximo() => nivelActual >= 3;
    public int ObtenerCosteSiguienteMejora() => (nivelActual == 1) ? costeMejora : costeMejoraNivel3;
    public float ObtenerRangoSiguiente() => (nivelActual == 1) ? rangoMejorado : rangoMejoradoNivel3;
    public float ObtenerCadenciaSiguiente() => (nivelActual == 1) ? cadenciaMejorada : cadenciaMejoradaNivel3;
    public float ObtenerDanoSiguiente() => (nivelActual == 1) ? danoMejorado : danoMejoradoNivel3;
    public float ObtenerZonaHieloSiguiente() => (nivelActual == 1) ? zonaImpactoHieloMejorada : zonaImpactoHieloMejoradaNivel3;
    public int ObtenerMaxImpactosSiguiente() => (nivelActual == 1) ? maxImpactosBoomerangMejorado : maxImpactosBoomerangNivel3;
    public float ObtenerRadioBasketSiguiente() => (nivelActual == 1) ? radioZonaBasketMejorado : radioZonaBasketNivel3;
    public float ObtenerDanoAreaBasketSiguiente() => (nivelActual == 1) ? danoAreaBasketMejorado : danoAreaBasketNivel3;
    public float ObtenerAturdimientoBasketSiguiente() => (nivelActual == 1) ? duracionAturdimientoBasketMejorada : duracionAturdimientoBasketNivel3;

    void Awake()
    {
        miSpriteRenderer = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
        escalaOriginal = transform.localScale;
    }

    private void AplicarColorBrillo(Color colorObjetivo)
    {
        if (miSpriteRenderer == null) return;
        if (propBlock == null) propBlock = new MaterialPropertyBlock();

        miSpriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(idColor, colorObjetivo);
        miSpriteRenderer.SetPropertyBlock(propBlock);
    }

    void Update()
    {
        if (!estaColocado) return;

        if (!inicializado)
        {
            posicionOriginal = transform.position;
            escalaOriginal = transform.localScale;
            temporizadorDisparo = cadenciaFuego;
            inicializado = true;
        }

        if (!listoParaSeleccionar)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                listoParaSeleccionar = true;
            }
        }
        else
        {
            bool bloqueado = (Time.timeScale == 0f) ||
                             (GestorColocacion.instancia != null && GestorColocacion.instancia.EstaOcupado()) ||
                             (ControladorJuego.instancia != null && (ControladorJuego.instancia.esGameOver || ControladorJuego.instancia.enTransicion));

            if (!bloqueado && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                bool ratonSobreUI = (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());

                if (!ratonSobreUI && Camera.main != null)
                {
                    Vector2 posicionRaton = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    Collider2D[] tocados = Physics2D.OverlapPointAll(posicionRaton);

                    bool meHanTocado = false;
                    foreach (Collider2D col in tocados)
                    {
                        if (col.transform == this.transform)
                        {
                            meHanTocado = true;
                            break;
                        }
                    }

                    if (meHanTocado)
                    {
                        seleccionado = true;
                        CodigoProfe.BloquearDisparoHastaSoltar();

                        if (GestionMejoras.instancia != null)
                        {
                            GestionMejoras.instancia.SeleccionarAlumno(this);
                        }
                    }
                    else if (seleccionado)
                    {
                        seleccionado = false;

                        if (GestionMejoras.instancia != null)
                        {
                            GestionMejoras.instancia.NotificarDeseleccion(this);
                        }
                    }
                }
            }
        }

        if (seleccionado)
        {
            if (!estabaSeleccionado)
            {
                estabaSeleccionado = true;
                tiempoCosenoSeleccion = 0f;
            }

            if (rutinaMejora == null && miSpriteRenderer != null)
            {
                tiempoCosenoSeleccion += Time.deltaTime * velocidadBrilloSeleccion;
                float ondaCoseno = (1f - Mathf.Cos(tiempoCosenoSeleccion)) * 0.5f;
                Color colorVerdeFuerte = new Color(0.6f, intensidadBrilloSeleccion, 0.45f, 1f);
                AplicarColorBrillo(Color.Lerp(Color.white, colorVerdeFuerte, ondaCoseno));
            }
        }
        else if (estabaSeleccionado)
        {
            estabaSeleccionado = false;
            if (rutinaMejora == null && miSpriteRenderer != null)
            {
                AplicarColorBrillo(Color.white);
            }
        }

        BuscarObjetivo();

        if (objetivoActual != null)
        {
            ApuntarSmooth();

            temporizadorDisparo += Time.deltaTime;
            if (temporizadorDisparo >= cadenciaFuego)
            {
                Vector3 direccion = objetivoActual.position - transform.position;
                float anguloObjetivo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

                float diferencia = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, anguloObjetivo));

                if (diferencia <= 25f)
                {
                    Disparar();
                    temporizadorDisparo = 0f;
                }
            }
        }
    }

    public void AplicarMejora()
    {
        if (EstaAlMaximo()) return;

        if (rutinaMejora != null) StopCoroutine(rutinaMejora);
        transform.localScale = escalaOriginal;

        TipoAlumno tipo = ObtenerTipoAlumno();

        if (nivelActual == 1)
        {
            nivelActual = 2;
            rangoAtaque = rangoMejorado;
            cadenciaFuego = cadenciaMejorada;
            danoProyectil = danoMejorado;

            if (tipo == TipoAlumno.Hielo && zonaImpactoHieloMejorada > 0f)
            {
                zonaImpactoHielo = zonaImpactoHieloMejorada;
            }
            else if (tipo == TipoAlumno.Boomerang && maxImpactosBoomerangMejorado > 0)
            {
                maxImpactosBoomerang = maxImpactosBoomerangMejorado;
            }
            else if (tipo == TipoAlumno.Basket)
            {
                if (radioZonaBasketMejorado > 0f) radioZonaBasket = radioZonaBasketMejorado;
                if (danoAreaBasketMejorado > 0f) danoAreaBasket = danoAreaBasketMejorado;
                if (duracionAturdimientoBasketMejorada > 0f) duracionAturdimientoBasket = duracionAturdimientoBasketMejorada;
            }

            if (prefabFlechaMejora != null)
            {
                GameObject flecha = Instantiate(prefabFlechaMejora, transform);
                flechaInstanciada = flecha.GetComponent<codigoFlechitaMejora>();
                if (flechaInstanciada != null)
                {
                    flechaInstanciada.Inicializar(posicionOriginal, escalaOriginal);
                }
            }
        }
        else if (nivelActual == 2)
        {
            nivelActual = 3;
            rangoAtaque = rangoMejoradoNivel3;
            cadenciaFuego = cadenciaMejoradaNivel3;
            danoProyectil = danoMejoradoNivel3;

            if (tipo == TipoAlumno.Hielo && zonaImpactoHieloMejoradaNivel3 > 0f)
            {
                zonaImpactoHielo = zonaImpactoHieloMejoradaNivel3;
            }
            else if (tipo == TipoAlumno.Boomerang && maxImpactosBoomerangNivel3 > 0)
            {
                maxImpactosBoomerang = maxImpactosBoomerangNivel3;
            }
            else if (tipo == TipoAlumno.Basket)
            {
                if (radioZonaBasketNivel3 > 0f) radioZonaBasket = radioZonaBasketNivel3;
                if (danoAreaBasketNivel3 > 0f) danoAreaBasket = danoAreaBasketNivel3;
                if (duracionAturdimientoBasketNivel3 > 0f) duracionAturdimientoBasket = duracionAturdimientoBasketNivel3;
            }

            if (flechaInstanciada == null && prefabFlechaMejora != null)
            {
                GameObject flecha = Instantiate(prefabFlechaMejora, transform);
                flechaInstanciada = flecha.GetComponent<codigoFlechitaMejora>();
                if (flechaInstanciada != null)
                {
                    flechaInstanciada.Inicializar(posicionOriginal, escalaOriginal);
                }
            }

            if (flechaInstanciada != null)
            {
                flechaInstanciada.CambiarASpriteNivel3();
            }
        }

        if (sonidoMejora != null)
        {
            GameObject audioTemporal = new GameObject("Sonido_MejoraAlumno");
            AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();

            fuenteAudio.clip = sonidoMejora;
            fuenteAudio.spatialBlend = 0f;
            fuenteAudio.volume = ControladorJuego.volumenSFXGuardado;

            fuenteAudio.Play();
            Destroy(audioTemporal, sonidoMejora.length + 0.1f);
        }

        rutinaMejora = StartCoroutine(EfectoVisualMejora());
    }

    IEnumerator EfectoVisualMejora()
    {
        float duracionSubida = 0.2f;
        float duracionBajada = 0.5f;

        Vector3 escalaMax = escalaOriginal * 1.15f;
        Color colorAzulFuerte = new Color(0.45f, intensidadBrilloMejora * 0.73f, intensidadBrilloMejora, 1f);

        float tiempo = 0f;
        while (tiempo < duracionSubida)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracionSubida);
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaMax, t);
            AplicarColorBrillo(Color.Lerp(Color.white, colorAzulFuerte, t));
            yield return null;
        }

        tiempo = 0f;
        while (tiempo < duracionBajada)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracionBajada);
            transform.localScale = Vector3.Lerp(escalaMax, escalaOriginal, t);
            AplicarColorBrillo(Color.Lerp(colorAzulFuerte, Color.white, t));
            yield return null;
        }

        transform.localScale = escalaOriginal;
        AplicarColorBrillo(Color.white);

        tiempoCosenoSeleccion = 0f;
        rutinaMejora = null;
    }

    void BuscarObjetivo()
    {
        Collider2D[] detectados = Physics2D.OverlapCircleAll(transform.position, rangoAtaque);

        float mayorProgreso = -1f;
        Transform mejorObjetivo = null;

        foreach (Collider2D col in detectados)
        {
            codigoExamen enemigo = col.GetComponent<codigoExamen>();

            if (enemigo != null && enemigo.vida > 0 && !enemigo.esInvisible)
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
        GameObject lapizDisparado = Instantiate(proyectilPrefab, transform.position, transform.rotation);
        CodigoLapiz scriptLapiz = lapizDisparado.GetComponent<CodigoLapiz>();

        if (scriptLapiz != null)
        {
            TipoAlumno tipo = ObtenerTipoAlumno();

            scriptLapiz.daño = danoProyectil;
            scriptLapiz.radioZonaHielo = (tipo == TipoAlumno.Hielo) ? zonaImpactoHielo : 0f;

            if (tipo == TipoAlumno.Boomerang && maxImpactosBoomerang > 0)
            {
                scriptLapiz.maxImpactosPorTramo = maxImpactosBoomerang;
            }

            if (tipo == TipoAlumno.Basket)
            {
                scriptLapiz.radioZonaBasket = radioZonaBasket;
                scriptLapiz.danoAreaBasket = danoAreaBasket;
                scriptLapiz.duracionAturdimientoBasket = duracionAturdimientoBasket;
            }

            scriptLapiz.AsignarOrigen(this.transform);
            scriptLapiz.AsignarObjetivo(objetivoActual);
        }

        if (rutinaRetroceso != null) StopCoroutine(rutinaRetroceso);
        rutinaRetroceso = StartCoroutine(EfectoRetrocesoVisual());
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

    private void OnDrawGizmosSelected()
    {
        if (Object.FindAnyObjectByType<MenuPrincipal>() == null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        }
    }
}