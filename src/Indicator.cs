using Il2Cpp;
using UnityEngine;

namespace FreeLook;

internal static class Indicator
{

    private const string RootName = "FreeLookIndicator";

    private static GameObject _root;

    private static UISprite _back;
    private static UISprite _over;

    private static bool _loggedAtlas;

    private static int _placedW, _placedH;
    private static float _placedX = float.NaN, _placedY = float.NaN;
    private static IndicatorCorner _placedCorner = (IndicatorCorner)(-1);

    private static int _sizedFor = -1;
    private static float _scaledFor = float.NaN, _tintedFor = float.NaN;

    internal static void Reset()
    {
        if (_root != null) UnityEngine.Object.Destroy(_root);
        _root = null;
        _back = null;
        _over = null;
        _loggedAtlas = false;
        _placedW = _placedH = 0;
        _placedX = _placedY = float.NaN;
        _placedCorner = (IndicatorCorner)(-1);
        _sizedFor = -1;
        _scaledFor = _tintedFor = float.NaN;
    }

    internal static void Refresh(Panel_HUD hud)
    {
        try
        {
            if (!Config.EnableMod || Config.ShowIcon == IndicatorVisibility.Never)
            {
                if (_root != null) _root.SetActive(false);
                return;
            }

            if (_root == null && !Build(hud)) return;

            if (Screen.width != _placedW || Screen.height != _placedH
                || Config.HorizontalOffset != _placedX || Config.VerticalOffset != _placedY
                || Config.ScreenAnchor != _placedCorner)
                ApplyPosition();

            if (Config.IconSize != _sizedFor
                || Config.IconOverlayScale != _scaledFor
                || Config.IconOpacity != _tintedFor)
            {
                ApplyAppearance();

                ApplyPosition();
            }

            bool want = Config.ShowIcon == IndicatorVisibility.WhenLooking
                ? FreeLookController.LookEngagedLive
                : FreeLookController.LookLatchedLive;
            if (_root.activeSelf != want) _root.SetActive(want);
        }
        catch (System.Exception ex)
        {

            if (Config.Verbose) Core.Log.Warning("indicator refresh failed, disabling it: " + ex.Message);
            Config.ShowIcon = IndicatorVisibility.Never;
        }
    }

    private static bool Build(Panel_HUD hud)
    {
        if (hud == null) return false;

        UISprite model = FindModelSprite(hud);
        if (model == null) return false;

        if (Config.Verbose && !_loggedAtlas) LogAtlasSprites(model);

        Transform host = hud.m_NonEssentialHud != null ? hud.m_NonEssentialHud.transform : hud.transform;

        var strays = hud.GetComponentsInChildren<Transform>(true);
        if (strays != null)
            for (int i = 0; i < strays.Length; i++)
                if (strays[i] != null && strays[i].name == RootName)
                    UnityEngine.Object.Destroy(strays[i].gameObject);

        _root = new GameObject(RootName);
        _root.transform.SetParent(host, false);
        _root.transform.localScale = Vector3.one;
        _root.transform.localPosition = Vector3.zero;

        int top = TopDepth(hud);
        _back = AddLayer("back", model, Config.IndicatorSprite, top + 1);
        if (!string.IsNullOrEmpty(Config.IndicatorSpriteOver))
        {
            _over = AddLayer("over", model, Config.IndicatorSpriteOver, top + 2);
        }

        if (_back == null && _over == null) { _root = null; return false; }

        ApplyAppearance();
        ApplyPosition();
        _root.SetActive(false);

        Core.Log.Msg($"indicator built: '{Config.IndicatorSprite}' + '{Config.IndicatorSpriteOver}' " +
                     $"at {Config.HorizontalOffset:0.00}% x {Config.VerticalOffset:0.00}% " +
                     $"in from {Config.ScreenAnchor}, height {Config.IconSize}");
        return true;
    }

    private static void IconHalfExtents(out float halfW, out float halfH)
    {
        float w = 0f, h = 0f;
        if (_back != null) { w = Mathf.Max(w, _back.width); h = Mathf.Max(h, _back.height); }
        if (_over != null) { w = Mathf.Max(w, _over.width); h = Mathf.Max(h, _over.height); }
        if (w <= 0f) w = Config.IconSize;
        if (h <= 0f) h = Config.IconSize;
        halfW = w * 0.5f;
        halfH = h * 0.5f;
    }

    private static void ApplyAppearance()
    {
        if (_root == null) return;

        int h = Mathf.Max(1, Config.IconSize);
        SizeLayer(_back, h);
        SizeLayer(_over, Mathf.Max(1, Mathf.RoundToInt(h * Config.IconOverlayScale)));

        _sizedFor = Config.IconSize;
        _scaledFor = Config.IconOverlayScale;
        _tintedFor = Config.IconOpacity;
    }

