using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    private Transform boostDirT;
    
    // Start is called before the first frame update
    void Start()
    {
        boostDirT = gameObject.GetComponentsInChildren<Transform>()[1];
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Golf Ball"))
        {
            Debug.Log("Applying Force");
            other.gameObject.GetComponent<Rigidbody>().AddForce((boostDirT.position
                                                                - transform.position).normalized
                                                                * 10000 * Time.deltaTime,
                                                                ForceMode.Force);
            Debug.Log(other.gameObject.GetComponent<Rigidbody>().velocity);
        }
    }
}
