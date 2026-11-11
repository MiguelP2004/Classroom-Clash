using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CodigoTrofeo : MonoBehaviour
{
    [Header("Audio de Victoria")]
    public AudioClip sonidoVictoria;

    [Header("Salto Inicial")]
    private float alturaPrimerSalto = 1.3f;
    private float duracionPrimerSalto = 0.55f;
    
    private Vector3 posicionAparicion = new Vector3(-4f, 0f, 0f);

    [Header("Flote y Brillo (Coseno)")]
    private float velocidadCoseno = 3.2f;
    public float tamañoAgrandado = 1.15f; // Multiplicador de tamaño máximo

    private SpriteRenderer miRenderer;
    private bool puedeRecogerse = false;
    private bool recogido = false;
    private float tiempoCoseno = 0f;
    
    private Vector3 escalaInicial; // Guarda el tamaño original puesto en el Editor

    void Awake()
    {
        miRenderer = GetComponent<SpriteRenderer>();

        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;
        }

        // Registramos el tamaño que le hayas puesto a mano en Unity
        escalaInicial = transform.localScale; 
    }

    void OnEnable()
    {
        recogido = false;
        puedeRecogerse = false;
        tiempoCoseno = 0f;

        transform.position = posicionAparicion;
        transform.localScale = escalaInicial;

        StartCoroutine(RutinaSaltoYRebote());
    }

    private IEnumerator RutinaSaltoYRebote()
    {
        // 1. Salto principal
        yield return StartCoroutine(ArcoRebote(alturaPrimerSalto, duracionPrimerSalto));
        // 2. Primer rebote
        yield return StartCoroutine(ArcoRebote(alturaPrimerSalto * 0.35f, duracionPrimerSalto * 0.6f));
        // 3. Segundo rebote pequeño
        yield return StartCoroutine(ArcoRebote(alturaPrimerSalto * 0.12f, duracionPrimerSalto * 0.4f));

        transform.position = posicionAparicion;
        puedeRecogerse = true;
    }

    private IEnumerator ArcoRebote(float altura, float duracion)
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempo / duracion);
            float y = Mathf.Sin(progreso * Mathf.PI) * altura;
            
            // Rebotamos usando la X base
            transform.position = new Vector3(posicionAparicion.x, posicionAparicion.y + y, 0f);
            yield return null;
        }
    }

    void Update()
    {
        tiempoCoseno += Time.deltaTime * velocidadCoseno;
        float ondaCoseno = (1f - Mathf.Cos(tiempoCoseno)) * 0.5f;

        // Escala dinámica y proporcional al tamaño que le hayas puesto en el Editor
        transform.localScale = Vector3.Lerp(escalaInicial, escalaInicial * tamañoAgrandado, ondaCoseno);

        if (miRenderer != null)
        {
            float brillo = Mathf.Lerp(0.92f, 1.15f, ondaCoseno);
            miRenderer.color = new Color(1f, Mathf.Clamp01(brillo), Mathf.Clamp01(0.75f + ondaCoseno * 0.25f), 1f);
        }

        if (recogido || !puedeRecogerse || Time.timeScale == 0f) return;
        if (ControladorJuego.instancia != null && (ControladorJuego.instancia.esGameOver || ControladorJuego.instancia.enTransicion)) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 posRaton = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D[] tocados = Physics2D.OverlapPointAll(posRaton);

            foreach (Collider2D col in tocados)
            {
                if (col.gameObject == this.gameObject)
                {
                    RecogerTrofeo();
                    break;
                }
            }
        }
    }

    private void RecogerTrofeo()
    {
        recogido = true;
        CodigoProfe.BloquearDisparoHastaSoltar();

        if (ControladorJuego.instancia != null)
        {
            ControladorJuego.instancia.IniciarSecuenciaVictoria(sonidoVictoria);
        }
    }
}