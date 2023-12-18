using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHoleBehavior : MonoBehaviour
{
    public string sceneToGoTo;
    public int currentHolePar;
    
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
            other.gameObject.GetComponent<GolfBallManager>().FinishHole(sceneToGoTo, currentHolePar);
        }
    }
}
