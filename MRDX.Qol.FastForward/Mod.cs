using MRDX.Base.Mod.Interfaces;
using MRDX.Qol.FastForward.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

namespace MRDX.Qol.FastForward;

/// <summary>
///     Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    private readonly WeakReference<IController>? _controller;
    private readonly WeakReference<IGameClient>? _gameClient;

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

    private bool _wasPressed;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _modConfig = context.ModConfig;
        _configuration = context.Configuration;

        _gameClient = _modLoader.GetController<IGameClient>();
        _controller = _modLoader.GetController<IController>();
        _controller.TryGetTarget(out var controller);
        controller!.OnInputChanged += HandleInputChanged;
    }

    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod()
    {
    }
#pragma warning restore CS8618

    #endregion

    private void HandleInputChanged(IInput input)
    {
        if (_gameClient == null || !_gameClient.TryGetTarget(out var game)) return;

        var isPressed = (input.Buttons & ButtonFlags.LTrigger) != 0;
        if (_configuration.UseToggle)
        {
            // If the user just pressed the toggle button, then change the fast forward state.
            if (isPressed && !_wasPressed)
                game.FastForwardOption = !game.FastForwardOption;
        }
        else
        {
            if (_wasPressed != isPressed)
                game.FastForwardOption = isPressed;
        }

        _wasPressed = isPressed;
    }

    public override void ConfigurationUpdated(Config configuration)
    {
        _configuration = configuration;
        if (_controller?.TryGetTarget(out var controller) ?? false)
            HandleInputChanged(controller.Current);
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
}