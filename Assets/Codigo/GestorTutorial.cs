using UnityEngine;
using TMPro;
using System.Collections;

public class GestorTutorial : MonoBehaviour
{
    public static GestorTutorial instancia;

    [Header("Referencias del Juego")]
    public TextMeshProUGUI textoTutorial;
    public EfectoBoton botonAlumno;
    public EfectoBoton botonBasura;
    public generacionExamenes generadorExamenes;
    public CodigoProfe profe;

    [Header("Para el tutorial")]
    public float velocidadFade = 2f;
    public float tamañoZoomExito = 1.2f;

    public GameObject flechaIndicacion;
    private float posX_Alumno = -350f;
    private float posX_Basura = 350f;

    private string msgBienvenida = "BIENVENIDO A <color=#F3FF00><size=60>CLASSROOM CLASH</size></color>\nVAMOS A APRENDER LOS CONCEPTOS BASICOS!"; 
    private string msgMoverCamara = "UTILIZA EL <color=#FF5151><size=60>MOUSE</size></color> EN LOS BORDES PARA DESPLAZARTE A LA DERECHA E IZQUIERDA DEL MAPA";
    private string msgExplicacionCamara = "MOVER LA <color=#51DBFF><size=60>CAMARA</size></color> ES FUNDAMENTAL PARA VIGILAR TODO EL RECORRIDO DE LOS EXAMENES Y ARMAR TU ESTRATEGIA DE DEFENSA";
    private string msgDisparar = "PRESIONA <color=#FF5151><size=60>CLICK IZQUIERDO</size></color> CON EL MOUSE PARA DISPARAR CON EL PROFESOR";
    private string msgExplicacionProfe = "CON EL <color=#51DBFF><size=60>PROFESOR</size></color> PUEDES DISPARAR A LOS EXAMENES TAMBIEN Y ES ESENCIAL PARA DAR APOYO A TUS ALUMNOS";
    private string msgPlantarAlumno = "SELECCIONA EL <color=#FF5151><size=60>BOTON DEL ALUMNO</size></color> PARA DESPLEGAR UN ALUMNO EN EL AULA";
    private string msgExplicacionAlumno = "LOS <color=#51DBFF><size=60>ALUMNOS</size></color> DISPARAN AUTOMATICAMENTE A LOS EXAMENES DENTRO DE SU RANGO.\nCOLOCA TANTOS COMO PUEDAS!";
    private string msgAlertaExamenes = "<color=#FF5151><size=60>CUIDADO!!\n</size></color> SE ACERCA UN GRUPO DE EXAMENES!!\nACABA CON ELLOS!";
    private string msgUsarBasura = "PERFECTO! ZONA LIMPIA. <color=#F3FF00><size=60>BUEN TRABAJO!!</size></color>\nUSA EL <color=#FF5151><size=60>BOTON DE LA BASURA</size></color> PARA RECOGER AL ALUMNO Y QUITARLO DEL AULA";
    private string msgExplicacionBasura = "LA <color=#51DBFF><size=60>BASURA</size></color> ES GRATIS Y TE PERMITE RECOGER A LOS ALUMNOS PARA ORGANIZAR MEJOR TUS DEFENSAS";
    private string msgFinal = "<color=#F3FF00><size=60>ENHORABUENA</size></color>\nHAS APRENDIDO LO BÁSICO\nCUIDADO QUE SE ACERCAN MAS! PON EN PRÁCTICA TUS HABILIDADES! <color=#FF5151><size=60>EXAMENES!!</size></color>";
    private string msgVictoria = "<color=#F3FF00><size=60>ENHORABUENA</size></color>\nHAS GANADO EL TUTORIAL!";
    private string msgProximamente = "<color=#51DBFF><size=60>PRONTO SALDRÁ</size></color>\nEL JUEGO COMPLETO";

