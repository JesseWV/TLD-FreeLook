using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace FreeLook;

[HarmonyPatch(typeof(vp_FPSCamera), nameof(vp_FPSCamera.UpdateMouseLook))]
internal static class Patch_vp_FPSCamera_UpdateMouseLook
{
    private static void Prefix(vp_FPSCamera __instance, ref Vector2 input)
    {
        FreeLookController.DivertYaw(__instance, ref input);
    }
}

[HarmonyPatch(typeof(vp_FPSCamera), nameof(vp_FPSCamera.DoLateUpdate))]
internal static class Patch_vp_FPSCamera_DoLateUpdate
{
    private static void Postfix(vp_FPSCamera __instance)
    {
        FreeLookController.ApplyToCamera(__instance);
    }
}

[HarmonyPatch(typeof(Panel_HUD), nameof(Panel_HUD.Update))]
internal static class Patch_Panel_HUD_Update
{
    private static void Postfix(Panel_HUD __instance)
    {
        Indicator.Refresh(__instance);
    }
}
