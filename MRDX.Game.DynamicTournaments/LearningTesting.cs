using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRDX.Base.Mod.Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using System.Diagnostics;
using System.Drawing;

[HookDef( BaseGame.Mr2, Region.Us,
"55 8B EC 8B 45 ?? 56 8B F1 3D E7 03 00 00 77 ?? 8B 56 ?? 66 8B 52 ?? 66 89 15 ?? ?? ?? ?? 66 8B D0 8B 4E ?? 66 2B 51 ?? 66 89 15 ?? ?? ?? ?? 8B 4E ?? 5E 66 89 41 ?? 5D C2 04 00 33 C0 5E 5D C2 04 00" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate uint LH_StatGain_Lif ( nuint self, uint value );

[HookDef( BaseGame.Mr2, Region.Us,
"55 8B EC 8B 45 ?? 56 8B F1 3D E7 03 00 00 77 ?? 8B 56 ?? 66 8B 52 0E" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate uint LH_StatGain_Pow ( nuint self, uint value );

[HookDef( BaseGame.Mr2, Region.Us,
"55 8B EC 8B 45 ?? 56 8B F1 3D E7 03 00 00 77 ?? 8B 56 ?? 66 8B 52 10" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate uint LH_StatGain_Def ( nuint self, uint value );

[HookDef( BaseGame.Mr2, Region.Us,
"55 8B EC 8B 45 ?? 56 8B F1 3D E7 03 00 00 77 ?? 8B 56 ?? 66 8B 52 12" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate uint LH_StatGain_Ski ( nuint self, uint value );

[HookDef( BaseGame.Mr2, Region.Us,
"55 8B EC 8B 45 ?? 56 8B F1 3D E7 03 00 00 77 ?? 8B 56 ?? 66 8B 52 14" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate uint LH_StatGain_Spd ( nuint self, uint value );

[HookDef( BaseGame.Mr2, Region.Us,
"55 8B EC 8B 45 ?? 56 8B F1 3D E7 03 00 00 77 ?? 8B 56 ?? 66 8B 52 16" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate uint LH_StatGain_Int ( nuint self, uint value );


[HookDef( BaseGame.Mr2, Region.Us, "55 8B EC 81 EC 80 00 00 00" )]
[Function( CallingConventions.Cdecl )]
public delegate void LH_TournamentComplete ( int value );

[HookDef( BaseGame.Mr2, Region.Us, "55 8B EC 83 EC 18 8B D1" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate void LH_DrillPerform_ChangeStats ( nuint self, nuint value );

[HookDef( BaseGame.Mr2, Region.Us, "55 8B EC 81 EC D0 00 00 00 A1 ?? ?? ?? ?? 33 C5 89 45 ?? A1 ?? ?? ?? ?? 53" )]
[Function( CallingConventions.Cdecl )]
public delegate void LH_TournamentLifeIndex ( nuint self ); // This function isn't right.


namespace MRDX.Game.DynamicTournaments
{

    class LearningTesting
    {
        private readonly IHooks? _hooks;

        private IHook<UpdateGenericState>? _hookGenericUpdate;
        private IHook<SetupCCtrlBattle>? _hookBattle;

        private IHook<LH_StatGain_Lif>? _statGainLif;
        private IHook<LH_StatGain_Pow>? _statGainPow;
        private IHook<LH_StatGain_Def>? _statGainDef;
        private IHook<LH_StatGain_Ski>? _statGainSki;
        private IHook<LH_StatGain_Spd>? _statGainSpd;
        private IHook<LH_StatGain_Int>? _statGainInt;
        private IHook<LH_TournamentComplete> _LHTournamentComplete;
        private IHook<LH_DrillPerform_ChangeStats> _LHDPCS;

        private nuint _address_currentweek;
        private uint _currentWeek;

        public bool _tournamentEntered = false;
        public int _tournamentStatBonus = 5; // A Number between 0 and X, 0-X is added to the stat gains.

        public LearningTesting ( IHooks hooks, nuint currentWeekAddress ) {
            _address_currentweek = currentWeekAddress;

            _hooks = hooks;

            _hooks.AddHook<UpdateGenericState>( SetupGenericUpdateHook )
            .ContinueWith( result => _hookGenericUpdate = result.Result.Activate() );

            _hooks.AddHook<SetupCCtrlBattle>( SetupCCtrlBattleHook )
                .ContinueWith( result => _hookBattle = result.Result.Activate() );



            _hooks.AddHook<LH_StatGain_Lif>( SetupLHStatGainLifHook )
                .ContinueWith( result => _statGainLif = result.Result.Activate() );

            _hooks.AddHook<LH_StatGain_Pow>( SetupLHStatGainPowHook )
                .ContinueWith( result => _statGainPow = result.Result.Activate() );

            _hooks.AddHook<LH_StatGain_Def>( SetupLHStatGainDefHook )
                .ContinueWith( result => _statGainDef = result.Result.Activate() );

            _hooks.AddHook<LH_StatGain_Ski>( SetupLHStatGainSkiHook )
                .ContinueWith( result => _statGainSki = result.Result.Activate() );

            _hooks.AddHook<LH_StatGain_Spd>( SetupLHStatGainSpdHook )
                .ContinueWith( result => _statGainSpd = result.Result.Activate() );

            _hooks.AddHook<LH_StatGain_Int>( SetupLHStatGainIntHook )
                .ContinueWith( result => _statGainInt = result.Result.Activate() );

            //_hooks.AddHook<LH_TournamentComplete>( SetupLHTournamentComplete ).ContinueWith( result => _LHTournamentComplete = result.Result.Activate() );

            /*_hooks.AddHook<LH_DrillPerform_ChangeStats>( SetupLHDPCS )
                .ContinueWith( result => _LHDPCS = result.Result.Activate() );*/
        }

        /// <summary>
        /// Called regularly from many places.
        /// This function keeps track of the current week, and if the week progresses forward, resets the tournamentEntered flag.
        /// The forward direction is required as for some reason, at the end of tournaments, the current date is rewound one week.
        /// </summary>
        private void SetupGenericUpdateHook ( nint parent ) {
            _hookGenericUpdate!.OriginalFunction( parent );
            
            Memory.Instance.Read<uint>( _address_currentweek, out uint currentWeek );

            if ( _currentWeek < currentWeek || _currentWeek == 0 ) { 
                _currentWeek = currentWeek; _tournamentEntered = false;
            }
            
        }

        /// <summary>
        /// This function is called when a battle is intially setup.
        /// At this point, it is guaranteed that the player is participating in a tournament and sets the tournamentEntered flag.
        /// </summary>
        /// <param name="parent"></param>
        private void SetupCCtrlBattleHook ( nuint parent ) {
            Debug.WriteLine( "Battle TEnt: " + _tournamentEntered );
            _hookBattle!.OriginalFunction( parent );

            _tournamentEntered = true;
        }

        // These functions all attempt to add an additional 0-TSB for all stat gains. Only called if the tournamentEntered flag is set.
        private uint SetupLHStatGainLifHook ( nuint self, uint value ) {
            Debug.WriteLine( "Lif Gain Hook: " + self + ", Val: " + value + ", TEnt: " + _tournamentEntered );
            if ( _tournamentEntered && _tournamentStatBonus > 0 ) { value = (uint) (value + (Random.Shared.Next() % _tournamentStatBonus) ); }
            var ret = _statGainLif!.OriginalFunction( self, value );
            return ret;
        }

        private uint SetupLHStatGainPowHook ( nuint self, uint value ) {
            Debug.WriteLine( "Pow Gain Hook: " + self + ", Val: " + value + ", TEnt: " + _tournamentEntered );
            if ( _tournamentEntered && _tournamentStatBonus > 0 ) { value = (uint) ( value + ( Random.Shared.Next() % _tournamentStatBonus ) ); }
            var ret = _statGainPow!.OriginalFunction( self, value );
            return ret;
        }

        private uint SetupLHStatGainDefHook ( nuint self, uint value ) {
            Debug.WriteLine( "Def Gain Hook: " + self + ", Val: " + value + ", TEnt: " + _tournamentEntered );
            if ( _tournamentEntered && _tournamentStatBonus > 0 ) { value = (uint) ( value + ( Random.Shared.Next() % _tournamentStatBonus ) ); }
            var ret = _statGainDef!.OriginalFunction( self, value );
            return ret;
        }

        private uint SetupLHStatGainSkiHook ( nuint self, uint value ) {
            Debug.WriteLine( "Ski Gain Hook: " + self + ", Val: " + value + ", TEnt: " + _tournamentEntered );
            if ( _tournamentEntered && _tournamentStatBonus > 0 ) { value = (uint) ( value + ( Random.Shared.Next() % _tournamentStatBonus ) ); }
            var ret = _statGainSki!.OriginalFunction( self, value );
            return ret;
        }

        private uint SetupLHStatGainSpdHook ( nuint self, uint value ) {
            Debug.WriteLine( "Spd Gain Hook: " + self + ", Val: " + value + ", TEnt: " + _tournamentEntered );
            if ( _tournamentEntered && _tournamentStatBonus > 0 ) { value = (uint) ( value + ( Random.Shared.Next() % _tournamentStatBonus ) ); }
            var ret = _statGainSpd!.OriginalFunction( self, value );
            return ret;
        }

        private uint SetupLHStatGainIntHook ( nuint self, uint value ) {
            Debug.WriteLine( "Int Gain Hook: " + self + ", Val: " + value + ", TEnt: " + _tournamentEntered );
            if ( _tournamentEntered && _tournamentStatBonus > 0 ) { value = (uint) ( value + ( Random.Shared.Next() % _tournamentStatBonus ) ); }
            var ret = _statGainInt!.OriginalFunction( self, value );
            return ret;
        }


        private void SetupLHTournamentComplete ( int value ) {
            Debug.WriteLine( "Tournament Complete Val: " + value );
            _LHTournamentComplete!.OriginalFunction( value );
        }

        private void SetupLHDPCS ( nuint self, nuint value ) {
            nuint nval = (nuint)  (1 + Random.Shared.Next() % 3);
            Debug.WriteLine( "Drill Stat?: " + value + " " + nval );
            
            _LHDPCS!.OriginalFunction( self, nval );
        }
    }
}

//47
//83 ff 03
//0f 8c 61 fe ff ff
