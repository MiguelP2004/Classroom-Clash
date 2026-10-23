using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OrdenarCapas : MonoBehaviour
{
    [Header("Configuración")]
    public int offset = 0;
    public bool ignorarEjeY = false;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (sr == null) return;

        int ordenBase = offset * 1000;

        if (ignorarEjeY)
        {
            sr.sortingOrder = ordenBase;
        }
        else
        {
            sr.sortingOrder = ordenBase - Mathf.RoundToInt(transform.position.y * 100f);
        }
    }
}