    private float distanciaCamaraRecorrida = 0f;
    private bool profeHaDisparado = false;
    private bool alumnoColocado = false;
    private bool alumnoBorrado = false;
    private int examenesMuertos = 0;

    void Awake()
    {
        if (instancia == null) instancia = this;
    }

    void Start()
    {
        if (flechaIndicacion != null) flechaIndicacion.SetActive(false);
        if (textoTutorial != null)
        {
            Color c = textoTutorial.color;
            c.a = 0f;
            textoTutorial.color = c;
        }

        StartCoroutine(SecuenciaTutorial());
    }

    void Update()
    {
        if (textoTutorial != null)
        {
            textoTutorial.transform.localScale += (Vector3.one - textoTutorial.transform.localScale) / 50f;
        }
    }

    IEnumerator SecuenciaTutorial()
    {
        // Bloqueo inicial total (incluida la cámara)
        if (generadorExamenes != null) generadorExamenes.generacionActiva = false;
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = true;
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = true;
        if (profe != null) profe.bloqueadoPorTutorial = true;
        if (ControladorJuego.instancia != null) ControladorJuego.instancia.camaraBloqueadaPorTutorial = true;

        yield return new WaitForSeconds(1.5f);

        // Bienvenida (con la cámara aún bloqueada)
        yield return StartCoroutine(MostrarTexto(msgBienvenida, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        // --- PASO 1: MOVER LA CÁMARA ---
        if (ControladorJuego.instancia != null) ControladorJuego.instancia.camaraBloqueadaPorTutorial = false;
        yield return StartCoroutine(MostrarTexto(msgMoverCamara, Color.white));
        
        distanciaCamaraRecorrida = 0f;
        yield return new WaitUntil(() => distanciaCamaraRecorrida >= 2.5f);
        yield return StartCoroutine(EfectoObjetivoCumplido());
        
        yield return StartCoroutine(MostrarTexto(msgExplicacionCamara, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        // --- PASO 2: DISPARAR CON EL PROFE ---
        if (profe != null) profe.bloqueadoPorTutorial = false;

        yield return StartCoroutine(MostrarTexto(msgDisparar, Color.white));
        profeHaDisparado = false;
        yield return new WaitUntil(() => profeHaDisparado);
        yield return StartCoroutine(EfectoObjetivoCumplido());
        yield return StartCoroutine(MostrarTexto(msgExplicacionProfe, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());

        yield return new WaitForSeconds(1.5f);

        // --- PASO 3: COLOCAR UN ALUMNO ---
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = false;
        yield return StartCoroutine(MostrarTexto(msgPlantarAlumno, Color.white));

        if (flechaIndicacion != null)
        {
            flechaIndicacion.SetActive(true);
            Vector3 pos = flechaIndicacion.transform.localPosition;
            pos.x = posX_Alumno;
            flechaIndicacion.transform.localPosition = pos;
        }
        
        alumnoColocado = false;
        yield return new WaitUntil(() => alumnoColocado);

        if (flechaIndicacion != null) flechaIndicacion.SetActive(false);
        
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = true; 
        yield return StartCoroutine(EfectoObjetivoCumplido());
        yield return StartCoroutine(MostrarTexto(msgExplicacionAlumno, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());

        yield return new WaitForSeconds(1.5f);

        // --- PASO 4: OLEADA DE 5 EXÁMENES ---
        yield return StartCoroutine(MostrarTexto(msgAlertaExamenes, Color.white));
        
        examenesMuertos = 0;
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 5; i++)
        {
            if (generadorExamenes != null) generadorExamenes.GenerarExamen();
            yield return new WaitForSeconds(1.5f);
        }
        
        yield return StartCoroutine(OcultarTexto());

        yield return new WaitUntil(() => examenesMuertos >= 5);

        // --- PASO 5: USAR LA BASURA ---
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = false;
        yield return StartCoroutine(MostrarTexto(msgUsarBasura, Color.white));

        if (flechaIndicacion != null)
        {
            flechaIndicacion.SetActive(true);
            Vector3 pos = flechaIndicacion.transform.localPosition;
            pos.x = posX_Basura;
            flechaIndicacion.transform.localPosition = pos;
        }

        alumnoBorrado = false;
        yield return new WaitUntil(() => alumnoBorrado);

        if (flechaIndicacion != null) flechaIndicacion.SetActive(false);
        
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = true;
        yield return StartCoroutine(EfectoObjetivoCumplido());

        yield return StartCoroutine(MostrarTexto(msgExplicacionBasura, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        // --- PASO 6: FIN DEL TUTORIAL Y COMIENZO DE LOS 2 MINUTOS ---
        yield return StartCoroutine(MostrarTexto(msgFinal, Color.white));
        textoTutorial.transform.localScale = new Vector3(tamañoZoomExito, tamañoZoomExito, 1f);
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1f);

        // Desbloqueamos temporalmente botones y activamos la oleada de 2 minutos
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = false;
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = false;
        
        if (generadorExamenes != null) generadorExamenes.generacionActiva = true;

        float tiempoRestante = 120f; // 2 minutos
        float temporizadorGeneracion = 0f;
        float intervaloGeneracion = 1.5f;

        while (tiempoRestante > 0f)
        {
            tiempoRestante -= Time.deltaTime;
            temporizadorGeneracion += Time.deltaTime;

            if (temporizadorGeneracion >= intervaloGeneracion)
            {
                temporizadorGeneracion = 0f;
                if (generadorExamenes != null) generadorExamenes.GenerarExamen();
            }

            yield return null;
        }

        // Se acaban los 2 minutos: dejamos de generar nuevos exámenes
        if (generadorExamenes != null) generadorExamenes.generacionActiva = false;

        // Esperar a que no queden enemigos con la etiqueta "Examen" en pantalla
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Examen").Length == 0);

        // --- PASO 7: MENSAJES DE VICTORIA FINAL ---
        yield return StartCoroutine(MostrarTexto(msgVictoria, Color.white));
        textoTutorial.transform.localScale = new Vector3(tamañoZoomExito, tamañoZoomExito, 1f);
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MostrarTexto(msgProximamente, Color.white));
        textoTutorial.transform.localScale = new Vector3(tamañoZoomExito, tamañoZoomExito, 1f);
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());

        // --- PASO 8: BLOQUEO FINAL DE CONTROLES ---
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = true;
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = true;
    }

    IEnumerator MostrarTexto(string mensaje, Color colorTexto)
    {
        if (textoTutorial == null) yield break;

        textoTutorial.text = mensaje;
        textoTutorial.transform.localScale = Vector3.one;
        
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * velocidadFade;
            textoTutorial.color = new Color(colorTexto.r, colorTexto.g, colorTexto.b, Mathf.Clamp01(alpha));
            yield return null;
        }
    }

    IEnumerator OcultarTexto()
    {
        if (textoTutorial == null) yield break;

        Color c = textoTutorial.color;
        float alpha = c.a;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * velocidadFade;
            textoTutorial.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));
            yield return null;
        }
    }

    IEnumerator EfectoObjetivoCumplido()
    {
        if (textoTutorial != null)
        {
            textoTutorial.text = System.Text.RegularExpressions.Regex.Replace(textoTutorial.text, "<color.*?>|</color>", "");

            textoTutorial.color = Color.green;
            textoTutorial.transform.localScale = new Vector3(tamañoZoomExito, tamañoZoomExito, 1f);
        }
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(OcultarTexto());
    }

    public void RegistrarMovimientoCamara(float cantidad) { distanciaCamaraRecorrida += cantidad; }
    public void RegistrarDisparoProfe() { profeHaDisparado = true; }
    public void RegistrarAlumnoColocado() { alumnoColocado = true; }
    public void RegistrarAlumnoBorrado() { alumnoBorrado = true; }
    public void RegistrarMuerteExamen() { examenesMuertos++; }
}