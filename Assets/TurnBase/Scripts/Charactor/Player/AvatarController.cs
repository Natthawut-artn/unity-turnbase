using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AvatarController : MonoBehaviour
{
    private bool isWalking = false;
    IEnumerator MoveTo(Vector3 hitPoint)
    {
        isWalking = true;
        Vector3 targetPos = new Vector3(hitPoint.x, transform.position.y , hitPoint.z);
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime);
            yield return null;
        }
        isWalking = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isWalking && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                StartCoroutine(MoveTo(hit.point));
            }
        }
    }
}
