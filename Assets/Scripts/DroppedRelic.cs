using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DroppedRelic : MonoBehaviour
{
    SpriteRenderer sprRenderer;
    public Relic relic;
    void Start()
    {
        sprRenderer = GetComponent<SpriteRenderer>();
        sprRenderer.sprite = relic.relicSprite;
    }

    public void Pick()
    {
        if (GameObject.Find("CurriculumChoice").activeSelf)
        {
            GameObject.Find("CurriculumChoice").SetActive(false);
        }
        Destroy(gameObject.GetComponentInParent<Transform>().gameObject);
    }
    public void Change()
    {
        sprRenderer.sprite = relic.relicSprite;
    }

    void Update()
    {
        float scale = 1f + 0.05f * Mathf.Sin(Time.time * 5f);
        this.transform.localScale = new Vector3(scale, scale, 1f);
    }

}
