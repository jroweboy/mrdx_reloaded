using MRDX.Base.BattleSimulator.Configuration;
using MRDX.Base.BattleSimulator.Template;
using MRDX.Base.ExtractDataBin.Interface;
using MRDX.Base.Mod.Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Mod.Interfaces;
using Reloaded.Universal.Redirector.Interfaces;

namespace MRDX.Base.BattleSimulator;

/// <summary>
///     Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    [HookDef(BaseGame.Mr2, Region.Us,
        "55 8B EC 6A FF 68 ?? ?? ?? ?? 64 A1 ?? ?? ?? ?? 50 51 53 56 57 A1 ?? ?? ?? ?? 33 C5 50 8D 45 ?? 64 A3 ?? ?? ?? ?? 8B F1 89 75 ?? 51 68 ?? ?? ?? ?? FF 75 ?? C7 46 ?? ?? ?? ?? ?? C7 86 ?? ?? ?? ?? 0C 00 00 00 E8 ?? ?? ?? ?? 8B 45 ?? C7 45 ?? 00 00 00 00 89 46 ?? 8B 45 ??")]
    [Function([FunctionAttribute.Register.ecx], FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Callee, [
        FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi
    ])]
    public delegate nint CModeComVsComConstructor(nint thisPtr, nint basePtr, nint unk2, nint unk3, byte leftMonId,
        byte rightMonId);

    /// <summary>
    ///     Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    ///     Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    ///     The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    /// <summary>
    ///     Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    ///     Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    ///     Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    private IHook<CModeComVsComConstructor>? _hookComVComConstructor;

    private List<IBattleMonsterData> _monsters = [];

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        var maybeGame = _modLoader.GetController<IGame>();
        if (maybeGame != null && maybeGame.TryGetTarget(out var game))
            game.OnMonsterBreedsLoaded.Subscribe(b =>
            {
                var redirector = _modLoader.GetController<IRedirectorController>();
                if (redirector != null && redirector.TryGetTarget(out var redirect))
                {
                    redirect.Loading += OnFileLoad;
                    Logger.Info("Tournament Simulator loaded!");
                }
                else
                {
                    Logger.Error("Cannot start battle simulator. Redirector not found.");
                }
            });
        // Debugger.Launch();
        _modLoader.GetController<IHooks>().TryGetTarget(out var hooks);
        hooks?.AddHook<CModeComVsComConstructor>(ComVComHook)
            .ContinueWith(result => _hookComVComConstructor = result.Result.Activate());
    }


    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod()
    {
    }
#pragma warning restore CS8618

    #endregion

    private nint ComVComHook(nint thisPtr, nint basePtr, nint unk2, nint unk3, byte leftMonId, byte rightMonId)
    {
        Logger.Error($"Testing battle: {unk2} {unk3} {leftMonId} {rightMonId}");

        return _hookComVComConstructor!.OriginalFunction(thisPtr, basePtr, unk2, unk3, leftMonId, rightMonId);
    }

    private void OnFileLoad(string fileName)
    {
        var tournamentMonsterFile = Path.Combine(IExtractDataBin.ExtractedPath!, @"\mf2\data\taikai\taikai_en.flk");
        if (fileName != tournamentMonsterFile)
            return;
        _monsters = LoadBattleMonsters(tournamentMonsterFile);
    }

    private static List<IBattleMonsterData> LoadBattleMonsters(string tournamentMonsterFile)
    {
        // Load all tourney monsters from this file so we can simulate battles
        var raw = File.ReadAllBytes(tournamentMonsterFile);
        var monsters = new List<IBattleMonsterData>();
        const int baseFilePos = 0xA8C + 60;
        for (var i = 1; i < 119; i++)
        {
            var start = baseFilePos + i * 60;
            var end = start + 60;
            monsters.Add(IBattleMonsterData.FromBytes(raw[start..end]));
        }

        return monsters;
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
}