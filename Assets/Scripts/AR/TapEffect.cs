using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TapEffect : MonoBehaviour
{
    public Image effectImage;
    public float animationTime = 0.25f;

    private void Awake()
    {
        effectImage.color = new Color(1,1,1,0); // מתחיל שקוף
    }

    public void PlayEffect(Vector2 screenPos)
    {
        // הצבה במסך
        transform.position = screenPos;

        // הפעלה מחדש
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float t = 0f;

        while (t < animationTime)
        {
            t += Time.deltaTime;
            float progress = t / animationTime;

            // Scale: קטן → גדול
            float scale = Mathf.Lerp(0.2f, 1.1f, progress);
            transform.localScale = new Vector3(scale, scale, 1f);

            // Alpha: 1 → 0
            float alpha = 1f - progress;
            effectImage.color = new Color(1,1,1,alpha);

            yield return null;
        }

        // בסוף נעלם
        effectImage.color = new Color(1,1,1,0);
    }
}
