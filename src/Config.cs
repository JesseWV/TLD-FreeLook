using UnityEngine;

namespace FreeLook;

public enum IndicatorCorner { BottomRight, BottomLeft, TopRight, TopLeft }

public enum IndicatorVisibility { WhenLooking, WhenLatched, Never }

internal static class Config
{
    internal static bool EnableMod = true;

    internal static KeyCode ModifierKey = KeyCode.LeftAlt;

    internal static bool ToggleMode = false;

    internal static bool DoubleTapLatch = false;

    internal static float YawLimit = 155f;

    internal static float ReturnSpeed = 600f;

    internal static IndicatorVisibility ShowIcon = IndicatorVisibility.WhenLatched;

    internal static string IndicatorSprite = "ico_ToD_arrow";

    internal static string IndicatorSpriteOver = "ico_status_fatigue1";

    internal static float IconOverlayScale = 0.70f;

    internal static IndicatorCorner ScreenAnchor = IndicatorCorner.BottomRight;

    internal static float HorizontalOffset = 3.25f;
    internal static float VerticalOffset = 7.5f;

    internal static int IconSize = 50;

    internal static float IconOpacity = 1f;

    internal static bool DisableWhileAiming = true;

    internal static bool TurnToAim = true;

    internal static bool ShowHeldItem = false;

    internal static float HideBeyond = 180f;

    internal static bool DisableWhenEquipped = false;

    internal static bool DisableWhileCrouched = false;

    internal static bool Verbose = false;
}
