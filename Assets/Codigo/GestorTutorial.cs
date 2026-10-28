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

    private string msgBienvenida = "BIENVENIDO A <color=green>CLASSROOM CLASH</color>!!\n ESTE ES EL TUTORIAL"; 
    private string msgDisparar = "PRESIONA <color=red>CLICK IZQUIERDO</color> CON EL MOUSE PARA DISPARAR CON EL PROFESOR";
    private string msgExplicacionProfe = "EL PROFESOR APUNTA A TU MOUSE Y ES ESENCIAL PARA DAR APOYO A TUS ALUMNOS";
    private string msgPlantarAlumno = "SELECCIONA EL <color=red>BOTON DEL ALUMNO</color> PARA DESPLEGAR UN ALUMNO EN EL AULA";
    private string msgExplicacionAlumno = "LOS ALUMNOS DISPARAN AUTOMATICAMENTE A LOS EXAMENES DENTRO DE SU RANGO\n COLOCA TANTOS COMO PUEDAS!";
    private string msgAlertaExamenes = "CUIDADO, SE ACERCA UN GRUPO DE EXAMENES!!\n ACABA CON ELLOS!!";
    private string msgUsarBasura = "PERFECTO, ZONA LIMPIA. BUEN TRABAJO!!\n USA LA <color=red>BASURA</color> PARA RECOGER AL ALUMNO Y QUITARLO DEL AULA";
    private string msgExplicacionBasura = "LA BASURA ES GRATIS Y TE PERMITE RECOGER A LOS ALUMNOS PARA ORGANIZAR MEJOR TUS DEFENSAS";
    private string msgFinal = "ENHORABUENA, HAS TERMINADO EL TUTORIAL!!\n CUIDADO QUE SE ACERCAN MAS EXAMENES!!";

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
        // Iniciamos con el texto invisible
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
            textoTutorial.transform.localScale += (Vector3.one - textoTutorial.transform.localScale) / 25f;
        }
    }

    IEnumerator SecuenciaTutorial()
    {
        if (generadorExamenes != null) generadorExamenes.generacionActiva = false;
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = true;
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = true;
        if (profe != null) profe.bloqueadoPorTutorial = true;

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(MostrarTexto(msgBienvenida, Color.white));
        yield return new WaitForSeconds(4.5f); // Tiempo para leerlo
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        if (profe != null) profe.bloqueadoPorTutorial = false; // ¡Desbloqueamos el disparo!

        yield return StartCoroutine(MostrarTexto(msgDisparar, Color.white));
        profeHaDisparado = false;
        yield return new WaitUntil(() => profeHaDisparado); // Espera el disparo
        yield return StartCoroutine(EfectoObjetivoCumplido());
        yield return StartCoroutine(MostrarTexto(msgExplicacionProfe, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());

        yield return new WaitForSeconds(1.5f);

        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = false; // Desbloqueamos solo el alumno
        yield return StartCoroutine(MostrarTexto(msgPlantarAlumno, Color.white));

        if (flechaIndicacion != null)
        {
            flechaIndicacion.SetActive(true);
            Vector3 pos = flechaIndicacion.transform.localPosition;
            pos.x = posX_Alumno;
            flechaIndicacion.transform.localPosition = pos;
        }
        
        alumnoColocado = false;
        yield return new WaitUntil(() => alumnoColocado); // Espera a que lo plante en el suelo

        if (flechaIndicacion != null) flechaIndicacion.SetActive(false);
        
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = true; 
        yield return StartCoroutine(EfectoObjetivoCumplido());
        yield return StartCoroutine(MostrarTexto(msgExplicacionAlumno, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(MostrarTexto(msgAlertaExamenes, Color.red));
        
        examenesMuertos = 0;
        for (int i = 0; i < 5; i++)
        {
            if (generadorExamenes != null) generadorExamenes.GenerarExamen();
            yield return new WaitForSeconds(1.5f);
        }
        
        yield return StartCoroutine(OcultarTexto());

        // Esperamos hasta que los 5 exámenes hayan muerto
        yield return new WaitUntil(() => examenesMuertos >= 5);

        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = false; // Desbloqueamos solo la basura
        yield return StartCoroutine(MostrarTexto(msgUsarBasura, Color.white));

        if (flechaIndicacion != null)
        {
            flechaIndicacion.SetActive(true);
            Vector3 pos = flechaIndicacion.transform.localPosition;
            pos.x = posX_Basura;
            flechaIndicacion.transform.localPosition = pos;
        }

        alumnoBorrado = false;
        yield return new WaitUntil(() => alumnoBorrado); // Espera a que borre al alumno

        if (flechaIndicacion != null) flechaIndicacion.SetActive(false);
        
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = true;
        yield return StartCoroutine(EfectoObjetivoCumplido());

        yield return StartCoroutine(MostrarTexto(msgExplicacionBasura, Color.white));
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(MostrarTexto(msgFinal, Color.white));
        textoTutorial.transform.localScale = new Vector3(tamañoZoomExito, tamañoZoomExito, 1f);
        yield return new WaitForSeconds(4.5f);
        yield return StartCoroutine(OcultarTexto());

        // Liberamos todos los botones y activamos el generador infinito
        if (botonAlumno != null) botonAlumno.bloqueadoPorTutorial = false;
        if (botonBasura != null) botonBasura.bloqueadoPorTutorial = false;
        if (generadorExamenes != null) generadorExamenes.generacionActiva = true;
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

    public void RegistrarDisparoProfe() { profeHaDisparado = true; }
    public void RegistrarAlumnoColocado() { alumnoColocado = true; }
    public void RegistrarAlumnoBorrado() { alumnoBorrado = true; }
    public void RegistrarMuerteExamen() { examenesMuertos++; }
}