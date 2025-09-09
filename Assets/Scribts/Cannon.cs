using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Cannon : NetworkBehaviour
{
    // Cannon components and properties
    [SerializeField] private Transform barrel;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject cannonballPrefab;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem smokeEffect;
    [SerializeField] private AudioSource fireSound;
    [SerializeField] private float aimSpeed = 20f;
    [SerializeField] private float maxUpAngle = 30f;
    [SerializeField] private float maxDownAngle = -10f;

    // Networked variables to sync state
    public NetworkVariable<bool> HasPowder = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> IsLoaded = new NetworkVariable<bool>(false);

    private float verticalAimAngle = 0f;

    private void Update()
    {
        if (!IsOwner) return;

        // Vertical aiming with mouse scroll wheel
        float scrollInput = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            verticalAimAngle += scrollInput * aimSpeed * Time.deltaTime;
            verticalAimAngle = Mathf.Clamp(verticalAimAngle, maxDownAngle, maxUpAngle);
            barrel.localRotation = Quaternion.Euler(verticalAimAngle, 0, 0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LoadPowderServerRpc(ulong playerId)
    {
        if (IsLoaded.Value) return;
        HasPowder.Value = true;
        Debug.Log($"Cannon loaded with powder by player {playerId}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void LoadCannonballServerRpc(ulong playerId)
    {
        if (IsLoaded.Value) return;
        IsLoaded.Value = true;
        Debug.Log($"Cannon loaded with cannonball by player {playerId}");
    }

    [ServerRpc]
    public void FireServerRpc(ulong playerId)
    {
        if (!IsLoaded.Value) return;

        // Perform fire effects and spawn cannonball on all clients
        FireEffectsClientRpc();

        // Spawn cannonball on the server
        GameObject cannonballGO = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);
        cannonballGO.GetComponent<NetworkObject>().Spawn(true);
        cannonballGO.GetComponent<Cannonball>().Activate(playerId);

        // Reset cannon state
        IsLoaded.Value = false;
        HasPowder.Value = false;
        Debug.Log("Cannon fired!");
    }

    [ClientRpc]
    private void FireEffectsClientRpc()
    {
        muzzleFlash.Play();
        smokeEffect.Play();
        fireSound.Play();
    }
}
