using UnityEngine;
using UnityEngine.InputSystem;

public class GestorColocacion : MonoBehaviour
{
    public enum ModoGestor { Nada, Colocando, Borrando }

    public GameObject alumnoPrefab;
    public Sprite spriteBasura; 

    private ModoGestor modoActual = ModoGestor.Nada;
    private GameObject objetoEnRaton; 
    private EfectoBoton botonSeleccionado;

    private CodigoAlumno alumnoEnPuntoDeMira;
    private Color colorOriginalAlumno = Color.white;
    public int costeAlumno1 = 20;

    void Update()
    {
        if (modoActual == ModoGestor.Nada) return;

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
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ColocarAlumno();
            }
        }
        else if (modoActual == ModoGestor.Borrando)
        {
            GestionarBorrado(posicionRaton);
        }
    }

    public void IniciarColocacion(EfectoBoton botonQueLlama)
    {
        if (!ControladorJuego.instancia.PuedeComprar(costeAlumno1)) return;

        if (modoActual == ModoGestor.Colocando)
        {
            CancelarAccion();
            return;
        }

        CancelarAccion(); 
        objetoEnRaton = Instantiate(alumnoPrefab);
        modoActual = ModoGestor.Colocando;
        
        botonSeleccionado = botonQueLlama; // Guardamos el botón en memoria
    }

    public void IniciarBorrado()
    {
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
        objetoEnRaton.GetComponent<CodigoAlumno>().estaColocado = true;
        objetoEnRaton = null; 
        modoActual = ModoGestor.Nada;

        ControladorJuego.instancia.GastarEnergia(costeAlumno1);

        if (GestorTutorial.instancia != null) GestorTutorial.instancia.RegistrarAlumnoColocado();

        if (botonSeleccionado != null)
        {
            botonSeleccionado.IniciarCooldown();
            botonSeleccionado = null;
        }
    }

    private void GestionarBorrado(Vector3 posicionRaton)
    {
        Collider2D tocado = Physics2D.OverlapPoint(posicionRaton);
        CodigoAlumno alumnoTocado = null;

        if (tocado != null)
        {
            alumnoTocado = tocado.GetComponent<CodigoAlumno>();
        }

        if (alumnoTocado != null && alumnoTocado.estaColocado)
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