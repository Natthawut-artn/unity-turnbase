using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DGMTxt : MonoBehaviour
{
    private TextMeshPro DGMAmount;
    private Vector3 targetPos;
    private float moveSpeed = 7.5f;
    private float FPS = 40f;

    public void Play(Vector3 gameObjPos , string damage , float duration)
    {
        DGMAmount = GetComponent<TextMeshPro>();
        DGMAmount.SetText(damage);

        targetPos = transform.position + (5f * Vector3.up);
        StartCoroutine(Animation(duration));
    }

    IEnumerator Animation(float duration)
    {
        for (int i = 0; i < duration * FPS; i++)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            i++;
            yield return new WaitForSeconds(duration / FPS);
        }
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180f, 0); 
    }
}
