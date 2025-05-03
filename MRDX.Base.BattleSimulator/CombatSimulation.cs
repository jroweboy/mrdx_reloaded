using MRDX.Base.Mod.Interfaces;

namespace MRDX.Base.BattleSimulator;

public enum Attacker
{
    Left,
    Right
}

public enum RecoilType
{
    MissSelf,
    HitSelf
}

public record struct CombatResult
{
    public int CritChance;
    public Range<int> Damage;
    public int HitChance;
    public Range<int> Withering;
}

public class CombatSimulation(IBattleMonster left, IBattleMonster right)
{
    private const double LevelDifferential = 12.5;
    private const double CorrectionFactor = 4.0;

    public Range<int> Damage(Attacker user, IMonsterTechnique tech)
    {
        var (attacker, defender) = user == Attacker.Left ? (left, right) : (right, left);

        int apow = tech.Scaling == TechType.Power ? attacker.Power : attacker.Intelligence;
        int dpow = tech.Scaling == TechType.Power ? defender.Power : defender.Intelligence;
        int ddef = defender.AdjustedDefense;

        if (apow == 999) apow = 1000;
        if (dpow == 999) dpow = 1000;
        if (ddef == 999) ddef = 1000;

        apow = (int)Math.Floor(Math.Round(apow / LevelDifferential * 10000) / 10000);
        ddef = (int)Math.Floor(
            Math.Round(Math.Floor(Math.Round((2d * ddef + dpow) / 3 * 10000) / 10000) / LevelDifferential * 10000) /
            10000);

        var baseDmg =
            Math.Floor(
                Math.Round(Math.Floor(Math.Round((2 * CorrectionFactor + apow) * tech.Force / 3 * 10000) / 10000) /
                    CorrectionFactor * 10000) /
                10000);
        var adj = Math.Floor(
            Math.Round((100 * CorrectionFactor + Math.Floor(Math.Round(10d * (apow - ddef) * 100000) / 100000)) /
                CorrectionFactor * 10000) / 10000);

        var dmg = Math.Floor(Math.Round(baseDmg * adj * 100) / 10000);

        var min = Math.Floor(
            Math.Round(Math.Floor(Math.Round((1 * CorrectionFactor + apow) * tech.Force / 20 * 10000) / 10000) /
                CorrectionFactor * 10000) / 10000);
        var max = Math.Floor(
            Math.Round(Math.Floor(Math.Round(3 * (1 * CorrectionFactor + apow) * tech.Force / 4 * 10000) / 10000) /
                CorrectionFactor * 10000) /
            10000);

        dmg = Math.Clamp(dmg, min, max);

        int mul;
        if (attacker.Guts <= 50)
            mul = (int)Math.Floor(Math.Round(Math.Floor((attacker.Guts + 200) * 65536.0 / 10000) / 16 * 10000) / 10000);
        else
            mul = (int)Math.Floor(Math.Round(Math.Floor((attacker.Guts + 200) * 65536.0 / 10000) / 16 * 10000) / 10000)
                  + (int)Math.Floor(Math.Round((attacker.GutsRate - 6) * (attacker.Guts - 50) / 5.0 * 10000) / 10000);

        int div;
        if (defender.Guts <= 50)
            div = (int)Math.Floor(Math.Round(Math.Floor((defender.Guts + 200) * 65536.0 / 10000) / 16 * 10000) / 10000);
        else
            div = (int)Math.Floor(Math.Round(Math.Floor((defender.Guts + 200) * 65536.0 / 10000) / 16 * 10000) / 10000)
                  + (int)Math.Floor(Math.Round((defender.GutsRate - 6) * (defender.Guts - 50) / 5.0 * 10000) / 10000);

        dmg = (int)Math.Floor(
            Math.Round(10240000 * Math.Floor(Math.Round(dmg * mul / 1024.0 * 1000000) / 1000000) / div) / 10000);

        // Battle Special Effects - Attacker
        if (attacker.ActiveBattleSpecial.HasFlag(BattleSpecials.Power) ||
            attacker.ActiveBattleSpecial.HasFlag(BattleSpecials.Fury))
            dmg *= 2;
        if (attacker.ActiveBattleSpecial.HasFlag(BattleSpecials.Guard) ||
            attacker.InactiveBattleSpecial.HasFlag(BattleSpecials.Real))
            dmg = (int)Math.Ceiling(dmg / 2.0);
        if (attacker.ActiveBattleSpecial.HasFlag(BattleSpecials.Real)) dmg = (int)Math.Floor(dmg * 1.5);

        // Battle Special Effects - Defender
        if (defender.ActiveBattleSpecial.HasFlag(BattleSpecials.Guard) ||
            defender.ActiveBattleSpecial.HasFlag(BattleSpecials.Real))
            dmg = (int)(dmg * 0.65625);

        if (defender.ActiveBattleSpecial.HasFlag(BattleSpecials.Ease) ||
            defender.InactiveBattleSpecial.HasFlag(BattleSpecials.Real))
            dmg *= 2;

        var lo = (int)Math.Clamp(dmg - 5, 1, 999);
        var hi = (int)Math.Clamp(dmg + 5, 1, 999);

        return new Range<int>(lo, hi);
    }

