using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoomTriggerEvent : MonoBehaviour
{
    public GameObject EventPanel;
    public Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventPanel.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            EventPanel.SetActive(false);
        }
    }
    public void CloseEventPanel()
    {
        EventPanel.SetActive(false);
    }

    public void DisableThisEvent()
    {

        StartCoroutine(DoAnimationAndClose());
        GameManager.Instance.ReleasePlayers();

    }

    IEnumerator DoAnimationAndClose()
    {
        animator.SetTrigger("CloseEvent");
        yield return new WaitForSeconds(2.5f); // Assuming the animation duration is 1 second
        this.gameObject.SetActive(false);
    }
}
