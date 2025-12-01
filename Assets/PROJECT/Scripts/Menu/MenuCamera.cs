using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    public Camera cam;
    float xRotation, yRotation;
    public Transform orientation;
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * 5f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 8f;


        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -10f, 10f);
        yRotation = Mathf.Clamp(yRotation, -15f, 15f);




        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        //orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
