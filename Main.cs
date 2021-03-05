using System;
using System.Collections;
using MelonLoader;
using UnityEngine;
using VRC.SDKBase;
using Harmony;
using System.Linq;
using System.Reflection;

namespace NoOutlines
{
    public class NoOutlines : MelonMod
    {
        public const string Pref_CategoryName = "NoOutlines";
        public bool Pref_DisableOutlines = false;
        public bool Pref_DisableBeam = false;
        public bool Pref_DebugOutput = false;

        private const string defaultHighlightMaterialName = "Hidden/VRChat/SelectionHighlight";
        private const string defaultHighlightShaderName = "Hidden/VRChat/SelectionHighlight";
        private const string replacementHighlightShaderName = "VRChat/Invisible"; 

        private HighlightsFX highlightsObject = null;
        private Material     highlightMaterial;
        private string       highlightMaterialName = "";
        private Shader       highlightShader; 
        private string       highlightShaderName = "";

        // VRCPlayer[Local] ... /AnimationController/HeadAndHandIK/RightEffector/PickupTether(Clone)
        private GameObject leftHandTether = null;
        private GameObject rightHandTether = null;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory(Pref_CategoryName);
            MelonPreferences.CreateEntry(Pref_CategoryName, nameof(Pref_DisableOutlines),   false,  "Disable selection outlines");
            MelonPreferences.CreateEntry(Pref_CategoryName, nameof(Pref_DisableBeam),       false,  "Disable selection beams");
            MelonPreferences.CreateEntry(Pref_CategoryName, nameof(Pref_DebugOutput),       false,  "Enable debug output");
            MelonLogger.Msg("Initialized!");
        }

