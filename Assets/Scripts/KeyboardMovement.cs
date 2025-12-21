// using UnityEngine;

// public class KeyboardMovement : MonoBehaviour
// {
//     public float speed = 1.5f;
//     public float rotationSpeed = 90f;

//     void Update()
//     {
//         float moveX = Input.GetAxis("Horizontal");
//         float moveZ = Input.GetAxis("Vertical");

//         Vector3 move = new Vector3(moveX, 0, moveZ) * speed * Time.deltaTime;
//         transform.Translate(move, Space.Self);

//         if (Input.GetKey(KeyCode.Q)) transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
//         if (Input.GetKey(KeyCode.E)) transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
//     }
// }


using UnityEngine;

public class KeyboardMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        Vector3 move = new Vector3(h, 0, v);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.Self);
    }
}
