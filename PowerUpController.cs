using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))] // Ensures Rigidbody is present
public class PowerUpController : MonoBehaviour
{
    public PowerUpData powerUpData;
    public float fallSpeed = 2f; // Speed at which the power-up falls

    private Rigidbody _rb; // Reference to the Rigidbody component

    private void Awake()
    {
        // Collider setup
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (!col.isTrigger)
            {
                // Ensure the collider is a trigger for OnTriggerEnter to work
                Debug.LogWarning($"PowerUpController ({gameObject.name}): Collider was not set to IsTrigger. Forcing it now. Please check prefab setup.", this);
                col.isTrigger = true;
            }
        }
        else
        {
            // This should not happen due to [RequireComponent]
            Debug.LogError($"PowerUpController ({gameObject.name}): Collider component missing! This is unexpected with RequireComponent.", this);
        }

        // Rigidbody setup
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            // This should not happen due to [RequireComponent]
            Debug.LogError($"PowerUpController ({gameObject.name}): Rigidbody component missing! Power-up will not move as intended. This is unexpected with RequireComponent.", this);
        }
        else
        {
            _rb.useGravity = false; // We'll control movement with velocity
        }
    }

    void Start()
    {
        // Check if PowerUpData is assigned. It's crucial for functionality.
        if (powerUpData == null)
        {
            Debug.LogError($"PowerUpController ({gameObject.name}): PowerUpData has not been assigned in the Inspector! The power-up may not function correctly.", this);
        }

        // Apply downward movement if Rigidbody is available
        if (_rb != null)
        {
            _rb.velocity = Vector3.down * fallSpeed;
            Debug.Log($"PowerUpController ({gameObject.name}): Movement initiated. Velocity: {_rb.velocity} (Fall Speed: {fallSpeed})", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (powerUpData == null)
        {
            Debug.LogError($"PowerUpController ({gameObject.name}): PowerUpData not assigned at collision time! Cannot apply power-up.", this);
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log($"PowerUpController ({gameObject.name}): Collided with Player ({other.name}). Attempting to apply power-up: {powerUpData.powerUpName}", this);

            PlayerModel playerModel = other.GetComponent<PlayerModel>();
            if (playerModel != null)
            {
                playerModel.ApplyPowerUp(powerUpData);
                Debug.Log($"PowerUpController: PlayerModel found on {other.name}. Power-up '{powerUpData.powerUpName}' applied.", this);
                Destroy(gameObject);
            }
            else
            {
                PlayerModel playerModel2 = other.GetComponent<PlayerModel>();
                if (playerModel2 != null)
                {
                    playerModel2.ApplyPowerUp(powerUpData);
                    Debug.Log($"PowerUpController: PlayerModel2 found on {other.name}. Power-up '{powerUpData.powerUpName}' applied via PlayerModel2.", this);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning($"PowerUpController ({gameObject.name}): Neither PlayerModel nor PlayerModel2 component found on Player object {other.name}. Cannot apply power-up.", this);
                }
            }
        }
    }
}