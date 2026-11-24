using UnityEngine;

public class SistemaPausa : MonoBehaviour
{
    [Header("Pausa")]
    public GameObject botonAjustes;
    public GameObject panelAjustes;
    public RectTransform panelBarrido;
    public AudioClip pausar;

    private float posicionY_Oculta = -1200f;
    private float posicionY_Visible = 0f;

    private bool juegoPausado = false;
    private bool animandoBarrido = false;
    private float velocidadBarrido = 0f;
    private bool barridoDentroDeAjustes = false;

    void Awake()
    {
        if (panelBarrido == null && panelAjustes != null)
        {
            panelBarrido = panelAjustes.GetComponent<RectTransform>();
        }

        if (panelAjustes != null && panelBarrido != null)
        {
            barridoDentroDeAjustes = (panelBarrido.gameObject == panelAjustes) || panelBarrido.transform.IsChildOf(panelAjustes.transform);
        }

        Time.timeScale = 1f;

        if (panelAjustes != null)
            panelAjustes.SetActive(false);

        if (panelBarrido != null)
        {
            Vector2 pos = panelBarrido.anchoredPosition;
            pos.y = posicionY_Oculta;
            panelBarrido.anchoredPosition = pos;
            panelBarrido.gameObject.SetActive(false);
        }

        if (botonAjustes != null)
            botonAjustes.SetActive(true);
    }

    void Update()
    {
        if (panelBarrido != null && animandoBarrido)
        {
            float objetivoY = juegoPausado ? posicionY_Visible : posicionY_Oculta;
            Vector2 posActual = panelBarrido.anchoredPosition;

            posActual.y = Mathf.SmoothDamp(posActual.y, objetivoY, ref velocidadBarrido, 0.1f, Mathf.Infinity, Time.unscaledDeltaTime);
            panelBarrido.anchoredPosition = posActual;

            if (Mathf.Abs(posActual.y - objetivoY) < 1f)
            {
                posActual.y = objetivoY;
                panelBarrido.anchoredPosition = posActual;

                if (!juegoPausado)
                {
                    panelBarrido.gameObject.SetActive(false);
                    if (barridoDentroDeAjustes && panelAjustes != null)
                    {
                        panelAjustes.SetActive(false);
                    }
                }
                animandoBarrido = false;
            }
        }
    }

    public void AlternarPausa()
    {
        if (MenuPrincipal.instancia != null && MenuPrincipal.instancia.enTransicion) return;

        if (juegoPausado)
        {
            ReanudarJuego();
        }
        else
        {
            if (pausar != null)
            {
                GameObject audioTemporal = new GameObject("Sonido_Pausa");
                AudioSource fuenteAudio = audioTemporal.AddComponent<AudioSource>();

                fuenteAudio.clip = pausar;
                fuenteAudio.spatialBlend = 0f;
                fuenteAudio.volume = ControladorJuego.volumenSFXGuardado;
                fuenteAudio.ignoreListenerPause = true;

                fuenteAudio.Play();
                Destroy(audioTemporal, pausar.length + 0.1f);
            }

            PausarJuego();
        }
    }

    private void PausarJuego()
    {
        juegoPausado = true;

        if (botonAjustes != null)
            botonAjustes.SetActive(false);

        if (panelAjustes != null)
            panelAjustes.SetActive(true);

        if (panelBarrido != null)
        {
            if (!panelBarrido.gameObject.activeSelf)
            {
                panelBarrido.gameObject.SetActive(true);
                Vector2 pos = panelBarrido.anchoredPosition;
                pos.y = posicionY_Oculta;
                panelBarrido.anchoredPosition = pos;
            }

            velocidadBarrido = 0f;
            animandoBarrido = true;
        }
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;

        if (!barridoDentroDeAjustes && panelAjustes != null)
            panelAjustes.SetActive(false);

        if (botonAjustes != null)
            botonAjustes.SetActive(true);

        if (panelBarrido != null)
        {
            velocidadBarrido = 0f;
            animandoBarrido = true;
        }
    }
}