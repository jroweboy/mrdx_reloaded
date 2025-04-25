
using MRDX.Qol.MagicBananaStatic.Template;
using MRDX.Qol.MagicBananaStatic.Configuration;

using MRDX.Base.Mod.Interfaces;

using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions;

using System.Diagnostics;
using System.Drawing;

using Reloaded.Hooks.Definitions.X86;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;

namespace MRDX.Qol.MagicBananaStatic;

[HookDef( BaseGame.Mr2, Region.Us, "55 8B EC B9 01 00 00 00 83 EC 18" )]
[Function( CallingConventions.Cdecl )]
public delegate void H_ItemUsed ( int p1, uint p2, uint p3 );



public class Mod : ModBase // <= Do not Remove.
{
    #region Reloaded Template
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
    #endregion

    private IHooks _iHooks;

    private IHook<UpdateGenericState>? _hook_genericUpdate;
    private IHook<H_ItemUsed> _hook_itemUsed;

    IMonster _monsterCurrent;
    IMonster _monsterSnapshot;

    private nuint _address_game;
    private nuint _address_currentweek;
    private nuint _address_monsterdata;

    public bool _snapshotUpdate = true;

    public byte _itemGiveHookCount = 0;
    public byte _itemIdGiven = 0;
    public byte _itemOriginalIdGiven = 0;
    public bool _itemGivenSuccess = false;

    public bool _itemHandleMagicBananas = false;
    public byte[] _magicBanana_preStats = new byte[ 5 ];

    public Mod ( ModContext context ) {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        _modLoader.GetController<IHooks>().TryGetTarget( out _iHooks );
        _modLoader.GetController<IGame>().TryGetTarget( out var iGame );

        var thisProcess = Process.GetCurrentProcess();
        var module = thisProcess.MainModule!;
        _address_game = (nuint) module.BaseAddress.ToInt64();

        if ( _iHooks == null ) { _logger.WriteLine( $"[{_modConfig.ModId}] Could not get hook controller.", Color.Red ); return; }
        if ( iGame == null ) { _logger.WriteLine( $"[{_modConfig.ModId}] Could not get iGame controller.", Color.Red ); return; }

        _iHooks.AddHook<UpdateGenericState>( SetupHookGenericUpdate ).ContinueWith( result => _hook_genericUpdate = result.Result.Activate() );
        _iHooks.AddHook<H_ItemUsed>( SetupHookItemUsed ).ContinueWith( result => _hook_itemUsed = result.Result.Activate() );

        _monsterCurrent = iGame.Monster;
        iGame.OnMonsterChanged += MonsterChanged;

        Debugger.Launch();
    }

    private void MonsterChanged ( IMonsterChange mon ) {
        if ( _snapshotUpdate ) {
            _monsterSnapshot = mon.Previous;
            _snapshotUpdate = false;
        }
    }
       

    private void SetupHookGenericUpdate ( nint parent ) {
        _hook_genericUpdate!.OriginalFunction( parent );

        if ( _itemGiveHookCount >= 5 ) {
            if ( _itemHandleMagicBananas ) { StaticBananas(); _itemGiveHookCount = 0; }
        }
        else { _itemGiveHookCount++; }

    }

    private void SetupHookItemUsed ( int p1, uint p2, uint p3 ) {

        uint addrItemId = (uint) p1 + 76;
        Memory.Instance.SafeRead( addrItemId, out _itemOriginalIdGiven );

        // TODO: TESTING PURPOSES ONLY INFINITE FEEDING!
        //Memory.Instance.SafeWrite( _address_monsterdata + 0xf6, 0 );

        _monsterCurrent.ItemUsed = false;

        //Memory.Instance.SafeRead( _address_monsterdata + 0xf6, out _itemGivenSuccess ); // Location of "Item already given this week" flag
        _itemGivenSuccess = !_monsterCurrent.ItemUsed;

        if ( _itemOriginalIdGiven == 28 ) { // Magic Bananas
            if ( _itemGivenSuccess ) {
                _snapshotUpdate = true;
                _itemHandleMagicBananas = true;

                /*Memory.Instance.SafeRead( _address_monsterdata + 0x1f, out _magicBanana_preStats[ 0 ] ); // Monster Fatigue
                Memory.Instance.SafeRead( _address_monsterdata + 0x23, out _magicBanana_preStats[ 1 ] ); // Monster Stress
                Memory.Instance.SafeRead( _address_monsterdata + 0x24, out _magicBanana_preStats[ 2 ] ); // Monster Spoil
                Memory.Instance.SafeRead( _address_monsterdata + 0x25, out _magicBanana_preStats[ 3 ] ); // Monster Fear
                Memory.Instance.SafeRead( _address_monsterdata + 0x26, out _magicBanana_preStats[ 4 ] ); // Monster Form*/

            }
        }

        _hook_itemUsed!.OriginalFunction( p1, p2, p3 );

        _itemGiveHookCount = 0;
    }

    private void StaticBananas () {
        if ( !_itemHandleMagicBananas ) { return; }

        if ( _configuration._config_bananaType == Config.EConfBananaType.Stress ) {
            _monsterCurrent.Fatigue = _monsterSnapshot.Fatigue;
            _monsterCurrent.Stress = (sbyte) ( _monsterSnapshot.Stress - 10 );
            _monsterCurrent.LoyalSpoil = (byte) ( _monsterSnapshot.LoyalSpoil + 10 );
            _monsterCurrent.LoyalFear = (byte) ( _monsterSnapshot.LoyalFear + 10 );
            _monsterCurrent.FormRaw = (sbyte) ( _monsterSnapshot.FormRaw - 1 );
        }
        /*Memory.Instance.SafeWrite( _address_monsterdata + 0x1f, _magicBanana_preStats[ 0 ] ); // Monster Fatigue
        Memory.Instance.SafeWrite( _address_monsterdata + 0x23, Math.Clamp( _magicBanana_preStats[ 1 ] - 10, 0, 100 ) ); // Monster Stress
        Memory.Instance.SafeWrite( _address_monsterdata + 0x24, Math.Clamp( _magicBanana_preStats[ 2 ] + 10, 0, 100 ) ); // Monster Spoil
        Memory.Instance.SafeWrite( _address_monsterdata + 0x25, Math.Clamp( _magicBanana_preStats[ 3 ] + 10, 0, 100 ) ); // Monster Fear
        //Memory.Instance.SafeWrite( _address_monsterdata + 0x26, Math.Clamp( _magicBanana_preStats[ 4 ] - 1, 0, 100 ) ); // Monster Form TODO: Form is weird and non-linear. Maths later
        Memory.Instance.SafeWrite( _address_monsterdata + 0x1f, _magicBanana_preStats[ 4 ] ); // Monster Form - NOT REALISTIC*/


        _itemHandleMagicBananas = false;
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
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}