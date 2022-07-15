using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using UnityEngine.Events;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace ColorTweaks.Configuration;

internal class PluginConfig
{
    internal enum XDisplayMode
    {
        Default,
        Normalized,
        MinToMax,
        Percentage
    }
    
    public virtual bool Enabled { get; set; } = true;
    public virtual float Increment { get; set; } = 0.1f;
    public virtual int Steps { get; set; } = 256;

    [UseConverter]
    public virtual XDisplayMode DisplayMode { get; set; } = XDisplayMode.Default;

    #region RGB Sliders

    public virtual float RedSliderMin { get; set; } = 0f;
    public virtual float RedSliderMax { get; set; } = 1f;

    public virtual float GreenSliderMin { get; set; } = 0f;
    public virtual float GreenSliderMax { get; set; } = 1f;

    public virtual float BlueSliderMin { get; set; } = 0f;
    public virtual float BlueSliderMax { get; set; } = 1f;

    #endregion

    #region HSV Panel

    public virtual float SaturationMin { get; set; } = 0f;
    public virtual float SaturationMax { get; set; } = 1f;

    public virtual float ValueMin { get; set; } = 0f;
    public virtual float ValueMax { get; set; } = 1f;

    #endregion

    public virtual bool EnableAlphaSlider { get; set; } = false;
    public virtual float AlphaSliderMin { get; set; } = 0f;
    public virtual float AlphaSliderMax { get; set; } = 1f;

    public virtual void Changed() => PropertyChanged.Invoke();
    
    [Ignore]
    internal UnityEvent PropertyChanged = new(); 

    // BSML
    [UIValue("DisplayModes")]
    [Ignore]
    protected List<object> BsmlDisplayModes = 
        Enum.GetValues(typeof(PluginConfig.XDisplayMode)).Cast<object>().ToList();
}