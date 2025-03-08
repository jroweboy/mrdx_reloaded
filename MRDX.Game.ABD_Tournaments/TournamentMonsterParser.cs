using System;
using System.Collections;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices.Marshalling;
using System.Xml.Linq;
using MRDX.Base.ExtractDataBin.Interface;
using MRDX.Base.Mod;
using MRDX.Base.Mod.Interfaces;
using Reloaded.Mod.Interfaces;
//using static MRDX.Base.Mod.Interfaces.TournamentData;
using Config = MRDX.Game.ABD_Tournaments.Configuration.Config;

namespace MRDX.Game.ABD_Tournaments;
public class TournamentMonsterParser
{
    static string ByteToHexASCII(ushort hex)
    {
        if (hex <= 9) { return hex.ToString(); }
        else if (hex == 10) { return "A"; }
        else if (hex == 11) { return "B"; }
        else if (hex == 12) { return "C"; }
        else if (hex == 13) { return "D"; }
        else if (hex == 14) { return "E"; }
        else if (hex == 15) { return "F"; }
        return "X";
    }

    ushort[] name = Mr2StringExtension.AsMr2("monsterName");



/*for (int i = 0; i<name.Length && i< 12; i++ )
    {
        fullNameHex += ToHexChar((ushort)(name[i] / 4096 % 16));
        fullNameHex += ToHexChar((ushort)(name[i] / 256 % 16));
        fullNameHex += ToHexChar((ushort)(name[i] / 16 % 16));
        fullNameHex += ToHexChar((ushort)(name[i] % 16));
        fullNameHex += " ";
    }*/


static void parseTest()
{
    Console.WriteLine("Path? " + Directory.GetCurrentDirectory());
    
    string test = "B5 0E B5 1A B5 24 B5 25 B5 1E B5 32 B5 26 B5 1A B5 27 FF 00 00 00 00 00 00 00 1B 1B C7 00 9E 00 51 00 37 00 93 00 17 00 19 14 14 00 81 02 00 00 01 0E 00 00 13 00 00 00 00 00 FF FF";
    test = test.Replace(" ", string.Empty);
    char[] monsterData = test.ToCharArray();

    string output = "Moster Name From Parse: ";


    for (var i = 0; i < 48; i += 4)
    {
        output += HexTextToCharacter(monsterData[i], monsterData[i + 1], monsterData[i + 2], monsterData[i + 3]);
    }

    
    Console.WriteLine(output);


}

static string HexTextToCharacter(char ht1, char ht2, char ht3, char ht4)
{
    ushort charValue = 0x0;
    charValue += HexTextToValue(ht4, ht3);
    charValue += (ushort)(HexTextToValue(ht2, ht1) * 256);
    return CharMap.Forward[charValue].ToString();
}

static ushort HexTextToValue(char ht1, char ht2)
{
    ushort charValue = 0x0;
    char[] hts = { ht1, ht2 };
    ushort htsValue = 0x0;
    for (var i = 0; i < 2; i++)
    {
        if (hts[i] == '0') { htsValue = 0; }
        else if (hts[i] == '1') { htsValue = 1; }
        else if (hts[i] == '2') { htsValue = 2; }
        else if (hts[i] == '3') { htsValue = 3; }
        else if (hts[i] == '4') { htsValue = 4; }
        else if (hts[i] == '5') { htsValue = 5; }
        else if (hts[i] == '6') { htsValue = 6; }
        else if (hts[i] == '7') { htsValue = 7; }
        else if (hts[i] == '8') { htsValue = 8; }
        else if (hts[i] == '9') { htsValue = 9; }
        else if (hts[i] == 'A') { htsValue = 10; }
        else if (hts[i] == 'B') { htsValue = 11; }
        else if (hts[i] == 'C') { htsValue = 12; }
        else if (hts[i] == 'D') { htsValue = 13; }
        else if (hts[i] == 'E') { htsValue = 14; }
        else if (hts[i] == 'F') { htsValue = 15; }

        charValue += (ushort)(htsValue * ((ushort)(Math.Pow(16, i))));
    }

    return charValue;
}
}

