using System;
using System.Collections; // Required for Coroutines
using UnityEngine;

public class PlayerModel : MonoBehaviour 
{
    [Header("Shooting")]
    [SerializeField] private GameObject initialBulletPrefab; // Renamed for clarity
    [SerializeField] private Transform firePoint;
    
    // This will be the publicly accessed bullet prefab, changed by power-ups
    public GameObject BulletPrefab { get; private set; } 
    public Transform FirePoint => firePoint;

    [Header("Movement")]
    [SerializeField] private Quaternion initTiltRotation;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float tiltAngle = 30f;
    [SerializeField] private float tiltSpeed = 5f;

    public int CurrentCoins { get; private set; }

    [Header("Estadisticas de vida")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    public event Action<int> OnCoinsChanged;
    public event Action<int, int> OnHealthChanged;
    public event Action OnPlayerDied;
    // Optional: public event Action<PowerUpData> OnPowerUpCollected;
    // Optional: public event Action OnPowerUpExpired;

    public float TiltSpeed { get => tiltSpeed; private set => tiltSpeed = value; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // Power-up related fields
    private PowerUpData currentPowerUp;
    private Coroutine powerUpCoroutine;
    // Removed defaultBulletPrefab, initialBulletPrefab serves this role.
    // currentBulletPrefab is also effectively replaced by the public BulletPrefab property.

    void Awake()
    {
        currentHealth = maxHealth;
        if (initialBulletPrefab == null)
        {
            Debug.LogError($"PlayerModel ({gameObject.name}): InitialBulletPrefab is not assigned in the Inspector!", this);
        }
        BulletPrefab = initialBulletPrefab; // Initialize current bullet prefab
        Debug.Log($"PlayerModel ({gameObject.name}): Initialized. Current bullet: {BulletPrefab?.name}", this);

        // Ensure the initial bullet pool is created by PlayerPresenter2 or here as a fallback.
        // PlayerPresenter2.Start() should handle this.
        // If PoolManager is guaranteed to exist, we could do:
        // if (PoolManager.Instance != null && BulletPrefab != null && !PoolManager.Instance.IsPoolCreated(BulletPrefab))
        // {
        //     PoolManager.Instance.CreatePool(BulletPrefab);
        // }
    }

    public Vector3 CalculateMove(Vector3 direction)
    {
        return direction.normalized * moveSpeed;
    }

    public Quaternion CalculateTargetRotation(float inputX)
    {
        float tiltZ = 0f;
        if (Mathf.Abs(inputX) > 0.01f)
            tiltZ = -inputX * tiltAngle;
        return initTiltRotation * Quaternion.Euler(0f, 0f, tiltZ);
    }

    public void AddCoin()
    {
        CurrentCoins++;
        Debug.Log($"PlayerModel ({gameObject.name}): Moneda agregada, total de monedas: {CurrentCoins}", this);
        OnCoinsChanged?.Invoke(CurrentCoins);
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        Debug.Log($"PlayerModel ({gameObject.name}): Took damage, current health {currentHealth}/{maxHealth}", this);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log($"PlayerModel ({gameObject.name}): Player died.", this);
            OnPlayerDied?.Invoke();
        }
    }

    // --- PowerUp Logic ---
    public void ApplyPowerUp(PowerUpData newPowerUpData)
    {
        if (newPowerUpData == null)
        {
            Debug.LogWarning($"PlayerModel ({gameObject.name}): ApplyPowerUp called with null data.", this);
            return;
        }

        Debug.Log($"PlayerModel ({gameObject.name}): Applying power-up - {newPowerUpData.powerUpName}", this);
        currentPowerUp = newPowerUpData;

        if (powerUpCoroutine != null)
        {
            StopCoroutine(powerUpCoroutine);
            powerUpCoroutine = null;
        }

        if (currentPowerUp.bulletPrefab != null)
        {
            BulletPrefab = currentPowerUp.bulletPrefab;
            Debug.Log($"PlayerModel ({gameObject.name}): BulletPrefab changed to {BulletPrefab.name}", this);
            if (PoolManager.Instance != null)
            {
                if (!PoolManager.Instance.IsPoolCreated(BulletPrefab))
                {
                    PoolManager.Instance.CreatePool(BulletPrefab);
                    Debug.Log($"PlayerModel ({gameObject.name}): Created new pool for power-up bullet: {BulletPrefab.name}", this);
                }
            }
            else
            {
                Debug.LogError($"PlayerModel2 ({gameObject.name}): PoolManager instance is null. Cannot create pool for power-up bullet.", this);
            }
        }
        else
        {
            Debug.LogWarning($"PlayerModel ({gameObject.name}): Power-up {currentPowerUp.powerUpName} has no bullet prefab. Reverting to initial.", this);
            BulletPrefab = initialBulletPrefab; // Revert to initial if power-up has no bullet
        }
        
        // OnPowerUpCollected?.Invoke(currentPowerUp);

        if (currentPowerUp.duration > 0)
        {
            powerUpCoroutine = StartCoroutine(PowerUpTimer(currentPowerUp.duration));
        }
        else
        {
             Debug.Log($"PlayerModel ({gameObject.name}): Power-up {currentPowerUp.powerUpName} has infinite duration or will be replaced by the next one.", this);
        }
    }

    private IEnumerator PowerUpTimer(float duration)
    {
        Debug.Log($"PlayerModel ({gameObject.name}): Power-up {currentPowerUp.powerUpName} active for {duration} seconds.", this);
        yield return new WaitForSeconds(duration);
        Debug.Log($"PlayerModel ({gameObject.name}): Power-up {currentPowerUp.powerUpName} duration ended.", this);
        RevertToInitialBullet();
        // OnPowerUpExpired?.Invoke();
        currentPowerUp = null; 
        powerUpCoroutine = null;
    }

    private void RevertToInitialBullet()
    {
        Debug.Log($"PlayerModel ({gameObject.name}): Reverting to initial bullet type: {initialBulletPrefab?.name}", this);
        if (initialBulletPrefab != null)
        {
            BulletPrefab = initialBulletPrefab;
            // Ensure pool for initial bullet still exists/is created (should be by PlayerPresenter2 or similar)
            if (PoolManager.Instance != null)
            {
                if (!PoolManager.Instance.IsPoolCreated(BulletPrefab))
                {
                     Debug.LogWarning($"PlayerModel ({gameObject.name}): Initial bullet pool for {BulletPrefab.name} not found on revert. Creating it now.", this);
                     PoolManager.Instance.CreatePool(BulletPrefab);
                }
            }
            else
            {
                Debug.LogError($"PlayerModel ({gameObject.name}): PoolManager instance is null during RevertToInitialBullet.", this);
            }
        }
        else
        {
            Debug.LogError($"PlayerModel ({gameObject.name}): InitialBulletPrefab is null. Cannot revert bullet type!", this);
            // Potentially assign a fallback bullet or handle error appropriately
        }
    }
}
