using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float speed = 30f; // Speed of the fireball
    [SerializeField] private float damage = 10f; // Damage dealt by the fireball
    [SerializeField] private float lifetime = 5f; // Time before the fireball is destroyed if not hitting anything

    private Vector2 direction; // Direction in which the fireball will move

    private void Start()
    {
        // Start the fireball's lifetime countdown
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 fireballDirection)
    {
        direction = fireballDirection;
    }

    private void Update()
    {
        // Move the fireball
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the fireball hits the player
        if (collision.CompareTag("Player"))
        {
            // Assuming the Player has a script with a TakeDamage method
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            // Destroy the fireball after hitting the player
            Destroy(gameObject);
        }
    }
}
