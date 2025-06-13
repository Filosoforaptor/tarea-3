using System.Collections;
using UnityEngine;

// Duplicate using statements removed

public class Respawner : MonoBehaviour
{
    [SerializeField] private BoxCollider respawnArea;     // Asignar en inspector
    [SerializeField] private float respawnY = 1f;          // Altura de respawn
    // Remember to add your PowerUp prefabs to this array in the Inspector!
    [SerializeField] private GameObject[] objectsToSpawn; // Array de prefabs (e.g., Meteorites, PowerUps)
    [SerializeField] private float spawnInterval = 2f;     // Intervalo entre spawns

    private void Start()
    {
        if (objectsToSpawn == null || objectsToSpawn.Length == 0)
        {
            Debug.LogError("Respawner: No objects assigned to spawn array (objectsToSpawn). Please assign prefabs in the Inspector.", this);
            return;
        }

        if (respawnArea == null)
        {
            Debug.LogError("Respawner: Respawn Area (BoxCollider) is not assigned. Please assign it in the Inspector.", this);
            return;
        }

        StartCoroutine(SpawnObjectsRoutine());
    }

    private IEnumerator SpawnObjectsRoutine()
    {
        while (true)
        {
            // Ensure objectsToSpawn is not empty to prevent error (already checked in Start, but good practice)
            if (objectsToSpawn.Length == 0) 
            {
                yield return new WaitForSeconds(spawnInterval); // Wait and try again, or break
                continue;
            }

            GameObject prefab = objectsToSpawn[Random.Range(0, objectsToSpawn.Length)];
            if (prefab == null)
            {
                Debug.LogWarning("Respawner: A null prefab was selected from objectsToSpawn array. Skipping this spawn cycle.", this);
                yield return new WaitForSeconds(spawnInterval);
                continue;
            }

            GameObject newObj = Instantiate(prefab);
            Debug.Log($"Respawner: Spawned object {newObj.name}", this);
            RespawnAtRandomX(newObj);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Detects if specific tagged objects re-enter the respawn trigger area from below (e.g. if they fall through world)
    // Note: This OnTriggerEnter is on the Respawner itself. If objects to be respawned (like meteorites)
    // have their own triggers for other purposes, ensure tags and layers are managed to prevent interference.
    private void OnTriggerEnter(Collider other)
    {
        if (respawnArea == null) return; // Should be assigned, checked in Start

        // Example: Respawn meteorites or coins if they somehow fall back into the respawn area.
        // This might be more relevant if objects could "miss" the play area and fall into a safety net trigger.
        // For items spawned from above like meteorites, this specific trigger might be less used
        // unless they are meant to be recycled if they pass through the game world without interaction.
        if (other.CompareTag("Meteorite") || other.CompareTag("Coin") || other.CompareTag("PowerUp")) // Added PowerUp tag
        {
            Debug.Log($"Respawner: Object {other.name} with tag {other.tag} re-entered respawn trigger. Respawning it.", this);
            RespawnAtRandomX(other.gameObject);
        }
    }

    public void RespawnAtRandomX(GameObject obj)
    {
        if (respawnArea == null)
        {
            Debug.LogError("Respawner: Respawn Area is null in RespawnAtRandomX. Cannot respawn object.", this);
            return;
        }
        Vector3 center = respawnArea.bounds.center;
        float halfWidth = respawnArea.bounds.extents.x;

        float randomX = Random.Range(center.x - halfWidth, center.x + halfWidth);
        // Assuming Z position should be same as respawn area's center Z, or 0 if 2D-like view
        Vector3 respawnPos = new Vector3(randomX, respawnY, center.z); 

        obj.transform.position = respawnPos;
        obj.transform.rotation = Quaternion.identity; // Reset rotation

        // Reset physics state
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // If the object was a pooled object that was set inactive, re-activate it.
        // This is crucial if using pooling with the respawner.
        if (!obj.activeSelf)
        {
            obj.SetActive(true);
        }
        
        // If it's a meteorite, ensure its model/presenter resets its speed/course if necessary.
        // For meteorites spawned by this system, their Start/OnEnable should handle initial movement.
        // If recycling existing meteorites, might need:
        // MeteoritePresenterFinal2 meteorite = obj.GetComponent<MeteoritePresenterFinal2>();
        // if (meteorite != null) { /* meteorite.ResetState(); */ } // ResetState would need to be implemented
    }
}
