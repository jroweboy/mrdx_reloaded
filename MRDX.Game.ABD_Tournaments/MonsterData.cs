using System;
using System.Drawing;
using MRDX.Base.Mod.Interfaces;
using Reloaded.Mod.Interfaces;

public class MonsterBreed
{
    public static List<MonsterBreed> AllBreeds = new List<MonsterBreed>();

    public string name;
    public string name_short;

    public byte breed_id;
    public byte sub_id;
    public string breed_identifier;

    public byte[] technique_types;
    public uint technique_basics;



    public MonsterBreed (string n, string nshort, byte bid, byte sid, string identifier, byte[] techniques) {
        /*
        name = IMonster.AllMonsters[ id ].Name;
        name_short = IMonster.AllMonsters[ id ].ShortName;
        breed_id = id;
        breed_identifier = name_short.Substring( 0, 2 );

        if ( breed_identifier == "un" ) {
            if ( name_short.EndsWith("1") ) { breed_identifier = "xx"; }
            else if ( name_short.EndsWith( "2" ) ) { breed_identifier = "xy"; }
            else if ( name_short.EndsWith( "3" ) ) { breed_identifier = "xz"; }
            else if ( name_short.EndsWith( "4" ) ) { breed_identifier = "yx"; }
            else if ( name_short.EndsWith( "5" ) ) { breed_identifier = "yy"; }
            else { breed_identifier = "zz"; }
        }*/

        name = n;
        name_short = nshort;

        breed_id = bid;
        sub_id = sid;
        breed_identifier = identifier;

        technique_types = new byte[ 24 ];
        for ( var t = 0; t < 24; t++ ) {
            technique_types[ t ]  = techniques[ t ];
            if ( techniques[ t ] == 0 ) {
                technique_basics += (uint) (1 << ( t ));
            }
        }
    }

    public static void SetupMonsterBreedList ( string gamePath ) {
        for ( var i = 0; i < IMonster.AllMonsters.Count(); i++ ) {
            MonsterInfo info = IMonster.AllMonsters[ i ];
            MonsterInfo sinfo = IMonster.AllMonsters[ i ];
            if ( !info.Name.StartsWith( "Unknown" ) ) {

                string breedFolder = gamePath + "\\mf2\\data\\mon\\" + info.ShortName + "\\";
                var textureFiles = Directory.EnumerateFiles( breedFolder, "??_??.tex" );
                var techniqueFile = breedFolder + info.ShortName.Substring( 0, 2 ) + "_" + info.ShortName.Substring( 0, 2 ) + "_wz.bin";

                // Build a singular tech list. This will be the same for every breed until I do the right now and actually check errantry (no thanks :( )
                FileStream fs = File.OpenRead( techniqueFile );
                byte [] techniqueList = ParseTechniqueFile( fs );

                // Enumerate through each species tex file and generate the final breeds.
                foreach ( string currentFile in textureFiles ) {
                    var m_identifier = currentFile.Substring( currentFile.Length - 9, 2 );
                    var sub_identifier = currentFile.Substring( currentFile.Length - 6, 2 );
                    var sub_id = i;

                    // Compares Sub Information to Known Monsters - Have to do some shenanigans to take care of the unknown ??? species in the database.
                    for ( var j = 0; j < IMonster.AllMonsters.Count(); j++ ) {
                        sinfo = IMonster.AllMonsters[ j ];

                        var infoCheck = sinfo.ShortName.Substring( 0, 2 );

                        if ( sinfo.ShortName.EndsWith( "1" ) ) { infoCheck = "xx"; }
                        else if ( sinfo.ShortName.EndsWith( "2" ) ) { infoCheck = "xy"; }
                        else if ( sinfo.ShortName.EndsWith( "3" ) ) { infoCheck = "xz"; }
                        else if ( sinfo.ShortName.EndsWith( "4" ) ) { infoCheck = "yx"; }
                        else if ( sinfo.ShortName.EndsWith( "5" ) ) { infoCheck = "yy"; }
                        else if ( sinfo.ShortName.EndsWith( "6" ) ) { infoCheck = "yz"; }

                        if ( infoCheck == sub_identifier ) {
                            sub_id = j;
                            break;
                        }
                    }
                    MonsterBreed newBreed = new MonsterBreed( info.Name + "/" + sinfo.Name, info.ShortName, (byte) i, (byte) sub_id, m_identifier + "_" + sub_identifier, techniqueList );
                    MonsterBreed.AllBreeds.Add( newBreed );
                }
            }

        }


    }

