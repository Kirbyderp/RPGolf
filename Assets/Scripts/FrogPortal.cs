using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogPortal : MonoBehaviour
{
    private GameObject otherEnd;
    public int frogID;
    private bool portalIsActive = false, travelDelay = false;
    private Quaternion relativeRotation;
    public BoxCollider bodyCollider;

    // Start is called before the first frame update
    void Start()
    {
        FrogPortal[] allFrogs = GameObject.FindObjectsOfType<FrogPortal>();
        foreach (FrogPortal frog in allFrogs)
        {
            if (frog.GetID() == frogID && !frog.CompareTag(gameObject.tag))
            {
                otherEnd = frog.gameObject;
                relativeRotation = Quaternion.FromToRotation(-transform.right, -frog.transform.right);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetID()
    {
        return frogID;
    }

    public void ActivatePortal()
    {
        bodyCollider.enabled = false;
        portalIsActive = true;
    }

    public void DeactivatePortal()
    {
        portalIsActive = false;
        bodyCollider.enabled = true;
    }

    public void SetTravelDelay(bool input)
    {
        travelDelay = input;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Golf Ball") && portalIsActive && !travelDelay)
        {
            travelDelay = true;
            otherEnd.GetComponent<FrogPortal>().SetTravelDelay(true);
            Vector3 posRelToThis = other.transform.position - transform.position;
            Vector3 relativePos = relativeRotation * posRelToThis + otherEnd.transform.position;
            other.gameObject.GetComponent<GolfBallManager>().PortalTravel(relativePos, relativeRotation);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Golf Ball") && travelDelay)
        {
            travelDelay = false;
        }
    }
}
