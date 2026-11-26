using UnityEngine;
using System.Collections.Generic;

public class CodigoLapiz : MonoBehaviour
{
    [Header("Estadísticas del Lápiz")]
    public float velocidad = 10f;
    [HideInInspector] public float daño = 1f;
    private float tiempoDeVida = 3f;

    [Header("Efecto Hielo")]
    public bool esHielo = false;
    private Color colorImpactoHielo = new Color(0.65f, 0.88f, 1f, 1f);
    public GameObject prefabParticulasZonaHielo;
    [HideInInspector] public float radioZonaHielo = 0f;

    [Header("Efecto Boomerang (Avión)")]
    public bool esBoomerang = false;
    [HideInInspector] public int maxImpactosPorTramo = 3;
    private float alcanceCurvaBoomerang = 4f;
    private float anchuraCurvaBoomerang = 3f;
    private float radioBusquedaCercano = 2f;
    private float radioBusquedaAlVolver = 2f;

    [Header("Efecto Basket (Cañón)")]
    public bool esBasket = false;
    [HideInInspector] public float radioZonaBasket = 1.5f;
    [HideInInspector] public float danoAreaBasket = 1.5f;
    [HideInInspector] public float duracionAturdimientoBasket = 0.5f;
    public GameObject prefabParticulasZonaBasket;

    [Header("Efectos y Sonidos")]
    public GameObject prefabImpacto;
    public AudioClip[] golpes;
    public AudioClip[] disparos;

    private Transform objetivo;
    private codigoExamen scriptObjetivo;
    private bool haImpactado = false;

    // Variables internas Boomerang
    private enum FaseBoomerang { Ida, CurvaVuelta, Vuelta }
    private FaseBoomerang faseBoomerang = FaseBoomerang.Ida;
    private Transform alumnoOrigen;
    private Vector3 posicionOrigenGuardada;
    private Vector3 posicionUltimoObjetivo;
    private float anguloActualVuelo;
    private Vector3 p0Curva;
    private Vector3 p1Curva;
    private Vector3 p2Curva;
    private Vector2 direccionAlImpactar;
    private float progresoCurva = 0f;
    private float duracionCurva = 1f;
    private bool enTramoVuelta = false;
    private codigoExamen objetivoBoomerangActual;
    private codigoExamen ultimoExamenIda;
    private float tiempoUltimoImpactoIda = -10f;
    private float velocidadActualBoomerang = 0f;
    private float tiempoEnVuelta = 0f;
    private float tiempoPersiguiendoActual = 0f;
    private HashSet<codigoExamen> impactadosIda = new HashSet<codigoExamen>();
    private HashSet<codigoExamen> impactadosVuelta = new HashSet<codigoExamen>();

    // Variables internas Basket
    private Vector2 direccionVuelo;

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

