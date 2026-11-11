using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class codigoFlechitaMejora : MonoBehaviour
{
    private float velocidadFlote = 2.2f;
    private float alturaFlote = 0.05f; // Amplitud en unidades de mundo

    private SpriteRenderer miSprite;
    private Vector3 posBaseMundo;
    private Vector3 escalaBaseMundo;
    private bool inicializado = false;

    void Awake()
    {
        InicializarPosicionFija();
    }

    public void InicializarPosicionFija()
    {
        if (inicializado) return;

        miSprite = GetComponent<SpriteRenderer>();

        if (transform.parent != null)
        {
            // Calculamos su posición arriba a la derecha ignorando cualquier rotación actual del Profe
            Vector3 escalaPadre = transform.parent.localScale;
            Vector3 offsetSinRotacion = new Vector3(
                transform.localPosition.x * escalaPadre.x,
                transform.localPosition.y * escalaPadre.y,
                0f
            );

            posBaseMundo = transform.parent.position + offsetSinRotacion;
            escalaBaseMundo = new Vector3(
                transform.localScale.x * escalaPadre.x,
                transform.localScale.y * escalaPadre.y,
                1f
            );
        }
        else
        {
            posBaseMundo = transform.position;
            escalaBaseMundo = transform.localScale;
        }

        inicializado = true;
    }

    void LateUpdate()
    {
        if (!inicializado) InicializarPosicionFija();

        float onda = Mathf.Cos(Time.time * velocidadFlote);

        float desplazamientoY = onda * alturaFlote;
        transform.position = new Vector3(posBaseMundo.x, posBaseMundo.y + desplazamientoY, posBaseMundo.z);

        transform.rotation = Quaternion.identity;

        if (transform.parent != null && transform.parent.localScale.x != 0f && transform.parent.localScale.y != 0f)
        {
            transform.localScale = new Vector3(
                escalaBaseMundo.x / transform.parent.localScale.x,
                escalaBaseMundo.y / transform.parent.localScale.y,
                1f
            );
        }

        if (miSprite != null)
        {
            Color c = miSprite.color;
            float t = (onda + 1f) * 0.5f;
            c.a = Mathf.Lerp(0.85f, 1.0f, t);
            miSprite.color = c;
        }
    }
}