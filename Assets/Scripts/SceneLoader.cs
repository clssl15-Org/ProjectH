using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string Scene;

    public void LoadScene()
    {
        SceneManager.LoadScene(Scene);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) 
        {
            LoadScene();
        }
    }
}
