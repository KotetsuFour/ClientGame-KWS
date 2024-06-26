using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLCGeneral : Person
{
    [SerializeField] private int[] triggers;
    [SerializeField] private string[] code;
    private int codeIdx;


    public override void react(GameNotification note)
    {
        for (codeIdx = triggers[(int)note.getNature()]; codeIdx < code.Length; codeIdx++)
        {
            processLine();
        }
    }

    private void processLine()
    {
        //TODO
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
