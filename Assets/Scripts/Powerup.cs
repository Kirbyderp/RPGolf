using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    private bool appliedPowerup = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Golf Ball") && !appliedPowerup)
        {
            appliedPowerup = true;
            if (gameObject.CompareTag("Fireball"))
            {
                other.gameObject.GetComponent<GolfBallManager>().FreeFireball();
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Air Ball"))
            {
                other.gameObject.GetComponent<GolfBallManager>().GainAirBall();
                gameObject.GetComponent<Animator>().SetTrigger("Curse");
                StartCoroutine(DestroyTimer());
            }
            else if (gameObject.CompareTag("Mega Bounce"))
            {
                other.gameObject.GetComponent<GolfBallManager>().GainMegaBounce();
                gameObject.GetComponent<Animator>().SetTrigger("Curse");
                StartCoroutine(DestroyTimer());
            }
            else if (gameObject.CompareTag("Tiny Ball"))
            {
                other.gameObject.GetComponent<GolfBallManager>().GainTinyBall();
                gameObject.GetComponent<Animator>().SetTrigger("Curse");
                StartCoroutine(DestroyTimer());
            }
            else if (gameObject.CompareTag("Ice Physics"))
            {
                other.gameObject.GetComponent<GolfBallManager>().GainIcePhysics();
                gameObject.GetComponent<Animator>().SetTrigger("Curse");
                StartCoroutine(DestroyTimer());
            }
        }
    }
}
