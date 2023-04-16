using MRDX.Base.Mod.Interfaces;

namespace MRDX.Base.Mod;

// Where the objects tend to be in the "RAM" for the steam release
[BaseOffset(BaseGame.Mr2, Region.Japan, 0x002CA504)]
[BaseOffset(BaseGame.Mr2, Region.Us, 0x002DEC6C)]
public class Monster : BaseObject<Monster>, IMonster
{
    public Monster(int offset) : base(offset)
    {
    }

    public MonsterCache Cache { get; private set; }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x00)]
    public ushort Age
    {
        get => Read<ushort>("Age");
        set => WatchWrite("Age", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x04)]
    public MonsterGenus GenusMain
    {
        get => Read<MonsterGenus>("GenusMain");
        set => WatchWrite("GenusMain", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x08)]
    public MonsterGenus GenusSub
    {
        get => Read<MonsterGenus>("GenusSub");
        set => WatchWrite("GenusSub", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x0C)]
    public ushort Life
    {
        get => Read<ushort>("Life");
        set => WatchWrite("Life", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x0E)]
    public ushort Power
    {
        get => Read<ushort>("Power");
        set => WatchWrite("Power", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x14)]
    public ushort Intelligence
    {
        get => Read<ushort>("Intelligence");
        set => WatchWrite("Intelligence", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x12)]
    public ushort Skill
    {
        get => Read<ushort>("Skill");
        set => WatchWrite("Skill", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x14)]
    public ushort Speed
    {
        get => Read<ushort>("Speed");
        set => WatchWrite("Speed", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x10)]
    public ushort Defense
    {
        get => Read<ushort>("Defense");
        set => WatchWrite("Defense", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x1C)]
    public ushort Lifespan
    {
        get => Read<ushort>("Lifespan");
        set => WatchWrite("Lifespan", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x1E)]
    public ushort InitalLifespan
    {
        get => Read<ushort>("InitalLifespan");
        set => WatchWrite("InitalLifespan", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x20)]

    public short NatureRaw
    {
        get => Read<short>("NatureRaw");
        set => WatchWrite("NatureRaw", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x45)]
    public sbyte NatureBase
    {
        get => Read<sbyte>("NatureBase");
        set => WatchWrite("NatureBase", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x23)]
    public byte Fatigue
    {
        get => Read<byte>("Fatigue");
        set => WatchWrite("Fatigue", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x24)]
    public byte Fame
    {
        get => Read<byte>("Fame");
        set => WatchWrite("Fame", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x27)]
    public sbyte Stress { get; set; }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x28)]
    public byte LoyalSpoil
    {
        get => Read<byte>("LoyalSpoil");
        set => WatchWrite("LoyalSpoil", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x29)]
    public byte LoyalFear
    {
        get => Read<byte>("LoyalFear");
        set => WatchWrite("LoyalFear", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x2A)]
    public sbyte FormRaw
    {
        get => Read<sbyte>("FormRaw");
        set => WatchWrite("FormRaw", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x2C)]
    public byte GrowthRatePower
    {
        get => Read<byte>("GrowthRatePower");
        set => WatchWrite("GrowthRatePower", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x2D)]
    public byte GrowthRateIntelligence
    {
        get => Read<byte>("GrowthRateIntelligence");
        set => WatchWrite("GrowthRateIntelligence", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x2E)]
    public byte GrowthRateLife
    {
        get => Read<byte>("GrowthRateLife");
        set => WatchWrite("GrowthRateLife", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x2F)]
    public byte GrowthRateSkill
    {
        get => Read<byte>("GrowthRateSkill");
        set => WatchWrite("GrowthRateSkill", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x30)]
    public byte GrowthRateSpeed
    {
        get => Read<byte>("GrowthRateSpeed");
        set => WatchWrite("GrowthRateSpeed", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x31)]
    public byte GrowthRateDefense
    {
        get => Read<byte>("GrowthRateDefense");
        set => WatchWrite("GrowthRateDefense", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x64)]
    public ushort TrainBoost
    {
        get => Read<ushort>("TrainBoost");
        set => WatchWrite("TrainBoost", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xB1)]
    public byte CupJellyCount
    {
        get => Read<byte>("CupJellyCount");
        set => WatchWrite("CupJellyCount", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xB2)]
    public bool UsedPeachGold
    {
        get => Read<bool>("UsedPeachGold");
        set => WatchWrite("UsedPeachGold", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xB3)]
    public bool UsedPeachSilver
    {
        get => Read<bool>("UsedPeachSilver");
        set => WatchWrite("UsedPeachSilver", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xE9)]
    public PlaytimeType Playtime
    {
        get => Read<PlaytimeType>("Playtime");
        set => WatchWrite("Playtime", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xF6)]
    public byte Drug
    {
        get => Read<byte>("Drug");
        set => WatchWrite("Drug", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xF8)]
    public byte DrugDuration
    {
        get => Read<byte>("DrugDuration");
        set => WatchWrite("DrugDuration", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xFA)]
    public bool ItemUsed
    {
        get => Read<bool>("ItemUsed");
        set => WatchWrite("ItemUsed", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x106)]
    public Item ItemLike
    {
        get => Read<Item>("ItemLike");
        set => WatchWrite("ItemLike", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x108)]
    public Item ItemDislike
    {
        get => Read<Item>("ItemDislike");
        set => WatchWrite("ItemDislike", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x18E)]
    public byte Rank
    {
        get => Read<byte>("Rank");
        set => WatchWrite("Rank", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x18F)]
    public LifeStage LifeStage
    {
        get => Read<LifeStage>("LifeStage");
        set => WatchWrite("LifeStage", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x190)]
    public LifeType LifeType
    {
        get => Read<LifeType>("LifeType");
        set => WatchWrite("LifeType", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x1D2)]
    public byte ArenaSpeed
    {
        get => Read<byte>("ArenaSpeed");
        set => WatchWrite("ArenaSpeed", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x1D3)]
    public byte GutsRate
    {
        get => Read<byte>("GutsRate");
        set => WatchWrite("GutsRate", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x1E)]

    public IList<IMonsterAttack> Moves { get; set; } = new List<IMonsterAttack>();

    public IList<byte> MoveUseCount { get; set; } = new List<byte>();

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xDC)]
    public byte MotivationDomino
    {
        get => Read<byte>("MotivationDomino");
        set => WatchWrite("MotivationDomino", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xDD)]
    public byte MotivationStudy
    {
        get => Read<byte>("MotivationStudy");
        set => WatchWrite("MotivationStudy", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xDE)]
    public byte MotivationRun
    {
        get => Read<byte>("MotivationRun");
        set => WatchWrite("MotivationRun", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xDF)]
    public byte MotivationShoot
    {
        get => Read<byte>("MotivationShoot");
        set => WatchWrite("MotivationShoot", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xE0)]
    public byte MotivationDodge
    {
        get => Read<byte>("MotivationDodge");
        set => WatchWrite("MotivationDodge", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xE1)]
    public byte MotivationEndure
    {
        get => Read<byte>("MotivationEndure");
        set => WatchWrite("MotivationEndure", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xE2)]
    public byte MotivationPull
    {
        get => Read<byte>("MotivationPull");
        set => WatchWrite("MotivationPull", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xE3)]
    public byte MotivationMeditate
    {
        get => Read<byte>("MotivationMeditate");
        set => WatchWrite("MotivationMeditate", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xE4)]
    public byte MotivationLeap
    {
        get => Read<byte>("MotivationLeap");
        set => WatchWrite("MotivationLeap", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0xE5)]
    public byte MotivationSwim
    {
        get => Read<byte>("MotivationSwim");
        set => WatchWrite("MotivationSwim", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x16C)]
    public string Name
    {
        get => ReadStr("Name", 12);
        set => WriteStr("Name", value);
    }

    [BaseOffset(BaseGame.Mr2, Region.Us | Region.Japan, 0x1E)]
    public uint PrizeMoney
    {
        get => Read<uint>("PrizeMoney");
        set => WatchWrite("PrizeMoney", value);
    }

    public void UpdateCache()
    {
    }

    /// <summary>
    ///     Event is fired whenever a Mod writes to this monster
    /// </summary>
    public event IGame.MonsterChange? ModChanged;

    private void WatchWrite<T>(string fieldName, T val) where T : unmanaged
    {
        // Always write just in case the cache is out of date or something
        Write(fieldName, val);
        if (Cache.Get<T>(fieldName).Equals(val)) return;
        var newCache = Cache with { };
        newCache.Set(fieldName, val);
        ModChanged?.Invoke(new ModChangedMonster
        {
            Offset = BaseAddress,
            Previous = Cache,
            Current = newCache,
            IsChangeFromMod = true
        });
        Cache = newCache;
    }
}

