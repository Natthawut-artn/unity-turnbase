using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    Rigidbody rb;
    public string ConnectToRoom;
    public bool isOpen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Open()
    {
        if (!isOpen)
        {
            StartCoroutine(View());
            isOpen = true;
        }
    }

    IEnumerator View()
    {
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.velocity = Vector3.up * 2f;
        yield return new WaitForSeconds(2.5f);
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
    }
}