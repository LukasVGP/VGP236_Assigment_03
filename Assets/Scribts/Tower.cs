using UnityEngine;
using Unity.Netcode;

public class Tower : NetworkBehaviour
{
    // Team affiliation and health
    public enum Team { British, Pirates }
    [SerializeField] private Team team;
    public Team PlayerTeam { get => team; set => team = value; }
    public NetworkVariable<int> Health = new NetworkVariable<int>(5);

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damageAmount)
    {
        Health.Value -= damageAmount;
        Debug.Log($"Tower health is now {Health.Value}");
    }
}
