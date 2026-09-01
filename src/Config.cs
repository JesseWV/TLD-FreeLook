using UnityEngine;

namespace FreeLook;

public enum IndicatorCorner { BottomRight, BottomLeft, TopRight, TopLeft, Centre }

public enum IndicatorVisibility { Never, WhenLatched, WhenLooking }

internal static class Config
{
    internal static bool EnableMod = true;

    internal static KeyCode ModifierKey = KeyCode.LeftAlt;

    internal static bool ToggleMode = false;

    internal static bool DoubleTapLatch = false;

    internal static float YawLimit = 155f;

    internal static float ReturnSeconds = 0.15f;

    internal static IndicatorVisibility IndicatorMode = IndicatorVisibility.WhenLatched;

    internal static string IndicatorSprite = "ico_ToD_arrow";

    internal static string IndicatorSpriteOver = "ico_status_fatigue1";

    internal static float IndicatorOverScale = 0.70f;

    internal static IndicatorCorner IndicatorAnchor = IndicatorCorner.BottomRight;

    internal static float IndicatorOffsetX = -40f;
    internal static float IndicatorOffsetY = 54f;

    internal static int IndicatorSize = 50;

    internal static float IndicatorOpacity = 1f;

    internal static bool DisableWhileAiming = true;

    internal static bool TurnToAim = true;

    internal static bool ShowHeldItem = false;

    internal static float HideBeyond = 180f;

    internal static bool DisableWhenEquipped = false;

    internal static bool DisableWhileCrouched = false;

    internal static bool Verbose = false;
}
