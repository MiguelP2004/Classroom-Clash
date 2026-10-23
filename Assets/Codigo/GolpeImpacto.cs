using UnityEngine;
using System.Collections;

public class GolpeImpacto : MonoBehaviour
{
    public Sprite[] sprites;
    private float tiempoEntreFrames = 0.05f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        if (sr != null)
        {
            Color colorFondo = sr.color;
            colorFondo.a = 0.8f;
            sr.color = colorFondo;
        }

        StartCoroutine(SecuenciaImpacto());
    }

    IEnumerator SecuenciaImpacto()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sr != null) sr.sprite = sprites[i];
            yield return new WaitForSeconds(tiempoEntreFrames);
        }

        Destroy(gameObject);
    }
}