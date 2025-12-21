using UnityEngine;

public class FreeFlyCamera : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lookSpeed = 2f;

    float yaw = 0f;
    float pitch = 0f;

    void Update()
    {
        // תנועה במקלדת
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        float up = 0f;

        if (Input.GetKey(KeyCode.Space)) up += 1;     // למעלה
        if (Input.GetKey(KeyCode.LeftShift)) up -= 1; // למטה

        Vector3 move = new Vector3(h, up, v) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.Self);

        // סיבוב עם עכבר
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}
