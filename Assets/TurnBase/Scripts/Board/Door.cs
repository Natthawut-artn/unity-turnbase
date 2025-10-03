using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    Rigidbody rigidbody;
    public string ConnectToRoom;
    public bool isOpen = false;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
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
        rigidbody.isKinematic = false;
        rigidbody.useGravity = false;
        rigidbody.velocity = Vector3.up * 2f;
        yield return new WaitForSeconds(2.5f);
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = true;
    }
}