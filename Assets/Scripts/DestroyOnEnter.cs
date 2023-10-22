using UnityEngine;

public class DestroyOnEnter : MonoBehaviour {
    // Sent when an incoming collider makes contact with this object's collider (2D physics only)
    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.CompareTag("Ball")) {
            BallGrid.current.pool.Release(collision.gameObject);
        }
        else {
            Destroy(collision.gameObject);
        }
    }
}