using UnityEngine;

public class CodigoLapiz : MonoBehaviour
{
    [Header("Estadísticas del Lápiz")]
    public float velocidad = 10f;
    public float daño = 1f; 
    private float tiempoDeVida = 3f;
    
    [Header("Efecto Hielo")]
    public bool esHielo = false;
    private Color colorImpactoHielo = new Color(0.65f, 0.88f, 1f, 1f);
    public GameObject prefabParticulasZonaHielo;
    [HideInInspector] public float radioZonaHielo = 0f;

    public GameObject prefabImpacto;
    
    private Transform objetivo;
    private codigoExamen scriptObjetivo;
    private bool haImpactado = false;

    public AudioClip[] golpes;
    public AudioClip[] disparos;

    void Start()
    {
        if (MenuPrincipal.instancia == null && disparos.Length > 0)
        {
            AudioClip disparo = disparos[Random.Range(0, disparos.Length)];

            GameObject audioTemporal = new GameObject("Sonido_Disparo");
            AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();
            fuenteAudio.clip = disparo;
            fuenteAudio.spatialBlend = 0f;
            fuenteAudio.volume = ControladorJuego.volumenSFXGuardado;

            fuenteAudio.Play();
            Destroy(audioTemporal, disparo.length + 0.1f);
        }
        transform.Rotate(0, 0, -90f);
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        if (objetivo != null && scriptObjetivo != null)
        {
            if (scriptObjetivo.vida > 0)
            {
                Vector3 direccion = objetivo.position - transform.position;
                float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
                
                transform.rotation = Quaternion.Euler(0, 0, angulo - 90f);
            }
            else
            {
                objetivo = null;
            }
        }
        
        transform.Translate(Vector3.up * velocidad * Time.deltaTime, Space.Self);
    }
    
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (haImpactado) return;
        if (otro.CompareTag("Examen"))
        {
            codigoExamen enemigo = otro.GetComponent<codigoExamen>();
            
            if (enemigo != null && enemigo.vida > 0)
            {
                haImpactado = true;

                if (esHielo)
                {
                    enemigo.AplicarCongelacion();

                    if (radioZonaHielo > 0f)
                    {
                        AplicarZonaHielo(enemigo);
                    }
                }

                // Solo hace daño al enemigo impactado directamente
                enemigo.RecibirImpacto(daño); 
                
                if (prefabImpacto != null)
                {
                    GameObject impacto = Instantiate(prefabImpacto, transform.position, Quaternion.identity);
                    if (esHielo)
                    {
                        GolpeImpacto scriptImpacto = impacto.GetComponent<GolpeImpacto>();
                        if (scriptImpacto != null)
                        {
                            scriptImpacto.AplicarTinte(colorImpactoHielo);
                        }
                    }
                }

                if (MenuPrincipal.instancia == null && golpes.Length > 0)
                {
                    AudioClip golpe = golpes[Random.Range(0, golpes.Length)];

                    GameObject audioTemporal = new GameObject("Sonido_Golpe");
                    AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();
                    fuenteAudio.clip = golpe;
                    fuenteAudio.spatialBlend = 0f;
                    fuenteAudio.volume = ControladorJuego.volumenSFXGuardado;

                    fuenteAudio.Play();
                    Destroy(audioTemporal, golpe.length + 0.1f);
                }

                Destroy(gameObject);
            }
        }
    }

    private void AplicarZonaHielo(codigoExamen enemigoImpactado)
    {
        if (prefabParticulasZonaHielo != null)
        {
            GameObject efectoZona = Instantiate(prefabParticulasZonaHielo, transform.position, Quaternion.identity);
            ParticleSystem ps = efectoZona.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                ps.Play();
                Destroy(efectoZona, ps.main.duration + ps.main.startLifetime.constantMax + 0.5f);
            }
            else
            {
                Destroy(efectoZona, 2f);
            }
        }

        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, radioZonaHielo);
        foreach (Collider2D col in cercanos)
        {
            if (!col.CompareTag("Examen")) continue;

            codigoExamen otroEnemigo = col.GetComponent<codigoExamen>();
            if (otroEnemigo != null && otroEnemigo != enemigoImpactado && otroEnemigo.vida > 0)
            {
                // Congela sin aplicar daño
                otroEnemigo.AplicarCongelacion();
            }
        }
    }

    public void AsignarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        
        if (objetivo != null)
        {
            scriptObjetivo = objetivo.GetComponent<codigoExamen>();
        }
    }
}