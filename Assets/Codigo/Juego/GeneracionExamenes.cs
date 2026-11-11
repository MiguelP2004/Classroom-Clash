using System.Collections;
using UnityEngine;
using TMPro;

public class generacionExamenes : MonoBehaviour
{
    public GameObject examenBasico;
    public GameObject examenPesado;
    public GameObject examenVeloz;
    [HideInInspector] public bool generacionActiva = false;

    [Header("Aparición y Ruta")]
    public float posX_Aparicion = -8.5f; 
    public float posY_Aparicion = 0.2f;
    public EdgeCollider2D rutaCamino;    

    [Header("Interfaz de oleadas")]
    public TMP_Text textoOleada;
    public GestorTutorial gestorTutorial;

    private float intervaloEntreExamenes = 0.75f;
    private Coroutine rutinaGeneracion;
    private int enemigosVivos = 0;

    private void OnDisable()
    {
        rutinaGeneracion = null;
    }

    void Update()
    {
        if (generacionActiva)
        {
            if (rutinaGeneracion == null)
            {
                rutinaGeneracion = StartCoroutine(RutinaGeneracion());
            }
        }
        else if (rutinaGeneracion != null)
        {
            StopCoroutine(rutinaGeneracion);
            rutinaGeneracion = null;
        }
    }

    private IEnumerator RutinaGeneracion()
    {
        if (MenuPrincipal.nivelElegido <= 1)
        {
            while (true)
            {
                yield return StartCoroutine(GenerarRachaEspecifica(examenBasico, 6, intervaloEntreExamenes));
                yield return new WaitForSeconds(4f);

                GenerarUnExamen(examenPesado);
                yield return new WaitForSeconds(intervaloEntreExamenes);
                GenerarUnExamen(examenBasico);
                yield return new WaitForSeconds(intervaloEntreExamenes);
                GenerarUnExamen(examenPesado);
                
                yield return new WaitForSeconds(4f);
            }
        }
        else
        {
            yield return StartCoroutine(GenerarOleadas());
        }
    }

    private IEnumerator GenerarOleadas()
    {
        for (int oleada = 1; oleada <= 6; oleada++)
        {
            if (textoOleada != null)
            {
                textoOleada.text = "OLEADA " + oleada + " / 6";
            }
            // Mostrar aviso de inicio de oleada
            if (GestorTutorial.instancia != null)
            {
                yield return StartCoroutine(
                    GestorTutorial.instancia.MostrarAvisoOleada(oleada)
                );
            }
            // Cada oleada aumenta 2 enemigos de cada tipo.
            int basicos = 5 + (oleada - 1) * 4;
            int pesados = 2 + (oleada - 1) * 4;
            int veloces = 4 + (oleada - 1) * 4;

            // Primera tanda: básicos
            yield return StartCoroutine(
                GenerarRachaEspecifica(examenBasico, basicos, 0.5f));

            yield return new WaitForSeconds(2f);

            // Segunda tanda: pesados
            yield return StartCoroutine(
                GenerarRachaEspecifica(examenPesado, pesados, 0.5f));

            yield return new WaitForSeconds(2f);

            // Tercera tanda: veloces
            yield return StartCoroutine(
                GenerarRachaEspecifica(examenVeloz, veloces, 0.25f));
            
            while (enemigosVivos > 0)
            {
                yield return null;
            }
            
            // Descanso antes de la siguiente oleada
            yield return new WaitForSeconds(1f);
        }
        if (GestorTutorial.instancia != null)
        {
            yield return StartCoroutine(
                GestorTutorial.instancia.MostrarVictoria()
            );
        }
        // Aquí terminan las 6 oleadas.
    }

    private IEnumerator GenerarRachaEspecifica(GameObject prefab, int cantidad, float intervalo)
    {
        for (int i = 0; i < cantidad; i++)
        {
            GenerarUnExamen(prefab);
            if (i < cantidad - 1)
            {
                yield return new WaitForSeconds(intervalo);
            }
        }
    }

    public void GenerarUnExamen(GameObject prefabElegido)
    {
        if (prefabElegido == null) return;
        
        Vector3 posicion = new Vector3(posX_Aparicion, posY_Aparicion, 0f);
        GameObject nuevoExamen = Instantiate(prefabElegido, posicion, Quaternion.identity);

        enemigosVivos++;

        codigoExamen script = nuevoExamen.GetComponent<codigoExamen>();

        if (script != null)
        {
            script.AsignarGenerador(this);

            if (rutaCamino != null)
            {
                script.AsignarRuta(rutaCamino);
            }
        }
    }

    public void EnemigoEliminado()
    {
        enemigosVivos--;

        if (enemigosVivos < 0)
        {
            enemigosVivos = 0;
        }
    }

    public void GenerarExamen()
    {
        GenerarUnExamen(examenBasico);
    }
}