using UnityEngine;
using System.Collections;

public class ObjetoInteractivo : MonoBehaviour
{
    [TextArea] public string mensajeExplicacion;

    public bool esNevera;
    public bool esOlla;
    public bool esBotonCongelador;

    [Header("Mensajes")]
    public string mensajeAccionNevera = "Has tomado un vaso de agua.";
    public string mensajeAccionOlla = "Has colocado el agua en la olla.";
    public string mensajeCongeladorEncendido = "El congelador está encendido.";
    public string mensajeCongeladorApagado = "El congelador se ha apagado.";
    public string mensajeSinAccion = "No hay acción para este objeto.";

    [Header("Referencias")]
    public GameObject prefabAgua;
    public ControlSolidificacion controlOlla;

    MensajeVRPro mensajeVR;
    InventarioJugador jugador;

    // Para evitar spam de clics
    private float ultimoClic = 0f;
    private float espera = 0.5f;

    void Awake()
    {
        mensajeVR = FindObjectOfType<MensajeVRPro>();
        jugador = FindObjectOfType<InventarioJugador>();
    }

    // Corrutina que recibe el tiempo exacto del clic
    IEnumerator RegistrarEventoSeguro(string nombre, string descripcion, int tiempoReal)
    {
        yield return new WaitUntil(() => GestorSimulacion.idSimulacionActual > 0);

        GestorSimulacionEvento.RegistrarEvento(
            GestorSimulacion.idSimulacionActual,
            nombre,
            descripcion,
            tiempoReal 
        );
        Debug.Log($"✅ Evento '{nombre}' enviado con éxito.");
    }

    public void OnHoverEnter()
    {
        mensajeVR?.MostrarMensaje(mensajeExplicacion, 999f);
    }

    public void OnHoverExit()
    {
        mensajeVR?.OcultarAhora();
    }

    public void Interactuar()
    {
        // Evitar que un solo clic registre 50 veces
        if (Time.time - ultimoClic < espera) return;
        ultimoClic = Time.time;

        int tiempoDelClic = (int)Time.time;

        if (esNevera)
        {
            jugador.TomarHielo(prefabAgua);
            mensajeVR?.MostrarMensaje(mensajeAccionNevera);
            StartCoroutine(RegistrarEventoSeguro("Saco un vaso de agua", "El usuario saco un vaso de agua", tiempoDelClic));
            return;
        }

        if (esOlla)
        {
            controlOlla?.RecibirAgua();
            jugador.ColocarHieloEnOlla(controlOlla);
            mensajeVR?.MostrarMensaje(mensajeAccionOlla);
            StartCoroutine(RegistrarEventoSeguro("Agua en olla", "El usuario coloco el hielo sobre la nevera/olla", tiempoDelClic));
            return;
        }

        if (esBotonCongelador)
        {
            if (controlOlla == null) return;

            controlOlla.ToggleCongelador();

            
            if (controlOlla.CongeladorEncendido)
            {
                mensajeVR?.MostrarMensaje(mensajeCongeladorEncendido);
                StartCoroutine(RegistrarEventoSeguro("Encendido nevera", "El usuario pulso el boton de encendido", tiempoDelClic));
            }
            else
            {
                mensajeVR?.MostrarMensaje(mensajeCongeladorApagado);
                StartCoroutine(RegistrarEventoSeguro("Apagada nevera", "El usuario pulso el boton de apagado", tiempoDelClic));
            }
            return;
        }

        mensajeVR?.MostrarMensaje(mensajeSinAccion);
    }
}