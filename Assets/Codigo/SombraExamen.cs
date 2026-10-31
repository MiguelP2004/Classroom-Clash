using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SombraExamen : MonoBehaviour
{
    public float offsetY = -0.4f;

    private Transform objetivo;
    private SpriteRenderer spriteObjetivo;
    private SpriteRenderer miSprite;
    private float alphaBase;

    public void Inicializar(Transform nuevoObjetivo, SpriteRenderer nuevoSprite)
    {
        objetivo = nuevoObjetivo;
        spriteObjetivo = nuevoSprite;
        
        miSprite = GetComponent<SpriteRenderer>();
        alphaBase = miSprite.color.a;
    }

    void LateUpdate()
    {
        if (objetivo == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = objetivo.position + new Vector3(0f, offsetY, 0f);
        
        transform.rotation = Quaternion.identity;

        if (spriteObjetivo != null && miSprite != null)
        {
            Color colorSombra = miSprite.color;
            colorSombra.a = spriteObjetivo.color.a * alphaBase; 
            miSprite.color = colorSombra;
        }
    }
}