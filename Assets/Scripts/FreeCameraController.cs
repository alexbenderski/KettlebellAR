using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lookSpeed = 2f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        // --- תזוזה עם מקשים ---
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        float moveY = 0f;

        if (Input.GetKey(KeyCode.E)) moveY += 1f;
        if (Input.GetKey(KeyCode.Q)) moveY -= 1f;

        Vector3 move = transform.right * moveX + transform.up * moveY + transform.forward * moveZ;
        transform.position += move * moveSpeed * Time.deltaTime;

        // --- סיבוב עם עכבר ---
        if (Input.GetMouseButton(1)) // רק כשמחזיקים את הכפתור הימני
        {
            rotationX += Input.GetAxis("Mouse X") * lookSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f);
            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }
    }
}
