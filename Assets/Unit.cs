using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private int recruitCost;
    [SerializeField] private string unitName;

    [SerializeField] private int numSoldiers;
    [SerializeField] private int movement;
    [SerializeField] private int stamina;
    [SerializeField] private int morale;
    [SerializeField] private int initiative;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string getUnitName()
    {
        return unitName;
    }
    public int getRecruitmentCost()
    {
        return recruitCost;
    }
}
