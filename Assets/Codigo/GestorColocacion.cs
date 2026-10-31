using UnityEngine;
using UnityEngine.InputSystem;

public class GestorColocacion : MonoBehaviour
{
    public static GestorColocacion instancia;

    public enum ModoGestor { Nada, Colocando, Borrando }

    public GameObject alumnoPrefab;
    public Sprite spriteBasura; 
    public int costeAlumno1 = 20;

    [Header("Restricción de Camino")]
    public Collider2D colliderCamino;
    public float radioOcupacionAlumno = 0.35f;

    private ModoGestor modoActual = ModoGestor.Nada;
    private GameObject objetoEnRaton; 
    private EfectoBoton botonSeleccionado;

    private CodigoAlumno alumnoEnPuntoDeMira;
    private Color colorOriginalAlumno = Color.white;

    void Awake()
    {
        if (instancia == null) instancia = this;
    }

    void Update()
    {
        if (modoActual == ModoGestor.Nada) return;

        if (ControladorJuego.instancia != null && ControladorJuego.instancia.esGameOver)
        {
            CancelarAccion();
            return;
        }

        Vector2 posicionPantalla = Mouse.current.position.ReadValue();
        Vector3 posicionRaton = Camera.main.ScreenToWorldPoint(posicionPantalla);
        posicionRaton.z = 0f;

        if (objetoEnRaton != null)
        {
            objetoEnRaton.transform.position = posicionRaton;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelarAccion();
            return;
        }

        if (modoActual == ModoGestor.Colocando)
        {
            bool puedePlantarAqui = EsPosicionValida(posicionRaton);

            if (objetoEnRaton != null)
            {
                SpriteRenderer sr = objetoEnRaton.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = puedePlantarAqui ? Color.white : Color.red;
                }
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && puedePlantarAqui)
            {
                ColocarAlumno();
            }
        }
        else if (modoActual == ModoGestor.Borrando)
        {
            GestionarBorrado(posicionRaton);
        }
    }

    private bool EsPosicionValida(Vector2 posicion)
    {
        if (colliderCamino != null)
        {
            Vector2 puntoMasCercano = colliderCamino.ClosestPoint(posicion);
            if (Vector2.Distance(posicion, puntoMasCercano) < radioOcupacionAlumno)
            {
                return false;
            }
        }

        Collider2D[] solapados = Physics2D.OverlapCircleAll(posicion, radioOcupacionAlumno);
        foreach (Collider2D col in solapados)
        {
            if (col.gameObject == objetoEnRaton) continue;
            if (col.CompareTag("Obstaculo")) return false;

            CodigoAlumno otroAlumno = col.GetComponent<CodigoAlumno>();
            if (otroAlumno != null && otroAlumno.estaColocado)
            {
                return false;
            }
        }

        return true;
    }

    public void IniciarColocacion(EfectoBoton botonQueLlama)
    {
        if (ControladorJuego.instancia != null && ControladorJuego.instancia.esGameOver) return;
        if (!ControladorJuego.instancia.PuedeComprar(costeAlumno1)) return;

        if (modoActual == ModoGestor.Colocando)
        {
            CancelarAccion();
            return;
        }

        CancelarAccion(); 
        objetoEnRaton = Instantiate(alumnoPrefab);
        modoActual = ModoGestor.Colocando;
        
        botonSeleccionado = botonQueLlama;
        if (botonSeleccionado != null) botonSeleccionado.forzadoActivo = true;
    }

    public void IniciarBorrado()
    {
        if (ControladorJuego.instancia != null && ControladorJuego.instancia.esGameOver) return;

        if (modoActual == ModoGestor.Borrando)
        {
            CancelarAccion();
            return;
        }

        CancelarAccion(); 

        if (spriteBasura != null)
        {
            objetoEnRaton = new GameObject("IconoBasuraFlotante");
            SpriteRenderer sr = objetoEnRaton.AddComponent<SpriteRenderer>();
            sr.sprite = spriteBasura;
            sr.sortingOrder = 20000;
            objetoEnRaton.transform.localScale = Vector3.one * 0.4f;
        }

        modoActual = ModoGestor.Borrando;
    }

    private void ColocarAlumno()
    {
        SpriteRenderer sr = objetoEnRaton.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;

        objetoEnRaton.GetComponent<CodigoAlumno>().estaColocado = true;
        objetoEnRaton = null; 
        modoActual = ModoGestor.Nada;

        ControladorJuego.instancia.GastarEnergia(costeAlumno1);

        if (GestorTutorial.instancia != null) GestorTutorial.instancia.RegistrarAlumnoColocado();

        if (botonSeleccionado != null)
        {
            botonSeleccionado.forzadoActivo = false;
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
            botonSeleccionado.IniciarCooldown();
            botonSeleccionado = null;
        }
    }

    private void GestionarBorrado(Vector3 posicionRaton)
    {
        Collider2D[] tocados = Physics2D.OverlapPointAll(posicionRaton);
        CodigoAlumno alumnoTocado = null;

        foreach (Collider2D col in tocados)
        {
            CodigoAlumno posibleAlumno = col.GetComponent<CodigoAlumno>();
            if (posibleAlumno != null && posibleAlumno.estaColocado)
            {
                alumnoTocado = posibleAlumno;
                break;
            }
        }

        if (alumnoTocado != null)
        {
            if (alumnoEnPuntoDeMira != alumnoTocado)
            {
                RestaurarColorAlumno(); 
                
                alumnoEnPuntoDeMira = alumnoTocado;
                SpriteRenderer sr = alumnoEnPuntoDeMira.GetComponent<SpriteRenderer>();
                
                colorOriginalAlumno = sr.color; 
                sr.color = Color.red; 
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Destroy(alumnoEnPuntoDeMira.gameObject);
                alumnoEnPuntoDeMira = null;
                CancelarAccion(); 
                if (GestorTutorial.instancia != null) GestorTutorial.instancia.RegistrarAlumnoBorrado();
            }
        }
        else
        {
            RestaurarColorAlumno();
        }
    }

    public void CancelarAccion()
    {
        if (botonSeleccionado != null) 
        {
            botonSeleccionado.forzadoActivo = false; 
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }
        
        if (objetoEnRaton != null) Destroy(objetoEnRaton);
        RestaurarColorAlumno();
        
        objetoEnRaton = null;
        botonSeleccionado = null; 
        modoActual = ModoGestor.Nada;
    }

    private void RestaurarColorAlumno()
    {
        if (alumnoEnPuntoDeMira != null)
        {
            alumnoEnPuntoDeMira.GetComponent<SpriteRenderer>().color = colorOriginalAlumno;
            alumnoEnPuntoDeMira = null;
        }
    }
}