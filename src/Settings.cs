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
    [Description("Hold the modifier key to look around without turning your character. Turning this off restores stock camera behavior entirely.")]
    public bool EnableMod = true;

    [Name("Free look key")]
    [Description("Held down to look around. Left Alt is the usual binding for this in other games. None = off.")]
    public KeyCode ModifierKey = KeyCode.LeftAlt;

    [Name("Toggle instead of hold")]
    [Description("Tap the key to enter free look and tap again to leave, rather than holding it down. Handy for long walks, but it is easy to forget you left it on.")]
    public bool ToggleMode = false;

    [Name("Double tap to latch")]
    [Description("Holding still works as normal, but a quick double tap latches free look on, and another releases it. Useful on a long walk, and the only way to use free look on a controller: while a controller is in use, this mode lets the game's auto-walk button trigger free look too, which on a pad is the left stick click. Auto-walk is a toggle, so the two presses leave it exactly as they were.")]
    public bool DoubleTapLatch = false;

    [Section("Feel")]
    [Name("Look range")]
    [Description("How far the view may swing from the direction you are walking, to each side. The default of 180 degrees is looking straight behind you, which between neck, eyes and a twist from the hips is about what a person can manage. The slider goes further for anyone who wants a view the body could not actually deliver.")]
    [Slider(15f, 270f, 256, NumberFormat = "{0:0}°")]
    public float YawLimit = 180f;

    [Name("Return speed")]
    [Description("How fast the view swings back to the direction of travel after you release the key, in degrees per second. A quick glance comes back promptly and a full swing takes proportionally longer, the way a head turning back does. Zero snaps back instantly.\n\nFor a sense of scale, looking around at a relaxed pace runs somewhere around 150°/s, and the fastest a person can swing their gaze is roughly 800°/s. Past that the view is moving quicker than you could actually look.")]
    [Slider(0f, 1200f, 41, NumberFormat = "{0:0}°/s")]
    public float ReturnSpeed = 600f;

    [Name("Turn to face your aim")]
    [Description("Raising a weapon while looking around turns you to face where you were looking, instead of swinging your view back to where you were walking. Off means the view snaps back and you aim where your body was pointing.")]
    public bool TurnToAim = true;

    [Name("Disable while aiming")]
    [Description("Suppress free look while a weapon is raised, so your aim is never pointed somewhere you are not looking. Recommended.")]
    public bool DisableWhileAiming = true;

    [Section("Focus View")]
    [Name("Enable focus view")]
    [Description("While you are already looking around, hold a second key to zoom in on whatever you turned to see. It only works during free look: the two are meant as halves of one gesture, looking away from where you are walking and then concentrating on what you found.")]
    public bool EnableFocus = true;

    [Name("Focus key")]
    [Description("Held down to zoom, while free look is engaged. The default is the middle mouse button, which The Long Dark itself leaves unbound - note that Unity calls that one Mouse2, not Mouse3. Right mouse is deliberately not offered as the default: in this game it aims, throws a torch or a flare, or switches a flashlight to its faster-draining high beam, depending on what you are holding. None = off.")]
    public KeyCode FocusKey = KeyCode.Mouse2;

    [Name("Toggle focus instead of holding")]
    [Description("Tap the focus key to zoom in and tap again to come back out, rather than holding it. The zoom still ends the moment free look does, so it can never be left on by accident.")]
    public bool FocusToggle = false;

    [Name("Zoom Amount")]
    [Description("How far in the focus view zooms, as a magnification. The game's own zoom when aiming a weapon is about 1.43x, so that is a good reference for what a modest value looks like. Your look sensitivity is reduced by the same factor, so the view does not turn twitchy at the far end.")]
    [Slider(1f, 4f, 61, NumberFormat = "{0:0.00}x")]
    public float FocusZoom = 2f;

    [Name("Zoom Time")]
    [Description("How long the zoom takes to arrive, and to leave again, in seconds.\n\nThis applies whenever you work the focus key yourself. When free look ENDS while you are still zoomed, the zoom ignores this and follows the view's own journey home instead, so the two arrive together rather than one waiting on the other.")]
    [Slider(0.25f, 2f, 36, NumberFormat = "{0:0.00}s")]
    public float FocusEase = 1f;

    [Name("Edge darkening")]
    [Description("Darkens the edges of the screen while you are focused, so concentrating reads on screen and not only in how much you can see. It is the game's own edge darkening turned up slightly, so the shape is the one you are already used to, and it fades in and out with the zoom.")]
    public bool FxEdgeDarkening = true;

    [Name("Foveal blur")]
    [Description("Sharp in the middle of the screen, blurring gently toward the edges, the way your own vision works. Built from the game's own bokeh blur driven by screen position rather than by distance. The middle is always the sharp part, and how much of the screen blurs is fixed rather than adjustable.")]
    public bool FxFovealBlur = true;

    [Section("Arms")]
    [Name("Show held item while looking")]
    [Description("Keeps your held item on screen while you look around. It will look wrong: these models are only built for a forward view, so expect hollow cut edges, geometry ending in mid-air, and the camera passing inside the mesh. The angles below hide it before that point.")]
    public bool ShowHeldItem = false;

    [Name("Hide beyond")]
    [Description("How far you may look, either way, before the held item is hidden after all. At 180 it is never hidden. Lower it if you would rather the incomplete meshes were taken away once you look far enough round.")]
    [Slider(0f, 180f, 181, NumberFormat = "{0:0}°")]
    public float HideBeyond = 180f;

    [Section("Indicator Icon")]
    [Name("Show Icon")]
    [Description("When to show the free look icon on the HUD:\nWhenever looking - when the key is held or latched\nWhile latched - only when free look is locked on with a latching option\nNever - do not show the icon at all")]
    [Choice("Whenever looking", "While latched", "Never")]
    public IndicatorVisibility ShowIcon = IndicatorVisibility.WhenLatched;

    [Name("Screen Anchor")]
    [Description("Which corner the two offsets below are measured from. The icon keeps the same place relative to that corner on any monitor or aspect ratio.")]
    public IndicatorCorner ScreenAnchor = IndicatorCorner.BottomRight;

    [Name("Horizontal Offset")]
    [Description("How far away from that corner, as a percentage of the screen width. 0 in the specified corner, and 100 the opposite one. Round numbers reach the obvious places: 50 here and 50 below is the middle of the screen. One at 0 and the other at 50 is a middle edge.")]
    [Slider(0f, 100f, 401, NumberFormat = "{0:0.00}%")]
    public float HorizontalOffset = 3.25f;

    [Name("Vertical Offset")]
    [Description("How far away from that corner, as a percentage of the screen height. 0 in the specified corner, and 100 the opposite one.")]
    [Slider(0f, 100f, 401, NumberFormat = "{0:0.00}%")]
    public float VerticalOffset = 7.5f;

    [Name("Size")]
    [Description("Height of the icon on the game's interface, so it scales with the interface rather than with your resolution.")]
    [Slider(16f, 96f, 81, NumberFormat = "{0:0}")]
    public float IconSize = 50f;

    [Name("Opacity")]
    [Description("How strongly the icon is drawn. The game's own icons are slightly muted, so full strength makes this one stand out a little.")]
    [Slider(0.2f, 1f, 17, NumberFormat = "{0:0.00}")]
    public float IconOpacity = 1f;

    [Section("Locks")]
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
