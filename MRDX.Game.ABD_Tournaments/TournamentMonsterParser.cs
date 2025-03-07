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
using static MRDX.Base.Mod.Interfaces.TournamentData;

namespace MRDX.Base.Mod.Interfaces;
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

public enum EMonsterRanks { S, A, B, C, D, E };
public class TournamentData
{
    public static ILogger _logger;

    public bool _initialized = false;
    public bool _firstweek = false;
    public static Random GrowthRNG = new Random(0);
    public static Random LifespanRNG = new Random(1);

    public uint _currentWeek = 0;
    public List<MonsterGenus> _unlockedTournamentBreeds = new List<MonsterGenus>();

    // Promotion Data
    //D Rank: 900
    //C Rank: 1300
    //B Rank: 1700
    //A Rank: 2300
    //S Rank: 2800
    //Major 4: 3500
    public enum e_pools { S, A, B, C, D, E };
    
    public Dictionary<e_pools, TournamentPool> tournamentPools = new Dictionary<e_pools, TournamentPool>();
    public List<ABD_TournamentMonster> monsters = new List<ABD_TournamentMonster>();

    public List<byte[]> startingMonsterTemplates = new List<byte[]>();
   
    public TournamentData(ILogger logger) 
    {
        TournamentData._logger = logger;

        tournamentPools.Add(e_pools.S, new TournamentPool(this, EMonsterRanks.S, "S Rank", 8, 1, 8,    2800, 9900, 5));
        tournamentPools.Add(e_pools.A, new TournamentPool(this, EMonsterRanks.A, "A Rank", 8, 9, 16,   2300, 2599, 4));
        tournamentPools.Add(e_pools.B, new TournamentPool(this, EMonsterRanks.B, "B Rank", 10, 17, 26, 1700, 2099, 3));
        tournamentPools.Add(e_pools.C, new TournamentPool(this, EMonsterRanks.C, "C Rank", 10, 27, 36, 1300, 1699, 2));
        tournamentPools.Add(e_pools.D, new TournamentPool(this, EMonsterRanks.D, "D Rank", 8, 37, 44,  1000, 1299, 1));
        tournamentPools.Add(e_pools.E, new TournamentPool(this, EMonsterRanks.E, "E Rank", 6, 45, 50,  800,  999 , 0));

        /*startingMonsterTemplates.Add([181, 1, 181, 40, 181, 43, 181, 26, 181, 39, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 7, 215, 0, 72, 1, 228, 0, 233, 0, 184, 0, 216, 0, 50, 50, 50, 0, 255, 104, 9, 0, 3, 13, 0, 0, 65, 0, 0, 0, 0, 0, 255, 255]); // 112 Boran?
        startingMonsterTemplates.Add([181, 14, 181, 26, 181, 36, 181, 37, 181, 30, 181, 50, 181, 38, 181, 26, 181, 39, 255, 0, 0, 0, 0, 0, 0, 0, 27, 27, 154, 0, 158, 0, 91, 0, 62, 0, 155, 0, 26, 0, 25, 20, 20, 0, 129, 2, 0, 0, 1, 14, 0, 0, 19, 0, 0, 0, 0, 0, 255, 255]); // TODO : OAKLEYMANNNN
        startingMonsterTemplates.Add([181, 12, 181, 40, 181, 41, 181, 30, 181, 45, 181, 40, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 7, 240, 0, 61, 1, 245, 0, 252, 0, 39, 1, 26, 1, 201, 40, 40, 0, 255, 0, 0, 0, 4, 9, 0, 0, 74, 1, 0, 0, 0, 0, 255, 255]); // 99 Mopeto
        startingMonsterTemplates.Add([181, 11, 181, 26, 181, 32, 181, 34, 181, 43, 181, 46, 181, 44, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 180, 1, 241, 1, 162, 1, 152, 2, 183, 1, 99, 2, 25, 50, 50, 0, 223, 55, 0, 0, 2, 17, 0, 0, 33, 0, 0, 0, 0, 0, 255, 255]); //65 Lagrius
        startingMonsterTemplates.Add([181, 11, 181, 30, 181, 40, 181, 39, 181, 30, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 13, 99, 0, 116, 0, 87, 0, 171, 0, 162, 0, 143, 0, 55, 30, 30, 0, 5, 0, 0, 0, 3, 11, 0, 0, 11, 1, 0, 0, 0, 0, 255, 255]); // 48Leone*/
    }

