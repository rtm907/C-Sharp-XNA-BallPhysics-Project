using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallPhysics
{
    public enum MatchState : sbyte
    {
        InGame = 0,
        KickOff,
        GoalKick,
        Corner,
        ThrowIn,
        FreeKick
    }

    public enum BallState : sbyte
    {
        InBounds = 0,
        NetLeft,
        NetRight,
        OutLeftTop,
        OutLeftBottom,
        OutRightTop,
        OutRightBottom,
        ThrowTop,
        ThrowBottom
    }

    public class Match
    {
        private float _timeEllapsed;
        public float TimeEllapsed
        {
            get
            {
                return _timeEllapsed;
            }
        }

        private MatchState _state;
        public MatchState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        private Ball _gameBall;
        public Ball GameBall
        {
            get
            {
                return _gameBall;
            }
            set
            {
                _gameBall = value;
            }
        }

        private Team _teamLeft;
        public Team TeamLeft
        {
            get
            {
                return _teamLeft;
            }
            set
            {
                _teamLeft = value;
            }
        }

        private Team _teamRight;
        public Team TeamRight
        {
            get
            {
                return _teamRight;
            }
            set
            {
                _teamRight = value;
            }
        }

        private Team _teamInPossession;
        public Team TeamInPossession
        {
            get
            {
                return _teamInPossession;
            }
            set
            {
                _teamInPossession = value;
            }
        }

        private Vector2d _deadBallLocation;
        public Vector2d DeadBallLocation
        {
            get
            {
                return _deadBallLocation;
            }
            set
            {
                _deadBallLocation = value;
            }
        }

        private sbyte _goalsTeamA;
        private sbyte _goalsTeamB;

        public Team OppositeTeam(Team aTeam)
        {
            if (_teamLeft == aTeam)
            {
                return _teamRight;
            }

            return _teamLeft;
        }

        private BallState DetermineBallState(Vector3d position)
        {
            // out overrides throw in
            if (position.X < Constants.GoalLineLeft)
            {
                if (position.Y < Constants.GoalTop)
                {
                    return BallState.OutLeftTop;
                }
                else if (position.Y < Constants.GoalBottom && position.Z < Constants.GoalHeight)
                {
                    return BallState.NetLeft;
                }
                else
                {
                    return BallState.OutLeftBottom;
                }
            }
            else if (position.X < Constants.GoalLineRight)
            {
                if (position.Y < Constants.TouchLineTop)
                {
                    return BallState.ThrowTop;
                }
                else if (position.Y < Constants.TouchLineBottom )
                {
                    return BallState.InBounds;
                }
                else
                {
                    return BallState.ThrowBottom;
                }
            }
            else
            {
                if (position.Y < Constants.GoalTop)
                {
                    return BallState.OutRightTop;
                }
                else if (position.Y < Constants.GoalBottom && position.Z < Constants.GoalHeight)
                {
                    return BallState.NetRight;
                }
                else
                {
                    return BallState.OutRightBottom;
                }
            }
        }

        private void UpdateStateOfPlay()
        {
            #region in play
            if (_state == MatchState.InGame)
            {
                // assuming the ball was in play last time we checked.
                BallState ballState = DetermineBallState(_gameBall.Position3d);
                switch (ballState)
                {
                    case BallState.InBounds:
                        break;
                        
                    case BallState.NetLeft:
                        ++_goalsTeamB;
                        _state = MatchState.KickOff;
                        _gameBall.SpawnAt(Constants.CenterPoint);
                        _teamInPossession = _teamLeft;
                        break;

                    case BallState.NetRight:
                        ++_goalsTeamA;
                        _state = MatchState.KickOff;
                        _gameBall.SpawnAt(Constants.CenterPoint);
                        _teamInPossession = _teamRight;
                        break;

                    case BallState.OutLeftTop:
                        if (_gameBall.LastTouch == _teamLeft)
                        {
                            _state = MatchState.Corner;
                            _deadBallLocation = Constants.CornerPoint[0];
                            _gameBall.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamRight;
                        }
                        else
                        {
                            _state = MatchState.GoalKick;
                            _deadBallLocation = Constants.GoalKickPoint[0];
                            _gameBall.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamLeft;
                        }
                        break;
                    case BallState.OutLeftBottom:
                        if (_gameBall.LastTouch == _teamLeft)
                        {
                            _state = MatchState.Corner;
                            _deadBallLocation = Constants.CornerPoint[1];
                            _gameBall.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamRight;
                        }
                        else
                        {
                            _state = MatchState.GoalKick;
                            _deadBallLocation = Constants.GoalKickPoint[0];
                            _gameBall.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamLeft;
                        }
                        break;
                    case BallState.OutRightTop:                     
                        if (_gameBall.LastTouch == _teamRight)
                        {
                            _state = MatchState.Corner;
                            _deadBallLocation = Constants.CornerPoint[2];
                            _gameBall.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamLeft;
                        }
                        else
                        {
                            _state = MatchState.GoalKick;
                            _deadBallLocation = Constants.GoalKickPoint[1];
                            _gameBall.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamRight;
                        }
                        break;
                    case BallState.OutRightBottom:
                        if (_gameBall.LastTouch == _teamRight)
                        {
                            _state = MatchState.Corner;
                            _deadBallLocation = Constants.CornerPoint[3];
                            _gameBall.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamLeft;
                        }
                        else
                        {
                            _state = MatchState.GoalKick;
                            _deadBallLocation = Constants.GoalKickPoint[1];
                            _gameBall.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamRight;
                        }
                        break;
                    case BallState.ThrowTop:
                        _state = MatchState.ThrowIn;
                        _deadBallLocation = new Vector2d(_gameBall.Position3d.X, Constants.TouchLineTop);
                        _gameBall.SpawnAt(_deadBallLocation);
                        if (_gameBall.LastTouch == _teamLeft)
                        {
                            _teamInPossession = _teamRight;
                        }
                        else
                        {
                            _teamInPossession = _teamLeft;
                        }
                        break;
                    case BallState.ThrowBottom:
                        _state = MatchState.ThrowIn;
                        _deadBallLocation = new Vector2d(_gameBall.Position3d.X, Constants.TouchLineBottom);
                        _gameBall.SpawnAt(_deadBallLocation);
                        if (_gameBall.LastTouch == _teamLeft)
                        {
                            _teamInPossession = _teamRight;
                        }
                        else
                        {
                            _teamInPossession = _teamLeft;
                        }
                        break;
                }
            }
            #endregion
            else if (_teamInPossession.HasTakenSetPiece)
            {
                _teamInPossession.HasTakenSetPiece = false;
                _state = MatchState.InGame;
            }
        }

        public void Update()
        {
            _timeEllapsed += Constants.MatchTimeDelta;
            UpdateStateOfPlay();
        }

        public void KickOff(bool left)
        {
            _state = MatchState.KickOff;
            _gameBall.SpawnAt(Constants.CenterPoint);
            _teamInPossession = left ? _teamLeft : _teamRight;
        }

        public Match(Ball gameBall)
        {
            _gameBall = gameBall;
            _timeEllapsed = 0;
            _state = MatchState.KickOff;
        }
    }
}
