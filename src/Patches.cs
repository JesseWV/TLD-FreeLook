using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace FreeLook;

internal static class PatchGuard
{

    private const int FaultLimit = 10;

    private const float FaultDecaySeconds = 10f;

    private static int _faults;
    private static float _lastFaultTime = float.NegativeInfinity;
    private static string _lastSite;
    private static string _lastType;

    internal static bool Faulted { get; private set; }

    internal static void Clear()
    {
        _faults = 0;
        _lastFaultTime = float.NegativeInfinity;
        _lastSite = null;
        _lastType = null;
        Faulted = false;
    }

    internal static void Failed(string site, System.Exception ex)
    {
        float now = UnityEngine.Time.unscaledTime;
        if (now - _lastFaultTime > FaultDecaySeconds) _faults = 0;
        _lastFaultTime = now;
        _faults++;

        string type = ex.GetType().Name;
        bool novel = site != _lastSite || type != _lastType;
        _lastSite = site;
        _lastType = type;

        if (_faults == 1 || novel)
            Core.Log.Error($"{site} threw {type}. Free look may misbehave; the mod stands down if " +
                           $"this keeps happening. {ex}");

        if (_faults < FaultLimit || Faulted) return;

        Faulted = true;
        Core.Log.Error($"{site} has thrown {_faults} times - standing the mod down for this scene. " +
                       "Everything it took is being put back.");

        try { FreeLookController.Reset(); }
        catch (System.Exception cleanup) { Core.Log.Error("stand-down also threw: " + cleanup); }
        try { Indicator.Reset(); }
        catch (System.Exception cleanup) { Core.Log.Error("icon teardown also threw: " + cleanup); }
    }
}

[HarmonyPatch(typeof(vp_FPSCamera), nameof(vp_FPSCamera.UpdateMouseLook))]
internal static class Patch_vp_FPSCamera_UpdateMouseLook
{
    private static void Prefix(vp_FPSCamera __instance, ref Vector2 input)
    {
        if (PatchGuard.Faulted) return;
        try { FreeLookController.DivertYaw(__instance, ref input); }
        catch (System.Exception ex) { PatchGuard.Failed("UpdateMouseLook prefix", ex); }
    }
}

[HarmonyPatch(typeof(vp_FPSCamera), nameof(vp_FPSCamera.DoLateUpdate))]
internal static class Patch_vp_FPSCamera_DoLateUpdate
{
    private static void Postfix(vp_FPSCamera __instance)
    {
        if (PatchGuard.Faulted) return;
        try { FreeLookController.ApplyToCamera(__instance); }
        catch (System.Exception ex) { PatchGuard.Failed("DoLateUpdate postfix", ex); }
    }
}

[HarmonyPatch(typeof(Panel_HUD), nameof(Panel_HUD.Update))]
internal static class Patch_Panel_HUD_Update
{
    private static void Postfix(Panel_HUD __instance)
    {
        if (PatchGuard.Faulted) return;
        try { Indicator.Refresh(__instance); }
        catch (System.Exception ex) { PatchGuard.Failed("Panel_HUD.Update postfix", ex); }
    }
}
