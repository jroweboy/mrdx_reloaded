namespace MRDX.Base.Mod.Interfaces;

public interface IMonsterAttack
{
    string Name { get; set; }
    ErrantryType Errantry { get; set; }
    TechRange Range { get; set; }
    TechNature Nature { get; set; }
    TechType Tech { get; set; }
    
    int Hit { get; set; }
    int Force { get; set; }
    int Wither { get; set; }
    int Sharp { get; set; }
    int GutsCost { get; set; }

    bool SGutsSteal { get; set; }
    bool SLifeSteal { get; set; }
    bool SLifeRecovery { get; set; }
    bool SDamageSelfMiss { get; set; }
    bool SDamageSelfHit { get; set; }
}

public interface IBattleAttack
{
    byte SelectedTechSlot1 { get; set; }
    byte SelectedTechSlot2 { get; set; }
    byte SelectedTechSlot3 { get; set; }
    byte SelectedTechSlot4 { get; set; }
}