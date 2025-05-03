using MRDX.Base.Mod.Interfaces;

namespace MRDX.Base.BattleSimulator;

public enum AiSide
{
    Left,
    Right
}

public enum AiStrategy
{
    Execute,
    Pushback,
    Wait,
    Forward,
    Backward
}

public record struct BattleResult
{
    public int Timer;
}

internal static class BattleData
{
    public static readonly Dictionary<MonsterGenus, int> MonsterBaseArenaLength = new()
    {
        { MonsterGenus.Ape, 450 },
        { MonsterGenus.Golem, 914 },
        { MonsterGenus.Naga, 720 },
        { MonsterGenus.Arrowhead, 943 },
        { MonsterGenus.Hare, 331 },
        { MonsterGenus.Niton, 200 },
        { MonsterGenus.Bajarl, 332 },
        { MonsterGenus.Henger, 364 },
        { MonsterGenus.Phoenix, 650 },
        { MonsterGenus.Baku, 1150 },
        { MonsterGenus.Hopper, 232 },
        { MonsterGenus.Pixie, 125 },
        { MonsterGenus.Beaclon, 1160 },
        { MonsterGenus.Jell, 460 },
        { MonsterGenus.Plant, 550 },
        { MonsterGenus.Centaur, 450 },
        { MonsterGenus.Jill, 565 },
        { MonsterGenus.Suezo, 270 },
        { MonsterGenus.ColorPandora, 1138 },
        { MonsterGenus.Joker, 783 },
        { MonsterGenus.Tiger, 640 },
        { MonsterGenus.Dragon, 800 },
        { MonsterGenus.Kato, 400 },
        { MonsterGenus.Undine, 165 },
        { MonsterGenus.Ducken, 220 },
        { MonsterGenus.Metalner, 280 },
        { MonsterGenus.Worm, 820 },
        { MonsterGenus.Durahan, 375 },
        { MonsterGenus.Mew, 250 },
        { MonsterGenus.Wracky, 254 },
        { MonsterGenus.Gaboo, 540 },
        { MonsterGenus.Mocchi, 250 },
        { MonsterGenus.Zilla, 460 },
        { MonsterGenus.Gali, 220 },
        { MonsterGenus.Mock, 873 },
        { MonsterGenus.Zuum, 820 },
        { MonsterGenus.Ghost, 220 },
        { MonsterGenus.Monol, 100 }
    };
}

internal class SimulatedMonster : BattleMonsterData, IBattleMonster
{
    private readonly int _arenaBackDirection;
    private readonly Dictionary<TechRange, List<IMonsterTechnique>> TechListCache = new();

    public SimulatedMonster(AiSide side, IBattleMonsterData data) : base(data)
    {
        TechData = MonsterBreed.GetBreed(GenusMain, GenusSub)?.TechList!;
        Hp = Life;
        Stress = 0;
        Fame = 0;
        ActiveBattleSpecial = 0;
        InactiveBattleSpecial = 0;
        Guts = 50;
        InFoolery = false;
        Loyalty = 50;
        ArenaLength = BattleData.MonsterBaseArenaLength[GenusMain];
        ArenaPosition = side == AiSide.Left ? -1950 : 1950;
        _arenaBackDirection = side == AiSide.Left ? -1 : 1;
        // For all possible techniques for this breed
        //   Add the technique to the cache if the monster actually knows it
        foreach (var t in TechData)
            switch (t.Slot)
            {
                case >= TechSlots.Long0 and <= TechSlots.Long5:
                    if (TechSlot.HasFlag(t.Slot)) TechListCache.GetOrAdd(TechRange.Long, _ => []);
                    break;
                case >= TechSlots.Medium0 and <= TechSlots.Medium5:
                    if (TechSlot.HasFlag(t.Slot)) TechListCache.GetOrAdd(TechRange.Medium, _ => []);
                    break;
                case >= TechSlots.Short0 and <= TechSlots.Short5:
                    if (TechSlot.HasFlag(t.Slot)) TechListCache.GetOrAdd(TechRange.Short, _ => []);
                    break;
                case >= TechSlots.Melee0 and <= TechSlots.Melee5:
                    if (TechSlot.HasFlag(t.Slot)) TechListCache.GetOrAdd(TechRange.Melee, _ => []);
                    break;
            }

        foreach (var (range, tech) in TechListCache)
            if (!SelectedTechnique.ContainsKey(range))
                SelectedTechnique[range] = tech.FirstOrDefault();
    }

    public int ArenaPosition { get; set; }
    public int ArenaLength { get; set; }

    public AiStrategy Strategy { get; set; }

    public int StratTimer { get; set; }

    public Dictionary<TechRange, IMonsterTechnique?> SelectedTechnique { get; } = new();
    public int StunTimer { get; set; }
    public bool InPushBack { get; set; }
    public ushort Hp { get; set; }
    public sbyte Stress { get; set; }
    public byte Fame { get; set; }
    public byte Loyalty { get; set; }
    public BattleSpecials ActiveBattleSpecial { get; set; }
    public BattleSpecials InactiveBattleSpecial { get; set; }
    public byte Guts { get; set; }
    public bool InFoolery { get; set; }
    public List<IMonsterTechnique> TechData { get; }

