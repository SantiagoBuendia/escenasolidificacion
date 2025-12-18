using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// HUD en World Space que se sitúa frente a la cámara y muestra mensajes con fade.
/// Colocar este script en CanvasMensajeVR (Render Mode = World Space).
/// </summary>
[RequireComponent(typeof(Canvas))]
public class MensajeVRPro : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI textoMensaje;

    [Header("Ajustes")]
    public float distancia = 1.5f;
    public float duracion = 3f;
    public float velocidadFade = 3f;

    Transform camara;
    Coroutine rutina;

    void Start()
    {
        if (Camera.main != null) camara = Camera.main.transform;
        SetAlpha(0f);
    }

    void LateUpdate()
    {
        if (camara == null)
        {
            if (Camera.main != null) camara = Camera.main.transform;
            else return;
        }

        Vector3 objetivo = camara.position + camara.forward * distancia;
        transform.position = objetivo;
        transform.LookAt(camara);
        transform.Rotate(0f, 180f, 0f, Space.Self);
    }

    /// <summary>
    /// Muestra un mensaje. Si durationOverride &gt; 0, lo usa; si es negativo usa duracion por defecto.
    /// Si durationOverride es muy grande (ej. 999) se mantiene hasta que se llame OcultarAhora().
    /// </summary>
    public void MostrarMensaje(string mensaje, float durationOverride = -1f)
    {
        if (textoMensaje == null)
        {
            Debug.LogWarning("MensajeVRPro: asigne textoMensaje (TextMeshProUGUI).");
            return;
        }

        if (rutina != null) StopCoroutine(rutina);
        rutina = StartCoroutine(MostrarCoroutine(mensaje, durationOverride));
    }

    IEnumerator MostrarCoroutine(string mensaje, float durationOverride)
    {
        textoMensaje.text = mensaje;

        // Fade in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadFade;
            SetAlpha(Mathf.Clamp01(t));
            yield return null;
        }

        // esperar (si durationOverride < 0 usa duracion)
        float wait = durationOverride > 0 ? durationOverride : duracion;
        if (durationOverride < 0)
            yield return new WaitForSeconds(wait);
        else if (durationOverride > 0 && durationOverride < 999f)
            yield return new WaitForSeconds(wait);
        else if (durationOverride >= 999f)
        {
            // mantener visible hasta OcultarAhora se llame
            yield break;
        }

        // Fade out
        while (t > 0f)
        {
            t -= Time.deltaTime * velocidadFade;
            SetAlpha(Mathf.Clamp01(t));
            yield return null;
        }

        SetAlpha(0f);
        rutina = null;
    }

    void SetAlpha(float a)
    {
        if (textoMensaje != null)
        {
            Color c = textoMensaje.color;
            c.a = a;
            textoMensaje.color = c;
        }

        var cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = a;
    }

    /// <summary>
    /// Oculta el mensaje inmediatamente (detiene coroutines).
    /// </summary>
    public void OcultarAhora()
    {
        if (rutina != null) StopCoroutine(rutina);
        SetAlpha(0f);
        rutina = null;
    }
}
