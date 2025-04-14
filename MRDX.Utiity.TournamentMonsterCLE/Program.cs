using System;
using System.IO;
using System.Text;
using MRDX.Base.Mod;
using MRDX.Base.Mod.Interfaces;

static string ToHexChar(ushort hex)
{
    if ( hex <= 9 ) { return hex.ToString(); }
    else if ( hex == 10 ) { return "A"; }
    else if (hex == 11) { return "B"; }
    else if (hex == 12) { return "C"; }
    else if (hex == 13) { return "D"; }
    else if (hex == 14) { return "E"; }
    else if (hex == 15) { return "F"; }
    return "X";
}

string monsterName = "Oakleyman";
ushort[] name = Mr2StringExtension.AsMr2(monsterName);
string fullNameHex = "";

    Console.WriteLine("START - " + monsterName);
for ( int i = 0; i < name.Length && i < 12; i++ )
{
    fullNameHex += ToHexChar((ushort)(name[i] / 4096 % 16));
    fullNameHex += ToHexChar((ushort)(name[i] / 256 % 16));
    fullNameHex += ToHexChar((ushort)(name[i] / 16 % 16));
    fullNameHex += ToHexChar((ushort)(name[i] % 16));
    fullNameHex += " ";
}

fullNameHex += "FF00 ";

for ( int i = name.Length; i < 12; i++ )
{
    fullNameHex += "0000 ";
}

Console.WriteLine(fullNameHex);
Console.WriteLine("END");

static void parseTest()
{
    Console.WriteLine("Path? " + Directory.GetCurrentDirectory());
    /*using (FileStream fs = File.Open(path, FileMode.Open))
    {
        fs.Position = 0xA0C;
        fs.Write(info, 0, info.Length);
    }*/


    string test = "B5 0E B5 1A B5 24 B5 25 B5 1E B5 32 B5 26 B5 1A B5 27 FF 00 00 00 00 00 00 00 1B 1B C7 00 9E 00 51 00 37 00 93 00 17 00 19 14 14 00 81 02 00 00 01 0E 00 00 13 00 00 00 00 00 FF FF";
    test = "B5 13 B5 21 B5 22 B5 2C B5 61 B5 1F B5 25 B5 1A B5 26 B5 22 B5 27 B5 20 B5 61 B5 1F B5 1E B5 1A B5 2D B5 21 B5 1E B5 2B B5 61 B5 22 B5 2C B5 61 B5 2D B5 28 B5 28";
    test = test.Replace(" ", string.Empty);
    char[] monsterData = test.ToCharArray();

    string output = "Moster Name From Parse: ";
    

    for ( var i = 0; i < test.Length; i+=4 )
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

parseTest();
//IMonster.AllMonsters[0]


