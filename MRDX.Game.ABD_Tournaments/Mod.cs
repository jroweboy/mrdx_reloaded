

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
using System.IO;
//using static MRDX.Base.Mod.Interfaces.TournamentData;
//using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace MRDX.Game.ABD_Tournaments;

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

    public TournamentData tournamentData;
    private uint lastCheckedWeek = 0;

    private SaveFileManager _saveFileManager;

    private IHook<CheckShrineUnlockRequirementHook>? _shrineUnlockHook;
    private bool monsterUnlockCheckDefaults = false;

    private string _gamePath = "";
    private nuint gameAddress = 0;
    private nuint _address_tournamentmonsters = 0;
    private nuint _address_currentweek = 0;
    private nuint _address_unlockedmonsters = 0;

    private List<MonsterGenus> _unlockedmonsters;

    
 
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        _saveFileManager = new SaveFileManager( this, _modLoader, _modConfig, _logger );

        _redirector = _modLoader.GetController<IRedirectorController>();
        _modLoader.GetController<IExtractDataBin>().TryGetTarget(out var extract);
        _modLoader.GetController<IHooks>().TryGetTarget(out var hooks);

        var startupScanner = _modLoader.GetController<IStartupScanner>();

        gameAddress = (nuint) Base.Mod.Base.ExeBaseAddress;

        _address_unlockedmonsters = gameAddress + 0x3795A2;
        _address_tournamentmonsters = gameAddress + 0x548D10;
        _address_currentweek = gameAddress + 0x379444;

        _unlockedmonsters = new List<MonsterGenus>();

        if (extract == null) {
            _logger.WriteLine($"[{_modConfig.ModId}] Failed to get extract data bin controller.", Color.Red);
            return; }

        _redirector.TryGetTarget(out var redirect);
        if (redirect == null) {
            _logger.WriteLine($"[{_modConfig.ModId}] Failed to get redirection controller.", Color.Red);
            return; }
        else {
            redirect.Loading += _saveFileManager.SaveDataMonitor;
        }

        _gamePath = extract.ExtractedPath;

        if (hooks == null) {
            _logger.WriteLine($"[{_modConfig.ModId}] Could not get hook controller.", Color.Red);
            return; }

        hooks.AddHook<UpdateGenericState>(SetupUpdateHook)
            .ContinueWith(result => _updateHook = result.Result.Activate());

        
        if (startupScanner != null) {
            startupScanner.TryGetTarget(out var scanner);
            SetupCheckShrineUnlockRequirementsHookX(scanner);
        }

        tournamentData = new TournamentData(_logger);
        SetupMonsterBreeds();
        SetupTournamentParticipantsFromTaikai();

        //hooks.AddHook<CheckShrineUnlockRequirementHook>(SetupCheckShrineUnlockRequirementsHook)
        //    .ContinueWith(result => _shrineUnlockHook = result.Result.Activate());
        //_logger.WriteLine(sm.ToString(), Color.Blue);
        // startupScanner.TryGetTarget(out var scanner)) SetupCheckShrineUnlockRequirementsHookX(scanner);
        //else
        //_logger.WriteLine($"[{_modConfig.ModId}] Could not load startup scanner!");
    }

    /*private void PrintOnFileLoad ( string path ) {
        
            if ( path.Contains( "usb#vid" ) || path.Contains( "hid#vid" ) )
                return;

        _logger.WriteLine( $"CUSTOM MONITOR: {path}", Color.Blue );
    }*/

    private void SetupMonsterBreeds() {
        MonsterBreed.SetupMonsterBreedList(_gamePath);
    }

    private void SetupTournamentParticipantsFromTaikai() {

        tournamentData.ClearAllData();

        string tournamentMonsterFile = _gamePath + "\\mf2\\data\\taikai\\taikai_en.flk";
        byte[] rawmonster = new byte[ 60 ];

        FileStream fs = new FileStream( tournamentMonsterFile, FileMode.Open );
        fs.Position = 0xA8C; // This relies upon nothing earlier in the file being appended. 
        for ( var i = 1; i < 120; i++ ) { // 0 = Dummy Monster so skip. 119 in the standard file.
            fs.Read( rawmonster, 0, 60 );
            TournamentMonster tm = new( rawmonster );
            tournamentData.AddExistingMonster( tm, i );

            string bytes = ""; for ( var z = 0; z < 60; z++ ) { bytes += rawmonster[ z ] + ","; }
            _logger.WriteLine( "Monster " + i + " Parsed: " + tm, Color.Lime ); //_logger.WriteLine( bytes, Color.Green );
        }

        tournamentData._initialized = true;
    }

    private void SetupUpdateHook(nint parent)
    {
        _updateHook!.OriginalFunction(parent);

        GetUnlockedMonsters( _address_unlockedmonsters );
        LoadGameUpdateTournamentData();
        AdvanceWeekUpdateTournamentMonsters(_address_currentweek, _unlockedmonsters);
        UpdateMemoryTournamentData(_address_tournamentmonsters);
        
        _logger.Write($"[{_modConfig.ModId}] update.", Color.Red);   
    }

    private void GetUnlockedMonsters(nuint unlockAddress) {
        Memory.Instance.ReadRaw( unlockAddress, out byte[] unlocks, 37 );

        _logger.Write( "[ABD Tournaments]:", Color.Pink );
        _unlockedmonsters.Clear();
        for ( var i = 0; i < unlocks.Length; i++ ) {
            if ( unlocks[ i ] == 0x01 ) {
                _unlockedmonsters.Add( (MonsterGenus) i );
                _logger.Write( i + ",", Color.Pink );
            }
        }
    }

    private void LoadGameUpdateTournamentData() {
        if ( _saveFileManager._saveData_gameLoaded ) {
            _logger.WriteLineAsync( "Mod: Thinks the game has been loaded." );
            List<ABD_TournamentMonster> monsters = _saveFileManager.LoadABDTournamentData();
            if ( monsters.Count == 0 ) {
                _logger.WriteLineAsync( "Mod: No custom tournament data found. Loading taikai_en." );
                SetupTournamentParticipantsFromTaikai();
            } else {
                _logger.WriteLineAsync( "Mod: Found Data for " + monsters.Count + " monsters." );
                tournamentData.ClearAllData();
                _logger.WriteLineAsync( "Mod: Cleared Existing Data" );
                foreach(ABD_TournamentMonster abdm in monsters) {
                    tournamentData.AddExistingMonster( abdm );
                }
            }
            tournamentData._initialized = true;
            tournamentData._firstweek = true;
            _logger.WriteLine( "Mod: Init Complete" );
        }
    }
    /// <summary>  </summary>
    private void UpdateMemoryTournamentData(nuint tournamentAddress)
    {
        // We are looking for a header of '0600 0000 0000 0000 2000 0000 F404 0000'
        Memory.Instance.ReadRaw(tournamentAddress, out byte[] header, 64);
        if (header[0] == 0x06 && header[8] == 0x20 && header[12] == 0xF4 && header[13] == 0x04)
        {
            var enemyAddresses = tournamentAddress + 0xA8C;
            List<ABD_TournamentMonster> monsters = tournamentData.GetTournamentMembers(1, 50);
            for ( var i = 1; i <= 50; i++ )
            {
                Memory.Instance.WriteRaw(enemyAddresses + ((nuint)(i * 60)), monsters[i-1].monster.raw_bytes);
            }
        }
    }

    private void AdvanceWeekUpdateTournamentMonsters(nuint currentWeekAddress, List<MonsterGenus> unlockedmonsters) {
        Memory.Instance.Read<uint>( currentWeekAddress, out uint currentWeek );
        if ( lastCheckedWeek != currentWeek ) {
            _logger.WriteLine( "[ABD Tournaments]: Advancing to week " + currentWeek, Color.Blue );
            lastCheckedWeek = currentWeek;
            tournamentData.AdvanceWeek(currentWeek, unlockedmonsters);
        }
    }

    /// <summary>
    /// This function replaces an assembly jump with two nops. The jmp originally checked the id of the monster against some bizarre set of mathematics that determined whether the monster was a 'default' species (i.e., unlocked at the beginning of the game).
    /// The game has flags that map to each species unlock requirements. Removing this jmp call results in the game checking all species against these flags. For normal gameplay, this does nothing. This 'fix' would only be useful in the instance where
    /// other mods restrict the main breeds available to each player, (i.e., disabling access to Pixie by setting the flag to 0 instead of the 1 it defaults to).
    /// </summary>
    /// <param name="scanner"></param>
    private void SetupCheckShrineUnlockRequirementsHookX(IStartupScanner scanner)
    {
        scanner.AddMainModuleScan("55 8B EC 53 8B 5D 08 8A C3 24 03 02 C0 56 57 8B F9 BE 01 00 00 00 B1 07", result =>
        {
            var addr = (nuint)(Base.Mod.Base.ExeBaseAddress + result.Offset);
            //Memory.Instance.SafeRead(addr + 0x2F, out ushort smx);
            Memory.Instance.SafeWrite(addr + 0x2f, (ushort)37008);
            //_shrineUnlockHook = new AsmHook(modifyCoords, addr, AsmHookBehaviour.ExecuteAfter).Activate();
        });
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