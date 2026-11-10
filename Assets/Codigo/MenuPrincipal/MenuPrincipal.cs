using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuPrincipal : MonoBehaviour
{
    public static MenuPrincipal instancia;

    [Header("Cámara")]
    public Camera camaraPrincipal;
    public float posicionXCamara = -5.4f;

    [Header("Paneles UI")]
    public GameObject panelMenu; 
    public GameObject panelNiveles;
    public GameObject panelTransicion;

    [Header("Tiempos de Transición")]
    private float duracionFadeOscurecer = 0.6f;
    private float duracionFadeAclarar = 0.4f;

    [Header("Música del Menú")]
    public AudioClip musicaMenu;
    private AudioSource fuenteMusica;
    private const float escalaVolumenMenu = 0.8f;

    [HideInInspector] public bool enTransicion = false;
    [HideInInspector] public bool nivelSeleccionado = false;
    
    private EfectoBoton botonNivelActual;
    private Image imagenTransicion;

    void Awake()
    {
        if (instancia == null) instancia = this;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (camaraPrincipal == null) camaraPrincipal = Camera.main;
        FijarCamara();

        if (panelNiveles != null) panelNiveles.SetActive(false);
        if (panelMenu != null) panelMenu.SetActive(true);

        if (panelTransicion != null)
        {
            imagenTransicion = panelTransicion.GetComponent<Image>();
            panelTransicion.SetActive(true);
            if (imagenTransicion != null)
            {
                Color c = imagenTransicion.color;
                c.a = 1f;
                imagenTransicion.color = c;
            }
        }
    }

    void Start()
    {
        if (musicaMenu != null)
        {
            fuenteMusica = gameObject.AddComponent<AudioSource>();
            fuenteMusica.clip = musicaMenu;
            fuenteMusica.loop = true;
            fuenteMusica.playOnAwake = false;
            fuenteMusica.spatialBlend = 0f;
            fuenteMusica.ignoreListenerPause = true;
            fuenteMusica.volume = ControladorJuego.volumenMusicaGuardada * escalaVolumenMenu; 
            fuenteMusica.Play();
        }

        StartCoroutine(RutinaAclararTransicion());
    }

    void Update()
    {
        if (fuenteMusica != null)
        {
            fuenteMusica.volume = ControladorJuego.volumenMusicaGuardada * escalaVolumenMenu;
        }
    }

    void LateUpdate()
    {
        FijarCamara();
    }

    private void FijarCamara()
    {
        if (camaraPrincipal != null)
        {
            Vector3 pos = camaraPrincipal.transform.position;
            pos.x = posicionXCamara;
            camaraPrincipal.transform.position = pos;
        }
    }

    public void SeleccionarNivel(EfectoBoton botonNivel)
    {
        if (enTransicion) return;

        if (botonNivelActual != null && botonNivelActual != botonNivel)
        {
            botonNivelActual.ForzarDeseleccion();
        }

        botonNivelActual = botonNivel;
        nivelSeleccionado = true;
    }

    private void ResetearSeleccionNivel()
    {
        nivelSeleccionado = false;
        if (botonNivelActual != null)
        {
            botonNivelActual.ForzarDeseleccion();
            botonNivelActual = null;
        }
    }

    public void BotonJugar()
    {
        if (enTransicion) return;
        BloquearBotonPulsado();
        StartCoroutine(RutinaAbrirPanelNiveles());
    }

    public void BotonSalir()
    {
        if (enTransicion) return;
        BloquearBotonPulsado();
        StartCoroutine(RutinaOscurecerYSalir());
    }

    public void BotonVolverAlMenu()
    {
        if (enTransicion) return;
        if (botonNivelActual != null) botonNivelActual.ForzarDeseleccion();
        BloquearBotonPulsado();
        StartCoroutine(RutinaVolverAlMenu());
    }

    public void BotonEmpezarPartida()
    {
        if (enTransicion || !nivelSeleccionado) return;
        if (botonNivelActual != null) botonNivelActual.ForzarDeseleccion();
        BloquearBotonPulsado();
        StartCoroutine(RutinaOscurecerYEmpezar());
    }

    private void BloquearBotonPulsado()
    {
        if (EfectoBoton.ultimoBotonPulsado != null)
        {
            EfectoBoton.ultimoBotonPulsado.forzadoActivo = true;
        }
        else if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            EfectoBoton ef = EventSystem.current.currentSelectedGameObject.GetComponent<EfectoBoton>();
            if (ef != null) ef.forzadoActivo = true;
        }
    }

    private IEnumerator RutinaAclararTransicion()
    {
        enTransicion = true;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

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

    private IEnumerator RutinaAbrirPanelNiveles()
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

        yield return new WaitForSecondsRealtime(0.5f);

        if (EfectoBoton.ultimoBotonPulsado != null)
        {
            EfectoBoton.ultimoBotonPulsado.ForzarDeseleccion();
        }
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        ResetearSeleccionNivel();

        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelNiveles != null) panelNiveles.SetActive(true);

        if (panelTransicion != null && imagenTransicion != null)
        {
            Color c = imagenTransicion.color;
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

    private IEnumerator RutinaVolverAlMenu()
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

        yield return new WaitForSecondsRealtime(0.5f);

        if (EfectoBoton.ultimoBotonPulsado != null)
        {
            EfectoBoton.ultimoBotonPulsado.ForzarDeseleccion();
        }
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        ResetearSeleccionNivel();

        if (panelNiveles != null) panelNiveles.SetActive(false);
        if (panelMenu != null) panelMenu.SetActive(true);

        if (panelTransicion != null && imagenTransicion != null)
        {
            Color c = imagenTransicion.color;
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

    private IEnumerator RutinaOscurecerYEmpezar()
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

        yield return new WaitForSecondsRealtime(0.5f);

        AsyncOperation operacionCarga = SceneManager.LoadSceneAsync("Juego");
        
        while (!operacionCarga.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator RutinaOscurecerYSalir()
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

        yield return new WaitForSecondsRealtime(1f);

        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}