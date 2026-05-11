using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI keyText;

    private static float timer = 0f;

    void Start()
    {
        //Active Level
        if (levelText != null)
        {
            levelText.text = "Level: " + SceneManager.GetActiveScene().name;
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
}
