// using UnityEngine;
// using UnityEngine.InputSystem; // ✅ חובה לתמיכה ב-Input System החדש

// public class VirtualHandController : MonoBehaviour
// {
//     public Transform cameraTransform;
//     public float moveRange = 0.2f;
//     public float moveSpeed = 3f;

//     private Vector3 initialOffset;
//     private Vector2 lastMousePos;

//     void Start()
//     {
//         if (cameraTransform == null)
//             cameraTransform = Camera.main.transform;

//         // מרחק קבוע מהמצלמה
//         initialOffset = cameraTransform.forward * 0.5f + cameraTransform.right * 0.15f;

//         // שמור את מיקום העכבר ההתחלתי
//         lastMousePos = Mouse.current.position.ReadValue();
//     }

//     void Update()
//     {
//         if (Mouse.current == null) return; // אם אין עכבר — צא

//         // חישוב שינוי במיקום העכבר
//         Vector2 currentMouse = Mouse.current.position.ReadValue();
//         Vector2 delta = currentMouse - lastMousePos;
//         lastMousePos = currentMouse;

//         float moveX = delta.x * 0.002f; // רגישות
//         float moveY = delta.y * 0.002f;

//         Vector3 offset = new Vector3(moveX, moveY, 0) * moveRange;

//         // מיקום יעד מול המצלמה
//         Vector3 targetPos = cameraTransform.position +
//                             cameraTransform.forward * 0.5f +
//                             cameraTransform.right * offset.x +
//                             cameraTransform.up * offset.y;

//         // מעבר חלק
//         transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
//     }
// }



using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualHandController : MonoBehaviour
{
    public Transform cameraTransform;
    public float moveRange = 0.2f;
    public float moveSpeed = 3f;

    private Vector3 initialLocalOffset;
    private Vector2 lastMousePos;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // נשמור מרחק קבוע מהמצלמה במרחב המקומי שלה
        initialLocalOffset = new Vector3(0.15f, -0.1f, 0.5f);

        // שמור את מיקום העכבר ההתחלתי
        lastMousePos = Mouse.current.position.ReadValue();
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // חישוב שינוי במיקום העכבר
        Vector2 currentMouse = Mouse.current.position.ReadValue();
        Vector2 delta = currentMouse - lastMousePos;
        lastMousePos = currentMouse;

        // תזוזה קטנה יחסית
        float moveX = delta.x * 0.002f;
        float moveY = delta.y * 0.002f;

        // יעד חדש במרחב המקומי של המצלמה
        Vector3 targetLocalPos = initialLocalOffset + new Vector3(moveX, moveY, 0) * moveRange;

        // מעבר חלק
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * moveSpeed);
        transform.localRotation = Quaternion.identity; // לא מסובבים את היד
    }
}
