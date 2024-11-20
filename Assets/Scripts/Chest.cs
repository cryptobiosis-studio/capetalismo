using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public bool opened;
    public GunObj[] guns;
    public GunObj itemA;
    public GunObj itemB;

    public GameObject DroppedGun;

    public Transform itemATrans;
    public Transform itemBTrans;

    GameObject _itemA;    
    GameObject _itemB;
    

    Animator chestAnim;

    void Start()
    {
        opened = false;
        chestAnim = transform.GetComponent<Animator>();
    }
    public void openChest(){
        
        if(!opened){
            itemA = guns[Random.Range(0, guns.Length)];
            itemB = guns[Random.Range(0, guns.Length)];
            
            chestAnim.SetBool("open", true);
            _itemA = Instantiate(DroppedGun, itemATrans.position, Quaternion.identity);
            _itemB = Instantiate(DroppedGun, itemBTrans.position, Quaternion.identity);
            _itemA.SetActive(false);
            _itemB.SetActive(false);
            _itemA.GetComponentInChildren<DroppedGun>().gun = itemA;
            _itemB.GetComponentInChildren<DroppedGun>().gun = itemB;
            _itemA.GetComponentInChildren<DroppedGun>().Change();
            _itemB.GetComponentInChildren<DroppedGun>().Change();

            opened = true;
        }
    }
    void Update()
    {
        AnimatorStateInfo stateInfo = chestAnim.GetCurrentAnimatorStateInfo(0);
        Debug.Log("Current Animation State: " + stateInfo.IsName("SafeOpened"));
        if(chestAnim.GetCurrentAnimatorStateInfo(0).IsName("SafeOpened")){
                Debug.Log("TargetAnim");
                _itemA.SetActive(true);
                _itemB.SetActive(true);
                GetComponent<Chest>().enabled = false;
        }
            
    }
}
