using UnityEngine;
using UnityEngine.Rendering;

public class OrdenarCapas : MonoBehaviour
{
    [Header("Configuración")]
    public float offset = 0;
    public bool ignorarEjeY = false;

    private SpriteRenderer sr;
    private Canvas canvasUI;
    private SortingGroup sg;
    private ParticleSystemRenderer psr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        canvasUI = GetComponent<Canvas>(); 
        sg = GetComponent<SortingGroup>();
        psr = GetComponent<ParticleSystemRenderer>();
    }

    void LateUpdate()
    {
        int ordenBase = Mathf.RoundToInt(offset * 1000f);
        int ordenFinal = ignorarEjeY ? ordenBase : ordenBase - Mathf.RoundToInt(transform.position.y * 100f);

        if (sg != null) sg.sortingOrder = ordenFinal;
        else if (sr != null) sr.sortingOrder = ordenFinal;
        else if (psr != null) psr.sortingOrder = ordenFinal; // Aplica la profundidad al Particle System

        if (canvasUI != null) canvasUI.sortingOrder = ordenFinal;
    }
}