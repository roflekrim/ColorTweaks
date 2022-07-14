using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using ColorTweaks.Configuration;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using Zenject;

namespace ColorTweaks.UI;

[Bind(Location.Menu)]
internal class SettingsViewController : IInitializable, IDisposable
{
    private readonly PluginConfig _config;
    
    public SettingsViewController(PluginConfig config)
    {
        _config = config;
    }

    public void Initialize()
    {
        BSMLSettings.instance.AddSettingsMenu("ColorTweaks", "ColorTweaks.UI.BSML.settings.bsml", this);
    }

    public void Dispose()
    {
        if (BSMLSettings.instance != null) 
            BSMLSettings.instance.RemoveSettingsMenu("ColorTweaks");
    }
}