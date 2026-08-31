using System.Reflection;
using ModSettings;
using UnityEngine;

namespace FreeLook;

internal static class ModSettingsIntegration
{
    internal static void Register()
    {
        var s = Settings.Instance;
        s.AddToModSettings("Free Look");
        s.ApplyVisibility();
        s.Push();
        s.Save();
    }
}

internal sealed class Settings : JsonModSettings
{
    internal static Settings Instance { get; } = new Settings();

    [Section("General")]
    [Name("Enable free look")]
    [Description("Hold the modifier key to look around without turning your character. Turning this off restores stock camera behaviour entirely.")]
    public bool EnableMod = true;

    [Name("Free look key")]
    [Description("Held down to look around. Left Alt is the usual binding for this in other games. None = off.")]
    public KeyCode ModifierKey = KeyCode.LeftAlt;

    [Name("Toggle instead of hold")]
    [Description("Tap the key to enter free look and tap again to leave, rather than holding it down. Handy for long walks, but it is easy to forget you left it on.")]
    public bool ToggleMode = false;

    [Section("Feel")]
    [Name("Look range")]
    [Description("How far the view may swing from the direction you are walking, to each side. The default of 110 degrees is about as far as an average person can look without moving their shoulders - roughly 80 degrees of neck rotation plus 30 of eye movement.")]
    [Slider(15f, 180f, 166, NumberFormat = "{0:0}°")]
    public float YawLimit = 110f;

    [Name("Return time")]
    [Description("How long the view takes to swing back to the direction of travel after you release the key. Zero snaps back instantly.")]
    [Slider(0f, 300f, 7, NumberFormat = "{0:0} ms")]
    public float ReturnMilliseconds = 150f;

    [Name("Return vertical too")]
    [Description("Bring your view back to the height it was at when you started looking around, not just the direction. Off leaves the vertical wherever you left it.")]
    public bool ReturnVertical = true;

    [Name("Disable while aiming")]
    [Description("Suppress free look while a weapon is raised, so your aim is never pointed somewhere you are not looking. Recommended.")]
    public bool DisableWhileAiming = true;

    [Section("Arms")]
    [Name("Show held item while looking")]
    [Description("Keeps your held item on screen while you look around. It will look wrong: these models are only built for a forward view, so expect hollow cut edges, geometry ending in mid-air, and the camera passing inside the mesh. The angles below hide it before that point.")]
    public bool ShowHeldItem = false;

    [Name("Hide beyond")]
    [Description("How far you may look, either way, before the held item is hidden anyway.")]
    [Slider(0f, 180f, 181, NumberFormat = "{0:0}°")]
    public float HideBeyond = 55f;

    [Section("Locks")]
    [Name("No free look with an item equipped")]
    [Description("Stand down entirely whenever something is in your hands, rather than hiding it.")]
    public bool DisableWhenEquipped = false;

    [Name("No free look while crouched")]
    [Description("Stand down while crouched, where the view is already restricted.")]
    public bool DisableWhileCrouched = false;

    [Name("Show diagnostics")]
    [Description("Hidden gate - edit the settings JSON to enable.")]
    public bool ShowDiagnostics = false;

    [Section("Diagnostics")]
    [Name("Verbose logging")]
    [Description("Writes patch confirmation and free look state changes to the MelonLoader log. Off means the mod is silent.")]
    public bool Verbose = false;

    internal void Push()
    {
        Config.EnableMod = EnableMod;
        Config.ModifierKey = ModifierKey;
        Config.ToggleMode = ToggleMode;
        Config.YawLimit = Mathf.Round(YawLimit);
        Config.ReturnSeconds = Mathf.Round(ReturnMilliseconds) / 1000f;
        Config.ReturnVertical = ReturnVertical;
        Config.DisableWhileAiming = DisableWhileAiming;
        Config.ShowHeldItem = ShowHeldItem;
        Config.HideBeyond = Mathf.Round(HideBeyond);
        Config.DisableWhenEquipped = DisableWhenEquipped;
        Config.DisableWhileCrouched = DisableWhileCrouched;
        Config.Verbose = Verbose;
    }

    internal void ApplyVisibility()
    {
        SetFieldVisible(nameof(ShowDiagnostics), false);
        SetFieldVisible(nameof(Verbose), ShowDiagnostics);

        SetFieldVisible(nameof(HideBeyond), ShowHeldItem);
        RefreshGUI();
    }

    protected override void OnChange(FieldInfo field, object oldValue, object newValue)
    {
        ApplyVisibility();
        Push();

        if (field.Name == nameof(ModifierKey) || field.Name == nameof(ToggleMode)
            || field.Name == nameof(EnableMod))
            FreeLookController.Reset();
    }

    protected override void OnConfirm()
    {
        base.OnConfirm();
        Push();
    }
}
