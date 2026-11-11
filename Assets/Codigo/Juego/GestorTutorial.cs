using UnityEngine;
using TMPro;
using System.Collections;

public class GestorTutorial : MonoBehaviour
{
    public static GestorTutorial instancia;

    [Header("Referencias del Juego")]
    public TextMeshProUGUI textoTutorial;
    public EfectoBoton botonAlumno;
    public EfectoBoton botonMejoraProfe;
    public EfectoBoton botonBasura;
    public generacionExamenes generadorExamenes;
    public CodigoProfe profe;
    public GameObject objetoTrofeo;

    [Header("Para el tutorial")]
    public float velocidadFade = 2f;
    public float tamañoZoomExito = 1.2f;

    public GameObject flechaIndicacion;
    
    [Header("Música de Combate")]
    public AudioClip musicaGameplay;

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
    private string msgComprarMejora = "VAMOS A PROBAR LA PRIMERA\n<color=#51DBFF><size=60>MEJORA DEL PROFESOR</size></color>\nCOMPRALA PARA OBTENER SU <color=#F3FF00><size=60>DISPARO DOBLE!!</size></color>";
    private string msgProbarMejora = "<color=#F3FF00><size=60>VAMOS A PROBARLO!</size></color>";
    private string msgExplicacionMejora = "LAS <color=#51DBFF><size=60>MEJORAS DEL PROFESOR</size></color> SON PERMANENTES Y MULTIPLICAN TU POTENCIA DE DISPARO PARA FRENAR GRANDES OLEADAS";
    private string msgFinal = "<color=#F3FF00><size=60>ENHORABUENA</size></color>\nHAS APRENDIDO LO BASICO\nCUIDADO QUE SE ACERCAN MAS <color=#FF5151><size=60>EXAMENES!</size></color>\nPON EN PRACTICA TUS HABILIDADES!";
    private string msgVictoria = "<color=#F3FF00><size=60>ENHORABUENA</size></color>\nHAS TERMINADO EL TUTORIAL!!";

    private float distanciaCamaraRecorrida = 0f;
    private bool profeHaDisparado = false;
    private bool alumnoColocado = false;
    private bool alumnoBorrado = false;
    private bool mejoraProfeComprada = false;
    private int examenesMuertos = 0;

    void Awake()
    {
        if (instancia == null) instancia = this;
        if (objetoTrofeo != null) objetoTrofeo.SetActive(false);
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

        if (MenuPrincipal.nivelElegido <= 1)
        {
            StartCoroutine(SecuenciaTutorial());
        }
        else
        {
            StartCoroutine(SecuenciaNivelNormal());
        }
    }

    void Update()
    {
        if (textoTutorial != null)
        {
            textoTutorial.transform.localScale = Vector3.Lerp(textoTutorial.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 15f);
        }
    }

    // Secuencia para el Nivel 2 (y posteriores): libertad total y cuenta atrás de 10 a 1
    IEnumerator SecuenciaNivelNormal()
    {
        if (generadorExamenes != null) generadorExamenes.generacionActiva = false;
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = false;
        if (botonMejoraProfe != null) botonMejoraProfe.bloqueadoPorTutorial = false;
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = false;
        if (profe != null) profe.bloqueadoPorTutorial = false;
        if (ControladorJuego.instancia != null) ControladorJuego.instancia.camaraBloqueadaPorTutorial = false;

        yield return new WaitForSeconds(1f);

        if (textoTutorial != null)
        {
            textoTutorial.text = "<color=#F3FF00><size=60>PREPARATE</size></color>\nLOS ENEMIGOS VENDRAN EN <color=#FF5151><size=60>10</size></color>";
            yield return StartCoroutine(MostrarTexto(textoTutorial.text, Color.white));

            for (int seg = 10; seg >= 1; seg--)
            {
                textoTutorial.text = "<color=#F3FF00><size=60>PREPARATE</size></color>\nLOS ENEMIGOS VENDRAN EN <color=#FF5151><size=60>" + seg + "</size></color>";
                textoTutorial.transform.localScale = new Vector3(1.12f, 1.12f, 1f);
                yield return new WaitForSeconds(1f);
            }

            yield return StartCoroutine(OcultarTexto());
        }
        else
        {
            yield return new WaitForSeconds(10f);
        }

        if (ControladorJuego.instancia != null && musicaGameplay != null)
        {
            ControladorJuego.instancia.CambiarMusicaFondo(musicaGameplay);
        }

        if (generadorExamenes != null)
        {
            generadorExamenes.generacionActiva = true;
        }
    }

