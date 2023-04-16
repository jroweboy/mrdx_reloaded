using System.Diagnostics;
using System.Reflection;
using System.Text;
using MRDX.Base.Mod.Interfaces;
using Reloaded.Memory.Sources;

namespace MRDX.Base.Mod;

public static class Base
{
    public static readonly long ExeBaseAddress;
    public static readonly BaseGame Game;
    public static readonly Region Region;

    static Base()
    {
        var thisProcess = Process.GetCurrentProcess();
        var module = thisProcess.MainModule!;
        ExeBaseAddress = module.BaseAddress.ToInt64();
        Game = string.Equals(module.FileName, "mf.exe") ? BaseGame.Mr1 : BaseGame.Mr2;
        Region = module.FileVersionInfo.FileDescription == "MonsterRancher 1&2 DX" ? Region.Us : Region.Japan;
    }
}

public class BaseObject<TParent> where TParent : class
{
    private readonly Memory _memory;

    private readonly IDictionary<string, IList<int>> _offsetMapping = new Dictionary<string, IList<int>>();
    public readonly long BaseAddress;

    protected BaseObject(int baseOffset = 0)
    {
        _memory = Memory.Instance;

        // If no base is provided, check the class for the offset attribute for static memory locations
        if (baseOffset == 0)
            foreach (var offset in typeof(TParent).GetCustomAttributes().OfType<BaseOffsetAttribute>())
            {
                if (offset.Game != Base.Game || offset.Region != Base.Region) continue;
                baseOffset = offset.Offset;
                break;
            }

        BaseAddress = Base.ExeBaseAddress + baseOffset;

        // Build a mapping of game / region and fieldname => offset
        // For each property in the class, check for any offsets for our current Game/Region
        foreach (var prop in typeof(TParent).GetProperties())
            // If we find an offset for this region, then we can store the mapping in dictionary
        foreach (var off in prop.GetCustomAttributes().OfType<BaseOffsetAttribute>())
        {
            if ((off.Game & Base.Game) == 0 || (off.Region & Base.Region) == 0)
                continue;
            // Store the offsets for this region in the 
            if (!_offsetMapping.TryGetValue(prop.Name, out var list))
            {
                list = new List<int>();
                _offsetMapping[prop.Name] = list;
            }

            list.Add(off.Offset);
        }
    }

    protected T Read<T>(string fieldName) where T : unmanaged
    {
        if (!_offsetMapping.TryGetValue(fieldName, out var list))
            throw new NotImplementedException(
                $"Read: Missing offset for field {fieldName} with game {Base.Game} and region {Base.Region}");
        return ReadOffset<T>(list.FirstOrDefault());
    }

    protected string ReadStr(string fieldName, int length)
    {
        if (!_offsetMapping.TryGetValue(fieldName, out var list))
            throw new NotImplementedException(
                $"ReadStr: Missing offset for field {fieldName} with game {Base.Game} and region {Base.Region}");
        return ReadStrOffset(list.FirstOrDefault(), length);
    }

    protected void Write<T>(string fieldName, T value) where T : unmanaged
    {
        if (!_offsetMapping.TryGetValue(fieldName, out var list))
            throw new NotImplementedException(
                $"Write: Missing offset for field {fieldName} with game {Base.Game} and region {Base.Region}");
        WriteOffset(value, list.FirstOrDefault());
    }

    protected void SafeWrite<T>(string fieldName, T value) where T : unmanaged
    {
        if (!_offsetMapping.TryGetValue(fieldName, out var list))
            throw new NotImplementedException(
                $"Write: Missing offset for field {fieldName} with game {Base.Game} and region {Base.Region}");
        SafeWriteOffset(value, list.FirstOrDefault());
    }

    protected void WriteStr(string fieldName, string value)
    {
        if (!_offsetMapping.TryGetValue(fieldName, out var list))
            throw new NotImplementedException(
                $"Write: Missing offset for field {fieldName} with game {Base.Game} and region {Base.Region}");
        WriteStrOffset(value, list.FirstOrDefault());
    }

    protected void WriteAll<T>(string fieldName, T value) where T : unmanaged
    {
        if (!_offsetMapping.TryGetValue(fieldName, out var list))
            throw new NotImplementedException(
                $"WriteAll: Missing offset for field {fieldName} with game {Base.Game} and region {Base.Region}");

        foreach (var offset in list) WriteOffset(value, offset);
    }

    protected void SafeWriteAll<T>(string fieldName, T value) where T : unmanaged
    {
        if (!_offsetMapping.TryGetValue(fieldName, out var list))
            throw new NotImplementedException(
                $"WriteAll: Missing offset for field {fieldName} with game {Base.Game} and region {Base.Region}");

        foreach (var offset in list) SafeWriteOffset(value, offset);
    }

    protected T ReadOffset<T>(int offset) where T : unmanaged
    {
        _memory.Read<T>((nuint)(BaseAddress + offset), out var value);
        return value;
    }

    protected void WriteOffset<T>(T val, int offset) where T : unmanaged
    {
        _memory.Write((nuint)(BaseAddress + offset), ref val);
    }

    protected void SafeWriteOffset<T>(T val, int offset) where T : unmanaged
    {
        _memory.SafeWrite((nuint)(BaseAddress + offset), ref val);
    }

    protected string ReadStrOffset(int offset, int length)
    {
        _memory.ReadRaw((nuint)(BaseAddress + offset), out var rawBytes, length * 2);
        // Strings are a variable length byte/ushort combination. If the first byte is 0xb0 <= x <= 0xb6 then
        // its a multi byte string and we read an extra number.
        // 0xff is a terminator byte for the string.

        var sb = new StringBuilder();
        for (var i = 0; i < length * 2; i++)
        {
            var b = rawBytes[i];
            if (b == 0xff) break;
            var o = b is >= 0xb0 and <= 0xb6 ? (ushort)((b << 8) | rawBytes[i++]) : b;
            sb.Append(CharMap.Forward.TryGetValue(o, out var s) ? s : '?');
        }

        return sb.ToString();
    }

    protected void WriteStrOffset(string val, int offset)
    {
        // TODO implement this
        var arr = new ushort[val.Length];
        var q = CharMap.Reverse['?'];
        for (var i = 0; i < val.Length; i++) arr[i] = CharMap.Reverse.TryGetValue(val[i], out var s) ? s : q;
        _memory.Write((nuint)(BaseAddress + offset), arr, true);
    }


    public static int Get(string propName)
    {
        return GetOffset<TParent>(propName);
    }

    public static int GetOffset<T>(string propName)
    {
        return typeof(T).GetProperty(propName)!.GetCustomAttributes<BaseOffsetAttribute>()
            .First(attr => (attr.Game & Base.Game) != 0 && (attr.Region & Base.Region) != 0).Offset;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
internal class BaseOffsetAttribute : Attribute
{
    public BaseOffsetAttribute(BaseGame game, Region region, int offset)
    {
        Game = game;
        Region = region;
        Offset = offset;
    }

    public BaseGame Game { get; }
    public Region Region { get; }
    public int Offset { get; }
}