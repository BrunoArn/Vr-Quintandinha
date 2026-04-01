using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpawnDestroyRoom : MonoBehaviour
{
    [SerializeField] BoxCollider triggerCollider; // The collider that will trigger the spawning and destroying of the room

    [SerializeField] bool show = false;
    [SerializeField] private GameObject roomToSHow; // The prefab of the room to spawn

    [Space]
    [SerializeField] bool hide = false;
    [SerializeField] private GameObject roomToHide; // The room to destroy when the player enters the trigger

    void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<BoxCollider>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (show == true)
        {
            roomToSHow.SetActive(true);
            show = false; // Set spawn to false to prevent multiple spawns if the player stays in the trigger
        }

        if (hide == true)
        {
            roomToHide.SetActive(false);
            hide = false; // Set destroy to false to prevent multiple destructions if the player stays in the trigger
        }
        Destroy(this.gameObject); // Destroy the trigger after it has been activated
    }
}