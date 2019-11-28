using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    float mouseCamControl = 5;

    [SerializeField]
    float cameraSpeed = 5;

    Transform player;
    Vector3 camTarget;

    Vector3 mousePos;

    void Start()
    {
        player = Player.script.transform;
    }

    Vector3 camGoTo;
    private void FixedUpdate()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        camTarget = player.position;

        camTarget += (mousePos - camTarget).normalized * mouseCamControl;

        camGoTo = Vector3.Lerp(transform.position, camTarget, Time.deltaTime * cameraSpeed);
        camGoTo.z = -10;
        transform.position = camGoTo;

    }
}
