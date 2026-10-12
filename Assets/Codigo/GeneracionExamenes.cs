using UnityEngine;

public class generacionExamenes : MonoBehaviour
{
    public GameObject prefabExamen;
    public float tiempoAparicion = 2f;

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

        Vector3 punto = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f));
        Vector3 posicion = new Vector3(punto.x - 0.5f, punto.y, 0f);

        Instantiate(prefabExamen, posicion, Quaternion.identity);
    }
}