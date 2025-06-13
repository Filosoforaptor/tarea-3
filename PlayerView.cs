using UnityEngine;
using TMPro;

public class PlayerView : MonoBehaviour
{
    private PlayerPresenter playerPresenter;

    [Header("Elementos IU")]
    [SerializeField] private TextMeshProUGUI coinsText; // Texto de monedas del jugador
    [SerializeField] private TextMeshProUGUI healthText; // Texto de vida del jugador
    [SerializeField] private GameObject gameOverCanvas; // Pantalla de perder

    [Header("Efectos")]
    [SerializeField] private AudioSource coinSound;
    [SerializeField] private ParticleSystem _particleSystem;

    void Awake()
    {
        playerPresenter = GetComponent<PlayerPresenter>();
        if (playerPresenter == null)
        {
            Debug.LogError("PlayerView: PlayerPresenter Componente no encontrado en el gameObject", this);
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PlayerView: El canvas del gameOver no esta asignado en el inspector", this);
        }
    }

    private void OnEnable()
    {
        if (playerPresenter != null)
        {
            playerPresenter.OnCoinsCollected += HandleCoinsCollected;
            playerPresenter.OnPlayerMoving += HandlePlayerMoving;
            playerPresenter.OnPlayerHealthChangedUI += HandlePlayerHealthChanged;
            playerPresenter.OnPlayerDiedUI += HandlePlayerDied_View;
        }
    }

    private void OnDisable()
    {
        if (playerPresenter != null)
        {
            playerPresenter.OnCoinsCollected -= HandleCoinsCollected;
            playerPresenter.OnPlayerMoving -= HandlePlayerMoving;
            playerPresenter.OnPlayerHealthChangedUI -= HandlePlayerHealthChanged;
            playerPresenter.OnPlayerDiedUI -= HandlePlayerDied_View;
        }
    }

    private void HandlePlayerMoving(bool isMoving)
    {
        if (_particleSystem == null) return;

        if (isMoving)
        {
            if (!_particleSystem.isPlaying) _particleSystem.Play();
        }
        else
        {
            if (_particleSystem.isPlaying) _particleSystem.Stop();
        }
    }

    private void HandleCoinsCollected(int newCoinCount)
    {
        if (coinSound != null) coinSound.Play();
        if (coinsText != null) coinsText.text = "Coins: " + newCoinCount;
    }

    private void HandlePlayerHealthChanged(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"Life: {currentHealth}/{maxHealth}";
        }
        else
        {
            Debug.LogWarning("PlayerView: El texto de vida del a IU no esta asignado en el inspector.", this);
        }
        Debug.Log($"PlayerView: Vida actualizada a {currentHealth}/{maxHealth}");
    }

    private void HandlePlayerDied_View()
    {
        Debug.Log("PlayerView: Evento PlayerDied se recibe de presenter. Te moriste!");
         if (gameOverCanvas != null)
         {
             gameOverCanvas.SetActive(true);
         }
         else
         {
             Debug.LogError("PlayerView: El canvas de GameOver no esta asignado no se puede mostrar pantalla de perder.", this);
         }

    }
}