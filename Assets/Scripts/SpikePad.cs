using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikePad : MonoBehaviour
{
    private bool spikesAreLowered = true;
    private float waitTime = 0;
    private Transform spikeChild;
    
    // Start is called before the first frame update
    void Start()
    {
        spikeChild = gameObject.GetComponentInChildren<SpikeChild>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Golf Ball"))
        {
            waitTime += Time.deltaTime;
            if (waitTime >= .4f && spikesAreLowered)
            {
                StartCoroutine(RaiseSpikes());
                spikesAreLowered = false;
                waitTime = 0;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        waitTime = 0;
    }

    IEnumerator RaiseSpikes()
    {
        for (int frameNum = 0; frameNum < 7; frameNum++)
        {
            spikeChild.localPosition = new Vector3 (spikeChild.localPosition.x, spikeChild.localPosition.y + .3f/7, spikeChild.localPosition.z);
            yield return new WaitForSeconds(1 / 60f);
        }
        spikeChild.localPosition = new Vector3(0, -.7f, 0);
        yield return new WaitForSeconds(2);
        StartCoroutine(LowerSpikes());
    }

    IEnumerator LowerSpikes()
    {
        for (int frameNum = 0; frameNum < 60; frameNum++)
        {
            spikeChild.localPosition = new Vector3(spikeChild.localPosition.x, spikeChild.localPosition.y - .3f/60f, spikeChild.localPosition.z);
            yield return new WaitForSeconds(1 / 60f);
        }
        spikeChild.localPosition = new Vector3(0, -1, 0);
        spikesAreLowered = true;
    }
}
