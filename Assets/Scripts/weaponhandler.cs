using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface weaponhandler
{
    //weapon calculates its damage, then the player can calculate further
    public void onWeaponCollision(Collider other, float damage);
}
