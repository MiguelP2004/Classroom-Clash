using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EfectoBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler
{
    public static EfectoBoton ultimoBotonPulsado;

    [Header("Configuración")]
    public int coste = 20;
    public bool esMenu = false; 
    public AudioClip sonidoBoton;
    public AudioClip sonidoError; // Arrastra aquí tu error.wav para los botones de gameplay
    [HideInInspector] public bool bloqueadoPorTutorial = false;
    [HideInInspector] public bool forzadoActivo = false;

    [Header("Escala")]
    public float tamañoNormal = 1f;
    public float tamañoAgrandado = 1.05f;

    [Header("Cooldown")]
    public float tiempoCooldown = 5f;
    public Image imagenCooldown; 
    
    private bool ratonEncima = false;
    private bool estaSeleccionado = false;
    
    private bool enCooldown = false;
    private float temporizador = 0f;
    private Button miBoton;
    private Image miImagenBase;

    private float tiempoParpadeoError = 0f;
    private const float duracionError = 0.24f;

    void Awake()
    {
        miBoton = GetComponent<Button>();
        miImagenBase = GetComponent<Image>();
        if (imagenCooldown != null) imagenCooldown.fillAmount = 0f;
    }

    void Start()
    {
        transform.localScale = new Vector3(tamañoNormal, tamañoNormal, 0f);
    }

    void OnDisable()
    {
        ratonEncima = false;
        estaSeleccionado = false;
        forzadoActivo = false;
        tiempoParpadeoError = 0f;
        transform.localScale = new Vector3(tamañoNormal, tamañoNormal, 0f);
    }

    void Update()
    {
        bool juegoPausado = (Time.timeScale == 0f);
        bool bloqueadoPorPausa = (juegoPausado && !esMenu); 
        bool enTransicion = (ControladorJuego.instancia != null && ControladorJuego.instancia.enTransicion);
        bool bloqueadoPorTransicion = (enTransicion && !forzadoActivo);
        bool puedeComprar = (coste <= 0) || (ControladorJuego.instancia != null && ControladorJuego.instancia.PuedeComprar(coste));

        Color colorEstado = Color.white;

        if (enCooldown)
        {
            temporizador -= Time.deltaTime;
            
            if (imagenCooldown != null) imagenCooldown.fillAmount = temporizador / tiempoCooldown;
            colorEstado = new Color(0.7f, 0.7f, 0.7f, 1f);
            if (miBoton != null) miBoton.interactable = false;

            if (temporizador <= 0f) enCooldown = false;
        }
        else 
        {
            if (bloqueadoPorTutorial || !puedeComprar || bloqueadoPorPausa || bloqueadoPorTransicion)
            {
                if (imagenCooldown != null) imagenCooldown.fillAmount = 0f; 
                colorEstado = (!puedeComprar) ? new Color(0.7f, 0.7f, 0.7f, 1f) : Color.white; 
                if (miBoton != null) miBoton.interactable = false; 
                
                ratonEncima = false;
                estaSeleccionado = false;
            }
            else
            {
                if (imagenCooldown != null) imagenCooldown.fillAmount = 0f;
                colorEstado = Color.white;
                if (miBoton != null) miBoton.interactable = true;
            }
        }

        if (tiempoParpadeoError > 0f)
        {
            tiempoParpadeoError -= Time.deltaTime;
            int fase = Mathf.FloorToInt((tiempoParpadeoError / duracionError) * 4f);
            if (miImagenBase != null)
            {
                miImagenBase.color = (fase == 3 || fase == 1) ? Color.red : colorEstado;
            }
        }
        else
        {
            if (miImagenBase != null) miImagenBase.color = colorEstado;
        }

        Vector3 objetivo = new Vector3(tamañoNormal, tamañoNormal, 0f);
        
        if (!bloqueadoPorTutorial && !enCooldown && puedeComprar && !bloqueadoPorPausa && !bloqueadoPorTransicion && (ratonEncima || estaSeleccionado || forzadoActivo))
        {
            objetivo = new Vector3(tamañoAgrandado, tamañoAgrandado, 0f);
        }
        
        transform.localScale += (objetivo - transform.localScale) / 25f;

        if (forzadoActivo && EventSystem.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                miBoton.Select(); 
            }
        }
    }

    void LateUpdate()
    {
        if (forzadoActivo && EventSystem.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                miBoton.Select();
            }
        }
    }

    public void IniciarCooldown()
    {
        enCooldown = true;
        temporizador = tiempoCooldown;
        
        if (miBoton != null) miBoton.interactable = false; 
        if (imagenCooldown != null) imagenCooldown.fillAmount = 1f; 
        if (miImagenBase != null) miImagenBase.color = new Color(0.7f, 0.7f, 0.7f, 1f); 
        
        ratonEncima = false;
        estaSeleccionado = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        CodigoProfe.BloquearDisparoHastaSoltar();
        ultimoBotonPulsado = this;

        bool juegoPausado = (Time.timeScale == 0f);
        bool bloqueadoPorPausa = (juegoPausado && !esMenu);
        bool enTransicion = (ControladorJuego.instancia != null && ControladorJuego.instancia.enTransicion);
        bool esGameOver = (ControladorJuego.instancia != null && ControladorJuego.instancia.esGameOver);

        if (!esMenu && !bloqueadoPorTutorial && !bloqueadoPorPausa && !enTransicion && !esGameOver)
        {
            bool puedeComprar = (coste <= 0) || (ControladorJuego.instancia != null && ControladorJuego.instancia.PuedeComprar(coste));

            if (!puedeComprar || enCooldown)
            {
                tiempoParpadeoError = duracionError;
                AudioClip clip = (sonidoError != null) ? sonidoError : sonidoBoton;
                if (clip != null && Camera.main != null)
                {
                    AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { if (!bloqueadoPorTutorial) ratonEncima = true; }
    public void OnPointerExit(PointerEventData eventData) { ratonEncima = false; }
    public void OnSelect(BaseEventData eventData) { if (!bloqueadoPorTutorial) estaSeleccionado = true; }
    public void OnDeselect(BaseEventData eventData) { if (!forzadoActivo) estaSeleccionado = false; }

    public void ReproducirSonidoClic()
    {
        if (ControladorJuego.instancia != null && ControladorJuego.instancia.enTransicion && !forzadoActivo) return;

        if (esMenu && sonidoBoton != null && miBoton != null && miBoton.interactable)
        {
            GameObject audioTemporal = new GameObject("SonidoClic_UI");
            AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();
            
            fuenteAudio.clip = sonidoBoton;
            fuenteAudio.spatialBlend = 0f; 
            fuenteAudio.ignoreListenerPause = true;

            if (ConfiguracionAudio.Instancia != null) ConfiguracionAudio.Instancia.ConfigurarFuenteSFX(fuenteAudio);
            
            fuenteAudio.Play();
            
            DontDestroyOnLoad(audioTemporal); 
            
            Destroy(audioTemporal, sonidoBoton.length + 0.1f);
        }
    }
}