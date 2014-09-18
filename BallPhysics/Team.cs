using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BallPhysics
{
    public class Team
    {
        private InfluenceMap _teamInfluenceMap;
        public InfluenceMap TeamInfluenceMap
        {
            get
            {
                return _teamInfluenceMap;
            }
            set
            {
                _teamInfluenceMap = value;
            }
        }

        private List<Footballer> _teamRoster;
        public List<Footballer> TeamRoster
        {
            get
            {
                return _teamRoster;
            }
            set
            {
                _teamRoster = value;
            }
        }

        private Color _teamColor;
        public Color TeamColor
        {
            get
            {
                return _teamColor;
            }
            set
            {
                _teamColor = value;
            }
        }

        // set piece takers, etc:

        // IN-GAME:
        private Match _currentGame;
        public Match CurrentGame
        {
            get
            {
                return _currentGame;
            }
            set
            {
                _currentGame = value;
            }
        }

        private bool _hasTakenSetPiece = false;
        public bool HasTakenSetPiece
        {
            get
            {
                return _hasTakenSetPiece;
            }
            set
            {
                _hasTakenSetPiece = value;
            }
        }

        private bool _attackingLeft;
        public bool AttackingLeft
        {
            get
            {
                return _attackingLeft;
            }
            set
            {
                _attackingLeft = value;
            }
        }

        public Footballer NearestTeammate(Footballer guy)
        {
            Footballer nearest = null;
            double smallestD = Constants.ActualXMax;

            for (int i = 0; i < _teamRoster.Count; ++i)
            {
                Footballer current = _teamRoster[i];
                if (current == guy)
                {
                    continue;
                }
                double currentD = current.DistanceToPlayer(guy);

                if (currentD < smallestD)
                {
                    nearest = current;
                    smallestD = currentD;
                }
            }

            return nearest;
        }

        public Footballer PlayerNearestToBall()
        {
            Footballer nearest = null;
            double smallestD = Constants.ActualXMax;

            for (int i = 0; i < _teamRoster.Count; ++i)
            {
                Footballer current = _teamRoster[i];
                double currentD = current.DistanceToBall();

                if (currentD < smallestD)
                {
                    nearest = current;
                    smallestD = currentD;
                }
            }

            return nearest;
        }

        public bool ReadyForOtherTeamToTakeKickOff()
        {
            for (int i = 0; i < _teamRoster.Count; ++i)
            {
                Footballer current = _teamRoster[i];
                if (!current.IsNear(current.DefaultPositionInHalf()))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ReadyToTakeKickOff()
        {
            for (int i = 0; i < _teamRoster.Count - 2; ++i)
            {
                Footballer current = _teamRoster[i];
                if (!current.IsNear(current.DefaultPositionInHalf()))
                {
                    return false;
                }
            }

            for (int i = _teamRoster.Count - 2; i < _teamRoster.Count; ++i)
            {
                Footballer current = _teamRoster[i];
                if(!current.IsNear(Constants.CenterPoint))
                {
                    return false;
                }
            }

            return true;
        }

        public Team(List<Footballer> teamRoster, InfluenceMap teamInfMap, Match currentGame, bool goingLeft, Color teamColor)
        {
            _teamInfluenceMap = teamInfMap;
            _teamColor = teamColor;
            _teamRoster = teamRoster;
            _attackingLeft = goingLeft;
            _currentGame = currentGame;
        }
    }
}
