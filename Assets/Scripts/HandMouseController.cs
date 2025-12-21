using UnityEngine;

public class HandMouseController : MonoBehaviour
{
    public float sensitivity = 2f;
    public float moveRange = 0.3f;

    private Vector3 startLocalPos;
    private Vector2 currentOffset = Vector2.zero;

    void Start()
    {
        startLocalPos = transform.localPosition;
        Cursor.lockState = CursorLockMode.None; // מאפשר להזיז עכבר חופשי
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        currentOffset.x = Mathf.Clamp(currentOffset.x + mouseX * Time.deltaTime, -moveRange, moveRange);
        currentOffset.y = Mathf.Clamp(currentOffset.y + mouseY * Time.deltaTime, -moveRange, moveRange);

        transform.localPosition = startLocalPos + new Vector3(currentOffset.x, -currentOffset.y, 0);
    }
}
