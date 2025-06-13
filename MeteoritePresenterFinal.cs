using UnityEngine;

// Ensure these components are present. The model should be the new one.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))] 
[RequireComponent(typeof(MeteoriteModelFinal))] // Changed to MeteoriteModelFinal2

public class MeteoritePresenterFinal : MonoBehaviour // Renamed class
{
    private MeteoriteModelFinal model; // Changed to MeteoriteModelFinal
    private MeteoriteViewFinal view; // Assuming MeteoriteViewFinal does not need changes for this step
    private Rigidbody rb;
    private bool isDestroyed = false;

    void Awake()
    {
        model = GetComponent<MeteoriteModelFinal>(); // Changed to MeteoriteModelFinal2
        view = GetComponent<MeteoriteViewFinal>();
        rb = GetComponent<Rigidbody>();

        if (model != null)
        {
            model.OnMeteoriteDestroyed += HandleMeteoriteModelDestroyed;
        }
        else
        {
            Debug.LogError($"MeteoritePresenterFinal ({gameObject.name}): MeteoriteModelFinal component not found!");
        }
    }

    void Start()
    {
    
    }

    void FixedUpdate()
    {
        if (model != null && !isDestroyed)
        {
            float currentSpeed = model.Speed;
            Vector3 direction = Vector3.down;
            // Calculate the change in position for this frame
            Vector3 positionChangeThisFrame = direction * currentSpeed * Time.fixedDeltaTime;
            // Calculate the new position
            Vector3 newCalculatedPosition = rb.position + positionChangeThisFrame;

            if (rb.isKinematic) // Only move if kinematic (as intended)
            {
                rb.MovePosition(newCalculatedPosition);
            }
            else
            {
                // If it's not kinematic, this log will appear.
                // It should be kinematic for this movement type to avoid external forces.
                Debug.LogWarning($"Meteorite ({gameObject.name}): Rigidbody is NOT Kinematic in FixedUpdate! Movement might be erratic or affected by physics.");
                // As a fallback if it's not kinematic for some reason, you might try:
                // rb.velocity = direction * currentSpeed; 
                // But the goal is for it to be kinematic.
            }
        }
    }

    private void OnDisable() 
    {
        if (model != null)
        {
            model.OnMeteoriteDestroyed -= HandleMeteoriteModelDestroyed;
        }
    }

    // Changed from OnTriggerEnter to OnCollisionEnter
    void OnCollisionEnter(Collision collision) 
    {
        if (isDestroyed) return;

        if (model == null)
        {
            Debug.LogError($"MeteoritePresenterFinal ({gameObject.name}): Model is null in OnCollisionEnter.", this);
            return;
        }

        // Player collision logic
        // Player collision logic
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"MeteoritePresenterFinal ({gameObject.name}): Collision with player.", this);
            PlayerPresenter playerPresenter = collision.gameObject.GetComponent<PlayerPresenter>();
            if (playerPresenter != null)
            {
                playerPresenter.ProcessDamage(model.DamageToPlayer);
            }
            else
            {
                Debug.LogError($"MeteoritePresenterFinal ({gameObject.name}): PlayerPresenter component not found on Player tagged object: {collision.gameObject.name}", collision.gameObject);
            }
            HandleDestruction();
            return;
        }

        // Bullet collision logic (Optional: Bullets now directly damage the model)
        // If the Bullet.cs script's OnCollisionEnter correctly calls model.TakeDamage(),
        // then this section in MeteoritePresenterFinal2 for "Bullet" tag might be redundant for damage dealing.
        // However, you might want to react to a bullet hit in the presenter for other reasons (e.g. specific sound/effect).
        // For now, we assume the bullet handles damaging. The OnMeteoriteDestroyed event will trigger HandleDestruction.
        // if (collision.gameObject.CompareTag("Bullet"))
        // {
        //     Debug.Log($"MeteoritePresenterFinal2 ({gameObject.name}): Collision with bullet. Damage handled by bullet script.");
        //     // Bullet script calls TakeDamage on MeteoriteModelFinal2.
        //     // If health drops to 0, OnMeteoriteDestroyed event will call HandleMeteoriteModelDestroyed -> HandleDestruction.
        // }
    }

    private void HandleMeteoriteModelDestroyed()
    {
        Debug.Log($"MeteoritePresenterFinal ({gameObject.name}): Received OnMeteoriteDestroyed event. Calling HandleDestruction.");
        HandleDestruction();
    }

    private void HandleDestruction()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        Debug.Log($"MeteoritePresenterFinal ({gameObject.name}): HandleDestruction called. BOOM!", this);

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            // Potentially disable the rigidbody to stop further physics interactions
            // rb.isKinematic = true; 
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // Disable collider to prevent further collisions
        }

        if (view != null)
        {
            view.PlayDestructionEffects();
        }
        else
        {
            Debug.LogWarning($"MeteoritePresenterFinal ({gameObject.name}): MeteoriteViewFinal not found, cannot play destruction effects.");
        }
        
        // Consider a delay if particles/sound need time to play before destruction
        // Destroy(gameObject, 2f); // Example: Destroy after 2 seconds
        Destroy(gameObject); 
    }
}