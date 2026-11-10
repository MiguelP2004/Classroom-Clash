using UnityEngine;

public class CodigoLapiz : MonoBehaviour
{
    [Header("Estadísticas del Lápiz")]
    public float velocidad = 10f;
    public float daño = 1f; 
    private float tiempoDeVida = 3f;
    public GameObject prefabImpacto;
    
    private Transform objetivo;
    private codigoExamen scriptObjetivo;

    public AudioClip[] golpes;
    public AudioClip[] disparos;

    void Start()
    {
        // Solo reproduce sonido de disparo si NO estamos en el Menú Principal
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
        if (otro.CompareTag("Examen"))
        {
            codigoExamen enemigo = otro.GetComponent<codigoExamen>();
            
            if (enemigo != null && enemigo.vida > 0)
            {
                enemigo.RecibirImpacto(daño); 
                
                if (prefabImpacto != null)
                {
                    Instantiate(prefabImpacto, transform.position, Quaternion.identity);
                }

                // Solo reproduce sonido de golpe si NO estamos en el Menú Principal
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

    public void AsignarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        
        if (objetivo != null)
        {
            scriptObjetivo = objetivo.GetComponent<codigoExamen>();
        }
    }
}