    /// <summary> Reads from the provided FileStream and returns a byte list. 0 = Basic, 1 Hit, 2 Heavy, 3 Withering, 4 Sharp, 5 Special, 6 Invalid </summary>
    private static byte [] ParseTechniqueFile(FileStream fs) {
        byte[] techniqueList = new byte[ 24 ];
        long tpos = 0;
        
        for ( var i = 0; i < 24; i++ ) {
            fs.Position = i * 4; 
            tpos = (long) fs.ReadByte(); tpos += (long) fs.ReadByte() * 256;

            if ( tpos == 0xFFFF ) { techniqueList[ i ] = 6; }
            else {
                fs.Position = (long) tpos + 0x10;
                techniqueList[ i ] = (byte) fs.ReadByte();
            }
        }
        fs.Close();
        return techniqueList;
    }

    public static MonsterBreed GetBreedInfo(byte main, byte sub) {
        for ( var i = 0; i < MonsterBreed.AllBreeds.Count; i++ ) {
            var mb = MonsterBreed.AllBreeds[ i ];
            if ( mb.breed_id == main && mb.sub_id == sub ) { return mb; }
        }
        return MonsterBreed.AllBreeds[ 0 ];
    }

    public static void SetupValidSubBreeds ( string gamePath ) {
        /*
        foreach ( MonsterBreed breed in AllBreeds ) {
            if ( !breed.name.StartsWith( "Unknown" ) ) {
                var textureFiles = Directory.EnumerateFiles( gamePath + "\\mf2\\data\\mon\\" + breed.name_short + "\\", breed.breed_identifier + "_??.tex" );
                foreach ( string currentFile in textureFiles ) {
                    var sub_identifier = currentFile.Substring( currentFile.Length - 6, 2 );

                    foreach ( MonsterBreed sub in AllBreeds ) {
                        if ( sub.breed_identifier == sub_identifier ) {
                            breed.breed_valid_subs = breed.breed_valid_subs + (ulong) ( 1 << sub.breed_id );
                        }
                    }
                }
            }
        }*/
    }
}

public class TournamentMonster {
    public byte[] raw_bytes = new byte[ 60 ];
    #region Raw Variables
    private string _name;
    private byte _breed_main;
    private byte _breed_sub;
    private ushort _stat_lif;
    private ushort _stat_pow;
    private ushort _stat_def;
    private ushort _stat_ski;
    private ushort _stat_spd;
    private ushort _stat_int;

    private byte _per_nature;
    private byte _per_fear;
    private byte _per_spoil;
    private uint _techniques;
    private byte _arena_movespeed;
    private byte _arena_gutsrate;
    private ushort _battle_specials;
    #endregion

    // 0 - 25 : Monster Name - MUST END IN FF00 - (12 Characters Long + FF00)
    public string name {
        get => _name; set {
            _name = value.Substring( 0, Math.Min( value.Length, 12 ) );
            var b = Mr2StringExtension.AsBytes( Mr2StringExtension.AsMr2( value ) );
            for ( var i = 0; i < 26; i++ ) {
                raw_bytes[ i ] = (byte) ( i < b.Length ? b[ i ] : 0 );
            }
        }
    }

    // 26, 27 : Monster Species / Subspecies
    public byte breed_main { get => _breed_main; set { _breed_main = value; raw_bytes[ 26 ] = value; } }
    public byte breed_sub { get => _breed_sub; set { _breed_sub = value; raw_bytes[ 27 ] = value; } }

    // 2 Bytes Per Stat, 28-39, Life-Pow-Def-Ski-Spd-Int
    public ushort stat_lif { get => _stat_lif; set { _stat_lif = value; raw_bytes[ 28 ] = (byte) ( value & 0xff ); raw_bytes[ 29 ] = (byte) ( ( value & 0xff00 ) >> 8 ); } }
    public ushort stat_pow { get => _stat_pow; set { _stat_pow = value; raw_bytes[ 30 ] = (byte) ( value & 0xff ); raw_bytes[ 31 ] = (byte) ( ( value & 0xff00 ) >> 8 ); } }
    public ushort stat_def { get => _stat_def; set { _stat_def = value; raw_bytes[ 32 ] = (byte) ( value & 0xff ); raw_bytes[ 33 ] = (byte) ( ( value & 0xff00 ) >> 8 ); } }
    public ushort stat_ski { get => _stat_ski; set { _stat_ski = value; raw_bytes[ 34 ] = (byte) ( value & 0xff ); raw_bytes[ 35 ] = (byte) ( ( value & 0xff00 ) >> 8 ); } }
    public ushort stat_spd { get => _stat_spd; set { _stat_spd = value; raw_bytes[ 36 ] = (byte) ( value & 0xff ); raw_bytes[ 37 ] = (byte) ( ( value & 0xff00 ) >> 8 ); } }
    public ushort stat_int { get => _stat_int; set { _stat_int = value; raw_bytes[ 38 ] = (byte) ( value & 0xff ); raw_bytes[ 39 ] = (byte) ( ( value & 0xff00 ) >> 8 ); } }

