using ColorTweaks.Configuration;
using Zenject;

namespace ColorTweaks.Installers;

internal class AppInstaller : Installer
{
    private PluginConfig _config;

    internal AppInstaller(PluginConfig config)
    {
        _config = config;
    }
    
    public override void InstallBindings()
    {
        Container.BindInstance(_config);
    }
}