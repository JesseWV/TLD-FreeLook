using Il2Cpp;
using UnityEngine;

namespace FreeLook;

internal static class FreeLookController
{

    private static bool _requested;

    private static bool _latched;

    private const float DoubleTapWindow = 0.3f;

    private static float _lastTapTime = -1f;

    private static bool _wasDown;

    private static vp_FPSCamera _pollContext;

    private static float _yawOffset;

    private static bool _wasEngaged;

    private static float _entryPitch;

    private static bool _returning;
    private static float _returnElapsed;
    private static float _returnDuration;
    private static float _returnYaw0;
    private static float _returnPitch0;

    private static float _lastYaw;

    private static float _opposedAccum;

    private static bool _focusRequested;
    private static bool _focusLatched;
    private static bool _focusWasDown;

    private static float _focusFactor;

    internal static void Reset()
    {
        ShowArms();
        RestoreFieldOfView();
        FocusEffect.Destroy();
        _focusRequested = false;
        _focusLatched = false;
        _focusWasDown = false;
        _focusFactor = 0f;
        _weaponCam = null;
        _requested = false;
        _latched = false;
        _revealHoldUntil = 0f;
        _lastTapTime = -1f;
        _yawOffset = 0f;
        _wasEngaged = false;
        _indicatorEngaged = false;
        _indicatorLatched = false;
        _returning = false;
        _returnElapsed = 0f;
        _returnDuration = 0f;
        _returnYaw0 = 0f;
        _returnPitch0 = 0f;
        _lastYaw = float.NaN;
        _opposedAccum = 0f;
    }

    private static bool NeedsReset() =>
        _yawOffset != 0f || _latched || _requested || _maskCleared || _focusFactor != 0f || _haveFov;

    private static int _gameplayFrame = -1;
    private static bool _gameplayCached;

    private static bool InGameplay()
    {
        if (_gameplayFrame == Time.frameCount) return _gameplayCached;

        _gameplayFrame = Time.frameCount;
        _gameplayCached = ReadGameplay();
        return _gameplayCached;
    }

    private static bool ReadGameplay()
    {

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(scene)) return false;
        if (scene.Contains("Boot", StringComparison.OrdinalIgnoreCase)) return false;
        if (scene.Contains("Empty", StringComparison.OrdinalIgnoreCase)) return false;
        if (scene.StartsWith("MainMenu", StringComparison.OrdinalIgnoreCase)) return false;

