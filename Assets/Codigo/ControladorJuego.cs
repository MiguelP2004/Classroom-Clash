using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego instancia;

    [Header("Energía")]
    public int energia = 50;
    public TextMeshProUGUI textoEnergia; 
    private float tamañoAgrandado = 1.35f;

    [Header("Vidas")]
    public int vidas = 12;
    public TextMeshProUGUI textoVidas;

    [Header("Cámara")]
    public Camera camaraPrincipal;
    [HideInInspector] public bool camaraBloqueadaPorTutorial = false;
    
    private float velocidadCamara = 7f;
    private float suavizadoCamara = 12f;
    private float margenBorde = 0.15f;
    private float limiteIzquierdo = -5.4f; 
    private float limiteDerecho = 0f;

    private Vector3 posicionObjetivoCamara;

    [Header("Pausa y Audio")]
    public GameObject botonPausa; 
    public GameObject panelPausa; 
    public RectTransform panelBarrido; 
    public Slider sliderSonidoFX;
    public Slider sliderMusica;

    [Header("Música")]
    public AudioClip musicaJuego;
    private AudioSource fuenteMusica;
    
    private float posicionY_Oculta = 1200f; 
    private float posicionY_Visible = 0f;  
    
    private bool juegoPausado = false;
    private bool animandoBarrido = false;
    private float velocidadBarrido = 0f;

    [Header("Game Over y Transición")]
    public GameObject panelGameOver;
    public GameObject panelTransicion;
    public AudioClip sonidoGameOver;
    
    private float duracionFadeOscurecer = 1f;
    private float duracionFadeAclarar = 0.8f;

    public AudioClip pausar;

    [HideInInspector] public bool esGameOver = false;
    [HideInInspector] public bool enTransicion = false;
    private Image imagenTransicion;

    private static bool viniendoDeReinicio = false;

    void Awake()
    {
        if (instancia == null) instancia = this;
        
        AudioListener.pause = false; 

        ActualizarTextoEnergia();
        ActualizarTextoVidas();
        
        if (camaraPrincipal == null) camaraPrincipal = Camera.main;
        if (camaraPrincipal != null) posicionObjetivoCamara = camaraPrincipal.transform.position;

        Time.timeScale = 1f;
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);

        if (panelBarrido != null)
        {
            Vector2 pos = panelBarrido.anchoredPosition;
            pos.y = posicionY_Oculta;
            panelBarrido.anchoredPosition = pos;
            panelBarrido.gameObject.SetActive(false);
        }
        if (botonPausa != null) botonPausa.SetActive(true);

        if (panelTransicion != null)
        {
            imagenTransicion = panelTransicion.GetComponent<Image>();
            if (viniendoDeReinicio)
            {
                panelTransicion.SetActive(true);
                if (imagenTransicion != null)
                {
                    Color c = imagenTransicion.color;
                    c.a = 1f;
                    imagenTransicion.color = c;
                }
            }
            else
            {
                panelTransicion.SetActive(false);
            }
        }
    }

    void Start()
    {
        if (musicaJuego != null)
        {
            fuenteMusica = gameObject.AddComponent<AudioSource>();

            fuenteMusica.clip = musicaJuego;
            fuenteMusica.loop = true;
            fuenteMusica.playOnAwake = false;
            fuenteMusica.spatialBlend = 0f;

            if (ConfiguracionAudio.Instancia != null)
            {
                ConfiguracionAudio.Instancia.ConfigurarFuenteMusica(fuenteMusica);
            }

            fuenteMusica.Play();
        }
        if (sliderSonidoFX != null)
        {
            sliderSonidoFX.onValueChanged.AddListener(CambiarVolumenFX);
            sliderSonidoFX.value = ConfiguracionAudio.Instancia != null
                ? ConfiguracionAudio.Instancia.ObtenerVolumenSFX()
                : 1f;
        }

        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);
            sliderMusica.value = ConfiguracionAudio.Instancia != null
                ? ConfiguracionAudio.Instancia.ObtenerVolumenMusica()
                : 1f;
        }
        if (viniendoDeReinicio)
        {
            viniendoDeReinicio = false;
            StartCoroutine(RutinaAclararTransicion());
        }
    }

    void Update()
    {
        if (!juegoPausado)
        {
            ManejarCamara();
        }

        if (panelBarrido != null && animandoBarrido)
        {
            float objetivoY = juegoPausado ? posicionY_Visible : posicionY_Oculta;
            Vector2 posActual = panelBarrido.anchoredPosition;
            
            posActual.y = Mathf.SmoothDamp(posActual.y, objetivoY, ref velocidadBarrido, 0.1f, Mathf.Infinity, Time.unscaledDeltaTime);
            panelBarrido.anchoredPosition = posActual;

            if (Mathf.Abs(posActual.y - objetivoY) < 1f)
            {
                posActual.y = objetivoY;
                panelBarrido.anchoredPosition = posActual;

                if (!juegoPausado) 
                {
                    panelBarrido.gameObject.SetActive(false);
                }
                animandoBarrido = false;
            }
        }

        if (textoEnergia != null)
        {
            textoEnergia.transform.localScale = Vector3.Lerp(textoEnergia.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 15f);
        }

        if (textoVidas != null)
        {
            textoVidas.transform.localScale = Vector3.Lerp(textoVidas.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 15f);
        }
    }

    public void CambiarVolumenFX(float nuevoValor)
    {
        if (ConfiguracionAudio.Instancia != null)
        {
            ConfiguracionAudio.Instancia.CambiarVolumenSFX(nuevoValor);
        }
    }

    public void CambiarVolumenMusica(float nuevoValor)
    {
        if (ConfiguracionAudio.Instancia != null)
        {
            ConfiguracionAudio.Instancia.CambiarVolumenMusica(nuevoValor);
        }
    }

    private void ManejarCamara()
    {
        if (camaraPrincipal == null || Mouse.current == null) return;

        if (!camaraBloqueadaPorTutorial)
        {
            Vector2 posRaton = Mouse.current.position.ReadValue();
            float anchoPantalla = Screen.width;
            float altoPantalla = Screen.height; 

            if (posRaton.y < (altoPantalla * 0.8f) && posRaton.y > (altoPantalla * 0.2f))
            {
                if (posRaton.x < anchoPantalla * margenBorde)
                {
                    posicionObjetivoCamara.x -= velocidadCamara * Time.deltaTime; 
                }
                else if (posRaton.x > anchoPantalla * (1f - margenBorde))
                {
                    posicionObjetivoCamara.x += velocidadCamara * Time.deltaTime;
                }
            }

            posicionObjetivoCamara.x = Mathf.Clamp(posicionObjetivoCamara.x, limiteIzquierdo, limiteDerecho);
        }

        Vector3 posicionActual = camaraPrincipal.transform.position;
        float posXAnterior = posicionActual.x;

        posicionActual.x = Mathf.Lerp(posicionActual.x, posicionObjetivoCamara.x, Time.deltaTime * suavizadoCamara); 
        camaraPrincipal.transform.position = posicionActual;

        float desplazamiento = Mathf.Abs(posicionActual.x - posXAnterior);
        if (!camaraBloqueadaPorTutorial && desplazamiento > 0.001f && GestorTutorial.instancia != null)
        {
            GestorTutorial.instancia.RegistrarMovimientoCamara(desplazamiento);
        }
    }

    public void AlternarPausa()
    {
        CodigoProfe.BloquearDisparoHastaSoltar();

        if (esGameOver || enTransicion) return;

        if (juegoPausado) ReanudarJuego();
        else
        {
            if (pausar != null)
            {
                GameObject audioTemporal = new GameObject("Sonido_Pausa");
                AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();

                fuenteAudio.clip = pausar;
                fuenteAudio.spatialBlend = 0f;

                if (ConfiguracionAudio.Instancia != null)
                {
                    ConfiguracionAudio.Instancia.ConfigurarFuenteSFX(fuenteAudio);
                }

                fuenteAudio.Play();
                Destroy(audioTemporal, pausar.length + 0.1f);
            }

            PausarJuego();
        }
    }

    private void PausarJuego()
    {
        juegoPausado = true;
        Time.timeScale = 0f; 
        
        if (botonPausa != null) botonPausa.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(true); 
        
        if (panelBarrido != null)
        {
            panelBarrido.gameObject.SetActive(true);
            Vector2 pos = panelBarrido.anchoredPosition;
            pos.y = posicionY_Oculta;
            panelBarrido.anchoredPosition = pos;
            
            velocidadBarrido = 0f; 
            animandoBarrido = true;
        }
    }

    private void ReanudarJuego()
    {
        juegoPausado = false;
        Time.timeScale = 1f; 
        
        if (panelPausa != null) panelPausa.SetActive(false); 
        if (botonPausa != null) botonPausa.SetActive(true);
        if (panelBarrido != null) 
        {
            velocidadBarrido = 0f;
            animandoBarrido = true; 
        }
    }

    public void SumarEnergia(int cantidad)
    {
        energia += cantidad;
        EfectoCambioEnergia();
    }

    public void GastarEnergia(int cantidad)
    {
        energia -= cantidad;
        EfectoCambioEnergia();
    }

    public bool PuedeComprar(int coste) { return energia >= coste; }

    private void EfectoCambioEnergia()
    {
        if (textoEnergia != null)
        {
            textoEnergia.text = energia.ToString();
            textoEnergia.transform.localScale = new Vector3(tamañoAgrandado, tamañoAgrandado, 1f);
        }
    }

    private void ActualizarTextoEnergia()
    {
        if (textoEnergia != null) textoEnergia.text = energia.ToString();
    }

    public void RestarVida(int cantidad)
    {
        if (esGameOver) return;

        vidas -= cantidad;
        if (vidas < 0) vidas = 0;
        EfectoCambioVidas();

        if (vidas == 0)
        {
            ActivarGameOver();
        }
    }

    private void EfectoCambioVidas()
    {
        if (textoVidas != null)
        {
            textoVidas.text = vidas.ToString();
            textoVidas.transform.localScale = new Vector3(tamañoAgrandado, tamañoAgrandado, 1f);
        }
    }

    private void ActualizarTextoVidas()
    {
        if (textoVidas != null) textoVidas.text = vidas.ToString();
    }

    private void ActivarGameOver()
    {
        esGameOver = true;
        AudioListener.pause = true; 

        if (botonPausa != null) botonPausa.SetActive(false);

        if (GestorColocacion.instancia != null) GestorColocacion.instancia.CancelarAccion();

        if (panelGameOver != null) panelGameOver.SetActive(true);

        if (sonidoGameOver != null)
        {
            GameObject audioTemporal = new GameObject("Sonido_GameOver");
            AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();

            fuenteAudio.clip = sonidoGameOver;
            fuenteAudio.spatialBlend = 0f;
            fuenteAudio.ignoreListenerPause = true;

            if (ConfiguracionAudio.Instancia != null)
            {
                ConfiguracionAudio.Instancia.ConfigurarFuenteSFX(fuenteAudio);
            }

            fuenteAudio.Play();
            Destroy(audioTemporal, sonidoGameOver.length + 0.1f);
        }
    }

    public void ReiniciarPartida()
    {
        if (enTransicion) return;

        if (EfectoBoton.ultimoBotonPulsado != null)
        {
            EfectoBoton.ultimoBotonPulsado.forzadoActivo = true;
        }
        else if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            EfectoBoton ef = EventSystem.current.currentSelectedGameObject.GetComponent<EfectoBoton>();
            if (ef != null) ef.forzadoActivo = true;
        }

        StartCoroutine(RutinaOscurecerYReiniciar());
    }

    private IEnumerator RutinaOscurecerYReiniciar()
    {
        enTransicion = true;

        if (panelTransicion != null && imagenTransicion != null)
        {
            panelTransicion.SetActive(true);
            Color c = imagenTransicion.color;
            c.a = 0f;
            imagenTransicion.color = c;

            float tiempo = 0f;
            while (tiempo < duracionFadeOscurecer)
            {
                tiempo += Time.unscaledDeltaTime;
                c.a = Mathf.Clamp01(tiempo / duracionFadeOscurecer);
                imagenTransicion.color = c;
                yield return null;
            }

            c.a = 1f;
            imagenTransicion.color = c;
        }

        yield return new WaitForSecondsRealtime(1.5f);

        viniendoDeReinicio = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator RutinaAclararTransicion()
    {
        enTransicion = true;

        if (panelTransicion != null && imagenTransicion != null)
        {
            Color c = imagenTransicion.color;
            c.a = 1f;
            imagenTransicion.color = c;

            float tiempo = 0f;
            while (tiempo < duracionFadeAclarar)
            {
                tiempo += Time.unscaledDeltaTime;
                c.a = 1f - Mathf.Clamp01(tiempo / duracionFadeAclarar);
                imagenTransicion.color = c;
                yield return null;
            }

            c.a = 0f;
            imagenTransicion.color = c;
            panelTransicion.SetActive(false);
        }
        enTransicion = false;
    }
}