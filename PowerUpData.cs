using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUpData", menuName = "Gameplay/PowerUp Data")]
public class PowerUpData : ScriptableObject
{
    [Header("Info")]
    public string powerUpName = "New PowerUp";
    public Sprite powerUpIcon; // Optional: for UI

    [Header("Gameplay Effect")]
    public GameObject bulletPrefab; // The bullet prefab this power-up grants. Can be null if the power-up has a different effect.
    public float duration = 10f;   // How long the power-up lasts. <= 0 for permanent or until replaced.

    [Header("Visuals")]
    public Color powerUpColor = Color.white; // Optional: for visual feedback on the player or bullet
}
