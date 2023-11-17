using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Golf Ball"))
        {
            other.gameObject.GetComponent<GolfBallManager>().OutOfBounds();
        }
    }
}