        try
        {

            if (Il2Cpp.GameManager.m_Instance == null) return false;

            return !Il2Cpp.GameManager.IsMainMenuActive();
        }
        catch
        {

            return false;
        }
    }

    private static bool ControlWasTakenAway()
    {
        try
        {
            return Il2Cpp.GameManager.ControlsLocked()
                   || Il2Cpp.InterfaceManager.IsOverlayActiveImmediate()
                   || ControlModeTookOver();
        }
        catch
        {
            return true;
        }
    }

    internal static void PollInput()
    {
        if (!Config.EnableMod)
        {

            if (NeedsReset()) Reset();
            return;
        }

        if (!InGameplay() || !Application.isFocused)
        {
            if (NeedsReset()) Reset();

            _wasDown = true;
            return;
        }

        bool down = ModifierIsDown();
        bool pressed = down && !_wasDown;
        _wasDown = down;

        if (Config.Verbose && pressed)
        {
            bool keyDown = Config.ModifierKey != KeyCode.None && Input.GetKey(Config.ModifierKey);
            bool latching = Config.ToggleMode || Config.DoubleTapLatch;
            bool ctrl = ControllerIsActive();
            bool autoWalk = false;
            if (_pollContext != null) autoWalk = Il2Cpp.InputManager.GetAutoWalkDown(_pollContext);
            Core.Log.Msg($"TRIGGER key={keyDown} latchingMode={latching} controllerActive={ctrl} autoWalk={autoWalk}");
        }

        PollFocus();
        UpdateFocus();

        if (!down && !pressed && !_latched)
        {
            _requested = false;
            return;
        }

        if (_latched && ControlWasTakenAway())
        {
            _latched = false;
            _lastTapTime = -1f;
        }

        if (Config.ToggleMode)
        {
            if (pressed) _latched = !_latched;
            _requested = _latched;
        }
        else if (Config.DoubleTapLatch)
        {
            if (pressed)
            {
                if (Time.unscaledTime - _lastTapTime <= DoubleTapWindow)
                {

                    _latched = !_latched;
                    _lastTapTime = -1f;
                }
                else
                {
                    _lastTapTime = Time.unscaledTime;
                }
            }

            _requested = _latched || down;
        }
        else
        {
            _latched = false;
            _requested = down;
        }
    }

    private static void PollFocus()
    {
        if (!Config.EnableFocus || Config.FocusKey == KeyCode.None)
        {
            _focusRequested = false;
            _focusLatched = false;
            _focusWasDown = false;
            return;
        }

        bool down = Input.GetKey(Config.FocusKey);
        bool pressed = down && !_focusWasDown;
        _focusWasDown = down;

        if (Config.FocusToggle)
        {
            if (pressed) _focusLatched = !_focusLatched;
            _focusRequested = _focusLatched;
        }
        else
        {
            _focusLatched = false;
            _focusRequested = down;
        }
    }

    private static bool ModifierIsDown()
    {
        if (Config.ModifierKey != KeyCode.None && Input.GetKey(Config.ModifierKey)) return true;

        if (!Config.ToggleMode && !Config.DoubleTapLatch) return false;
        if (!ControllerIsActive()) return false;

        if (_pollContext == null) _pollContext = UnityEngine.Object.FindObjectOfType<vp_FPSCamera>();
        return _pollContext != null && Il2Cpp.InputManager.GetAutoWalkDown(_pollContext);
    }

    private static bool _indicatorEngaged;
    private static bool _indicatorLatched;
    private static float _indicatorStamp = -999f;

    private static bool LookStateFresh => Time.unscaledTime - _indicatorStamp < 0.5f;

    internal static bool LookEngagedLive => _indicatorEngaged && LookStateFresh;

    internal static bool LookLatchedLive => _indicatorLatched && LookStateFresh;

    private static bool ShouldEngage(vp_FPSCamera camera)
    {
        if (!Config.EnableMod || !_requested || camera == null) return false;

        if (camera.IsFreeCameraLookEnabled()) return false;

        if (Config.DisableWhileAiming && camera.IsZoomed) return false;

        if (Config.DisableWhenEquipped && !NothingEquipped()) return false;

        if (Config.DisableWhileCrouched && IsCrouching()) return false;

        if (!ControlModeAllowsFreeLook()) return false;

        if (CameraDetachedFromBody(camera)) return false;

        return true;
    }

    internal static void DivertYaw(vp_FPSCamera camera, ref Vector2 input)
    {
        if (!ShouldEngage(camera)) return;

        float scale = FocusInputScale();
        if (scale != 1f)
        {
            input.x *= scale;
            input.y *= scale;
        }

        float limit = Mathf.Max(0f, Config.YawLimit);
        _yawOffset = Mathf.Clamp(_yawOffset + input.x, -limit, limit);

        input.x = 0f;
    }

    internal static void ApplyToCamera(vp_FPSCamera camera)
    {
        if (camera == null) return;

        if (!InGameplay()) return;

        bool engaged = ShouldEngage(camera);

        TurnBodyToAim(camera, engaged);

        if (_latched && Config.DisableWhileAiming && camera.IsZoomed)
        {
            _latched = false;
            _requested = false;
            _lastTapTime = -1f;
            if (Config.Verbose) Core.Log.Msg("weapon raised - latched free look released");
        }

        UpdateReturn(camera, engaged);

        UpdateArms(camera, engaged);

        if (_focusFactor > 0f) ApplyZoom(camera); else RestoreFieldOfView();

        if (Config.Verbose && engaged != _wasEngaged)
            Core.Log.Msg($"free look {(engaged ? "engaged" : "released")} (offset {_yawOffset:0.0} deg)");
        _wasEngaged = engaged;

        _indicatorEngaged = engaged;
        _indicatorLatched = engaged && _latched;
        _indicatorStamp = Time.unscaledTime;

        if (_yawOffset == 0f)
        {
            _haveWritten = false;
            return;
        }

        Transform t = camera.transform;
        if (t == null) return;

        if (_haveWritten && t.rotation == _lastWritten)
        {
            _yawOffset = 0f;
            _returning = false;
            _haveWritten = false;
            return;
        }

        t.rotation = Quaternion.AngleAxis(_yawOffset, Vector3.up) * t.rotation;
        _lastWritten = t.rotation;
        _haveWritten = true;
    }

    private const float DetachedDistance = 5f;

    private static bool CameraDetachedFromBody(vp_FPSCamera camera)
    {
        var player = Il2Cpp.GameManager.GetPlayerTransform();
        if (player == null) return false;

        Transform t = camera.transform;
        if (t == null) return false;

        return (t.position - player.position).sqrMagnitude > DetachedDistance * DetachedDistance;
    }

    private static bool ControllerIsActive()
    {
        var input = Il2Cpp.InputManager.m_InputSystem;
        if (input == null) return false;

        return input.m_LastActiveController != Il2Cpp.InputSystemRewired.ActiveControllerType.None;
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

    private static bool ControlModeAllowsFreeLook()
    {
        var pm = Il2Cpp.GameManager.GetPlayerManagerComponent();
        if (pm == null) return false;

        var mode = pm.GetControlMode();
        if (IsFreeLookControlMode(mode)) return true;

        return IsAimControlMode(mode) && !Config.DisableWhileAiming;
    }

    private static bool IsFreeLookControlMode(Il2Cpp.PlayerControlMode mode) =>
        mode == Il2Cpp.PlayerControlMode.Normal ||
        mode == Il2Cpp.PlayerControlMode.InVehicle;

    private static bool IsAimControlMode(Il2Cpp.PlayerControlMode mode) =>
        mode == Il2Cpp.PlayerControlMode.AimRevolver || mode == Il2Cpp.PlayerControlMode.BearSpear;

    private static bool ControlModeTookOver()
    {
        var pm = Il2Cpp.GameManager.GetPlayerManagerComponent();
        if (pm == null) return false;

        var mode = pm.GetControlMode();
        return !IsFreeLookControlMode(mode) && !IsAimControlMode(mode);
    }

    private static Camera _fovCam;
    private static float _fovBase;
    private static float _fovWritten;
    private static bool _haveFov;

    private static bool _wasFocused;

    private static float FocusEased => _focusFactor * _focusFactor * (3f - 2f * _focusFactor);

    private static float ZoomProgress
    {
        get
        {
            float mag = Mathf.Max(1f, Config.FocusZoom);
            if (mag <= 1.0001f) return FocusEased;

            float current = 1f / Mathf.Lerp(1f, 1f / mag, FocusEased);
            return Mathf.Clamp01((current - 1f) / (mag - 1f));
        }
    }

    private static float FocusInputScale()
    {
        if (_focusFactor <= 0f) return 1f;
        float mag = Mathf.Max(1f, Config.FocusZoom);
        return Mathf.Lerp(1f, 1f / mag, FocusEased);
    }

    private static void UpdateFocus()
    {
        bool engaged = LookEngagedLive;

        if (!engaged) _focusLatched = false;

        bool focus = engaged && Config.EnableFocus && _focusRequested;

        if (Config.Verbose && focus != _wasFocused) Core.Log.Msg($"focus {(focus ? "engaged" : "released")}");
        _wasFocused = focus;

        float dt = Time.unscaledDeltaTime;

        if (!engaged && Config.ReturnSpeed <= 0f)
        {

            _focusFactor = 0f;
        }
        else if (!focus && _returning && _returnDuration > _returnElapsed)
        {

            float remaining = _returnDuration - _returnElapsed;
            _focusFactor = remaining > dt
                ? Mathf.Max(0f, _focusFactor - _focusFactor * dt / remaining)
                : 0f;
        }
        else
        {

            float ease = Mathf.Max(0f, Config.FocusEase);
            float target = focus ? 1f : 0f;
            _focusFactor = ease > 0f ? Mathf.MoveTowards(_focusFactor, target, dt / ease) : target;
        }

        FocusEffect.Apply(_focusFactor <= 0f ? 0f : ZoomProgress);
    }

    private static void ApplyZoom(vp_FPSCamera camera)
    {
        Camera cam = camera.m_Camera;
        if (cam == null) return;

        if (!ReferenceEquals(cam, _fovCam)) { _fovCam = cam; _haveFov = false; }

        float current = cam.fieldOfView;
        if (!_haveFov || current != _fovWritten) _fovBase = current;

        float mag = Mathf.Max(1f, Config.FocusZoom);
        float target = _fovBase * Mathf.Lerp(1f, 1f / mag, FocusEased);

        cam.fieldOfView = target;
        _fovWritten = target;
        _haveFov = true;
    }

    private static void RestoreFieldOfView()
    {
        if (!_haveFov) return;
        _haveFov = false;

        if (_fovCam != null && _fovCam.fieldOfView == _fovWritten) _fovCam.fieldOfView = _fovBase;
        _fovCam = null;
    }

    private static void UpdateReturn(vp_FPSCamera camera, bool engaged)
    {
        if (engaged)
        {

            if (!_wasEngaged)
            {
                _entryPitch = camera.m_Pitch;
            }

            _returning = false;
            _lastYaw = camera.m_Yaw;
            return;
        }

        if (_wasEngaged)
        {
            BeginReturn(camera);

            if (_returning) ApplyPitch(camera, _returnPitch0);
            _lastYaw = camera.m_Yaw;
            return;
        }

        if (!_returning) { _lastYaw = camera.m_Yaw; return; }

        float errYaw = _yawOffset;
        float errPitch = camera.m_Pitch - _entryPitch;
        float errMag = Mathf.Sqrt(errYaw * errYaw + errPitch * errPitch);
        if (errMag > 0.0001f)
        {
            float inYaw = float.IsNaN(_lastYaw) ? 0f : Mathf.DeltaAngle(_lastYaw, camera.m_Yaw);
            float inPitch = camera.m_TargetPitch - camera.m_Pitch;
            float opposed = (inYaw * errYaw + inPitch * errPitch) / errMag;
            if (opposed > 0f) _opposedAccum += opposed;
        }

        if (_opposedAccum > TakeoverDeadzone)
        {
            CommitReturnToBody(camera);
            _lastYaw = camera.m_Yaw;
            return;
        }

        _returnElapsed += Time.unscaledDeltaTime;

        float u = _returnDuration > 0f ? Mathf.Clamp01(_returnElapsed / _returnDuration) : 1f;
        float eased = u * u * (3f - 2f * u);

        _yawOffset = Mathf.Lerp(_returnYaw0, 0f, eased);
        ApplyPitch(camera, Mathf.Lerp(_returnPitch0, _entryPitch, eased));

        if (u >= 1f)
        {
            _yawOffset = 0f;
            ApplyPitch(camera, _entryPitch);
            _returning = false;
        }

        _lastYaw = camera.m_Yaw;
    }

    private const float TakeoverDeadzone = 3f;

    private static void BeginReturn(vp_FPSCamera camera)
    {

        _returnYaw0 = Mathf.DeltaAngle(0f, _yawOffset);
        _yawOffset = _returnYaw0;
        _returnPitch0 = camera.m_Pitch;
        _returnElapsed = 0f;
        _opposedAccum = 0f;

        float dPitch = _returnPitch0 - _entryPitch;
        float distance = Mathf.Sqrt(_returnYaw0 * _returnYaw0 + dPitch * dPitch);

        if (distance <= 0f) { _returning = false; return; }

        if (Config.ReturnSpeed <= 0f)
        {
            _yawOffset = 0f;
            ApplyPitch(camera, _entryPitch);
            _returning = false;
            return;
        }

        _returnDuration = distance / Config.ReturnSpeed;
        _returning = true;
    }

    private static void CommitReturnToBody(vp_FPSCamera camera)
    {
        _returning = false;

        float committed = _yawOffset;
        if (committed == 0f) return;

        camera.m_Yaw += committed;
        camera.m_TargetYaw += committed;
        camera.m_CurrentYaw += committed;
        _yawOffset = 0f;

        Transform t = camera.transform;
        if (t != null) t.rotation = Quaternion.AngleAxis(committed, Vector3.up) * t.rotation;
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
        _returning = false;
        _entryPitch = camera.m_Pitch;

        Transform t = camera.transform;
        if (t != null) t.rotation = Quaternion.AngleAxis(committed, Vector3.up) * t.rotation;

        camera.SnapSprings();

        _revealHoldUntil = Time.unscaledTime + AimRevealHoldSeconds;
        if (Config.Verbose) Core.Log.Msg($"raised a weapon mid-look; turned the body {committed:0.0} deg to face it");
    }

    private const float AimRevealHoldSeconds = 0.035f;

    private static Quaternion _lastWritten;

    private static bool _haveWritten;

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
