using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityHor = 5f;
    public float sensitivityVert = 5f;

    public float minmumVert = -45f;
    public float maxmumVert = 45f;

    private float _rotationX = 0;

    public Camera Cam;
    private float ScrollSpeed = 10f;
    private Vector3 CameraPosition;
    public float CamSpeed = 0.1f;

    [SerializeField]
    private float input_V;
    private float input_H;
    private float angle_Sum;

    void Start()
    {
        CameraPosition = this.transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (axes == RotationAxes.MouseX)
            {
                transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityHor * 0.1f, 0);
            }
            else if (axes == RotationAxes.MouseY)
            {
                _rotationX = _rotationX - Input.GetAxis("Mouse Y") * sensitivityVert * 0.1f;
                _rotationX = Mathf.Clamp(_rotationX, minmumVert, maxmumVert);

                float rotationY = transform.localEulerAngles.y;

                transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
            }
            else
            {
                _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
                _rotationX = Mathf.Clamp(_rotationX, minmumVert, maxmumVert);

                float delta = Input.GetAxis("Mouse X") * sensitivityHor;
                float rotationY = transform.localEulerAngles.y + delta;

                transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
            }
        }

        Cam.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            input_V = Input.GetAxis("Vertical");
            input_H = Input.GetAxis("Horizontal");

            Vector3 move = transform.right * input_H + transform.forward * input_V;
            move.y = 0.0f;

            transform.position += move * CamSpeed * Time.deltaTime * 25;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 move = new Vector3(0, 2.5f, 0);
            if (transform.position.y < 13f)
            {
                transform.position += move * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 move = new Vector3(0, 2.5f, 0);
            if (transform.position.y > 0.1f)
            {
                transform.position -= move * Time.deltaTime;
            }
        }
    }
}
