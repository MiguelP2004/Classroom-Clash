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
    private float margenBorde = 0.2f;
    private float limiteIzquierdo = -4.05f; 
    private float limiteDerecho = 0f;

    private Vector3 posicionObjetivoCamara;

    void Awake()
    {
        if (instancia == null) instancia = this;
        ActualizarTexto();
        
        if (camaraPrincipal == null) camaraPrincipal = Camera.main;
        if (camaraPrincipal != null) posicionObjetivoCamara = camaraPrincipal.transform.position;
    }

    void Update()
    {
        ManejarCamara();

        if (textoEnergia != null)
        {
            textoEnergia.transform.localScale += (Vector3.one - textoEnergia.transform.localScale) / 40f;
        }
    }

    private void ManejarCamara()
    {
        if (camaraPrincipal == null || Mouse.current == null) return;

        // Si el tutorial bloquea la cámara, no permitimos mover el objetivo con el ratón
        if (!camaraBloqueadaPorTutorial)
        {
            Vector2 posRaton = Mouse.current.position.ReadValue();
            float anchoPantalla = Screen.width;

            if (posRaton.x < anchoPantalla * margenBorde)
            {
                posicionObjetivoCamara.x -= velocidadCamara * Time.deltaTime; 
            }
            else if (posRaton.x > anchoPantalla * (1f - margenBorde))
            {
                posicionObjetivoCamara.x += velocidadCamara * Time.deltaTime;
            }

            posicionObjetivoCamara.x = Mathf.Clamp(posicionObjetivoCamara.x, limiteIzquierdo, limiteDerecho);
        }

        Vector3 posicionActual = camaraPrincipal.transform.position;
        float posXAnterior = posicionActual.x;

        posicionActual.x = Mathf.Lerp(posicionActual.x, posicionObjetivoCamara.x, Time.deltaTime * suavizadoCamara); 
        camaraPrincipal.transform.position = posicionActual;

        // Avisamos al tutorial de cuánto se ha movido la cámara realmente
        float desplazamiento = Mathf.Abs(posicionActual.x - posXAnterior);
        if (!camaraBloqueadaPorTutorial && desplazamiento > 0.001f && GestorTutorial.instancia != null)
        {
            GestorTutorial.instancia.RegistrarMovimientoCamara(desplazamiento);
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