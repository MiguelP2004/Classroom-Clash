
using UnityEngine;
using UnityEngine.UI;

public class SliderVolumenAudio : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider != null)
        {
            slider.onValueChanged.AddListener(CambiarVolumen);
            CambiarVolumen(slider.value);
        }
    }

    private void CambiarVolumen(float valor)
    {
        if (ConfiguracionAudio.Instancia != null)
        {
            ConfiguracionAudio.Instancia.CambiarVolumenSFX(valor);
        }
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(CambiarVolumen);
    }
}
