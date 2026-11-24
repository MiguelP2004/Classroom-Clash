
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ControladorCursor : MonoBehaviour
{
    [Header("Cursores del menú")]
    public Texture2D cursorMano;
    public Texture2D cursorClick;

    [Header("Cursor del juego")]
    public Texture2D cursorDiana;

    [Header("Puntos de referencia")]
    public Vector2 puntoMano = new Vector2(18, 2);
    public Vector2 puntoClick = new Vector2(18, 2);
    public Vector2 puntoDiana = new Vector2(32, 32);

    private bool estaEnMenu;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    void Start()
    {
        CambiarEscena(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (!estaEnMenu) return;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Cursor.SetCursor(cursorClick, puntoClick, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(cursorMano, puntoMano, CursorMode.Auto);
        }
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        CambiarEscena(escena.name);
    }

    void CambiarEscena(string nombreEscena)
    {
        estaEnMenu = nombreEscena == "MenuPrincipal";

        if (estaEnMenu)
        {
            Cursor.SetCursor(cursorMano, puntoMano, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(cursorDiana, puntoDiana, CursorMode.Auto);
        }
    }
}
