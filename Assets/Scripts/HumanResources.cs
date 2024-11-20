using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HumanResources : MonoBehaviour
{
    public Transform spawn;
    public GameObject DroppedRelic;
    public Relic[] relics;

    public TextMeshProUGUI relic1Title;
    public TextMeshProUGUI relic1Description;

    public GameObject relic;

    void Start()
    {
        
    }

    public void Interacted(){
        relic = Instantiate(DroppedRelic, spawn.position, Quaternion.identity);
        relic.GetComponentInChildren<DroppedRelic>().relic = relics[Random.Range(0, relics.Length)];
        relic1Title = GameObject.Find("TextChoice1").GetComponent<TextMeshProUGUI>();
        relic1Description = GameObject.Find("TextChoice1Description").GetComponent<TextMeshProUGUI>();
        relic1Title.text = relic.GetComponentInChildren<DroppedRelic>().relic.relicName;
        relic1Description.text = relic.GetComponentInChildren<DroppedRelic>().relic.description;
        this.enabled = false;
        relic.GetComponentInChildren<DroppedRelic>().Change();
        
    }
}
