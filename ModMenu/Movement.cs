using GorillaLocomotion;
using UnityEngine;

public static class Movement
{
    // Speed configurations
    private const float SuperSpeedBoost = 2.5f;  // 250% speed
    private const float MiniSpeedBoost = 1.5f;   // 150% speed
    private const float JumpMultiplier = 1.8f;   // Jump enhancement

    private static float originalSpeed;
    private static float originalJump;
    private static bool speedModified = false;

    public static void Initialize()
    {
        // Store original values
        originalSpeed = Player.Instance.maxJumpSpeed;
        originalJump = Player.Instance.jumpMultiplier;
    }

    public static void ApplySuperSpeed()
    {
        if (!speedModified)
        {
            Player.Instance.maxJumpSpeed = originalSpeed * SuperSpeedBoost;
            Player.Instance.jumpMultiplier = JumpMultiplier;
            speedModified = true;
            
            NotifiLib.SendNotification(
                "<color=#00FF00>SUPER SPEED ACTIVATED!</color>\n" +
                $"Movement: {SuperSpeedBoost}x\n" +
                $"Jump: {JumpMultiplier}x"
            );
        }
    }

    public static void ApplyMiniSpeed()
    {
        if (!speedModified)
        {
            Player.Instance.maxJumpSpeed = originalSpeed * MiniSpeedBoost;
            Player.Instance.jumpMultiplier = JumpMultiplier * 0.8f; // Slightly lower jump
            speedModified = true;
            
            NotifiLib.SendNotification(
                "<color=#55FF55>Mini Speed Activated</color>\n" +
                $"Movement: {MiniSpeedBoost}x\n" +
                $"Jump: {JumpMultiplier * 0.8f}x"
            );
        }
    }

    public static void ResetSpeed()
    {
        if (speedModified)
        {
            Player.Instance.maxJumpSpeed = originalSpeed;
            Player.Instance.jumpMultiplier = originalJump;
            speedModified = false;
            
            NotifiLib.SendNotification(
                "<color=#FF5555>Speed Reset</color>\n" +
                "Back to normal movement"
            );
        }
    }

    // Auto-reset when mod is disabled
    public static void Update()
    {
        if (!speedModified) return;
        
        // Optional: Add gradual speed decay here if desired
        // Player.Instance.maxJumpSpeed = Mathf.Lerp(Player.Instance.maxJumpSpeed, originalSpeed, 0.1f);
    }
}