    private static void SizeLayer(UISprite sp, int height)
    {
        if (sp == null) return;
        float aspect = 1f;
        var data = sp.GetAtlasSprite();
        if (data != null && data.height > 0) aspect = (float)data.width / data.height;
        sp.height = height;
        sp.width = Mathf.Max(1, Mathf.RoundToInt(height * aspect));
        sp.color = new Color(VanillaTint, VanillaTint, VanillaTint, Mathf.Clamp01(Config.IconOpacity));
    }

    private static void ApplyPosition()
    {
        if (_root == null) return;

        float h = 720f;
        var root = _root.GetComponentInParent<UIRoot>();
        if (root != null && root.activeHeight > 0) h = root.activeHeight;

        int sw = Mathf.Max(1, Screen.width);
        int sh = Mathf.Max(1, Screen.height);
        float w = h * ((float)sw / sh);

        float dx = Mathf.Clamp01(Config.HorizontalOffset / 100f) * w;
        float dy = Mathf.Clamp01(Config.VerticalOffset / 100f) * h;

        float x, y;
        switch (Config.ScreenAnchor)
        {
            case IndicatorCorner.BottomLeft: x = -w * 0.5f + dx; y = -h * 0.5f + dy; break;
            case IndicatorCorner.TopRight:   x =  w * 0.5f - dx; y =  h * 0.5f - dy; break;
            case IndicatorCorner.TopLeft:    x = -w * 0.5f + dx; y =  h * 0.5f - dy; break;
            default:                         x =  w * 0.5f - dx; y = -h * 0.5f + dy; break;
        }

        float halfW, halfH;
        IconHalfExtents(out halfW, out halfH);

        x = (halfW * 2f >= w) ? 0f : Mathf.Clamp(x, -w * 0.5f + halfW, w * 0.5f - halfW);
        y = (halfH * 2f >= h) ? 0f : Mathf.Clamp(y, -h * 0.5f + halfH, h * 0.5f - halfH);

        _root.transform.localPosition = new Vector3(Mathf.Round(x), Mathf.Round(y), 0f);

        _placedW = sw; _placedH = sh;
        _placedX = Config.HorizontalOffset; _placedY = Config.VerticalOffset;
        _placedCorner = Config.ScreenAnchor;

        if (Config.Verbose)
            Core.Log.Msg($"indicator at ({x:0}, {y:0}), {Config.HorizontalOffset:0.00}% x " +
                         $"{Config.VerticalOffset:0.00}% in from {Config.ScreenAnchor} " +
                         $"in a {w:0}x{h:0} canvas for a {sw}x{sh} screen");
    }

    private const int OverlaySize = 4000;

    private const float VanillaTint = 0.98f;

    private static int TopDepth(Panel_HUD hud)
    {
        int top = 0;
        var all = hud.GetComponentsInChildren<UIWidget>(true);
        if (all != null)
            for (int i = 0; i < all.Length; i++)
            {
                var w = all[i];
                if (w == null) continue;
                if (w.width >= OverlaySize || w.height >= OverlaySize) continue;
                if (w.depth > top) top = w.depth;
            }
        return top;
    }

    private static UISprite AddLayer(string name, UISprite model, string spriteName, int depth)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_root.transform, false);
        go.transform.localPosition = Vector3.zero;

        var sp = go.AddComponent<UISprite>();
        sp.atlas = model.atlas;
        sp.spriteName = spriteName;
        sp.depth = depth;

        if (sp.GetAtlasSprite() == null)
            Core.Log.Warning($"indicator sprite '{spriteName}' is not in the atlas - it will draw " +
                             "nothing. Names are listed in TLD-Knowledge/UI-Atlas.csv.");

        return sp;
    }

    private static UISprite FindModelSprite(Panel_HUD hud)
    {
        var bars = hud.GetComponentsInChildren<UISprite>(true);
        if (bars == null || bars.Length == 0) return null;

        UISprite best = null;
        for (int i = 0; i < bars.Length; i++)
        {
            var s = bars[i];
            if (s == null || s.atlas == null) continue;

            if (s.transform.parent == null) continue;
            best = s;
            break;
        }
        return best;
    }

    private static void LogAtlasSprites(UISprite model)
    {
        _loggedAtlas = true;
        try
        {
            var names = model.atlas.GetListOfSprites();
            if (names == null) { Core.Log.Msg("atlas returned no sprite list"); return; }
            Core.Log.Msg($"atlas '{model.atlas.name}' has {names.size} sprites:");
            for (int i = 0; i < names.size; i++)
                Core.Log.Msg("  sprite: " + names[i]);
        }
        catch (System.Exception ex)
        {
            Core.Log.Warning("could not list atlas sprites: " + ex.Message);
        }
    }
}
