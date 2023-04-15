using MRDX.Base.Mod.Interfaces;
using MRDX.Base.Mod.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace MRDX.Base.Mod;

public class Hooks : IHooks
{
    private readonly IReloadedHooks _hooks;
    private readonly ILogger _logger;
    private readonly WeakReference<IStartupScanner>? _startupScanner;

    public Hooks(ModContext context)
    {
        _hooks = context.Hooks!;
        _logger = context.Logger;
        _startupScanner = context.ModLoader.GetController<IStartupScanner>();
    }

    public Task<IHook<T>> AddHook<T>(T hookFunc)
    {
        var promise = new TaskCompletionSource<IHook<T>>();
        if (_startupScanner != null && _startupScanner.TryGetTarget(out var scanner))
        {
            var signature = Utils.GetSignature<T>(Base.Game, Base.Region);
            scanner.AddMainModuleScan(signature, result =>
            {
                _logger.WriteLine($"[Hooks] Created hook for {hookFunc} @ {Base.ExeBaseAddress + result.Offset}");
                promise.SetResult(_hooks.CreateHook(hookFunc, Base.ExeBaseAddress + result.Offset));
            });
        }
        else
        {
            _logger.WriteLine("[Hooks] Error: Failed to get scanner!");
            promise.SetCanceled();
        }

        return promise.Task;
    }

    public Task<T> CreateWrapper<T>() where T : Delegate
    {
        var promise = new TaskCompletionSource<T>();
        if (_startupScanner != null && _startupScanner.TryGetTarget(out var scanner))
        {
            var signature = Utils.GetSignature<T>(Base.Game, Base.Region);
            scanner.AddMainModuleScan(signature, result =>
            {
                _logger.WriteLine($"[Hooks] Created wrapper for {typeof(T)} @ {Base.ExeBaseAddress + result.Offset}");
                promise.SetResult(_hooks!.CreateWrapper<T>(Base.ExeBaseAddress + result.Offset, out var _));
            });
        }
        else
        {
            _logger.WriteLine("[Hooks] Error: Failed to get scanner!");
            promise.SetCanceled();
        }

        return promise.Task;
    }
}