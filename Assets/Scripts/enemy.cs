using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemy : MonoBehaviour, weaponhandler
{
    private NavMeshAgent agent;
    private GameObject player;
    private Animator animator;
    [SerializeField]
    GameObject hpbar;
    [SerializeField]
    private float maxhp;
    private float hp;
    private Vector3 previousPosition;
    public float curSpeed;
    private GameObject rightHandObject;

    public void onWeaponCollision(Collider other, float damage)
    {
        if (other.gameObject.name == "player")
        {
            other.gameObject.GetComponent<ThirdPersonController>().damage(damage);
            print("enemy damages player for " + damage);
        }
    }
    public void damage(float damage)
    {
        hp -= damage;
        hpbar.GetComponent<healthbar>().UpdateHealthBar(hp, maxhp);
        animator.SetTrigger("stagger");
        print("enemy damaged");
        if (hp < 0)
        {
            die();
        }
    }

    private void die()
    {
        GameObject.Destroy(rightHandObject, 1f);
        agent.enabled = false;
        animator.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        createRBskeleton();
    }

    private void createRBskeleton()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("player");
        animator = GetComponent<Animator>();
        hp = maxhp;
    }
    private void speedTracker()
    {
        Vector3 curMove = transform.position - previousPosition;
        curSpeed = curMove.magnitude / Time.deltaTime;
        previousPosition = transform.position;
        animator.SetFloat("speed", curSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        speedTracker();
        if (Vector3.Distance(transform.position, player.transform.position) < 2f)
        {
            agent.destination = transform.position;
            animator.SetBool("LightAttack", true);
        }
        else
        {
            animator.SetBool("LightAttack", false);
            agent.destination = player.transform.position;
        }
    }
}
