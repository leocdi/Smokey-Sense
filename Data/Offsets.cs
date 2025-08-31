// This is all the offsets used for reading memory from the CS2.
// These offsets are automatically updated on startup by fetching the latest values from a public GitHub repository (see OffsetGrabber.cs).
// I've structured this as a static class for easy access throughout the project.
// All offsets start at 0x00 and get populated dynamically.

using System;

namespace Microsoft.COM.Surogate.Data
{
    public static class Offsets
    {
        // Camera-related offsets
        public static int m_pCameraServices = 0x00;  // Pointer to camera services structure
        public static int m_iFOV = 0x00;  // Field of view value
        public static int m_bIsScoped = 0x00;  // Flag indicating if the player is scoped in

        // Player health and visibility
        public static int m_iHealth = 0x00;  // Player's current health
        public static int m_entitySpottedState = 0x00;  // State for entity spotting
        public static int m_bSpotted = 0x00;  // Whether the entity is spotted
        public static int m_iIDEntIndex = 0x00;  // Index for entity ID

        // Scene and view related
        public static int m_pSceneNode = 0x00;  // Pointer to scene node
        public static int dwViewMatrix = 0x00;  // View matrix for world to screen calculations
        public static int m_vecViewOffset = 0x00;  // View offset vector
        public static int dwViewAngles = 0x00;  // View angles

        // Player state
        public static int m_lifeState = 0x00;  // Life state (alive/dead)
        public static int m_vOldOrigin = 0x00;  // Old origin position
        public static int m_iTeamNum = 0x00;  // Team number
        public static int m_hPlayerPawn = 0x00;  // Handle to player pawn
        public static int dwLocalPlayerPawn = 0x00;  // Local player pawn address

        // Global game pointers
        public static int dwEntityList = 0x00;  // Entity list pointer
        public static int m_flFlashBangTime = 0x00;  // Flashbang effect time
        public static int m_modelState = 0x00;  // Model state
        public static int m_pGameSceneNode = 0x00;  // Game scene node pointer
        public static int dwCSGOInput = 0x00;  // CSGO input pointer

        // Bomb-related
        public static int m_flC4Blow = 0x00;  // C4 blow time
        public static int current_time = 0x00;  // Current game time
        public static int dwGlobalVars = 0x00;  // Global variables pointer
        public static int dwPlantedC4 = 0x00;  // Planted C4 pointer
        public static int m_bBombPlanted = 0x00;  // Bomb planted flag
        public static int dwGameRules = 0x00;  // Game rules pointer

        // Sensitivity and player name
        public static int dwSensitivity_sensitivity = 0x00;  // Sensitivity value offset
        public static int dwSensitivity = 0x00;  // Sensitivity pointer
        public static int m_iszPlayerName = 0x00;  // Player name string

        // Weapon-related
        public static int m_pClippingWeapon = 0x00;  // Clipping weapon pointer
        public static int m_Item = 0x00;  // Item pointer
        public static int m_iItemDefinitionIndex = 0x00;  // Item definition index
        public static int m_AttributeManager = 0x00;  // Attribute manager

        // Input buttons
        public static int attack = 0x00;  // Attack button
        public static int jump = 0x00;  // Jump button

        // Spotted by mask and weapon services
        public static int m_bSpottedByMask = 0x00;  // Spotted by mask
        public static int m_pWeaponServices = 0x00;  // Weapon services pointer
        public static int m_hActiveWeapon = 0x00;  // Active weapon handle

        // Velocity and flags
        public static int m_vecAbsVelocity = 0x00;  // Absolute velocity
        public static int m_fFlags = 0x00;  // Flags
        public static int m_iShotsFired = 0x00;  // Shots fired count

        // Local player controller
        public static int dwLocalPlayerController = 0x00;  // Local player controller pointer
    }
}