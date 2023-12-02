using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    private Transform boostDirT;
    
    // Start is called before the first frame update
    void Start()
    {
        boostDirT = gameObject.GetComponentsInChildren<BoostDir>()[0].transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Golf Ball"))
        {
            other.gameObject.GetComponent<Rigidbody>().AddForce((boostDirT.position
                                                                - transform.position).normalized
                                                                * 5000 * Time.deltaTime,
                                                                ForceMode.Force);
        }
    }
}
