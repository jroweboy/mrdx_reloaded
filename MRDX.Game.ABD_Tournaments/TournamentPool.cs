using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRDX.Base.Mod.Interfaces;

namespace MRDX.Game.ABD_Tournaments
{
    public class TournamentPool {
        public TournamentData tournamentData;
        public EMonsterRanks _monsterRank;
        public ETournamentPools _tournamentPool;

        public string _name = "Pool";
        public int _minimumSize = 0;
        public int _indexStart = 0;
        public int _indexEnd = 0;

        public int stat_start = 0;
        public int stat_end = 0;

        public int tournament_tier = 0; // Used for adding techniques and specials to new monsters generated for this tournament

        public List<ABD_TournamentMonster> monsters = new List<ABD_TournamentMonster>();

        public TournamentPool ( TournamentData data, EMonsterRanks rank, ETournamentPools pool, string name, int size, int istart, int iend, int sstart, int send, int tier ) {
            tournamentData = data;
            _monsterRank = rank;
            _tournamentPool = pool;

            _name = name;
            _minimumSize = size;
            _indexStart = istart;
            _indexEnd = iend;

            stat_start = sstart;
            stat_end = send;

            tournament_tier = tier;
        }

        public void MonsterAdd ( ABD_TournamentMonster m ) {
            if ( !monsters.Contains( m ) ) {
                monsters.Add( m );
                m.pools.Add( this );
            }
        }

        public void MonsterRemove ( ABD_TournamentMonster m ) {
            if ( monsters.Contains( m ) ) {
                monsters.Remove( m );
                m.pools.Remove( this );
            }
        }

