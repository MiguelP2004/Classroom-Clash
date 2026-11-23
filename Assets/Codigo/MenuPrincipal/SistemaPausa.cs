
using UnityEngine;

public class SistemaPausa : MonoBehaviour
{
    [Header("Pausa")]
    public GameObject botonAjustes;
    public GameObject panelAjustes;

    private bool juegoPausado = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (panelAjustes != null)
            panelAjustes.SetActive(false);

        if (botonAjustes != null)
            botonAjustes.SetActive(true);
    }

    public void AlternarPausa()
    {
        if (juegoPausado)
            ReanudarJuego();
        else
            PausarJuego();
    }

    private void PausarJuego()
    {
        juegoPausado = true;

        if (botonAjustes != null)
            botonAjustes.SetActive(false);

        if (panelAjustes != null)
            panelAjustes.SetActive(true);
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;

        if (panelAjustes != null)
            panelAjustes.SetActive(false);

        if (botonAjustes != null)
            botonAjustes.SetActive(true);
    }
}
