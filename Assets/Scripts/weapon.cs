using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour
{
    public int damage;
    private ThirdPersonController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("player").GetComponent<ThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        print("crash");
        bool isStatic = other.gameObject.isStatic;
        if (isStatic)
        {
            controller.stagger();
        }

    }
}