        public void MonsterPromoteToNewPool ( TournamentPool newPool ) {
            TournamentData._logger.WriteLineAsync( "[ABD Tournaments]: Promoting Monster to new pool : " + newPool, Color.Orange );
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

        public void GenerateNewValidMonster ( List<MonsterGenus> available ) {
            TournamentData._logger.WriteLineAsync( "[ABD Tournaments] TP: Getting Breed", Color.AliceBlue );
            MonsterBreed breed = MonsterBreed.AllBreeds[ 0 ];

            // TODO: Smarter Logic for deciding which breed to make the monster!
            // // // // // // 

            if ( _tournamentPool == ETournamentPools.A_Phoenix ) {
                for ( var i = 0; i < 500; i++ ) {
                    breed = MonsterBreed.AllBreeds[ Random.Shared.Next() % MonsterBreed.AllBreeds.Count ];
                    if ( breed.breed_id == MonsterGenus.Phoenix ) {
                        break;
                    }
                }
            }

            else if ( _tournamentPool == ETournamentPools.B_Dragon ) {
                for ( var i = 0; i < 500; i++ ) {
                    breed = MonsterBreed.AllBreeds[ Random.Shared.Next() % MonsterBreed.AllBreeds.Count ];
                    if ( breed.breed_id == MonsterGenus.Dragon ) {
                        break;
                    }
                }
            }

            else if ( _tournamentPool == ETournamentPools.A_DEdge ) {
                for ( var i = 0; i < 500; i++ ) {
                    breed = MonsterBreed.AllBreeds[ Random.Shared.Next() % MonsterBreed.AllBreeds.Count ];
                    if ( breed.breed_id == MonsterGenus.Durahan ) {
                        break;
                    }
                }
            }

            else if ( _tournamentPool == ETournamentPools.F_Elder ) {
                for ( var i = 0; i < 250; i++ ) {
                    breed = MonsterBreed.AllBreeds[ Random.Shared.Next() % MonsterBreed.AllBreeds.Count ];
                    if ( available.Contains( breed.breed_id ) && available.Contains( breed.sub_id ) ) {
                        if ( breed.breed_id == MonsterGenus.Plant || breed.breed_id == MonsterGenus.Mew || breed.breed_id == MonsterGenus.Ape || breed.breed_id == MonsterGenus.Arrowhead
                            || breed.breed_id == MonsterGenus.Durahan || breed.breed_id == MonsterGenus.ColorPandora || breed.breed_id == MonsterGenus.Mock || breed.breed_id == MonsterGenus.Wracky ) {
                            break;
                        }
                    }
                }
            }

            else if ( _tournamentPool == ETournamentPools.F_Hero ) {
                for ( var i = 0; i < 250; i++ ) {
                    breed = MonsterBreed.AllBreeds[ Random.Shared.Next() % MonsterBreed.AllBreeds.Count ];
                    if ( available.Contains( breed.breed_id ) && available.Contains( breed.sub_id ) ) {
                        if ( breed.breed_id == MonsterGenus.Baku || breed.breed_id == MonsterGenus.Centaur || breed.breed_id == MonsterGenus.ColorPandora || breed.breed_id == MonsterGenus.Ducken ||
                                breed.breed_id == MonsterGenus.Gali || breed.breed_id == MonsterGenus.Hare || breed.breed_id == MonsterGenus.Henger || breed.breed_id == MonsterGenus.Mocchi ||
                                breed.breed_id == MonsterGenus.Niton || breed.breed_id == MonsterGenus.Tiger || breed.breed_id == MonsterGenus.Undine
                            ) {
                            break;
                        }
                    }
                }
            }

            else if ( _tournamentPool == ETournamentPools.F_Heel ) {
                for ( var i = 0; i < 250; i++ ) {
                    breed = MonsterBreed.AllBreeds[ Random.Shared.Next() % MonsterBreed.AllBreeds.Count ];
                    if ( available.Contains( breed.breed_id ) && available.Contains( breed.sub_id ) ) {
                        if ( breed.breed_id == MonsterGenus.Ape || breed.breed_id == MonsterGenus.Dragon || breed.breed_id == MonsterGenus.Joker || breed.breed_id == MonsterGenus.Kato
                            || breed.breed_id == MonsterGenus.Monol || breed.breed_id == MonsterGenus.Naga || breed.breed_id == MonsterGenus.Pixie || breed.breed_id == MonsterGenus.Suezo || breed.breed_id == MonsterGenus.Wracky ) {
                            break;
                        }
                    }
                }
            }



           else {
                for ( var i = 0; i < 100; i++ ) {
                    breed = MonsterBreed.AllBreeds[ Random.Shared.Next() % MonsterBreed.AllBreeds.Count ];
                    if ( available.Contains( breed.breed_id ) && available.Contains( breed.sub_id ) ) {
                        TournamentData._logger.WriteLineAsync( "[ABD Tournaments]: Found valid breed. " + breed.breed_id + "/" + breed.sub_id );
                        break;
                    }
                }
            }
            GenerateNewValidMonster( breed );

        }
        private void GenerateNewValidMonster ( MonsterBreed breed ) {
            TournamentData._logger.WriteLineAsync( "[ABD Tournaments] TP: Generating", Color.AliceBlue );
            //byte[] nmraw = [ 181, 0, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 215, 0, 72, 1, 228, 0, 233, 0, 184, 0, 216, 0, 50, 50, 50, 0, 255, 104, 9, 0, 3, 13, 0, 0, 65, 0, 0, 0, 0, 0, 255, 255 ];
            byte[] nmraw = new byte[ 60 ];// This doesn't matter, it gets completely overwritten below anyways.
            TournamentMonster nm = new TournamentMonster( nmraw );


            nm.breed_main = breed.breed_id; nm.breed_sub = breed.sub_id;
            TournamentData._logger.WriteLineAsync( "TP: Breed " + nm.breed_main + " " + nm.breed_sub, Color.AliceBlue );
            // // // // // //

            ABD_TournamentMonster abdm = new ABD_TournamentMonster( nm );

            abdm.monster.name = TournamentData._random_name_list[ Random.Shared.Next() % TournamentData._random_name_list.Length ];
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

            TournamentData._logger.WriteLineAsync( "[ABD Tournaments] TP: Basics Setup", Color.AliceBlue );
            while ( abdm.monster.stat_total < stat_start ) {
                abdm.AdvanceMonth();
            }
            TournamentData._logger.WriteLineAsync( "[ABD Tournaments] TP: Advancing", Color.AliceBlue );
            for ( int i = 0; i < tournament_tier; i++ ) {
                abdm.LearnTechnique();
            }
            TournamentData._logger.WriteLineAsync( "[ABD Tournaments] TP: Techs", Color.AliceBlue );
            if ( !abdm.alive ) {
                abdm.alive = true;
                abdm.lifespan = 6;
            }

            abdm._monsterRank = _monsterRank;

            TournamentData._logger.WriteLineAsync( "[ABD Tournaments] TP: Cleanup", Color.AliceBlue );
            tournamentData.monsters.Add( abdm );
            this.MonsterAdd( abdm );
        }


        /// <summary>
        /// Adds the tournament participants to the provided list. This is a random selection of [_minimumSize] monsters.
        /// </summary>
        /// <param name="participants"></param>
        public void AddTournamentParticipants ( List<ABD_TournamentMonster> participants ) {
            for ( var i = 0; i < _minimumSize; i++ ) { participants.Add( monsters[ i ] ); }
        }

    }
}
