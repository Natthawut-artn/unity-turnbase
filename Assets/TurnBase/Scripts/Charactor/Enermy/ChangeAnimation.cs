using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAnimation : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeToWalk()
    {
        animator.SetBool("isWalking", true);
    }
    public void changeToIdle()
    {
        animator.SetBool("isWalking", false);
    }
    public void changeToAttack()
    {
        animator.SetTrigger("isAttack");
    }
}
