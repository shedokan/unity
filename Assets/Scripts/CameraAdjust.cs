using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAdjust : MonoBehaviour
{
    public Camera cameraToTrack;
    
    // Start is called before the first frame update
    void Start()
    {
        // transform = GetComponent<Transform>();
        print(cameraToTrack.rect);
        // print(transform.scale);
        // transform.localScale = camera.rect;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
