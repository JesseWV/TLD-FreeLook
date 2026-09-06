using System.Linq;
using System.Reflection;
using FreeLook;
using MelonLoader;

[assembly: MelonInfo(typeof(Core), "FreeLook", "1.5.0", "Lycanthor")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonOptionalDependencies("ModSettings")]

namespace FreeLook;

public class Core : MelonMod
{
    internal static Core Instance { get; private set; }
    internal static MelonLogger.Instance Log => Instance.LoggerInstance;

    public override void OnInitializeMelon()
    {
        Instance = this;

        LoggerInstance.Msg("build " + (Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "unknown"));

        bool modSettingsPresent = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.GetName().Name == "ModSettings");

        if (modSettingsPresent)
        {

            try
            {
                ModSettingsIntegration.Register();
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Could not load settings, continuing on defaults. " +
                                     "Deleting Mods/FreeLook.json will clear this. " + ex.Message);
            }
        }
        else
        {
            LoggerInstance.Msg("ModSettings not detected - running on defaults. " +
                               "Hold Left Alt to look around. Install ModSettings to rebind it.");
        }

    }

    public override void OnLateInitializeMelon()
    {
        ReportPatch(typeof(Il2Cpp.vp_FPSCamera), "UpdateMouseLook");
        ReportPatch(typeof(Il2Cpp.vp_FPSCamera), "DoLateUpdate");
        ReportPatch(typeof(Il2Cpp.Panel_HUD), "Update");
    }

    private void ReportPatch(Type type, string method)
    {

        var target = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == method);

        if (target == null)
        {
            LoggerInstance.Error($"{type.Name}.{method} not found - that part of the mod is NOT " +
                                 "active. The game version is probably newer than this mod supports.");
            return;
        }

        var info = HarmonyLib.Harmony.GetPatchInfo(target);
        bool ours = info != null &&
                    info.Prefixes.Concat(info.Postfixes).Any(p => p.owner == HarmonyInstance.Id);

        if (!ours)
            LoggerInstance.Error($"{type.Name}.{method} exists but carries no patch of ours - " +
                                 "that part of the mod is NOT active.");
        else if (Config.Verbose)
            LoggerInstance.Msg($"patched {type.Name}.{method}");
    }

    public override void OnUpdate()
    {
        if (PatchGuard.Faulted) return;
        try { FreeLookController.PollInput(); }
        catch (Exception ex) { PatchGuard.Failed("OnUpdate poll", ex); }
    }

    public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
    {
        FreeLookController.Reset();
        FreeLookController.ForgetMark();
        PatchGuard.Clear();

        Indicator.Reset();
    }
}
