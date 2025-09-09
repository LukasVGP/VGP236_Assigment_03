using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    // Player and camera properties
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float interactDistance = 3.0f;
    [SerializeField] private GameObject carriedCannonball;
    [SerializeField] private GameObject carriedPowder;

    // Internal state variables
    private Rigidbody rb;
    private Vector3 moveVelocity;
    private Vector2 lookInput;
    private bool hasPowder = false;
    private bool hasCannonball = false;

    // UI for player messages
    public NetworkVariable<string> PlayerName = new NetworkVariable<string>("Player");
    public NetworkVariable<bool> IsPirate = new NetworkVariable<bool>(false);

    // Input actions
    private PlayerInputActions playerInputActions;

    public bool HasPowder { get => hasPowder; set => hasPowder = value; }
    public bool HasCannonball { get => hasCannonball; set => hasCannonball = value; }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable movement and camera for non-local players
            GetComponentInChildren<Camera>().enabled = false;
            enabled = false;
            return;
        }

        // Setup input actions for the local player
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Move.Enable();
        playerInputActions.Player.Look.Enable();
        playerInputActions.Player.Interact.Enable();
        playerInputActions.Player.Interact.performed += OnInteractPerformed;

        // Get the Rigidbody
        rb = GetComponent<Rigidbody>();

        // Hide and lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Assign player team based on ClientId
        AssignTeamServerRpc();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            // Cleanup input actions on despawn
            playerInputActions.Player.Interact.performed -= OnInteractPerformed;
            playerInputActions.Dispose();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Handle player rotation
        lookInput = playerInputActions.Player.Look.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * lookInput.x * rotationSpeed * Time.deltaTime);

        // Update carried item visuals
        carriedPowder.SetActive(hasPowder);
        carriedCannonball.SetActive(hasCannonball);

        // Update UI messages
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDistance))
        {
            Cannon cannon = hit.collider.GetComponent<Cannon>();
            if (cannon != null)
            {
                if (!cannon.IsLoaded.Value && hasPowder && !cannon.HasPowder.Value)
                {
                    GameUIManager.Instance.SetPlayerMessage("Load Powder");
                }
                else if (!cannon.IsLoaded.Value && hasCannonball && cannon.HasPowder.Value)
                {
                    GameUIManager.Instance.SetPlayerMessage("Load Ball");
                }
                else if (cannon.IsLoaded.Value)
                {
                    GameUIManager.Instance.SetPlayerMessage("FIRE!");
                }
                else if (hasCannonball && !hasPowder)
                {
                    GameUIManager.Instance.SetPlayerMessage("Blackpowder First!");
                }
                else
                {
                    GameUIManager.Instance.ClearPlayerMessage();
                }
            }
            else
            {
                GameUIManager.Instance.ClearPlayerMessage();
            }
        }
        else
        {
            GameUIManager.Instance.ClearPlayerMessage();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Handle player movement using Rigidbody
        Vector2 moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();
        moveVelocity = (transform.forward * moveInput.y + transform.right * moveInput.x) * moveSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        // Handle interactions
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            ItemPickup item = hit.collider.GetComponent<ItemPickup>();
            Cannon cannon = hit.collider.GetComponent<Cannon>();

            if (item != null)
            {
                // Pickup item
                if (item.ItemType == ItemPickup.Type.Powder && !hasPowder)
                {
                    hasPowder = true;
                    item.DespawnServerRpc();
                }
                else if (item.ItemType == ItemPickup.Type.Cannonball && !hasCannonball)
                {
                    hasCannonball = true;
                    item.DespawnServerRpc();
                }
            }
            else if (cannon != null)
            {
                // Interact with cannon
                if (hasPowder && !cannon.HasPowder.Value)
                {
                    cannon.LoadPowderServerRpc(OwnerClientId);
                    hasPowder = false;
                }
                else if (hasCannonball && cannon.HasPowder.Value && !cannon.IsLoaded.Value)
                {
                    cannon.LoadCannonballServerRpc(OwnerClientId);
                    hasCannonball = false;
                }
                else if (cannon.IsLoaded.Value)
                {
                    cannon.FireServerRpc(OwnerClientId);
                }
            }
        }
    }

    [ServerRpc]
    private void AssignTeamServerRpc()
    {
        // Player 1 (host) is British, Player 2 is Pirates
        if (OwnerClientId == 0)
        {
            IsPirate.Value = false;
        }
        else if (OwnerClientId == 1)
        {
            IsPirate.Value = true;
        }
    }
}
