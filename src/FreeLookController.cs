using System;
using Il2Cpp;
using UnityEngine;

namespace FreeLook;

internal static class FreeLookController
{

    private static bool _requested;

    private static bool _latched;

    private const float DoubleTapWindow = 0.3f;

    private static float _lastTapTime = -1f;

    private static float _yawOffset;

    private static float _returnVelocity;

    private static bool _wasEngaged;

    private static float _entryPitch;

    private static bool _pitchReturning;

    private static float _pitchVelocity;

    internal static void Reset()
    {
        ShowArms();
        _weaponCam = null;
        _requested = false;
        _latched = false;
        _revealHoldUntil = 0f;
        _lastTapTime = -1f;
        _yawOffset = 0f;
        _returnVelocity = 0f;
        _wasEngaged = false;
        _pitchReturning = false;
        _pitchVelocity = 0f;
    }

    internal static void PollInput()
    {
        if (!Config.EnableMod)
        {

            if (_yawOffset != 0f || _latched || _requested || _maskCleared) Reset();
            return;
        }

        if (Config.ModifierKey == KeyCode.None)
        {
            _requested = false;
            return;
        }

        if (_latched && (Il2Cpp.GameManager.ControlsLocked()
                         || Il2Cpp.InterfaceManager.IsOverlayActiveImmediate()))
        {
            _latched = false;
            _lastTapTime = -1f;
        }

        if (Config.ToggleMode)
        {
            if (Input.GetKeyDown(Config.ModifierKey)) _latched = !_latched;
            _requested = _latched;
        }
        else if (Config.DoubleTapLatch)
        {
            if (Input.GetKeyDown(Config.ModifierKey))
            {
                if (_latched)
                {

                    _latched = false;
                    _lastTapTime = -1f;
                }
                else if (Time.unscaledTime - _lastTapTime <= DoubleTapWindow)
                {
                    _latched = true;
                    _lastTapTime = -1f;
                }
                else
                {
                    _lastTapTime = Time.unscaledTime;
                }
            }

            _requested = _latched || Input.GetKey(Config.ModifierKey);
        }
        else
        {
            _latched = false;
            _requested = Input.GetKey(Config.ModifierKey);
        }
    }

    private static bool ShouldEngage(vp_FPSCamera camera)
    {
        if (!Config.EnableMod || !_requested || camera == null) return false;

        if (camera.IsFreeCameraLookEnabled()) return false;

        if (Config.DisableWhileAiming && camera.IsZoomed) return false;

        if (Config.DisableWhenEquipped && !NothingEquipped()) return false;

        if (Config.DisableWhileCrouched && IsCrouching()) return false;

        return true;
    }

    internal static void DivertYaw(vp_FPSCamera camera, ref Vector2 input)
    {
        if (!ShouldEngage(camera)) return;

        float limit = Mathf.Max(0f, Config.YawLimit);
        _yawOffset = Mathf.Clamp(_yawOffset + input.x, -limit, limit);

        input.x = 0f;
    }

    internal static void ApplyToCamera(vp_FPSCamera camera)
    {
        if (camera == null) return;

        bool engaged = ShouldEngage(camera);

        TurnBodyToAim(camera, engaged);

        if (engaged)
        {

            _returnVelocity = 0f;
        }
        else if (_yawOffset != 0f)
        {
            if (Config.ReturnSeconds <= 0f)
            {
                _yawOffset = 0f;
                _returnVelocity = 0f;
            }
            else
            {

                _yawOffset = Mathf.SmoothDamp(_yawOffset, 0f, ref _returnVelocity,
                                              Config.ReturnSeconds, Mathf.Infinity, Time.unscaledDeltaTime);
                if (Mathf.Abs(_yawOffset) < 0.01f)
                {
                    _yawOffset = 0f;
                    _returnVelocity = 0f;
                }
            }
        }

        UpdateArms(camera, engaged);

        UpdateVerticalReturn(camera, engaged);

        if (Config.Verbose && engaged != _wasEngaged)
            Core.Log.Msg($"free look {(engaged ? "engaged" : "released")} (offset {_yawOffset:0.0} deg)");
        _wasEngaged = engaged;

        if (_yawOffset == 0f) return;

        Transform t = camera.transform;
        if (t == null) return;
        t.rotation = Quaternion.AngleAxis(_yawOffset, Vector3.up) * t.rotation;
    }

    private static bool IsCrouching()
    {
        var pm = Il2Cpp.GameManager.GetPlayerMovementComponent();
        return pm != null && pm.m_IsCrouching;
    }

