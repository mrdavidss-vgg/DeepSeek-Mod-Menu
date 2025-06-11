using UnityEngine;
using UnityEngine.InputSystem;
using System.Text;
using GorillaLocomotion;

public class DeepSeekModMenu : MonoBehaviour
{
    // Menu State
    public enum MenuState { Main, Movement, Visual, Fun, Safety, OP, Miscellaneous }
    public MenuState currentState = MenuState.Main;
    private int selectedOption = 0;
    private float lastInputTime = 0f;
    private const float inputCooldown = 0.2f;

    // Performance
    private float fpsRefreshTime = 0.5f;
    private float fpsTimer;
    private int frameCount;
    private float currentFPS;

    // Visual Elements
    private readonly Color menuColor = new Color(0.1f, 0.6f, 0.9f, 1f);
    private GameObject pointer;
    private Material pointerMaterial;

    // Menu Content
    private readonly string[] mainMenuOptions = 
    {
        "Fun Mods",
        "Movement",
        "Visual",
        "Miscellaneous",
        "Safety",
        "OP Mods"
    };

    private void Start()
    {
        CreatePointer();
        fpsTimer = fpsRefreshTime;
    }

    private void CreatePointer()
    {
        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointer.transform.localScale = Vector3.one * 0.05f;
        pointerMaterial = new Material(Shader.Find("Standard"));
        pointerMaterial.color = menuColor;
        pointer.GetComponent<Renderer>().material = pointerMaterial;
        pointer.SetActive(false);
    }

    private void Update()
    {
        UpdateFPS();
        HandleInput();
        UpdatePointer();
        pointer.SetActive(currentState != MenuState.Main);
    }

    private void UpdateFPS()
    {
        frameCount++;
        fpsTimer -= Time.deltaTime;
        
        if (fpsTimer <= 0f)
        {
            currentFPS = frameCount / fpsRefreshTime;
            frameCount = 0;
            fpsTimer = fpsRefreshTime;
        }
    }

    private void HandleInput()
    {
        if (Time.time - lastInputTime < inputCooldown) return;
        
        // Toggle Menu
        if (Gamepad.current?.buttonWest.wasPressedThisFrame ?? false)
        {
            currentState = (currentState == MenuState.Main) ? MenuState.Main : MenuState.Main;
            lastInputTime = Time.time;
            selectedOption = 0;
            return;
        }

        // Back Navigation
        if ((Gamepad.current?.buttonEast.wasPressedThisFrame ?? false) && currentState != MenuState.Main)
        {
            ReturnToMain();
            lastInputTime = Time.time;
            return;
        }

        // Menu Navigation
        switch (currentState)
        {
            case MenuState.Main:
                HandleMainMenuInput();
                break;
            default:
                HandleSubMenuInput();
                break;
        }
    }

    private void HandleMainMenuInput()
    {
        int optionCount = mainMenuOptions.Length;

        // Vertical Navigation
        if (Gamepad.current?.leftStick.up.wasPressedThisFrame ?? false)
        {
            selectedOption = (selectedOption - 1 + optionCount) % optionCount;
            lastInputTime = Time.time;
        }
        else if (Gamepad.current?.leftStick.down.wasPressedThisFrame ?? false)
        {
            selectedOption = (selectedOption + 1) % optionCount;
            lastInputTime = Time.time;
        }
        // Selection
        else if (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false)
        {
            SelectMenuItem(selectedOption);
            lastInputTime = Time.time;
        }
    }

    private void HandleSubMenuInput()
    {
        // Implement sub-menu navigation similarly
        // ...
    }

    private void UpdatePointer()
    {
        if (Player.Instance != null && pointer.activeSelf)
        {
            pointer.transform.position = Player.Instance.headCollider.transform.position + 
                                       Player.Instance.headCollider.transform.forward * 0.3f;
        }
    }

    public void ReturnToMain()
    {
        currentState = MenuState.Main;
        selectedOption = 0;
    }

    public void SelectMenuItem(int index)
    {
        currentState = index switch
        {
            0 => MenuState.Fun,
            1 => MenuState.Movement,
            2 => MenuState.Visual,
            3 => MenuState.Miscellaneous,
            4 => MenuState.Safety,
            5 => MenuState.OP,
            _ => MenuState.Main
        };
        selectedOption = 0;
    }

    private void OnGUI()
    {
        if (currentState == MenuState.Main) return;

        // Menu Background
        GUI.Box(new Rect(50, 50, 350, 250), "");

        // Header
        GUI.Label(new Rect(60, 55, 330, 30), $"<size=24><color=#{ColorUtility.ToHtmlStringRGB(menuColor)}>DeepSeek V1 Menu</color></size>");
        GUI.Label(new Rect(60, 85, 330, 30), $"<size=18>FPS: {currentFPS:00.0}</size>");
        GUI.Label(new Rect(60, 110, 330, 2), "——————————————");

        // Page Title
        GUI.Label(new Rect(60, 120, 330, 30), $"<size=20><b>{currentState}</b></size>");

        // Render appropriate menu content
        switch (currentState)
        {
            case MenuState.Main:
                RenderMainMenu();
                break;
            case MenuState.Movement:
                // Render movement menu
                break;
            // Other cases...
        }

        // Page Navigation
        if (currentState != MenuState.Main)
        {
            if (GUI.Button(new Rect(315, 150, 30, 30), "<"))
            {
                CycleMenu(-1);
            }

            if (GUI.Button(new Rect(315, 185, 30, 30), ">"))
            {
                CycleMenu(1);
            }
        }
    }

    private void RenderMainMenu()
    {
        for (int i = 0; i < mainMenuOptions.Length; i++)
        {
            bool isSelected = (i == selectedOption);
            Color bgColor = isSelected ? menuColor : Color.gray;
            Color textColor = isSelected ? Color.white : Color.black;

            GUI.backgroundColor = bgColor;
            if (GUI.Button(new Rect(60, 150 + i * 35, 250, 30), mainMenuOptions[i]))
            {
                SelectMenuItem(i);
            }
            GUI.backgroundColor = Color.white;

            if (isSelected)
            {
                GUI.Box(new Rect(55, 150 + i * 35, 5, 30), "");
            }
        }
    }

    private void CycleMenu(int direction)
    {
        int stateCount = System.Enum.GetValues(typeof(MenuState)).Length;
        currentState = (MenuState)(((int)currentState + direction + stateCount) % stateCount);
        if (currentState == MenuState.Main) currentState = (MenuState)(direction > 0 ? 1 : stateCount - 1);
        selectedOption = 0;
    }
}