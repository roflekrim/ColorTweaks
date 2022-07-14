using System.Reflection;
using IPA;
using IPA.Config;
using ColorTweaks.Configuration;
using ColorTweaks.Installers;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace ColorTweaks
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        [Init]
        public void Init(Zenjector zenjector, IPALogger logger, Config config)
        {
            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseLogger(logger);
            zenjector.UseAutoBinder();
            
            zenjector.Install<AppInstaller>(Location.App, config.Generated<PluginConfig>());
        }
    }
}