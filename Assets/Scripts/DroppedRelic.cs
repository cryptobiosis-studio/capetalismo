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
        GameObject.Find("CurriculumChoice").SetActive(false);
        Destroy(gameObject.GetComponentInParent<Transform>().gameObject);
    }
    public void Change(){
        sprRenderer.sprite = relic.relicSprite;
    }
    
}
