using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private GameObject camRotator, golfBall;
    private float speed = 120;
    KeyCode clockwise = KeyCode.A;
    KeyCode counterClockwise = KeyCode.D;


    // Start is called before the first frame update
    void Start()
    {
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
    }

    private void LateUpdate()
    {
        camRotator.transform.position = golfBall.transform.position;
    }
}
