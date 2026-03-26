using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpawnDestroyRoom : MonoBehaviour
{
    [SerializeField] BoxCollider triggerCollider; // The collider that will trigger the spawning and destroying of the room

    [SerializeField] bool spawn = false;
    [SerializeField] private GameObject roomPrefab; // The prefab of the room to spawn
    [SerializeField] private Transform spawnPoint; // The point where the room will be spawned

    [Space]
    [SerializeField] bool destroy = false;
    [SerializeField] private GameObject roomToDestroy; // The room to destroy when the player enters the trigger

    void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<BoxCollider>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (spawn == true)
        {
            Instantiate(roomPrefab, spawnPoint.position, roomPrefab.transform.rotation);
            spawn = false; // Set spawn to false to prevent multiple spawns if the player stays in the trigger
        }

        if (destroy == true)
        {
            Destroy(roomToDestroy);
            destroy = false; // Set destroy to false to prevent multiple destructions if the player stays in the trigger
        }
    }
}