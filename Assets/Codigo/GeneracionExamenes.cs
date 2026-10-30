using UnityEngine;

public class generacionExamenes : MonoBehaviour
{
    public GameObject prefabExamen;
    public float tiempoAparicion = 2f;
    [HideInInspector] public bool generacionActiva = false;

    [Header("Aparición y Ruta")]
    public float posX_Aparicion = -8.5f; // Ajusta en el Inspector a la flecha de entrada
    public float posY_Aparicion = 0.2f;
    public EdgeCollider2D rutaCamino;    // Arrastra aquí el mismo objeto Camino

    float temporizador;

    void Update()
    {
        if (!generacionActiva) return;

        temporizador += Time.deltaTime;
        if (temporizador >= tiempoAparicion)
        {
            temporizador = 0f;
            GenerarExamen();
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