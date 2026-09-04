using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace FreeLook;

internal static class FocusEffect
{
    private static GameObject _host;

    private static PostProcessVolume _vol;
    private static PostProcessProfile _prof;

    private static Il2Cpp.SuffocationScreenEffect _edge;
    private static Il2Cpp.MaskedBlurEffect _blur;

    private static Texture2D _fovealMask;

    private static int _builtSignature = int.MinValue;

    private static bool _failed;

    private static float _weight = -1f;

    private static int Signature =>
        (Config.FxEdgeDarkening ? 1 : 0) | (Config.FxFovealBlur ? 2 : 0);

    internal static void Apply(float f)
    {
        f = Mathf.Clamp01(f);

        if (f <= 0f && _host == null) return;

        if (Signature == 0)
        {

            if (_host != null) Destroy();
            return;
        }

        if (!Ensure()) return;

        SetWeight(_vol, f > 0f ? 1f : 0f, ref _weight);

        if (f <= 0f) { MarkChanged(); return; }

        DriveParameters(f);
        MarkChanged();
    }

    private static void SetWeight(PostProcessVolume v, float w, ref float cached)
    {
        if (v == null || w == cached) return;
        cached = w;
        v.weight = w;
    }

    private static void MarkChanged()
    {
        try { PostProcessManager.MarkSettingsChanged(); }
        catch {  }
    }

    private static void DriveParameters(float f)
    {
        if (_edge != null)
        {

            _edge.m_Intensity.value = f;
            _edge.m_VignetteStrength.value = EdgeStrength;

            _edge.m_RadialBlurAmount.value = 0f;
            _edge.m_RadialBlurPower.value = 0f;
            _edge.m_RadialBlurCenterOffset.value = 0f;
        }

        if (_blur != null)
        {
            _blur.focusDistance.value = FocusDepth;

            float r = f * BlurRatio / BlurRatioMax;

            _blur.depthBlendAmount.value = Mathf.Lerp(0.01f, 1f, r);
            _blur.aperture.value = Mathf.Lerp(22f, 5.6f, r);
            _blur.focalLength.value = Mathf.Lerp(18f, 55f, r);

            _blur.kernelSize.value = r < 0.3f ? KernelSize.Small
                                   : r < 0.6f ? KernelSize.Medium
                                   : r < 0.85f ? KernelSize.Large
                                   : KernelSize.VeryLarge;
        }
    }

    private static bool Ensure()
    {
        if (_host != null && _builtSignature == Signature) return true;
        if (_host != null) Destroy();
        if (_failed) return false;

        try
        {
            int layer = ResolveLayer();
            _host = new GameObject("FreeLook_FocusEffects") { layer = layer };

            _prof = NewProfile();

            if (Config.FxEdgeDarkening) _edge = AddEdgeDarkening();
            if (Config.FxFovealBlur) _blur = AddFovealBlur();

            _vol = NewVolume(_prof);
            _weight = 0f;
            _builtSignature = Signature;

            if (Config.Verbose) Core.Log.Msg($"focus effects built on layer {layer}, set {Signature}");
            return true;
        }
        catch (Exception ex)
        {

            Core.Log.Warning("Focus effects unavailable, the zoom is unaffected. " + ex);

            Destroy();
            _failed = true;
            return false;
        }
    }

    private static PostProcessProfile NewProfile()
    {
        var p = ScriptableObject.CreateInstance(Il2CppType.Of<PostProcessProfile>()).TryCast<PostProcessProfile>();
        if (p == null) throw new Exception("could not create a PostProcessProfile");
        return p;
    }

    private static PostProcessVolume NewVolume(PostProcessProfile profile)
    {
        var v = _host.AddComponent(Il2CppType.Of<PostProcessVolume>()).TryCast<PostProcessVolume>();
        if (v == null) throw new Exception("could not add a PostProcessVolume");
        v.isGlobal = true;
        v.blendDistance = 0f;
        v.priority = 100f;
        v.weight = 0f;
        v.sharedProfile = profile;
        return v;
    }

