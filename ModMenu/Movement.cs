using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public static class Movement
{
    // Configuration
    private const float SuperSpeedBoost = 2.5f;
    private const float MiniSpeedBoost = 1.5f;
    private const float JumpMultiplier = 1.8f;
    private const float DashSpeed = 8f;
    private const float BranchSpeed = 10f;
    private const float BranchDistance = 5f;
    private const float PlatformSize = 0.5f;
    private const float PlatformHeight = 0.1f;

    // State
    private static float originalSpeed;
    private static float originalJump;
    private static bool speedModified = false;
    private static bool lastDashInput = false;
    private static bool lastOnBranch = false;
    private static Vector3 leftPos = Vector3.zero;
    private static Vector3 rightPos = Vector3.zero;
    private static List<Vector3> branchPositions = new List<Vector3>();
    private static List<GameObject> spawnedPlatforms = new List<GameObject>();
    private static Material platformMaterial;

    // Input
    private static bool rightPrimary => Gamepad.current?.rightShoulder.isPressed ?? false;
    private static bool rightGrab => Gamepad.current?.rightGrip.isPressed ?? false;
    private static bool leftGrab => Gamepad.current?.leftGrip.isPressed ?? false;

    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void Initialize()
    {
        originalSpeed = Player.Instance.maxJumpSpeed;
        originalJump = Player.Instance.jumpMultiplier;
        CacheBranchPositions();
        
        // Create blue platform material
        platformMaterial = new Material(Shader.Find("Standard"));
        platformMaterial.color = new Color(0.2f, 0.4f, 1f, 0.8f);
        platformMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        platformMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        platformMaterial.EnableKeyword("_ALPHABLEND_ON");
        platformMaterial.renderQueue = 3000;
    }

    public static void Update()
    {
        HandleSpeed();
        HandleDash();
        HandleAutoBranch();
        HandlePlatformSpawning();
    }

    #region Speed System
    public static void ApplySuperSpeed()
    {
        if (speedModified) return;
        
        Player.Instance.maxJumpSpeed = originalSpeed * SuperSpeedBoost;
        Player.Instance.jumpMultiplier = JumpMultiplier;
        speedModified = true;
        
        NotifiLib.SendNotification(
            "<color=#00FF00>SUPER SPEED ACTIVATED!</color>\n" +
            $"Movement: {SuperSpeedBoost}x\n" +
            $"Jump: {JumpMultiplier}x"
        );
    }

    public static void ApplyMiniSpeed()
    {
        if (speedModified) return;
        
        Player.Instance.maxJumpSpeed = originalSpeed * MiniSpeedBoost;
        Player.Instance.jumpMultiplier = JumpMultiplier * 0.8f;
        speedModified = true;
        
        NotifiLib.SendNotification(
            "<color=#55FF55>Mini Speed Activated</color>\n" +
            $"Movement: {MiniSpeedBoost}x\n" +
            $"Jump: {JumpMultiplier * 0.8f}x"
        );
    }

    public static void ResetSpeed()
    {
        if (!speedModified) return;
        
        Player.Instance.maxJumpSpeed = originalSpeed;
        Player.Instance.jumpMultiplier = originalJump;
        speedModified = false;
        
        NotifiLib.SendNotification("<color=#FF5555>Speed Reset</color>");
    }

    private static void HandleSpeed()
    {
        if (!speedModified) return;
        // Optional speed decay logic here
    }
    #endregion

    #region Dash System
    private static void HandleDash()
    {
        bool currentDash = rightPrimary;
        if (currentDash && !lastDashInput)
        {
            Player.Instance.GetComponent<Rigidbody>().AddForce(
                Player.Instance.headCollider.transform.forward * DashSpeed,
                ForceMode.VelocityChange
            );
            NotifiLib.SendNotification("<color=#FFAA00>DASH!</color>");
        }
        lastDashInput = currentDash;
    }
    #endregion

    #region Auto-Branch System
    private static void CacheBranchPositions()
    {
        branchPositions = new List<Vector3>
        {
            new Vector3(-2.383f, 3.784f, 0.738f),
            new Vector3(1.55f, 5.559f, -1.56f),
            // ... (all other branch positions)
        };
    }

    private static void HandleAutoBranch()
    {
        if (!rightGrab)
        {
            leftPos = Player.Instance.leftHandTransform.position;
            rightPos = Player.Instance.rightHandTransform.position;
            if (lastOnBranch) UpdatePlayerColliders(true);
            lastOnBranch = false;
            return;
        }

        bool isOnBranch = false;
        Vector3 bodyPos = Player.Instance.bodyCollider.transform.position;

        // Right hand branch
        Vector3 rightBranch = FindClosestBranch(bodyPos, 1);
        if (rightBranch != Vector3.zero)
        {
            isOnBranch = true;
            rightPos = Vector3.Lerp(rightPos, rightBranch, BranchLerpTime);
        }
        else
        {
            rightPos = Vector3.Lerp(rightPos, Player.Instance.rightHandTransform.position, BranchLerpTime);
        }

        // Left hand branch
        Vector3 leftBranch = FindClosestBranch(bodyPos, -1);
        if (leftBranch != Vector3.zero)
        {
            isOnBranch = true;
            leftPos = Vector3.Lerp(leftPos, leftBranch, BranchLerpTime);
        }
        else
        {
            leftPos = Vector3.Lerp(leftPos, Player.Instance.leftHandTransform.position, BranchLerpTime);
        }

        // Apply positions and movement
        Player.Instance.rightHandTransform.position = rightPos;
        Player.Instance.leftHandTransform.position = leftPos;

        if (isOnBranch)
        {
            Player.Instance.GetComponent<Rigidbody>().velocity = 
                Player.Instance.headCollider.transform.forward * BranchSpeed;
            if (!lastOnBranch) UpdatePlayerColliders(false);
        }

        lastOnBranch = isOnBranch;
    }

    private static Vector3 FindClosestBranch(Vector3 position, float direction)
    {
        Vector3 closest = Vector3.zero;
        float minDistance = float.MaxValue;

        foreach (Vector3 branch in branchPositions)
        {
            float distance = Vector3.Distance(position, branch);
            float dot = Vector3.Dot(branch - position, Player.Instance.bodyCollider.transform.right) * direction;
            
            if (distance < minDistance && distance < BranchDistance && dot > 0)
            {
                minDistance = distance;
                closest = branch;
            }
        }
        return closest;
    }
    #endregion

    #region Platform System
    private static void HandlePlatformSpawning()
    {
        if (rightGrab) CreatePlatform(Player.Instance.rightHandTransform);
        if (leftGrab) CreatePlatform(Player.Instance.leftHandTransform);
        
        // Cleanup old platforms
        while (spawnedPlatforms.Count > 20)
        {
            GameObject oldest = spawnedPlatforms[0];
            spawnedPlatforms.RemoveAt(0);
            if (oldest != null) GameObject.Destroy(oldest);
        }
    }

    private static void CreatePlatform(Transform hand)
    {
        Vector3 spawnPos = hand.position;
        spawnPos.y = Mathf.Max(spawnPos.y - PlatformHeight/2, 0.1f);

        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.position = spawnPos;
        platform.transform.localScale = new Vector3(PlatformSize, PlatformHeight, PlatformSize);
        
        platform.GetComponent<Renderer>().material = platformMaterial;
        platform.GetComponent<Collider>().isTrigger = false;
        
        GameObject.Destroy(platform, 10f);
        spawnedPlatforms.Add(platform);
    }
    #endregion

    private static void UpdatePlayerColliders(bool enabled)
    {
        foreach (Collider col in Player.Instance.GetComponentsInChildren<Collider>())
        {
            col.enabled = enabled;
        }
    }
}