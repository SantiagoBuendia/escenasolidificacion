using UnityEngine;
using TMPro;
using System.Collections;

public class ControlSolidificacion : MonoBehaviour
{
    [Header("Referencias de escena")]
    public Transform hielo;
    public Transform agua;
    public Light luzEstufa;
    public TMP_Text textoUI;

    [Header("Ajustes")]
    public float velocidadBajadaTemp = 0.05f;

    public bool CongeladorEncendido = false;
    public bool tieneAgua = false;

    private float tiempoInicioSimulacion;
    private bool simulacionIniciadaBD = false;
    private bool simulacionFinalizadaBD = false;
    private bool peticionEnCurso = false;

    float temperaturaActual = 25f;

    void Start()
    {
        if (agua != null) agua.gameObject.SetActive(false);
        if (hielo != null) hielo.gameObject.SetActive(false);
        ActualizarTextoUI();
    }

    void Update()
    {
        if (tieneAgua && CongeladorEncendido && !simulacionFinalizadaBD)
        {
            temperaturaActual -= velocidadBajadaTemp * Time.deltaTime * 60f;
            temperaturaActual = Mathf.Clamp(temperaturaActual, -5f, 25f);

            ActualizarTextoUI();

            if (temperaturaActual <= 0f)
            {
                simulacionFinalizadaBD = true;

                float tiempoFinReal = Time.time;

                if (agua != null) agua.gameObject.SetActive(false);
                if (hielo != null) hielo.gameObject.SetActive(true);

                StartCoroutine(RegistrarEventoSeguro("Solidificacion completada", "El agua se ha transformado en hielo"));

                StartCoroutine(FinalizarSimulacionSeguro(tiempoFinReal));
            }
        }
    }

    IEnumerator FinalizarSimulacionSeguro(float tiempoFinCapturado)
    {
        yield return new WaitUntil(() => GestorSimulacion.idSimulacionActual > 0);

        int duracionCalculada = (int)(tiempoFinCapturado - tiempoInicioSimulacion);

        if (duracionCalculada <= 0) duracionCalculada = (int)(Time.time - tiempoInicioSimulacion);

        GestorSimulacionResultado.RegistrarResultado(
            GestorSimulacion.idSimulacionActual,
            "Punto de solidificacion",
            temperaturaActual.ToString("F1"),
            "°C"
        );

        GestorSimulacionFinalizar.FinalizarSimulacion(
            GestorSimulacion.idSimulacionActual,
            duracionCalculada
        );

        if (textoUI != null)
        {
            textoUI.text = "¡SOLIDIFICACIÓN\nCOMPLETADA!";
            textoUI.color = Color.green;
        }

        Debug.Log("Simulación finalizada. Duración Real: " + duracionCalculada + "s");

        Invoke("CerrarAplicacion", 5f);
    }

    IEnumerator RegistrarEventoSeguro(string nombre, string desc)
    {
        yield return new WaitUntil(() => GestorSimulacion.idSimulacionActual > 0);
        GestorSimulacionEvento.RegistrarEvento(
            GestorSimulacion.idSimulacionActual,
            nombre,
            desc,
            (int)Time.time
        );
    }

    public void RecibirAgua()
    {
        if (simulacionIniciadaBD || peticionEnCurso) return;

        peticionEnCurso = true;
        simulacionIniciadaBD = true;
        tieneAgua = true;

        if (agua != null) agua.gameObject.SetActive(true);
        if (hielo != null) hielo.gameObject.SetActive(false);

        temperaturaActual = 25f;

        tiempoInicioSimulacion = Time.time;

        GestorSimulacion.IniciarSimulacion(
            SesionUsuario.IdUsuario,
            "Solidificacion",
            "Cambio de liquido a solido",
            "VR"
        );

        StartCoroutine(RegistrarEventoSeguro("Inicio Enfriamiento", "El agua ha entrado en el congelador"));
        ActualizarTextoUI();
    }

    public void ToggleCongelador()
    {
        CongeladorEncendido = !CongeladorEncendido;
        if (luzEstufa != null) luzEstufa.enabled = CongeladorEncendido;
    }

    void ActualizarTextoUI()
    {
        if (textoUI != null)
            textoUI.text = "Temperatura\n" + Mathf.RoundToInt(temperaturaActual) + " °C";
    }

    public void ResetProceso()
    {
        simulacionIniciadaBD = false;
        simulacionFinalizadaBD = false;
        peticionEnCurso = false;
        tieneAgua = false;
        CongeladorEncendido = false;
        temperaturaActual = 25f;

        if (agua != null) agua.gameObject.SetActive(false);
        if (hielo != null) hielo.gameObject.SetActive(false);
        if (luzEstufa != null) luzEstufa.enabled = false;

        ActualizarTextoUI();
    }

    void CerrarAplicacion() { Application.Quit(); }
}