    private TechRange GetTechRange(int distanceBetweenMonsters)
    {
        return distanceBetweenMonsters switch
        {
            <= 400 => TechRange.Melee,
            <= 1000 => TechRange.Short,
            <= 1800 => TechRange.Medium,
            <= 2800 => TechRange.Long,
            _ => TechRange.OutOfRange
        };
    }

    private List<IMonsterTechnique> GetTechsByDistance(int distanceBetweenMonsters)
    {
        return TechListCache[GetTechRange(distanceBetweenMonsters)];
    }

    public List<AiStrategy> GetStrategies(Random rng, int distanceBetweenMonsters, int distanceToEdge,
        SimulatedMonster other)
    {
        List<AiStrategy> strategies = [];
        var availableTechs = GetTechsByDistance(distanceBetweenMonsters);
        var selectedTech = SelectedTechnique[GetTechRange(distanceBetweenMonsters)];

        // 1 - If MED < 100 AND MBD <= 50, then PUSH BACK is populated as the first option;
        // otherwise, if no technique exists at the current range, then FORWARD is populated as the first option;
        // otherwise, WAIT is chosen as the first populated option. 
        if (distanceToEdge < 100 && distanceBetweenMonsters <= 50)
            strategies.Add(AiStrategy.Pushback);
        else if (selectedTech != null)
            strategies.Add(AiStrategy.Forward);
        else
            strategies.Add(AiStrategy.Wait);

        // 2 - If MBD > 0, then FORWARD is chosen as the next populated option;
        // otherwise, nothing is chosen as the next option.
        // Note: there could be two FORWARD choices in the populated options depending on how the first option was populated.
        if (distanceBetweenMonsters > 0)
            strategies.Add(AiStrategy.Forward);

        // 3 - If MED > 80, then BACKWARD is chosen as the next populated option;
        // otherwise, nothing is chosen as the next option.
        if (distanceToEdge > 80)
            strategies.Add(AiStrategy.Backward);

        // 4 - IF MED < 1500 AND MBD <= 50, then PUSH BACK is selected as the next option;
        // otherwise, nothing is chosen as the next option.
        // Note: there could be two PUSH BACK choices in the populated options depending on how the first option was populated.
        if (distanceToEdge < 1500 && distanceBetweenMonsters <= 50)
            strategies.Add(AiStrategy.Pushback);

        // The final options that are populated will all be EXECUTE and are chosen based on the following criteria:
        // technique existence at current range, technique guts cost for technique at current range, monster life left, and monster guts.
        // The amount of EXECUTE options that are populated as choices will be explained below followed by an example for both scenarios.
        // The amount of EXECUTE options added can be zero, and will never exceed an amount that would cause all
        // possible options to exceed a count of 16.
        // For example, if based on the previous 4 populated option choices, only 3 were chosen, then the maximum amount
        // of EXECUTE possible options that could be added would be 16 - 3 = 13. 

        // If there is no technique at the current range or there are not enough guts to execute the current technique,
        // then zero EXECUTE options will be populated as possible options.

        // 5 - EXECUTE populate method #1:
        // This method will be used if the current technique that is at the monster range is a life recovery technique
        // that cannot do any guts damage or life damage to the opposing monster.
        var executeCount = 0;
        if (selectedTech is { LifeRecovery: > 0 })
        {
            executeCount = (int)Math.Floor(10 + 2 * Math.Floor(10.0 * other.Hp / other.Life) - 3 * (10.0 * Hp / Life));
        }
        else if (selectedTech is { Force: > 0 } or { Withering: > 0 } or { GutsSteal: > 0 })
        {
            // DX CHANGES:
            // In DX it will always add at least 1 EXECUTE command (if you have an attacking move in that slot)
            // AND there's a 30% chance that it just adds an extra EXECUTE command.
            // From there, the higher the guts you have, the more EXECUTE commands you'll get added.
            // These two changes to DX means that there's always a small chance that you'll try to attack,
            // and there's also a random small chance boost added for good measure.
            // And also the thresholds are now 30 guts (15 if opp in foolery) to start ramping up the number of attacks added to the list.
            var gutsFactor = other.InFoolery ? 15 : 30;
            gutsFactor = Guts - gutsFactor;

            if (rng.Next(0, 100) > 69) executeCount++;
            while (executeCount + strategies.Count < 16)
            {
                executeCount++;
                if (gutsFactor < 0) break;
                gutsFactor -= 5;
            }
        }

        // Add all the executes up to the limit of 16
        strategies.AddRange(Enumerable.Repeat(AiStrategy.Execute, Math.Min(executeCount, 16 - strategies.Count)));

        return strategies;
    }

