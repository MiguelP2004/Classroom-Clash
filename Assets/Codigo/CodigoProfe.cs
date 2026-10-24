using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CodigoProfe : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject proyectilPrefab;
    public float cadenciaFuego = 0.5f;

    private float fuerzaRetroceso = 0.1f;
    private float tiempoAtras = 0.1f;
    private float tiempoRecuperacion = 0.2f;

    private float temporizadorDisparo;
    private Coroutine rutinaRetroceso;
    private Vector3 posicionOriginal;

    void Start()
    {
        posicionOriginal = transform.position;
    }

    void Update()
    {
        if (Mouse.current == null || Camera.main == null) return;

        Vector3 posicionRaton = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direccion = posicionRaton - transform.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angulo);

        temporizadorDisparo += Time.deltaTime;
        
        if (Mouse.current.leftButton.isPressed && temporizadorDisparo >= cadenciaFuego)
        {
            Disparar();
            temporizadorDisparo = 0f;
        }
    }

    void Disparar()
    {
        if (proyectilPrefab != null)
        {
            Instantiate(proyectilPrefab, transform.position, transform.rotation);
        }

        if (rutinaRetroceso != null) StopCoroutine(rutinaRetroceso);
        rutinaRetroceso = StartCoroutine(EfectoRetrocesoVisual());
    }

    IEnumerator EfectoRetrocesoVisual()
    {
        float tiempo = 0f;
        Vector3 posAtras = posicionOriginal - (transform.right * fuerzaRetroceso);

        while (tiempo < tiempoAtras)
        {
            tiempo += Time.deltaTime;
            transform.position = Vector3.Lerp(posicionOriginal, posAtras, tiempo / tiempoAtras);
            yield return null;
        }

        tiempo = 0f;

        while (tiempo < tiempoRecuperacion)
        {
            tiempo += Time.deltaTime;
            transform.position = Vector3.Lerp(posAtras, posicionOriginal, tiempo / tiempoRecuperacion);
            yield return null;
        }

        transform.position = posicionOriginal;
        rutinaRetroceso = null;
    }
}