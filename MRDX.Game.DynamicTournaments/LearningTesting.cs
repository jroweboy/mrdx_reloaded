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

[HookDef( BaseGame.Mr2, Region.Us,
"55 8B EC 8B 45 ?? 56 8B F1 3D E7 03 00 00 77 ?? 8B 56 ?? 66 8B 52 ?? 66 89 15 ?? ?? ?? ?? 66 8B D0 8B 4E ?? 66 2B 51 ?? 66 89 15 ?? ?? ?? ?? 8B 4E ?? 5E 66 89 41 ?? 5D C2 04 00 33 C0 5E 5D C2 04 00" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate uint LH_StatGain_Lif ( nuint self, uint value );


[HookDef( BaseGame.Mr2, Region.Us, "55 8B EC 81 EC 80 00 00 00" )]
[Function( CallingConventions.Stdcall )]
public delegate void LH_TournamentComplete ( int value ); // This function isn't right.

[HookDef( BaseGame.Mr2, Region.Us, "55 8B EC 83 EC 18 8B D1" )]
[Function( CallingConventions.MicrosoftThiscall )]
public delegate void LH_DrillPerform_ChangeStats ( nuint self, nuint value );

[HookDef( BaseGame.Mr2, Region.Us, "55 8B EC 81 EC D0 00 00 00 A1 ?? ?? ?? ?? 33 C5 89 45 ?? A1 ?? ?? ?? ?? 53" )]
[Function( CallingConventions.Stdcall )]
public delegate void LH_TournamentLifeIndex ( nuint self ); // This function isn't right.


namespace MRDX.Game.DynamicTournaments
{

    class LearningTesting
    {
        private readonly IHooks? _hooks;
        private IHook<LH_StatGain_Lif>? _statGainLif;
        private IHook<LH_TournamentComplete> _LHTournamentComplete;
        private IHook<LH_DrillPerform_ChangeStats> _LHDPCS;

        public LearningTesting ( IHooks hooks ) {
            _hooks = hooks;
            _hooks.AddHook<LH_StatGain_Lif>( SetupLHStatGainLifHook )
                .ContinueWith( result => _statGainLif = result.Result.Activate() );

            /*_hooks.AddHook<LH_TournamentComplete>( SetupLHTournamentComplete )
                .ContinueWith( result => _LHTournamentComplete = result.Result.Activate() );*/

            _hooks.AddHook<LH_DrillPerform_ChangeStats>( SetupLHDPCS )
                .ContinueWith( result => _LHDPCS = result.Result.Activate() );
        }

        private uint SetupLHStatGainLifHook ( nuint self, uint value ) {
            Debug.WriteLine( "Stat Gain Hook: " + self + ", Val: " + value );
            var ret = _statGainLif!.OriginalFunction( self, value+6 );
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
