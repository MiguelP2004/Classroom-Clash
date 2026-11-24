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

        if (alumnoSeleccionado.EstaAlMaximo())
        {
            if (botonSi != null)
            {
                botonSi.ForzarDeseleccion();
                botonSi.gameObject.SetActive(false);
            }

            if (iconoCafe != null) iconoCafe.SetActive(false);

            if (textoInfoMejora != null)
            {
                List<string> lineasMax = new List<string>();

                string alcanceMax = (alumnoSeleccionado.rangoAtaque * 10f).ToString("0.#", CultureInfo.InvariantCulture);
                string cadenciaMax = alumnoSeleccionado.cadenciaFuego.ToString("0.0#", CultureInfo.InvariantCulture);
                string golpeMax = alumnoSeleccionado.danoProyectil.ToString("0.0#", CultureInfo.InvariantCulture);

                lineasMax.Add($"ALCANCE: <color=#51DBFF><size=25>{alcanceMax}</size></color>");
                lineasMax.Add($"CADENCIA: <color=#51DBFF><size=25>{cadenciaMax}</size></color>");
                lineasMax.Add($"GOLPE: <color=#51DBFF><size=25>{golpeMax}</size></color>");

                if (alumnoSeleccionado.zonaImpactoHielo > 0f)
                {
                    string zonaMax = alumnoSeleccionado.zonaImpactoHielo.ToString("0.##", CultureInfo.InvariantCulture);
                    lineasMax.Add($"ZONA HIELO: <color=#51DBFF><size=25>{zonaMax}</size></color>");
                }

                string saltoMax = (lineasMax.Count >= 4) ? "\n" : "\n\n";

                textoInfoMejora.text =
                    "<color=#F3FF00><size=25>NIVEL MAXIMO!!</size></color>" +
                    saltoMax +
                    string.Join("\n", lineasMax);
            }
            return;
        }

        int nivelActual = alumnoSeleccionado.nivelActual;
        int siguienteNivel = nivelActual + 1;
        int costeSiguiente = alumnoSeleccionado.ObtenerCosteSiguienteMejora();

        float rangoSiguiente = alumnoSeleccionado.ObtenerRangoSiguiente();
        float cadenciaSiguiente = alumnoSeleccionado.ObtenerCadenciaSiguiente();
        float danoSiguiente = alumnoSeleccionado.ObtenerDanoSiguiente();
        float zonaSiguiente = alumnoSeleccionado.ObtenerZonaHieloSiguiente();

        if (botonSi != null)
        {
            botonSi.gameObject.SetActive(true);
            botonSi.coste = costeSiguiente;
        }

        if (iconoCafe != null) iconoCafe.SetActive(true);

        if (textoInfoPrecio != null)
        {
            textoInfoPrecio.text = costeSiguiente.ToString();
        }

        if (textoInfoMejora != null)
        {
            List<string> lineasMejora = new List<string>();

            if (Mathf.Abs(rangoSiguiente - alumnoSeleccionado.rangoAtaque) > 0.001f)
            {
                string alcanceActual = (alumnoSeleccionado.rangoAtaque * 10f).ToString("0.#", CultureInfo.InvariantCulture);
                string alcanceNuevo = (rangoSiguiente * 10f).ToString("0.#", CultureInfo.InvariantCulture);
                lineasMejora.Add($"ALCANCE: {alcanceActual} -- <color=#FF5151><size=25>{alcanceNuevo}</size></color>");
            }

            if (Mathf.Abs(cadenciaSiguiente - alumnoSeleccionado.cadenciaFuego) > 0.001f)
            {
                string cadenciaActual = alumnoSeleccionado.cadenciaFuego.ToString("0.0#", CultureInfo.InvariantCulture);
                string cadenciaNueva = cadenciaSiguiente.ToString("0.0#", CultureInfo.InvariantCulture);
                lineasMejora.Add($"CADENCIA: {cadenciaActual} -- <color=#FF5151><size=25>{cadenciaNueva}</size></color>");
            }

            if (Mathf.Abs(danoSiguiente - alumnoSeleccionado.danoProyectil) > 0.001f)
            {
                string golpeActual = alumnoSeleccionado.danoProyectil.ToString("0.0#", CultureInfo.InvariantCulture);
                string golpeNuevo = danoSiguiente.ToString("0.0#", CultureInfo.InvariantCulture);
                lineasMejora.Add($"GOLPE: {golpeActual} -- <color=#FF5151><size=25>{golpeNuevo}</size></color>");
            }

            if (Mathf.Abs(zonaSiguiente - alumnoSeleccionado.zonaImpactoHielo) > 0.001f)
            {
                string zonaActual = alumnoSeleccionado.zonaImpactoHielo.ToString("0.##", CultureInfo.InvariantCulture);
                string zonaNueva = zonaSiguiente.ToString("0.##", CultureInfo.InvariantCulture);
                lineasMejora.Add($"ZONA HIELO: {zonaActual} -- <color=#FF5151><size=25>{zonaNueva}</size></color>");
            }

            // Si hay 4 líneas, eliminamos la línea en blanco para que no se salga del papel
            string salto = (lineasMejora.Count >= 4) ? "\n" : "\n\n";

            textoInfoMejora.text =
                $"Subir de nivel <color=#F3FF00><size=25>{nivelActual}</size></color> a <color=#51DBFF><size=25>{siguienteNivel}</size></color>?" +
                salto +
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

        if (alumnoSeleccionado == null || alumnoSeleccionado.EstaAlMaximo()) return;
        if (ControladorJuego.instancia == null || ControladorJuego.instancia.esGameOver || ControladorJuego.instancia.enTransicion) return;

        int coste = alumnoSeleccionado.ObtenerCosteSiguienteMejora();
        if (!ControladorJuego.instancia.PuedeComprar(coste)) return;

        ControladorJuego.instancia.GastarEnergia(coste);
        alumnoSeleccionado.AplicarMejora();

        if (botonSi != null) botonSi.ForzarDeseleccion();
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