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

        CodigoAlumno.TipoAlumno tipo = alumnoSeleccionado.ObtenerTipoAlumno();

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

                if (tipo == CodigoAlumno.TipoAlumno.Hielo && alumnoSeleccionado.zonaImpactoHielo > 0f)
                {
                    string zonaMax = alumnoSeleccionado.zonaImpactoHielo.ToString("0.##", CultureInfo.InvariantCulture);
                    lineasMax.Add($"ZONA HIELO: <color=#51DBFF><size=25>{zonaMax}</size></color>");
                }
                else if (tipo == CodigoAlumno.TipoAlumno.Boomerang && alumnoSeleccionado.maxImpactosBoomerang > 0)
                {
                    lineasMax.Add($"IMPACTOS: <color=#51DBFF><size=25>{alumnoSeleccionado.maxImpactosBoomerang}</size></color>");
                }
                else if (tipo == CodigoAlumno.TipoAlumno.Basket)
                {
                    if (alumnoSeleccionado.radioZonaBasket > 0f)
                    {
                        string areaMax = alumnoSeleccionado.radioZonaBasket.ToString("0.##", CultureInfo.InvariantCulture);
                        lineasMax.Add($"AREA: <color=#51DBFF><size=25>{areaMax}</size></color>");
                    }
                    if (alumnoSeleccionado.danoAreaBasket > 0f)
                    {
                        string golpeAreaMax = alumnoSeleccionado.danoAreaBasket.ToString("0.0#", CultureInfo.InvariantCulture);
                        lineasMax.Add($"GOLPE AREA: <color=#51DBFF><size=25>{golpeAreaMax}</size></color>");
                    }
                    if (alumnoSeleccionado.duracionAturdimientoBasket > 0f)
                    {
                        string aturdirMax = alumnoSeleccionado.duracionAturdimientoBasket.ToString("0.##", CultureInfo.InvariantCulture);
                        lineasMax.Add($"ATURDIR: <color=#51DBFF><size=25>{aturdirMax}</size></color>");
                    }
                }

                textoInfoMejora.text = FormatearBloqueStats("<color=#F3FF00><size=25>NIVEL MAXIMO!!</size></color>", lineasMax);
            }
            return;
        }

        int nivelActual = alumnoSeleccionado.nivelActual;
        int siguienteNivel = nivelActual + 1;
        int costeSiguiente = alumnoSeleccionado.ObtenerCosteSiguienteMejora();

        float rangoSiguiente = alumnoSeleccionado.ObtenerRangoSiguiente();
        float cadenciaSiguiente = alumnoSeleccionado.ObtenerCadenciaSiguiente();
        float danoSiguiente = alumnoSeleccionado.ObtenerDanoSiguiente();

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

            if (tipo == CodigoAlumno.TipoAlumno.Hielo)
            {
                float zonaSiguiente = alumnoSeleccionado.ObtenerZonaHieloSiguiente();
                if (zonaSiguiente > 0f && Mathf.Abs(zonaSiguiente - alumnoSeleccionado.zonaImpactoHielo) > 0.001f)
                {
                    string zonaActual = alumnoSeleccionado.zonaImpactoHielo.ToString("0.##", CultureInfo.InvariantCulture);
                    string zonaNueva = zonaSiguiente.ToString("0.##", CultureInfo.InvariantCulture);
                    lineasMejora.Add($"ZONA HIELO: {zonaActual} -- <color=#FF5151><size=25>{zonaNueva}</size></color>");
                }
            }
            else if (tipo == CodigoAlumno.TipoAlumno.Boomerang)
            {
                int impactosSiguiente = alumnoSeleccionado.ObtenerMaxImpactosSiguiente();
                if (impactosSiguiente > 0 && impactosSiguiente != alumnoSeleccionado.maxImpactosBoomerang)
                {
                    lineasMejora.Add($"IMPACTOS: {alumnoSeleccionado.maxImpactosBoomerang} -- <color=#FF5151><size=25>{impactosSiguiente}</size></color>");
                }
            }
            else if (tipo == CodigoAlumno.TipoAlumno.Basket)
            {
                float radioBasketSiguiente = alumnoSeleccionado.ObtenerRadioBasketSiguiente();
                if (radioBasketSiguiente > 0f && Mathf.Abs(radioBasketSiguiente - alumnoSeleccionado.radioZonaBasket) > 0.001f)
                {
                    string areaActual = alumnoSeleccionado.radioZonaBasket.ToString("0.##", CultureInfo.InvariantCulture);
                    string areaNueva = radioBasketSiguiente.ToString("0.##", CultureInfo.InvariantCulture);
                    lineasMejora.Add($"AREA: {areaActual} -- <color=#FF5151><size=25>{areaNueva}</size></color>");
                }

                float danoAreaBasketSiguiente = alumnoSeleccionado.ObtenerDanoAreaBasketSiguiente();
                if (danoAreaBasketSiguiente > 0f && Mathf.Abs(danoAreaBasketSiguiente - alumnoSeleccionado.danoAreaBasket) > 0.001f)
                {
                    string golpeAreaActual = alumnoSeleccionado.danoAreaBasket.ToString("0.0#", CultureInfo.InvariantCulture);
                    string golpeAreaNuevo = danoAreaBasketSiguiente.ToString("0.0#", CultureInfo.InvariantCulture);
                    lineasMejora.Add($"GOLPE AREA: {golpeAreaActual} -- <color=#FF5151><size=25>{golpeAreaNuevo}</size></color>");
                }

                float aturdimientoSiguiente = alumnoSeleccionado.ObtenerAturdimientoBasketSiguiente();
                if (aturdimientoSiguiente > 0f && Mathf.Abs(aturdimientoSiguiente - alumnoSeleccionado.duracionAturdimientoBasket) > 0.001f)
                {
                    string aturdirActual = alumnoSeleccionado.duracionAturdimientoBasket.ToString("0.##", CultureInfo.InvariantCulture);
                    string aturdirNuevo = aturdimientoSiguiente.ToString("0.##", CultureInfo.InvariantCulture);
                    lineasMejora.Add($"ATURDIR: {aturdirActual}s -- <color=#FF5151><size=25>{aturdirNuevo}s</size></color>");
                }
            }

            string titulo = $"Subir de nivel <color=#F3FF00><size=25>{nivelActual}</size></color> a <color=#51DBFF><size=25>{siguienteNivel}</size></color>?";
            textoInfoMejora.text = FormatearBloqueStats(titulo, lineasMejora);
        }
    }

    private string FormatearBloqueStats(string titulo, List<string> lineas)
    {
        string salto = (lineas.Count >= 4) ? "\n" : "\n\n";
        string cuerpo = string.Join("\n", lineas);

        if (lineas.Count >= 6)
        {
            titulo = "<size=19>" + titulo.Replace("<size=25>", "<size=22>") + "</size>";
            cuerpo = "<size=16>" + cuerpo.Replace("<size=25>", "<size=19>") + "</size>";
        }
        else if (lineas.Count == 5)
        {
            cuerpo = "<size=18>" + cuerpo.Replace("<size=25>", "<size=22>") + "</size>";
        }

        return titulo + salto + cuerpo;
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