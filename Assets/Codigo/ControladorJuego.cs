using UnityEngine;
using TMPro; // Necesario para modificar textos modernos en Unity

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego instancia; // Para acceder desde cualquier script fácilmente

    [Header("Energía")]
    public int energia = 50;
    public TextMeshProUGUI textoEnergia; 
    public float tamañoAgrandado = 1.1f; // Cuánto crece al cambiar de valor

    void Awake()
    {
        if (instancia == null) instancia = this;
        ActualizarTexto();
    }

    void Update()
    {
        if (textoEnergia != null)
        {
            textoEnergia.transform.localScale += (Vector3.one - textoEnergia.transform.localScale) / 40f;
        }
    }

    public void SumarEnergia(int cantidad)
    {
        energia += cantidad;
        EfectoCambioEnergia();
    }

    public void GastarEnergia(int cantidad)
    {
        energia -= cantidad;
        EfectoCambioEnergia();
    }

    public bool PuedeComprar(int coste)
    {
        return energia >= coste;
    }

    private void EfectoCambioEnergia()
    {
        if (textoEnergia != null)
        {
            textoEnergia.text = energia.ToString();
            textoEnergia.transform.localScale = new Vector3(tamañoAgrandado, tamañoAgrandado, 1f);
        }
    }

    private void ActualizarTexto()
    {
        if (textoEnergia != null) textoEnergia.text = energia.ToString();
    }
}