public enum EMonsterRanks { L, M, S, A, B, C, D, E };
public enum ETournamentPools { L, M, S, A, B, C, D, E,
    A_Phoenix, A_DEdge, B_Dragon, F_Hero, F_Heel, F_Elder,
    S_FIMBA, A_FIMBA, B_FIMBA, C_FIMBA, D_FIMBA,
    S_FIMBA2, A_FIMBA2, B_FIMBA2, C_FIMBA2, D_FIMBA2,
}
public class TournamentData
{
    public static Mod _mod;
    public static ILogger _logger;
    public static Config _configuration;
    
    public static string[] _random_name_list = ["Cimasio", "Kyrades", "Ambroros", "Teodeus", "Lazan", "Pegetus", "Perseos", "Asandrou", "Agametrios", 
        "Lazion", "Morphosyne", "Gelantinos", "Narkelous", "Taloclus", "Baltsalus", "Hypnaeon", "Atrol", "Alexede", "Baccinos", "Idastos", "Ophyroe","Larissa", "Asperata",
        "Alnifolia","Dentala","Celsa","Hempera","Laurel","Haldiphe","Saffronea", "Quinn",
        "Poplarbush","Snowdrop","Funnyfluff","Firo","Limespice","Herb","Twinklespa","Spring","Shinyglade",
        "Almond","Foggytree","Pecan","Jesterfeet","Skylark","Rainbowhor","Snow","Oakswamp","Liri", "Briarpuff",
        "Extos","Grimes","Talis","Anemia","Tinder","Neige","Lec","Chinook","Graund","Greidax","Pigatt",
        "Kuanezz","Nidar","Danuzz","Razodrug","Krorodurr","Galae",
        "Tepua","Uvle","Ujay","Surlul","Razsa","Dezunu","Urabu","Sholgokoh","Abedin","Yetsi","Jaedrey",
        "Leadeth","Baudr","Araldyng","Gilparymr","Sawah","Mazraeh",
        "Aayuh","Grimstriker","Twistmight","Pyregaze","Heatmarch","Omega","Dalton","Alpha","Beta","Gamma","Zeta","Phi","Jaeger",
        "Dimbranch","Raindust","Hillbrace","Storm","Sohish","Sunhol","Ehtae","Lilsof","Ghostsign","Snowlock","Mystic","Nevi","Azahz","Owen","Denzel","Robinson"];

    public bool _initialized = false;
    public bool _firstweek = false;
    public static Random GrowthRNG = new Random(0);
    public static Random LifespanRNG = new Random(1);

    public uint _currentWeek = 0;
    public List<MonsterGenus> _unlockedTournamentBreeds = new List<MonsterGenus>();

    public Dictionary<ETournamentPools, TournamentPool> tournamentPools = new Dictionary<ETournamentPools, TournamentPool>();
    public List<ABD_TournamentMonster> monsters = new List<ABD_TournamentMonster>();

    public List<byte[]> startingMonsterTemplates = new List<byte[]>();
   
