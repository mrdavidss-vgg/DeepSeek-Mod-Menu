using UnityEngine;
using UnityEngine.InputSystem;
using System.Text;
using Photon.Pun;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using GorillaLocomotion;

public class TagModMenu : MonoBehaviour
{
    // Menu State
    private bool menuVisible = false;
    private int selectedOption = 0;
    private float lastInputTime = 0f;
    private StringBuilder menuText = new StringBuilder();

    // Tag Systems
    private bool tagGunEnabled = true;
    private bool gunLocked = false;
    private VRRig lockTarget;
    private bool joinAsTagged = false;
    private bool longArmsEnabled = false;
    private bool flickTagEnabled = false;
    private float armlength = 1.5f; // Default arm length multiplier

    private void Update()
    {
        // Toggle menu with X button
        if (Time.time - lastInputTime > 0.3f && 
            (Gamepad.current?.buttonWest.wasPressedThisFrame ?? false))
        {
            menuVisible = !menuVisible;
            lastInputTime = Time.time;
            UpdateMenuText();
            NotifiLib.SendNotification($"<color=#1AE6D6>Menu {(menuVisible ? "Opened" : "Closed")}</color>");
        }

        // Navigation
        if (menuVisible)
        {
            Vector2 nav = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
            
            if (Time.time - lastInputTime > 0.2f)
            {
                if (nav.y > 0.5f) // Up
                {
                    selectedOption = Mathf.Max(0, selectedOption - 1);
                    lastInputTime = Time.time;
                    UpdateMenuText();
                }
                else if (nav.y < -0.5f) // Down
                {
                    selectedOption = Mathf.Min(3, selectedOption + 1); // Increased to 4 options
                    lastInputTime = Time.time;
                    UpdateMenuText();
                }
                else if (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false) // Select (A)
                {
                    HandleOptionSelection();
                }
            }
        }

        if (tagGunEnabled) TagGun();
        if (flickTagEnabled) FlickTagGun();
        if (longArmsEnabled) EnableSteamLongArms();
    }

    private void HandleOptionSelection()
    {
        switch(selectedOption)
        {
            case 0: ToggleTagGun(); break;
            case 1: ToggleJoinStatus(); break;
            case 2: ToggleLongArms(); break;
            case 3: ToggleFlickTag(); break;
        }
    }

    private void ToggleTagGun()
    {
        tagGunEnabled = !tagGunEnabled;
        NotifiLib.SendNotification($"<color=#1AE6D6>Tag Gun {(tagGunEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}</color>");
        UpdateMenuText();
    }

    private void ToggleLongArms()
    {
        longArmsEnabled = !longArmsEnabled;
        NotifiLib.SendNotification($"<color=#1AE6D6>Long Arms {(longArmsEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}</color>");
        UpdateMenuText();
    }

    private void ToggleFlickTag()
    {
        flickTagEnabled = !flickTagEnabled;
        NotifiLib.SendNotification($"<color=#1AE6D6>Flick Tag {(flickTagEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}</color>");
        UpdateMenuText();
    }

    private void EnableSteamLongArms()
    {
        Player.Instance.transform.localScale = new Vector3(armlength, armlength, armlength);
    }

    private void FlickTagGun()
    {
        if (GetGunInput(false))
        {
            var gunData = RenderGun();
            if (GetGunInput(true))
            {
                Player.Instance.rightControllerTransform.position = 
                    gunData.NewPointer.transform.position + new Vector3(0f, 0.1f, 0f);
            }
        }
    }

    private (RaycastHit Ray, GameObject NewPointer) RenderGun()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 50f);

        GameObject pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointer.transform.position = hit.point;
        pointer.transform.localScale = Vector3.one * 0.1f;
        pointer.GetComponent<Renderer>().material.color = Color.red;
        Destroy(pointer.GetComponent<Collider>());

        return (hit, pointer);
    }

    // [Rest of your existing methods (TagGun, ToggleJoinStatus, etc.) remain unchanged...]

    private void UpdateMenuText()
    {
        menuText.Clear();
        menuText.AppendLine("<color=#1AE6D6>=== TAG MODS ===</color>");
        
        // Tag Gun
        menuText.AppendLine(selectedOption == 0 ? 
            $"> {(tagGunEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>TAG GUN</color> <" : 
            $"{(tagGunEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>TAG GUN</color>");
        
        // Join Status
        menuText.AppendLine(selectedOption == 1 ? 
            $"> JOIN AS {(joinAsTagged ? "<color=red>TAGGED</color>" : "<color=green>UNTAGGED</color>")} <" : 
            $"JOIN AS {(joinAsTagged ? "<color=red>TAGGED</color>" : "<color=green>UNTAGGED</color>")}");
        
        // Long Arms
        menuText.AppendLine(selectedOption == 2 ? 
            $"> {(longArmsEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>LONG ARMS</color> <" : 
            $"{(longArmsEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>LONG ARMS</color>");
        
        // Flick Tag
        menuText.AppendLine(selectedOption == 3 ? 
            $"> {(flickTagEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>FLICK TAG</color> <" : 
            $"{(flickTagEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>FLICK TAG</color>");
        
        menuText.AppendLine("\n<color=#AAAAAA>A: Select  X: Close</color>");
    }

    // [Rest of your existing methods (OnGUI, PlayerIsTagged, etc.) remain unchanged...]
}