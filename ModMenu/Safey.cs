using UnityEngine;
using Photon.Pun;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class Safety
{
    // Configuration
    private const float threshold = 0.15f; // Distance in meters to detect report attempts
    private const bool smartarp = true; // Smart Anti-Report Protection
    private static string buttonClickPlayer = "";
    private static int buttonClickTime = 0;

    public static void Initialize()
    {
        // Hook into game events if needed
        GorillaScoreboardTotalUpdater.OnScoreboardUpdate += CheckReportButtons;
    }

    public static void AntiReportDisconnect()
    {
        try
        {
            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer == NetworkSystem.Instance.LocalPlayer)
                {
                    Transform report = line.reportButton.gameObject.transform;
                    
                    // Visualize the danger zone if enabled
                    if (ModMenu.visualizeAntiReport)
                    {
                        VisualizeAura(report.position, threshold, Color.red);
                    }

                    CheckHandProximity(report.position, line);
                }
            }
        }
        catch 
        {
            // Silently handle errors (not connected, etc)
        }
    }

    private static void CheckHandProximity(Vector3 reportPosition, GorillaPlayerScoreboardLine line)
    {
        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
        {
            if (vrrig != GorillaTagger.Instance.offlineVRRig)
            {
                float rightHandDist = Vector3.Distance(vrrig.rightHandTransform.position, reportPosition);
                float leftHandDist = Vector3.Distance(vrrig.leftHandTransform.position, reportPosition);

                if (rightHandDist < threshold || leftHandDist < threshold)
                {
                    HandleReportAttempt(vrrig, line);
                }
            }
        }
    }

    private static void HandleReportAttempt(VRRig vrrig, GorillaPlayerScoreboardLine line)
    {
        if (!smartarp || (smartarp && ShouldTriggerProtection(line)))
        {
            ExecuteProtection(vrrig);
        }
    }

    private static bool ShouldTriggerProtection(GorillaPlayerScoreboardLine line)
    {
        return line.linePlayer.UserId == buttonClickPlayer && 
               Time.frameCount == buttonClickTime && 
               PhotonNetwork.CurrentRoom.IsVisible && 
               !PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("MODDED");
    }

    private static void ExecuteProtection(VRRig reporterRig)
    {
        // Immediate disconnect
        NetworkSystem.Instance.ReturnToSinglePlayer();
        
        // Additional protection
        RPCProtection();
        
        // Notification
        string reporterName = GetPlayerFromVRRig(reporterRig).NickName;
        NotifiLib.SendNotification(
            $"<color=#FF00FF>⚠️ ANTI-REPORT TRIGGERED ⚠️</color>\n" +
            $"Blocked report attempt from: {reporterName}"
        );
    }

    // Helper Methods
    private static Photon.Realtime.Player GetPlayerFromVRRig(VRRig rig)
    {
        return rig.photonView?.Owner ?? PhotonNetwork.LocalPlayer;
    }

    private static void VisualizeAura(Vector3 position, float radius, Color color)
    {
        // Implementation for visualization sphere
        GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        aura.transform.position = position;
        aura.transform.localScale = Vector3.one * radius * 2;
        aura.GetComponent<Renderer>().material.color = new Color(color.r, color.g, color.b, 0.3f);
        GameObject.Destroy(aura, 0.5f);
    }

    private static void RPCProtection()
    {
        // Clears potentially malicious RPCs
        PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
        
        // Reset player properties
        Hashtable props = new Hashtable();
        props["protected"] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}