    public TournamentData(Mod m, ILogger logger, Config config) 
    {
        TournamentData._mod = m;
        TournamentData._logger = logger;
        TournamentData._configuration = config;

        tournamentPools.Add( ETournamentPools.L, new TournamentPool( this, EMonsterRanks.L, ETournamentPools.L, "Legend", 2, 52, 53,     _configuration._confABD_tournament_rank_m4, 9999, 10 ) );
        tournamentPools.Add( ETournamentPools.M, new TournamentPool( this, EMonsterRanks.M, ETournamentPools.M, "Major 4", 8, 54, 61,    _configuration._confABD_tournament_rank_s,  _configuration._confABD_tournament_rank_m4 - 1, 8 ) );
        tournamentPools.Add( ETournamentPools.S, new TournamentPool( this, EMonsterRanks.S, ETournamentPools.S, "S Rank", 8, 1, 8,       _configuration._confABD_tournament_rank_a,  _configuration._confABD_tournament_rank_s - 1, 6));
        tournamentPools.Add( ETournamentPools.A, new TournamentPool( this, EMonsterRanks.A, ETournamentPools.A, "A Rank", 8, 9, 16,      _configuration._confABD_tournament_rank_b,  _configuration._confABD_tournament_rank_a - 1, 5));
        tournamentPools.Add( ETournamentPools.B, new TournamentPool( this, EMonsterRanks.B, ETournamentPools.B, "B Rank", 10, 17, 26,    _configuration._confABD_tournament_rank_c,  _configuration._confABD_tournament_rank_b - 1, 4));
        tournamentPools.Add( ETournamentPools.C, new TournamentPool( this, EMonsterRanks.C, ETournamentPools.C, "C Rank", 10, 27, 36,    _configuration._confABD_tournament_rank_d,  _configuration._confABD_tournament_rank_c - 1, 2));
        tournamentPools.Add( ETournamentPools.D, new TournamentPool( this, EMonsterRanks.D, ETournamentPools.D, "D Rank", 8, 37, 44,     _configuration._confABD_tournament_rank_e,  _configuration._confABD_tournament_rank_d - 1, 1));
        tournamentPools.Add( ETournamentPools.E, new TournamentPool( this, EMonsterRanks.E, ETournamentPools.E, "E Rank", 6, 45, 50,     750,                                        _configuration._confABD_tournament_rank_e - 1, 0));


        tournamentPools.Add( ETournamentPools.A_Phoenix, new TournamentPool( this, EMonsterRanks.A,     ETournamentPools.A_Phoenix, "A - Phoenix", 3, 62, 64, _configuration._confABD_tournament_rank_b, _configuration._confABD_tournament_rank_a, 6 ) );
        tournamentPools.Add( ETournamentPools.A_DEdge, new TournamentPool( this, EMonsterRanks.A,       ETournamentPools.A_DEdge,   "A - Double Edge", 1, 66, 66, _configuration._confABD_tournament_rank_b, _configuration._confABD_tournament_rank_a, 6 ) );
        tournamentPools.Add( ETournamentPools.B_Dragon, new TournamentPool( this, EMonsterRanks.B,      ETournamentPools.B_Dragon,  "B - Dragon Tusk", 1, 65, 65, _configuration._confABD_tournament_rank_c, _configuration._confABD_tournament_rank_b, 6 ) );
        tournamentPools.Add( ETournamentPools.F_Hero,  new TournamentPool( this, EMonsterRanks.A,       ETournamentPools.F_Hero,    "F - Hero", 5, 67, 71, _configuration._confABD_tournament_rank_b, 9999, 7 ) );
        tournamentPools.Add( ETournamentPools.F_Heel,  new TournamentPool( this, EMonsterRanks.A,       ETournamentPools.F_Heel,    "F - Heel", 5, 72, 76, _configuration._confABD_tournament_rank_b, 9999, 7 ) );
        tournamentPools.Add( ETournamentPools.F_Elder, new TournamentPool( this, EMonsterRanks.A,       ETournamentPools.F_Elder,   "F - Elder", 3, 77, 79, _configuration._confABD_tournament_rank_b, 9999, 8 ) );

        tournamentPools.Add( ETournamentPools.S_FIMBA, new TournamentPool(this, EMonsterRanks.S,        ETournamentPools.S_FIMBA,   "S FIMBA", 4, 80, 83, _configuration._confABD_tournament_rank_a + 200, _configuration._confABD_tournament_rank_m4, 8 ) );
        tournamentPools.Add( ETournamentPools.A_FIMBA, new TournamentPool( this, EMonsterRanks.A,       ETournamentPools.A_FIMBA,   "A FIMBA", 4, 84, 87, _configuration._confABD_tournament_rank_b + 200, _configuration._confABD_tournament_rank_a, 6 ) );
        tournamentPools.Add( ETournamentPools.B_FIMBA, new TournamentPool( this, EMonsterRanks.B,       ETournamentPools.B_FIMBA,   "B FIMBA", 4, 88, 91, _configuration._confABD_tournament_rank_c + 200, _configuration._confABD_tournament_rank_b, 5 ) );
        tournamentPools.Add( ETournamentPools.C_FIMBA, new TournamentPool( this, EMonsterRanks.C,       ETournamentPools.C_FIMBA,   "C FIMBA", 4, 92, 95, _configuration._confABD_tournament_rank_d + 100, _configuration._confABD_tournament_rank_c, 4 ) );
        tournamentPools.Add( ETournamentPools.D_FIMBA, new TournamentPool( this, EMonsterRanks.D,       ETournamentPools.D_FIMBA,   "D FIMBA", 4, 96, 99, _configuration._confABD_tournament_rank_e + 100, _configuration._confABD_tournament_rank_d, 2 ) );

        tournamentPools.Add( ETournamentPools.S_FIMBA2, new TournamentPool( this, EMonsterRanks.S,      ETournamentPools.S_FIMBA2,  "S FIMBA2", 4, 100, 103, _configuration._confABD_tournament_rank_a + 200, _configuration._confABD_tournament_rank_m4, 8 ) );
        tournamentPools.Add( ETournamentPools.A_FIMBA2, new TournamentPool( this, EMonsterRanks.A,      ETournamentPools.A_FIMBA2,  "A FIMBA2", 4, 104, 107, _configuration._confABD_tournament_rank_b + 200, _configuration._confABD_tournament_rank_a, 6 ) );
        tournamentPools.Add( ETournamentPools.B_FIMBA2, new TournamentPool( this, EMonsterRanks.B,      ETournamentPools.B_FIMBA2,  "B FIMBA2", 4, 108, 111, _configuration._confABD_tournament_rank_c + 200, _configuration._confABD_tournament_rank_b, 5 ) );
        tournamentPools.Add( ETournamentPools.C_FIMBA2, new TournamentPool( this, EMonsterRanks.C,      ETournamentPools.C_FIMBA2,  "C FIMBA2", 4, 112, 115, _configuration._confABD_tournament_rank_d + 100, _configuration._confABD_tournament_rank_c, 4 ) );
        tournamentPools.Add( ETournamentPools.D_FIMBA2, new TournamentPool( this, EMonsterRanks.D,      ETournamentPools.D_FIMBA2,  "D FIMBA2", 4, 116, 118, _configuration._confABD_tournament_rank_e + 100, _configuration._confABD_tournament_rank_d, 2 ) );
    }

