using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
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
            if (gameObject.CompareTag("Fireball"))
            {
                other.gameObject.GetComponent<GolfBallManager>().FreeFireball();
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Air Ball"))
            {
                other.gameObject.GetComponent<GolfBallManager>().GainAirBall();
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Mega Bounce"))
            {
                other.gameObject.GetComponent<GolfBallManager>().GainMegaBounce();
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Tiny Ball"))
            {
                other.gameObject.GetComponent<GolfBallManager>().GainTinyBall();
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Ice Physics"))
            {
                other.gameObject.GetComponent<GolfBallManager>().GainIcePhysics();
                Destroy(gameObject);
            }
        }
    }
}
