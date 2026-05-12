using Unity.VisualScripting;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    public float patrolDistance = 3f;

    [Header("Wall Detection Settings")]
    public float wallCheckDistance = 0.2f;
    public Transform checkPoint;
    public LayerMask groundLayer;

    private float startX;
    private bool movingRight = true;

    void Start()
    {
        startX = transform.position.x;
    }
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        bool reachedDistanceLimit = false;

        if (movingRight && transform.position.x > startX + patrolDistance)
        {
            reachedDistanceLimit = true;
        }
        else if (!movingRight && transform.position.x < startX - patrolDistance)
        {
            reachedDistanceLimit = true;
        }

        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallInfo = Physics2D.Raycast(checkPoint.position, direction, wallCheckDistance, groundLayer);

        if (reachedDistanceLimit == true || wallInfo.collider != null)
        {
            Flip();
        }
        void Flip()
        {
            movingRight = !movingRight;
            transform.eulerAngles = new Vector3(0, movingRight ? 0 : 180, 0);
        }
    }

    private void OnDrawGizmos()
    {
        if (checkPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 wallDir = movingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(checkPoint.position, checkPoint.position + wallDir * wallCheckDistance);
        }
    }
}
