using System;
using System.Linq;
using System.Reflection;
using FreeLook;
using MelonLoader;

[assembly: MelonInfo(typeof(Core), "FreeLook", "1.2.1", "Lycanthor")]
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

        WarnIfMissing("UpdateMouseLook");
        WarnIfMissing("DoLateUpdate");
    }

    private static void WarnIfMissing(string method)
    {

        bool found = typeof(Il2Cpp.vp_FPSCamera)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(m => m.Name == method);

        if (!found)
            Log.Error($"vp_FPSCamera.{method} not found - free look is NOT active. " +
                      "The game version is probably newer than this mod supports.");
        else if (Config.Verbose)
            Log.Msg($"patched vp_FPSCamera.{method}");
    }

    public override void OnUpdate() => FreeLookController.PollInput();

    public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
    {
        FreeLookController.Reset();

        Indicator.Reset();
    }
}
