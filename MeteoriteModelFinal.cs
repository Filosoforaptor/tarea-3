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
    public int MaxHealth => maxHealth; // Añadido para posible UI u otro acceso

    void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"MeteoriteModelFinal ({gameObject.name}): Inicializado con salud {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; // Ya destruido o pendiente de destrucción

        currentHealth -= amount;
        Debug.Log($"MeteoriteModelFinal ({gameObject.name}): Recibió {amount} de daño, salud actual: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"MeteoriteModelFinal ({gameObject.name}): Salud agotada, invocando OnMeteoriteDestroyed.");
            OnMeteoriteDestroyed?.Invoke();
        }
    }
}
