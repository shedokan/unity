using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(0, 0, 2000);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
