// Fetches latest offsets from A2X on GitHub. (thank you btw :heart:)
// I mapped field names to possible dumper names for flexibility.
// Uses HTTP to download and regex to parse.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.COM.Surogate.Modules
{
    internal static class OffsetGrabber
    {
        private static readonly Dictionary<string, int> offsets = new Dictionary<string, int>();  // Temp storage for fetched offsets
        private static readonly HttpClient httpClient = new HttpClient();  // Client for downloads
        private const string OffsetsUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.cs";
        private const string ClientDllUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.cs";
        private const string ButtonsUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/buttons.cs";

        // Mapping from our field names to possible names in dumper files
        private static readonly Dictionary<string, List<string>> _fieldNameMapping = new Dictionary<string, List<string>>
        {
            { "dwViewMatrix", new List<string> { "dwViewMatrix" } },
            { "dwEntityList", new List<string> { "dwEntityList" } },
            { "dwLocalPlayerPawn", new List<string> { "dwLocalPlayerPawn" } },
            { "dwViewAngles", new List<string> { "dwViewAngles" } },
            { "dwGlobalVars", new List<string> { "dwGlobalVars" } },
            { "dwPlantedC4", new List<string> { "dwPlantedC4" } },
            { "dwGameRules", new List<string> { "dwGameRules" } },
            { "dwSensitivity", new List<string> { "dwSensitivity" } },
            { "dwSensitivity_sensitivity", new List<string> { "dwSensitivity_sensitivity" } },
            { "dwCSGOInput", new List<string> { "dwCSGOInput" } },
            { "attack", new List<string> { "attack", "+attack" } },
            { "jump", new List<string> { "jump", "+jump" } },
            { "m_pCameraServices", new List<string> { "m_pCameraServices", "m_CameraServices" } },
            { "m_iFOV", new List<string> { "m_iFOV", "m_iDesiredFOV" } },
            { "m_bIsScoped", new List<string> { "m_bIsScoped", "m_bIsScopedIn" } },
            { "m_iHealth", new List<string> { "m_iHealth" } },
            { "m_entitySpottedState", new List<string> { "m_entitySpottedState" } },
            { "m_bSpotted", new List<string> { "m_bSpotted", "m_bSpottedBy" } },
            { "m_iIDEntIndex", new List<string> { "m_iIDEntIndex" } },
            { "m_pSceneNode", new List<string> { "m_pSceneNode", "m_pRenderingNode" } },
            { "m_vecViewOffset", new List<string> { "m_vecViewOffset", "m_vViewOffset" } },
            { "m_lifeState", new List<string> { "m_lifeState" } },
            { "m_vOldOrigin", new List<string> { "m_vOldOrigin" } },
            { "m_iTeamNum", new List<string> { "m_iTeamNum" } },
            { "m_hPlayerPawn", new List<string> { "m_hPlayerPawn" } },
            { "m_flFlashBangTime", new List<string> { "m_flFlashBangTime", "m_flFlashDuration" } },
            { "m_modelState", new List<string> { "m_modelState" } },
            { "m_pGameSceneNode", new List<string> { "m_pGameSceneNode" } },
            { "m_flC4Blow", new List<string> { "m_flC4Blow", "m_flDetonateTime" } },
            { "current_time", new List<string> { "current_time", "m_flCurrentTime" } },
            { "m_bBombPlanted", new List<string> { "m_bBombPlanted", "m_bBombTicking" } },
            { "m_iszPlayerName", new List<string> { "m_iszPlayerName" } },
            { "m_pClippingWeapon", new List<string> { "m_pClippingWeapon" } },
            { "m_Item", new List<string> { "m_Item", "m_hActiveWeapon" } },
            { "m_iItemDefinitionIndex", new List<string> { "m_iItemDefinitionIndex" } },
            { "m_AttributeManager", new List<string> { "m_AttributeManager" } },
            { "m_bSpottedByMask", new List<string> { "m_bSpottedByMask" } },
            { "m_pWeaponServices", new List<string> { "m_pWeaponServices" } },
            { "m_hActiveWeapon", new List<string> { "m_hActiveWeapon" } },
            { "m_vecAbsVelocity", new List<string> { "m_vecAbsVelocity" } },
            { "m_fFlags", new List<string> { "m_fFlags" } },
            { "m_iShotsFired", new List<string> { "m_iShotsFired" } },
            { "dwLocalPlayerController", new List<string> { "dwLocalPlayerController" } }
            // Super easy to add more, just follow the same format.
        };

        public static async Task UpdateOffsets()  // Main update method
        {
            try
            {
                offsets.Clear();
                await DownloadAndParseFile(OffsetsUrl, ParseOffsetsFile);
                await DownloadAndParseFile(ClientDllUrl, ParseClientDllFile);
                await DownloadAndParseFile(ButtonsUrl, ParseButtonsFile);
                UpdateOffsetsClass();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
        }

        private static async Task DownloadAndParseFile(string url, Action<string> parseMethod)  // Download and parse helper
        {
            try
            {
                var response = await httpClient.GetStringAsync(url);
                parseMethod(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
        }

        private static void ParseOffsetsFile(string content)  // Parse offsets.cs
        {
            var matches = Regex.Matches(content, @"public (?:const|static) nint (\w+) = (0x[0-9A-Fa-f]+);");
            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                offsets[name] = value;
            }
        }

        private static void ParseClientDllFile(string content)  // Parse client_dll.cs
        {
            var matches = Regex.Matches(content, @"public (?:const|static) nint (\w+) = (0x[0-9A-Fa-f]+);");
            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                offsets[name] = value;
            }
        }

        private static void ParseButtonsFile(string content)  // Parse buttons.cs
        {
            var matches = Regex.Matches(content, @"public const nint ([\w\+]+) = (0x[0-9A-Fa-f]+);");
            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                int value = Convert.ToInt32(match.Groups[2].Value, 16);
                offsets[name] = value;
            }
        }

        private static void UpdateOffsetsClass()  // Apply fetched offsets to Offsets class
        {
            Type offsetsType = typeof(Microsoft.COM.Surogate.Data.Offsets);
            FieldInfo[] fields = offsetsType.GetFields(BindingFlags.Public | BindingFlags.Static);
            int updatedCount = 0;

            foreach (FieldInfo field in fields)
            {
                string fieldName = field.Name;
                if (_fieldNameMapping.TryGetValue(fieldName, out List<string> possibleNames))
                {
                    bool found = false;
                    foreach (string dumperName in possibleNames)
                    {
                        if (offsets.TryGetValue(dumperName, out int value))
                        {
                            int currentValue = (int)field.GetValue(null);
                            field.SetValue(null, value);
                            Console.WriteLine($"[i]: Updated {fieldName} from 0x{currentValue:X} to 0x{value:X}");
                            updatedCount++;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Console.WriteLine($"[ERROR]: No Offset Found for {fieldName} (tried: {string.Join(", ", possibleNames)})");
                    }
                }
                else
                {
                    Console.WriteLine($"[ERROR]: No Mapping for {fieldName}");
                }
            }
            Console.WriteLine($"[i]: Scan Complete, {updatedCount}/{fields.Length} offsets updated!");
        }
    }
}