        // Skip over initial loading of (buildIndex, sceneName): [(0, "app"), (1, "ui")]
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            switch (buildIndex) {
                case 0: 
                    break; 
                case 1: 
                    break;  
                default:
                    ApplyAllSettings();
                    break;
            }
        }

        public override void OnPreferencesSaved()
        {
            ApplyAllSettings();
        }

        private void ApplyAllSettings()
        {
            UpdatePreferences();
            ClearAllReferences(); // might be unnecessary to clear first
            SetHighlightsFXReferences();
            if (ValidateHighlightsFXReferences())
            {
                ApplyOutlineVisibilitySettings();
            }

            // This might still throw an error if player is chaanging settings on world join and before their avatar loads
            MelonCoroutines.Start(WaitUntilPlayerIsLoadedToApplyTetherSettings()); // Replaced with hook to OnAvatarInstantiated
        }

        private void ApplyOutlineVisibilitySettings()
        {
            if (Pref_DisableOutlines)
            {
                LogDebugMsg("Disabling outlines.");
                highlightsObject.field_Protected_Shader_0 = UnityEngine.Shader.Find("Sprites/Mask");
            }
            else
            {
                LogDebugMsg("Enabling outlines.");
                highlightsObject.field_Protected_Shader_0 = UnityEngine.Shader.Find(defaultHighlightShaderName);
            }
        }

        private void ApplyBeamVisibilitySettings()
        {
            // Get local player transform
            if(Pref_DisableBeam)
            {
                LogDebugMsg("Disabling beams.");
            }
            else
            {
                LogDebugMsg("Enabling beams.");
            }
            leftHandTether.SetActive(!Pref_DisableBeam);
            rightHandTether.SetActive(!Pref_DisableBeam);
        }

        private void UpdatePreferences()
        {
            Pref_DisableOutlines   = MelonPreferences.GetEntryValue<bool>(Pref_CategoryName, nameof(Pref_DisableOutlines));
            Pref_DisableBeam       = MelonPreferences.GetEntryValue<bool>(Pref_CategoryName, nameof(Pref_DisableBeam));
            Pref_DebugOutput       = MelonPreferences.GetEntryValue<bool>(Pref_CategoryName, nameof(Pref_DebugOutput));
        }

        private bool ValidateHighlightsFXReferences()
        {
            return (highlightsObject != null && highlightMaterialName.Length > 0 && highlightShaderName.Length > 0);
        }

        private IEnumerator WaitUntilPlayerIsLoadedToApplyTetherSettings()
        {
            // Apply settings only when player is valid and tethers exist
            while(VRCPlayer.field_Internal_Static_VRCPlayer_0 == null)
            {
                yield return new WaitForSeconds(0.2f);
            }
            // Avatar is guaranteed to be loaded here
            ApplyTetherSettings();
        }

        // Call only when player avatar is loaded in
        private void ApplyTetherSettings()
        {
            SetTetherReferences();
            if (ValidateTetherReferences())
            {
                ApplyBeamVisibilitySettings();
            }
            LogDebugMsg("Tether settings applied.");
        }

        private bool ValidateTetherReferences()
        {
            return (leftHandTether != null  && rightHandTether != null);
        }

        private void SetTetherReferences()
        {
            try 
            {
                VRCPlayer player = VRCPlayer.field_Internal_Static_VRCPlayer_0; // is not null
                leftHandTether  = GameObject.Find(player.gameObject.name + "/AnimationController/HeadAndHandIK/LeftEffector/PickupTether(Clone)/Tether/Quad").gameObject;
                rightHandTether = GameObject.Find(player.gameObject.name + "/AnimationController/HeadAndHandIK/RightEffector/PickupTether(Clone)/Tether/Quad").gameObject;
            }
            catch(Exception e)
            {
                MelonLogger.Error(e.ToString());
            }
            finally
            {
                if(ValidateTetherReferences())
                {
                    LogDebugMsg("Found tethers: " + leftHandTether.name + "," + rightHandTether.name);
                }
                else
                {
                    MelonLogger.Error("Error finding tether references!");
                }
            }
        }
        private void SetHighlightsFXReferences()
        {
            try 
            {
                highlightsObject = HighlightsFX.prop_HighlightsFX_0;
                highlightMaterial= highlightsObject.field_Protected_Material_0;
                highlightMaterialName = highlightMaterial.name;
                highlightShader = highlightsObject.field_Protected_Shader_0;
                highlightShaderName = highlightShader.name;
            }
            catch(Exception e)
            {
                MelonLogger.Error(e.ToString());
            }
            finally
            {
                if(ValidateHighlightsFXReferences())
                {
                    LogDebugMsg("Found HighlightsFX Object: "          + highlightsObject.name);
                    LogDebugMsg("HighlightsFX Material Name: "         + highlightMaterialName);
                    LogDebugMsg("HighlightsFX Shader Name: "           + highlightShaderName);
                }
                else
                {
                    MelonLogger.Error("Error finding HighlightsFX references!");
                }
            }
        }
        private void ClearAllReferences()
        {
            LogDebugMsg("Clearing object references.");
            highlightsObject = null;
            leftHandTether = null;
            rightHandTether = null;

            //   Not necessary to clear these fields since they won't be changing
            // highlightMaterial= highlightsObject.field_Protected_Material_0;
            // highlightMaterialName = highlightsObject.field_Protected_Material_0.name;
            // highlightShader= highlightsObject.field_Protected_Shader_0; 
            // highlightShaderName = highlightsObject.field_Protected_Shader_0.name; 
        }

        private void LogDebugMsg(string msg)
        {
            if (!Pref_DebugOutput)
            {
                return; 
            }
            MelonLogger.Msg(msg);
        }

        // Most/all of this section comes from DynamicBonesSafety mod by Ben
        // https://github.com/BenjaminZehowlt/DynamicBonesSafety/
        // Thanks to loukylor for pointing me in the right direction
        #region HOOK_AVATAR_INSTANTIATED 

        public override void VRChat_OnUiManagerInit()
        {
            MelonLogger.Msg("Hooking OnAvatarInstantiated");
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("NoOutlinesMod");
            typeof(VRCPlayer).GetMethods().Where(m => m.Name.StartsWith("Method_Private_Void_GameObject_VRC_AvatarDescriptor_Boolean_") && !m.checkXref("Avatar is Ready, Initializing")).ToList()
            .ForEach(m => harmonyInstance.Patch(m, postfix: new HarmonyMethod(typeof(NoOutlines).GetMethod("AvatarFinishedLoadingPostfix", BindingFlags.NonPublic))));
        }

        [HarmonyPostfix]
        private void AvatarFinishedLoadingPostfix(VRCPlayer __instance, GameObject __0, VRC_AvatarDescriptor __1, bool __2)
        {
            if (__instance == null
                || __0 == null
                || __1 == null
                || !__2)
                return;

            // Not local player
            if (__instance != VRCPlayer.field_Internal_Static_VRCPlayer_0)
                return;

            LogDebugMsg("Avatar is loaded! Applying tether settings...");
            // Apply tether settings here
            MelonCoroutines.Start(WaitUntilPlayerIsLoadedToApplyTetherSettings());
        }
        #endregion
        
    }
}