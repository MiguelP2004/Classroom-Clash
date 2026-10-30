using UnityEngine;

public class OrdenarCapas : MonoBehaviour
{
    [Header("Configuración")]
    public float offset = 0;
    public bool ignorarEjeY = false;

    private SpriteRenderer sr;
    private Canvas canvasUI;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        canvasUI = GetComponent<Canvas>(); 
    }

    void LateUpdate()
    {
        int ordenBase = Mathf.RoundToInt(offset * 1000f);
        int ordenFinal = ignorarEjeY ? ordenBase : ordenBase - Mathf.RoundToInt(transform.position.y * 100f);

        if (sr != null) sr.sortingOrder = ordenFinal;
        if (canvasUI != null) canvasUI.sortingOrder = ordenFinal;
    }
}