    private static int ResolveLayer()
    {
        const int measured = 1;

        var rig = UnityEngine.Object.FindObjectOfType<Il2Cpp.CameraGlobalRT>();
        if (rig == null)
        {
            Core.Log.Warning("Focus effects: no CameraGlobalRT to ask which layer carries the image " +
                             $"effects; assuming layer {measured}. If nothing appears on screen, this is why.");
            return measured;
        }

        var ppl = rig.m_ImageEffectsPostProcessLayer;
        if (ppl == null)
        {
            Core.Log.Warning("Focus effects: CameraGlobalRT has no image-effects PostProcessLayer; " +
                             $"assuming layer {measured}.");
            return measured;
        }

        int mask = ppl.volumeLayer.value;
        if (mask == 0)
        {
            Core.Log.Warning($"Focus effects: '{ppl.gameObject.name}' has an empty volumeLayer mask, " +
                             $"so it looks at no volumes at all; assuming layer {measured}.");
            return measured;
        }

        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            if (i != measured)
                Core.Log.Msg($"Focus effects: image effects are on layer {i} (mask 0x{mask:X}), " +
                             $"not the {measured} this build expected; following the game.");
            return i;
        }

        return measured;
    }

    private static Il2Cpp.SuffocationScreenEffect AddEdgeDarkening()
    {
        var e = Create<Il2Cpp.SuffocationScreenEffect>();

        e.m_Intensity.overrideState = true; e.m_Intensity.value = 0f;
        e.m_VignetteStrength.overrideState = true; e.m_VignetteStrength.value = EdgeStrength;

        e.m_RadialBlurAmount.overrideState = true; e.m_RadialBlurAmount.value = 0f;
        e.m_RadialBlurPower.overrideState = true; e.m_RadialBlurPower.value = 0f;
        e.m_RadialBlurCenterOffset.overrideState = true; e.m_RadialBlurCenterOffset.value = 0f;

        e.m_NoiseTexture.overrideState = true; e.m_NoiseTexture.value = null;
        e.m_NoiseScaleAndSpeed.overrideState = true; e.m_NoiseScaleAndSpeed.value = Vector4.zero;
        return e;
    }

    private static Il2Cpp.MaskedBlurEffect AddFovealBlur()
    {
        var b = Create<Il2Cpp.MaskedBlurEffect>();
        b.depthTexture.overrideState = true; b.depthTexture.value = BuildFovealMask();
        b.depthBlendAmount.overrideState = true; b.depthBlendAmount.value = 0.01f;
        b.focusDistance.overrideState = true; b.focusDistance.value = FocusDepth;
        b.aperture.overrideState = true; b.aperture.value = 32f;
        b.focalLength.overrideState = true; b.focalLength.value = 1f;
        b.kernelSize.overrideState = true; b.kernelSize.value = KernelSize.Medium;
        return b;
    }

    private static T Create<T>() where T : PostProcessEffectSettings
    {
        var s = ScriptableObject.CreateInstance(Il2CppType.Of<T>()).TryCast<T>();
        if (s == null) throw new Exception("could not create " + typeof(T).Name);

        s.enabled.overrideState = true;
        s.enabled.value = true;
        s.active = true;

        _prof.AddSettings(s);
        return s;
    }

    private const float MaskFloor = 0f;

    private const float FocusDepth = 1f;

    private const float BlurRatio = 0.6f;

    private const float BlurRatioMax = 0.75f;

    private const float EdgeStrength = 0.6f;

    private static Texture2D BuildFovealMask()
    {
        const int n = 128;

        if (_fovealMask == null)
        {
            _fovealMask = new Texture2D(n, n, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "FreeLook_FovealMask",
            };
        }

        const float ratio = BlurRatio;
        float inner = 1f - ratio;

        float spread = Mathf.Lerp(0.08f, 0.35f, ratio / BlurRatioMax);

        var pixels = new Il2CppStructArray<Color32>(n * n);
        float half = (n - 1) * 0.5f;

        for (int y = 0; y < n; y++)
        {
            float dy = (y - half) / half;
            for (int x = 0; x < n; x++)
            {
                float dx = (x - half) / half;

                float u = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));

                float t = Mathf.SmoothStep(inner, 1f, u);
                byte v = (byte)Mathf.RoundToInt(Mathf.Clamp01(MaskFloor + t * spread) * 255f);
                pixels[y * n + x] = new Color32(v, v, v, 255);
            }
        }

        _fovealMask.SetPixels32(pixels);
        _fovealMask.Apply(false, false);
        return _fovealMask;
    }

    internal static void Destroy()
    {
        if (_host != null) UnityEngine.Object.DestroyImmediate(_host);
        DestroyAsset(_prof);
        DestroyAsset(_edge);
        DestroyAsset(_blur);
        DestroyAsset(_fovealMask);

        _host = null;
        _vol = null;
        _prof = null;
        _edge = null;
        _blur = null;
        _fovealMask = null;
        _builtSignature = int.MinValue;
        _weight = -1f;
        _failed = false;
    }

    private static void DestroyAsset(UnityEngine.Object o)
    {
        if (o != null) UnityEngine.Object.DestroyImmediate(o);
    }
}
