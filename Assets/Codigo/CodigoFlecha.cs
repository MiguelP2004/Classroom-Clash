using UnityEngine;
using UnityEngine.UI;

public class CodigoFlecha : MonoBehaviour
{
    private float velocidadFlote = 2.5f;
    private float alturaFlote = 11f;
    private float opacidadFinal = 0.8f;
    private float duracionFade = 1f;
    private Image miImagen;
    private float posY_Base;
    private float tiempoFade;

    void Awake()
    {
        miImagen = GetComponent<Image>();
        posY_Base = transform.localPosition.y; // Guardamos su altura base
    }

    void OnEnable()
    {
        tiempoFade = 0f;

        if (miImagen != null)
        {
            Color c = miImagen.color;
            c.a = 0f;
            miImagen.color = c;
        }
    }

    void Update()
    {
        tiempoFade += Time.deltaTime;

        float faseFlote = Time.time * velocidadFlote;
        float movimientoSuave = Mathf.Cos(faseFlote);

        if (miImagen != null)
        {
            Color c = miImagen.color;
            float progresoFade = duracionFade > 0f ? tiempoFade / duracionFade : 1f;
            c.a = Mathf.SmoothStep(0f, opacidadFinal, Mathf.Clamp01(progresoFade));
            miImagen.color = c;
        }

        float desplazamientoY = movimientoSuave * alturaFlote;
        
        Vector3 nuevaPosicion = transform.localPosition;
        nuevaPosicion.y = posY_Base + desplazamientoY;
        transform.localPosition = nuevaPosicion;
    }
}