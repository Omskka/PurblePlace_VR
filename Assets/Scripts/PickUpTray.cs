using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpTray : MonoBehaviour
{
    private bool isHeld = false;
    private Transform holdPoint;
    private Rigidbody rb;

    // Reference to spawner
    public TraySpawner spawner;

    // All tray colliders in the scene
    private Collider[] allTrayColliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // create a hold point in front of the camera
        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        holdPoint.SetParent(Camera.main.transform);

        // find all trays in the scene and store their colliders
        Tray[] trays = FindObjectsOfType<Tray>();
        allTrayColliders = new Collider[trays.Length];
        for (int i = 0; i < trays.Length; i++)
        {
            allTrayColliders[i] = trays[i].GetComponent<Collider>();
        }
    }

    void Update()
    {
        if (isHeld)
            transform.position = holdPoint.position;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!isHeld && Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    // PICK UP
                    isHeld = true;
                    rb.isKinematic = true;

                    // ignore collisions with other trays while held
                    foreach (var col in allTrayColliders)
                    {
                        if (col != GetComponent<Collider>())
                            Physics.IgnoreCollision(GetComponent<Collider>(), col, true);
                    }

                    // spawn a new tray at the spawner
                    if (spawner != null)
                        spawner.SpawnNewTray();
                }
            }
            else if (isHeld)
            {
                // DROP tray
                isHeld = false;
                rb.isKinematic = false;

                // re-enable collisions with other trays
                foreach (var col in allTrayColliders)
                {
                    if (col != GetComponent<Collider>())
                        Physics.IgnoreCollision(GetComponent<Collider>(), col, false);
                }
            }
        }
    }
}
