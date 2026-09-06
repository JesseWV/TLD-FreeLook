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
    [Description("Look around without turning your character. Off restores stock camera behavior entirely.")]
    public bool EnableMod = true;

    [Name("Free look key")]
    [Description("Held down to look around. Left Alt is the usual binding for this in other games. None = off.")]
    public KeyCode ModifierKey = KeyCode.LeftAlt;

    [Name("Toggle instead of hold")]
    [Description("Tap to enter free look and tap again to leave, rather than holding. Easy to forget you left it on.")]
    public bool ToggleMode = false;

    [Name("Double tap to latch")]
    [Description("Holding still works, but a quick double tap latches free look on and another releases it. It is also the only way in on a controller, where the game's auto-walk button - left stick click - latches it too.")]
    public bool DoubleTapLatch = false;

    [Section("Feel")]
    [Name("Look range")]
    [Description("How far the view may swing from your direction of travel, to each side. 180 is straight behind you, about what neck, eyes and a twist from the hips can manage. The slider goes further for a view the body could not deliver.")]
    [Slider(15f, 270f, 256, NumberFormat = "{0:0}°")]
    public float YawLimit = 180f;

    [Name("Return speed")]
    [Description("How fast the view swings back once released, in degrees per second. A glance comes back promptly and a full swing takes proportionally longer. Zero snaps instantly.\n\nFor scale, a relaxed look around is about 150°/s and the fastest a person can swing their gaze is roughly 800°/s.")]
    [Slider(0f, 1200f, 41, NumberFormat = "{0:0}°/s")]
    public float ReturnSpeed = 600f;

    [Section("Focus View")]
    [Name("Enable focus view")]
    [Description("Hold a second key to zoom in on whatever you are looking at. By default it works only while you are already looking around, the two being halves of one gesture; the setting below frees it.")]
    public bool EnableFocus = true;

    [Name("Focus key")]
    [Description("Held down to zoom. The default is the middle mouse button, which the game itself leaves unbound - note that Unity calls it Mouse2, not Mouse3. Right mouse is not offered: it aims, throws a torch or flare, or switches a flashlight to high beam. None = off.")]
    public KeyCode FocusKey = KeyCode.Mouse2;

    [Name("Toggle focus instead of holding")]
    [Description("Tap to zoom in and tap again to come back out, rather than holding. It still ends by itself whenever you could not have started it, such as raising a weapon or opening a menu.")]
    public bool FocusToggle = false;

    [Name("Focus without free look")]
    [Description("Lets the focus key zoom on its own, with the view still facing the way you are walking. Off means it only works while you are already looking around.")]
    public bool FocusStandalone = false;

    [Name("Zoom amount")]
    [Description("How far focus view zooms in. The game's own zoom when aiming is about 1.43x, for reference. Look sensitivity drops by the same factor, so the view does not turn twitchy at the far end.")]
    [Slider(1f, 4f, 61, NumberFormat = "{0:0.00}x")]
    public float FocusZoom = 2f;

    [Name("Zoom time")]
    [Description("How long the zoom takes to arrive, and to leave again.\n\nWhen free look ENDS while you are still zoomed, the zoom ignores this and follows the view home instead, so the two arrive together.")]
    [Slider(0.25f, 2f, 36, NumberFormat = "{0:0.00}s")]
    public float FocusEase = 1f;

    [Name("Edge darkening")]
    [Description("Darkens the edges of the screen while you are focused. It is the game's own edge darkening turned up slightly, so the shape is one you already know, and it fades in and out with the zoom.")]
    public bool FxEdgeDarkening = true;

    [Name("Foveal blur")]
    [Description("Sharp in the middle of the screen and blurring gently toward the edges, the way your own vision works. How much of the screen blurs is fixed rather than adjustable.")]
    public bool FxFovealBlur = true;

    [Section("Arms")]
    [Name("Show held item while looking")]
    [Description("Keeps your held item on screen while you look around. It will look wrong: these models are built for a forward view only, so expect hollow cut edges, geometry ending in mid-air, and the camera passing inside the mesh.")]
    public bool ShowHeldItem = false;

    [Name("Hide beyond")]
    [Description("How far you may look, either way, before the held item is hidden after all. At 180 it is never hidden.")]
    [Slider(0f, 180f, 181, NumberFormat = "{0:0}°")]
    public float HideBeyond = 180f;

    [Section("Indicator Icon")]
    [Name("Show icon")]
    [Description("When the free look icon appears on the HUD:\nWhenever looking - key held or latched\nWhile latched - only when locked on\nNever - not at all")]
    [Choice("Whenever looking", "While latched", "Never")]
    public IndicatorVisibility ShowIcon = IndicatorVisibility.WhenLatched;

    [Name("Screen anchor")]
    [Description("Which corner the two offsets below are measured from. The icon keeps that place on any monitor or aspect ratio.")]
    public IndicatorCorner ScreenAnchor = IndicatorCorner.BottomRight;

    [Name("Horizontal offset")]
    [Description("Distance from that corner, as a percentage of the screen width. 0 is in the corner and 100 the opposite side. 50 here with 50 below is the middle of the screen; one at 0 and the other at 50 is a middle edge.")]
    [Slider(0f, 100f, 401, NumberFormat = "{0:0.00}%")]
    public float HorizontalOffset = 3.25f;

    [Name("Vertical offset")]
    [Description("Distance from that corner, as a percentage of the screen height.")]
    [Slider(0f, 100f, 401, NumberFormat = "{0:0.00}%")]
    public float VerticalOffset = 7.5f;

    [Name("Size")]
    [Description("Height of the icon on the game's interface, so it scales with the interface rather than with your resolution.")]
    [Slider(16f, 96f, 81, NumberFormat = "{0:0}")]
    public float IconSize = 50f;

    [Name("Opacity")]
    [Description("How strongly the icon is drawn. The game's own icons are slightly muted, so full strength stands out a little.")]
    [Slider(0.2f, 1f, 17, NumberFormat = "{0:0.00}")]
    public float IconOpacity = 1f;

    [Section("Locks")]
    [Name("Disable while aiming")]
    [Description("Stand down while a weapon is raised, so your aim is never pointed somewhere you are not looking. Recommended.")]
    public bool DisableWhileAiming = true;

    [Name("Turn to face your aim")]
    [Description("Raising a weapon turns you to face where you were looking. Off swings the view back instead, and you aim where your body was pointing.")]
    public bool TurnToAim = true;

    [Name("No free look with an item equipped")]
    [Description("Stand down entirely whenever something is in your hands, rather than hiding it.")]
    public bool DisableWhenEquipped = false;

    [Name("No free look while crouched")]
    [Description("Stand down while crouched, where the view is already restricted.")]
    public bool DisableWhileCrouched = false;

    [Name("Indicator overlay scale")]
    [Description("Hidden - edit the settings JSON.")]
    public float IconOverlayScale = 0.70f;

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
        Config.DoubleTapLatch = DoubleTapLatch;
        Config.YawLimit = Mathf.Round(YawLimit);
        Config.ReturnSpeed = Mathf.Round(ReturnSpeed);
        Config.DisableWhileAiming = DisableWhileAiming;
        Config.TurnToAim = TurnToAim;
        Config.ShowHeldItem = ShowHeldItem;
        Config.HideBeyond = Mathf.Round(HideBeyond);
        Config.DisableWhenEquipped = DisableWhenEquipped;
        Config.DisableWhileCrouched = DisableWhileCrouched;
        Config.EnableFocus = EnableFocus;
        Config.FocusKey = FocusKey;
        Config.FocusToggle = FocusToggle;
        Config.FocusStandalone = FocusStandalone;
        Config.FocusZoom = Round2(Mathf.Clamp(FocusZoom, 1f, 4f));

        Config.FocusEase = Round2(Mathf.Clamp(FocusEase, 0.25f, 2f));
        Config.FxEdgeDarkening = FxEdgeDarkening;
        Config.FxFovealBlur = FxFovealBlur;

        Config.ShowIcon = ShowIcon;
        Config.IconOverlayScale = IconOverlayScale;
        Config.ScreenAnchor = ScreenAnchor;
        Config.HorizontalOffset = HorizontalOffset;
        Config.VerticalOffset = VerticalOffset;
        Config.IconSize = Mathf.RoundToInt(IconSize);
        Config.IconOpacity = IconOpacity;
        Config.Verbose = Verbose;
    }

    private static float Round2(float v) => Mathf.Round(v * 100f) / 100f;

    internal void ApplyVisibility()
    {

        SetFieldVisible(nameof(IconOverlayScale), false);

        SetFieldVisible(nameof(ScreenAnchor), ShowIcon != IndicatorVisibility.Never);
        SetFieldVisible(nameof(HorizontalOffset), ShowIcon != IndicatorVisibility.Never);
        SetFieldVisible(nameof(VerticalOffset), ShowIcon != IndicatorVisibility.Never);
        SetFieldVisible(nameof(IconSize), ShowIcon != IndicatorVisibility.Never);
        SetFieldVisible(nameof(IconOpacity), ShowIcon != IndicatorVisibility.Never);

        SetFieldVisible(nameof(ShowDiagnostics), false);
        SetFieldVisible(nameof(Verbose), ShowDiagnostics);

        SetFieldVisible(nameof(HideBeyond), ShowHeldItem);

        SetFieldVisible(nameof(DoubleTapLatch), !ToggleMode);

        SetFieldVisible(nameof(TurnToAim), DisableWhileAiming);

        SetFieldVisible(nameof(FocusKey), EnableFocus);
        SetFieldVisible(nameof(FocusToggle), EnableFocus);
        SetFieldVisible(nameof(FocusStandalone), EnableFocus);
        SetFieldVisible(nameof(FocusZoom), EnableFocus);
        SetFieldVisible(nameof(FocusEase), EnableFocus);
        SetFieldVisible(nameof(FxEdgeDarkening), EnableFocus);
        SetFieldVisible(nameof(FxFovealBlur), EnableFocus);

        RefreshGUI();
    }

    protected override void OnChange(FieldInfo field, object oldValue, object newValue)
    {
        ApplyVisibility();
    }

    protected override void OnConfirm()
    {
        base.OnConfirm();
        Push();

        FreeLookController.Reset();
    }
}
