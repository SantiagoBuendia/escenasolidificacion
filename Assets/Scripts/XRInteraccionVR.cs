using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


/// <summary>
/// Colocar en el GameObject del controlador que tiene XRRayInteractor (LeftHand Controller / RightHand Controller).
/// Detecta hover via TryGetCurrent3DRaycastHit y gatillo real del controlador VR (InputDevice.trigger).
/// Funciona con XR Origin (VR) y Meta Quest sin Action-based.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor))]
public class XRInteraccionVR : MonoBehaviour
{
    public enum HandRole { AutoDetectByName, Left, Right }
    [Tooltip("Si está en AutoDetectByName, buscará 'Left'/'Right' en el nombre del objeto.")]
    public HandRole handRole = HandRole.AutoDetectByName;

    // Ajuste para el control de rotación manual (Yaw)
    public float rotationSpeed = 60f; // Velocidad de giro horizontal

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor interactor;
    // ⬅️ Usando el nombre correcto: ObjetoInteractivo
    ObjetoInteractivo objetoActual;
    InputDevice device;
    bool triggerPressedPrev = false;
    float triggerThreshold = 0.6f; // ajustar sensibilidad

    // CORRECCIÓN ADVERTENCIA: Variable de estado para controlar el intento de asignación
    private bool deviceAssignedAttempted = false;

    void Start()
    {
        interactor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        TryAssignDevice();
    }

    void TryAssignDevice()
    {
        // Esta función se encarga de buscar y asignar el dispositivo.
        InputDeviceRole role = InputDeviceRole.RightHanded;
        if (handRole == HandRole.Left) role = InputDeviceRole.LeftHanded;
        else if (handRole == HandRole.Right) role = InputDeviceRole.RightHanded;
        else
        {
            // Auto detect by name
            string n = gameObject.name.ToLower();
            if (n.Contains("left")) role = InputDeviceRole.LeftHanded;
            else if (n.Contains("right")) role = InputDeviceRole.RightHanded;
            else role = InputDeviceRole.RightHanded; // fallback
        }

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithRole(role, devices);

        if (devices.Count > 0)
        {
            device = devices[0];
        }
        else
        {
            device = default(InputDevice);
        }

        deviceAssignedAttempted = true;
    }

    void Update()
    {
        // CORRECCIÓN ADVERTENCIA: Intentar re-asignar solo si no es válido
        if (!device.isValid)
        {
            TryAssignDevice();
        }

        // 1) Control de Yaw (Horizontal) con Joystick
        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValue))
            {
                // El objeto que debe rotar es el XR Origin (el Rig completo), no solo la cámara
                // Asumimos que el XR Origin es el abuelo del controlador (transform.parent.parent)
                Transform targetToRotate = transform.parent.parent;

                // --- PITCH (Vertical: Arriba/Abajo) - ELIMINADO ---

                // --- YAW (Horizontal: Derecha/Izquierda) ---
                float yawInput = primary2DAxisValue.x;
                if (Mathf.Abs(yawInput) > 0.1f)
                {
                    // Rota el Rig alrededor del eje Y (Yaw)
                    float yawAmount = yawInput * rotationSpeed * Time.deltaTime;
                    targetToRotate.Rotate(Vector3.up, yawAmount, Space.Self);
                }
            }
        }


        // 2) Hover detection via ray interactor
        if (interactor != null)
        {
            if (interactor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                // ⬅️ Usando el tipo correcto: ObjetoInteractivo
                ObjetoInteractivo nuevo = hit.collider.GetComponent<ObjetoInteractivo>();
                if (nuevo != objetoActual)
                {
                    if (objetoActual != null)
                        objetoActual.OnHoverExit();

                    objetoActual = nuevo;

                    if (objetoActual != null)
                        objetoActual.OnHoverEnter();
                }
            }
            else
            {
                if (objetoActual != null)
                {
                    objetoActual.OnHoverExit();
                    objetoActual = null;
                }
            }
        }

        // 3) Trigger detection via InputDevice
        bool triggered = false;

        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                triggered = triggerValue > triggerThreshold;
            }
            else if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primary))
            {
                triggered = primary;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
                triggered = true;
        }

        // Edge detect: only fire OnPress once per press
        if (triggered && !triggerPressedPrev)
        {
            if (objetoActual != null)
            {
                objetoActual.Interactuar();
            }
        }

        triggerPressedPrev = triggered;
    }
}