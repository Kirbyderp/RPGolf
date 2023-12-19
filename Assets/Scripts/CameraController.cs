using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float maxCamDistance = 24.39924f, curCamDistance = 24.39924f;
    private GameObject camRotator, golfBall, mainCamera;
    private float speed = 120;
    KeyCode clockwise = KeyCode.D;
    KeyCode counterClockwise = KeyCode.A;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        camRotator = GameObject.Find("Camera Rotator");
        golfBall = GameObject.Find("Golf Ball");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(clockwise))
        {
            camRotator.transform.Rotate(new Vector3(0, 1, 0) * speed * Time.deltaTime, Space.Self);
        }
        else if (Input.GetKey(counterClockwise))
        {
            camRotator.transform.Rotate(new Vector3(0, -1, 0) * speed * Time.deltaTime, Space.Self);
        }
        if (Physics.Raycast(camRotator.transform.position, mainCamera.transform.position - camRotator.transform.position, out RaycastHit hitInfo, 24.4f))
        {
            if ((hitInfo.point - camRotator.transform.position).magnitude < maxCamDistance)
            {
                if ((hitInfo.point - camRotator.transform.position).magnitude > 1.15f)
                {
                    mainCamera.transform.localPosition *= (hitInfo.point - camRotator.transform.position).magnitude / curCamDistance;
                    curCamDistance = (hitInfo.point - camRotator.transform.position).magnitude;
                }
                else if (curCamDistance != 1.15f)
                {
                    mainCamera.transform.localPosition *= 1.15f / curCamDistance;
                    curCamDistance = 1.15f;
                }
            }
        }
        else if (curCamDistance < maxCamDistance)
        {
            mainCamera.transform.localPosition *= maxCamDistance / curCamDistance;
            curCamDistance = maxCamDistance;
        }
    }

    private void LateUpdate()
    {
        camRotator.transform.position = golfBall.transform.position;
    }
}
