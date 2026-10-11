using UnityEngine;
using UnityEngine.InputSystem;

public class CodigoProfe : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current == null || Camera.main == null) return;

        Vector3 posicionRaton = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direccion = posicionRaton - transform.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angulo);
    }
}
