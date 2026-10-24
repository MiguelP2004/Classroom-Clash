using UnityEngine;
using UnityEngine.EventSystems;

public class EfectoBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private float tamañoNormal = 1f;
    private float tamañoAgrandado = 1.1f;
    private bool ratonEncima = false;
    private bool estaSeleccionado = false;

    void Start()
    {
        transform.localScale = new Vector3(tamañoNormal, tamañoNormal, 0f);
    }

    void Update()
    {
        Vector3 objetivo = new Vector3(tamañoNormal, tamañoNormal, 0f);
        // Si el ratón está encima o el botón está seleccionado
        if (ratonEncima || estaSeleccionado)
        {
            objetivo = new Vector3(tamañoAgrandado, tamañoAgrandado, 0f);
        }
        transform.localScale += (objetivo - transform.localScale) / 30f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ratonEncima = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ratonEncima = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        estaSeleccionado = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        estaSeleccionado = false;
    }
}