    public void AddExistingMonster(TournamentMonster m, int id)
    {
        e_pools pool = e_pools.S;
        ABD_TournamentMonster abdm = new ABD_TournamentMonster(m);

        if ( id >= 1 && id <= 8 )           { pool = TournamentData.e_pools.S; }
        else if ( id >= 9 && id <= 16 )     { pool = TournamentData.e_pools.A; }
        else if ( id >= 17 && id <= 26 )    { pool = TournamentData.e_pools.B; }
        else if ( id >= 27 && id <= 36 )    { pool = TournamentData.e_pools.C; }
        else if ( id >= 37 && id <= 44 )    { pool = TournamentData.e_pools.D; }
        else if ( id >= 45 && id <= 50 )    { pool = TournamentData.e_pools.E; }

        abdm._monsterRank = tournamentPools[ pool ]._monsterRank;

        monsters.Add(abdm);
        tournamentPools[pool].MonsterAdd(abdm);
    }

    public void AddExistingMonster(ABD_TournamentMonster abdm) {
        monsters.Add( abdm );
        foreach ( TournamentPool pool in tournamentPools.Values ) {
            if ( ( abdm.monster.stat_total >= pool.stat_start && abdm.monster.stat_total <= pool.stat_end ) ) {
                pool.MonsterAdd( abdm );
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

        _logger.WriteLineAsync( "[ABD Tournaments]: Finished Advancing Weeks, Checking Pools", Color.Orange );
        foreach ( TournamentPool pool in tournamentPools.Values ) {
            while ( pool.monsters.Count < pool._minimumSize ) {
                pool.GenerateNewValidMonster(_unlockedTournamentBreeds);
            }

            // Shuffle Monsters - TODO: This should happen weekly.
            ABD_TournamentMonster[] ml = pool.monsters.ToArray();
            Random.Shared.Shuffle( ml );
            pool.monsters = new List<ABD_TournamentMonster>( ml );
        }
    }
    public void AdvanceMonth()
    {
        _logger.WriteLineAsync( "[ABD Tournaments]: Advancing Month", Color.Blue );
        for (var i = monsters.Count -1 ; i >= 0 ; i--) {
            var m = monsters[i];

            m.AdvanceMonth();
            if ( !m.alive ) {
                foreach ( TournamentPool tp in m.pools ) {
                    tp.MonsterRemove( m );
                }
                monsters.Remove( m );
            }

            // TODO CONFIG TECHNIQUE RATE
            if ( Random.Shared.Next() % 20 == 0 ) {
                m.LearnTechnique();
            }
        }
    }

    private void AdvanceTournamentPromotions() {
        _logger.WriteLineAsync( "[ABD Tournaments]: Tournament Promotions", Color.Orange );
        tournamentPools[ e_pools.A ].MonsterPromoteToNewPool( tournamentPools[ e_pools.S ] );
        tournamentPools[ e_pools.B ].MonsterPromoteToNewPool( tournamentPools[ e_pools.A ] );
        tournamentPools[ e_pools.C ].MonsterPromoteToNewPool( tournamentPools[ e_pools.B ] );
        tournamentPools[ e_pools.D ].MonsterPromoteToNewPool( tournamentPools[ e_pools.C ] );
        tournamentPools[ e_pools.E ].MonsterPromoteToNewPool( tournamentPools[ e_pools.D ] );
    }

    public List<ABD_TournamentMonster>GetTournamentMembers( int start, int end ) {
        List<ABD_TournamentMonster> participants = new List<ABD_TournamentMonster>();

        for (var i = 0; i < 8; i++) { participants.Add(tournamentPools[e_pools.S].monsters[i]); }
        for (var i = 0; i < 8; i++) { participants.Add(tournamentPools[e_pools.A].monsters[i]); }
        for (var i = 0; i < 10; i++) { participants.Add(tournamentPools[e_pools.B].monsters[i]); }
        for (var i = 0; i < 10; i++) { participants.Add(tournamentPools[e_pools.C].monsters[i]); }
        for ( var i = 0; i < 8; i++ ){ participants.Add(tournamentPools[e_pools.D].monsters[i]);}
        for ( var i = 0; i < 6; i++ ) { participants.Add(tournamentPools[e_pools.E].monsters[i]); }

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

public class TournamentPool
{
    public TournamentData tournamentData;
    public EMonsterRanks _monsterRank;
    public string _name = "Pool";
    public int _minimumSize = 0;
    public int _indexStart = 0;
    public int _indexEnd = 0;

    public int stat_start = 0;
    public int stat_end = 0;

    public int tournament_tier = 0; // Used for adding techniques and specials to new monsters generated for this tournament

    public List<ABD_TournamentMonster> monsters = new List<ABD_TournamentMonster>();

    public TournamentPool(TournamentData data, EMonsterRanks rank, string name, int size, int istart, int iend, int sstart, int send, int tier)
    {
        tournamentData = data;
        _monsterRank = rank;

        _name = name;
        _minimumSize = size;
        _indexStart = istart;
        _indexEnd = iend;

        stat_start = sstart;
        stat_end = send;

        tournament_tier = tier;
    }

    public void MonsterAdd(ABD_TournamentMonster m)
    {
        if (!monsters.Contains(m)) {
            monsters.Add(m);
            m.pools.Add(this);
        }
    }

    public void MonsterRemove(ABD_TournamentMonster m)
    {
        if ( monsters.Contains(m) ) {
            monsters.Remove(m);
            m.pools.Remove(this);
        }
    }

    public void MonsterPromoteToNewPool(TournamentPool newPool) {
        _logger.WriteLineAsync( "[ABD Tournaments]: Promoting Monster to new pool : " + newPool, Color.Orange );
        int stattotal = 0; ABD_TournamentMonster promoted;
        promoted = monsters[ 0 ];

        foreach ( ABD_TournamentMonster abdm in monsters ) {
            stattotal += abdm.monster.stat_total;
        }

        stattotal = Random.Shared.Next() % stattotal;
        for ( var i = 0; i < monsters.Count; i++ ) {
            stattotal -= monsters[ i ].monster.stat_total;
            promoted = monsters[ i ];
            if ( stattotal < 0 ) { break; }
        }

        promoted.LearnTechnique();
        promoted._monsterRank = newPool._monsterRank;

        MonsterRemove( promoted );
        newPool.MonsterAdd( promoted );
    }

    public void GenerateNewValidMonster(List<MonsterGenus> available)
    {
        _logger.WriteLineAsync( "[ABD Tournaments] TP: Generating", Color.AliceBlue );
        byte[] nmraw = [ 181, 0, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 215, 0, 72, 1, 228, 0, 233, 0, 184, 0, 216, 0, 50, 50, 50, 0, 255, 104, 9, 0, 3, 13, 0, 0, 65, 0, 0, 0, 0, 0, 255, 255 ]; // This doesn't matter, it gets completely overwritten below anyways.
        TournamentMonster nm = new TournamentMonster(nmraw);

        // TODO: Smarter Logic for deciding which breed to make the monster!
        // // // // // // 

        _logger.WriteLineAsync( "[ABD Tournaments] TP: Getting Breed", Color.AliceBlue );
        MonsterBreed randomBreed = MonsterBreed.AllBreeds[ 0 ];
        for ( var i = 0; i < 100; i++ ) {
            randomBreed = MonsterBreed.AllBreeds[ Random.Shared.Next() % MonsterBreed.AllBreeds.Count ];
            if ( available.Contains( randomBreed.breed_id ) && available.Contains( randomBreed.sub_id ) ) { 
                _logger.WriteLineAsync( "[ABD Tournaments]: Found valid breed. " + randomBreed.breed_id + "/" + randomBreed.sub_id ); 
                break; }
        }
        nm.breed_main = randomBreed.breed_id; nm.breed_sub = randomBreed.sub_id;
        _logger.WriteLineAsync( "TP: Breed " + nm.breed_main + " " + nm.breed_sub, Color.AliceBlue );
        // // // // // //

        ABD_TournamentMonster abdm = new ABD_TournamentMonster(nm);

        abdm.monster.name = ABD_TournamentMonster.random_name_list[ Random.Shared.Next() % ABD_TournamentMonster.random_name_list.Length ];
        abdm.monster.stat_lif = 100;
        abdm.monster.stat_pow = 80;
        abdm.monster.stat_ski = 100;
        abdm.monster.stat_spd = 100;
        abdm.monster.stat_def = 80;
        abdm.monster.stat_int = 80;

        abdm.monster.per_nature = (byte) ( Random.Shared.Next() % 255 );
        abdm.monster.per_fear = (byte) ( Random.Shared.Next() % 100 );
        abdm.monster.per_spoil = (byte) ( Random.Shared.Next() % 100 );
        
        abdm.monster.arena_movespeed = (byte) ( Random.Shared.Next() % 4 ); // TODO: Where does this come from?
        abdm.monster.arena_gutsrate = (byte) ( 7 + Random.Shared.Next() % 14 ); // 7 - 20?

        abdm.monster.battle_specials = 0;

        abdm.monster.techniques = abdm.breedInfo.technique_basics;

        _logger.WriteLineAsync( "[ABD Tournaments] TP: Basics Setup", Color.AliceBlue );
        while ( abdm.monster.stat_total < stat_start ) {
            abdm.AdvanceMonth();
        }
        _logger.WriteLineAsync( "[ABD Tournaments] TP: Advancing", Color.AliceBlue );
        for ( int i = 0; i < tournament_tier; i++ ) {
            abdm.LearnTechnique();
        }
        _logger.WriteLineAsync( "[ABD Tournaments] TP: Techs", Color.AliceBlue );
        if ( !abdm.alive ) {
            abdm.alive = true;
            abdm.lifespan += 6;
        }

        abdm._monsterRank = _monsterRank;

        _logger.WriteLineAsync( "[ABD Tournaments] TP: Cleanup", Color.AliceBlue );
        tournamentData.monsters.Add(abdm);
        this.MonsterAdd(abdm);
    }
}


public class ABD_TournamentMonster
{

    public static string[] random_name_list = ["Cimasio", "Kyrades", "Ambroros", "Teodeus", "Lazan", "Pegetus", "Perseos", "Asandrou", "Agametrios", "Lazion", "Morphosyne", "Gelantinos", "Narkelous", "Taloclus", "Baltsalus", "Hypnaeon", "Atrol", "Alexede", "Baccinos", "Idastos", "Ophyroe","Larissa","Asperata",
        "Alnifolia","Dentala","Celsa","Hempera","Laurel","Haldiphe","Saffronea", "Quinn",
"Poplarbush","Snowdrop","Funnyfluff","Firo","Limespice","Herb","Twinklespa","Spring","Shinyglade","Almond","Foggytree","Pecan","Jesterfeet","Skylark","Rainbowhor","Snow","Oakswamp","Liri", "Briarpuff",];

    public TournamentMonster monster;
    public MonsterBreed breedInfo;

    public List<TournamentPool> pools = new List<TournamentPool>();

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
        //_logger.WriteLineAsync( "[ABD Tournaments]: Creating monster from game data.", Color.Orange );
        monster = m;
        breedInfo = MonsterBreed.GetBreedInfo( monster.breed_main, monster.breed_sub );

        lifetotal = (ushort) ( 30 + TournamentData.LifespanRNG.Next() % 31 ); // 30-60
        lifespan = lifetotal;
        growth_rate = (ushort) ( 6 + TournamentData.GrowthRNG.Next() % 11 ); // 6-16
        growth_group = (growth_groups) ( TournamentData.GrowthRNG.Next() % 6 );


        // This section applies special bonuses to monster breeds. Dragons are in both groups, and a pure Dragon/Dragon gets +4/+2 to its growth rate. A Tiger/Gali would only get +0/+1 as Gali is only in one group and only the sub.
        List<MonsterGenus> bonuses = new List<MonsterGenus>(); List<MonsterGenus> bonuses2 = new List<MonsterGenus>();
        bonuses.AddRange( [MonsterGenus.Dragon, MonsterGenus.Centaur, MonsterGenus.Beaclon, MonsterGenus.Henger, MonsterGenus.Wracky, MonsterGenus.Durahan, MonsterGenus.Gali, MonsterGenus.Zilla, MonsterGenus.Bajarl, MonsterGenus.Phoenix, 
            MonsterGenus.Metalner, MonsterGenus.Jill, MonsterGenus.Joker, MonsterGenus.Undine, MonsterGenus.Mock, MonsterGenus.Unknown1, MonsterGenus.Unknown2, MonsterGenus.Unknown3, MonsterGenus.Unknown4, MonsterGenus.Unknown5, MonsterGenus.Unknown6] );

        bonuses2.AddRange( [MonsterGenus.Dragon, MonsterGenus.Centaur, MonsterGenus.Beaclon, MonsterGenus.Durahan, MonsterGenus.Zilla, MonsterGenus.Phoenix, MonsterGenus.Metalner, MonsterGenus.Joker, MonsterGenus.Undine,
            MonsterGenus.Unknown1, MonsterGenus.Unknown2, MonsterGenus.Unknown3, MonsterGenus.Unknown4, MonsterGenus.Unknown5, MonsterGenus.Unknown6] );

        if ( bonuses.Contains( (MonsterGenus) monster.breed_main) ) { growth_rate += 2; }
        if ( bonuses2.Contains( (MonsterGenus) monster.breed_main ) ) { growth_rate += 2; }
        if ( bonuses.Contains( (MonsterGenus) monster.breed_sub ) ) { growth_rate++; }
        if ( bonuses2.Contains( (MonsterGenus) monster.breed_sub ) ) { growth_rate++; }

        SetupGrowthOptions();
    }

    public ABD_TournamentMonster(byte[] rawabd) {
        _logger.WriteLineAsync( "[ABD Tournaments]: Loading monster from ABD Save File.", Color.Orange );
        byte[] rawmonster = new byte[ 60 ];
        for ( var i = 0; i < 60; i++ ) { rawmonster[ i ] = rawabd[ i + 40 ]; }

        monster = new TournamentMonster( rawmonster );
        breedInfo = MonsterBreed.GetBreedInfo( monster.breed_main, monster.breed_sub );

        lifetotal = rawabd[ 0 ];
        lifespan = rawabd[ 2 ];
        growth_rate = rawabd[ 4 ];
        growth_group = (growth_groups) rawabd[ 6 ];

        _monsterRank = (EMonsterRanks) rawabd[ 8 ];

        SetupGrowthOptions();
    }

    private void SetupGrowthOptions() {
        if ( growth_group == growth_groups.balanced ) { growth_options =    [ 0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5 ]; }
        else if ( growth_group == growth_groups.power ) { growth_options =  [ 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 4, 4, 4, 4, 5 ]; }
        else if ( growth_group == growth_groups.intel ) { growth_options =  [ 0, 0, 0, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 5, 5, 5, 5, 5, 5 ]; }
        else if ( growth_group == growth_groups.defend ) { growth_options = [ 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 2, 3, 4, 4, 4, 4, 4, 4, 5, 5 ]; }
        else if ( growth_group == growth_groups.wither ) { growth_options = [ 0, 0, 0, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 5, 5, 5, 5 ]; }
        else if ( growth_group == growth_groups.speedy ) { growth_options = [ 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 5 ]; }
    }

    public void AdvanceMonth()
    {

        _logger.WriteLineAsync( "[ABD Tournaments]: ABD Monster advancing month.", Color.Orange );

        int agegroup = 1;
        lifespan--;
        alive = lifespan > 0;

        if ( (lifetotal - lifespan) > 9) { agegroup++; }
        if ( ( lifetotal - lifespan ) > 18 ) { agegroup++; }
        if ( lifespan < 12 ) { agegroup--; }
        if ( lifespan < 6 ) { agegroup--; }

        agegroup *= growth_rate;
        // TODO: SPEED UP! REMOVE
        //agegroup += 50;

        for (var i = 0; i < agegroup; i++) {
            var stat = growth_options[TournamentData.GrowthRNG.Next() % growth_options.Length];
            if (stat == 0) { monster.stat_lif++; }
            else if (stat == 1) { monster.stat_pow++; }
            else if (stat == 2) { monster.stat_ski++; }
            else if (stat == 3) { monster.stat_spd++; }
            else if (stat == 4) { monster.stat_def++; }
            else if (stat == 5) { monster.stat_int++; }
        }
    }

    public void LearnTechnique() { // TODO: Smarter Logic About which tech to get
        _logger.WriteLineAsync( "[ABD Tournaments]: Monster attempting to learn technique.", Color.Orange );
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

        /*
        for ( var i = 0; i < 100; i++ ) {
            for ( var )
            var newTech = Random.Shared.Next() % 24;
            if ( breedInfo.technique_types[ newTech ] == 6 || ( ( monster.techniques >> newTech ) & 1 ) == 1 ) { } // Invalid or Already Learned

            else if ( breedInfo.technique_types[ newTech ] == 5 && ( _monsterRank == EMonsterRanks.S || _monsterRank == EMonsterRanks.A || _monsterRank == EMonsterRanks.B ) ) { // Special techs only for B Rank+ Monsters
                monster.techniques += (uint) ( 1 << newTech );
                i = 101;
            }

            else if ( breedInfo.technique_types[newTech ] <= 4 ) {
                monster.techniques += (uint) ( 1 << newTech );
                i = 101;
            }
        }*/
    }

    public byte[] ToSaveFile() {
        // ABD_TournamentMonster data will consist of 40 new bytes, followed by the standard 60 tracked by the game for Tournament Monsters.
        // 0-1, Lifetotal
        // 2-3, Current Remaining Lifespan
        // 4-5, Growth Rate Per Months
        // 6-7, Growth Group (Enum)
        // 8-9, Monster Rank (Enum)
        // 8-39, UNUSED

        byte[] data = new byte[ 40 + 60 ]; 

        data[ 0 ] = (byte) lifetotal;
        data[ 2 ] = (byte) lifespan;
        data[ 4 ] = (byte) growth_rate;
        data[ 6 ] = (byte) growth_group;
        data[ 8 ] = (byte) _monsterRank;

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