public class ModChangedMonster : IMonsterModChange
{
    public StatFlags FieldsChangedFlags { get; init; }
    public long Offset { get; init; }
    public IMonster Previous { get; init; } = new MonsterCache();
    public IMonster Current { get; init; } = new MonsterCache();
    public bool IsChangeFromMod { get; init; }
}

public record struct MonsterCache : IMonster
{
    public MonsterCache()
    {
        Name = "";
        Moves = new List<IMonsterAttack>();
        MoveUseCount = new List<byte>();
        Age = 0;
        GenusMain = MonsterGenus.Pixie;
        GenusSub = MonsterGenus.Pixie;
        Life = 0;
        Power = 0;
        Intelligence = 0;
        Skill = 0;
        Speed = 0;
        Defense = 0;
        Lifespan = 0;
        InitalLifespan = 0;
        NatureRaw = 0;
        NatureBase = 0;
        Fatigue = 0;
        Fame = 0;
        Stress = 0;
        LoyalSpoil = 0;
        LoyalFear = 0;
        FormRaw = 0;
        GrowthRateLife = 0;
        GrowthRatePower = 0;
        GrowthRateIntelligence = 0;
        GrowthRateSkill = 0;
        GrowthRateSpeed = 0;
        GrowthRateDefense = 0;
        TrainBoost = 0;
        CupJellyCount = 0;
        UsedPeachGold = false;
        UsedPeachSilver = false;
        Playtime = PlaytimeType.MudFight;
        Drug = 0;
        DrugDuration = 0;
        ItemUsed = false;
        ItemLike = Item.Potato;
        ItemDislike = Item.Potato;
        Rank = 0;
        LifeStage = LifeStage.Baby;
        LifeType = LifeType.Normal;
        ArenaSpeed = 0;
        GutsRate = 0;
        MotivationDomino = 0;
        MotivationStudy = 0;
        MotivationRun = 0;
        MotivationShoot = 0;
        MotivationDodge = 0;
        MotivationEndure = 0;
        MotivationPull = 0;
        MotivationMeditate = 0;
        MotivationLeap = 0;
        MotivationSwim = 0;
        PrizeMoney = 0;
    }