        if (esBoomerang)
        {
            posicionOrigenGuardada = (alumnoOrigen != null) ? alumnoOrigen.position : transform.position;
            velocidadActualBoomerang = velocidad;
            objetivoBoomerangActual = scriptObjetivo;

            if (objetivo != null)
            {
                posicionUltimoObjetivo = objetivo.position;
                Vector2 dir = (Vector2)(objetivo.position - transform.position);
                anguloActualVuelo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }
            else
            {
                anguloActualVuelo = transform.eulerAngles.z;
                posicionUltimoObjetivo = transform.position + transform.right * 3f;
            }

            transform.rotation = Quaternion.Euler(0, 0, anguloActualVuelo - 90f);
            Destroy(gameObject, 8f);
        }
        else if (esBasket)
        {
            if (objetivo != null)
            {
                direccionVuelo = ((Vector2)(objetivo.position - transform.position)).normalized;
            }
            else
            {
                direccionVuelo = transform.right.normalized;
            }

            Destroy(gameObject, tiempoDeVida);
        }
        else
        {
            transform.Rotate(0, 0, -90f);
            Destroy(gameObject, tiempoDeVida);
        }
    }

    void Update()
    {
        if (esBoomerang)
        {
            ActualizarBoomerang();
            return;
        }

        if (esBasket)
        {
            ActualizarBasket();
            return;
        }

        // Lápiz normal y Boli congelado
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

    private void ActualizarBasket()
    {
        if (objetivo != null && scriptObjetivo != null)
        {
            if (scriptObjetivo.vida > 0)
            {
                Vector2 haciaObjetivo = (Vector2)(objetivo.position - transform.position);
                if (haciaObjetivo.sqrMagnitude > 0.0001f)
                {
                    direccionVuelo = haciaObjetivo.normalized;
                }
            }
            else
            {
                objetivo = null;
            }
        }

        transform.position += (Vector3)(direccionVuelo * velocidad * Time.deltaTime);
        transform.Rotate(0f, 0f, 450f * Time.deltaTime, Space.Self);
    }

    private void ActualizarBoomerang()
    {
        if (alumnoOrigen != null)
        {
            posicionOrigenGuardada = alumnoOrigen.position;
        }

        if (faseBoomerang == FaseBoomerang.Ida)
        {
            if (impactadosIda.Count >= maxImpactosPorTramo)
            {
                IniciarCurvaBoomerang();
                return;
            }

            bool objetivoValido = (objetivoBoomerangActual != null &&
                                   objetivoBoomerangActual.vida > 0 &&
                                   !impactadosIda.Contains(objetivoBoomerangActual));

            if (!objetivoValido)
            {
                if (impactadosIda.Count > 0)
                {
                    objetivoBoomerangActual = BuscarExamenCercano(impactadosIda, radioBusquedaCercano);
                    tiempoPersiguiendoActual = 0f;

                    if (objetivoBoomerangActual == null)
                    {
                        IniciarCurvaBoomerang();
                        return;
                    }
                    objetivoValido = true;
                }
                else
                {
                    // Si el primer objetivo murió antes de llegar, intenta redirigirse a uno cercano o gira al llegar
                    codigoExamen reemplazo = BuscarExamenCercano(impactadosIda, radioBusquedaCercano);
                    if (reemplazo != null)
                    {
                        objetivoBoomerangActual = reemplazo;
                        tiempoPersiguiendoActual = 0f;
                        objetivoValido = true;
                    }
                }
            }

            if (objetivoValido)
            {
                posicionUltimoObjetivo = objetivoBoomerangActual.transform.position;
                tiempoPersiguiendoActual += Time.deltaTime;

                if (impactadosIda.Count > 0 && tiempoPersiguiendoActual > 1.1f)
                {
                    IniciarCurvaBoomerang();
                    return;
                }

                MoverAvionHacia(posicionUltimoObjetivo, true);
            }
            else
            {
                Vector2 dir = (Vector2)(posicionUltimoObjetivo - transform.position);
                float distanciaIda = dir.magnitude;

                MoverAvionHacia(posicionUltimoObjetivo, false);

                float rad = anguloActualVuelo * Mathf.Deg2Rad;
                Vector2 dirAvance = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                bool cercaDelPunto = distanciaIda <= Mathf.Max(0.45f, velocidadActualBoomerang * Time.deltaTime * 1.5f);
                bool puntoRebasado = (distanciaIda < 1.5f && Vector2.Dot(dir, dirAvance) <= 0f);

                if (cercaDelPunto || puntoRebasado)
                {
                    IniciarCurvaBoomerang();
                }
            }
        }
        else if (faseBoomerang == FaseBoomerang.CurvaVuelta)
        {
            progresoCurva += Time.deltaTime / duracionCurva;

            if (progresoCurva >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            float t = progresoCurva;
            float u = 1f - t;
            Vector3 p3 = posicionOrigenGuardada;

            Vector3 nuevaPos =
                (u * u * u) * p0Curva +
                (3f * u * u * t) * p1Curva +
                (3f * u * t * t) * p2Curva +
                (t * t * t) * p3;

            Vector3 derivada =
                (3f * u * u) * (p1Curva - p0Curva) +
                (6f * u * t) * (p2Curva - p1Curva) +
                (3f * t * t) * (p3 - p2Curva);

            transform.position = nuevaPos;

            if (derivada.sqrMagnitude > 0.0001f)
            {
                anguloActualVuelo = Mathf.Atan2(derivada.y, derivada.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, anguloActualVuelo - 90f);

                if (!enTramoVuelta && Vector2.Dot(((Vector2)derivada).normalized, direccionAlImpactar) <= -0.2f)
                {
                    enTramoVuelta = true;
                    velocidadActualBoomerang = Mathf.Clamp(derivada.magnitude / duracionCurva, velocidad * 0.35f, velocidad);
                    tiempoEnVuelta = 0f;
                    tiempoPersiguiendoActual = 0f;

                    objetivoBoomerangActual = BuscarExamenCercano(impactadosVuelta, radioBusquedaAlVolver);
                    if (objetivoBoomerangActual == null && ultimoExamenIda != null && ultimoExamenIda.vida > 0)
                    {
                        objetivoBoomerangActual = ultimoExamenIda;
                    }

                    if (objetivoBoomerangActual != null)
                    {
                        faseBoomerang = FaseBoomerang.Vuelta;
                    }
                }
                else if (enTramoVuelta && impactadosVuelta.Count < maxImpactosPorTramo)
                {
                    // Si mientras termina la curva de regreso aparece un examen cerca, engancha hacia él
                    codigoExamen cercano = BuscarExamenCercano(impactadosVuelta, radioBusquedaCercano);
                    if (cercano != null)
                    {
                        velocidadActualBoomerang = Mathf.Clamp(derivada.magnitude / duracionCurva, velocidad * 0.35f, velocidad);
                        objetivoBoomerangActual = cercano;
                        tiempoEnVuelta = 0f;
                        tiempoPersiguiendoActual = 0f;
                        faseBoomerang = FaseBoomerang.Vuelta;
                    }
                }
            }
        }
        else if (faseBoomerang == FaseBoomerang.Vuelta)
        {
            tiempoEnVuelta += Time.deltaTime;

            bool puedeCazar = (impactadosVuelta.Count < maxImpactosPorTramo) && (tiempoEnVuelta < 3.5f);

            if (puedeCazar)
            {
                bool objetivoValido = (objetivoBoomerangActual != null &&
                                       objetivoBoomerangActual.vida > 0 &&
                                       !impactadosVuelta.Contains(objetivoBoomerangActual));

                if (!objetivoValido)
                {
                    objetivoBoomerangActual = BuscarExamenCercano(impactadosVuelta, radioBusquedaCercano);
                    tiempoPersiguiendoActual = 0f;
                }
                else
                {
                    tiempoPersiguiendoActual += Time.deltaTime;
                    if (tiempoPersiguiendoActual > 1.1f)
                    {
                        objetivoBoomerangActual = null;
                        tiempoEnVuelta = 3.5f;
                    }
                }
            }
            else
            {
                objetivoBoomerangActual = null;
            }

            if (objetivoBoomerangActual != null)
            {
                MoverAvionHacia(objetivoBoomerangActual.transform.position, true);
            }
            else
            {
                float distanciaAlumno = Vector2.Distance(transform.position, posicionOrigenGuardada);
                float radioCapturaAlumno = Mathf.Max(0.45f, velocidadActualBoomerang * Time.deltaTime * 1.5f);

                if (distanciaAlumno <= radioCapturaAlumno)
                {
                    Destroy(gameObject);
                    return;
                }

                MoverAvionHacia(posicionOrigenGuardada, false);
            }
        }
    }

    private void MoverAvionHacia(Vector3 destino, bool esObjetivoEnemigo)
    {
        Vector2 dir = (Vector2)(destino - transform.position);
        float distancia = dir.magnitude;
        float velocidadObjetivo = velocidad;

        if (distancia > 0.001f)
        {
            float anguloDeseado = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float diferenciaAngulo = Mathf.Abs(Mathf.DeltaAngle(anguloActualVuelo, anguloDeseado));

            // Desacelera levemente al cerrar curva hacia un examen cercano para mantener fluidez
            if (esObjetivoEnemigo)
            {
                velocidadObjetivo = Mathf.Lerp(velocidad, velocidad * 0.7f, Mathf.Clamp01(diferenciaAngulo / 135f));
            }

            velocidadActualBoomerang = Mathf.MoveTowards(velocidadActualBoomerang, velocidadObjetivo, velocidad * 2.4f * Time.deltaTime);

            float giroBase = Mathf.Max(520f, velocidadActualBoomerang * 58f);
            float giroAntiOrbita = (velocidadActualBoomerang / Mathf.Max(0.22f, distancia * 0.42f)) * Mathf.Rad2Deg;
            float tiempoRef = esObjetivoEnemigo ? tiempoPersiguiendoActual : tiempoEnVuelta;
            float refuerzoTiempo = 1f + Mathf.Clamp01(tiempoRef / 0.7f) * 1.5f;

            float velocidadGiro = Mathf.Clamp(Mathf.Max(giroBase, giroAntiOrbita) * refuerzoTiempo, 520f, 2400f);

            anguloActualVuelo = Mathf.MoveTowardsAngle(anguloActualVuelo, anguloDeseado, velocidadGiro * Time.deltaTime);
        }
        else
        {
            velocidadActualBoomerang = Mathf.MoveTowards(velocidadActualBoomerang, velocidad, velocidad * 2.4f * Time.deltaTime);
        }

        float rad = anguloActualVuelo * Mathf.Deg2Rad;
        Vector3 avance = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * (velocidadActualBoomerang * Time.deltaTime);
        transform.position += avance;
        transform.rotation = Quaternion.Euler(0f, 0f, anguloActualVuelo - 90f);
    }

    private codigoExamen BuscarExamenCercano(HashSet<codigoExamen> excluidos, float radio)
    {
        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, radio);
        codigoExamen mejor = null;
        float menorDistSq = float.MaxValue;

        foreach (Collider2D col in cercanos)
        {
            if (!col.CompareTag("Examen")) continue;

            codigoExamen ex = col.GetComponent<codigoExamen>();
            if (ex == null || ex.vida <= 0 || excluidos.Contains(ex)) continue;

            if (enTramoVuelta && ex == ultimoExamenIda && (Time.time - tiempoUltimoImpactoIda) < 0.12f)
            {
                continue;
            }

            float distSq = ((Vector2)(ex.transform.position - transform.position)).sqrMagnitude;
            if (distSq < menorDistSq)
            {
                menorDistSq = distSq;
                mejor = ex;
            }
        }

        return mejor;
    }

    private void IniciarCurvaBoomerang()
    {
        if (faseBoomerang != FaseBoomerang.Ida) return;

        p0Curva = transform.position;
        objetivoBoomerangActual = null;

        float rad = anguloActualVuelo * Mathf.Deg2Rad;
        direccionAlImpactar = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        Vector2 perpendicular = new Vector2(-direccionAlImpactar.y, direccionAlImpactar.x);

        float avance = Mathf.Max(0.5f, alcanceCurvaBoomerang);

        p1Curva = p0Curva + (Vector3)(direccionAlImpactar * avance);
        p2Curva = p0Curva + (Vector3)(direccionAlImpactar * (avance * 0.78f) + perpendicular * anchuraCurvaBoomerang);

        float velEntrada = Mathf.Max(velocidad * 0.5f, velocidadActualBoomerang);
        duracionCurva = (3f * avance) / Mathf.Max(0.1f, velEntrada);
        progresoCurva = 0f;
        enTramoVuelta = false;
        faseBoomerang = FaseBoomerang.CurvaVuelta;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (esBoomerang)
        {
            ProcesarColisionBoomerang(otro);
            return;
        }

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

                enemigo.RecibirImpacto(daño);

                if (esBasket)
                {
                    if (duracionAturdimientoBasket > 0f && enemigo.vida > 0)
                    {
                        enemigo.AplicarAturdimiento(duracionAturdimientoBasket);
                    }

                    if (radioZonaBasket > 0f)
                    {
                        AplicarZonaBasket(enemigo);
                    }
                }

                InstanciarEfectoImpacto(transform.position);
                ReproducirSonidoGolpe();

                Destroy(gameObject);
            }
        }
    }

    void OnTriggerStay2D(Collider2D otro)
    {
        if (esBoomerang)
        {
            ProcesarColisionBoomerang(otro);
        }
    }

    private void ProcesarColisionBoomerang(Collider2D otro)
    {
        if (!otro.CompareTag("Examen")) return;

        codigoExamen enemigo = otro.GetComponent<codigoExamen>();
        if (enemigo == null || enemigo.vida <= 0) return;

        HashSet<codigoExamen> listaTramo = enTramoVuelta ? impactadosVuelta : impactadosIda;
        if (listaTramo.Contains(enemigo)) return;

        if (enTramoVuelta && enemigo == ultimoExamenIda && (Time.time - tiempoUltimoImpactoIda) < 0.12f)
        {
            return;
        }

        if (listaTramo.Count >= maxImpactosPorTramo) return;

        listaTramo.Add(enemigo);

        if (!enTramoVuelta)
        {
            ultimoExamenIda = enemigo;
            tiempoUltimoImpactoIda = Time.time;
        }

        enemigo.RecibirImpacto(daño);

        InstanciarEfectoImpacto(Vector3.Lerp(transform.position, enemigo.transform.position, 0.5f));
        ReproducirSonidoGolpe();

        if (faseBoomerang == FaseBoomerang.Ida)
        {
            if (impactadosIda.Count >= maxImpactosPorTramo)
            {
                IniciarCurvaBoomerang();
            }
            else if (objetivoBoomerangActual == enemigo || objetivoBoomerangActual == null ||
                     objetivoBoomerangActual.vida <= 0 || impactadosIda.Contains(objetivoBoomerangActual))
            {
                objetivoBoomerangActual = BuscarExamenCercano(impactadosIda, radioBusquedaCercano);
                tiempoPersiguiendoActual = 0f;

                if (objetivoBoomerangActual == null)
                {
                    IniciarCurvaBoomerang();
                }
            }
        }
        else if (faseBoomerang == FaseBoomerang.Vuelta)
        {
            if (impactadosVuelta.Count >= maxImpactosPorTramo)
            {
                objetivoBoomerangActual = null;
            }
            else if (objetivoBoomerangActual == enemigo || objetivoBoomerangActual == null ||
                     objetivoBoomerangActual.vida <= 0 || impactadosVuelta.Contains(objetivoBoomerangActual))
            {
                objetivoBoomerangActual = BuscarExamenCercano(impactadosVuelta, radioBusquedaCercano);
                tiempoPersiguiendoActual = 0f;
            }
        }
    }

    private void AplicarZonaHielo(codigoExamen enemigoImpactado)
    {
        InstanciarParticulasZona(prefabParticulasZonaHielo);

        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, radioZonaHielo);
        foreach (Collider2D col in cercanos)
        {
            if (!col.CompareTag("Examen")) continue;

            codigoExamen otroEnemigo = col.GetComponent<codigoExamen>();
            if (otroEnemigo != null && otroEnemigo != enemigoImpactado && otroEnemigo.vida > 0)
            {
                otroEnemigo.AplicarCongelacion();
            }
        }
    }

    private void AplicarZonaBasket(codigoExamen enemigoImpactado)
    {
        InstanciarParticulasZona(prefabParticulasZonaBasket);

        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, radioZonaBasket);
        foreach (Collider2D col in cercanos)
        {
            if (!col.CompareTag("Examen")) continue;

            codigoExamen otroEnemigo = col.GetComponent<codigoExamen>();
            if (otroEnemigo != null && otroEnemigo != enemigoImpactado && otroEnemigo.vida > 0)
            {
                otroEnemigo.RecibirImpacto(danoAreaBasket);
            }
        }
    }

    private void InstanciarParticulasZona(GameObject prefabParticulas)
    {
        if (prefabParticulas == null) return;

        GameObject efectoZona = Instantiate(prefabParticulas, transform.position, Quaternion.identity);
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

    private void InstanciarEfectoImpacto(Vector3 posicion)
    {
        if (prefabImpacto == null) return;

        GameObject impacto = Instantiate(prefabImpacto, posicion, Quaternion.identity);
        if (esHielo)
        {
            GolpeImpacto scriptImpacto = impacto.GetComponent<GolpeImpacto>();
            if (scriptImpacto != null)
            {
                scriptImpacto.AplicarTinte(colorImpactoHielo);
            }
        }
    }

    private void ReproducirSonidoGolpe()
    {
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
    }

    public void AsignarOrigen(Transform origen)
    {
        alumnoOrigen = origen;
        if (origen != null)
        {
            posicionOrigenGuardada = origen.position;
        }
    }

    public void AsignarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;

        if (objetivo != null)
        {
            scriptObjetivo = objetivo.GetComponent<codigoExamen>();
            posicionUltimoObjetivo = objetivo.position;
        }
    }
}