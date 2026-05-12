using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI keyText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    [Header("Game Clear UI")]
    public GameObject gameClearPanel;

    private static float timer = 0f;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);

        //Active Level
        if (levelText != null)
        {
            levelText.text = SceneManager.GetActiveScene().name;
        }

        //Key check (No key)
        UpdateKeyStatus(false);
    }

    void Update()
    {
        //Time Calculation
        timer += Time.deltaTime;

        //Time Update UI (ทศนิยมสองตำแหน่ง)
        if (timeText != null)
        {
            timeText.text = "Time: " + timer.ToString("F2") + " s";
        }
    }

    public void UpdateKeyStatus(bool hasKey)
    {
        if (keyText != null)
        {
            if (hasKey)
            {
                keyText.text = "Key: Acquired";
                keyText.color = Color.green; // Change text color to green when key is acquired

            }
            else
            {
                keyText.text = "Key: Not Acquired";
                keyText.color = Color.red; // Change text color to red when key is not acquired
            }
        }

    }

    public static void ResetTimer()
    {
        timer = 0f;
    }

    public void ShowGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // เปิดหน้าจอ UI
            Time.timeScale = 0f;           // หยุดเวลาในเกมทั้งหมด (รวมถึงศัตรูและผู้เล่น)
        }
    }

    // เอาไปผูกกับ OnClick() ของปุ่ม Restart ในหน้า Game Over
    public void RestartGame()
    {
        Time.timeScale = 1f; // 🚨 สำคัญมาก! คืนค่าเวลาให้เดินปกติก่อนโหลดฉาก
        ResetTimer();        // เริ่มนับเวลาใหม่
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // โหลดฉากปัจจุบันซ้ำ
    }

    // เอาไปผูกกับ OnClick() ของปุ่ม Main Menu ในหน้า Game Over
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // 🚨 สำคัญมาก! คืนค่าเวลาให้เดินปกติ
        ResetTimer();        // รีเซ็ตเวลาทิ้งไปเลย

        // ตรง "MainMenu" อย่าลืมแก้ให้ตรงกับชื่อ Scene หน้าเมนูหลักของโปรเจกต์นี้นะครับ
        SceneManager.LoadScene("MainMenu");
    }
    public void ShowGameClearUI()
    {
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true); // เปิดหน้าจอ UI
            Time.timeScale = 0f;           // หยุดเวลาในเกมทั้งหมด (รวมถึงศัตรูและผู้เล่น)
        }
    }
}
