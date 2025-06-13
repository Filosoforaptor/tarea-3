using System;
using UnityEngine;

public class MeteoriteModelFinal : MonoBehaviour 
{
    [Header("Configuracion")]
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private float speed = 5f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    public event Action OnMeteoriteDestroyed;

    public int DamageToPlayer => damageToPlayer;
    public float Speed => speed;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth; // Added for potential UI or other access

    void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"MeteoriteModelFinal ({gameObject.name}): Initialized with health {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; // Already destroyed or pending destruction

        currentHealth -= amount;
        Debug.Log($"MeteoriteModelFinal ({gameObject.name}): Took {amount} damage, current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"MeteoriteModelFinal ({gameObject.name}): Health depleted, invoking OnMeteoriteDestroyed.");
            OnMeteoriteDestroyed?.Invoke();
        }
    }
}
