using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerModel))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerPresenter : MonoBehaviour
{
    private Rigidbody _rb;
    private PlayerModel _model;
    private PlayerInput _pInput;

    [SerializeField]
    private GameObject _mesh;

    public Action<bool> OnPlayerMoving { get; set; }
    public Action<int> OnCoinsCollected { get; set; }
    public Action<int, int> OnPlayerHealthChangedUI { get; set; }
    public Action OnPlayerDiedUI { get; set; }

    // Shooting related fields
    private bool canShoot = true;
    [SerializeField] private float fireRate = 0.5f; // Bullets per second

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _model = GetComponent<PlayerModel>();
        _pInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        if (_model != null && _model.BulletPrefab != null && PoolManager.Instance != null)
        {
            PoolManager.Instance.CreatePool(_model.BulletPrefab); 
        }
        else
        {
            if (_model == null) Debug.LogError("PlayerPresenter: PlayerModel is null in Start.");
            if (_model != null && _model.BulletPrefab == null) Debug.LogError("PlayerPresenter: BulletPrefab is not assigned in PlayerModel.");
            if (PoolManager.Instance == null) Debug.LogError("PlayerPresenter: PoolManager instance is not available in Start.");
        }
    }

    private void OnEnable()
    {
        if (_model != null)
        {
            _model.OnCoinsChanged += PresenterCoinsChanged;
            _model.OnHealthChanged += HandleModelHealthChanged;
            _model.OnPlayerDied += HandleModelPlayerDied;
        }

        if (_pInput != null)
        {
            _pInput.OnFireInput += HandleFire;
        }
    }

    private void OnDisable()
    {
        if (_model != null)
        {
            _model.OnCoinsChanged -= PresenterCoinsChanged;
            _model.OnHealthChanged -= HandleModelHealthChanged;
            _model.OnPlayerDied -= HandleModelPlayerDied;
        }

        if (_pInput != null)
        {
            _pInput.OnFireInput -= HandleFire;
        }
    }

    void FixedUpdate()
    {
        if (_pInput != null && !_pInput.enabled)
        {
            return;
        }

        Vector3 input = _pInput.Axis;
        ApplyMovement(input);
        UpdateTilt(input.x);
    }

    public void ApplyMovement(Vector3 direction)
    {
        if (_model == null) return;
        _rb.velocity = _model.CalculateMove(direction);

        bool isMoving = direction.magnitude > 0.1f;
        OnPlayerMoving?.Invoke(isMoving);
    }

    private void UpdateTilt(float inputX)
    {
        if (_model == null || _mesh == null) return;
        Quaternion targetRotation = _model.CalculateTargetRotation(inputX);
        Quaternion currentRotation = _mesh.transform.localRotation;
        _mesh.transform.localRotation = Quaternion.Slerp(currentRotation, targetRotation, _model.TiltSpeed * Time.fixedDeltaTime);
    }

    public void ProcessDamage(int damageAmount)
    {
        if (_model != null)
        {
            _model.TakeDamage(damageAmount);
        }
    }

    private void HandleModelHealthChanged(int currentHealth, int maxHealth)
    {
        OnPlayerHealthChangedUI?.Invoke(currentHealth, maxHealth);
    }

    private void HandleModelPlayerDied()
    {
        if (_pInput != null)
        {
            _pInput.enabled = false;
        }
        OnPlayerDiedUI?.Invoke();
    }

    private void HandleFire()
    {
        if (!canShoot) return;

        if (_model == null || _model.BulletPrefab == null || PoolManager.Instance == null || _model.FirePoint == null)
        {
            Debug.LogError("PlayerPresenter: Missing references for shooting. Model: " + (_model != null) + 
                           ", BulletPrefab: " + (_model?.BulletPrefab != null) + 
                           ", PoolManager: " + (PoolManager.Instance != null) + 
                           ", FirePoint: " + (_model?.FirePoint != null));
            return;
        }

        GameObject bulletObject = PoolManager.Instance.GetObject(_model.BulletPrefab);
        if (bulletObject != null)
        {
            // Ensure the bullet's Rigidbody is kinematic if it's being controlled directly by transform
            // or ensure it's not kinematic if Move method applies forces.
            // For now, assuming Bullet.Move handles its own physics state.
            bulletObject.transform.position = _model.FirePoint.position;
            bulletObject.transform.rotation = _model.FirePoint.rotation; 
            
            Bullet bulletComponent = bulletObject.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Move(_model.FirePoint.forward);
            }
            else
            {
                Debug.LogError("PlayerPresenter: Bullet prefab does not have a Bullet component. Returning to pool.");
                PoolManager.Instance.ReturnObject(bulletObject);
            }
            StartCoroutine(ShootCooldown());
        }
    }

    private IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_model == null) 
        {
            Debug.LogError("PlayerPresenter: PlayerModel is null in OnTriggerEnter. Cannot process collision.", this);
            return;
        }

        if (other.CompareTag("Coin"))
        {
            _model.AddCoin();
            Destroy(other.gameObject); // Coins are not pooled in this example
        }
        // else if (other.CompareTag("PowerUp"))
        // {
        //     // Power-up collection is handled by PowerUpController.cs, which directly
        //     // interacts with PlayerModel.ApplyPowerUp(). No specific action needed here
        //     // unless PlayerPresenter2 needs to react to the power-up in a unique way
        //     // independent of PlayerModel's handling.
        //     // Debug.Log("PlayerPresenter2: Ignored PowerUp tag collision, handled by PowerUpController.");
        // }
        // Other specific trigger collisions for PlayerPresenter2 could be added here.
    }

    private void PresenterCoinsChanged(int newCoinCount)
    {
        OnCoinsCollected?.Invoke(newCoinCount);
    }
}
