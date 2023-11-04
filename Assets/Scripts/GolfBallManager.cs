using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GolfBallManager : MonoBehaviour
{
    private Vector3 mouseStarterPos, mousePos;
    private bool planningShot = false;
    private GameObject powerBar, triRotator, tri;
    private float barPercentage = 0;
    private Rigidbody golfBallRb;
    private float bounciness = .8f, friction = .01f;
    private Vector3 curVel = new Vector3(0, 0, 0), lastFrameVel = new Vector3(0, 0, 0);
    
    // Start is called before the first frame update
    void Start()
    {
        powerBar = GameObject.Find("PowerBar");
        powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
        powerBar.GetComponent<Image>().color = GetBarColor();
        triRotator = GameObject.Find("Tri Rotator");
        tri = GameObject.Find("Tri");
        tri.SetActive(false);
        golfBallRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //Keeps track of the golf ball's velocity for collisions
        lastFrameVel = curVel;
        curVel = golfBallRb.velocity;
        
        //Friction
        if (golfBallRb.velocity.magnitude > friction)
        {
            golfBallRb.velocity -= golfBallRb.velocity.normalized * friction;
        }
        else if (golfBallRb.velocity.magnitude != 0)
        {
            golfBallRb.velocity = Vector3.zero;
        }

        //Can't fire a shot if the golf ball is moving
        if (golfBallRb.velocity == new Vector3(0, 0, 0))
        {
            //If the player releases lmb, fire shot
            if (Input.GetMouseButtonUp(0))
            {
                planningShot = false;
                golfBallRb.AddForce(50 * barPercentage * (tri.transform.position - triRotator.transform.position),
                                    ForceMode.Impulse);
                tri.SetActive(false);
                powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
                barPercentage = 0;
                powerBar.GetComponent<Image>().color = GetBarColor();
            }

            if (planningShot)
            {
                mousePos = Input.mousePosition;
                Vector3 mouseDif = new Vector3(mousePos.x - mouseStarterPos.x, 0, mousePos.y - mouseStarterPos.y);
                if (mouseDif.magnitude == 0)
                {
                    return;
                }
                tri.SetActive(true);
                Vector3 triDif = tri.transform.position - triRotator.transform.position;
                triRotator.transform.rotation *= Quaternion.FromToRotation(triDif, -mouseDif);
                if (triRotator.transform.rotation.eulerAngles.x != 0 || triRotator.transform.rotation.eulerAngles.z != 0)
                {
                    triRotator.transform.rotation = Quaternion.Euler(0, triRotator.transform.rotation.eulerAngles.y, 0);
                }
                if (mouseDif.magnitude < 250)
                {
                    powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, mouseDif.magnitude * 700 / 250);
                    barPercentage = mouseDif.magnitude / 250;
                }
                else
                {
                    powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 700);
                    barPercentage = 1;
                }
                powerBar.GetComponent<Image>().color = GetBarColor();
            }

            //If the player clicks down lmb, begin planning shot
            if (Input.GetMouseButtonDown(0))
            {
                planningShot = true;
                mouseStarterPos = Input.mousePosition;
            }
        }
    }

    private Color GetBarColor()
    {
        return Color.red * Mathf.Sqrt(barPercentage) + Color.green * Mathf.Sqrt(1 - barPercentage);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bouncy"))
        {
            Physics.Raycast(transform.position, collision.transform.position - transform.position, out RaycastHit hitInfo);
            golfBallRb.velocity = lastFrameVel;
            golfBallRb.velocity -= 2 * Vector3.Dot(golfBallRb.velocity, hitInfo.normal) * hitInfo.normal;
            golfBallRb.velocity *= bounciness;
            curVel = golfBallRb.velocity;
        }
    }
}
