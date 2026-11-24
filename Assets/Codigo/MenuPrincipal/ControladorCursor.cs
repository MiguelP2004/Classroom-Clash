using UnityEngine;

public class ControladorCursor : MonoBehaviour
{
    public static ControladorCursor instancia;
    public Texture2D cursorMano;
    private int tamanoCursor = 95;
    private Vector2 puntoClickRelativo = new Vector2(0.75f, 0.1f);
    private Texture2D texturaFinal;

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        AplicarCursor();
    }

    void OnApplicationFocus(bool tieneFoco)
    {
        if (tieneFoco)
        {
            AplicarCursor();
        }
    }

    void OnDestroy()
    {
        if (texturaFinal != null)
        {
            Destroy(texturaFinal);
        }
    }

    private void AplicarCursor()
    {
        if (cursorMano == null) return;

        if (texturaFinal == null)
        {
            texturaFinal = RedimensionarTextura(cursorMano, Mathf.Max(16, tamanoCursor));
        }

        Vector2 hotspot = new Vector2(
            texturaFinal.width * Mathf.Clamp01(puntoClickRelativo.x),
            texturaFinal.height * Mathf.Clamp01(puntoClickRelativo.y)
        );

        Cursor.visible = true;
        Cursor.SetCursor(texturaFinal, hotspot, CursorMode.ForceSoftware);
    }

    private Texture2D RedimensionarTextura(Texture2D origen, int tamanoMax)
    {
        float escala = (float)tamanoMax / Mathf.Max(origen.width, origen.height);
        int nuevoAncho = Mathf.Max(1, Mathf.RoundToInt(origen.width * escala));
        int nuevoAlto = Mathf.Max(1, Mathf.RoundToInt(origen.height * escala));

        Texture2D resultado = new Texture2D(nuevoAncho, nuevoAlto, TextureFormat.RGBA32, false, true);
        resultado.wrapMode = TextureWrapMode.Clamp;
        resultado.filterMode = FilterMode.Bilinear;

        Color[] pixeles = new Color[nuevoAncho * nuevoAlto];

        for (int y = 0; y < nuevoAlto; y++)
        {
            float v = (y + 0.5f) / nuevoAlto;
            for (int x = 0; x < nuevoAncho; x++)
            {
                float u = (x + 0.5f) / nuevoAncho;
                pixeles[y * nuevoAncho + x] = origen.GetPixelBilinear(u, v);
            }
        }

        resultado.SetPixels(pixeles);
        resultado.Apply();

        return resultado;
    }
}