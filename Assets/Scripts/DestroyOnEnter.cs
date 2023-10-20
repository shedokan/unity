using UnityEngine;

public class DestroyOnEnter : MonoBehaviour {
    // Sent when an incoming collider makes contact with this object's collider (2D physics only)
    private void OnCollisionEnter2D(Collision2D collision) {
        // Debug.Log("Collision");
        // if (collision.gameObject.CompareTag("Enemy"))
        // {
        // collision.gameObject.SendMessage("ApplyDamage", 10);
        // }
        Destroy(collision.gameObject);
    }
}