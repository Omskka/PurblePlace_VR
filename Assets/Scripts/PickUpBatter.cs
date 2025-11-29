using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpBatter : MonoBehaviour
{
    private bool isHeld = false;
    private Transform holdPoint;
    private Rigidbody rb;

    // Reference to spawner
    public BatterSpawner spawner;

    // All batter colliders in the scene
    private Collider[] allBatterColliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // create a hold point in front of the camera
        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        holdPoint.SetParent(Camera.main.transform);

        // find all batters in the scene and store their colliders
        PickUpBatter[] batters = FindObjectsOfType<PickUpBatter>();
        allBatterColliders = new Collider[batters.Length];
        for (int i = 0; i < batters.Length; i++)
        {
            allBatterColliders[i] = batters[i].GetComponent<Collider>();
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

                    // ignore collisions with other batters while held
                    foreach (var col in allBatterColliders)
                    {
                        if (col != GetComponent<Collider>())
                            Physics.IgnoreCollision(GetComponent<Collider>(), col, true);
                    }

                    // spawn a new batter at the spawner
                    if (spawner != null)
                    {
                        spawner.ClearCurrentBatter(); // optional: mark spawner free
                        spawner.SpawnNewBatter();
                    }
                }
            }
            else if (isHeld)
            {
                // DROP batter
                isHeld = false;
                rb.isKinematic = false;

                // re-enable collisions with other batters
                foreach (var col in allBatterColliders)
                {
                    if (col != GetComponent<Collider>())
                        Physics.IgnoreCollision(GetComponent<Collider>(), col, false);
                }
            }
        }
    }
}
