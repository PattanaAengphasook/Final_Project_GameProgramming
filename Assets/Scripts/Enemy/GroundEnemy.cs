using System.Dynamic;
using UnityEngine;

public class GroundEnemy : MonoBehaviour
{
    public float speed = 2f;

    [Header("Detection Settings")]
    public float edgeCheckDistance = 0.5f;
    public float wallCheckDistance = 0.2f;
    public Transform checkPoint;
    public LayerMask groundLayer;

    private bool movingRight = true;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb.linearVelocity = new Vector2((movingRight ? speed : -speed), rb.linearVelocity.y);

        RaycastHit2D groundInfo = Physics2D.Raycast(checkPoint.position, Vector2.down, edgeCheckDistance, groundLayer);

        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallInfo = Physics2D.Raycast(checkPoint.position, direction, wallCheckDistance, groundLayer);

        if (groundInfo.collider == false || wallInfo.collider == true)
        {
            Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.eulerAngles = new Vector3(0, movingRight ? 0 : 180, 0);
    }

    private void OnDrawGizmos()
    {
        if (checkPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(checkPoint.position, checkPoint.position + Vector3.down * edgeCheckDistance);
            Vector3 wallDir = movingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(checkPoint.position, checkPoint.position + wallDir * wallCheckDistance);
        }
    }
}