    public void AddExistingMonster(TournamentMonster m, int id)
    {
        ETournamentPools pool = ETournamentPools.S;
        ABD_TournamentMonster abdm = new ABD_TournamentMonster(m);

        if ( id >= 1 && id <= 8 )           { pool = ETournamentPools.S; }
        else if ( id >= 9 && id <= 16 )     { pool = ETournamentPools.A; }
        else if ( id >= 17 && id <= 26 )    { pool = ETournamentPools.B; }
        else if ( id >= 27 && id <= 36 )    { pool = ETournamentPools.C; }
        else if ( id >= 37 && id <= 44 )    { pool = ETournamentPools.D; }
        else if ( id >= 45 && id <= 50 )    { pool = ETournamentPools.E; }

        else if ( id >= 52 && id <= 53 ) { pool = ETournamentPools.L; }
        else if ( id >= 54 && id <= 61 ) { pool = ETournamentPools.M; }

        else if ( id >= 62 && id <= 64 ) { pool = ETournamentPools.A_Phoenix; }
        else if ( id >= 65 && id <= 65 ) { pool = ETournamentPools.B_Dragon; }
        else if ( id >= 66 && id <= 66 ) { pool = ETournamentPools.A_DEdge; }
        
        else if ( id >= 67 && id <= 71 ) { pool = ETournamentPools.F_Hero; }
        else if ( id >= 72 && id <= 76 ) { pool = ETournamentPools.F_Heel; }
        else if ( id >= 77 && id <= 79 ) { pool = ETournamentPools.F_Elder; }

        else if ( id >= 80 && id <= 83 ) { pool = ETournamentPools.S_FIMBA; }
        else if ( id >= 84 && id <= 87 ) { pool = ETournamentPools.A_FIMBA; }
        else if ( id >= 88 && id <= 91 ) { pool = ETournamentPools.B_FIMBA; }
        else if ( id >= 92 && id <= 95 ) { pool = ETournamentPools.C_FIMBA; }
        else if ( id >= 96 && id <= 99 ) { pool = ETournamentPools.D_FIMBA; }

        else if ( id >= 100 && id <= 103 ) { pool = ETournamentPools.S_FIMBA2; }
        else if ( id >= 104 && id <= 107 ) { pool = ETournamentPools.A_FIMBA2; }
        else if ( id >= 108 && id <= 111 ) { pool = ETournamentPools.B_FIMBA2; }
        else if ( id >= 112 && id <= 115 ) { pool = ETournamentPools.C_FIMBA2; }
        else if ( id >= 116 && id <= 118 ) { pool = ETournamentPools.D_FIMBA2; }

        abdm._monsterRank = tournamentPools[ pool ]._monsterRank;

        monsters.Add(abdm);
        tournamentPools[pool].MonsterAdd(abdm);
    }

