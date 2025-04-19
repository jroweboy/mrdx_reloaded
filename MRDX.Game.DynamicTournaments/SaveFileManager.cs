using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRDX.Base.Mod.Interfaces;
using Reloaded.Mod.Interfaces;

namespace MRDX.Game.DynamicTournaments
{
    class SaveFileManager
    {
        Mod _modMaster;
        private readonly IModLoader _modLoader;
        private readonly ILogger _logger;
        private readonly IModConfig _modConfig;

        private string _saveData_slot = "";
        public byte _saveData_readCount = 0;
        public bool _saveData_gameLoaded = false;

        private string _saveData_folder = "";
        public bool _saveData_autosaves_enabled = false;

        public SaveFileManager(Mod master, IModLoader loader, IModConfig config, ILogger logger, bool autosavesEnabled) {
            _modMaster = master;
            _modLoader = loader;
            _modConfig = config;
            _logger = logger;

            _saveData_autosaves_enabled = autosavesEnabled;

            _saveData_folder = _modLoader.GetDirectoryForModId( _modConfig.ModId ) + "\\SaveData\\";
        }

        /// <summary>
        /// Tracks the files read by the game. There is a file load/hook signature we can use until we find the save function/load functions to duplicate this effect.
        /// 3+ Access followed by a psdata access is for saving.
        /// 3+ Access followed by a GameEventHook is for loading.
        /// </summary>
        public void SaveDataMonitor ( string filename ) {
            if ( filename.Contains( "BISLPS-" ) && ( !filename.Contains("614") || _saveData_autosaves_enabled ) ) {
                var newSlot = filename.Substring( filename.Length - 2 );

                if ( _saveData_slot != newSlot ) {
                    _saveData_slot = newSlot;
                    _saveData_readCount = 0;
                }
                _saveData_readCount++;

                if ( _saveData_readCount >= 4 ) {
                    _saveData_gameLoaded = true;
                }

                _saveData_folder = filename.Substring(0, filename.Length - 14 );
            }

            else if ( filename.Contains( "psdata001.bin" ) ) {
                if ( _saveData_readCount > 3 ) {
                    SaveABDTournamentData();
                    _saveData_gameLoaded = false;
                }
            }

            else {
                _saveData_readCount = 0;
                _saveData_gameLoaded = false;
            }

            //_logger.WriteLine( $"CUSTOM MONITOR: {filename}", Color.Blue );
        }

        public void SaveABDTournamentData () {
            string modDir = _modLoader.GetDirectoryForModId( _modConfig.ModId );
            Directory.CreateDirectory( modDir + "\\SaveData\\" );

            string savefile = modDir + "\\SaveData\\dtp_monsters_" + _saveData_slot + ".bin";

            savefile = _saveData_folder + "dtp_monsters_" + _saveData_slot + ".bin";

            FileStream fs = new FileStream( savefile, FileMode.Create );
            for ( var i = 0; i < _modMaster.tournamentData.monsters.Count; i++ ) {
                ABD_TournamentMonster monster = _modMaster.tournamentData.monsters[ i ];
                fs.Write( monster.ToSaveFile() );
            }
            fs.Close();
            _logger.WriteLine( savefile + " successfully written.", Color.OrangeRed );
        }


        public List<ABD_TournamentMonster> LoadABDTournamentData() {
            List<ABD_TournamentMonster> monsters = new List<ABD_TournamentMonster>();

            string modDir = _modLoader.GetDirectoryForModId( _modConfig.ModId );
            string savefile = modDir + "\\SaveData\\dtp_monsters_" + _saveData_slot + ".bin";
            savefile = _saveData_folder + "dtp_monsters_" + _saveData_slot + ".bin";

            if ( File.Exists( savefile ) ) {
                byte[] rawabd = new byte[ 100 ];
                
                FileStream fs = new FileStream( savefile, FileMode.Open );
                long remaining = fs.Length / 100;
                while (remaining > 0) {
                    remaining--;

                    fs.ReadExactly( rawabd, 0, 100 );
                    monsters.Add( new ABD_TournamentMonster( rawabd ) );
                }
                fs.Close();
            }

            _saveData_readCount = 0;
            _saveData_gameLoaded = false;

            return monsters;
        }
    }
}
