using UnityEngine;
using System.Text;
using GorillaLocomotion;
using UnityEngine.InputSystem;
using Photon.Pun;

public class FunMods : MonoBehaviour
{
    // Menu State
    private bool menuVisible = false;
    private int selectedOption = 0;
    private float lastInputTime = 0f;
    private StringBuilder menuText = new StringBuilder();

    // Fun Mod Toggles
    private bool dancePartyEnabled = false;
    private bool gravityFlipEnabled = false;
    private bool rainbowModeEnabled = false;
    private bool giantMonkeyEnabled = false;
    private bool flightModeEnabled = false;
    private bool waterGunEnabled = false;
    private bool urineGunEnabled = false;

    // Flight Systems
    public static bool noclip = false;
    private static float flySpeed = 10f;
    private static float rightTrigger;
    private static bool rightPrimary;
    private static Vector2 leftJoystick;

    // Gun Systems
    private static bool gunLocked = false;
    private static VRRig lockTarget;
    private static float splashDel = 0f;

    private void Update()
    {
        // Get controller inputs
        rightTrigger = Gamepad.current?.rightTrigger.ReadValue() ?? 0f;
        rightPrimary = Gamepad.current?.rightShoulder.isPressed ?? false;
        leftJoystick = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;

        // Update active systems
        if (flightModeEnabled) UpdateFlightSystem();
        if (waterGunEnabled) WaterSplashGun();
        if (urineGunEnabled) UrineGun();
    }

    #region Flight Systems
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

    #region Gun Systems
    private static void WaterSplashGun()
    {
        if (GetGunInput(false))
        {
            var gunData = RenderGun();
            if (GetGunInput(true))
            {
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = gunData.NewPointer.transform.position - new Vector3(0, 1, 0);
                
                if (Time.time > splashDel)
                {
                    GorillaTagger.Instance.myVRRig.photonView.RPC(
                        "RPC_PlaySplashEffect", 
                        RpcTarget.All,
                        gunData.NewPointer.transform.position,
                        Quaternion.Euler(new Vector3(
                            Random.Range(0,360), 
                            Random.Range(0,360), 
                            Random.Range(0,360))),
                        4f,
                        100f,
                        true,
                        false
                    );
                    splashDel = Time.time + 0.1f;
                }
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }
    }

    private static void UrineGun()
    {
        if (GetGunInput(false))
        {
            var gunData = RenderGun();
            if (gunLocked && lockTarget != null)
            {
                Vector3 startpos = lockTarget.transform.position + 
                    new Vector3(0f, -0.4f, 0f) + 
                    (lockTarget.transform.forward * 0.2f);
                Vector3 charvel = lockTarget.transform.forward * -8.33f;
                BetaFireProjectile("ScienceCandyLeftAnchor", startpos, charvel, Color.yellow);
            }

            if (GetGunInput(true))
            {
                VRRig target = gunData.Ray.collider?.GetComponentInParent<VRRig>();
                if (target && !PlayerIsLocal(target))
                {
                    gunLocked = true;
                    lockTarget = target;
                }
            }
        }
        else if (gunLocked)
        {
            gunLocked = false;
        }
    }

    private static (RaycastHit Ray, GameObject NewPointer) RenderGun()
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

    private static bool GetGunInput(bool primary) => 
        primary ? (Gamepad.current?.rightTrigger.ReadValue() > 0.8f) 
                : (Gamepad.current?.leftTrigger.ReadValue() > 0.5f);

    private static bool PlayerIsLocal(VRRig rig) => rig.photonView.IsMine;
    #endregion

    #region Menu System
    private void UpdateMenuText()
    {
        menuText.Clear();
        menuText.AppendLine("<color=#1AE6D6>=== FUN MODS ===</color>");
        
        // Flight Options
        menuText.AppendLine(selectedOption == 0 ? 
            $"> {(flightModeEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>FLIGHT MODE</color> <" : 
            $"{(flightModeEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>FLIGHT MODE</color>");
        
        // Gun Options
        menuText.AppendLine(selectedOption == 1 ? 
            $"> {(waterGunEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>WATER GUN</color> <" : 
            $"{(waterGunEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>WATER GUN</color>");
        
        menuText.AppendLine(selectedOption == 2 ? 
            $"> {(urineGunEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>URINE GUN</color> <" : 
            $"{(urineGunEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>URINE GUN</color>");
        
        // Fun Mods
        menuText.AppendLine(selectedOption == 3 ? 
            $"> {(dancePartyEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>DANCE PARTY</color> <" : 
            $"{(dancePartyEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>DANCE PARTY</color>");
        
        menuText.AppendLine(selectedOption == 4 ? 
            $"> {(gravityFlipEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>GRAVITY FLIP</color> <" : 
            $"{(gravityFlipEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>GRAVITY FLIP</color>");
        
        menuText.AppendLine("<color=#AAAAAA>A: Toggle  B: Back  X: Close</color>");
    }

    private void HandleInput()
    {
        if (Time.time - lastInputTime > 0.2f)
        {
            Vector2 nav = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
            
            if (nav.y > 0.5f) // Up
            {
                selectedOption = Mathf.Max(0, selectedOption - 1);
                lastInputTime = Time.time;
            }
            else if (nav.y < -0.5f) // Down
            {
                selectedOption = Mathf.Min(4, selectedOption + 1);
                lastInputTime = Time.time;
            }
            else if (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false) // A
            {
                ToggleSelectedOption();
            }
            
            UpdateMenuText();
        }
    }

    private void ToggleSelectedOption()
    {
        switch(selectedOption)
        {
            case 0: ToggleFlightMode(); break;
            case 1: ToggleWaterGun(); break;
            case 2: ToggleUrineGun(); break;
            case 3: ToggleDanceParty(); break;
            case 4: ToggleGravityFlip(); break;
        }
    }

    private void ToggleFlightMode()
    {
        flightModeEnabled = !flightModeEnabled;
        NotifiLib.SendNotification($"Flight Mode {(flightModeEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
        if (!flightModeEnabled && noclip) UpdateClipColliders(true);
    }

    private void ToggleWaterGun()
    {
        waterGunEnabled = !waterGunEnabled;
        NotifiLib.SendNotification($"Water Gun {(waterGunEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
    }

    private void ToggleUrineGun()
    {
        urineGunEnabled = !urineGunEnabled;
        NotifiLib.SendNotification($"Urine Gun {(urineGunEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
    }

    private void ToggleDanceParty()
    {
        dancePartyEnabled = !dancePartyEnabled;
        NotifiLib.SendNotification($"Dance Party {(dancePartyEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
    }

    private void ToggleGravityFlip()
    {
        gravityFlipEnabled = !gravityFlipEnabled;
        NotifiLib.SendNotification($"Gravity Flip {(gravityFlipEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
        Physics.gravity = gravityFlipEnabled ? new Vector3(0, 9.81f, 0) : new Vector3(0, -9.81f, 0);
    }
    #endregion

    private void OnGUI()
    {
        if (!menuVisible) return;
        
        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(50, 50, 350, 400), menuText.ToString(), style);
    }
}