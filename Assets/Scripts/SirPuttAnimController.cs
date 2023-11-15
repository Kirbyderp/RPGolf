using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SirPuttAnimController : MonoBehaviour
{
    private Animator animController;

    // Start is called before the first frame update
    void Start()
    {
        animController = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(TestAnims());
    }

    IEnumerator TestAnims()
    {
        yield return new WaitForSeconds(5);
        animController.Play("Swing");
        yield return new WaitForSeconds(5);
        animController.Play("Jump");
    }
}
