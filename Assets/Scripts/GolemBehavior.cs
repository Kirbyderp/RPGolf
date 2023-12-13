using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemBehavior : MonoBehaviour
{
    public GolemHands[] hands;

    // Start is called before the first frame update
    void Start()
    {
        hands = GetComponentsInChildren<GolemHands>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GolemHands hand in hands)
        {
            hand.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    public void StartSlam()
    {
        foreach (GolemHands hand in hands)
        {
            hand.gameObject.tag = "Damage";
        }
    }

    public void EndSlam()
    {
        foreach (GolemHands hand in hands)
        {
            hand.gameObject.tag = "Bouncy";
        }
    }
}
