﻿using System.Drawing;
using MRDX.Base.Mod.Interfaces;
using MRDX.Qol.SkipDrillAnim.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;

namespace MRDX.Qol.SkipDrillAnim;

/// <summary>
///     Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    ///     Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    private readonly WeakReference<IController> _input;

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

    private IHook<IsTrainingDone>? _drillhook;

    private CheckIfMonsterGrabbedAnItem? _itemfunc;
    private IHook<InitializeRuins2dData>? _itemhook;


    private Config.SkipAnimationSetting _skipDrillAnimation;
    private bool _skipItemFindAnimation;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _modConfig = context.ModConfig;
        _skipDrillAnimation = context.Configuration.SkipDrill;
        _skipItemFindAnimation = context.Configuration.SkipItemFind;

        _input = _modLoader.GetController<IController>();
        _modLoader.GetController<IHooks>().TryGetTarget(out var hooks);
        if (hooks == null)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Could not get hook controller.", Color.Red);
            return;
        }

        hooks.AddHook<IsTrainingDone>(ShouldSkipTraining)
            .ContinueWith(result => _drillhook = result.Result);

        hooks.AddHook<InitializeRuins2dData>(ShouldSkipItemFind)
            .ContinueWith(result => { _itemhook = result.Result; });

        hooks.CreateWrapper<CheckIfMonsterGrabbedAnItem>().ContinueWith(result => _itemfunc = result.Result);
    }


    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod()
    {
    }
#pragma warning restore CS8618

    #endregion

    public override void ConfigurationUpdated(Config configuration)
    {
        _skipDrillAnimation = configuration.SkipDrill;
        _skipItemFindAnimation = configuration.SkipItemFind;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }

    private bool ShouldSkipTraining(nint self)
    {
        _input.TryGetTarget(out var controller);
        var manualInput = _skipDrillAnimation == Config.SkipAnimationSetting.Manual &&
                          (controller?.Current.Buttons & (ButtonFlags.Circle | ButtonFlags.Triangle)) != 0;
        if (_skipDrillAnimation == Config.SkipAnimationSetting.Auto || manualInput)
            // The number at offset 8 is checked by the original function to see if we are done, and so we should update it before
            // returning from this function if we ended. In my testing the game usually sets it to 2 after the training animation ends.
            Memory.Instance.Write(nuint.Add((nuint)self, 8), 2);

        return _drillhook!.OriginalFunction.Invoke(self);
    }

    private void ShouldSkipItemFind(nint self)
    {
        // Call the original init function first
        _itemhook!.OriginalFunction.Invoke(self);

        // If we aren't trying to skip the animation, then early return
        if (!_skipItemFindAnimation) return;

        // Now call the function that actually "finds" the item or not based on the INT stat
        _itemfunc!.Invoke(self);

        // The number at offset 4 seems to relate to the monster's current animation state.
        // If we bump this to 0x0b, then the function will increment it to 0x0c and then it seems to complete early
        Memory.Instance.Write(nuint.Add((nuint)self, 4), 0x0b);
    }
}