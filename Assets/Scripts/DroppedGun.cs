using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedGun : MonoBehaviour
{
    SpriteRenderer sprRenderer;
    public GunObj gun;
    void Start()
    {
        sprRenderer = GetComponent<SpriteRenderer>();
        sprRenderer.sprite = gun.gunSprite;
    }

    public void Change()
    {
        sprRenderer.sprite = gun.gunSprite;
    }
}
