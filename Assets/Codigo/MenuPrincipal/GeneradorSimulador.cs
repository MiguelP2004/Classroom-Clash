using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorSimulador : MonoBehaviour
{
    [System.Serializable]
    public struct PosicionAlumno
    {
        public float x;
        public float y;
    }

    [Header("Simulación de Alumnos")]
    // ¡NUEVO! Array para meter a todos los tipos de alumnos posibles
    public GameObject[] tiposDeAlumnos; 
    public int cantidadAlumnosAColocar = 5;
    public List<PosicionAlumno> posiblesPosiciones = new List<PosicionAlumno>();

    [Header("Simulación de Exámenes")]
    public GameObject[] tiposDeExamenes;
    public float posX_Aparicion = -8.5f;
    public float posY_Aparicion = 0.2f;
    public EdgeCollider2D rutaCamino;
    
    private float intervaloEntreEnemigos = 0.5f;
    private float pausaEntreOleadas = 2.5f;

    void Start()
    {
        ColocarAlumnosRandomDeGolpe();
        StartCoroutine(RutinaGenerarExamenesInfinitos());
    }

    private void ColocarAlumnosRandomDeGolpe()
    {
        // Si no hay alumnos asignados en la lista, salimos para no dar error
        if (tiposDeAlumnos == null || tiposDeAlumnos.Length == 0 || posiblesPosiciones.Count == 0) return;

        List<PosicionAlumno> bolsaPosiciones = new List<PosicionAlumno>(posiblesPosiciones);
        int cantidadReal = Mathf.Min(cantidadAlumnosAColocar, bolsaPosiciones.Count);

        for (int i = 0; i < cantidadReal; i++)
        {
            // 1. Elegimos una posición al azar y la quitamos de la bolsa
            int indiceRandom = Random.Range(0, bolsaPosiciones.Count);
            PosicionAlumno posElegida = bolsaPosiciones[indiceRandom];
            bolsaPosiciones.RemoveAt(indiceRandom);
            Vector3 posicionFinal = new Vector3(posElegida.x, posElegida.y, 0f);

            // 2. Elegimos un alumno al azar de la nueva lista
            GameObject prefabElegido = tiposDeAlumnos[Random.Range(0, tiposDeAlumnos.Length)];

            if (prefabElegido != null)
            {
                // 3. Lo instanciamos en la posición elegida
                GameObject nuevoAlumno = Instantiate(prefabElegido, posicionFinal, Quaternion.identity);

                CodigoAlumno scriptAlumno = nuevoAlumno.GetComponent<CodigoAlumno>();
                if (scriptAlumno != null)
                {
                    scriptAlumno.estaColocado = true;
                }
            }
        }
    }

    private IEnumerator RutinaGenerarExamenesInfinitos()
    {
        // Espera mínima para evitar bloqueos del editor al compilar/arrancar
        yield return new WaitForSeconds(0.1f);

        while (true)
        {
            int cantidadEnemigosOleada = Random.Range(3, 6);

            for (int i = 0; i < cantidadEnemigosOleada; i++)
            {
                GenerarExamenAleatorio();
                if (i < cantidadEnemigosOleada - 1)
                {
                    yield return new WaitForSeconds(intervaloEntreEnemigos);
                }
            }

            yield return new WaitForSeconds(pausaEntreOleadas);
        }
    }

    private void GenerarExamenAleatorio()
    {
        if (tiposDeExamenes == null || tiposDeExamenes.Length == 0) return;

        GameObject prefabElegido = tiposDeExamenes[Random.Range(0, tiposDeExamenes.Length)];
        if (prefabElegido == null) return;

        Vector3 posicion = (rutaCamino != null && rutaCamino.pointCount > 0) 
            ? rutaCamino.transform.TransformPoint(rutaCamino.points[0]) 
            : new Vector3(posX_Aparicion, posY_Aparicion, 0f);
            
        GameObject nuevoExamen = Instantiate(prefabElegido, posicion, Quaternion.identity);

        if (rutaCamino != null)
        {
            codigoExamen script = nuevoExamen.GetComponent<codigoExamen>();
            if (script != null) script.AsignarRuta(rutaCamino);
        }
    }
}