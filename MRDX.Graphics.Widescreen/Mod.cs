using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using MRDX.Graphics.Widescreen.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;

namespace MRDX.Graphics.Widescreen;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private const uint Mr2HeightOffset = 0x165C93;
    private const uint Mr2WidthOffset = 0x165C9A;
    
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect([In] nint hWnd, [Out] out RECT lpRect);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }
    
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;


        var thisProcess = Process.GetCurrentProcess();
        var baseAddress = thisProcess.MainModule!.BaseAddress;
        
        var memory = Memory.Instance;
        var originalWidth = 320.0f;
        var originalAspectRatio = ConvertAspectRatio(Config.AspectRatioEnum.Force_4_3);
        var newAspectRatio = ConvertAspectRatio(_configuration.AspectRatio);
        var rescaledWidth = originalWidth * newAspectRatio / originalAspectRatio;
        memory.SafeWrite(baseAddress + Mr2WidthOffset, rescaledWidth);
        // memory.SafeWrite(baseAddress + Mr2HeightOffset, 1080.0f);
    }
    
    private static float ConvertAspectRatio(Config.AspectRatioEnum val)
    {
        switch (val)
        {
            default:
            case Config.AspectRatioEnum.Auto:
                return 16.0f / 9; 
            case Config.AspectRatioEnum.Force_4_3:
                return 4.0f / 3;
            case Config.AspectRatioEnum.Force_16_9:
                return 16.0f / 9;
        }
    }

    #region Standard Overrides

    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }

    #endregion

    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod()
    {
    }
#pragma warning restore CS8618

    #endregion
}