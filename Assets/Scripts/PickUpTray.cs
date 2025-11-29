using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpTray : MonoBehaviour
{
    private bool isHeld = false;
    private Transform holdPoint;
    private Rigidbody rb;

    public TraySpawner spawner;
    private Collider[] allTrayColliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Hold point in front of camera
        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        holdPoint.SetParent(Camera.main.transform);

        // Cache all trays for collision ignoring
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
        {
            transform.position = holdPoint.position;
        }

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

                    // Ignore collisions while held
                    foreach (var col in allTrayColliders)
                        if (col != GetComponent<Collider>())
                            Physics.IgnoreCollision(GetComponent<Collider>(), col, true);

                    if (spawner != null)
                        spawner.SpawnNewTray();
                }
            }
            else if (isHeld)
            {
                // DROP tray
                isHeld = false;
                rb.isKinematic = false;

                // Re-enable collisions
                foreach (var col in allTrayColliders)
                    if (col != GetComponent<Collider>())
                        Physics.IgnoreCollision(GetComponent<Collider>(), col, false);

                // --- SNAP LOGIC ---
                TraySnapPoint[] snapPoints = FindObjectsOfType<TraySnapPoint>();
                TraySnapPoint closestSnap = null;
                float closestDist = Mathf.Infinity;

                foreach (var snap in snapPoints)
                {
                    float dist = Vector3.Distance(transform.position, snap.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestSnap = snap;
                    }
                }

                // Snap if within 1 unit (adjust if needed)
                if (closestSnap != null && closestDist <= 1f)
                {
                    closestSnap.SnapTray(gameObject);
                    rb.isKinematic = true; // keep it fixed
                }
            }
        }
    }
}
