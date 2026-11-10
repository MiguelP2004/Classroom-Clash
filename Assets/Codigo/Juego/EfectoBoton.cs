using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class EfectoBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler
{
    public static EfectoBoton ultimoBotonPulsado;
    public static EfectoBoton ultimoNivelSeleccionado;

    [Header("Configuración")]
    public int coste = 20;
    public bool esMenu = false; 
    
    [Header("Niveles")]
    public bool esNivel = false;
    public int numeroDeNivel = 1;
    public bool esBotonEmpezar = false;

    [Header("Audio")]
    public AudioClip sonidoBoton;
    public AudioClip sonidoError;
    
    [HideInInspector] public bool bloqueadoPorTutorial = false;
    [HideInInspector] public bool forzadoActivo = false;
    [HideInInspector] public bool compradoPermanente = false;

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
    private TextMeshProUGUI miTextoHijo;

    private float tiempoParpadeoError = 0f;
    private const float duracionError = 0.24f;
    
    private bool nivelBloqueado = false;
    
    private Sprite spriteNormalOriginal;
    private Sprite spriteIluminadoOriginal;

    void Awake()
    {
        miBoton = GetComponent<Button>();
        miImagenBase = GetComponent<Image>();
        miTextoHijo = GetComponentInChildren<TextMeshProUGUI>();
        if (imagenCooldown != null) imagenCooldown.fillAmount = 0f;

        if (miBoton != null)
        {
            spriteNormalOriginal = miBoton.image.sprite;
            spriteIluminadoOriginal = miBoton.spriteState.highlightedSprite;
        }
    }

    void Start()
    {
        transform.localScale = new Vector3(tamañoNormal, tamañoNormal, 0f);
        VerificarBloqueoNivel();
    }

    void OnEnable()
    {
        VerificarBloqueoNivel();
    }

    void OnDisable()
    {
        ratonEncima = false;
        estaSeleccionado = false;
        forzadoActivo = false;
        tiempoParpadeoError = 0f;
        transform.localScale = new Vector3(tamañoNormal, tamañoNormal, 0f);
        
        if (esNivel)
        {
            if (miImagenBase != null && spriteNormalOriginal != null)
            {
                miImagenBase.sprite = spriteNormalOriginal; 
            }
            
            if (ultimoNivelSeleccionado == this)
            {
                ultimoNivelSeleccionado = null;
            }
        }
    }
    
    private void VerificarBloqueoNivel()
    {
        if (esNivel)
        {
            int progresoActual = PlayerPrefs.GetInt("NivelActual", 1);
            nivelBloqueado = (numeroDeNivel > progresoActual);
        }
        else
        {
            nivelBloqueado = false;
        }
    }

    private bool EstaBloqueadoPorFaltaDeNivel()
    {
        return esBotonEmpezar && MenuPrincipal.instancia != null && !MenuPrincipal.instancia.nivelSeleccionado;
    }

    private bool EstaEnTransicionGlobal()
    {
        bool transicionJuego = (ControladorJuego.instancia != null && ControladorJuego.instancia.enTransicion);
        bool transicionMenu = (MenuPrincipal.instancia != null && MenuPrincipal.instancia.enTransicion);
        return transicionJuego || transicionMenu;
    }

    void Update()
    {
        bool juegoPausado = (Time.timeScale == 0f);
        bool bloqueadoPorPausa = (juegoPausado && !esMenu); 
        bool enTransicion = EstaEnTransicionGlobal();
        bool bloqueadoPorTransicion = (enTransicion && !forzadoActivo);
        bool puedeComprar = esMenu || (coste <= 0) || (ControladorJuego.instancia != null && ControladorJuego.instancia.PuedeComprar(coste));
        bool sinNivelElegido = EstaBloqueadoPorFaltaDeNivel();

        Color colorEstado = Color.white;

        if (sinNivelElegido)
        {
            colorEstado = new Color(1f, 1f, 1f, 0.65f);
            if (miBoton != null) miBoton.interactable = false;
            ratonEncima = false;
            estaSeleccionado = false;
        }
        else if (nivelBloqueado)
        {
            colorEstado = new Color(0.45f, 0.45f, 0.45f, 0.95f);
            if (miBoton != null) miBoton.interactable = false;
            ratonEncima = false;
            estaSeleccionado = false;
        }
        else if (compradoPermanente)
        {
            if (imagenCooldown != null) imagenCooldown.fillAmount = 0f;
            colorEstado = new Color(0.65f, 0.65f, 0.65f, 1f);
            if (miBoton != null) miBoton.interactable = false;
            ratonEncima = false;
            estaSeleccionado = false;
        }
        else if (enCooldown)
        {
            temporizador -= Time.deltaTime;
            
            if (imagenCooldown != null) imagenCooldown.fillAmount = temporizador / tiempoCooldown;
            colorEstado = new Color(0.8f, 0.8f, 0.8f, 1f);
            if (miBoton != null) miBoton.interactable = false;

            if (temporizador <= 0f) enCooldown = false;
        }
        else 
        {
            if (bloqueadoPorTutorial || !puedeComprar || bloqueadoPorPausa || bloqueadoPorTransicion)
            {
                if (imagenCooldown != null) imagenCooldown.fillAmount = 0f; 
                colorEstado = (!puedeComprar) ? new Color(0.8f, 0.8f, 0.8f, 1f) : Color.white; 
                if (miBoton != null) miBoton.interactable = false; 
                
                if (!(esNivel && ultimoNivelSeleccionado == this))
                {
                    ratonEncima = false;
                    estaSeleccionado = false;
                }
            }
            else
            {
                if (imagenCooldown != null) imagenCooldown.fillAmount = 0f;
                colorEstado = Color.white;
                if (miBoton != null) miBoton.interactable = true;
            }
        }

        if (esBotonEmpezar && miTextoHijo != null)
        {
            Color cTexto = miTextoHijo.color;
            cTexto.a = sinNivelElegido ? 0.65f : 1f;
            miTextoHijo.color = cTexto;
        }

        if (tiempoParpadeoError > 0f && !compradoPermanente && !nivelBloqueado && !sinNivelElegido)
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
        
        bool esElNivelActivo = (esNivel && ultimoNivelSeleccionado == this);
        bool debeResaltar = (!sinNivelElegido && !nivelBloqueado && !compradoPermanente && !bloqueadoPorTutorial && !enCooldown && puedeComprar && !bloqueadoPorPausa);
        
        if ((debeResaltar && (ratonEncima || estaSeleccionado)) || forzadoActivo || esElNivelActivo)
        {
            objetivo = new Vector3(tamañoAgrandado, tamañoAgrandado, 0f);
            
            // Forzamos el sprite iluminado por código solo para el nivel activo, ignorando el Sprite Swap
            if (esElNivelActivo && miImagenBase != null && spriteIluminadoOriginal != null)
            {
                miImagenBase.sprite = spriteIluminadoOriginal;
            }
        }
        else
        {
            // Si no está activo y es un nivel, devolvemos el control al Sprite Swap de Unity restaurando el original
            if (esNivel && miImagenBase != null && spriteNormalOriginal != null)
            {
                miImagenBase.sprite = spriteNormalOriginal;
            }
        }
        
        transform.localScale = Vector3.Lerp(transform.localScale, objetivo, Time.unscaledDeltaTime * 15f);

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

    public void ForzarDeseleccion()
    {
        forzadoActivo = false;
        estaSeleccionado = false;
        ratonEncima = false;
        
        if (esNivel && ultimoNivelSeleccionado == this)
        {
            ultimoNivelSeleccionado = null;
            if (miImagenBase != null && spriteNormalOriginal != null)
            {
                miImagenBase.sprite = spriteNormalOriginal;
            }
        }
    }

    public void IniciarCooldown()
    {
        enCooldown = true;
        temporizador = tiempoCooldown;
        
        if (miBoton != null) miBoton.interactable = false; 
        if (imagenCooldown != null) imagenCooldown.fillAmount = 1f; 
        if (miImagenBase != null) miImagenBase.color = new Color(0.8f, 0.8f, 0.8f, 1f); 
        
        ratonEncima = false;
        estaSeleccionado = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        CodigoProfe.BloquearDisparoHastaSoltar();
        
        if (compradoPermanente || nivelBloqueado || EstaBloqueadoPorFaltaDeNivel()) return;
        if (EstaEnTransicionGlobal()) return;

        if (!esBotonEmpezar)
        {
            ultimoBotonPulsado = this;
        }

        if (esNivel)
        {
            if (ultimoNivelSeleccionado != null && ultimoNivelSeleccionado != this)
            {
                ultimoNivelSeleccionado.estaSeleccionado = false;
                // Devolvemos el sprite original al nivel deseleccionado
                if (ultimoNivelSeleccionado.miImagenBase != null && ultimoNivelSeleccionado.spriteNormalOriginal != null)
                {
                    ultimoNivelSeleccionado.miImagenBase.sprite = ultimoNivelSeleccionado.spriteNormalOriginal;
                }
            }
            
            ultimoNivelSeleccionado = this;
            estaSeleccionado = true;
            
            if (MenuPrincipal.instancia != null)
            {
                MenuPrincipal.instancia.SeleccionarNivel(this);
            }
        }

        bool juegoPausado = (Time.timeScale == 0f);
        bool bloqueadoPorPausa = (juegoPausado && !esMenu);
        bool enTransicion = EstaEnTransicionGlobal();
        bool esGameOver = (ControladorJuego.instancia != null && ControladorJuego.instancia.esGameOver);

        if (!esMenu && !bloqueadoPorTutorial && !bloqueadoPorPausa && !enTransicion && !esGameOver)
        {
            bool puedeComprar = (coste <= 0) || (ControladorJuego.instancia != null && ControladorJuego.instancia.PuedeComprar(coste));

            if (!puedeComprar || enCooldown)
            {
                tiempoParpadeoError = duracionError;
                AudioClip clip = (sonidoError != null) ? sonidoError : sonidoBoton;
                if (clip != null)
                {
                    GameObject audioTemporal = new GameObject("Sonido_Error");
                    AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();

                    fuenteAudio.clip = clip;
                    fuenteAudio.spatialBlend = 0f;
                    fuenteAudio.volume = ControladorJuego.volumenSFXGuardado;
                    fuenteAudio.ignoreListenerPause = true;

                    fuenteAudio.Play();
                    Destroy(audioTemporal, clip.length + 0.1f);
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) 
    { 
        if (!EstaBloqueadoPorFaltaDeNivel() && !nivelBloqueado && !bloqueadoPorTutorial && !compradoPermanente && !EstaEnTransicionGlobal()) 
        {
            ratonEncima = true; 
        }
    }
    
    public void OnPointerExit(PointerEventData eventData) 
    { 
        ratonEncima = false; 
    }
    
    public void OnSelect(BaseEventData eventData) 
    { 
        if (!EstaBloqueadoPorFaltaDeNivel() && !nivelBloqueado && !bloqueadoPorTutorial && !compradoPermanente && !EstaEnTransicionGlobal()) 
        {
            estaSeleccionado = true; 
        }
    }
    
    public void OnDeselect(BaseEventData eventData) 
    { 
        if (!forzadoActivo) 
        {
            if (esNivel && ultimoNivelSeleccionado == this)
            {
            }
            else
            {
                estaSeleccionado = false; 
            }
        }
    }

    public void ReproducirSonidoClic()
    {
        if (EstaEnTransicionGlobal() && !forzadoActivo && !esBotonEmpezar) return;

        if (esMenu && !nivelBloqueado && !EstaBloqueadoPorFaltaDeNivel() && sonidoBoton != null && miBoton != null && miBoton.interactable)
        {
            GameObject audioTemporal = new GameObject("SonidoClic_UI");
            AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();
            
            fuenteAudio.clip = sonidoBoton;
            fuenteAudio.spatialBlend = 0f; 
            fuenteAudio.volume = ControladorJuego.volumenSFXGuardado;
            fuenteAudio.ignoreListenerPause = true;

            fuenteAudio.Play();
            
            DontDestroyOnLoad(audioTemporal); 
            Destroy(audioTemporal, sonidoBoton.length + 0.1f);
        }
    }
}