    public int CritChance(Attacker user, IMonsterTechnique tech)
    {
        var (attacker, defender) = user == Attacker.Left ? (left, right) : (right, left);

        int c = tech.Sharpness;
        if (attacker.Nature < 0) // Bad natured monsters
        {
            c += attacker.Stress / 10;
            c += attacker.Fame / 20;
            c += defender.Fame / 20;
        }
        else // Good natured monsters
        {
            c += attacker.Fame / 10;
        }

        if (attacker.ActiveBattleSpecial.HasFlag(BattleSpecials.Hurry))
            c *= 2;
        if (attacker.ActiveBattleSpecial.HasFlag(BattleSpecials.Real))
            c = (int)(Math.Round(c * 15000.0) / 10000);
        if (attacker.InactiveBattleSpecial.HasFlag(BattleSpecials.Real))
            c = (int)(Math.Round(c * 5000.0) / 10000);

        if (c > 100)
            c = 100;

        return c;
    }

    public int CalcHitChance(Attacker user, IMonsterTechnique tech)
    {
        var (attacker, defender) = user == Attacker.Left ? (left, right) : (right, left);
        // Calculate base hit contribution from TEC and guts
        var atkGutsComponent = Math.Round(attacker.Guts / (14.0 - Math.Floor(attacker.GutsRate / 2.0)) * 10000) / 10000;
        var defGutsComponent = Math.Round(defender.Guts / (14.0 - Math.Floor(defender.GutsRate / 2.0)) * 10000) / 10000;

        var hitChance = tech.HitPercent + Math.Floor(atkGutsComponent) - Math.Floor(defGutsComponent);

        hitChance +=
            Math.Floor(Math.Round((attacker.Skill * 8.0 - defender.AdjustedSpeed * 8.0 + 5000) * 100) / 10000.0);

        // Count hit modifiers
        var hitModifier = 0;
        if (defender.InFoolery) hitModifier++; // Foolery
        if (attacker.ActiveBattleSpecial.HasFlag(BattleSpecials.Will)) hitModifier++; // Will
        if (defender.ActiveBattleSpecial.HasFlag(BattleSpecials.Anger)) hitModifier++; // Anger
        if (defender.ActiveBattleSpecial.HasFlag(BattleSpecials.Ease)) hitModifier--; // Ease

        switch (hitModifier)
        {
            case > 0:
            {
                double i = hitModifier + 1;
                hitChance = 100 - Math.Pow(100 - hitChance, i) / Math.Pow(100, hitModifier);
                break;
            }
            case < 0:
                hitChance = Math.Pow(hitChance, 2) / 100;
                break;
        }

        // Clamp the hit chance
        hitChance = Math.Clamp(hitChance, 1, 99);
        return (int)Math.Floor(hitChance);
    }

    public Range<int> CalcWithering(Attacker user, IMonsterTechnique tech)
    {
        var (attacker, defender) = user == Attacker.Left ? (left, right) : (right, left);

        int w = tech.Withering;
        // Apply battle specials for attacker
        w = attacker.ActiveBattleSpecial switch
        {
            BattleSpecials.Anger or BattleSpecials.Fury =>
                w * 2,
            BattleSpecials.Real =>
                (int)Math.Floor(w * 1.5),
            _ => w
        };

        if (attacker.InactiveBattleSpecial.HasFlag(BattleSpecials.Real))
            w = (int)Math.Floor(w / 2.0);

        // Apply battle specials for defender
        if (defender.ActiveBattleSpecial.HasFlag(BattleSpecials.Real))
            w = (int)Math.Floor(w * 0.65625);
        if (defender.InactiveBattleSpecial.HasFlag(BattleSpecials.Real))
            w *= 2;

        // Add in RNG range
        return new Range<int>(Math.Clamp(w - 5, 0, 99), Math.Clamp(w + 5, 0, 99));
    }

