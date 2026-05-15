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
    [SerializeField] private float iceSlipFactor = 0.5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float doubleTapTime = 0.3f;

    [Header("Physics: Air Resistance (Glide)")]
    [SerializeField] private float dragCoefficient = 10f;

    [Header("Ground & Ice Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask iceLayer;

    [Header("Game State and UI")]
    public bool hasKey = false;
    [SerializeField] private GameUIManager uiManager;
    public bool isFinalLevel = false;

    [Header("Combat Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("State Variables")]
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isOnIce;
    private bool isSprinting;
    private bool isGliding;

    [Header("Dash Variables")]
    private bool isDashing;
    private float dashTimeLeft;
    private float dashCooldownTimer;
    private float lastTapTime;
    private float lastTapDirection; // 1 = ขวา, -1 = ซ้าย

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("🚨 สคริปต์ PlayerController หา Rigidbody2D ไม่เจอครับ!");
    }

    void Update()
    {
        CheckSurroundings();
        UpdateTimers();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // ถ้ากำลัง Dash อยู่ ให้ข้ามการเดินปกติไปเลย
        if (isDashing)
        {
            HandleDashPhysics();
            return;
        }

        HandleMovement();
        HandleGlide();
    }

    // ==========================================
    // 🧠 LOGIC METHODS (แยกส่วนให้ Clean ขึ้น)
    // ==========================================

    private void CheckSurroundings()
    {
        if (groundCheck == null) return;

        bool touchGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool touchIce = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, iceLayer);

        isGrounded = touchGround || touchIce;
        isOnIce = touchIce && !touchGround;
    }

    private void UpdateTimers()
    {
        // ลดคูลดาวน์ Dash
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        // ลดเวลาของสถานะ Dash
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                rb.gravityScale = 1f; // คืนค่าแรงโน้มถ่วงให้กลับมาปกติหลัง Dash เสร็จ
            }
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = isOnIce ? iceMoveSpeed : moveSpeed;
        if (isSprinting) currentSpeed *= sprintMultiplier;

        if (isOnIce)
        {
            float targetSpeedX = moveInput.x * currentSpeed;
            float smoothedX = Mathf.Lerp(rb.linearVelocity.x, targetSpeedX, Time.fixedDeltaTime * iceSlipFactor);
            rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
        }
    }

    private void HandleGlide()
    {
        if (rb.linearVelocity.y < 0 && isGliding && !isGrounded)
        {
            float velocityY = rb.linearVelocity.y;
            float dragForce = dragCoefficient * (velocityY * velocityY);
            rb.AddForce(new Vector2(0, dragForce));
        }
    }

    private void HandleDashPhysics()
    {
        // บังคับให้พุ่งไปข้างหน้าตรงๆ แกน Y เป็น 0 เพื่อไม่ให้ร่วงลงพื้นตอนพุ่ง
        rb.linearVelocity = new Vector2(lastTapDirection * dashSpeed, 0f);
    }

    private void StartDash(float direction)
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownTimer = dashCooldown;
        rb.gravityScale = 0f; // ปิดแรงโน้มถ่วงชั่วคราวตอนพุ่ง

        // รีเซ็ตความเร็วเดิมทิ้งไปก่อน
        rb.linearVelocity = Vector2.zero;

        Debug.Log("💨 Dash!");
    }

    private void Attack()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            Destroy(enemy.gameObject);
            Debug.Log("ฟาดศัตรู " + enemy.name + " ร่วงแล้ว!");
        }
    }

    private void Die()
    {
        if (uiManager != null)
        {
            uiManager.ShowGameOverUI();
            this.enabled = false;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // ==========================================
    // 🎮 INPUT METHODS (ใช้ของ New Input System)
    // ==========================================

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        // ลอจิกตรวจจับการกดปุ่ม 2 ที (Double Tap)
        if (input.x != 0 && moveInput.x == 0) // แปลว่าเพิ่งกดปุ่มลูกศรลงไป
        {
            float currentDirection = Mathf.Sign(input.x); // หาทิศทาง (1 คือขวา, -1 คือซ้าย)

            // เช็คว่า: เวลากดห่างจากครั้งที่แล้วไม่เกินที่กำหนด? + ทิศทางเดียวกัน? + คูลดาวน์เสร็จหรือยัง?
            if (Time.time - lastTapTime < doubleTapTime && currentDirection == lastTapDirection && dashCooldownTimer <= 0)
            {
                StartDash(currentDirection);
            }

            // บันทึกเวลาและทิศทางของการกดครั้งนี้ไว้เช็ครอบหน้า
            lastTapTime = Time.time;
            lastTapDirection = currentDirection;
        }

        moveInput = input;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded && !isDashing) // ห้ามกระโดดตอนกำลังพุ่ง
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnSprint(InputValue value) { isSprinting = value.isPressed; }

    public void OnGlide(InputValue value) { isGliding = value.isPressed; }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed) Attack();
    }

    // ==========================================
    // 💥 COLLISION & TRIGGER METHODS
    // ==========================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
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
            if (hasKey)
            {
                if (isFinalLevel)
                {
                    if (uiManager != null) uiManager.ShowGameClearUI();
                    this.enabled = false;
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
            GameUIManager.ResetTimer();
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}