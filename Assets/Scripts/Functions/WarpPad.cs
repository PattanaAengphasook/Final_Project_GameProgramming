using UnityEngine;
using System.Collections;
using TMPro; // 👈 สำคัญ! ต้องเพิ่มบรรทัดนี้เพื่อเรียกใช้ระบบ Text UI

public class WarpPad : MonoBehaviour
{
    [Header("Warp Settings")]
    [SerializeField] private Transform destinationPad;
    [SerializeField] private float warpDelay = 3f;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI countdownText; // 👈 ช่องสำหรับใส่ตัวหนังสือ

    private bool canWarp = true;
    private bool isCountingDown = false;
    private Coroutine warpCoroutine;

    private void Start()
    {
        // เริ่มเกมมา ให้ซ่อนข้อความนับถอยหลังไว้ก่อน
        if (countdownText != null) countdownText.text = "";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (destinationPad == null) return; // ถ้าลืมลากแท่นปลายทางมาใส่ ให้หยุดทำงานไปเลย

            if (canWarp)
            {
                warpCoroutine = StartCoroutine(WarpSequence(collision.transform));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isCountingDown)
        {
            if (warpCoroutine != null)
            {
                StopCoroutine(warpCoroutine);
                canWarp = true;
                isCountingDown = false;

                // 🚫 ถ้ายกเลิก ลบข้อความทิ้ง
                if (countdownText != null) countdownText.text = "";
            }
        }
    }

    private IEnumerator WarpSequence(Transform playerTransform)
    {
        canWarp = false;
        isCountingDown = true;

        for (int i = (int)warpDelay; i > 0; i--)
        {
            // ⏳ อัปเดตตัวเลขโชว์บนหน้าจอ (สามารถแก้คำว่า "Warp in" เป็นคำอื่นได้ตามใจชอบ)
            if (countdownText != null) countdownText.text = "Warp in " + i.ToString();

            yield return new WaitForSeconds(1f);
        }

        isCountingDown = false;

        // ✨ วาร์ปเสร็จแล้ว ลบข้อความทิ้ง
        if (countdownText != null) countdownText.text = "";

        WarpPad destScript = destinationPad.GetComponent<WarpPad>();
        if (destScript != null)
        {
            destScript.StartCooldown();
        }

        playerTransform.position = new Vector3(destinationPad.position.x, destinationPad.position.y + 0.5f, playerTransform.position.z);

        yield return new WaitForSeconds(0.5f);
        canWarp = true;
    }

    public void StartCooldown()
    {
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        canWarp = false;
        yield return new WaitForSeconds(1.5f);
        canWarp = true;
    }
}