    public MonsterCache(IMonster mon)
    {
        Age = mon.Age;
        GenusMain = mon.GenusMain;
        GenusSub = mon.GenusSub;
        Life = mon.Life;
        Power = mon.Power;
        Intelligence = mon.Intelligence;
        Skill = mon.Skill;
        Speed = mon.Speed;
        Defense = mon.Defense;
        Lifespan = mon.Lifespan;
        InitalLifespan = mon.InitalLifespan;
        NatureRaw = mon.NatureRaw;
        NatureBase = mon.NatureBase;
        Fatigue = mon.Fatigue;
        Fame = mon.Fame;
        Stress = mon.Stress;
        LoyalSpoil = mon.LoyalSpoil;
        LoyalFear = mon.LoyalFear;
        FormRaw = mon.FormRaw;
        GrowthRateLife = mon.GrowthRateLife;
        GrowthRatePower = mon.GrowthRatePower;
        GrowthRateIntelligence = mon.GrowthRateIntelligence;
        GrowthRateSkill = mon.GrowthRateSkill;
        GrowthRateSpeed = mon.GrowthRateSpeed;
        GrowthRateDefense = mon.GrowthRateDefense;
        TrainBoost = mon.TrainBoost;
        CupJellyCount = mon.CupJellyCount;
        UsedPeachGold = mon.UsedPeachGold;
        UsedPeachSilver = mon.UsedPeachSilver;
        Playtime = mon.Playtime;
        Drug = mon.Drug;
        DrugDuration = mon.DrugDuration;
        ItemUsed = mon.ItemUsed;
        ItemLike = mon.ItemLike;
        ItemDislike = mon.ItemDislike;
        Rank = mon.Rank;
        LifeStage = mon.LifeStage;
        LifeType = mon.LifeType;
        ArenaSpeed = mon.ArenaSpeed;
        GutsRate = mon.GutsRate;
        Moves = mon.Moves;
        MoveUseCount = mon.MoveUseCount;
        MotivationDomino = mon.MotivationDomino;
        MotivationStudy = mon.MotivationStudy;
        MotivationRun = mon.MotivationRun;
        MotivationShoot = mon.MotivationShoot;
        MotivationDodge = mon.MotivationDodge;
        MotivationEndure = mon.MotivationEndure;
        MotivationPull = mon.MotivationPull;
        MotivationMeditate = mon.MotivationMeditate;
        MotivationLeap = mon.MotivationLeap;
        MotivationSwim = mon.MotivationSwim;
        Name = mon.Name;
        PrizeMoney = mon.PrizeMoney;
    }

