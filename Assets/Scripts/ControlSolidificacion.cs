using UnityEngine;
using TMPro;

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
    }

    // -----------------------------
    //        RECIBIR AGUA
    // -----------------------------
    public void RecibirAgua()
    {
        tieneAgua = true;
        agua.gameObject.SetActive(true);
        hielo.gameObject.SetActive(false);

        temperaturaActual = 25f; // reset a temperatura ambiente
        temperaturaActual = Mathf.Min(temperaturaActual, TEMPERATURA_MAXIMA);

        ActualizarTextoUI();
    }

    void Update()
    {
        if (tieneAgua && CongeladorEncendido)
        {
            temperaturaActual -= velocidadBajadaTemp * Time.deltaTime * 60f;
            temperaturaActual = Mathf.Clamp(temperaturaActual, -5f, 25f);
            temperaturaActual = Mathf.Min(temperaturaActual, TEMPERATURA_MAXIMA);

            // Cuando llega a 0°C ? se solidifica
            if (temperaturaActual <= 0f)
            {
                agua.gameObject.SetActive(false);
                hielo.gameObject.SetActive(true);
            }

            ActualizarTextoUI();
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
}
