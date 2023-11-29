using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TargetHoleBehavior : MonoBehaviour
{
    public string sceneToGoTo;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Golf Ball"))
        {
            
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToGoTo, LoadSceneMode.Single);
    }
}
