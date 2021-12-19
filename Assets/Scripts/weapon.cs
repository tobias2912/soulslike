using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour
{
    public int damage;
    private weaponhandler controller;

    // Start is called before the first frame update
    void Start()
    {
        controller=GetComponentInParent<weaponhandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        controller.onWeaponCollision(other, damage);

    }
}
