using System.Diagnostics;
using System.Runtime.InteropServices;
using CoreAudio;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Memory.Sources;
using volume_config.Template;
using volume_config.Configuration;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace volume_config;

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
    private static ILogger? _logger;

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
    
    private readonly nint _musicVolumeAddress;
    private readonly nint _sfxVolumeAddress;
    private readonly nint _sfxVolumeMr2Address;
    // private static nint _mediaAddress;
    private readonly bool _isMr2Dx;
    
    private static IHook<PlayFmv>? _playFmvHook;
    private static IHook<StopFmv>? _stopFmvHook;
    private static SimpleAudioVolume? _volumeControl;
    private static float _currentFmvVolume;
    private static bool _isPlayingFmv = false;
    private static float _originalVolume = -1.0f;

    private const nint Mr1MusicOffset = 0xE4C428;
    // The instruction that the volume is initialized with originally
    private const nint Mr2MusicLoadOffset = 0x166A60;
    // The location that the volume is copied to when creating the audio render class
    private const nint Mr2MusicRunOffset = 0x1D481D0;
    // For MR2DX the value is loaded from the code every time it seems (?)
    private const nint Mr2SfxOffset = 0x1677DF;
    // Additional offset for default volume that can potentially get called when triggering the same audio rapidly
    private const nint Mr2DefaultSfxOffset = 0x1677FB;

    private static Guid GUID = Guid.NewGuid();
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
        _modLoader.GetController<IStartupScanner>().TryGetTarget(out var startupScanner);
        
        startupScanner?.AddMainModuleScan(PlayFmvSignature, 
            result => _playFmvHook = _hooks!.CreateHook<PlayFmv>(PlayFmvHook, (long)baseAddress + result.Offset).Activate());
        startupScanner?.AddMainModuleScan(StopFmvSignature, 
            result => _stopFmvHook = _hooks!.CreateHook<StopFmv>(StopFmvHook, (long)baseAddress + result.Offset).Activate());
        
        if (_modLoader.GetAppConfig().AppId.ToLower() == "mf.exe")
        {
            _isMr2Dx = false;
            _musicVolumeAddress = baseAddress + Mr1MusicOffset;
            _sfxVolumeAddress = baseAddress + Mr1MusicOffset + 4;
        }
        else
        {
            _isMr2Dx = true;
            // Write once to the music volume load address
            Memory.Instance.SafeWrite(baseAddress + Mr2MusicLoadOffset, ClampVolume(_configuration.MusicVolume));
            _musicVolumeAddress = baseAddress + Mr2MusicRunOffset;
            _sfxVolumeAddress = baseAddress + Mr2SfxOffset;
            _sfxVolumeMr2Address = baseAddress + Mr2DefaultSfxOffset;
            var deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            MMDevice device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            _originalVolume = device.AudioEndpointVolume?.MasterVolumeLevelScalar ?? 1.0f;
        }

        WriteVolumeToMemory(_configuration);
        _logger.WriteLine($"[{_modConfig.ModId}] done building mod");
    }

    #region Standard Overrides

    public override void ConfigurationUpdated(Config configuration)
    {
        WriteVolumeToMemory(configuration);
        _configuration = configuration;
        _logger!.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }


    #endregion
    
    #region Custom Code for writing the volume to memory
    private void WriteVolumeToMemory(Config configuration)
    {
        var memory = Memory.Instance;
        memory.SafeWrite(_musicVolumeAddress, ClampVolume(configuration.MusicVolume));
        memory.SafeWrite(_sfxVolumeAddress, ClampVolume(configuration.SfxVolume));
        if (_isMr2Dx)
        {
            memory.SafeWrite(_sfxVolumeMr2Address, ClampVolume(configuration.SfxVolume));
            _currentFmvVolume = ClampVolume(configuration.FmvVolume);
            if (_isPlayingFmv)
            {
                _logger!.WriteLine($" setting volume with existing volume control");
                _currentFmvVolume = ClampVolume(configuration.FmvVolume);
                if (_volumeControl != null)
                    _volumeControl.MasterVolume = _currentFmvVolume;
            }
        }
    }

    private static float ClampVolume(int volume)
    {
        return Math.Max(0, Math.Min(100, volume)) / 100.0f;
    }

    private static int PlayFmvHook([In] nint self, [In] nint unk)
    {
        _isPlayingFmv = true;
        if (_volumeControl == null)
        {
            // We have to delay finding the current audio stream until after the client starts up the Media Foundation session
            var device = GetDefaultAudioDevice();
            foreach (var session in device.AudioSessionManager2!.Sessions!)
            {
                var proc = Process.GetProcessById((int)session.ProcessID);
                if (proc.ProcessName.ToLower() == "mf2")
                {
                    _volumeControl = session?.SimpleAudioVolume;
                    break;
                }
            }            
        }
        
        if (_volumeControl != null)
            _volumeControl.MasterVolume = _currentFmvVolume;
        return _playFmvHook!.OriginalFunction(self, unk);
    }

    private static nint StopFmvHook([In] nint self, [In] byte shouldDestroy)
    {
        _isPlayingFmv = false;
        if (_volumeControl != null)
            _volumeControl.MasterVolume = _originalVolume;
        return _stopFmvHook!.OriginalFunction(self, shouldDestroy);
    }

    private static MMDevice GetDefaultAudioDevice()
    {
        var deviceEnumerator = new MMDeviceEnumerator(GUID);
        return deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }
    
    #endregion

    #region DLL Imports and COM madness
    // Function in MR2 that will start playing the FMV that we want to hook to set the volume before it starts playing.
    private const string PlayFmvSignature = "55 8B EC 83 EC 14 8B 45 ?? 56";
    [Function(CallingConventions.MicrosoftThiscall)]
    private delegate int PlayFmv([In] nint self, [In] nint unk);

    private const string StopFmvSignature =
        "55 8B EC 6A FF 68 ?? ?? ?? ?? 64 A1 ?? ?? ?? ?? 50 56 A1 ?? ?? ?? ?? 33 C5 50 8D 45 ?? 64 A3 ?? ?? ?? ?? 8B F1 C7 45 ?? 00 00 00 00 C7 06 ?? ?? ?? ??";
    [Function(CallingConventions.MicrosoftThiscall)]
    private delegate nint StopFmv([In] nint self, [In] byte shouldDestroy);

    #endregion
    
    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod()
    {
    }
#pragma warning restore CS8618

    #endregion

}