    private static bool NothingEquipped()
    {
        var pm = Il2Cpp.GameManager.GetPlayerManagerComponent();
        return pm == null || pm.m_ItemInHands == null;
    }

    private static void UpdateVerticalReturn(vp_FPSCamera camera, bool engaged)
    {
        if (engaged)
        {

            if (!_wasEngaged)
            {
                _entryPitch = camera.m_Pitch;
                _pitchVelocity = 0f;
            }

            _pitchReturning = false;
            return;
        }

        if (_wasEngaged) _pitchReturning = true;

        if (!_pitchReturning) return;

        if (Config.ReturnSeconds <= 0f)
        {
            ApplyPitch(camera, _entryPitch);
            _pitchReturning = false;
            _pitchVelocity = 0f;
            return;
        }

        float pitch = Mathf.SmoothDamp(camera.m_Pitch, _entryPitch, ref _pitchVelocity,
                                       Config.ReturnSeconds, Mathf.Infinity, Time.unscaledDeltaTime);

        if (Mathf.Abs(pitch - _entryPitch) < 0.05f)
        {
            pitch = _entryPitch;
            _pitchReturning = false;
            _pitchVelocity = 0f;
        }

        ApplyPitch(camera, pitch);
    }

    private static void ApplyPitch(vp_FPSCamera camera, float pitch)
    {
        camera.m_Pitch = pitch;
        camera.m_TargetPitch = pitch;
    }

    private const float RevealAngle = 8f;

    private const float MaxLookAngle = 180f;

    private static void TurnBodyToAim(vp_FPSCamera camera, bool engaged)
    {
        if (engaged || !_wasEngaged || _yawOffset == 0f) return;
        if (!Config.TurnToAim || !Config.DisableWhileAiming) return;
        if (!_requested || !camera.IsZoomed) return;

        float committed = _yawOffset;

        camera.m_Yaw += committed;
        camera.m_TargetYaw += committed;
        camera.m_CurrentYaw += committed;

        _yawOffset = 0f;
        _returnVelocity = 0f;
        _entryPitch = camera.m_Pitch;

        Transform t = camera.transform;
        if (t != null) t.rotation = Quaternion.AngleAxis(committed, Vector3.up) * t.rotation;

        camera.SnapSprings();

        _revealHoldUntil = Time.unscaledTime + AimRevealHoldSeconds;
        if (Config.Verbose) Core.Log.Msg($"raised a weapon mid-look; turned the body {committed:0.0} deg to face it");
    }

    private const float AimRevealHoldSeconds = 0.035f;

    private static float _revealHoldUntil;

    private static bool WantHidden(vp_FPSCamera camera, bool engaged, bool active)
    {

        if (Time.unscaledTime < _revealHoldUntil) return true;

        if (!active) return false;

        if (camera.IsZoomed) return false;

        if (!engaged && Mathf.Abs(_yawOffset) <= RevealAngle) return false;

        if (NothingEquipped()) return true;

        if (!Config.ShowHeldItem) return true;

        if (Config.HideBeyond >= MaxLookAngle) return false;

        return Mathf.Abs(_yawOffset) >= Config.HideBeyond;
    }

    private static Camera _weaponCam;
    private static int _savedCullingMask;
    private static bool _maskCleared;

    private static void UpdateArms(vp_FPSCamera camera, bool engaged)
    {

        bool active = engaged || _yawOffset != 0f;
        bool wantHidden = WantHidden(camera, engaged, active);

        if (wantHidden == _maskCleared) return;

        if (wantHidden)
        {
            var wc = ResolveWeaponCamera(camera);
            if (wc == null) return;

            _savedCullingMask = wc.cullingMask;
            wc.cullingMask = 0;
            _maskCleared = true;

            if (Config.Verbose)
                Core.Log.Msg($"first-person meshes hidden (mask 0x{_savedCullingMask:X} stashed)");
        }
        else
        {

            if (_weaponCam != null) _weaponCam.cullingMask = _savedCullingMask;
            _maskCleared = false;
        }
    }

    private static Camera ResolveWeaponCamera(vp_FPSCamera camera)
    {
        if (_weaponCam != null) return _weaponCam;
        if (camera == null) return null;

        _weaponCam = camera.GetWeaponCamera();
        return _weaponCam;
    }

    private static void ShowArms()
    {
        if (!_maskCleared) return;
        if (_weaponCam != null) _weaponCam.cullingMask = _savedCullingMask;
        _maskCleared = false;
    }

}
