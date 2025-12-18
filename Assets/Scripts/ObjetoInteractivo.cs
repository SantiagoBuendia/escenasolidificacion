using UnityEngine;

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

    void Awake()
    {
        mensajeVR = FindObjectOfType<MensajeVRPro>();
        jugador = FindObjectOfType<InventarioJugador>();
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
        if (esNevera)
        {
            jugador.TomarHielo(prefabAgua);
            mensajeVR?.MostrarMensaje(mensajeAccionNevera);
            return;
        }

        if (esOlla)
        {
            jugador.ColocarHieloEnOlla(controlOlla);
            mensajeVR?.MostrarMensaje(mensajeAccionOlla);
            return;
        }

        if (esBotonCongelador)
        {
            controlOlla.ToggleCongelador();

            if (controlOlla.CongeladorEncendido)
                mensajeVR?.MostrarMensaje(mensajeCongeladorEncendido);
            else
                mensajeVR?.MostrarMensaje(mensajeCongeladorApagado);

            return;
        }

        mensajeVR?.MostrarMensaje(mensajeSinAccion);
    }
}
