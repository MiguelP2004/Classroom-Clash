using UnityEngine;

public class generacionExamenes : MonoBehaviour
{
    public GameObject prefabExamen;
    public float tiempoAparicion = 2f;
    [HideInInspector] public bool generacionActiva = false; // Controlado por el tutorial

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

    public void GenerarExamen() // Ahora es public para que el tutorial pueda llamarlo
    {
        Camera cam = Camera.main;
        float bordeIzquierdoX = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        Vector3 posicion = new Vector3(bordeIzquierdoX - 0.5f, Random.Range(-1f, 1f), 0f);

        Instantiate(prefabExamen, posicion, Quaternion.identity);
    }
}