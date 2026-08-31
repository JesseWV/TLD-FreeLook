using UnityEngine;

namespace FreeLook;

internal static class Config
{
    internal static bool EnableMod = true;

    internal static KeyCode ModifierKey = KeyCode.LeftAlt;

    internal static bool ToggleMode = false;

    internal static float YawLimit = 110f;

    internal static float ReturnSeconds = 0.15f;

    internal static bool ReturnVertical = true;

    internal static bool DisableWhileAiming = true;

    internal static bool ShowHeldItem = false;

    internal static float HideBeyond = 55f;

    internal static bool DisableWhenEquipped = false;

    internal static bool DisableWhileCrouched = false;

    internal static bool Verbose = false;
}
