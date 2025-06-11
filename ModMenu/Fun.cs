using UnityEngine;
using System.Text;
using GorillaLocomotion;
using UnityEngine.InputSystem;

public class FunMods : MonoBehaviour
{
    // Existing Fun Mods
    private bool dancePartyEnabled = false;
    private bool gravityFlipEnabled = false;
    private bool rainbowModeEnabled = false;
    private bool giantMonkeyEnabled = false;

    // Flight Systems
    public static bool noclip = false;
    private static float flySpeed = 10f;
    private static float rightTrigger;
    private static bool rightPrimary;
    private static Vector2 leftJoystick;

    private StringBuilder menuText = new StringBuilder();

    private void Update()
    {
        // Get controller inputs
        rightTrigger = Gamepad.current?.rightTrigger.ReadValue() ?? 0f;
        rightPrimary = Gamepad.current?.rightShoulder.isPressed ?? false;
        leftJoystick = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;

        // Update active flight systems
        if (flightModeEnabled) UpdateFlightSystem();
    }

    #region Flight Systems
    public static bool flightModeEnabled = false;

    public static void UpdateFlightSystem()
    {
        if (rightTrigger > 0.5f) TriggerFly();
        if (rightPrimary) NoclipFly();
        JoystickFly();
    }

    private static void TriggerFly()
    {
        Player.Instance.transform.position += 
            Player.Instance.headCollider.transform.forward * Time.deltaTime * flySpeed;
        Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private static void NoclipFly()
    {
        Player.Instance.transform.position += 
            Player.Instance.headCollider.transform.forward * Time.deltaTime * flySpeed;
        Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;

        if (!noclip)
        {
            noclip = true;
            UpdateClipColliders(false);
        }
    }

    private static void JoystickFly()
    {
        if (Mathf.Abs(leftJoystick.x) > 0.3f || Mathf.Abs(leftJoystick.y) > 0.3f)
        {
            Player.Instance.transform.position += 
                (Player.Instance.headCollider.transform.forward * Time.deltaTime * (leftJoystick.y * flySpeed)) + 
                (Player.Instance.headCollider.transform.right * Time.deltaTime * (leftJoystick.x * flySpeed));
            Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    private static void UpdateClipColliders(bool enabled)
    {
        Player.Instance.GetComponent<Collider>().enabled = enabled;
        Player.Instance.bodyCollider.enabled = enabled;
    }
    #endregion

    #region Menu System
    public void ShowMenu()
    {
        menuText.Clear();
        menuText.AppendLine("<color=#1AE6D6>=== DEEPSEEK FUN MODS ===</color>");
        
        // Flight Options
        menuText.AppendLine(flightModeEnabled ? 
            "<color=#00FF80>• Flight Mode: ON</color> <color=#AAAAAA>[X]</color>" : 
            "<color=#FF4D4D>• Flight Mode: OFF</color> <color=#AAAAAA>[X]</color>");
        menuText.AppendLine($"<color=#AAAAAA>RT: Forward  |  Joystick: Direction</color>");

        // Existing Fun Mods
        menuText.AppendLine(dancePartyEnabled ? 
            "<color=#00FF80>• Dance Party: ON</color> <color=#AAAAAA>[X]</color>" : 
            "<color=#FF4D4D>• Dance Party: OFF</color> <color=#AAAAAA>[X]</color>");
        menuText.AppendLine(gravityFlipEnabled ? 
            "<color=#00FF80>• Gravity Flip: ON</color> <color=#AAAAAA>[X]</color>" : 
            "<color=#FF4D4D>• Gravity Flip: OFF</color> <color=#AAAAAA>[X]</color>");
        menuText.AppendLine(rainbowModeEnabled ? 
            "<color=#00FF80>• Rainbow Mode: ON</color> <color=#AAAAAA>[X]</color>" : 
            "<color=#FF4D4D>• Rainbow Mode: OFF</color> <color=#AAAAAA>[X]</color>");
        menuText.AppendLine(giantMonkeyEnabled ? 
            "<color=#00FF80>• Giant Monkey: ON</color> <color=#AAAAAA>[X]</color>" : 
            "<color=#FF4D4D>• Giant Monkey: OFF</color> <color=#AAAAAA>[X]</color>");
        
        menuText.AppendLine("<color=#AAAAAA>B: Back  |  X: Toggle</color>");
    }

    public void ToggleFlightMode()
    {
        flightModeEnabled = !flightModeEnabled;
        NotifiLib.SendNotification($"Flight Mode {(flightModeEnabled ? "<color=#00FF80>ENABLED</color>" : "<color=#FF4D4D>DISABLED</color>")}");

        if (!flightModeEnabled && noclip)
        {
            noclip = false;
            UpdateClipColliders(true);
        }
    }

    public void ToggleDanceParty()
    {
        dancePartyEnabled = !dancePartyEnabled;
        NotifiLib.SendNotification($"Dance Party {(dancePartyEnabled ? "<color=#00FF80>ENABLED</color>" : "<color=#FF4D4D>DISABLED</color>")}");
    }

    public void ToggleGravityFlip()
    {
        gravityFlipEnabled = !gravityFlipEnabled;
        NotifiLib.SendNotification($"Gravity Flip {(gravityFlipEnabled ? "<color=#00FF80>ENABLED</color>" : "<color=#FF4D4D>DISABLED</color>")}");
        Physics.gravity = gravityFlipEnabled ? new Vector3(0, 9.81f, 0) : new Vector3(0, -9.81f, 0);
    }

    public void ToggleGiantMonkey()
    {
        giantMonkeyEnabled = !giantMonkeyEnabled;
        NotifiLib.SendNotification($"<color=#FFAA00>Giant Monkey Mode</color> {(giantMonkeyEnabled ? "<color=#00FF80>ENABLED</color>" : "<color=#FF4D4D>DISABLED</color>")}", 2000);
        Player.Instance.transform.localScale = giantMonkeyEnabled ? Vector3.one * 2f : Vector3.one;
    }
    #endregion
}