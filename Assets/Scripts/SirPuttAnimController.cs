using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SirPuttAnimController : MonoBehaviour
{
    private GolfBallManager golfBallManager;
    private Animator animController;
    private GameObject triRotator, triRotatorCopy;
    private float animFrameNum = 0;
    private float animSpeed = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        golfBallManager = GameObject.Find("Golf Ball").GetComponent<GolfBallManager>();
        animController = GetComponent<Animator>();
        triRotator = GameObject.Find("Tri Rotator");
        triRotatorCopy = GameObject.Find("Tri Rotator Copy");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Swing()
    {
        animController.SetTrigger("Swing Trigger");
    }

    public void Jump()
    {
        animController.SetTrigger("Jump Trigger");
        StartCoroutine(WaitForJump());
    }

    IEnumerator WaitForJump()
    {
        yield return new WaitForSeconds((25 / animSpeed) / 60f);
        InvokeRepeating("JumpMovement", 0, 1 / (60 * animSpeed));
    }

    public void JumpMovement()
    {
        animFrameNum++;
        triRotatorCopy.transform.position += Vector3.up * 30/95f;
        if (animFrameNum == 95)
        {
            CancelInvoke();
            animFrameNum = 0;
            triRotatorCopy.transform.position = new Vector3(triRotator.transform.position.x,
                                                            triRotator.transform.position.y + 30,
                                                            triRotator.transform.position.z);
            animController.SetTrigger("Land Trigger");
            StartCoroutine(WaitForLand());
        }
    }

    IEnumerator WaitForLand()
    {
        float curRotation = triRotatorCopy.transform.rotation.eulerAngles.y;
        golfBallManager.ResetSirPuttsalot();
        if (!golfBallManager.CanRotateAtDegree30((int)(((curRotation + 15) % 360) / 30)))
        {
            for (int deg30 = 0; deg30 < 12; deg30++)
            {
                if (golfBallManager.CanRotateAtDegree30(deg30))
                {
                    triRotator.transform.rotation = Quaternion.Euler(triRotator.transform.rotation.eulerAngles.x,
                                                                     deg30 * 30,
                                                                     triRotator.transform.rotation.eulerAngles.z);
                    triRotatorCopy.transform.rotation = Quaternion.Euler(triRotatorCopy.transform.rotation.eulerAngles.x,
                                                                         deg30 * 30,
                                                                         triRotatorCopy.transform.rotation.eulerAngles.z);

                    break;
                }
            }
        }
        yield return new WaitForSeconds((27 / animSpeed) / 60f);
        InvokeRepeating("LandMovement", 0, 1 / (60 * animSpeed));
    }

    public void LandMovement()
    {
        animFrameNum++;
        triRotatorCopy.transform.position += Vector3.down * 30 / 95f;
        if (animFrameNum == 95)
        {
            CancelInvoke();
            animFrameNum = 0;
            triRotatorCopy.transform.position = triRotator.transform.position;
            golfBallManager.SetBallInAnim(false);
        }
    }
}
