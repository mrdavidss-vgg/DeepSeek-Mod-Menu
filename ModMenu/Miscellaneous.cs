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
                    selectedOption = Mathf.Min(1, selectedOption + 1);
                    lastInputTime = Time.time;
                    UpdateMenuText();
                }
                else if (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false) // Select (A)
                {
                    if (selectedOption == 0) ToggleTagGun();
                    if (selectedOption == 1) ToggleJoinStatus();
                }
            }
        }

        if (tagGunEnabled) TagGun();
    }

    private void ToggleTagGun()
    {
        tagGunEnabled = !tagGunEnabled;
        NotifiLib.SendNotification($"<color=#1AE6D6>Tag Gun {(tagGunEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}</color>");
        UpdateMenuText();
    }

    private void ToggleJoinStatus()
    {
        joinAsTagged = !joinAsTagged;
        if (joinAsTagged) 
        {
            TagOnJoin();
            NotifiLib.SendNotification("<color=#1AE6D6>Will now join as </color><color=red>TAGGED</color>");
        }
        else 
        {
            NoTagOnJoin();
            NotifiLib.SendNotification("<color=#1AE6D6>Will now join as </color><color=green>UNTAGGED</color>");
        }
        UpdateMenuText();
    }

    private void TagGun()
    {
        if (GetGunInput(false)) // Right Trigger
        {
            if (gunLocked && lockTarget != null && !PlayerIsTagged(lockTarget))
            {
                GorillaTagger.Instance.offlineVRRig.transform.position = 
                    lockTarget.transform.position - new Vector3(0f, 3f, 0f);
                NotifiLib.SendNotification($"<color=#1AE6D6>Teleported to </color><color=yellow>{lockTarget.photonView.Owner.NickName}</color>");
            }

            if (GetGunInput(true)) // Left Trigger
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, 
                    Camera.main.transform.forward, out hit, 50f))
                {
                    VRRig target = hit.collider.GetComponentInParent<VRRig>();
                    if (target && !PlayerIsLocal(target))
                    {
                        gunLocked = true;
                        lockTarget = target;
                        NotifiLib.SendNotification($"<color=#1AE6D6>Locked onto </color><color=yellow>{target.photonView.Owner.NickName}</color>");
                    }
                }
            }
        }
        else if (gunLocked)
        {
            gunLocked = false;
        }
    }

    public void UntagSelf()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Disconnect();
            NoTagOnJoin();
            PhotonNetwork.ConnectUsingSettings();
            NotifiLib.SendNotification("<color=#1AE6D6>Reconnecting to untag yourself...</color>");
        }
        else
        {
            RemoveInfected(PhotonNetwork.LocalPlayer);
            NotifiLib.SendNotification("<color=#1AE6D6>Removed tag from yourself</color>");
        }
        GorillaLocomotion.Player.Instance.disableMovement = false;
    }

    public void UntagAll()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            NotifiLib.SendNotification("<color=red>ERROR:</color> You must be master client to untag everyone");
        }
        else
        {
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                RemoveInfected(player);
            }
            NotifiLib.SendNotification("<color=#1AE6D6>Removed tags from all players</color>");
        }
    }

    private void NoTagOnJoin()
    {
        PlayerPrefs.SetString("didTutorial", "nope");
        Hashtable h = new Hashtable { { "didTutorial", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);
        PlayerPrefs.Save();
    }

    private void TagOnJoin()
    {
        PlayerPrefs.SetString("didTutorial", "done");
        Hashtable h = new Hashtable { { "didTutorial", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);
        PlayerPrefs.Save();
    }

    private bool GetGunInput(bool primary) => 
        primary ? (Gamepad.current?.rightTrigger.ReadValue() > 0.8f) 
                : (Gamepad.current?.leftTrigger.ReadValue() > 0.5f);

    private bool PlayerIsTagged(VRRig rig) => rig.mainSkin.material.name.Contains("infected");
    private bool PlayerIsLocal(VRRig rig) => rig.photonView.IsMine;

    private void UpdateMenuText()
    {
        menuText.Clear();
        menuText.AppendLine("<color=#1AE6D6>=== TAG MODS ===</color>");
        menuText.AppendLine(selectedOption == 0 ? 
            $"> {(tagGunEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>TAG GUN</color> <" : 
            $"{(tagGunEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>")} <color=#1AE6D6>TAG GUN</color>");
        
        menuText.AppendLine(selectedOption == 1 ? 
            $"> JOIN AS {(joinAsTagged ? "<color=red>TAGGED</color>" : "<color=green>UNTAGGED</color>")} <" : 
            $"JOIN AS {(joinAsTagged ? "<color=red>TAGGED</color>" : "<color=green>UNTAGGED</color>")}");
        
        menuText.AppendLine("\n<color=#AAAAAA>A: Select  X: Close</color>");
    }

    private void OnGUI()
    {
        if (!menuVisible) return;
        
        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(50, 50, 300, 300), menuText.ToString(), style);
    }

    private static void RemoveInfected(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.ContainsKey("infected"))
        {
            Hashtable hash = new Hashtable();
            hash.Add("infected", false);
            player.SetCustomProperties(hash);
        }
    }
}