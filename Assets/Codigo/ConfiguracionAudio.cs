
using UnityEngine;
using UnityEngine.Audio;

public class ConfiguracionAudio : MonoBehaviour
{
    public static ConfiguracionAudio Instancia { get; private set; }

    [Header("Mezclador de audio")]
    [SerializeField] private AudioMixer mezcladorAudio;

    [Header("Grupos del mezclador")]
    [SerializeField] private AudioMixerGroup grupoSFX;
    [SerializeField] private AudioMixerGroup grupoMusica;

    private const string PARAMETRO_SFX = "VolumenSFX";
    private const string PARAMETRO_MUSICA = "VolumenMusica";

    private const string GUARDADO_SFX = "VolumenSFXGuardado";
    private const string GUARDADO_MUSICA = "VolumenMusicaGuardado";

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayerPrefs.DeleteKey(GUARDADO_SFX);
        PlayerPrefs.DeleteKey(GUARDADO_MUSICA);

        CambiarVolumenSFX(PlayerPrefs.GetFloat(GUARDADO_SFX, 1f));
        CambiarVolumenMusica(PlayerPrefs.GetFloat(GUARDADO_MUSICA, 1f));
    }

    public void CambiarVolumenSFX(float volumen)
    {
        AplicarVolumen(PARAMETRO_SFX, GUARDADO_SFX, volumen);
    }

    public void CambiarVolumenMusica(float volumen)
    {
        AplicarVolumen(PARAMETRO_MUSICA, GUARDADO_MUSICA, volumen);
    }

    private void AplicarVolumen(
        string parametro, string claveGuardado, float volumen)
    {
        volumen = Mathf.Clamp01(volumen);

        float decibelios = volumen <= 0.0001f
            ? -80f
            : Mathf.Log10(volumen) * 20f;

        if (mezcladorAudio != null)
        {
            if (!mezcladorAudio.SetFloat(parametro, decibelios))
            {
                Debug.LogWarning(
                    "No se encuentra el parámetro: " + parametro);
            }
        }

        PlayerPrefs.SetFloat(claveGuardado, volumen);
        PlayerPrefs.Save();
    }

    public void ConfigurarFuenteSFX(AudioSource fuente)
    {
        if (fuente != null && grupoSFX != null)
        {
            fuente.outputAudioMixerGroup = grupoSFX;
        }
    }

    public void ConfigurarFuenteMusica(AudioSource fuente)
    {
        if (fuente != null && grupoMusica != null)
        {
            fuente.outputAudioMixerGroup = grupoMusica;
        }
    }
    public float ObtenerVolumenSFX()
    {
        return PlayerPrefs.GetFloat(GUARDADO_SFX, 0.75f);
    }

    public float ObtenerVolumenMusica()
    {
        return PlayerPrefs.GetFloat(GUARDADO_MUSICA, 0.75f);
    }
}
