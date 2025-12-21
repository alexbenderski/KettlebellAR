using UnityEngine;

public class FingerTrailFollow : MonoBehaviour
{
    public Camera cam;                 // Main Camera
    public Transform image;            // הילד עם ה-Sprite (העיגול)
    public TrailRenderer trail;        // הילד עם ה-Trail Renderer

    private bool isTouching = false;

    void Update()
    {
        if (Input.touchCount == 0)
        {
            if (isTouching)
            {
                isTouching = false;
                trail.emitting = false;
                image.gameObject.SetActive(false);
            }
            return;
        }

        Touch t = Input.GetTouch(0);

        Vector3 pos = cam.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, 0.5f));
        pos.z = 0;

        transform.position = pos;

        if (t.phase == TouchPhase.Began)
        {
            isTouching = true;
            trail.Clear();
            trail.emitting = true;
            image.gameObject.SetActive(true);
        }

        if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
        {
            image.gameObject.SetActive(true);
        }

        if (t.phase == TouchPhase.Ended)
        {
            isTouching = false;
            trail.emitting = false;
            image.gameObject.SetActive(false);
        }
    }
}