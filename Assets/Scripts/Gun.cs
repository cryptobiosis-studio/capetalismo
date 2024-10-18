using System;
using JetBrains.Annotations;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class LookAt2D_v2 : MonoBehaviour
{
    public float offset;
    public SpriteRenderer sprRen;
    void Start(){
        sprRen = GetComponent<SpriteRenderer>();
        sprRen.flipX = true; // a bundinha da arma tava olhando pro mouse e não a ponta dela ent fiz essa gambiarra (boa sorte kayk, arruma ai pls).
    }

    void Update () {
		//Weapon Rotation based on mouse position on screen.
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 5.23f;
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg; // random goofy aah math
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);

        if (rotZ < 89 && rotZ > -89) //Check rotation and flips gun sprite, so it doesn't stay upside down.
        {
            sprRen.flipY = false;
        }
        else
        {
            sprRen.flipY = true;
        }
    }
}