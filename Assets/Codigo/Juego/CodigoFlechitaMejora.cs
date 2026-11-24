using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class codigoFlechitaMejora : MonoBehaviour
{
    [Header("Sprites de Mejora")]
    public Sprite spriteNivel3; // Arrastra aquí el sprite "mejora 2"

    [Header("Offsets de Posición")]
    public Vector2 offsetProfe = Vector2.zero;
    public Vector2 offsetAlumno = Vector2.zero;

    private float velocidadFlote = 2.2f;
    private float alturaFlote = 0.05f;

    private SpriteRenderer miSprite;
    private Vector3 localPosPrefab;
    private Vector3 localScalePrefab;
    private Vector3 centroPadreBase;
    private Vector3 offsetPrefabBase;
    private Vector3 escalaBaseMundo;
    private bool esProfe = false;
    private bool inicializado = false;

    void Awake()
    {
        miSprite = GetComponent<SpriteRenderer>();
        localPosPrefab = transform.localPosition;
        localScalePrefab = transform.localScale;
    }

    public void Inicializar(Vector3 centroPadreMundo, Vector3 escalaPadreOriginal)
    {
        if (miSprite == null) miSprite = GetComponent<SpriteRenderer>();

        esProfe = (transform.parent != null && transform.parent.GetComponent<CodigoProfe>() != null);
        centroPadreBase = centroPadreMundo;

        offsetPrefabBase = new Vector3(
            localPosPrefab.x * escalaPadreOriginal.x,
            localPosPrefab.y * escalaPadreOriginal.y,
            0f
        );

        escalaBaseMundo = new Vector3(
            localScalePrefab.x * escalaPadreOriginal.x,
            localScalePrefab.y * escalaPadreOriginal.y,
            1f
        );

        AplicarTransformacion(0f);
        inicializado = true;
    }

    public void CambiarASpriteNivel3()
    {
        if (miSprite == null) miSprite = GetComponent<SpriteRenderer>();

        if (miSprite != null && spriteNivel3 != null)
        {
            miSprite.sprite = spriteNivel3;
        }
    }

    void LateUpdate()
    {
        if (!inicializado && transform.parent != null)
        {
            Inicializar(transform.parent.position, transform.parent.localScale);
        }

        float onda = Mathf.Cos(Time.time * velocidadFlote);
        AplicarTransformacion(onda * alturaFlote);

        if (miSprite != null)
        {
            Color c = miSprite.color;
            float t = (onda + 1f) * 0.5f;
            c.a = Mathf.Lerp(0.85f, 1.0f, t);
            miSprite.color = c;
        }
    }

    private void AplicarTransformacion(float desplazamientoY)
    {
        Vector2 offsetExtra = esProfe ? offsetProfe : offsetAlumno;

        transform.position = new Vector3(
            centroPadreBase.x + offsetPrefabBase.x + offsetExtra.x,
            centroPadreBase.y + offsetPrefabBase.y + offsetExtra.y + desplazamientoY,
            0f
        );

        transform.rotation = Quaternion.identity;

        if (transform.parent != null && transform.parent.localScale.x != 0f && transform.parent.localScale.y != 0f)
        {
            transform.localScale = new Vector3(
                escalaBaseMundo.x / transform.parent.localScale.x,
                escalaBaseMundo.y / transform.parent.localScale.y,
                1f
            );
        }
    }
}