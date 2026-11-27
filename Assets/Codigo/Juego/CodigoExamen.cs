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

    [Header("Vida")]
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

    private float tiempoRestanteAturdimiento = 0f;

    [Header("Científico")]
    public bool esCientifico = false;
    public GameObject prefabParticulasTeleport;
    public AudioClip sonidoTeleport;

    [HideInInspector] public bool esInvisible = false;

    [Header("Sombra")]
    public GameObject prefabSombra;

    [HideInInspector]
    public float distanciaRecorrida = 0f; 

    private EdgeCollider2D ruta;
    private int indicePuntoActual = 0;
    private Vector3 desvioCamino;

    private SpriteRenderer[] todosLosSprites;
    private Collider2D[] todosLosColliders;
    private Color[] coloresBase;
    private Coroutine rutinaGolpe;
    private Coroutine rutinaCientifico;
    private Animator miAnimator;
    private SombraExamen scriptSombra;
    private bool haciendoAnimacion = false;
    private string tagOriginal = "Examen";

    private generacionExamenes generador;

    void Awake()
    {
        vidaMaxima = vida;
        velocidadOriginal = velocidad;
        tagOriginal = gameObject.tag;

        desvioCamino = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.2f, 0.2f), 0f);

        todosLosSprites = GetComponentsInChildren<SpriteRenderer>();
        todosLosColliders = GetComponentsInChildren<Collider2D>();
        miAnimator = GetComponent<Animator>();
        if (miAnimator == null) miAnimator = GetComponentInChildren<Animator>();

        // Desactivamos el Animator al caminar para que no interfiera con el balanceo
        if (esCientifico && miAnimator != null) miAnimator.enabled = false;

        coloresBase = new Color[todosLosSprites.Length];
        for (int i = 0; i < todosLosSprites.Length; i++)
        {
            coloresBase[i] = todosLosSprites[i].color;
        }

        if (canvasBarraVida != null) canvasBarraVida.SetActive(false);
        if (particulasHielo != null) particulasHielo.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Start()
    {
        if (prefabSombra != null && todosLosSprites.Length > 0)
        {
            GameObject miSombra = Instantiate(prefabSombra, transform.position, Quaternion.identity);
            scriptSombra = miSombra.GetComponent<SombraExamen>();
            
            if (scriptSombra != null)
            {
                scriptSombra.Inicializar(this.transform, todosLosSprites[0]);
            }
        }

        if (esCientifico)
        {
            rutinaCientifico = StartCoroutine(RutinaCientifico());
        }
    }

    void OnDestroy()
    {
        if (scriptSombra != null)
        {
            Destroy(scriptSombra.gameObject);
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

    private void EstablecerAnimacionEspecial(bool activa)
    {
        haciendoAnimacion = activa;
        tiempoBalanceo = 0f;
        transform.rotation = Quaternion.identity;
    }

    private float ObtenerDuracionAnimacion(string nombreEstado)
    {
        if (miAnimator == null) return 0f;

        AnimatorStateInfo info = miAnimator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(nombreEstado) && info.length > 0.01f)
        {
            return info.length;
        }

        if (miAnimator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in miAnimator.runtimeAnimatorController.animationClips)
            {
                if (clip != null && clip.name == nombreEstado && clip.length > 0.01f)
                {
                    return clip.length;
                }
            }
        }

        return 0.5f;
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

        // Mientras es invisible no camina ni puede cruzar la meta
        if (esInvisible) return;

        // Gestión del Aturdimiento: se queda totalmente quieto
        if (tiempoRestanteAturdimiento > 0f)
        {
            tiempoRestanteAturdimiento -= Time.deltaTime;
            return;
        }

        // Se para obligatoriamente durante cualquier animación especial
        if (haciendoAnimacion) return;

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
            if (rutinaCientifico != null) StopCoroutine(rutinaCientifico);
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
        if (estaMuriendo || esInvisible) return;

        if (!haciendoAnimacion && tiempoRestanteAturdimiento <= 0f)
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

    private IEnumerator RutinaCientifico()
    {
        float tiempoEspera = 1.5f + Random.Range(0f, 1f);

        while (!estaMuriendo)
        {
            float transcurrido = 0f;
            while (transcurrido < tiempoEspera)
            {
                if (estaMuriendo) yield break;
                transcurrido += Time.deltaTime;
                yield return null;
            }

            if (estaMuriendo) yield break;

            // 1. Se para recto y reproduce la animación entera hacia adelante (0 -> 1)
            EstablecerAnimacionEspecial(true);
            float duracionAnim = 0.5f;

            if (miAnimator != null)
            {
                miAnimator.enabled = true;
                miAnimator.speed = 1f;
                miAnimator.Play("Teleport", 0, 0f);
                miAnimator.Update(0f);

                duracionAnim = ObtenerDuracionAnimacion("Teleport");
                miAnimator.speed = 0f;

                float progreso = 0f;
                while (progreso < 1f)
                {
                    yield return null;
                    if (estaMuriendo) yield break;

                    float factorVel = congelado ? 0.6f : 1f;
                    progreso += (Time.deltaTime * factorVel) / duracionAnim;

                    miAnimator.Play("Teleport", 0, Mathf.Clamp01(progreso));
                    miAnimator.Update(0f);
                }
            }

            if (estaMuriendo) yield break;

            if (miAnimator != null) miAnimator.enabled = false;
            InstanciarParticulasTeleport(transform.position);
            ReproducirSonidoTeleport();
            EstablecerVisibilidad(false);

            AvanzarPorRuta(5f + Random.Range(0f, 1f)); // Avanza entre 5 y 6 unidades de distancia

            float tiempoOculto = 0f;
            while (tiempoOculto < 1f)
            {
                if (estaMuriendo) yield break;
                tiempoOculto += Time.deltaTime;
                yield return null;
            }

            if (estaMuriendo) yield break;

            // 3. Reaparece parado y reproduce exactamente la misma animación al revés (1 -> 0)
            EstablecerVisibilidad(true);
            InstanciarParticulasTeleport(transform.position);
            ReproducirSonidoTeleport();

            if (miAnimator != null)
            {
                miAnimator.enabled = true;
                miAnimator.speed = 0f;
                miAnimator.Play("Teleport", 0, 1f);
                miAnimator.Update(0f);

                float progreso = 1f;
                while (progreso > 0f)
                {
                    yield return null;
                    if (estaMuriendo) yield break;

                    float factorVel = congelado ? 0.6f : 1f;
                    progreso -= (Time.deltaTime * factorVel) / duracionAnim;

                    miAnimator.Play("Teleport", 0, Mathf.Clamp01(progreso));
                    miAnimator.Update(0f);
                }

                miAnimator.enabled = false;
            }

            // Vuelve al balanceo normal empezando exactamente recto desde 0 grados
            EstablecerAnimacionEspecial(false);
            tiempoEspera = 3.5f + Random.Range(0f, 1f);
        }
    }

    private void EstablecerVisibilidad(bool visible)
    {
        esInvisible = !visible;
        gameObject.tag = visible ? tagOriginal : "Untagged";

        if (todosLosColliders != null)
        {
            for (int i = 0; i < todosLosColliders.Length; i++)
            {
                if (todosLosColliders[i] != null) todosLosColliders[i].enabled = visible;
            }
        }

        if (todosLosSprites != null)
        {
            for (int i = 0; i < todosLosSprites.Length; i++)
            {
                if (todosLosSprites[i] != null) todosLosSprites[i].enabled = visible;
            }
        }

        if (scriptSombra != null)
        {
            scriptSombra.EstablecerVisible(visible);
        }

        if (canvasBarraVida != null)
        {
            bool debeMostrarBarra = visible && (vida < vidaMaxima) && (vida > 0f);
            canvasBarraVida.SetActive(debeMostrarBarra);
        }

        if (particulasHielo != null)
        {
            if (!visible)
            {
                particulasHielo.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particulasHielo.Clear(true);
            }
            else if (congelado)
            {
                particulasHielo.Clear(true);
                particulasHielo.Play(true);
            }
        }
    }

    private void AvanzarPorRuta(float distanciaRestante)
    {
        const float limiteSeguroX = 8.8f;

        if (ruta == null || ruta.pointCount == 0)
        {
            if (transform.position.x < limiteSeguroX)
            {
                float avance = Mathf.Min(distanciaRestante, limiteSeguroX - transform.position.x);
                transform.position += Vector3.right * avance;
                distanciaRecorrida += avance;
            }
            return;
        }

        while (distanciaRestante > 0f && indicePuntoActual < ruta.pointCount)
        {
            Vector3 puntoDestino = ruta.transform.TransformPoint(ruta.points[indicePuntoActual]) + desvioCamino;
            puntoDestino.z = 0f;

            float distHastaPunto = Vector2.Distance(transform.position, puntoDestino);

            if (distHastaPunto <= distanciaRestante)
            {
                if (transform.position.x < limiteSeguroX && puntoDestino.x > limiteSeguroX)
                {
                    float factor = (limiteSeguroX - transform.position.x) / (puntoDestino.x - transform.position.x);
                    Vector3 posLimite = Vector3.Lerp(transform.position, puntoDestino, Mathf.Clamp01(factor));
                    distanciaRecorrida += Vector3.Distance(transform.position, posLimite);
                    transform.position = posLimite;
                    return;
                }

                transform.position = puntoDestino;
                distanciaRecorrida += distHastaPunto;
                distanciaRestante -= distHastaPunto;
                indicePuntoActual++;
            }
            else
            {
                Vector3 nuevaPos = Vector3.MoveTowards(transform.position, puntoDestino, distanciaRestante);
                if (transform.position.x < limiteSeguroX && nuevaPos.x > limiteSeguroX)
                {
                    float factor = (limiteSeguroX - transform.position.x) / (nuevaPos.x - transform.position.x);
                    nuevaPos = Vector3.Lerp(transform.position, nuevaPos, Mathf.Clamp01(factor));
                }

                distanciaRecorrida += Vector3.Distance(transform.position, nuevaPos);
                transform.position = nuevaPos;
                distanciaRestante = 0f;
            }
        }
    }

    private void InstanciarParticulasTeleport(Vector3 posicion)
    {
        if (prefabParticulasTeleport == null) return;

        GameObject efecto = Instantiate(prefabParticulasTeleport, posicion, Quaternion.identity);
        ParticleSystem ps = efecto.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ps.Play();
            Destroy(efecto, ps.main.duration + ps.main.startLifetime.constantMax + 0.5f);
        }
        else
        {
            Destroy(efecto, 2f);
        }
    }

    private void ReproducirSonidoTeleport()
    {
        if (MenuPrincipal.instancia == null && sonidoTeleport != null)
        {
            GameObject audioTemporal = new GameObject("Sonido_Teleport");
            AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();
            fuenteAudio.clip = sonidoTeleport;
            fuenteAudio.spatialBlend = 0f;
            fuenteAudio.volume = ControladorJuego.volumenSFXGuardado;
            fuenteAudio.Play();
            Destroy(audioTemporal, sonidoTeleport.length + 0.1f);
        }
    }

    public void AplicarAturdimiento(float duracion)
    {
        if (estaMuriendo || esInvisible) return;

        if (duracion > tiempoRestanteAturdimiento)
        {
            tiempoRestanteAturdimiento = duracion;
        }
    }

    public void AplicarCongelacion()
    {
        if (estaMuriendo || esInvisible) return;

        tiempoRestanteCongelacion = 8f; 

        if (!congelado)
        {
            congelado = true;
            velocidad = velocidadOriginal * 0.5f; 

            if (particulasHielo != null) particulasHielo.Play(true);

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
        if (estaMuriendo || esInvisible) return;

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

            if (rutinaCientifico != null) StopCoroutine(rutinaCientifico);
            if (miAnimator != null) miAnimator.enabled = false;
            
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
        if (canvasBarraVida != null && !canvasBarraVida.activeSelf && vida > 0 && !esInvisible) 
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