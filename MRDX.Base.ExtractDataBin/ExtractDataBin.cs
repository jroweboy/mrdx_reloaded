using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using MRDX.Base.ExtractDataBin.Interface;
using MRDX.Base.ExtractDataBin.Template;
using Reloaded.Mod.Interfaces;
using Reloaded.Universal.Redirector.Interfaces;
using SevenZip;

namespace MRDX.Base.ExtractDataBin;

public class ExtractDataBin : IExtractDataBin
{
    private static readonly object LockMr1 = new();
    private static readonly object LockMr2 = new();
    private readonly string? _exepath;

    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;

    private readonly string _mr1RelExtractPath = Path.Join("data");

    private readonly string _mr1RelZipPath = Path.Join("data", "data.bin");

    // private readonly string _mr1RelZipPathRenamed = Path.Join("data", "data.bin.backup");
    private readonly string _mr1TestPath = Path.Join("MF", "DATA");
    private readonly string _mr2RelExtractPath = Path.Join("Resources", "data");

    private readonly string _mr2RelZipPath = Path.Join("Resources", "data", "data.bin");

    // private readonly string _mr2RelZipPathRenamed = Path.Join("Resources", "data", "data.bin.backup");
    private readonly string _mr2TestPath = Path.Join("mf2", "data", "mon");
    private readonly WeakReference<IRedirectorController> _redirector;
    private string? _extractedPath;

    public ExtractDataBin(ModContext context)
    {
        _modConfig = context.ModConfig;
        _logger = context.Logger;
        _redirector = context.ModLoader.GetController<IRedirectorController>();
        var mainModule = Process.GetCurrentProcess().MainModule;
        if (mainModule == null)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] ERROR: Cannot get main module process", Color.Red);
            return;
        }

        _exepath = Path.GetDirectoryName(mainModule.FileName);
        _extractedPath = Path.Combine(_exepath ?? "/",
            context.ModLoader.GetAppConfig().AppId == "mf2.exe"
                ? _mr2RelExtractPath
                : _mr1RelExtractPath);
    }

    string? IExtractDataBin.ExtractedPath => _extractedPath;

    public string? ExtractMr1()
    {
        lock (LockMr1)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Lock acquired. Extracting MR1 data.bin");
            var tokenPath = Path.Combine(_extractedPath!, "extraction_complete.txt");
            if (!File.Exists(tokenPath))
                Extract(_mr1RelExtractPath, _mr1RelZipPath);
            // if (_extractedPath != null)
            // {
            //     _logger.WriteLine(
            //         $"[{_modConfig.ModId}] Renaming data.bin to data.bin.backup to prevent the game from trying to read from it.");
            //     File.Move(_mr1RelZipPath, _mr1RelZipPathRenamed);
            // }
            using var token = File.CreateText(tokenPath);

            return _extractedPath;
        }
    }

    public string? ExtractMr2()
    {
        lock (LockMr2)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Lock acquired. Extracting MR2 data.bin");
            var tokenPath = Path.Combine(_extractedPath!, "extraction_complete.txt");
            if (!File.Exists(tokenPath))
                Extract(_mr2RelExtractPath, _mr2RelZipPath);

            // We shouldn't need to rename the data.bin file anymore
            // if (_extractedPath != null)
            // {
            //     _logger.WriteLine(
            //         $"[{_modConfig.ModId}] Renaming data.bin to data.bin.backup to prevent the game from trying to read from it.");
            //     File.Move(_mr2RelZipPath, _mr2RelZipPathRenamed);
            // }
            using var token = File.CreateText(tokenPath);

            return _extractedPath;
        }
    }

    public event OnExtractComplete? ExtractComplete;

    private void Extract(string relExtractPath, string relZipPath)
    {
        var extPath = Path.Join(_exepath, relExtractPath);

        _redirector.TryGetTarget(out var redirector);
        if (redirector != null)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Disabling file redirector until extraction is complete.");
            redirector.Disable();
        }

        try
        {
            var zipPath = Path.Join(_exepath, relZipPath);
            _logger.WriteLine($"[{_modConfig.ModId}] Starting to extract from {zipPath} to {extPath}");
            // this is absolutely stupid. The library path loader is broken on old versions of .netcore which this library runs on
            // and we can't just "set" it since its an internal class, so forcably set it with reflection.
            var assembly = typeof(SevenZipExtractor).Assembly;
            var libraryLoader = assembly.GetType("SevenZip.SevenZipLibraryManager");
            if (libraryLoader == null)
            {
                _logger.WriteLine(
                    $"[{_modConfig.ModId}] ERROR: Failed to get library loader from {assembly.FullName}",
                    Color.Red);
                return;
            }

            // Reloaded II ships with the 7z dlls so just use those
            var dirname = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (dirname == null)
            {
                _logger.WriteLine(
                    $"[{_modConfig.ModId}] ERROR: Failed to get directory name from {assembly.FullName}",
                    Color.Red);
                return;
            }

            var libfile = Path.Combine(dirname,
                Environment.Is64BitProcess ? @"..\..\7z64.dll" : @"..\..\7z.dll");
            _logger.WriteLine($"[{_modConfig.ModId}] Extracting using the {libfile}");
            var field = libraryLoader.GetField("_libraryFileName", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                _logger.WriteLine(
                    $"[{_modConfig.ModId}] ERROR: Failed to get directory name from {assembly.FullName}",
                    Color.Red);
                return;
            }

            field.SetValue(null, libfile);
            var zip = new SevenZipExtractor(zipPath, "KoeiTecmoMF1&2");
            zip.Extracting += (sender, args) =>
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Extraction progress: {args.PercentDone}% complete",
                    Color.Aqua);
            };
            zip.ExtractionFinished += (sender, args) =>
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Extraction to {extPath} complete");
                _extractedPath = extPath;
                if (redirector != null)
                {
                    _logger.WriteLine($"[{_modConfig.ModId}] Enabling file redirector now.");
                    redirector.Enable();
                }

                ExtractComplete?.Invoke(_extractedPath);
            };
            zip.ExtractArchiveAsync(extPath).Wait();
        }
        catch (Exception e)
        {
            _logger.WriteLine(
                $"[{_modConfig.ModId}] Exception when extracting to {extPath}: {e} inner: {e.InnerException}");
        }
    }
}