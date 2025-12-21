using UnityEngine;

public class GlobalTouchAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip tapSound;

    private Vector2 touchStart;
    private float swipeThreshold = 80f; // לא לבלבל בין סווייפ לטאפ

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            touchStart = t.position;
        }

        if (t.phase == TouchPhase.Ended)
        {
            float dx = Mathf.Abs(t.position.x - touchStart.x);
            float dy = Mathf.Abs(t.position.y - touchStart.y);

            // אם הייתה תנועה גדולה — זה סווייפ, לא טאפ
            if (dx > swipeThreshold || dy > swipeThreshold)
                return;

            // אחרת — TAP אמיתי
            PlayTapSound();
        }
    }

    void PlayTapSound()
    {
        if (audioSource != null && tapSound != null)
            audioSource.PlayOneShot(tapSound);
    }
}