    public int stat_total { get { return _stat_lif + _stat_pow + _stat_ski + _stat_spd + _stat_def + _stat_int; } }

    public byte per_nature { get => _per_nature; set { _per_nature = value; raw_bytes[ 40 ] = value; } }
    public byte per_fear { get => _per_fear; set { _per_fear = value; raw_bytes[ 41 ] = value; } }
    public byte per_spoil { get => _per_spoil; set { _per_spoil = value; raw_bytes[ 42 ] = value; } }

    // 43-46 Techniques, Represented by Bits (First Byte is always empty)
    public uint techniques { get => _techniques; set { _techniques = value; raw_bytes[ 43 ] = 0; raw_bytes[ 44 ] = (byte) ( ( value & 0xff0000 ) >> 16 ); raw_bytes[ 45 ] = (byte) ( ( value & 0xff00 ) >> 8 ); raw_bytes[ 46 ] = (byte) ( value & 0xff ); } }

    public byte arena_movespeed { get => _arena_movespeed; set { _arena_movespeed = value; raw_bytes[ 48 ] = value; } }
    public byte arena_gutsrate { get => _arena_gutsrate; set { _arena_gutsrate = value; raw_bytes[ 49 ] = value; } }

    // 52-53 Battle Specials, Represented by Bits (Only Last 13 Bits Used | 0001111111111111 )
    public ushort battle_specials { get => _battle_specials; set { _battle_specials = value; raw_bytes[ 52 ] = (byte) ( ( value & 0xff00 ) >> 8 ); raw_bytes[ 53 ] = (byte) ( value & 0xff ); } }

    public override string ToString () {
        return name + ": " +
            "[LIF:" + stat_lif + ", POW:" + stat_pow + ", SKI:" + stat_ski + ", SPD:" + stat_spd + ", DEF:" + stat_def + ", INT:" + stat_int + "], " +
            "[NAT:" + per_nature + ", FEAR:" + per_fear + ", SPOIL:" + per_spoil + "], " +
            "[TECHS:" + techniques + ", MOV:" + arena_movespeed + ", GUTS:" + arena_gutsrate + ", BATSPC:" + battle_specials + "]";
    }
    public TournamentMonster ( byte[] raw ) {
        for ( var i = 0; i < 60; i++ ) raw_bytes[ i ] = 0;
        parseBytesToMonster( raw );
    }


    private void parseBytesToMonster ( byte[] raw ) {

        int lc = 1;
        for ( var i = 0; i < raw.Length; i++ ) {
            if ( raw[ i ] == 255 ) { lc = i + 1; i = 9999; }
        }

        byte[] rawname = new byte[ lc ];
        for ( var i = 0; i < lc; i++ ) {
            rawname[ i ] = raw[ i ];
        }

        name = Mr2StringExtension.AsString( Mr2StringExtension.AsShorts( rawname ) );

        breed_main = raw[ 26 ];
        breed_sub = raw[ 27 ];

        stat_lif = (ushort) ( raw[ 28 ] + ( raw[ 29 ] << 8 ) );
        stat_pow = (ushort) ( raw[ 30 ] + ( raw[ 31 ] << 8 ) );
        stat_def = (ushort) ( raw[ 32 ] + ( raw[ 33 ] << 8 ) );
        stat_ski = (ushort) ( raw[ 34 ] + ( raw[ 35 ] << 8 ) );
        stat_spd = (ushort) ( raw[ 36 ] + ( raw[ 37 ] << 8 ) );
        stat_int = (ushort) ( raw[ 38 ] + ( raw[ 39 ] << 8 ) );

        per_nature = raw[ 40 ];
        per_fear = raw[ 41 ];
        per_spoil = raw[ 42 ];

        techniques = (uint) ( ( raw[ 44 ] << 16 ) + ( raw[ 45 ] << 8 ) + raw[ 46 ] );

        arena_movespeed = raw[ 48 ];
        arena_gutsrate = raw[ 49 ];

        battle_specials = (ushort) ( ( raw[ 52 ] << 8 ) + raw[ 53 ] );

        raw_bytes[ 58 ] = 0xFF;
        raw_bytes[ 59 ] = 0xFF;

    }


}