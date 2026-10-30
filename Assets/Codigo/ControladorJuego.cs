using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego instancia;

    [Header("Energía")]
    public int energia = 50;
    public TextMeshProUGUI textoEnergia; 
    public float tamañoAgrandado = 1.1f;

    [Header("Cámara")]
    public Camera camaraPrincipal;
    [HideInInspector] public bool camaraBloqueadaPorTutorial = false;
    
    private float velocidadCamara = 7f;
    private float suavizadoCamara = 12f;
    private float margenBorde = 0.15f;
    private float limiteIzquierdo = -5.4f; 
    private float limiteDerecho = 0f;

    private Vector3 posicionObjetivoCamara;

    [Header("Pausa")]
    public GameObject botonPausa; 
    public GameObject panelPausa; 
    public RectTransform panelBarrido; 
    private float posicionY_Oculta = 1200f; 
    private float posicionY_Visible = 0f;  
    
    private bool juegoPausado = false;
    private bool animandoBarrido = false;
    private float velocidadBarrido = 0f;

    void Awake()
    {
        if (instancia == null) instancia = this;
        ActualizarTexto();
        
        if (camaraPrincipal == null) camaraPrincipal = Camera.main;
        if (camaraPrincipal != null) posicionObjetivoCamara = camaraPrincipal.transform.position;

        Time.timeScale = 1f;
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelBarrido != null)
        {
            Vector2 pos = panelBarrido.anchoredPosition;
            pos.y = posicionY_Oculta;
            panelBarrido.anchoredPosition = pos;
            panelBarrido.gameObject.SetActive(false);
        }
        if (botonPausa != null) botonPausa.SetActive(true);
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
            
            posActual.y = Mathf.SmoothDamp(posActual.y, objetivoY, ref velocidadBarrido, 0.15f, Mathf.Infinity, Time.unscaledDeltaTime);
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
            textoEnergia.transform.localScale += (Vector3.one - textoEnergia.transform.localScale) * (Time.unscaledDeltaTime * 10f);
        }
    }

    private void ManejarCamara()
    {
        if (camaraPrincipal == null || Mouse.current == null) return;

        if (!camaraBloqueadaPorTutorial)
        {
            Vector2 posRaton = Mouse.current.position.ReadValue();
            float anchoPantalla = Screen.width;
            float altoPantalla = Screen.height; // Obtenemos la altura total de la pantalla actual

            // Comprobamos que el ratón esté dentro de la franja Y permitida antes de mover la cámara
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
        if (juegoPausado) ReanudarJuego();
        else PausarJuego();
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

    public bool PuedeComprar(int coste)
    {
        return energia >= coste;
    }

    private void EfectoCambioEnergia()
    {
        if (textoEnergia != null)
        {
            textoEnergia.text = energia.ToString();
            textoEnergia.transform.localScale = new Vector3(tamañoAgrandado, tamañoAgrandado, 1f);
        }
    }

    private void ActualizarTexto()
    {
        if (textoEnergia != null) textoEnergia.text = energia.ToString();
    }
}