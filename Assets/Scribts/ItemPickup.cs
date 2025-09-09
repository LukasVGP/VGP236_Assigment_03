using UnityEngine;
using Unity.Netcode;

public class ItemPickup : NetworkBehaviour
{
    // Item types
    public enum Type { Powder, Cannonball }
    public Type ItemType;

    [ServerRpc(RequireOwnership = false)]
    public void DespawnServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
}
