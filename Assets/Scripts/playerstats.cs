using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerstats
{
    public int level = 1;
    public float hp = 100;
    public float maxhp=100;

    internal void damage(float hp)
    {
        this.hp-=hp;
    }
}
