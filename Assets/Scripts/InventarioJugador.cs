using UnityEngine;

public class InventarioJugador : MonoBehaviour
{
    public GameObject objetoEnMano;
    public Transform manoJugador;

    public bool tieneAgua = false;

    void Start()
    {
        if (objetoEnMano != null)
            objetoEnMano.SetActive(false);
    }

    public void TomarHielo(GameObject prefabAgua)
    {
        if (tieneAgua) return;

        objetoEnMano = Instantiate(prefabAgua, manoJugador);
        objetoEnMano.transform.localPosition = Vector3.zero;
        objetoEnMano.transform.localRotation = Quaternion.identity;
        objetoEnMano.SetActive(true);

        tieneAgua = true;
    }

    public void ColocarHieloEnOlla(ControlSolidificacion control)
    {
        if (!tieneAgua) return;

        control.RecibirAgua();

        if (objetoEnMano) Destroy(objetoEnMano);

        objetoEnMano = null;
        tieneAgua = false;
    }
}
