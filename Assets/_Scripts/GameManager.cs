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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && !fast)
        {
            slow = !slow;
        }

        if (Input.GetKeyDown(KeyCode.V) && !slow)
        {
            fast = !fast;
        }

        if (Input.GetKeyDown(KeyCode.N) && !slow)
        {
            fast = false;
            faster = true;
            fastest = false;
        }

        if (Input.GetKeyDown(KeyCode.Z) && !slow)
        {
            fast = false;
            faster = false;
            fastest = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
        }

        if (paused)
        {
            speedText.text = "Paused";
            Time.timeScale = 0f;
            return; // Skip further updates if paused
        }

        if (fast && !slow)
        {
            speedText.text = "GameSpeed = 2x";
            Time.timeScale = 2f;
        }
        else if (slow && !fast)
        {
            speedText.text = "GameSpeed = 0.5x";
            Time.timeScale = 0.5f;
        }
        else if (!slow && faster)
        {
            speedText.text = "GameSpeed = 10x";
            Time.timeScale = 10f;
        }
        else if (!slow && fastest)
        {
            speedText.text = "GameSpeed = 25x";
            Time.timeScale = 25f;
        }
        else
        {
            speedText.text = "";
            Time.timeScale = 1f;
        }
    }
}
