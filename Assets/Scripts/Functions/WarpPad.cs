using UnityEngine;
using System.Collections; // สำคัญ ต้องมีเพื่อใช้ IEnumerator

public class WarpPad : MonoBehaviour
{
    [Header("Warp Settings")]
    [SerializeField] private Transform destinationPad;

    private bool canWarp = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"⚠️ มีสิ่งนี้มาเหยียบแท่นวาร์ป: ชื่อ [{collision.gameObject.name}] | Tag ของมันคือ [{collision.tag}]");

        if (collision.CompareTag("Player"))
        {
            if (destinationPad == null)
            {
                Debug.LogError("❌ วาร์ปไม่ได้! เพราะลากแท่นปลายทาง (Destination Pad) มาใส่ในช่อง Inspector หรือยัง?");
                return;
            }

            if (!canWarp)
            {
                Debug.Log("⏳ วาร์ปไม่ได้! แท่นนี้กำลังติดคูลดาวน์พักเหนื่อยอยู่");
                return;
            }
            StartCoroutine(WarpSequence(collision.transform));
        }
        /*if (collision.CompareTag("Player") && canWarp && destinationPad != null)
        {
            StartCoroutine(WarpSequence(collision.transform));
        }*/

    }

    private IEnumerator WarpSequence(Transform playerTransform)
    {
        canWarp = false;
        WarpPad destScript = destinationPad.GetComponent<WarpPad>();
        if (destScript != null)
        {
            destScript.StartCooldown();
        }

        // ย้ายตัวละคร (ขยับแกน Y ขึ้น 0.5f กันติดพื้น)
        playerTransform.position = new Vector3(destinationPad.position.x, destinationPad.position.y + 0.5f, playerTransform.position.z);
        Debug.Log("วาร์ปสำเร็จ!");

        yield return new WaitForSeconds(0.5f);
        canWarp = true;
    }

    // ฟังก์ชันนี้เปิดไว้ให้แท่นอื่นมาสั่งให้แท่นนี้พักเหนื่อย (คูลดาวน์)
    public void StartCooldown()
    {
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        canWarp = false;
        yield return new WaitForSeconds(0.5f);
        canWarp = true;
    }
}