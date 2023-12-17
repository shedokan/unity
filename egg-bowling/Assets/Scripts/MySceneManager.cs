using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MySceneManager : MonoBehaviour
{
    public Button restartSceneButton;
    // Start is called before the first frame update
    void Awake()
    {
        restartSceneButton.onClick.AddListener((() =>
        {
            Debug.Log("On CLick");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
