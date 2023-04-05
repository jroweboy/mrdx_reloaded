using System.ComponentModel;
using MRDX.Qol.SkipDrillAnim.Template.Configuration;

namespace MRDX.Qol.SkipDrillAnim;

public class Config : Configurable<Config>
{
    /*
        User Properties:
            - Please put all of your configurable properties here.
    
        By default, configuration saves as "Config.json" in mod user config folder.    
        Need more config files/classes? See Configuration.cs
    
        Available Attributes:
        - Category
        - DisplayName
        - Description
        - DefaultValue

        // Technically Supported but not Useful
        - Browsable
        - Localizable

        The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */
    
    [DisplayName("Auto Skip Drill")]
    [Description("When On, no input is required to skip the drill. When Off, you must press /\\ to skip")]
    [DefaultValue(true)]
    public bool AutoSkip { get; set; } = true;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}