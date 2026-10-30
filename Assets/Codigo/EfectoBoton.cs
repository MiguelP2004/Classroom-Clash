using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EfectoBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Configuración")]
    public int coste = 20;
    public bool esMenu = false; 
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

    // Asegura que al ocultar un botón (ej: desaparecer el de Pausa) no se quede grande ni seleccionado
    void OnDisable()
    {
        ratonEncima = false;
        estaSeleccionado = false;
        forzadoActivo = false;
        transform.localScale = new Vector3(tamañoNormal, tamañoNormal, 0f);
    }

    void Update()
    {
        bool juegoPausado = (Time.timeScale == 0f);
        bool bloqueadoPorPausa = (juegoPausado && !esMenu); // Si está pausado y NO es menú, se bloquea
        bool puedeComprar = (coste <= 0) || (ControladorJuego.instancia != null && ControladorJuego.instancia.PuedeComprar(coste));

        if (enCooldown)
        {
            temporizador -= Time.deltaTime;
            
            if (imagenCooldown != null) imagenCooldown.fillAmount = temporizador / tiempoCooldown;
            if (miImagenBase != null) miImagenBase.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            if (miBoton != null) miBoton.interactable = false;

            if (temporizador <= 0f) enCooldown = false;
        }
        else 
        {
            if (bloqueadoPorTutorial || !puedeComprar || bloqueadoPorPausa)
            {
                if (imagenCooldown != null) imagenCooldown.fillAmount = 0f; 
                if (miImagenBase != null) miImagenBase.color = (!puedeComprar) ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white; 
                if (miBoton != null) miBoton.interactable = false; 
                
                ratonEncima = false;
                estaSeleccionado = false;
            }
            else
            {
                if (imagenCooldown != null) imagenCooldown.fillAmount = 0f;
                if (miImagenBase != null) miImagenBase.color = Color.white;
                if (miBoton != null) miBoton.interactable = true;
            }
        }

        Vector3 objetivo = new Vector3(tamañoNormal, tamañoNormal, 0f);
        
        if (!bloqueadoPorTutorial && !enCooldown && puedeComprar && !bloqueadoPorPausa && (ratonEncima || estaSeleccionado || forzadoActivo))
        {
            objetivo = new Vector3(tamañoAgrandado, tamañoAgrandado, 0f);
        }
        
        // Tu fórmula original (funciona perfecta en pausa porque el Update sigue corriendo y divide la distancia por frame)
        transform.localScale += (objetivo - transform.localScale) / 25f;

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
        if (miImagenBase != null) miImagenBase.color = new Color(0.6f, 0.6f, 0.6f, 1f); 
        
        ratonEncima = false;
        estaSeleccionado = false;
    }

    public void OnPointerEnter(PointerEventData eventData) { if (!bloqueadoPorTutorial) ratonEncima = true; }
    public void OnPointerExit(PointerEventData eventData) { ratonEncima = false; }
    public void OnSelect(BaseEventData eventData) { if (!bloqueadoPorTutorial) estaSeleccionado = true; }
    public void OnDeselect(BaseEventData eventData) { estaSeleccionado = false; }
}