using UnityEngine;

public class HandTouch : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float fadeDuration = 0.5f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnergyBall"))
        {
            StartCoroutine(FadeAndExplode(other.gameObject));
        }
    }

    private System.Collections.IEnumerator FadeAndExplode(GameObject ball)
    {
        Renderer renderer = ball.GetComponent<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        // ננסה לשנות את _BaseColor אם קיים
        Color startColor = Color.black;
        Color endColor = Color.yellow;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Color newColor = Color.Lerp(startColor, endColor, elapsed / fadeDuration);

            block.SetColor("_BaseColor", newColor); // <- שינוי חשוב
            renderer.SetPropertyBlock(block);

            yield return null;
        }

        // פיצוץ
        if (explosionPrefab)
            Instantiate(explosionPrefab, ball.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.1f);
        Destroy(ball);
    }
}
