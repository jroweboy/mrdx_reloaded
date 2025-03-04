

using MRDX.Game.ABD_Tournaments.Template;
using MRDX.Game.ABD_Tournaments.Configuration;
using Reloaded.Universal.Redirector.Interfaces;

using System.Drawing;
using MRDX.Base.ExtractDataBin.Interface;
using MRDX.Base.Mod;
using MRDX.Base.Mod.Interfaces;
using Reloaded.Memory.Sources;

using Config = MRDX.Game.ABD_Tournaments.Configuration.Config;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Mod.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Hooks;

using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Resources;
//using static MRDX.Base.Mod.Interfaces.TournamentData;
//using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;


namespace MRDX.Game.ABD_Tournaments;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    #region Reloaded Templating
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

    [HookDef(BaseGame.Mr2, Region.Us, "55 8B EC 53 8B 5D 08 8A C3 24 03 02 C0 56 57 8B F9 BE 01 00 00 00 B1 07")]
    [Function(CallingConventions.Fastcall)]
    public delegate void CheckShrineUnlockRequirementHook(nint parent);

    private readonly WeakReference<IRedirectorController> _redirector;

    private IHook<UpdateGenericState>? _updateHook;

    private bool il_tournamentData = false;
    private TournamentData tournamentData;
    private uint lastCheckedWeek = 0;

    private IHook<CheckShrineUnlockRequirementHook>? _shrineUnlockHook;
    private bool monsterUnlockCheckDefaults = false;

    private nuint gameAddress = 0;
 
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        _redirector = _modLoader.GetController<IRedirectorController>();
        _modLoader.GetController<IExtractDataBin>().TryGetTarget(out var extract);
        _modLoader.GetController<IHooks>().TryGetTarget(out var hooks);
        var startupScanner = _modLoader.GetController<IStartupScanner>();
        
        
        if (extract == null) {
            _logger.WriteLine($"[{_modConfig.ModId}] Failed to get extract data bin controller.", Color.Red);
            return; }

        _redirector.TryGetTarget(out var redirect);
        if (redirect == null) {
            _logger.WriteLine($"[{_modConfig.ModId}] Failed to get redirection controller.", Color.Red);
            return; }

        
        if (hooks == null) {
            _logger.WriteLine($"[{_modConfig.ModId}] Could not get hook controller.", Color.Red);
            return; }

        

        hooks.AddHook<UpdateGenericState>(SetupUpdateHook)
            .ContinueWith(result => _updateHook = result.Result.Activate());

        
        if (startupScanner != null) {
            startupScanner.TryGetTarget(out var scanner);
            SetupCheckShrineUnlockRequirementsHookX(scanner);
        }

        SetupMonsterBreeds(extract.ExtractedPath);

        
        

        

        //hooks.AddHook<CheckShrineUnlockRequirementHook>(SetupCheckShrineUnlockRequirementsHook)
        //    .ContinueWith(result => _shrineUnlockHook = result.Result.Activate());
        //_logger.WriteLine(sm.ToString(), Color.Blue);
        // startupScanner.TryGetTarget(out var scanner)) SetupCheckShrineUnlockRequirementsHookX(scanner);
        //else
        //_logger.WriteLine($"[{_modConfig.ModId}] Could not load startup scanner!");
    }

    private void SetupMonsterBreeds(string gamePath) {
        MonsterBreed.SetupMonsterBreedList(gamePath);
    }

    private void SetupUpdateHook(nint parent)
    {
        var b = Mr2StringExtension.AsBytes(Mr2StringExtension.AsMr2("Oakleyman"));
        _updateHook!.OriginalFunction(parent);
        gameAddress = (nuint)Base.Mod.Base.ExeBaseAddress;

        var tournamentAddress = gameAddress + 0x548D10;
        var currentWeekAddress = gameAddress + 0x379444;

        AdvanceMonthUpdateTournamentMonsters(currentWeekAddress);
        UpdateInGameTournamentData(tournamentAddress);
        
         _logger.Write($"[{_modConfig.ModId}] update.", Color.Red);   
    }


    /// <summary> Performs a one-time load of the original Tournament Monster Data (taikai_en.flk) </summary>
    private void UpdateInGameTournamentData(nuint tournamentAddress)
    {
        // We are looking for a header of '0600 0000 0000 0000 2000 0000 F404 0000'
        Memory.Instance.ReadRaw(tournamentAddress, out byte[] header, 64);
        if (header[0] == 0x06 && header[8] == 0x20 && header[12] == 0xF4 && header[13] == 0x04)
        {
            if (!il_tournamentData) { ReadTournamentMonsterData(tournamentAddress); }

            var enemyAddresses = tournamentAddress + 0xA8C;
            List<ABD_TournamentMonster> monsters = tournamentData.GetTournamentMembers(1, 50);
            for ( var i = 1; i <= 50; i++ )
            {
                Memory.Instance.WriteRaw(enemyAddresses + ((nuint)(i * 60)), monsters[i-1].monster.raw_bytes);
            }
        }
    }

    private void AdvanceMonthUpdateTournamentMonsters(nuint currentWeekAddress) {
        Memory.Instance.Read<uint>( currentWeekAddress, out uint currentWeek );
        if ( lastCheckedWeek != currentWeek ) {
            lastCheckedWeek = currentWeek;
            if ( lastCheckedWeek % 4 == 0 ) {
                _logger.WriteLine( "Advancing Month!", Color.Blue );
                if ( il_tournamentData ) { tournamentData.AdvanceMonth(); }
            }
        }
    }
    /// <summary> Performs a one-time load of the original Tournament Monster Data (taikai_en.flk) </summary>
    private void ReadTournamentMonsterData(nuint tournamentAddress)
    {
        il_tournamentData = true;

        tournamentData = new TournamentData();

        var enemyAddresses = tournamentAddress + 0xA8C;
        _logger.WriteLine("Read from " + enemyAddresses, Color.Lime);
        for ( var i = 0; i < 120; i++ )
        {
            Memory.Instance.ReadRaw(enemyAddresses + ((nuint) (i * 60)), out byte[] tm_raw, 60);
            TournamentMonster tm = new(tm_raw);
            tournamentData.AddExistingMonster(tm, i);
            
            _logger.WriteLine("Monster " + i + " Parsed: " + tm, Color.Lime);

            string bytes = "";
            for ( var z = 0; z < 60; z++ )
            {
                bytes += tm_raw[z] + ",";
            }_logger.WriteLine(bytes, Color.Green);
        }
    }

    private void SetupCheckShrineUnlockRequirementsHookX(IStartupScanner scanner)
    {
        scanner.AddMainModuleScan("55 8B EC 53 8B 5D 08 8A C3 24 03 02 C0 56 57 8B F9 BE 01 00 00 00 B1 07", result =>
        {
            var addr = (nuint)(Base.Mod.Base.ExeBaseAddress + result.Offset);
            Memory.Instance.SafeRead(addr + 0x2F, out ushort smx);
            _logger.WriteLine(result.Offset.ToString(), Color.Blue);
            Memory.Instance.SafeWrite(addr + 0x2f, (ushort)37008);
            //_shrineUnlockHook = new AsmHook(modifyCoords, addr, AsmHookBehaviour.ExecuteAfter).Activate();
        });
    }

    private void SetupCheckShrineUnlockRequirementsHook(nint parent)
    {

        
        // 37008 = 90 90, Two No Op Instructions
        // 0x2F (47 Bytes) from Function Start)

        if (!monsterUnlockCheckDefaults)
        {
            //_logger.WriteLine(parent.ToString(), Color.Blue);
            //Memory.Instance.SafeRead(parent + 0x2F, out uint smx);

            

            //Memory.Instance.SafeWrite(parent + 0x2f, (uint)2313);

            monsterUnlockCheckDefaults = true;
        }
        _shrineUnlockHook!.OriginalFunction(parent);
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }

    /// <summary>
    ///     Returns true if the suspend functionality is supported, else false.
    /// </summary>
    public override bool CanSuspend() { return true; }

    
    public override bool CanUnload() { return true; }

    
    public override void Suspend() {
        var redirector = _modLoader.GetController<IRedirectorController>();
        if (redirector != null && redirector.TryGetTarget(out var re)) re.Disable();
    }

    
    public override void Unload() { Suspend(); }

    
    public override void Resume() {
        var redirector = _modLoader.GetController<IRedirectorController>();
        if (redirector != null && redirector.TryGetTarget(out var re)) re.Enable();
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}