    IEnumerator SecuenciaTutorial()
    {
        if (generadorExamenes != null) generadorExamenes.generacionActiva = false;
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = true;
        if (botonMejoraProfe != null) botonMejoraProfe.bloqueadoPorTutorial = true;
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = true;
        if (profe != null) profe.bloqueadoPorTutorial = true;
        if (ControladorJuego.instancia != null) ControladorJuego.instancia.camaraBloqueadaPorTutorial = true;

        yield return new WaitForSeconds(1.5f);

        // 1. Bienvenida
        yield return StartCoroutine(MostrarTexto(msgBienvenida, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        // 2. Mover Cámara
        if (ControladorJuego.instancia != null) ControladorJuego.instancia.camaraBloqueadaPorTutorial = false;
        yield return StartCoroutine(MostrarTexto(msgMoverCamara, Color.white));
        
        distanciaCamaraRecorrida = 0f;
        yield return new WaitUntil(() => distanciaCamaraRecorrida >= 2.5f);
        yield return StartCoroutine(EfectoObjetivoCumplido());
        yield return StartCoroutine(MostrarTexto(msgExplicacionCamara, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        // 3. Disparar con el Profesor
        if (profe != null) profe.bloqueadoPorTutorial = false;

        yield return StartCoroutine(MostrarTexto(msgDisparar, Color.white));
        profeHaDisparado = false;
        yield return new WaitUntil(() => profeHaDisparado);
        yield return StartCoroutine(EfectoObjetivoCumplido());
        yield return StartCoroutine(MostrarTexto(msgExplicacionProfe, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        // 4. Plantar Alumno
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = false;
        yield return StartCoroutine(MostrarTexto(msgPlantarAlumno, Color.white));

        if (flechaIndicacion != null)
        {
            flechaIndicacion.SetActive(true);
            Vector3 pos = flechaIndicacion.transform.localPosition;
            pos.x = botonAlumno.transform.localPosition.x; 
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

        // 5. Primera oleada (5 exámenes)
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

        // 6. Usar Basura
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = false;
        yield return StartCoroutine(MostrarTexto(msgUsarBasura, Color.white));

        if (flechaIndicacion != null)
        {
            flechaIndicacion.SetActive(true);
            Vector3 pos = flechaIndicacion.transform.localPosition;
            pos.x = botonBasura.transform.localPosition.x; 
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

        // 7. Comprar Mejora x2 del Profesor
        if (botonMejoraProfe != null) botonMejoraProfe.bloqueadoPorTutorial = false;
        yield return StartCoroutine(MostrarTexto(msgComprarMejora, Color.white));

        if (flechaIndicacion != null && botonMejoraProfe != null)
        {
            flechaIndicacion.SetActive(true);
            Vector3 pos = flechaIndicacion.transform.localPosition;
            pos.x = botonMejoraProfe.transform.localPosition.x; 
            flechaIndicacion.transform.localPosition = pos;
        }

        mejoraProfeComprada = false;
        yield return new WaitUntil(() => mejoraProfeComprada);

        if (flechaIndicacion != null) flechaIndicacion.SetActive(false);
        if (botonMejoraProfe != null) botonMejoraProfe.bloqueadoPorTutorial = true;

        yield return StartCoroutine(EfectoObjetivoCumplido());

        // 8. Probar el disparo doble con 8 exámenes cada 0.5s
        yield return StartCoroutine(MostrarTexto(msgProbarMejora, Color.white));
        textoTutorial.transform.localScale = new Vector3(tamañoZoomExito, tamañoZoomExito, 1f);

        examenesMuertos = 0;
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 8; i++)
        {
            if (generadorExamenes != null) generadorExamenes.GenerarExamen();
            yield return new WaitForSeconds(0.5f);
        }

        yield return StartCoroutine(OcultarTexto());
        yield return new WaitUntil(() => examenesMuertos >= 8);

        // 9. Información importante sobre las mejoras del profesor
        yield return StartCoroutine(MostrarTexto(msgExplicacionMejora, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        // 10. Batalla Final del Tutorial
        yield return StartCoroutine(MostrarTexto(msgFinal, Color.white));
        textoTutorial.transform.localScale = new Vector3(tamañoZoomExito, tamañoZoomExito, 1f);
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1f);

        if (ControladorJuego.instancia != null && musicaGameplay != null)
        {
            ControladorJuego.instancia.CambiarMusicaFondo(musicaGameplay);
        }

        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = false;
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = false;
        yield return new WaitForSeconds(1f);

        if (generadorExamenes != null) generadorExamenes.generacionActiva = true;
        yield return new WaitForSeconds(60f);

        if (generadorExamenes != null) generadorExamenes.generacionActiva = false;

        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Examen").Length == 0);

        yield return StartCoroutine(MostrarTexto(msgVictoria, Color.white));
        textoTutorial.transform.localScale = new Vector3(tamañoZoomExito, tamañoZoomExito, 1f);
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(0.5f);

        if (objetoTrofeo != null)
        {
            objetoTrofeo.SetActive(true);
        }
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
        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(OcultarTexto());
    }

    public void RegistrarMovimientoCamara(float cantidad) { distanciaCamaraRecorrida += cantidad; }
    public void RegistrarDisparoProfe() { profeHaDisparado = true; }
    public void RegistrarAlumnoColocado() { alumnoColocado = true; }
    public void RegistrarAlumnoBorrado() { alumnoBorrado = true; }
    public void RegistrarMejoraProfe() { mejoraProfeComprada = true; }
    public void RegistrarMuerteExamen() { examenesMuertos++; }
}