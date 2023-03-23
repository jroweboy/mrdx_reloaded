using System.ComponentModel;
using widescreen.Template.Configuration;

namespace widescreen.Configuration;

public class Config : Configurable<Config>
{
    
    [DisplayName("Aspect Ratio")]
    [Description("Defaults to 16:9")]
    [DefaultValue(AspectRatioEnum.Auto)]
    public AspectRatioEnum AspectRatio { get; set; } = AspectRatioEnum.Auto;

    public enum AspectRatioEnum
    {
        Auto,
        Force_4_3,
        Force_16_9,
    }
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}