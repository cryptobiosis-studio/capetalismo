using UnityEngine;
using TMPro;
using System.Collections;

public class FloorUIController : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float fadeDuration = 0.05f;
    public float visibleDuration = 0.05f;

    void Start()
    {
        ShowFloorLevel();
    }

    public void ShowFloorLevel()
    {
        int floorLevel = GameManager.Instance.floorLevel;
        text.text = "Floor " + floorLevel;
        StartCoroutine(FadeTextCoroutine());
    }

    IEnumerator FadeTextCoroutine()
    {
        Color originalColor = text.color;

        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // Fade In
        for (float t = 0; t < fadeDuration; t += Time.deltaTime * 2)
        {
            float alpha = t / fadeDuration;
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        yield return new WaitForSeconds(visibleDuration);

        // Fade Out
        for (float t = fadeDuration; t > 0; t -= Time.deltaTime * 2)
        {
            float alpha = t / fadeDuration;
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        text.gameObject.SetActive(false);
    }
}
