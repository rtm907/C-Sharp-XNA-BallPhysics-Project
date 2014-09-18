using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace BallPhysics
{
    /// <summary>
    /// Footballers interface.
    /// Stores the Footballers ID and Coords, its brain, map and tile references, its sight range,
    /// and other relevant data.
    /// </summary>
    public class Footballer : IComparable
    {
        #region Properties

        // Footballer unique ID
        private UInt32 _uniqueID;
        public UInt32 UniqueID
        {
            get
            {
                return this._uniqueID;
            }
            set
            {
                this._uniqueID = value;
            }
        }

        // Reference to the Map the Footballer lives in. 
        private Map _inhabitedMap;
        public Map InhabitedMap
        {
            get
            {
                return this._inhabitedMap;
            }
            set
            {
                this._inhabitedMap = value;
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

        private Vector2d _positionDouble;
        public Vector2d PositionDouble
        {
            get
            {
                return _positionDouble;
            }
            set
            {
                _positionDouble = value;
            }
        }

        protected SpriteBatchFootballer _myBitmap;
        public SpriteBatchFootballer MyBitmap
        {
            get
            {
                return _myBitmap;
            }
            set
            {
                _myBitmap = value;
            }
        }

        // Footballer statistics (there should be two of these, base and current)
        private UInt16[] _statistics;
        public UInt16[] Statistics
        {
            get
            {
                return this._statistics;
            }
            set
            {
                this._statistics = value;
            }
        }

        private UInt16 _radiusX;
        public UInt16 RadiusX
        {
            get
            {
                return _radiusX;
            }
        }
        private UInt16 _radiusY;
        public UInt16 RadiusY
        {
            get
            {
                return _radiusY;
            }
        }

        private Brain _FootballerBrain;
        public Brain FootballerBrain
        {
            get
            {
                return _FootballerBrain;
            }
            set
            {
                this._FootballerBrain = value;
            }
        }

        // The label below the Footballer (Footballer name?)
        private String _labelLower;
        public String LabelLower
        {
            get
            {
                return this._labelLower;
            }
            set
            {
                this._labelLower = value;
            }
        }

        // The label above the Footballer (for talking)
        private String _labelUpper;
        public String LabelUpper
        {
            get
            {
                return this._labelUpper;
            }
            set
            {
                this._labelUpper = value;
            }
        }

        /// <summary>
        /// Player's default position on the field, as vector on (0,1)x(0,1);
        /// For example a striker could be around (0.5, 0.8).
        /// A left-back could be at (0.1, 0.2).
        /// Goalkeeper is at (0.5, 0);
        /// </summary>
        private Vector2d _defaultPosition;
        public Vector2d DefaultPosition
        {
            get
            {
                return _defaultPosition;
            }
            set
            {
                _defaultPosition = value;
            }
        }

        #endregion

        #region Motion-related

        protected double _moveSpeedCurrent;
        public double MoveSpeedCurrent
        {
            get
            {
                return _moveSpeedCurrent;
            }
            set
            {
                _moveSpeedCurrent = value;
            }
        }

        // should be a unit vector
        private Vector2d _facingDirection;
        public Vector2d FacingDirection
        {
            get
            {
                return _facingDirection;
            }
            set
            {
                _facingDirection = value;
            }
        }

        // should be a unit vector
        private Vector2d _desiredDirection;
        public Vector2d DesiredDirection
        {
            get
            {
                return _desiredDirection;
            }
            set
            {
                if (value.Length() > 0)
                {
                    _desiredDirection = value;
                }
            }
        }

        protected double _moveSpeedMax;
        public double MoveSpeedMax
        {
            get
            {
                return _moveSpeedMax;
            }
        }

        protected double _moveAcceleration;
        public double MoveAcceleration
        {
            get
            {
                return _moveAcceleration;
            }
        }

        protected double _turnSpeed;
        public double TurnSpeeed
        {
            get
            {
                return _turnSpeed;
            }
        }

        private bool _isMoving;
        public bool IsMoving
        {
            get
            {
                return _isMoving;
            }
            set
            {
                _isMoving = value;
            }
        }

        private Nullable<Vector2d> _moveTarget;

        private Team _team;
        public Team Team
        {
            get
            {
                return _team;
            }
        }

        private Team _enemyTeam;
        public Team EnemyTeam
        {
            get
            {
                if (_enemyTeam == null)
                {
                    this._enemyTeam = _team.CurrentGame.OppositeTeam(_team);
                }

                return _enemyTeam;
            }
            set
            {
                _enemyTeam = value;
            }
        }

        private Match _myMatch;
        public Match MyMatch
        {
            get
            {
                return _myMatch;
            }
            set
            {
                _myMatch = value;
            }
        }

        /*
        private SortedList<UInt32, Footballer> _myTeamRoster;
        public SortedList<UInt32, Footballer> MyTeamRoster
        {
            get
            {
                return _myTeamRoster;
            }
        }

        private SortedList<UInt32, Footballer> _enemyTeamRoster;
        public SortedList<UInt32, Footballer> EnemyTeamRoster
        {
            get
            {
                return _enemyTeamRoster;
            }
        }
        */

        private Coords _positionAtLastInfMapUpdate;
        public Coords PositionAtLastInfMapUpdate
        {
            get
            {
                return _positionAtLastInfMapUpdate;
            }
            set
            {
                _positionAtLastInfMapUpdate = value;
            }
        }

        #region Methods

        private void Accelerate()
        {
            if (this.MoveSpeedCurrent != this.MoveSpeedMax)
            {
                this.MoveSpeedCurrent = Math.Min(this.MoveSpeedCurrent + this.MoveAcceleration, this.MoveSpeedMax);
            }
        }

        private void Decelerate()
        {
            if (this.MoveSpeedCurrent != 0)
            {
                this.MoveSpeedCurrent = Math.Max(this.MoveSpeedCurrent - this.MoveAcceleration, 0);
                //this.MoveSpeedCurrent = 0;
            }
        }

        private void Turn()
        {
            if (_facingDirection == _desiredDirection)
            {
                // no need to turn
                return;
            }

            double angle = _facingDirection.AngleBetween(_desiredDirection);

            double angleMagnitude = Math.Min(angle, Math.Abs(2 * Math.PI - angle));

                            
            double turnAngle = 
                Math.Min(angleMagnitude, this._turnSpeed * 
                (Constants.BaseTurningCoefficient + (1 - Constants.BaseTurningCoefficient) * (1 - this._moveSpeedCurrent / this._moveSpeedMax)));
             
            if (turnAngle == angleMagnitude)
            {
                this._facingDirection = this._desiredDirection;
                return;
            }

            if (angleMagnitude == angle)
            {
                // we're turning to the left  
                this._facingDirection = this._facingDirection.Rotate(turnAngle);
            }
            else
            {
                // turn to the right
                this._facingDirection = this._facingDirection.Rotate(2*Math.PI - turnAngle);
            }
        }

        public void TurnToFaceBallStoppingPlace()
        {
            List<Vector3d> ballProjection = _gameBall.Projection;

            if (ballProjection.Count == 0)
            {
                this.TurnToFaceBall();
                return;
            }

            this.TurnToFace(ballProjection[0].ProjectionXY() -
                _positionDouble);
        }

        public void TurnToFaceBall()
        {
            this._desiredDirection = _gameBall.Position3d.ProjectionXY() - this._positionDouble;
        }

        public void TurnToFace(Vector2d direction)
        {
            if (direction.Length() > 0)
            {
                this._desiredDirection = direction;
                _desiredDirection.ScaleToLength(1);
            }
        }

        /// <summary>
        /// Returns true if the projection of the ball is at the projected player position 'time' ticks in the future.
        /// </summary>
        public bool BallInControlRangePrediction(UInt16 time)
        {
            double speed = this._moveSpeedCurrent;
            double distanceCovered=0;
            for (int i = 0; i < time; ++i)
            {
                distanceCovered += speed;
                speed = Math.Max(_moveSpeedMax, speed + _moveAcceleration);
            }

            Vector2d distanceCoveredVector = this._facingDirection;
            distanceCoveredVector.ScaleToLength(distanceCovered);

            //Vector3d projectedBallPosition = _gameBall.Projection[_gameBall.Projection.Count - 1 - time];
            Vector3d projectedBallPosition = _gameBall.Position3d;
            if (_gameBall.Projection.Count > 0)
            {
                projectedBallPosition = _gameBall.Projection[Math.Max(0, _gameBall.Projection.Count - 1 - time)];
            }


            return (projectedBallPosition.ProjectionXY().
                DistanceTo(this._positionDouble + distanceCoveredVector) < Constants.BallControlRadius &&
                projectedBallPosition.Z < Constants.MaxControlHeight);
        }
        public bool BallInControlRangeArcPrediction(UInt16 time, Vector2d arcDirection, double arcHalfAngle)
        {
            double speed = this._moveSpeedCurrent;
            double distanceCovered = 0;
            for (int i = 0; i < time; ++i)
            {
                distanceCovered += speed;
                speed = Math.Max(_moveSpeedMax, speed + _moveAcceleration);
            }

            Vector2d distanceCoveredVector = this._facingDirection;
            distanceCoveredVector.ScaleToLength(distanceCovered);


            Vector3d projectedBallPosition = _gameBall.Position3d;
            if (_gameBall.Projection.Count > 0)
            {
                projectedBallPosition = _gameBall.Projection[Math.Max(0,_gameBall.Projection.Count - 1 - time)];
            }
            Vector2d projectedDirVector = projectedBallPosition.ProjectionXY() - (this._positionDouble + distanceCoveredVector);

            return ((projectedDirVector.Length() < Constants.BallControlRadius) &&
                (projectedBallPosition.Z < Constants.MaxControlHeight)) && (projectedDirVector.AngleBetween(arcDirection) < arcHalfAngle);
        }
        public bool BallInControlFacingRangePrediction(UInt16 time)
        {
            return BallInControlRangeArcPrediction(time, _facingDirection, Constants.BallControlDefaultHalfArc);
        }

        /// <summary>
        /// Does a rough check on whether the trajectory of the ball is reachable from the player's current position.
        /// If so, returns the supposed possition of collision.
        /// If not, returns null.
        /// </summary>
        public Nullable<Vector3d> BallReachablePrediction(UInt16 time)
        {
            double projectedSpeed = _moveSpeedCurrent;
            double projectedDistanceCovered = 0;
            
            List<Vector3d> projection = _gameBall.Projection;

            Vector3d projectedBallPosition = _gameBall.Position3d;
            for (int i = 0; (i < time) ; ++i)
            {
                projectedDistanceCovered += projectedSpeed;

                if (_gameBall.Projection.Count > 0)
                {
                    projectedBallPosition = projection[Math.Max(0, projection.Count - 1 - i)];
                }

                //Vector3d projectedBallPosition = projection[Math.Max(0, projection.Count - 1 - i)];

                if ((projectedBallPosition.ProjectionXY() - _positionDouble).Length() < projectedDistanceCovered
                    && projectedBallPosition.Z < Constants.MaxControlHeight)
                {
                    return projectedBallPosition;
                }
                projectedSpeed = Math.Min(projectedSpeed + _moveAcceleration, _moveSpeedMax);
            }

            return null;
        }

        public bool BallInControlRange()
        {
            return (_gameBall.Position3d.ProjectionXY() - this.PositionDouble).Length() < Constants.BallControlRadius;
        }
        public bool BallInControlRangeArc(Vector2d arcDirection, double arcHalfAngle)
        {
            Vector2d vectorToBall = (_gameBall.Position3d.ProjectionXY() - this.PositionDouble);
            return (vectorToBall.Length() < Constants.BallControlRadius) && (_gameBall.Position3d.Z < Constants.MaxControlHeight) &&
                (vectorToBall.AngleBetween(arcDirection) < arcHalfAngle);
        }
        public bool BallInControlFacingRange()
        {
            return BallInControlRangeArc(this._facingDirection, Constants.BallControlDefaultHalfArc);
        }

        public bool BallInDribblingRange()
        {
            return (_gameBall.Position3d.ProjectionXY() - this.PositionDouble).Length() < Constants.BallDribbleRadius;
        }

        public bool FacingBall()
        {
            return (_facingDirection.AngleBetween(_gameBall.Position3d.ProjectionXY() - _positionDouble) < Constants.DirectionCheckTolerance);
        }

        public bool FacingBallStoppingPlace()
        {
            List<Vector3d> ballProjection = _gameBall.Projection;

            if (ballProjection.Count == 0)
            {
                return this.FacingBall();
            }
            
            return (_facingDirection.AngleBetween(ballProjection[0].ProjectionXY() - 
                _positionDouble) < Constants.DirectionCheckTolerance);
        }

        public bool IsNear(Vector2d place)
        {
            return (_positionDouble - place).Length() < Constants.BallControlRadius;
        }

        public void Advance()
        {
            Vector2d delta = _facingDirection;
            delta.ScaleToLength(this._moveSpeedMax);
            this.MoveTo(_positionDouble + delta);
        }

        public void AdvanceInDirection(Vector2d direction)
        {
            Vector2d delta = direction;
            delta.ScaleToLength(this._moveSpeedMax);
            this.MoveTo(_positionDouble + delta);
        }

        public void AdvanceTowardBall()
        {
            Vector2d delta = _gameBall.Position3d.ProjectionXY() - this._positionDouble;
            delta.ScaleToLength(this._moveSpeedMax);
            this.MoveTo(_positionDouble + delta);
        }

        public void AdvanceTowardBallStoppingPlace()
        {
            List<Vector3d> ballProjection = _gameBall.Projection;

            if (ballProjection.Count == 0)
            {
                this.AdvanceTowardBall();
                return;
            }

            Vector2d delta = ballProjection[0].ProjectionXY() - this._positionDouble;
            delta.ScaleToLength(this._moveSpeedMax);
            this.MoveTo(_positionDouble + delta);
        }

        public void MoveTo(Vector2d goal)
        {
            this._moveTarget = goal;
            this._desiredDirection = goal - this._positionDouble;
            _desiredDirection.ScaleToLength(1);
            this._isMoving = true;
        }

        public bool FacingDesiredDirection()
        {
            return (this._desiredDirection == this._facingDirection);
        }

        public void UpdateMovement()
        {
            if (this._moveTarget != null)
            {
                if (this.IsNear(_moveTarget.Value))
                {
                    this._isMoving = false;
                    this._moveTarget = null;
                }
                else
                {
                    if (_desiredDirection != _facingDirection)
                    {
                        Vector2d motionVector = _moveTarget.Value - this._positionDouble;
                        if (motionVector.Length() > 0)
                        {
                            this._desiredDirection = (motionVector);
                            _desiredDirection.ScaleToLength(1);
                        }
                        else
                        {
                            _desiredDirection = _facingDirection;
                        }
                    }
                }
            }

            if (_desiredDirection != _facingDirection)
            {
                this.Decelerate();
                this.Turn();
            }

            if (_isMoving)
            {
                // accelerate
                this.Accelerate();
            }
            else
            {
                // decelerate
                this.Decelerate();
            }

            // move in facing dir
            // NOTE: this scaling should preferably be avoided
            Vector2d delta = _facingDirection;
            delta.ScaleToLength(_moveSpeedCurrent);

            Vector2d oldPosition = this._positionDouble;

            this._positionDouble += delta;

            this.OutOfBoundsCheck();

            // collision detection
            this._inhabitedMap.MyCollider.HandleCollisions(this, oldPosition);     
        }

        private void OutOfBoundsCheck()
        {
            if (this._positionDouble.X < _radiusX)
            {
                this._positionDouble.X = _radiusX;          
                this._isMoving = false;
                _moveSpeedCurrent = 0;
            }

            if (this._positionDouble.Y < _radiusY)
            {
                this._positionDouble.Y = _radiusY;
                this._isMoving = false;
                _moveSpeedCurrent = 0;
            }

            if (this._positionDouble.X > Constants.ActualXMax-_radiusX)
            {
                this._positionDouble.X = Constants.ActualXMax - 2*_radiusX;
                this._isMoving = false;
                _moveSpeedCurrent = 0;
            }

            if (this._positionDouble.Y > Constants.ActualYMax - _radiusY)
            {
                this._positionDouble.Y = Constants.ActualYMax - 2*_radiusY;
                this._isMoving = false;
                _moveSpeedCurrent = 0;
            }
        }

        public void ControlBall()
        {
            if ((_gameBall.Position3d.ProjectionXY() - this._positionDouble).Length() < Constants.BallControlRadius
                && (_gameBall.Position3d.Z < Constants.MaxControlHeight))
            {
                // depending on the ball velocity and direction and the player velocity and direction, there
                // are many stopping techniques of varying difficulties.
                // For now we take the simple route.

                _gameBall.Kick(new Vector3d(0, 0, 0.1 * _gameBall.Velocity.Length()), new Vector3d(0, 0, 0), this);
            }
        }

        public void Stop()
        {
            _moveTarget = null;
            _isMoving = false;
        }

        public void KickBall(Vector3d velocity, Vector3d spin)
        {
            if ((_gameBall.Position3d.ProjectionXY() - this._positionDouble).Length() < Constants.BallControlRadius)
            {
                if (velocity.Length() > Constants.GeneralMaxShotStrength)
                {
                    velocity.ScaleToLength(Constants.GeneralMaxShotStrength);
                }
                _gameBall.Kick(velocity, spin, this);
            }

            _myMatch.TeamInPossession = _team;
        }

        public double DistanceToBall()
        {
            return (this._positionDouble - _gameBall.Position3d.ProjectionXY()).Length();
        }

        public double DistanceToPlayer(Footballer someGuy)
        {
            return (this._positionDouble - someGuy.PositionDouble).Length();
        }

        #endregion

        public Int32 CompareTo(object obj)
        {
            if (!(obj is Footballer))
            {
                throw new Exception("Bad Footballer comparison.");
            }

            Footballer compared = (Footballer)obj;

            return (Int32)(this._uniqueID - compared._uniqueID);
        }

        public override bool Equals(object obj)
        {
            return (obj is Footballer) && this == (Footballer)obj;
        }

        public override Int32 GetHashCode()
        {
            return (Int32) _uniqueID;
        }

        public Coords PositionOnInfMap()
        {
            return new Coords((Int32)(this.PositionDouble.X / Constants.InfMapDefaultBoxSizeX),
                        (Int32)(this.PositionDouble.Y / Constants.InfMapDefaultBoxSizeY));
        }

        #endregion

        public Vector2d DefaultPositionInHalf()
        {
            Vector2d position = new Vector2d(Constants.GoalLeft + _defaultPosition.Y * (0.5 * Constants.ActualXMax - Constants.GoalLeft),
                    _defaultPosition.X * Constants.ActualYMax);
            if (!_team.AttackingLeft)
            {
                position = new Vector2d(Constants.ActualXMax - position.X, Constants.ActualYMax - position.Y);
            }

            return position;
        }

        /// <summary>
        /// Returns the player's default position on the field, which is given by his default formation position,
        /// and the current position of the ball.
        /// </summary>
        public Vector2d DefaultPositionInFullField()
        {
            Vector2d position = new Vector2d(Constants.GoalLeft + _defaultPosition.Y * (Constants.ActualXMax - Constants.GoalLeft),
                _defaultPosition.X * Constants.ActualYMax);

            // adjust in X according to ball
            double ballAdjustment = 300 * (2*_gameBall.Position3d.X - Constants.ActualXMax) / Constants.ActualXMax;

            position.X += ballAdjustment;

            if (!_team.AttackingLeft)
            {
                position = new Vector2d(Constants.ActualXMax - position.X, Constants.ActualYMax - position.Y);
            }

            return position;
        }

        #region Constructors

        // Creates the Footballer at startPos on the Map
        public Footballer(Map currentMap, Vector2d startPos, Vector2d defaultPlayingPosition, UInt32 ID, Team team)
        {
            this._myBitmap = SpriteBatchFootballer.Player;

            this._inhabitedMap = currentMap;
            this._uniqueID = ID;
            this._gameBall = _inhabitedMap.BallReference;
            this._team = team;
            this._myMatch = _inhabitedMap.MatchOnMap;
            this._defaultPosition = defaultPlayingPosition;

            this._FootballerBrain = new Brain();
            _FootballerBrain.MyFootballer = this;

            this._facingDirection = new Vector2d(0, 1);
            this._desiredDirection = this._facingDirection;
            this.PositionDouble = startPos;

            this._turnSpeed = Constants.BaseTurningSpeed;
            this._moveSpeedCurrent = 0;
            this._moveSpeedMax = 20;
            this._moveAcceleration = 5;

            this._radiusX = Constants.CollisionRadiusX;
            this._radiusY = Constants.CollisionRadiusY;
            // register in collider
            this._inhabitedMap.MyCollider.RegisterFootballer(this);

            
            //this._myTeamRoster = _team.TeamRoster;
            this._inhabitedMap.RosterAddFootballerTo(ID, this);
            this._team.TeamRoster.Add(this);
            
            //this._enemyTeamRoster = _inhabitedMap.TeamRoster((FootballerTeam)(((sbyte)_team + 1) % 2));
        }

        #endregion

    }

}
