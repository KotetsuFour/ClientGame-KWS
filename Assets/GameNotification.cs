using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNotification
{
    private Nature nature;

    public GameNotification(Nature nature)
    {
        this.nature = nature;
    }
    public Nature getNature()
    {
        return nature;
    }
    public enum Nature
    {

    }
}
