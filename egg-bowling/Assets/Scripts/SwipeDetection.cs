using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.Controls;


struct SwipeContext
{
    public Vector2 delta;
    public float swipeTime;
}

public class SwipeDetection : MonoBehaviour
{
    public float minimumDistance = .05f;
    [Tooltip("Maximum time in Seconds")]
    public float maximumTime = 1;
    
    [Range(0, 1)]
    public float verticalThreshold = 0.9f;
    // [Range(0, 1)]
    // public float horizontalThreshold = 0.9f;

    public Rigidbody thingToPush;
    public float forceMultiplier = 400f;
    
    // Trail
    public GameObject trail;
    private Coroutine _trailCoroutine;
    
    
    private InputManager _inputManager;
    private Vector2 _startPos, _endPos;
    private float _startTime, _endTime; // in seconds


    private void Awake()
    {
        _inputManager = InputManager.Instance;
    }

    private void OnEnable()
    {
        _inputManager.OnStartTouch += SwipeStart;
        _inputManager.OnEndTouch += SwipeEnd;
    }

    private void OnDisable()
    {
        _inputManager.OnStartTouch -= SwipeStart;
        _inputManager.OnEndTouch -= SwipeEnd;
    }

    private void SwipeStart(Vector2 position, float time)
    {
        _startPos = position;
        _startTime = time;
        
        trail.SetActive(true);
        trail.transform.position = position;

        _trailCoroutine = StartCoroutine(Trail());
    }

    private IEnumerator Trail()
    {
        while (true)
        {
            trail.transform.position = _inputManager.PrimaryPosition();
            // yield return new WaitForSeconds(3f);
            yield return null;
        }
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        trail.SetActive(false);
        StopCoroutine(_trailCoroutine);
        
        _endPos = position;
        _endTime = time;
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        var dist = Vector2.Distance(_startPos, _endPos);
        var swipeTime = _endTime - _startTime;
        // Debug.LogFormat("_startPos: {0}, _endPos: {1}", _startPos, _endPos);
        if (dist < minimumDistance || swipeTime > maximumTime) return;

        Debug.DrawLine(_startPos, _endPos, Color.red, 10);
        var delta = new Vector2(_endPos.x - _startPos.x, _endPos.y - _startPos.y);
        OnSwipe(new SwipeContext {
            delta = delta,
            swipeTime = swipeTime
        });
        // else
        // {
        //     Debug.LogFormat("dist: {0}", dist);
        // }
    }

    private void OnSwipe(SwipeContext swipe)
    {
        var normalDirection = swipe.delta.normalized;
        // Debug.Log(Vector2.Dot(Vector2.up, normalDirection));
        if (Vector2.Dot(Vector2.down, normalDirection) > verticalThreshold)
        {
            var distance = swipe.delta.magnitude;
            Debug.LogFormat("Swipe Up: {0}, {1}", distance, swipe.delta);
            // thingToPush.velocity = swipe.delta * forceMultiplier;
            thingToPush.AddForce(normalDirection * forceMultiplier / swipe.swipeTime);
            Debug.DrawLine(thingToPush.position, thingToPush.position + (Vector3)swipe.delta, Color.yellow, 5f, false);
        } /*else if (Vector2.Dot(Vector2.up, normalDirection) > verticalThreshold)
        {
            Debug.Log("Swipe Down");
        }*/
        // if (Vector2.Dot(Vector2.left, normalDirection) > horizontalThreshold)
        // {
        //     Debug.Log("Swipe Left");
        // } else if (Vector2.Dot(Vector2.right, normalDirection) > horizontalThreshold)
        // {
        //     Debug.Log("Swipe Right");
        // }
    }
}
