using UnityEngine;
using UnityEngine.InputSystem;

public class CubeRotate : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public float moveSpeed = 5f;

    void Update()
    {
        // Simple rotation regardless of input
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // // Movement with new Input System
        // if (Keyboard.current != null)
        // {
        //     Vector3 movement = Vector3.zero;

        //     // Check WASD or arrow keys
        //     if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        //         movement.z = 1;
        //     if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        //         movement.z = -1;
        //     if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        //         movement.x = -1;
        //     if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        //         movement.x = 1;

        //     // Apply movement
        //     transform.Translate(movement * moveSpeed * Time.deltaTime);
        // }
    }
}