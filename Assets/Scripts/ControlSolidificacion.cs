using UnityEngine;
using TMPro;
using System.Collections;

public class ControlSolidificacion : MonoBehaviour
{
    [Header("Referencias de escena")]
    public Transform hielo;        // Cubo de hielo final
    public Transform agua;         // Modelo de agua en la olla
    public Light luzEstufa;        // Luz indicadora del congelador
    public TMP_Text textoUI;       // Texto de temperatura

    [Header("Ajustes")]
    public float velocidadBajadaTemp = 0.05f;   // Qué tan rápido baja la temperatura
    public float velocidadDerretir = 0.005f;    // NO SE USA
    public float velocidadSubidaAgua = 0.05f;   // NO SE USA

    // -------- ESTADOS INTERNOS --------
    public bool CongeladorEncendido = false;
    public bool tieneAgua = false;

    // VARIABLES PARA BASE DE DATOS
    private float tiempoInicioSimulacion;
    private bool simulacionIniciadaBD = false;
    private bool simulacionFinalizadaBD = false;

    float temperaturaActual = 25f;  // Temperatura ambiente al inicio

    // ?? ADICIÓN: límite máximo de temperatura
    private const float TEMPERATURA_MAXIMA = 150f;

    Vector3 aguaPosInicial;
    Vector3 hieloPosInicial;

    void Start()
    {
        aguaPosInicial = agua.localPosition;
        hieloPosInicial = hielo.localPosition;

        agua.gameObject.SetActive(false);
        hielo.gameObject.SetActive(false);

        ActualizarTextoUI();
    }

    // -----------------------------
    //     ENCIENDE / APAGA
    // -----------------------------
    public void ToggleCongelador()
    {
        CongeladorEncendido = !CongeladorEncendido;

        if (luzEstufa != null)
            luzEstufa.enabled = CongeladorEncendido;

        // --- REGISTRAR EVENTO EN BASE DE DATOS ---
        if (simulacionIniciadaBD && !simulacionFinalizadaBD && GestorSimulacion.idSimulacionActual > 0)
        {
            GestorSimulacionEvento.RegistrarEvento(
                GestorSimulacion.idSimulacionActual,
                CongeladorEncendido ? "Congelador Encendido" : "Congelador Apagado",
                "Cambio de estado del interruptor",
                (int)Time.time
            );
        }
    }

    // -----------------------------
    //        RECIBIR AGUA
    // -----------------------------
    public void RecibirAgua()
    {
        if (simulacionIniciadaBD) return;

        tieneAgua = true;
        agua.gameObject.SetActive(true);
        hielo.gameObject.SetActive(false);

        temperaturaActual = 25f; // reset a temperatura ambiente
        temperaturaActual = Mathf.Min(temperaturaActual, TEMPERATURA_MAXIMA);

        // INICIAR EN BD (Una sola vez)
        simulacionIniciadaBD = true;
        tiempoInicioSimulacion = Time.time;

        GestorSimulacion.IniciarSimulacion(
            SesionUsuario.IdUsuario,
            "Solidificacion",
            "Cambio de liquido a solido",
            "VR"
        );

        StartCoroutine(RegistrarEventoInicial());

        ActualizarTextoUI();
    }

    void Update()
    {
        
        if (tieneAgua && CongeladorEncendido && !simulacionFinalizadaBD)
        {
            temperaturaActual -= velocidadBajadaTemp * Time.deltaTime * 60f;
            temperaturaActual = Mathf.Clamp(temperaturaActual, -5f, 25f);

            ActualizarTextoUI();

            // Cuando llega a 0°C -> se solidifica
            if (temperaturaActual <= 0f)
            {
                // Marcamos como finalizado para que este IF no se repita más
                simulacionFinalizadaBD = true;

                agua.gameObject.SetActive(false);
                hielo.gameObject.SetActive(true);

                // LLAMAR A GUARDADO ÚNICO
                FinalizarSimulacionSolidificacion();
            }
        }
    }

    // -----------------------------
    //      UI DE TEMPERATURA
    // -----------------------------
    void ActualizarTextoUI()
    {
        if (textoUI != null)
        {
            textoUI.text = "temperatura\n" + Mathf.RoundToInt(temperaturaActual) + " grados";
        }
    }

    void FinalizarSimulacionSolidificacion()
    {
        // 1. Guardar Resultado
        GestorSimulacionResultado.RegistrarResultado(
            GestorSimulacion.idSimulacionActual,
            "Punto de solidificacion",
            temperaturaActual.ToString("F1"),
            "°C"
        );

        // 2. Finalizar Simulación
        int duracionReal = (int)(Time.time - tiempoInicioSimulacion);
        GestorSimulacionFinalizar.FinalizarSimulacion(
            GestorSimulacion.idSimulacionActual,
            duracionReal
        );

        if (textoUI != null)
        {
            textoUI.text = "¡SOLIDIFICACIÓN\nCOMPLETADA!";
            textoUI.color = Color.green;
        }

        // 3. Cerrar App en 5 segundos
        Invoke("CerrarAplicacion", 5f);
    }

    void CerrarAplicacion()
    {
        Debug.Log("Saliendo del simulador...");
        Application.Quit();
    }

    IEnumerator RegistrarEventoInicial()
    {
        yield return new WaitUntil(() => GestorSimulacion.idSimulacionActual > 0);
        GestorSimulacionEvento.RegistrarEvento(
            GestorSimulacion.idSimulacionActual,
            "Inicio Enfriamiento",
            "El agua ha entrado en el congelador",
            (int)Time.time
        );
    }
}