    public ushort Age { get; set; }
    public MonsterGenus GenusMain { get; set; }
    public MonsterGenus GenusSub { get; set; }
    public ushort Life { get; set; }
    public ushort Power { get; set; }
    public ushort Intelligence { get; set; }
    public ushort Skill { get; set; }
    public ushort Speed { get; set; }
    public ushort Defense { get; set; }
    public ushort Lifespan { get; set; }
    public ushort InitalLifespan { get; set; }
    public short NatureRaw { get; set; }
    public sbyte NatureBase { get; set; }
    public byte Fatigue { get; set; }
    public byte Fame { get; set; }
    public sbyte Stress { get; set; }
    public byte LoyalSpoil { get; set; }
    public byte LoyalFear { get; set; }
    public sbyte FormRaw { get; set; }
    public byte GrowthRateLife { get; set; }
    public byte GrowthRatePower { get; set; }
    public byte GrowthRateIntelligence { get; set; }
    public byte GrowthRateSkill { get; set; }
    public byte GrowthRateSpeed { get; set; }
    public byte GrowthRateDefense { get; set; }
    public ushort TrainBoost { get; set; }
    public byte CupJellyCount { get; set; }
    public bool UsedPeachGold { get; set; }
    public bool UsedPeachSilver { get; set; }
    public PlaytimeType Playtime { get; set; }
    public byte Drug { get; set; }
    public byte DrugDuration { get; set; }
    public bool ItemUsed { get; set; }
    public Item ItemLike { get; set; }
    public Item ItemDislike { get; set; }
    public byte Rank { get; set; }
    public LifeStage LifeStage { get; set; }
    public LifeType LifeType { get; set; }
    public byte ArenaSpeed { get; set; }
    public byte GutsRate { get; set; }
    public IList<IMonsterAttack> Moves { get; set; } = new List<IMonsterAttack>();
    public IList<byte> MoveUseCount { get; set; } = new List<byte>();
    public byte MotivationDomino { get; set; }
    public byte MotivationStudy { get; set; }
    public byte MotivationRun { get; set; }
    public byte MotivationShoot { get; set; }
    public byte MotivationDodge { get; set; }
    public byte MotivationEndure { get; set; }
    public byte MotivationPull { get; set; }
    public byte MotivationMeditate { get; set; }
    public byte MotivationLeap { get; set; }
    public byte MotivationSwim { get; set; }
    public string Name { get; set; }
    public uint PrizeMoney { get; set; }

    public T Get<T>(string propName)
    {
        return (T)GetType().GetProperty(propName)!.GetValue(this, null)!;
    }

    public void Set<T>(string propName, T val)
    {
        GetType().GetProperty(propName)!.SetValue(this, val, null);
    }
}