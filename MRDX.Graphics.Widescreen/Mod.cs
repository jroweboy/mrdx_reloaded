using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using MRDX.Graphics.Widescreen.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

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
    private static ILogger _logger = null!;

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

    private const uint Mr2RenderHeightOffset = 0x165C93;
    private const uint Mr2RenderWidthOffset = 0x165C9A;
    
    // private const uint Mr2ViewportHeightOffset = 0x1684F5;
    // private const uint Mr2ViewportInverseWidthOffset = 0x1684FC;

    private const uint Mr2ViewportOffset = 0x1684E5; // (Off[23] = 2/w, Off[16] = -2/h, Off[8] = -w/2, Off[0] = -h/2)

    private readonly long _mr2RenderWidthAddr;
    private readonly long _mr2ViewportAddr;
    private Memory _memory = Memory.Instance;

    private static IHook<CreateOverlay>? _createOverlayHook;

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
        _mr2RenderWidthAddr = baseAddress + Mr2RenderWidthOffset;
        _mr2ViewportAddr = baseAddress + Mr2ViewportOffset;

        UpdateWindowBounds(_configuration.AspectRatio);
        
        _modLoader.GetController<IStartupScanner>().TryGetTarget(out var startupScanner);
        startupScanner?.AddMainModuleScan(CreateOverlaySignature, 
            result => _createOverlayHook = _hooks!.CreateHook<CreateOverlay>(CreateOverlayHook, (long)baseAddress + result.Offset).Activate());
    }

    private float CalculateNewWidth(float width, Config.AspectRatioEnum ratio)
    {
        var originalAspectRatio = ConvertAspectRatio(Config.AspectRatioEnum.Force_4_3);
        var newAspectRatio = ConvertAspectRatio(ratio);
        return width * newAspectRatio / originalAspectRatio;
    }
    
    private void UpdateWindowBounds(Config.AspectRatioEnum ratio)
    {
        const float originalWidth = 320.0f;
        var newWidth = CalculateNewWidth(originalWidth, ratio);
        _memory.SafeWrite(_mr2RenderWidthAddr, newWidth);
        // setting this widens the screen to the window size
        _memory.SafeWrite(_mr2ViewportAddr + 23, 2.0f / newWidth);
        // Other width offset - setting this shifts the screen all the way to the left
        // _memory.SafeWrite(_mr2ViewportAddr + 8, newWidth / -2.0f);
    }
    
    private static float ConvertAspectRatio(Config.AspectRatioEnum val)
    {
        switch (val)
        {
            default:
            case Config.AspectRatioEnum.Auto:
                // TODO adjust window size to match monitor aspect ratio as well?
                // return CalculateAspectRatio();
                return 16.0f / 9;
            case Config.AspectRatioEnum.Force_4_3:
                return 4.0f / 3;
            case Config.AspectRatioEnum.Force_16_9:
                return 16.0f / 9;
        }
    }

    private static float CalculateAspectRatio()
    {
        var hwnd = Process.GetCurrentProcess().MainWindowHandle;
        GetWindowRect(hwnd, out var rect);
        return (float)(rect.Right - rect.Left) / (rect.Bottom - rect.Top);
    }

    private enum OverlayDrawMode
    {
        Unknown0 = 0,
        MainMode = 1,
        Unknown2 = 2,
        Unknown3 = 3,
        Unknown4 = 4,
        NoOverlay = 5,
    }
    private static nint CreateOverlayHook(nint self, OverlayDrawMode drawMode)
    {
        _logger!.WriteLine($"[] original drawmode value: {(uint)drawMode}");
        return _createOverlayHook!.OriginalFunction(self, OverlayDrawMode.NoOverlay);
    }

    private const string CreateOverlaySignature = "55 8B EC 8B 45 ?? 53 8B D9 89 83 ?? ?? ?? ??";

    [Function(CallingConventions.MicrosoftThiscall)]
    private delegate nint CreateOverlay([In] nint self, [In] OverlayDrawMode unkEnum);
    
    
    // TODO: Use this stuff to autodetect the player window size?
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
    
    #region Standard Overrides

    public override void ConfigurationUpdated(Config configuration)
    {
        _configuration = configuration;
        UpdateWindowBounds(configuration.AspectRatio);
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