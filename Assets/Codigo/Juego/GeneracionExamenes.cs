using System.Collections;
using UnityEngine;

public class generacionExamenes : MonoBehaviour
{
    public GameObject examenBasico;
    public GameObject examenPesado;
    [HideInInspector] public bool generacionActiva = false;

    [Header("Aparición y Ruta")]
    public float posX_Aparicion = -8.5f; 
    public float posY_Aparicion = 0.2f;
    public EdgeCollider2D rutaCamino;    

    private float intervaloEntreExamenes = 0.75f;
    private Coroutine rutinaGeneracion;

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
        while (true)
        {
            yield return StartCoroutine(GenerarRachaBasicos(6));
            yield return new WaitForSeconds(4f);

            GenerarUnExamen(examenPesado);
            yield return new WaitForSeconds(intervaloEntreExamenes);
            GenerarUnExamen(examenBasico);
            yield return new WaitForSeconds(intervaloEntreExamenes);
            GenerarUnExamen(examenPesado);
            
            yield return new WaitForSeconds(4f);
        }
    }

    private IEnumerator GenerarRachaBasicos(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            GenerarUnExamen(examenBasico);
            if (i < cantidad - 1)
            {
                yield return new WaitForSeconds(intervaloEntreExamenes);
            }
        }
    }

    public void GenerarUnExamen(GameObject prefabElegido)
    {
        if (prefabElegido == null) return;
        
        Vector3 posicion = new Vector3(posX_Aparicion, posY_Aparicion, 0f);
        GameObject nuevoExamen = Instantiate(prefabElegido, posicion, Quaternion.identity);

        if (rutaCamino != null)
        {
            codigoExamen script = nuevoExamen.GetComponent<codigoExamen>();
            if (script != null) script.AsignarRuta(rutaCamino);
        }
    }

    public void GenerarExamen()
    {
        GenerarUnExamen(examenBasico);
    }
}