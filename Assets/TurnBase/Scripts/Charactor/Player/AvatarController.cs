using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class AvatarController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private bool isWalking = false;
    private Animator animator;
    public LayerMask layerMask;

    private void Start()
    {
        // Get Animator from the same GameObject
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on Avatar! Please attach one.");
        }
    }

    private IEnumerator MoveTo(Vector3 hitPoint)
    {
        isWalking = true;
        animator?.SetBool("Walking", true);

        Vector3 targetPos = new Vector3(hitPoint.x, transform.position.y, hitPoint.z);

        // Optional: Rotate toward target
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isWalking = false;
        animator?.SetBool("Walking", false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isWalking && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity , layerMask))
            {
                StartCoroutine(MoveTo(hit.point));
            }
        }
    }
}