    public void AddExistingMonster(ABD_TournamentMonster abdm) {
        monsters.Add( abdm );

        foreach ( TournamentPool pool in tournamentPools.Values ) {
            for ( var i = 0; i < 4; i++ ) {
                if ( (ETournamentPools) abdm._rawpools[ i ] == pool._tournamentPool ) {
                    pool.MonsterAdd( abdm );
                }
            }
        }
    }

    public void AdvanceWeek ( uint currentWeek, List<MonsterGenus> unlockedmonsters ) {
        _unlockedTournamentBreeds = unlockedmonsters;

        if ( !_initialized ) { _currentWeek = currentWeek; return; }
        if ( _firstweek ) { _currentWeek = currentWeek - 1; }

        while ( _currentWeek < currentWeek ) {
            _currentWeek++;

            if ( _currentWeek % 4 == 0 ) {
                AdvanceMonth();
            }

            if ( _currentWeek % 12 == 0 ) {
                AdvanceTournamentPromotions();
            }

        }

        _mod.DebugLog( 1, "Finished Advancing Weeks, Checking Pools", Color.Yellow );
        foreach ( TournamentPool pool in tournamentPools.Values ) {
            while ( pool.monsters.Count < pool._minimumSize ) {
                pool.GenerateNewValidMonster(_unlockedTournamentBreeds);
            }

            // Shuffle Monsters - TODO: This should happen weekly.
            ABD_TournamentMonster[] ml = pool.monsters.ToArray();
            Random.Shared.Shuffle( ml );
            pool.monsters = new List<ABD_TournamentMonster>( ml );
        }

        _firstweek = false;
    }
    public void AdvanceMonth()
    {
        _mod.DebugLog( 1, "Advancing month from TournamentData", Color.Blue );
        for (var i = monsters.Count - 1 ; i >= 0 ; i--) {
            var m = monsters[i];

            m.AdvanceMonth();
            if ( !m.alive ) {
                _mod.DebugLog( 3, m.monster.name + " has died. Removing from pools.", Color.Blue );
                for ( var j = m.pools.Count() - 1; j >= 0; j-- ) {
                    m.pools[j].MonsterRemove( m );
                    _mod.DebugLog( 3, m.monster.name + " removed from a pool.", Color.Blue );
                }
                _mod.DebugLog( 3, m.monster.name + " removing from master pool list.", Color.Blue );
                monsters.Remove( m );
                _mod.DebugLog( 3, m.monster.name + " removed from master pool list.", Color.Blue );
            }

            // TODO CONFIG TECHNIQUE RATE
            if ( Random.Shared.Next() % 25 == 0 ) {
                m.LearnTechnique();
            }
        }
    }

    private void AdvanceTournamentPromotions() {
        tournamentPools[ ETournamentPools.M ].MonsterPromoteToNewPool( tournamentPools[ ETournamentPools.L ] );
        tournamentPools[ ETournamentPools.S ].MonsterPromoteToNewPool( tournamentPools[ ETournamentPools.M ] );
        tournamentPools[ ETournamentPools.A ].MonsterPromoteToNewPool( tournamentPools[ ETournamentPools.S ] );
        tournamentPools[ ETournamentPools.B ].MonsterPromoteToNewPool( tournamentPools[ ETournamentPools.A ] );
        tournamentPools[ ETournamentPools.C ].MonsterPromoteToNewPool( tournamentPools[ ETournamentPools.B ] );
        tournamentPools[ ETournamentPools.D ].MonsterPromoteToNewPool( tournamentPools[ ETournamentPools.C ] );
        tournamentPools[ ETournamentPools.E ].MonsterPromoteToNewPool( tournamentPools[ ETournamentPools.D ] );
    }

