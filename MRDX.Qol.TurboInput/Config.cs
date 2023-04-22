using System.ComponentModel;
using MRDX.Qol.TurboInput.Template.Configuration;

namespace MRDX.Qol.TurboInput.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("Turbo Cross")]
    [Description("Enable Turbo for Cross `X`")]
    [DefaultValue(true)]
    public bool TurboCross { get; set; } = true;

    [DisplayName("Turbo Circle")]
    [Description("Enable Turbo for Circle `O`")]
    [DefaultValue(true)]
    public bool TurboCircle { get; set; } = true;

    [DisplayName("Turbo Triangle")]
    [Description("Enable Turbo for Triangle `/\\`")]
    [DefaultValue(true)]
    public bool TurboTriangle { get; set; } = true;

    [DisplayName("Turbo Square")]
    [Description("Enable Turbo for Square `[]`")]
    [DefaultValue(true)]
    public bool TurboSquare { get; set; } = true;

    [DisplayName("Use Frame Count")]
    [Description("Use the speed and delay values as number of frames instead of number of milliseconds")]
    [DefaultValue(false)]
    public bool UseFrameCount { get; set; } = false;

    [DisplayName("Speed")]
    [Description(
        "Number of milliseconds or number of frames BETWEEN each turbo press.\n" +
        "A value of 0 will toggle the button every frame.\n" +
        "A value of 300 ms will hold the button for 300 ms, and then let go for 300ms (for a button press every 600ms)")]
    [DefaultValue(0)]
    public int Speed { get; set; } = 0;

    [DisplayName("Delay")]
    [Description("Number of milliseconds or number of frames BEFORE turbo activates.\n" +
                 "A value of 0 will start toggling immediately after holding.\n" +
                 "If you find that sometimes you are seeing double presses, try increasing the delay value a little")]
    [DefaultValue(100)]
    public int Delay { get; set; } = 100;
}

/// <summary>
///     Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
///     Override elements in <see cref="ConfiguratorMixinBase" /> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}