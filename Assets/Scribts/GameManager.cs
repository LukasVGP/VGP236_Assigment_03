using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    // UI screen prefabs
    [SerializeField] private GameObject winScreenPrefab;
    [SerializeField] private GameObject loseScreenPrefab;

    private Dictionary<ulong, Tower> playerTowers = new Dictionary<ulong, Tower>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Register player towers on the server
            var towers = FindObjectsOfType<Tower>();
            for (int i = 0; i < towers.Length; i++)
            {
                playerTowers.Add((ulong)i, towers[i]);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckWinConditionServerRpc(ulong damagerId)
    {
        // This is a simple implementation. A better approach would be to track hits
        if (playerTowers[damagerId].Health.Value <= 0)
        {
            GameWonClientRpc(damagerId);
        }
        else
        {
            // Find the other player's tower and check its health
            foreach (var entry in playerTowers)
            {
                if (entry.Key != damagerId)
                {
                    if (entry.Value.Health.Value <= 0)
                    {
                        GameWonClientRpc(damagerId);
                    }
                }
            }
        }
    }

    [ClientRpc]
    private void GameWonClientRpc(ulong winnerId)
    {
        if (NetworkManager.Singleton.LocalClientId == winnerId)
        {
            Debug.Log("You Won!");
            Instantiate(winScreenPrefab);
        }
        else
        {
            Debug.Log("You Lost!");
            Instantiate(loseScreenPrefab);
        }
    }
}
