using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float iceMoveSpeed = 8f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float iceSlipFactor = 3f;

    [Header("Physics: Air Resistance (Glide)")]
    [SerializeField] private float dragCoefficient = 10f; // ค่า k (สัมประสิทธิ์แรงต้านอากาศ) ปรับให้ร่อนช้าหรือเร็วได้ตรงนี้
    private bool isGliding; // เช็คว่ากำลังกดปุ่มร่อนอยู่ไหม

    [Header("Ground & Ice Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask iceLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isOnIce;
    private bool isSprinting;
    private Vector2 moveInput;

    [Header("Game State and UI")]
    public bool hasKey = false;
    [SerializeField] private GameUIManager uiManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("🚨 สคริปต์ PlayerController หา Rigidbody2D ไม่เจอครับ!");
    }

    void Update()
    {
        if (groundCheck == null) return;

        bool touchGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool touchIce = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, iceLayer);

        isGrounded = touchGround || touchIce;
        isOnIce = touchIce && !touchGround;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // --- 1. Walking And Sprinting ---
            float currentMoveSpeed = moveSpeed;
            float currentIceSpeed = iceMoveSpeed;

            if (isSprinting)
            {
                currentMoveSpeed *= sprintMultiplier;
                currentIceSpeed *= sprintMultiplier;
            }

            if (isOnIce)
            {
                float targetSpeedX = moveInput.x * currentIceSpeed;
                float smoothedX = Mathf.Lerp(rb.linearVelocity.x, targetSpeedX, Time.fixedDeltaTime * iceSlipFactor);
                rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
               
            }
            else
            {
                rb.linearVelocity = new Vector2(moveInput.x * currentMoveSpeed, rb.linearVelocity.y);
            }

            // --- 2. Air Resistance (Glide) ---
            if (rb.linearVelocity.y < 0 && isGliding && !isGrounded)
            {
                float velocityY = rb.linearVelocity.y;

                // คํานวณแรงต้านตามสูตร: F_drag = k * v^2
                // (ใช้ v*v ค่าจะออกมาเป็นบวกเสมอ ซึ่งถูกต้องเพราะแรงต้านต้องมีทิศทางชี้ขึ้น สวนกับความเร็วที่ตกลงมา)
                float dragForce = dragCoefficient * (velocityY * velocityY);

                rb.AddForce(new Vector2(0, dragForce));
            }
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnGlide(InputValue value)
    {
        isGliding = value.isPressed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            hasKey = true;
            Destroy(collision.gameObject);

            if (uiManager != null) uiManager.UpdateKeyStatus(true);
        }

        if (collision.CompareTag("Trap") || collision.CompareTag("Lava"))
        {
            Die();
        }

        if (collision.CompareTag("Win"))
        {
            if (hasKey) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            GameUIManager.ResetTimer();
        }
    }

    private void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}