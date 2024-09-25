using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFoward : MonoBehaviour
{
    float speed =  20f;
    [SerializeField]
    public Rigidbody2D rbcirculo;
    public Transform playerPosition;

    void Start()
    {
        rbcirculo = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {   
        bolinha();
    }
    void bolinha(){
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        if(x != 0 || y!=0){
            rbcirculo.velocity = new Vector2(x * speed, y * speed);
        }else{
            rbcirculo.velocity = Vector2.zero;
        }

        if(Input.GetKeyDown(KeyCode.Backspace)){
            this.gameObject.SetActive(false);
        }

        
        
    }
}
