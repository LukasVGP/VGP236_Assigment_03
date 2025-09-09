using UnityEngine;
using Unity.Netcode;

public class Cannonball : NetworkBehaviour
{
    // Cannonball properties
    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private int damage = 1;

    // Internal state
    private Rigidbody rb;
    private float timeAlive = 0f;
    private ulong ownerClientId;

    public void Activate(ulong clientId)
    {
        ownerClientId = clientId;
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    private void Update()
    {
        if (!IsServer) return;

        // Despawn the cannonball after its lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Check if we hit a tower
        Tower tower = other.GetComponent<Tower>();
        if (tower != null && tower.OwnerClientId != ownerClientId)
        {
            Debug.Log($"Cannonball hit tower with ID {tower.OwnerClientId}!");
            tower.TakeDamageServerRpc(damage);

            // Trigger win/loss on hit
            GameManager.Instance.CheckWinConditionServerRpc(ownerClientId);

            // Destroy the cannonball
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
