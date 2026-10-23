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

        float bordeIzquierdoX = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;

        Vector3 posicion = new Vector3(bordeIzquierdoX - 0.5f, Random.Range(-1f, 1f), 0f);

        Instantiate(prefabExamen, posicion, Quaternion.identity);
    }
}