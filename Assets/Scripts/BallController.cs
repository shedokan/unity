using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {
    private bool moving = false;
    private TargetJoint2D _targetJoint2D;
    
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
    
    void AfterShot() {
        // TODO: Find ball grid and round
        // _ballGrid = UnityObjectUtil.FindObjectFromInstanceID<BallGrid>(ballGridID);
        
        _targetJoint2D = GetComponent<TargetJoint2D>();
        if(_targetJoint2D) {
            _targetJoint2D.enabled = false;
        }

        moving = true;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(moving && collision.collider.CompareTag("Ball")) {
            // TODO: Check if collided with wall
            // collision.otherCollider.CompareTag("Hitable")
            Debug.Log($"OnCollisionEnter2D, collider tag: {collision.collider}");
            // foreach (ContactPoint contact in collision.contacts)
            // {
            //     Debug.DrawLine(contact.point, contact.point + contact.normal * 100, Color.white);
            // }

            
            if(_targetJoint2D) {
                _targetJoint2D.autoConfigureTarget = false;
                _targetJoint2D.target = BallGrid.Current.RoundToNearestGrid(transform.position);
                // TODO: Add to grid
                _targetJoint2D.enabled = true;
            }
            // Destroy(gameObject);
            moving = false;
            
            
            // var colliderBallController = collision.collider.gameObject.GetComponent<BallController>();
            // if(colliderBallController.moving) return;
            // // Falling
            // collision.collider.attachedRigidbody.gravityScale = 1;
            // collision.collider.gameObject.GetComponent<TargetJoint2D>().enabled = false;
            // collision.collider.gameObject.GetComponent<BallController>().moving = true;
            //
            // // Us
            // collision.otherCollider.attachedRigidbody.gravityScale = 1;
        }

        // if (collision.relativeVelocity.magnitude > 2)
        //     audioSource.Play();
    }
}