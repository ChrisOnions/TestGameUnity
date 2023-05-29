using UnityEngine;

public class BulletDeleteAfterFire : MonoBehaviour
{
    [Header("Bullet Parameters")]
    [SerializeField] private float destroyBulletOnExit = 2f;
    [SerializeField] private int damageAmount = 10; // Amount of damage the bullet causes

    private void Start()
    {
        Destroy(gameObject, destroyBulletOnExit);

    }

    private void OnCollisionEnter(Collision collision)
    {   
        
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (collision.gameObject.CompareTag("Ground"))
        {
        Destroy(gameObject); 
        
        return;
        }
        if (enemy != null)
        {
            enemy.TakeDamage(damageAmount);

            Destroy(gameObject);
        }
        Destroy(gameObject);
        
    }
}
