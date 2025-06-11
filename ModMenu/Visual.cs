using UnityEngine;
using System.Text;
using GorillaLocomotion;
using Photon.Pun;
using System.Collections.Generic;

public class VisualMods : MonoBehaviour
{
    private StringBuilder menuText = new StringBuilder();
    private bool xrayEnabled = false;
    private bool noclipEnabled = false;
    private Dictionary<VRRig, GameObject> espBoxes = new Dictionary<VRRig, GameObject>();
    private Collider[] playerColliders;

    private void Update()
    {
        if (noclipEnabled)
        {
            NoClip();
        }

        if (xrayEnabled)
        {
            UpdateESP();
        }
        else
        {
            ClearESP();
        }
    }

    public void ShowMenu()
    {
        menuText.Clear();
        menuText.AppendLine("<color=#3380FF>=== VISUAL MODS ===</color>");
        menuText.AppendLine(xrayEnabled ? "<color=#00FF80>• X-Ray: ON</color>" : "<color=#FF4D4D>• X-Ray: OFF</color>");
        menuText.AppendLine(noclipEnabled ? "<color=#00FF80>• NoClip: ON</color>" : "<color=#FF4D4D>• NoClip: OFF</color>");
        menuText.AppendLine("<color=#AAAAAA>B:Back  X:Toggle</color>");
    }

    public void ToggleXRay()
    {
        xrayEnabled = !xrayEnabled;
        NotifiLib.SendNotification($"X-Ray {(xrayEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
        
        if (!xrayEnabled)
        {
            ClearESP();
        }
    }

    public void ToggleNoClip()
    {
        noclipEnabled = !noclipEnabled;
        NotifiLib.SendNotification($"NoClip {(noclipEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
        
        if (noclipEnabled)
        {
            playerColliders = Player.Instance.GetComponentsInChildren<Collider>();
        }
        UpdateClipColliders(!noclipEnabled);
    }

    private void NoClip()
    {
        if (Gamepad.current?.rightTrigger.ReadValue() > 0.5f || Input.GetKey(KeyCode.E))
        {
            foreach (var collider in playerColliders)
            {
                collider.enabled = false;
            }
        }
        else
        {
            foreach (var collider in playerColliders)
            {
                collider.enabled = true;
            }
        }
    }

    private void UpdateClipColliders(bool enabled)
    {
        if (playerColliders == null) return;
        
        foreach (var collider in playerColliders)
        {
            collider.enabled = enabled;
        }
    }

    private void UpdateESP()
    {
        // Find all players in the room
        VRRig[] allRigs = FindObjectsOfType<VRRig>();

        foreach (VRRig rig in allRigs)
        {
            // Skip local player
            if (rig.isMyPlayer) continue;

            // Create or update ESP box
            if (!espBoxes.ContainsKey(rig))
            {
                CreateESPBox(rig);
            }
            else
            {
                UpdateESPBoxPosition(rig, espBoxes[rig]);
            }
        }

        // Remove boxes for players who left
        List<VRRig> toRemove = new List<VRRig>();
        foreach (var kvp in espBoxes)
        {
            if (kvp.Key == null || !allRigs.Contains(kvp.Key))
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var rig in toRemove)
        {
            espBoxes.Remove(rig);
        }
    }

    private void CreateESPBox(VRRig rig)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(box.GetComponent<Collider>()); // We don't need physics
        
        // Set red transparent material
        Renderer renderer = box.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = new Color(1, 0, 0, 0.3f);
        renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        renderer.material.EnableKeyword("_ALPHABLEND_ON");
        renderer.material.renderQueue = 3000; // Transparent queue
        
        // Scale to fit player
        box.transform.localScale = new Vector3(0.5f, 1.8f, 0.5f);
        box.transform.SetParent(rig.transform, false);
        
        espBoxes.Add(rig, box);
    }

    private void UpdateESPBoxPosition(VRRig rig, GameObject box)
    {
        if (rig != null && box != null)
        {
            box.transform.position = rig.transform.position;
            box.transform.rotation = rig.transform.rotation;
        }
    }

    private void ClearESP()
    {
        foreach (var kvp in espBoxes)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        espBoxes.Clear();
    }
}