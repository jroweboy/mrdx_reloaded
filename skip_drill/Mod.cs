using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using SkipDrillAnim.Template;
using System.Diagnostics;
using System.Runtime.InteropServices;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace SkipDrillAnim
{
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
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private const int userInputOffset = 0x3723B0;

        private const int trainingIsDoneOffset = 0xC4EE0;

        private const int skipButton = 0x10; // 0x10 == Triangle button

        private static nint baseAddress;

        private IHook<IsTrainingDone> _hook;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _modConfig = context.ModConfig;


            // For more information about this template, please see
            // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

            // If you want to implement e.g. unload support in your mod,
            // and some other neat features, override the methods in ModBase.

            var thisProcess = Process.GetCurrentProcess();
            baseAddress = thisProcess.MainModule!.BaseAddress;

            _hook = _hooks!.CreateHook<IsTrainingDone>(ShouldSkipTraining, baseAddress + trainingIsDoneOffset).Activate();
        }

        private unsafe static bool ShouldSkipTraining([In] int self)
        {
            int* inputPtr = (int *) (baseAddress + userInputOffset);
            return (*inputPtr & skipButton) != 0;
        }

        [Function(CallingConventions.MicrosoftThiscall)]
        private delegate bool IsTrainingDone([In] int self);

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}