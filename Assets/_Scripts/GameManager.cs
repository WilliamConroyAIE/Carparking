using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool paused;

    [Space]
    public bool slow;
    public bool fast, faster, fastest;

    [Space]
    public TextMeshProUGUI speedText;

    void Update()
    {
        // Toggle slow mode if not fast
        if (Input.GetKeyDown(KeyCode.X) && !fast && !faster && !fastest)
        {
            slow = !slow;
        }

        // Toggle fast mode if not slow
        if (Input.GetKeyDown(KeyCode.V) && !slow)
        {
            fast = !fast;
            faster = false;
            fastest = false;
        }

        // 10x speed (faster)
        if (Input.GetKeyDown(KeyCode.N) && !slow)
        {
            fast = false;
            faster = true;
            fastest = false;
        }

        // 25x speed (fastest)
        if (Input.GetKeyDown(KeyCode.Z) && !slow)
        {
            fast = false;
            faster = false;
            fastest = true;
        }

        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
        }

        // Handle pause
        if (paused)
        {
            speedText.text = "Paused";
            Time.timeScale = 0f;
            return;
        }

        // Set speed text and Time.timeScale
        if (slow)
        {
            speedText.text = "GameSpeed = 0.5x";
            Time.timeScale = 0.5f;
        }
        else if (fast)
        {
            speedText.text = "GameSpeed = 2x";
            Time.timeScale = 2f;
        }
        else if (faster)
        {
            speedText.text = "GameSpeed = 10x";
            Time.timeScale = 10f;
        }
        else if (fastest)
        {
            speedText.text = "GameSpeed = 25x";
            Time.timeScale = 25f;
        }
        else
        {
            speedText.text = "GameSpeed = 1x";
            Time.timeScale = 1f;
        }
    }
}
