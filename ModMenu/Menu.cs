using UnityEngine;
using UnityEngine.InputSystem;
using System.Text;

public class MainMenuController : MonoBehaviour
{
    // Menu state
    private bool menuVisible = false;
    private StringBuilder menuText = new StringBuilder();
    private float lastInputTime = 0f;
    
    // Colors
    private readonly Color headerColor = new Color(0.2f, 0.5f, 1f);
    private readonly Color itemColor = new Color(0.9f, 0.9f, 0.9f);

    private void Update()
    {
        // Toggle menu with X button
        if (Time.time - lastInputTime > 0.3f && 
            (Gamepad.current?.buttonWest.wasPressedThisFrame ?? false))
        {
            menuVisible = !menuVisible;
            lastInputTime = Time.time;
            UpdateMenu();
        }
    }

    private void UpdateMenu()
    {
        menuText.Clear();
        menuText.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(headerColor)}>=== DEEPSEEK MODS ===</color>");
        menuText.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(itemColor)}>Miscellaneous</color>");
        menuText.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(itemColor)}>Movement</color>");
        menuText.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(itemColor)}>Visual</color>");
        menuText.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(itemColor)}>Safey</color>");
        menuText.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(itemColor)}>Fun</color>");
        menuText.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(itemColor)}>OP</color>");
        menuText.AppendLine("<color=#AAAAAA>▲/▼:Navigate  X:Select</color>");
    }

    private void OnGUI()
    {
        if (!menuVisible) return;
        GUI.Label(new Rect(50, 50, 400, 600), menuText.ToString(), new GUIStyle { richText = true, fontSize = 20 });
    }
}