    public List<ABD_TournamentMonster>GetTournamentMembers( int start, int end ) {
        List<ABD_TournamentMonster> participants = new List<ABD_TournamentMonster>();

        tournamentPools[ ETournamentPools.S ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.A ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.B ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.C ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.D ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.E ].AddTournamentParticipants( participants );

        // Edge Case to Handle the ONE SLOT that is skipped For Moo.
        participants.Add( tournamentPools[ ETournamentPools.S ].monsters[ 0 ] );

        tournamentPools[ ETournamentPools.L ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.M ].AddTournamentParticipants( participants );

        tournamentPools[ ETournamentPools.A_Phoenix ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.B_Dragon ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.A_DEdge ].AddTournamentParticipants( participants );

        tournamentPools[ ETournamentPools.F_Hero ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.F_Heel ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.F_Elder ].AddTournamentParticipants( participants );

        tournamentPools[ ETournamentPools.S_FIMBA ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.A_FIMBA ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.B_FIMBA ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.C_FIMBA ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.D_FIMBA ].AddTournamentParticipants( participants );

        tournamentPools[ ETournamentPools.S_FIMBA2 ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.A_FIMBA2 ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.B_FIMBA2 ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.C_FIMBA2 ].AddTournamentParticipants( participants );
        tournamentPools[ ETournamentPools.D_FIMBA2 ].AddTournamentParticipants( participants );

        return participants;
    }

    public void ClearAllData() {
        _initialized = false;

        monsters.Clear();
        foreach( TournamentPool pool in tournamentPools.Values ) {
            pool.monsters.Clear();
        }
    }
}


public class ABD_TournamentMonster
{
    public TournamentMonster monster;
    public MonsterBreed breedInfo;

    public List<TournamentPool> pools = new List<TournamentPool>();
    public byte [] _rawpools;

    public bool alive = true;
    public ushort lifetotal = 0;
    public ushort lifespan = 0;

    ushort growth_rate = 0;
    private byte[] growth_options;

    public EMonsterRanks _monsterRank;

    public enum growth_groups { balanced, power, intel, defend, wither, speedy }
    growth_groups growth_group = growth_groups.balanced;
    

    public ABD_TournamentMonster(TournamentMonster m)
    {
        TournamentData._mod.DebugLog( 1, "Creating monster from game data.", Color.Lime );
        monster = m;
        breedInfo = MonsterBreed.GetBreedInfo( monster.breed_main, monster.breed_sub );

        lifetotal = (ushort) ( 40 + TournamentData.LifespanRNG.Next() % 31 ); // 40-70
        
        lifespan = lifetotal;
        lifespan -= (ushort) ( 4 * ( monster.stat_total / 500 ) ); // Take an arbitrary amount of life off for starting stats.

        growth_rate = (ushort) TournamentData._configuration._confABD_growth_monthly;
        for ( var i = 0; i < 4; i++ ) {
            growth_rate -= (ushort) TournamentData._configuration._confABD_growth_monthlyvariance;
            growth_rate += (ushort) (TournamentData.GrowthRNG.Next() % ( 2 * TournamentData._configuration._confABD_growth_monthlyvariance ));
        }

        growth_rate = Math.Clamp( growth_rate, (ushort) (TournamentData._configuration._confABD_growth_monthly / 2), (ushort) (TournamentData._configuration._confABD_growth_monthly * 1.66) );
        TournamentData._mod.DebugLog( 3, "Growth Post Variance" + growth_rate, Color.Lime );

        growth_rate = (ushort) (1 + ( growth_rate / 4 ));// Account for Prime Bonus, 4x
        TournamentData._mod.DebugLog( 3, "Growth Post Prime" + growth_rate, Color.Lime );

        growth_group = (growth_groups) ( TournamentData.GrowthRNG.Next() % 6 );

        // This section applies special bonuses to monster breeds. Dragons are in both groups, and a pure Dragon/Dragon gets +6/+4 to its growth rate. A Tiger/Gali would only get +0/+2 as Gali is only in one group and only the sub.
        List<MonsterGenus> bonuses = new List<MonsterGenus>(); List<MonsterGenus> bonuses2 = new List<MonsterGenus>();
        bonuses.AddRange( [MonsterGenus.Dragon, MonsterGenus.Centaur, MonsterGenus.Beaclon, MonsterGenus.Henger, MonsterGenus.Wracky, MonsterGenus.Durahan, MonsterGenus.Gali, MonsterGenus.Zilla, MonsterGenus.Bajarl, MonsterGenus.Phoenix, 
            MonsterGenus.Metalner, MonsterGenus.Jill, MonsterGenus.Joker, MonsterGenus.Undine, MonsterGenus.Mock, MonsterGenus.Unknown1, MonsterGenus.Unknown2, MonsterGenus.Unknown3, MonsterGenus.Unknown4, MonsterGenus.Unknown5, MonsterGenus.Unknown6] );

        bonuses2.AddRange( [MonsterGenus.Dragon, MonsterGenus.Centaur, MonsterGenus.Beaclon, MonsterGenus.Durahan, MonsterGenus.Zilla, MonsterGenus.Phoenix, MonsterGenus.Metalner, MonsterGenus.Joker, MonsterGenus.Undine,
            MonsterGenus.Unknown1, MonsterGenus.Unknown2, MonsterGenus.Unknown3, MonsterGenus.Unknown4, MonsterGenus.Unknown5, MonsterGenus.Unknown6] );

        if ( bonuses.Contains( (MonsterGenus) monster.breed_main) ) { growth_rate += 3; }
        if ( bonuses2.Contains( (MonsterGenus) monster.breed_main ) ) { growth_rate += 3; }
        if ( bonuses.Contains( (MonsterGenus) monster.breed_sub ) ) { growth_rate += 2; }
        if ( bonuses2.Contains( (MonsterGenus) monster.breed_sub ) ) { growth_rate += 2; }

        TournamentData._mod.DebugLog( 2, "Monster Created: " + m.name + ", " + m.breed_main + "/" + m.breed_sub + ", LIFE: " + lifespan + ", GROWTH: " + growth_rate, Color.Lime );

        SetupGrowthOptions();
    }

