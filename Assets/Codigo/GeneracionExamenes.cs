using System.Collections;
using UnityEngine;

public class generacionExamenes : MonoBehaviour
{
    public GameObject prefabExamen;
    [HideInInspector] public bool generacionActiva = false;

    [Header("Aparición y Ruta")]
    public float posX_Aparicion = -8.5f; // Ajusta en el Inspector a la flecha de entrada
    public float posY_Aparicion = 0.2f;
    public EdgeCollider2D rutaCamino;    // Arrastra aquí el mismo objeto Camino

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
            yield return StartCoroutine(GenerarRacha(6));
            yield return new WaitForSeconds(4f);

            yield return StartCoroutine(GenerarRacha(4));
            yield return new WaitForSeconds(4f);
        }
    }

    private IEnumerator GenerarRacha(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            GenerarExamen();
            if (i < cantidad - 1)
            {
                yield return new WaitForSeconds(intervaloEntreExamenes);
            }
        }
    }

    public void GenerarExamen()
    {
        Vector3 posicion = new Vector3(posX_Aparicion, posY_Aparicion, 0f);
        GameObject nuevoExamen = Instantiate(prefabExamen, posicion, Quaternion.identity);

        if (rutaCamino != null)
        {
            codigoExamen script = nuevoExamen.GetComponent<codigoExamen>();
            if (script != null) script.AsignarRuta(rutaCamino);
        }
    }
}