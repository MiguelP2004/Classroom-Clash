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
    
    private Vector3 posicionAparicion;
    
    public static Vector3 posicionUltimaBaja; 

    [Header("Flote y Brillo (Coseno)")]
    private float velocidadCoseno = 3.2f;
    public float tamañoAgrandado = 1.15f; 

    private SpriteRenderer miRenderer;
    private bool puedeRecogerse = false;
    private bool recogido = false;
    private float tiempoCoseno = 0f;
    
    private Vector3 escalaInicial; 

    void Awake()
    {
        miRenderer = GetComponent<SpriteRenderer>();

        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;
        }

        escalaInicial = transform.localScale; 
    }

    void OnEnable()
    {
        recogido = false;
        puedeRecogerse = false;
        tiempoCoseno = 0f;

        if (MenuPrincipal.nivelElegido <= 1)
        {
            posicionAparicion = posicionUltimaBaja;
            posicionAparicion.z = 1f;
        }
        else
        {
            posicionAparicion = posicionUltimaBaja;
            posicionAparicion.z = 1f;
        }

        transform.position = posicionAparicion;
        transform.localScale = escalaInicial;

        StartCoroutine(RutinaSaltoYRebote());
    }

    private IEnumerator RutinaSaltoYRebote()
    {
        yield return StartCoroutine(ArcoRebote(alturaPrimerSalto, duracionPrimerSalto));
        yield return StartCoroutine(ArcoRebote(alturaPrimerSalto * 0.35f, duracionPrimerSalto * 0.6f));
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
            
            transform.position = new Vector3(posicionAparicion.x, posicionAparicion.y + y, 0f);
            yield return null;
        }
    }

    void Update()
    {
        if (recogido || !puedeRecogerse || Time.timeScale == 0f) return;
        if (ControladorJuego.instancia != null && (ControladorJuego.instancia.esGameOver || ControladorJuego.instancia.enTransicion)) return;

        tiempoCoseno += Time.deltaTime * velocidadCoseno;
        float ondaCoseno = (1f - Mathf.Cos(tiempoCoseno)) * 0.5f;

        transform.localScale = Vector3.Lerp(escalaInicial, escalaInicial * tamañoAgrandado, ondaCoseno);

        if (miRenderer != null)
        {
            float brillo = Mathf.Lerp(0.92f, 1.15f, ondaCoseno);
            miRenderer.color = new Color(1f, Mathf.Clamp01(brillo), Mathf.Clamp01(0.75f + ondaCoseno * 0.25f), 1f);
        }

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
        
        // ¡NUEVO! Al hacer click, el trofeo se mueve suavemente hacia (-3, 0) siempre
        StartCoroutine(MoverHaciaCentro(3f));
    }

    private IEnumerator MoverHaciaCentro(float duracion)
    {
        float tiempo = 0f;
        Vector3 posInicial = transform.position;
        Vector3 posDestino = new Vector3(-3f, 0f, 0f);

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracion);
            transform.position = Vector3.Lerp(posInicial, posDestino, t);
            yield return null;
        }

        transform.position = posDestino;
    }
}