    public ABD_TournamentMonster(byte[] rawabd) {
        TournamentData._mod.DebugLog( 1, "Loading monster from ABD Save File.", Color.Lime );
        byte[] rawmonster = new byte[ 60 ];
        for ( var i = 0; i < 60; i++ ) { rawmonster[ i ] = rawabd[ i + 40 ]; }

        monster = new TournamentMonster( rawmonster );
        breedInfo = MonsterBreed.GetBreedInfo( monster.breed_main, monster.breed_sub );

        lifetotal = rawabd[ 0 ];
        lifespan = rawabd[ 2 ];
        growth_rate = rawabd[ 4 ];
        growth_group = (growth_groups) rawabd[ 6 ];

        _monsterRank = (EMonsterRanks) rawabd[ 8 ];

        _rawpools = new byte[ 4 ];
        for ( var i = 0; i < 4; i++ ) {
            _rawpools[ i ] = rawabd[ 10 + i ];
        }

        SetupGrowthOptions();
    }

    private void SetupGrowthOptions() {
        if ( growth_group == growth_groups.balanced ) { growth_options =    [ 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5 ]; }
        else if ( growth_group == growth_groups.power ) { growth_options =  [ 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 4, 4, 4, 4, 4, 5 ]; }
        else if ( growth_group == growth_groups.intel ) { growth_options =  [ 0, 0, 0, 0, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5 ]; }
        else if ( growth_group == growth_groups.defend ) { growth_options = [ 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 2, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5 ]; }
        else if ( growth_group == growth_groups.wither ) { growth_options = [ 0, 0, 0, 0, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 5, 5, 5, 5, 5 ]; }
        else if ( growth_group == growth_groups.speedy ) { growth_options = [ 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 5, 5 ]; }
    }

    public void AdvanceMonth()
    {
        int agegroup = 2;
        lifespan--;
        if ( lifespan == 0 ) { alive = false; }

        if ( (lifetotal - lifespan) > 9) { agegroup++; }
        if ( ( lifetotal - lifespan ) > 18 ) { agegroup++; }
        if ( lifespan < 12 ) { agegroup--;  }
        if ( lifespan < 6 ) { agegroup--; agegroup--; }

        agegroup *= growth_rate;

        TournamentData._mod.DebugLog( 2, "Monster " + monster.name + " Advancing Month: [GROWTH:" + growth_rate + " CGROW: " + agegroup + ", LIFE: " + lifespan + "]", Color.Yellow );

        for (var i = 0; i < agegroup; i++) {
            var stat = growth_options[TournamentData.GrowthRNG.Next() % growth_options.Length];
            if (stat == 0) { monster.stat_lif++; }
            else if (stat == 1) { monster.stat_pow++; }
            else if (stat == 2) { monster.stat_ski++; }
            else if (stat == 3) { monster.stat_spd++; }
            else if (stat == 4) { monster.stat_def++; }
            else if (stat == 5) { monster.stat_int++; }
            else { monster.stat_lif++; }
        }

        if ( monster.per_fear < 100 ) { monster.per_fear += (byte) (TournamentData.GrowthRNG.Next() % 2); }
        if ( monster.per_spoil < 100 ) { monster.per_spoil += (byte) ( TournamentData.GrowthRNG.Next() % 2); }

        TournamentData._mod.DebugLog( 3, "Monster " + monster.name + " Completed Growth: [GROWTH:" + growth_rate + ", LIFE: " + lifespan + ", ALIVE: " + alive + "]", Color.Yellow );
    }

