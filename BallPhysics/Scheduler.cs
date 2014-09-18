using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallPhysics
{
    /// <summary>
    /// Keeps track of game-time and takes care of having all the actors execute their actions.
    /// </summary>
    public class Scheduler
    {
        // Time since game started
        private UInt64 _timeCounter = 0;
        public UInt64 TimeCounter
        {
            get
            {
                return this._timeCounter;
            }
        }

        // Should be updated to a 'World' reference once there is a 'gameworld' class.
        private Map _gameMap;

        /// <summary>
        /// The scheduler updates its map's actors for the current tick.
        /// </summary>
        public void Update()
        {
            _gameMap.MatchOnMap.Update();

            foreach (KeyValuePair<UInt32, Footballer> kvp in _gameMap.Roster)
            {
                Footballer someGuy = kvp.Value;

                Brain currentBrain = someGuy.FootballerBrain;
                if (currentBrain != null)
                {
                    currentBrain.Update();
                }

                // update inf map if necessary.
                // should ignore the GKs...
                if (_timeCounter % Constants.InfMapUpdatePeriod == someGuy.UniqueID)
                {
                    Coords newInfMapPosition = new Coords((Int32)(someGuy.PositionDouble.X / Constants.InfMapDefaultBoxSizeX),
                        (Int32)(someGuy.PositionDouble.Y / Constants.InfMapDefaultBoxSizeY));
                    _gameMap.InfMapGenerator.UpdateMapViaSourceMap(someGuy.Team.TeamInfluenceMap, 
                        _gameMap.DefaultInfSourceMap, someGuy.PositionAtLastInfMapUpdate, newInfMapPosition);
                    someGuy.PositionAtLastInfMapUpdate = newInfMapPosition;
                }

                this._timeCounter++;
            }
            _gameMap.BallReference.UpdateMotion3D();
        }

        public Scheduler(Map gamemap)
        {
            this._gameMap = gamemap;
        }
    }
}
