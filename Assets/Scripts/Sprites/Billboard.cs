using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool rotateWithOrientation = false;
    public Transform orientation;
    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //transform.LookAt(camera.transform);

        /*transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position);

        if (lockY)
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);*/

        if (rotateWithOrientation)
            transform.rotation = Quaternion.Euler(90, orientation.rotation.eulerAngles.y, 0);
        else
            transform.rotation = camera.transform.rotation;
    }
}