    public void RunStrategyTick(int distanceBetweenMonsters)
    {
        if (StunTimer > 0)
        {
            StunTimer--;
            if (InPushBack)
            {
                // Move away from enemy at a rate of 25 movespeed every frame until you reach the wall or the timer expires
                ArenaPosition += _arenaBackDirection * 25;
                ArenaPosition = Math.Clamp(-3000, ArenaPosition, 3000);
                if (StunTimer <= 0 || Math.Abs(ArenaPosition) == 3000) InPushBack = false;
            }

            // Getting attacked so don't move
            return;
        }

        switch (Strategy)
        {
            case AiStrategy.Execute:
            case AiStrategy.Pushback:
            case AiStrategy.Wait:
                // Do nothing during these timers except decrement the current timer
                break;
            case AiStrategy.Forward:
            {
                var dist = -1 * _arenaBackDirection * ArenaSpeed;
                ArenaPosition += dist;
                ArenaPosition = Math.Clamp(-3000, ArenaPosition, 3000);
                // If we collided with the other monster, cancel the forward movement
                if (distanceBetweenMonsters < Math.Abs(dist)) StratTimer = 0;
                break;
            }
            case AiStrategy.Backward:
            {
                var dist = _arenaBackDirection * ArenaSpeed;
                ArenaPosition += dist;
                ArenaPosition = Math.Clamp(-3000, ArenaPosition, 3000);
                // If we collided with the wall, cancel the backwards movement
                if (Math.Abs(ArenaPosition) == 3000) StratTimer = 0;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        StratTimer--;
    }
}

public class BattleSimulation(IBattleMonsterData leftData, IBattleMonsterData rightData)
{
    private const int DEAD_ZONE = 500;

    private readonly SimulatedMonster _left = new(AiSide.Left, leftData);
    private readonly SimulatedMonster _right = new(AiSide.Right, rightData);

    // These values are updated every step
    private int _distanceBetweenMonsters;
    private int _leftDistanceFromEdge;
    private int _rightDistanceFromEdge;
    private int _timer;

    public async Task<BattleResult> Battle(Random rng, int numberOfSecondsInMatch = 60)
    {
        _timer = 30 * numberOfSecondsInMatch; // 30 Ticks per second and 60 seconds in a match.
        while (_timer > 0)
        {
            await Step(rng);
            _timer--;
        }

        return new BattleResult
        {
            Timer = _timer
        };
    }

    private async Task Step(Random rng)
    {
        _distanceBetweenMonsters = _right.ArenaPosition - _left.ArenaPosition - _right.ArenaLength - _left.ArenaLength -
                                   DEAD_ZONE;
        _leftDistanceFromEdge = 3000 + _left.ArenaPosition;
        _rightDistanceFromEdge = 3000 - _right.ArenaPosition;
        // If the timer for either monster has expired, choose the next strategy
        // Choose a new strategy for the monsters
        if (_left is { Strategy: AiStrategy.Execute, StratTimer: > 0 })
        {
            // Left is attacking so keep the right side paused
            _left.StratTimer--;
        }
        else if (_right is { Strategy: AiStrategy.Execute, StratTimer: > 0 })
        {
            _right.StratTimer--;
        }
        else
        {
            _left.StratTimer--;
            _right.StratTimer--;

            if (_left.StratTimer <= 0)
            {
            }
        }
    }

    private void ChangeStrategy(Random rng, AiSide side)
    {
        var (self, other) = side == AiSide.Left ? (_left, _right) : (_right, _left);
        var distanceFromEdge = side == AiSide.Left ? _leftDistanceFromEdge : _rightDistanceFromEdge;
        var leftStrats = self.GetStrategies(rng, _distanceBetweenMonsters, distanceFromEdge, other);
        var strat = rng.GetItems(leftStrats.ToArray(), 1)[0];
        self.Strategy = strat;
        switch (strat)
        {
            case AiStrategy.Forward:
            {
                // FORWARD – The monster will begin moving towards the opposing monster.
                // A distance amount will be initialized based on MBD, then decremented by the monster’s Arena Movement
                // Rate every Monster AI execution until the amount is less than or equal to 0. At that time the monster will stop moving.
                // % is the modulo/remainder function
                // LRN = Large Random Number (0-32767, next value 
                // Initial Move Value = (LRN % (MBD / 64)) x 64 [round down] 
                var timer = rng.Next(0, _distanceBetweenMonsters / 64) * 64;
                self.StratTimer = timer;
                break;
            }
            case AiStrategy.Wait:
                // WAIT – The monster will select between a 0, 1, or 2 second time delay, and then wait that amount of time.
                // The number is chosen using the small random number generator.
                self.StratTimer = rng.Next(0, 3) * 30;
                break;
            case AiStrategy.Backward:
            {
                // BACKWARD – The monster will begin moving away from the opposing monster.
                // A distance amount will be initialized based on MED, then decremented by the monster’s Arena Movement
                // Rate every Monster AI execution until the amount is less than or equal to 0. At that time the monster will stop moving.
                // % is the modulo/remainder function
                // LRN = Large Random Number (0-32767, next value 
                // Initial Move Value = (LRN % (MED / 64)) x 64 [round down] 
                var timer = rng.Next(0, distanceFromEdge / 64) * 64;
                self.StratTimer = timer;
                break;
            }
            case AiStrategy.Execute:
            case AiStrategy.Pushback:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}