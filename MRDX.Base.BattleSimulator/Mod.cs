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
        "55 8B EC 51 53 56 57 8B F1 0F 1F 80 00 00 00 00")]
    [Function(CallingConventions.Fastcall)]
    public delegate int CModeComVsCom(nint comVsComptr);

    [HookDef(BaseGame.Mr2, Region.Us,
        "55 8B EC 6A FF 68 ?? ?? ?? ?? 64 A1 ?? ?? ?? ?? 50 83 EC 08 56 57 A1 ?? ?? ?? ?? 33 C5 50 8D 45 ?? 64 A3 ?? ?? ?? ?? 8B 75 ?? F6 46 ?? 03 0F 84 ?? ?? ?? ?? 6A 13 51 E8 ?? ?? ?? ?? 8B F8 85 FF 74 ?? 8B 0D ?? ?? ?? ?? A1 ?? ?? ?? ?? 83 C1 13 3B C1 89 0D ?? ?? ?? ?? 0F 4C C1 83 C7 10 A3 ?? ?? ?? ?? 89 7D ?? C7 45 ?? 00 00 00 00 85 FF 74 ?? 8B 56 ?? 8B CA 6A 01 E8 ?? ?? ?? ?? 0F B6 C8 8B 42 ?? 6A 00 8A 04 ?? 8B CA 88 45 ?? E8 ?? ?? ?? ?? 0F B6 C8 8B 42 ?? 51 FF 75 ?? 8A 04 ?? 8B CF 88 45 ?? FF 75 ?? FF 76 ?? 52 FF 75 ?? E8 ?? ?? ?? ?? 8B F8 EB ?? 33 FF C7 45 ?? FF FF FF FF 8B 4E ?? 85 C9 74 ?? 8B 01 6A 01 FF 50 ?? 89 7E ?? EB ?? 8B 7D ?? 8B CF E8 ?? ?? ?? ?? 85 C0 74 ?? B8 01 00 00 00 8B 4D ?? 64 89 0D ?? ?? ?? ?? 59 5F 5E 8B E5 5D C3 8B 4E ?? 85 C9 74 ?? 8B 01 6A 01 FF 50 ?? 8B 4E ?? C7 46 ?? 00 00 00 00 8B 41 ?? 8B 40 ?? 0F B6 00 83 E8 00 74 ?? 83 E8 02 75 ?? 6A 01 6A 01 6A 03 E8 ?? ?? ?? ?? EB ?? 6A 01 6A 01 6A 03 E8 ?? ?? ?? ?? 6A 02 BA ?? ?? ?? ?? B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C4 04 C6 46 ?? 11")]
    [Function(CallingConventions.MicrosoftThiscall)]
    public delegate nint CModeComVsComCaller2(nint thisPtr, int param1, nint outptr, nint vtable);

    [HookDef(BaseGame.Mr2, Region.Us,
        "55 8B EC 6A FF 68 ?? ?? ?? ?? 64 A1 ?? ?? ?? ?? 50 83 EC 08 56 57 A1 ?? ?? ?? ?? 33 C5 50 8D 45 ?? 64 A3 ?? ?? ?? ?? 8B 75 ?? F6 46 ?? 03 0F 84 ?? ?? ?? ?? 6A 13 51 E8 ?? ?? ?? ?? 8B F8 85 FF 74 ?? 8B 0D ?? ?? ?? ?? A1 ?? ?? ?? ?? 83 C1 13 3B C1 89 0D ?? ?? ?? ?? 0F 4C C1 83 C7 10 A3 ?? ?? ?? ?? 89 7D ?? C7 45 ?? 00 00 00 00 85 FF 74 ?? 8B 56 ?? 8B CA 6A 01 E8 ?? ?? ?? ?? 0F B6 C8 8B 42 ?? 6A 00 8A 04 ?? 8B CA 88 45 ?? E8 ?? ?? ?? ?? 0F B6 C8 8B 42 ?? 51 FF 75 ?? 8A 04 ?? 8B CF 88 45 ?? FF 75 ?? FF 76 ?? 52 FF 75 ?? E8 ?? ?? ?? ?? 8B F8 EB ?? 33 FF C7 45 ?? FF FF FF FF 8B 4E ?? 85 C9 74 ?? 8B 01 6A 01 FF 50 ?? 89 7E ?? EB ?? 8B 7D ?? 8B CF E8 ?? ?? ?? ?? 85 C0 74 ?? B8 01 00 00 00 8B 4D ?? 64 89 0D ?? ?? ?? ?? 59 5F 5E 8B E5 5D C3 8B 4E ?? 85 C9 74 ?? 8B 01 6A 01 FF 50 ?? 8B 4E ?? C7 46 ?? 00 00 00 00 8B 41 ?? 8B 40 ?? 0F B6 00 83 E8 00 74 ?? 83 E8 02 75 ?? 6A 01 6A 01 6A 03 E8 ?? ?? ?? ?? EB ?? 6A 01 6A 01 6A 03 E8 ?? ?? ?? ?? 6A 02 BA ?? ?? ?? ?? B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C4 04 C6 46 ?? 12")]
    [Function(CallingConventions.MicrosoftThiscall)]
    public delegate nint CModeComVsComCaller3(nint thisPtr, int param1, nint outptr, nint vtable);

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

    private IHook<CModeComVsCom>? _hookComVsCom;
    // private IHook<CModeComVsComCaller2>? _hookComVsComCaller2;
    // private IHook<CModeComVsComCaller3>? _hookComVsComCaller3;

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
        // hooks?.AddHook<CModeComVsComConstructor>(ComVComHook)
        //     .ContinueWith(result => _hookComVComConstructor = result.Result.Activate());
        hooks?.AddHook<CModeComVsCom>(ComVsComHook)
            .ContinueWith(result => _hookComVsCom = result.Result.Activate());
        // hooks?.AddHook<CModeComVsComCaller2>(ComVsComHookCaller2)
        //     .ContinueWith(result => _hookComVsComCaller2 = result.Result.Activate());
        // hooks?.AddHook<CModeComVsComCaller3>(ComVsComHookCaller3)
        //     .ContinueWith(result => _hookComVsComCaller3 = result.Result.Activate());
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

    private int ComVsComHook(nint thisPtr)
    {
        var ret = _hookComVsCom!.OriginalFunction(thisPtr);
        if (ret == 0)
        {
            Logger.Error($"Testing caller 1: {thisPtr} ");
            Logger.Error($"ret: {ret} ");
        }

        return ret;
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