    public void LearnTechnique() { // TODO: Smarter Logic About which tech to get
        TournamentData._mod.DebugLog( 2, "Monster " + monster.name + " attempting to learn technique.", Color.Orange );
        // 0 = Basic, 1 Hit, 2 Heavy, 3 Withering, 4 Sharp, 5 Special, 6 Invalid
        List<byte> toLearn = new List<byte>();
        for ( var i = 0; i < 24; i++ ) {
            var type = breedInfo.technique_types[ i ];
            if ( type != 6 && ( ( monster.techniques >> i ) & 1 ) == 0 ) {
                if ( growth_group == growth_groups.power && type == 2 ) { toLearn.Add( (byte) i ); }
                else if ( growth_group == growth_groups.intel && type == 1 ) { toLearn.Add( (byte) i ); }
                else if ( growth_group == growth_groups.wither && type == 3 ) { toLearn.Add( (byte) i ); }
                else if ( growth_group == growth_groups.speedy && type == 4 ) { toLearn.Add( (byte) i ); }

                if ( breedInfo.technique_types[ i ] == 5 ) {
                    if ( ( _monsterRank == EMonsterRanks.S || _monsterRank == EMonsterRanks.A || _monsterRank == EMonsterRanks.B ) ) { toLearn.Add( (byte) i ); toLearn.Add( (byte) i ); }
                }
                else { toLearn.Add( (byte) i ); }
            }
        }

        if ( toLearn.Count > 0 ) {
            monster.techniques += (uint) ( 1 << toLearn[ Random.Shared.Next() % toLearn.Count ] );
        }
    }

    /// <summary>
    /// Promotes a Monster to a specifc rank, learning specials at D and A ranks. 
    /// Relies on the order of the enum to work properly!
    /// </summary>
    /// <param name="rank"></param>
    public void PromoteToRank ( EMonsterRanks rank ) {
        if ( rank >= _monsterRank ) { _monsterRank = rank; return; }

        else {
            while ( _monsterRank != rank ) {
                _monsterRank--;
                if ( _monsterRank == EMonsterRanks.A ) { LearnBattleSpecial(); }
                if ( _monsterRank == EMonsterRanks.D ) { LearnBattleSpecial(); }
            }
        }
    }

    public void LearnBattleSpecial() {
        monster.battle_specials |= (ushort) ( 1 << TournamentData.GrowthRNG.Next() % 13 );
    }

    public byte[] ToSaveFile() {
        // ABD_TournamentMonster data will consist of 40 new bytes, followed by the standard 60 tracked by the game for Tournament Monsters.
        // 0-1, Lifetotal
        // 2-3, Current Remaining Lifespan
        // 4-5, Growth Rate Per Months
        // 6-7, Growth Group (Enum)
        // 8-9, Monster Rank (Enum)
        // 10-13, TournamentPools (Enums)
        // 14-39, UNUSED

        byte[] data = new byte[ 40 + 60 ]; 

        data[ 0 ] = (byte) lifetotal;
        data[ 2 ] = (byte) lifespan;
        data[ 4 ] = (byte) growth_rate;
        data[ 6 ] = (byte) growth_group;
        data[ 8 ] = (byte) _monsterRank;

        data[ 10 ] = 0xFF; data[ 11 ] = 0xFF; data[ 12 ] = 0xFF; data[ 13 ] = 0xFF;
        for ( var i = 0; ( i < pools.Count() && i < 4); i++ ) {
            data[ 10 + i ] = (byte) pools[ i ]._tournamentPool;
        }

        for ( var i = 0; i < 60; i++ ) {
            data[ i + 40 ] = monster.raw_bytes[ i ];
        }
        return data;
    }
}

class TM_Growth
{
    public int growthSeed;

    public TM_Growth(int seed)
    {
        growthSeed = seed;
    }

    public TM_Growth() { }
}

