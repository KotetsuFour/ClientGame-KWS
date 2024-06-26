using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Person : MonoBehaviour
{
    [SerializeField] private string generalName;
    [SerializeField] private int maxHealth;
    private int currentHealth;
    [SerializeField] private int attack;
    [SerializeField] private int initiative;
    [SerializeField] private int influenceZone;


    public abstract void react(GameNotification note);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
