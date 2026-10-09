using UnityEngine;

public class generacionExamenes : MonoBehaviour
{
    public GameObject prefabExamen;
    public float tiempoAparicion = 2f;

    // Altura de aparición: 0 = abajo del todo, 1 = arriba del todo
    [Range(0f, 1f)]
    public float alturaAparicion = 0.5f;

    float temporizador;

    void Update()
    {
        temporizador += Time.deltaTime;
        if (temporizador >= tiempoAparicion)
        {
            temporizador = 0f;
            GenerarExamen();
        }
    }

    void GenerarExamen()
    {
        Camera cam = Camera.main;

        // Fuera de la pantalla por la izquierda, a la altura elegida
        Vector3 punto = cam.ViewportToWorldPoint(new Vector3(0f, alturaAparicion, 0f));
        Vector3 posicion = new Vector3(punto.x - 0.5f, punto.y, 0f);

        Instantiate(prefabExamen, posicion, Quaternion.identity);
    }
}