
using UnityEngine;

public class GestorAudio : MonoBehaviour
{
    public static GestorAudio instancia;

    public static float volumenMusica = 1f;
    public static float volumenFX = 1f;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        // Cargar valores guardados
        volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 1f);
        volumenFX = PlayerPrefs.GetFloat("VolumenFX", 1f);
    }

    public void CambiarVolumenMusica(float valor)
    {
        volumenMusica = valor;
        PlayerPrefs.SetFloat("VolumenMusica", valor);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenFX(float valor)
    {
        volumenFX = valor;
        PlayerPrefs.SetFloat("VolumenFX", valor);
        PlayerPrefs.Save();
    }
}