    private int CalcRecoil(Attacker user, IMonsterTechnique tech, RecoilType recoilType)
    {
        var (attacker, defender) = user == Attacker.Left ? (left, right) : (right, left);
        var pow = attacker.Power == 999 ? 1000 : attacker.Power;
        var def = defender.AdjustedDefense == 999 ? 1000 : defender.AdjustedDefense;
        pow = (int)Math.Floor(Math.Round(pow / LevelDifferential * 10000) / 10000);
        def = (int)Math.Floor(Math.Round(def / LevelDifferential * 10000) / 10000);

        var pct = recoilType == RecoilType.HitSelf ? tech.ForceHitSelf : tech.ForceMissSelf;

        var dmg = Math.Floor(
            Math.Floor(Math.Round(Math.Floor(Math.Round(pct * (2.0 * CorrectionFactor + pow) / 3 * 10000) / 10000) /
                CorrectionFactor * 10000) / 10000) *
            Math.Round((10.0 * (pow - def) + 100 * CorrectionFactor) / CorrectionFactor * 100) / 10000);
        dmg = attacker.ActiveBattleSpecial switch
        {
            BattleSpecials.Power or BattleSpecials.Fury =>
                dmg * 2,
            BattleSpecials.Real =>
                dmg * 1.5,
            BattleSpecials.Guard =>
                Math.Ceiling(dmg / 2.0),
            _ => dmg
        };
        if (attacker.InactiveBattleSpecial.HasFlag(BattleSpecials.Real))
            dmg = Math.Ceiling(dmg / 2.0);

        return Math.Clamp((int)Math.Floor(dmg), 1, attacker.Hp - 1);
    }

    private int CalcLifeSteal(Attacker user, IMonsterTechnique tech)
    {
        var (attacker, defender) = user == Attacker.Left ? (left, right) : (right, left);
        if (tech.LifeSteal == 255)
            return 100;
        if (tech.LifeSteal == 0)
            return 0;
        int apow = tech.Scaling == TechType.Power ? attacker.Power : attacker.Intelligence;
        int dpow = tech.Scaling == TechType.Power ? defender.Power : defender.Intelligence;

        // DX special case
        if (apow == 999) apow = 1000;
        if (dpow == 999) dpow = 1000;

        // Normalize levels
        apow = (int)Math.Floor(Math.Round(apow / LevelDifferential * 10000) / 10000);
        dpow = (int)Math.Floor(Math.Round(dpow / LevelDifferential * 10000) / 10000);

        var re = (int)Math.Floor(
            Math.Round((tech.LifeSteal * CorrectionFactor + apow - dpow) / CorrectionFactor * 10000) / 10000);
        if (re > 100) re = 100;

        return re;
    }

    private int CalcGutsSteal(Attacker user, IMonsterTechnique tech)
    {
        var (attacker, defender) = user == Attacker.Left ? (left, right) : (right, left);

        if (tech.GutsSteal == 255)
            return 100;
        if (tech.GutsSteal == 0)
            return 0;
        var atkInt = attacker.Intelligence;
        var defInt = defender.Intelligence;
        if (atkInt == 999) atkInt = 1000;
        if (defInt == 999) defInt = 1000;

        // Normalize intelligence levels
        var aint = (int)Math.Floor(Math.Round(atkInt / LevelDifferential * 10000) / 10000);
        var dint = (int)Math.Floor(Math.Round(defInt / LevelDifferential * 10000) / 10000);

        var m = (int)Math.Floor(
            Math.Round((tech.GutsSteal * CorrectionFactor + (aint - dint)) / CorrectionFactor * 10000) / 10000);
        if (m > 100) m = 100;

        return m;
    }

    private int GutsCost(Attacker user, IMonsterTechnique tech)
    {
        var (attacker, defender) = user == Attacker.Left ? (left, right) : (right, left);
        // Adjust guts cost based on battle special
        var g = (int)tech.GutsCost;
        g = attacker.ActiveBattleSpecial switch
        {
            BattleSpecials.Ease => (int)Math.Ceiling(g * 0.65625),
            BattleSpecials.Will => g * 2,
            _ => g
        };
        return g;
    }
}