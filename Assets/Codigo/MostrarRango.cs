using UnityEngine;

public class MostrarRango : MonoBehaviour
{
    [Header("Configuración del Aura")]
    public Color colorAura = new Color(1f, 0.3f, 0.1f, 1f); 
    public float grosor = 0.025f;

    private LineRenderer[] lineas = new LineRenderer[3];
    private CodigoAlumno scriptAlumno; 
    private int segmentos = 50;

    void Awake()
    {
        scriptAlumno = GetComponent<CodigoAlumno>();

        LineRenderer viejo = GetComponent<LineRenderer>();
        if (viejo != null) Destroy(viejo);

        for (int i = 0; i < 3; i++)
        {
            GameObject hijoLinea = new GameObject("AuraLinea_" + i);
            hijoLinea.transform.SetParent(this.transform);
            hijoLinea.transform.localPosition = Vector3.zero;

            LineRenderer lr = hijoLinea.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = grosor;
            lr.endWidth = grosor;
            lr.sortingOrder = 20000;
            lr.positionCount = segmentos;
            
            lineas[i] = lr;
        }
    }

    void Update()
    {
        if (scriptAlumno == null) return;

        bool debeMostrar = (!scriptAlumno.estaColocado) || (scriptAlumno.seleccionado);

        for (int i = 0; i < 3; i++)
        {
            if (lineas[i] != null)
            {
                lineas[i].enabled = debeMostrar; // Encendemos o apagamos el dibujo
            }
        }

        if (!debeMostrar) return;

        float radioBase = scriptAlumno.rangoAtaque; 
        float tiempo = Time.unscaledTime;

        for (int i = 0; i < 3; i++)
        {
            if (lineas[i] == null) continue;

            float desfase = i * 2f;
            float variacionRadio = Mathf.Sin(tiempo * (1.5f + i * 0.5f) + desfase) * 0.08f;
            float radioActual = radioBase + variacionRadio;

            float alfa = Mathf.Lerp(0.1f, 0.6f, (Mathf.Sin(tiempo * 3f + desfase) + 1f) / 2f);
            Color colorActual = colorAura;
            colorActual.a = alfa;
            
            lineas[i].startColor = colorActual;
            lineas[i].endColor = colorActual;

            DibujarCirculo(lineas[i], radioActual);
        }
    }

    void DibujarCirculo(LineRenderer lr, float radio)
    {
        float angulo = 0f;
        for (int j = 0; j < segmentos; j++)
        {
            float x = transform.position.x + (Mathf.Sin(Mathf.Deg2Rad * angulo) * radio);
            float y = transform.position.y + (Mathf.Cos(Mathf.Deg2Rad * angulo) * radio);

            lr.SetPosition(j, new Vector3(x, y, 0f));
            angulo += (360f / segmentos);
        }
    }
}