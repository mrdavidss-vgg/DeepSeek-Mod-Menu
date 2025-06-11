using UnityEngine;
using Photon.Pun;

public class OverpoweredMods : MonoBehaviour 
{
    // OP Mod Toggles
    public static bool timeFreezeEnabled = false;
    private static float timeScale = 1f;

    public static void Update()
    {
        if (timeFreezeEnabled && (Gamepad.current?.rightTrigger.wasPressedThisFrame ?? false))
        {
            ToggleTimeFreeze();
        }
    }

    // === Time Freeze ===
    private static void ToggleTimeFreeze()
    {
        timeScale = (timeScale == 1f) ? 0.1f : 1f; // 10% speed when frozen
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * timeScale;
        
        // Visual feedback
        foreach (var player in GameObject.FindObjectsOfType<GorillaLocomotion.Player>())
        {
            var particles = player.GetComponentInChildren<ParticleSystem>();
            if (timeScale < 1f) particles.Play();
            else particles.Stop();
        }
    }
}