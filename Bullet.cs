using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 1; // Damage the bullet deals

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        StartCoroutine(ReturnToPoolAfterDelay(lifeTime));
    }

    public void Move(Vector3 direction)
    {
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
        else
        {
            Debug.LogError("Rigidbody component not found on the bullet.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Bullet collided with {collision.gameObject.name} (tag: {collision.gameObject.tag})", this);

        // Try to get the MeteoriteModelFinal component from the collided object
        MeteoriteModelFinal meteoriteModel = collision.gameObject.GetComponent<MeteoriteModelFinal>();
        if (meteoriteModel != null)
        {
            Debug.Log($"Bullet dealing {damage} damage to {collision.gameObject.name}", this);
            meteoriteModel.TakeDamage(damage);
        }
        // Example for other destructibles:
        // else if (collision.gameObject.CompareTag("EnemyShield"))
        // {
        //     EnemyShield shield = collision.gameObject.GetComponent<EnemyShield>();
        //     if (shield != null) shield.TakeDamage(damage);
        // }
        
        // Always return the bullet to the pool after any collision.
        ReturnToPool();
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        // For now, just disable the GameObject.
        // Later, this will call the PoolManager to disable the bullet.
        gameObject.SetActive(false);
        Debug.Log("Bullet returned to pool.");
    }
}
