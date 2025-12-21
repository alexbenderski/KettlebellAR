using UnityEngine;
using System.Collections.Generic;

public class CircleTrail : MonoBehaviour
{
    public Camera cam;
    public GameObject circlePrefab; // ה-Prefab של העיגול
    public int poolSize = 20;       // כמה נקודות זנב אפשריות
    public float spawnDistance = 0.1f; 
    public float fadeTime = 0.3f;   

    private List<SpriteRenderer> pool = new List<SpriteRenderer>();
    private Vector3 lastPos;

    void Start()
    {
        // יצירת הבריכה
        for (int i = 0; i < poolSize; i++)
        {
            GameObject c = Instantiate(circlePrefab);
            c.SetActive(false);
            pool.Add(c.GetComponent<SpriteRenderer>());
        }

        lastPos = transform.position;
    }

   void Update()
{
    if (Input.touchCount == 0)
        return;

    Touch t = Input.GetTouch(0);

    // ממירים למסך → עולם
    Vector3 rawPos = cam.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, 1f));

    // ⭐ החלקה — זה מה שמעלים את הגאפים
    Vector3 pos = Vector3.Lerp(lastPos, rawPos, 0.5f);

    // מזיזים את האובייקט
    transform.position = pos;

    // בודקים מרחק לעיגול הבא
    float distance = Vector3.Distance(pos, lastPos);
    if (distance > spawnDistance)
    {
        SpawnCircle(pos);
        lastPos = pos;
    }
}

    void SpawnCircle(Vector3 pos)
    {
        SpriteRenderer circle = pool.Find(c => !c.gameObject.activeSelf);
        if (circle == null) return;

        circle.transform.position = pos;
        circle.color = new Color(1,1,1,1);
        circle.gameObject.SetActive(true);

        StartCoroutine(FadeCircle(circle));
    }

    System.Collections.IEnumerator FadeCircle(SpriteRenderer c)
    {
        float t = 0;
        Color startColor = c.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = 1 - (t / fadeTime);
            c.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        c.gameObject.SetActive(false);
    }
}
