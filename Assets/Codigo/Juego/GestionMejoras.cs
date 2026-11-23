using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Globalization;

public class GestionMejoras : MonoBehaviour
{
    public static GestionMejoras instancia;

    [Header("Interfaz del Panel")]
    public RectTransform panelMejoras;
    public TextMeshProUGUI textoInfoMejora;
    public TextMeshProUGUI textoInfoPrecio;
    public EfectoBoton botonSi;
    public EfectoBoton botonNo;
    public GameObject iconoCafe;

    [Header("Animación Smooth")]
    public float posicionY_Visible = -337f;
    public float posicionY_Oculta = -900f;
    public float tiempoSuavizado = 0.12f;

    private CodigoAlumno alumnoSeleccionado;
    private bool panelAbierto = false;
    private bool animandoPanel = false;
    private float velocidadY = 0f;

    void Awake()
    {
        if (instancia == null) instancia = this;

        if (panelMejoras != null)
        {
            Vector2 pos = panelMejoras.anchoredPosition;
            pos.y = posicionY_Oculta;
            panelMejoras.anchoredPosition = pos;
            panelMejoras.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (panelAbierto)
        {
            bool juegoPausado = (Time.timeScale == 0f);
            bool gestorOcupado = (GestorColocacion.instancia != null && GestorColocacion.instancia.EstaOcupado());
            bool bloqueadoGlobal = (ControladorJuego.instancia != null &&
                                   (ControladorJuego.instancia.esGameOver || ControladorJuego.instancia.enTransicion));

            if (juegoPausado || gestorOcupado || bloqueadoGlobal || alumnoSeleccionado == null)
            {
                CerrarPanel(true);
            }
        }

        if (panelMejoras != null && animandoPanel)
        {
            float objetivoY = panelAbierto ? posicionY_Visible : posicionY_Oculta;
            Vector2 posActual = panelMejoras.anchoredPosition;

            posActual.y = Mathf.SmoothDamp(posActual.y, objetivoY, ref velocidadY, tiempoSuavizado, Mathf.Infinity, Time.unscaledDeltaTime);
            panelMejoras.anchoredPosition = posActual;

            if (Mathf.Abs(posActual.y - objetivoY) < 1f)
            {
                posActual.y = objetivoY;
                panelMejoras.anchoredPosition = posActual;

                if (!panelAbierto)
                {
                    panelMejoras.gameObject.SetActive(false);
                }
                animandoPanel = false;
            }
        }
    }

    public void SeleccionarAlumno(CodigoAlumno nuevoAlumno)
    {
        if (MenuPrincipal.nivelElegido <= 1) return;

        if (nuevoAlumno == null)
        {
            CerrarPanel(true);
            return;
        }

        if (alumnoSeleccionado == nuevoAlumno && panelAbierto) return;

        if (alumnoSeleccionado != null && alumnoSeleccionado != nuevoAlumno)
        {
            alumnoSeleccionado.seleccionado = false;
        }

        alumnoSeleccionado = nuevoAlumno;

        ActualizarInterfazAlumno();
        AbrirPanelDesdeAbajo();
    }

    public void NotificarDeseleccion(CodigoAlumno alumno)
    {
        if (alumnoSeleccionado == alumno)
        {
            alumnoSeleccionado = null;
            CerrarPanel(false);
        }
    }

    private void ActualizarInterfazAlumno()
    {
        if (alumnoSeleccionado == null) return;

        if (botonNo != null) botonNo.gameObject.SetActive(true);

        if (alumnoSeleccionado.estaMejorado)
        {
            if (botonSi != null)
            {
                botonSi.ForzarDeseleccion();
                botonSi.gameObject.SetActive(false);
            }

            if (iconoCafe != null) iconoCafe.SetActive(false);

            if (textoInfoMejora != null)
            {
                textoInfoMejora.text = "\n<color=#F3FF00><size=34>NIVEL MAXIMO</size></color>";
            }
            return;
        }

        if (botonSi != null)
        {
            botonSi.gameObject.SetActive(true);
            botonSi.coste = alumnoSeleccionado.costeMejora;
        }

        if (iconoCafe != null) iconoCafe.SetActive(true);

        if (textoInfoPrecio != null)
        {
            textoInfoPrecio.text = alumnoSeleccionado.costeMejora.ToString();
        }

        if (textoInfoMejora != null)
        {
            List<string> lineasMejora = new List<string>();

            if (Mathf.Abs(alumnoSeleccionado.rangoMejorado - alumnoSeleccionado.rangoAtaque) > 0.001f)
            {
                string alcanceActual = (alumnoSeleccionado.rangoAtaque * 10f).ToString("0.#", CultureInfo.InvariantCulture);
                string alcanceNuevo = (alumnoSeleccionado.rangoMejorado * 10f).ToString("0.#", CultureInfo.InvariantCulture);
                lineasMejora.Add($"ALCANCE: {alcanceActual} -- <color=#FF5151><size=25>{alcanceNuevo}</size></color>");
            }

            if (Mathf.Abs(alumnoSeleccionado.cadenciaMejorada - alumnoSeleccionado.cadenciaFuego) > 0.001f)
            {
                string cadenciaActual = alumnoSeleccionado.cadenciaFuego.ToString("0.0#", CultureInfo.InvariantCulture);
                string cadenciaNueva = alumnoSeleccionado.cadenciaMejorada.ToString("0.0#", CultureInfo.InvariantCulture);
                lineasMejora.Add($"CADENCIA: {cadenciaActual} -- <color=#FF5151><size=25>{cadenciaNueva}</size></color>");
            }

            if (Mathf.Abs(alumnoSeleccionado.danoMejorado - alumnoSeleccionado.danoProyectil) > 0.001f)
            {
                string golpeActual = alumnoSeleccionado.danoProyectil.ToString("0.0#", CultureInfo.InvariantCulture);
                string golpeNuevo = alumnoSeleccionado.danoMejorado.ToString("0.0#", CultureInfo.InvariantCulture);
                lineasMejora.Add($"GOLPE: {golpeActual} -- <color=#FF5151><size=25>{golpeNuevo}</size></color>");
            }

            if (Mathf.Abs(alumnoSeleccionado.zonaImpactoHieloMejorada - alumnoSeleccionado.zonaImpactoHielo) > 0.001f)
            {
                string zonaActual = alumnoSeleccionado.zonaImpactoHielo.ToString("0.##", CultureInfo.InvariantCulture);
                string zonaNueva = alumnoSeleccionado.zonaImpactoHieloMejorada.ToString("0.##", CultureInfo.InvariantCulture);
                lineasMejora.Add($"ZONA HIELO: {zonaActual} -- <color=#FF5151><size=25>{zonaNueva}</size></color>");
            }

            textoInfoMejora.text =
                "Subir de nivel <color=#F3FF00><size=25>1</size></color> a <color=#51DBFF><size=25>2</size></color>?\n\n" +
                string.Join("\n", lineasMejora);
        }
    }

    private void AbrirPanelDesdeAbajo()
    {
        if (panelMejoras == null) return;

        panelMejoras.gameObject.SetActive(true);

        Vector2 pos = panelMejoras.anchoredPosition;
        pos.y = posicionY_Oculta;
        panelMejoras.anchoredPosition = pos;
        velocidadY = 0f;

        panelAbierto = true;
        animandoPanel = true;
    }

    public void ConfirmarMejora()
    {
        CodigoProfe.BloquearDisparoHastaSoltar();

        if (alumnoSeleccionado == null || alumnoSeleccionado.estaMejorado) return;
        if (ControladorJuego.instancia == null || ControladorJuego.instancia.esGameOver || ControladorJuego.instancia.enTransicion) return;
        if (!ControladorJuego.instancia.PuedeComprar(alumnoSeleccionado.costeMejora)) return;

        ControladorJuego.instancia.GastarEnergia(alumnoSeleccionado.costeMejora);
        alumnoSeleccionado.AplicarMejora();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        ActualizarInterfazAlumno();
        AbrirPanelDesdeAbajo();
    }

    public void CancelarYCerrar()
    {
        CodigoProfe.BloquearDisparoHastaSoltar();
        CerrarPanel(true);
    }

    public void CerrarPanel(bool deseleccionarAlumno)
    {
        if (deseleccionarAlumno && alumnoSeleccionado != null)
        {
            alumnoSeleccionado.seleccionado = false;
            alumnoSeleccionado = null;
        }

        if (botonSi != null) botonSi.ForzarDeseleccion();
        if (botonNo != null) botonNo.ForzarDeseleccion();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (panelAbierto)
        {
            panelAbierto = false;
            animandoPanel = true;
        }
    }
}