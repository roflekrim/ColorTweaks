/*
 name is 100% not fitting
*/

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using BeatSaberMarkupLanguage;
using ColorTweaks.Configuration;
using ColorTweaks.UI;
using HMUI;
using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Attributes;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace ColorTweaks.Affinity_Patches;

// ReSharper disable InconsistentNaming
[Bind(Location.Menu)]
internal class SliderMagic : IAffinity, IInitializable, IDisposable
{
    private SiraLog _siraLog;
    private PluginConfig _config;

    private RGBPanelController? _rgbPanelController;
    private ColorGradientSlider? _redSlider;
    private ColorGradientSlider? _greenSlider;
    private ColorGradientSlider? _blueSlider;
    private ColorGradientSlider? _alphaSlider;

    public SliderMagic(SiraLog siraLog, PluginConfig config)
    {
        _siraLog = siraLog;
        _config = config;
    }

    public void Initialize()
    {
        _config.PropertyChanged.AddListener(Refresh);
    }

    public void Dispose()
    {
        _config.PropertyChanged.RemoveListener(Refresh);
    }

    #region Patches
    
    [AffinityPostfix]
    [AffinityPatch(typeof(RGBPanelController), nameof(RGBPanelController.Awake))]
    internal void Awake(RGBPanelController __instance, ColorGradientSlider ____redSlider,
        ColorGradientSlider ____greenSlider,
        ColorGradientSlider ____blueSlider)
    {
        _rgbPanelController = __instance;
        _redSlider = ____redSlider;
        _greenSlider = ____greenSlider;
        _blueSlider = ____blueSlider;

        _redSlider.numberOfSteps = _config.Steps;
        _greenSlider.numberOfSteps = _config.Steps;
        _blueSlider.numberOfSteps = _config.Steps;
        
        // alpha slider
        if (!_config.EnableAlphaSlider) return;
        CreateAlphaSlider();
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(RGBPanelController), nameof(RGBPanelController.OnDestroy))]
    internal void OnDestroy(RGBPanelController __instance)
    {
        if (_alphaSlider != null)
            _alphaSlider.colorDidChangeEvent -= __instance.HandleSliderColorDidChange;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(RGBPanelController), nameof(RGBPanelController.RefreshSlidersValues))]
    internal void RefreshSlidersValues(RGBPanelController __instance)
    {
        if (_redSlider != null)
            _redSlider.normalizedValue =
                __instance.color.r.Remap(_config.RedSliderMin, _config.RedSliderMax, 0f, 1f);
        
        if (_greenSlider != null)
            _greenSlider.normalizedValue =
                __instance.color.g.Remap(_config.GreenSliderMin, _config.GreenSliderMax, 0f, 1f);
        
        if (_blueSlider != null)
            _blueSlider.normalizedValue =
                __instance.color.b.Remap(_config.BlueSliderMin, _config.BlueSliderMax, 0f, 1f);
        
        if (_alphaSlider != null)
            _alphaSlider.normalizedValue =
                __instance.color.a.Remap(_config.AlphaSliderMin, _config.AlphaSliderMax, 0f, 1f);
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(ColorGradientSlider), "TextForNormalizedValue")]
    internal bool TextForNormalizedValue(ColorGradientSlider __instance, ref string __result, float normalizedValue)
    {
        var stringBuilder = typeof(ColorGradientSlider).GetField("_stringBuilder", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null!) as StringBuilder;
        if (stringBuilder == null) return true;

        var mapped = GetRemappedFromSlider(__instance, normalizedValue);
        stringBuilder.Clear();
        stringBuilder.Append(__instance.GetField<string, ColorGradientSlider>("_textPrefix"));
        
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (_config.DisplayMode)
        {
            case PluginConfig.XDisplayMode.Default:
                stringBuilder.Append(Mathf.RoundToInt(mapped * 255f));
                break;
            case PluginConfig.XDisplayMode.Normalized:
                stringBuilder.Append(Mathf.RoundToInt(normalizedValue));
                break;
            case PluginConfig.XDisplayMode.MinToMax:
                stringBuilder.Append(mapped);
                break;
            case PluginConfig.XDisplayMode.Percentage:
                stringBuilder.Append(mapped.ToString("0%"));
                break;
        }

        __result = stringBuilder.ToString();
        return false;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(RGBPanelController), nameof(RGBPanelController.RefreshSlidersColors))]
    internal bool SetColors(RGBPanelController __instance, Color ____color)
    {
        if (_redSlider == null || _greenSlider == null || _blueSlider == null)
            return true;
        
        ____color = ____color.ColorWithAlpha(1f);
        _redSlider.SetColors(____color.ColorWithR(_config.RedSliderMin), ____color.ColorWithR(_config.RedSliderMax));
        _greenSlider.SetColors(____color.ColorWithG(_config.GreenSliderMin), ____color.ColorWithG(_config.GreenSliderMax));
        _blueSlider.SetColors(____color.ColorWithB(_config.BlueSliderMin), ____color.ColorWithB(_config.BlueSliderMax));
        if (_alphaSlider != null)
            _alphaSlider.SetColors(____color.ColorWithAlpha(_config.AlphaSliderMin), ____color.ColorWithAlpha(_config.AlphaSliderMax));
        return false;
    }
    
    #endregion

    private float GetRemappedFromSlider(ColorGradientSlider? slider, float normalizedValue)
    {
        if (slider == null) return normalizedValue;
        float min, max;

        if (slider == _redSlider)
        {
            min = _config.RedSliderMin;
            max = _config.RedSliderMax;
        } else if (slider == _greenSlider)
        {
            min = _config.GreenSliderMin;
            max = _config.GreenSliderMax;
        } else if (slider == _blueSlider)
        {
            min = _config.BlueSliderMin;
            max = _config.BlueSliderMax;
        } else if (slider == _alphaSlider)
        {
            min = _config.AlphaSliderMin;
            max = _config.AlphaSliderMax;
        }
        else
        {
            min = 0f;
            max = 1f;
        }
        
        return normalizedValue.Remap(0f, 1f, min, max);
    }

    private void CreateAlphaSlider()
    {
        if (_rgbPanelController == null || _greenSlider == null || _blueSlider == null) return;
        var slider = Object.Instantiate(_blueSlider.gameObject, _blueSlider.transform.parent);
        var sliderComponent = slider.GetComponent<ColorGradientSlider>();
        sliderComponent.numberOfSteps = _config.Steps;
        sliderComponent.SetColors(Color.black.ColorWithAlpha(1f), Color.white);
        sliderComponent.SetField<ColorGradientSlider, string>("_textPrefix", "A: ");

        var container = slider.transform.parent;
        var rectTransform = slider.GetComponent<RectTransform>();
        var greenRectTransform = _greenSlider.gameObject.GetComponent<RectTransform>();
        var blueRectTransform = _blueSlider.gameObject.GetComponent<RectTransform>();

        rectTransform.offsetMin += blueRectTransform.offsetMin - greenRectTransform.offsetMin;
        rectTransform.offsetMax += blueRectTransform.offsetMax - greenRectTransform.offsetMax;
        rectTransform.sizeDelta += (blueRectTransform.sizeDelta - greenRectTransform.sizeDelta) / new Vector2(2f, 1f);

        container.transform.position += new Vector3(0f, 0.05f, 0f);

        _alphaSlider = sliderComponent;
        _alphaSlider.colorDidChangeEvent += _rgbPanelController.HandleSliderColorDidChange;
    }
    
    private void Refresh()
    {
        _siraLog.Warn($"{_rgbPanelController}");
        if (_rgbPanelController == null) return;
        _redSlider!.numberOfSteps = _config.Steps;
        _greenSlider!.numberOfSteps = _config.Steps;
        _blueSlider!.numberOfSteps = _config.Steps;

        switch (_config.EnableAlphaSlider)
        {
            case false when _alphaSlider != null:
                _alphaSlider.colorDidChangeEvent -= _rgbPanelController.HandleSliderColorDidChange;
                _alphaSlider.transform.parent.position -= new Vector3(0f, 0.05f, 0f);
                Object.Destroy(_alphaSlider.gameObject);
                _alphaSlider = null;
                break;
            case true when _alphaSlider == null:
                CreateAlphaSlider();
                break;
            case true when _alphaSlider != null:
                _alphaSlider.numberOfSteps = _config.Steps;
                break;
        }
        
        _rgbPanelController.RefreshSlidersColors();
        _rgbPanelController.RefreshSlidersValues();
    }
}