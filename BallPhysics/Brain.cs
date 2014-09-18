using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallPhysics
{

    public class Brain
    {
        private Footballer _owner;
        public Footballer MyFootballer
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
            }
        }

        private Action _currentAction;

        private void SetAction(Action newAction)
        {
            _currentAction = newAction;
            _owner.LabelUpper = newAction.ToString();
        }

        /*
          Ball gameBall = _owner.GameBall;


          if (_owner.BallInDribblingRange())
          {
              if (dribblingCoutner < 30)
              {
                  this.Dribble(gameBall);
                  ++dribblingCoutner;
                  return;
              }
              else
              {
                  if (!_owner.BallInControlRange())
                  {
                      this.GoTowardBall(gameBall);
                      _owner.UpdateMovement();
                      return;
                  }
              }
          }


          if (_owner.BallInControlRange())
          {
              dribblingCoutner = 0;

              // ball near player
              if (gameBall.Velocity.ProjectionXY().Length() > 0)
              {
                  // if ball is in motion, control it
                  _owner.ControlBall(gameBall);
              }
              else if (gameBall.Position3d.Z == 0)
              {
                  // once ball is at rest, pass
                  LookForPass(gameBall);
              }

          }*/

        /// <summary>
        /// Forces an action on the player. For when he falls down, stumbles, etc. 
        /// </summary>
        private void ForceAction(Action forcer)
        {
            _currentAction = forcer;
        }

        /// <summary>
        /// Analyzes the situation and provides the footballer with a new action.
        /// </summary>
        /*
        private void AcquireNewAction()
        {
            // if player doesn't control ball, set to anticipate
            if (!_owner.BallInControlRange())
            {
                _currentAction = new ActionAnticipate(_owner);
                return;
            }

            // pick a pass with high prob, dribble with some prob, shot with low prob
            RandomStuff randomator = _owner.InhabitedMap.Randomator;
            UInt32 decider = randomator.NSidedDice(20, 1);

            if (decider < 10)
            {
                // pass
                Footballer target = this.LookForPass();
                _currentAction = new ActionPassToSpot(_owner, target.PositionDouble, (PassType)(decider % 2));
                return;
            }
            else if (decider < 15)
            {
                // dribble
                Vector2d delta = _owner.FacingDirection;
                delta.Rotate((Math.PI / 6) * decider);
                delta.ScaleToLength(250);

                _currentAction = new ActionDribbleTo(_owner, _owner.PositionDouble + delta);
                return;
            }
            else
            {
                // take a shot
                _currentAction = new ActionTakeAShot(_owner);
                return;
            }
        }
        */

        /// <summary>
        /// Sets up a kick off situation.
        /// </summary>
        private void AcquireNewActionKickOff()
        {
            List<Footballer> roster = _owner.Team.TeamRoster;
            if ((_owner.UniqueID - roster[0].UniqueID) < roster.Count - 2 || _owner.MyMatch.TeamInPossession != _owner.Team)
            {
                // if team isn't in possession, or is not one of the last 2 field players, go to default position.
                if (!_owner.IsNear(_owner.DefaultPositionInHalf()))
                {
                    SetAction(new ActionGoto(_owner, _owner.DefaultPositionInHalf()));
                    return;
                }
                else
                {
                    SetAction(new ActionWait(_owner, 10));
                    return;
                }
            }

            else
            {
                // have two guys take kick off
                if (!_owner.IsNear(Constants.CenterPoint))
                {
                    SetAction(new ActionGoto(_owner, Constants.CenterPoint));
                    return;
                }
                else
                {
                    // check to see if everything is ready to go.
                    if (_owner.Team.ReadyToTakeKickOff() && _owner.EnemyTeam.ReadyForOtherTeamToTakeKickOff())
                    {
                        // kick off
                        SetAction(new ActionPassToSpot(_owner, roster.Last().PositionDouble, PassType.Short));
                        _owner.Team.HasTakenSetPiece = true;
                        return;
                    }
                }
            }
            if (_currentAction == null)
            {
                SetAction(new ActionWait(_owner, 10)); 
            }
        }

        private void AcquireNewActionThrowIn()
        {
            List<Footballer> roster = _owner.Team.TeamRoster;
            if (_owner.MyMatch.TeamInPossession != _owner.Team || !(_owner == _owner.Team.PlayerNearestToBall()))
            {
                // if team isn't in possession, or is not one of the last 2 field players, go to default position.
                if (!_owner.IsNear(_owner.DefaultPositionInFullField()))
                {
                    SetAction(new ActionGoto(_owner, PositionInDirectionOf(_owner.DefaultPositionInFullField())));
                    return;
                }
                else
                {
                    SetAction(new ActionWait(_owner, 10)); 
                }
            }
            else
            {
                // have a guy take the throw
                if (_owner.IsNear(_owner.MyMatch.DeadBallLocation))
                {
                    SetAction(new ActionPassToSpot(_owner, _owner.Team.NearestTeammate(_owner).PositionDouble, PassType.Lob));
                    _owner.Team.HasTakenSetPiece = true;
                    return;
                }
                else
                {
                    SetAction(new ActionGoto(_owner, _owner.MyMatch.DeadBallLocation));
                }
            }
        }

        private void AcquireNewActionCornerKick()
        {
            List<Footballer> roster = _owner.Team.TeamRoster;
            if (_owner.MyMatch.TeamInPossession != _owner.Team || !(_owner == _owner.Team.PlayerNearestToBall()))
            {
                // if team isn't in possession, or is not one of the last 2 field players, go to default position.
                if (!_owner.IsNear(_owner.DefaultPositionInFullField()))
                {
                    SetAction(new ActionGoto(_owner, PositionInDirectionOf(_owner.DefaultPositionInFullField())));
                    return;
                }
                else
                {
                    SetAction(new ActionWait(_owner, 10));
                }
            }
            else
            {
                // have a guy take the kick
                if (_owner.IsNear(_owner.MyMatch.DeadBallLocation))
                {
                    //DoubleFootballerPair bestLob = GetBestLobPass();
                    if (_owner.Team.AttackingLeft)
                    {
                        SetAction(new ActionPassToSpot(_owner, Constants.PenaltyRight, PassType.Lob));
                    }
                    else
                    {
                        SetAction(new ActionPassToSpot(_owner, Constants.PenaltyLeft, PassType.Lob));
                    }
                    _owner.Team.HasTakenSetPiece = true;
                    return;
                }
                else
                {
                    SetAction(new ActionGoto(_owner, _owner.MyMatch.DeadBallLocation));
                }
            }

        }

        private void AcquireNewActionGoalKick()
        {
            List<Footballer> roster = _owner.Team.TeamRoster;
            if (_owner.MyMatch.TeamInPossession != _owner.Team || !(_owner == roster[0]))
            {
                // if team isn't in possession, or is not one of the last 2 field players, go to default position.
                if (!_owner.IsNear(_owner.DefaultPositionInFullField()))
                {
                    SetAction(new ActionGoto(_owner, PositionInDirectionOf(_owner.DefaultPositionInFullField())));
                    return;
                }
                else
                {
                    SetAction(new ActionWait(_owner, 10));
                }
            }
            else
            {
                // have a guy take the kick
                if (_owner.IsNear(_owner.MyMatch.DeadBallLocation))
                {
                    //DoubleFootballerPair bestLob = GetBestLobPass();
                    SetAction(new ActionPassToSpot(_owner, Constants.CenterPoint, PassType.Lob));
                    _owner.Team.HasTakenSetPiece = true;
                    return;
                }
                else
                {
                    SetAction(new ActionGoto(_owner, _owner.MyMatch.DeadBallLocation));
                }
            }

        }

        private void AcquireNewActionPlayerHasBall()
        {
            // get shot eval
            double shotEval = EvaluatorShot();

            // get best short and lob passes
            DoubleFootballerPair bestShort = GetBestShortPass();
            DoubleFootballerPair bestLob = GetBestLobPass();

            // get dribbling eval
            double dribblingEval = EvaluatorDribble();

            // make decision

            double bestChoice = (new double[] { shotEval, bestShort.value, bestLob.value, dribblingEval }).Max();

            if (bestChoice == shotEval)
            {
                SetAction(new ActionTakeAShot(_owner));
                return;
            }
            else if (bestChoice == bestShort.value)
            {
                SetAction(new ActionPassToSpot(_owner, bestShort.player.PositionDouble, PassType.Short));
                return;
            }
            else if (bestChoice == bestLob.value)
            {
                SetAction(new ActionPassToSpot(_owner, bestLob.player.PositionDouble, PassType.Lob));
                return;
            }
            else
            {

                Vector2d goalPosition = Constants.Uprights[0]; //upper-left post
                if (!_owner.Team.AttackingLeft)
                {
                    goalPosition = Constants.Uprights[2]; //upper-right post
                }
                Vector2d goalVector = goalPosition - _owner.PositionDouble;
                goalVector.ScaleToLength(_owner.MoveSpeedCurrent);

                SetAction(new ActionDribbleTo(_owner, _owner.PositionDouble + goalVector));
                return;
            }

        }

        /// <summary>
        /// Used to evaluate passes.
        /// </summary>
        private struct DoubleFootballerPair
        {
            public double value;
            public Footballer player;

            public DoubleFootballerPair(double val, Footballer guy)
            {
                value = val;
                player = guy;
            }
        }

        private DoubleFootballerPair GetBestShortPass()
        {
            List<Footballer> roster = _owner.Team.TeamRoster;
            
            Footballer best = null;
            double bestVal = 0;
            
            for (int i = 0; i < roster.Count; ++i)
            {
                Footballer current = roster[i];
                if (current == _owner)
                {
                    continue;
                }

                double currentVal = EvaluatorPassGround(current);

                if (currentVal > bestVal)
                {
                    bestVal = currentVal;
                    best = current;
                }
            }

            return new DoubleFootballerPair(bestVal, best);
        }

        private DoubleFootballerPair GetBestLobPass()
        {
            List<Footballer> roster = _owner.Team.TeamRoster;

            Footballer best = null;
            double bestVal = 0;

            for (int i = 0; i < roster.Count; ++i)
            {
                Footballer current = roster[i];
                if (current == _owner)
                {
                    continue;
                }

                double currentVal = EvaluatorPassLob(current);

                if (currentVal > bestVal)
                {
                    bestVal = currentVal;
                    best = current;
                }
            }

            return new DoubleFootballerPair(bestVal, best);
        }

        private void AcquireNewActionInGameTeamHasPossession()
        {
            // check to see if player has ball
            if (_owner.BallInControlRange())
            {
                this.AcquireNewActionPlayerHasBall();
                return;
            }

            // player doesn't have the ball.

            // if player nearest to ball, go get it
            if (_owner.Team.PlayerNearestToBall() == _owner)
            {
                Nullable<Vector3d> ballDir = _owner.BallReachablePrediction(100);
                if (ballDir != null)
                {
                    SetAction(new ActionGoto(_owner, PositionInDirectionOf(ballDir.Value.ProjectionXY())));
                    return;
                }
            }

            // if ball is approaching, try to control it
            if (_owner.BallInControlRangePrediction(10))
            {
                SetAction(new ActionAnticipate(_owner));
                return;
            }

            // otherwise, advance toward preferred spot
            Vector2d desiredSpot = _owner.DefaultPositionInFullField();
            if (_owner.IsNear(desiredSpot))
            {
                SetAction(new ActionAnticipate(_owner));
                return;
            }

            SetAction( new ActionGoto(_owner, desiredSpot));
        }

        private Vector2d PositionInDirectionOf(Vector2d target)
        {
            Vector2d delta = (target - _owner.PositionDouble);
            delta.ScaleToLength(2*Constants.BallControlRadius);
            return (_owner.PositionDouble + delta);
        }

        private void AcquireNewActionInGameTeamHasntPossession()
        {
            
            // if ball in control range, challenge
            Nullable<Vector3d> ballNear = _owner.BallReachablePrediction(10);
            if (ballNear != null)
            {
                SetAction(new ActionChallenge(_owner, ballNear.Value.ProjectionXY() - _owner.DefaultPosition));
                return;
            }

            // if ball reasonably close, move to intercept
            Nullable<Vector3d> reachBall = _owner.BallReachablePrediction(20);
            if (reachBall != null)
            {
                SetAction( new ActionGoto(_owner, reachBall.Value.ProjectionXY()));
                return;
            }

            // if player nearest to ball, go get it
            if (_owner.Team.PlayerNearestToBall() == _owner)
            {
                Nullable<Vector3d> ballDir = _owner.BallReachablePrediction(100);
                if (ballDir != null)
                {
                    SetAction(new ActionGoto(_owner, PositionInDirectionOf(ballDir.Value.ProjectionXY())));
                    return;
                }
            }

            // otherwise go to default place
            Vector2d desiredSpot = _owner.DefaultPositionInFullField();
            bool ownernear = _owner.IsNear(desiredSpot);
            if (_owner.IsNear(desiredSpot))
            {
                SetAction(new ActionAnticipate(_owner));
                return;
            }

            //Vector2d delta = (desiredSpot - _owner.PositionDouble);
            //delta.ScaleToLength(_owner.MoveSpeedCurrent);
            SetAction(new ActionGoto(_owner, PositionInDirectionOf(desiredSpot)));

        }

        /// <summary>
        /// Main AI method. Chooses an Action according to the situation.
        /// </summary>
        private void AcquireNewActionInGame()
        {
            if (_owner.MyMatch.TeamInPossession == _owner.Team)
            {
                this.AcquireNewActionInGameTeamHasPossession();
            }
            else
            {
                this.AcquireNewActionInGameTeamHasntPossession();
            }
        }

        private void AcquireNewAction()
        {
            switch (_owner.MyMatch.State)
            {
                case MatchState.InGame:
                    this.AcquireNewActionInGame();
                    break;

                case MatchState.KickOff:
                    this.AcquireNewActionKickOff();
                    break;

                case MatchState.ThrowIn:
                    this.AcquireNewActionThrowIn();
                    break;

                case MatchState.Corner:
                    this.AcquireNewActionCornerKick();
                    break;

                case MatchState.GoalKick:
                    this.AcquireNewActionGoalKick();
                    break;
            }
        }


        /// <summary>
        /// Main update function to be called from the scheduler each tick.
        /// </summary>
        public virtual void Update()
        {
            // if current action is null or finished, acquire new one
            if (_currentAction == null || _currentAction.Finished)
            {
                this.AcquireNewAction();
            }

            _currentAction.Update();
        }

        /// <summary>
        /// Returns a double that measures the usefulness of a ground pass to passTarget
        /// </summary>
        private double EvaluatorPassGround(Footballer passTarget)
        {
            double evaluation = 0;

            Vector2d passVector = passTarget.PositionDouble - _owner.PositionDouble;

            // add bonus for short passes (1,e)
            evaluation += Math.Pow(Math.E, (1 - passVector.Length() / Constants.ActualXMax));

            // add bonus for going in the direction of enemy goal (0,1)
            Vector2d goalPosition = Constants.Uprights[0]; //upper-left post
            if (!_owner.Team.AttackingLeft)
            {
                goalPosition = Constants.Uprights[2]; //upper-right post
            }
            double angle = passVector.AngleBetween(goalPosition - _owner.PositionDouble);
            angle = Math.Abs(angle - Math.PI);
            evaluation += (angle / Math.PI);

            // add large bonus if lane of passing is empty (0 or 3).
            List<Coords> lane = _owner.InhabitedMap.RayTracer(_owner.PositionOnInfMap(), passTarget.PositionOnInfMap());
            InfluenceMap enemyMap = _owner.EnemyTeam.TeamInfluenceMap;
            double interdiction = 0;
            for (int i = 0; i < lane.Count; ++i)
            {
                // these should be weighted somehow
                double toAdd = enemyMap.GetMapValue(lane[i]);
                if(toAdd > 0.5)
                {
                    evaluation += toAdd;
                }
            }
            if(interdiction == 0)
            {
                evaluation += 3;
            }

            // there should be a bonus for players making runs

            return evaluation;
        }

        /// <summary>
        /// Returns a double that measures the usefulness of a lob pass to passTarget
        /// </summary>
        private double EvaluatorPassLob(Footballer passTarget)
        {
            double evaluation = 0;

            Vector2d passVector = passTarget.PositionDouble - _owner.PositionDouble;

            // add bonus for long passes (0,1)
            evaluation += passVector.Length() / Constants.ActualXMax;

            // add bonus for going in the direction of enemy goal (0,1)
            Vector2d goalPosition = Constants.Uprights[0]; //upper-left post
            if (!_owner.Team.AttackingLeft)
            {
                goalPosition = Constants.Uprights[2]; //upper-right post
            }
            double angle = passVector.AngleBetween(goalPosition - _owner.PositionDouble);
            angle = Math.Abs(angle - Math.PI);
            evaluation += (angle / Math.PI);

            // add large bonus if target is alone.
            InfluenceMap enemyMap = _owner.EnemyTeam.TeamInfluenceMap;
            double interdiction = enemyMap.SumInArea(passTarget.PositionOnInfMap(), (UInt16)(passVector.Length()/(0.1*Constants.ActualXMax)+1));
            if (interdiction < 1)
            {
                evaluation += 3;
            }

            return evaluation;
        }

        private double EvaluatorShot()
        {
            double evaluation = 0;

            // large bonus for proximity to goal (1,e).
            Vector2d goalPosition = Constants.Uprights[0]; //upper-left post
            if (!_owner.Team.AttackingLeft)
            {
                goalPosition = Constants.Uprights[2]; //upper-right post
            }
            Vector2d goalVector = goalPosition - _owner.PositionDouble;
            double maxShotDistance = 2000;
            if (goalVector.Length() < maxShotDistance)
            {
                evaluation += Math.Pow(Math.E, (1 - goalVector.Length() / maxShotDistance));
            }

            // large bonus for clear line of shot
            // add large bonus if lane of passing is empty (0,3).
            List<Coords> lane = _owner.InhabitedMap.RayTracer(_owner.PositionOnInfMap(), StaticMathFunctions.PositionOnFieldToInfMapCoords(goalPosition));
            InfluenceMap enemyMap = _owner.EnemyTeam.TeamInfluenceMap;
            double interdiction = 0;
            for (int i = 0; i < lane.Count; ++i)
            {
                // these should be weighted somehow
                double toAdd = enemyMap.GetMapValue(lane[i]);
                if (toAdd > 0.7)
                {
                    evaluation += toAdd;
                }
            }
            if (interdiction < 3)
            {
                evaluation += (3-interdiction);
            }

            // there should be a bonus for easy shots

            // there should be a monster bonus for when the GK is out of position

            return evaluation;
        }

        private double EvaluatorDribble()
        {
            double evaluation = 0;

            // bonus for proximity to enemy goal (0,e-1).
            Vector2d goalPosition = Constants.Uprights[0]; //upper-left post
            if (!_owner.Team.AttackingLeft)
            {
                goalPosition = Constants.Uprights[2]; //upper-right post
            }
            Vector2d goalVector = goalPosition - _owner.PositionDouble;
            double maxShotDistance = 2000;
            if (goalVector.Length() < maxShotDistance)
            {
                evaluation += (Math.Pow(Math.E, (1 - goalVector.Length() / maxShotDistance))-1);
            }

            // bonus for player being under little pressure
            InfluenceMap enemyMap = _owner.EnemyTeam.TeamInfluenceMap;
            double interdiction = enemyMap.SumInArea(_owner.PositionOnInfMap(), 3);
            if (interdiction < 3)
            {
                evaluation += (3 - interdiction);
            }

            // there should be a dominating bonus for players with good dribbling ability

            return evaluation;
        }

        private Footballer LookForPass()
        {
            Footballer target = null;


            // pick random target

            List<Footballer> roster = _owner.Team.TeamRoster;
            UInt16 choices = (UInt16)roster.Count;
            RandomStuff randomator = _owner.InhabitedMap.Randomator;
            UInt32 choice = randomator.NSidedDice(choices, 1) - 1;

            while (choice == _owner.UniqueID && roster.Count > 1)
            {
                choice = randomator.NSidedDice(choices, 1) - 1;
            }

            target = roster[(Int32) choice];

            return target;
        }

        public Brain()
        {

        }
    }

    public class BrainPlayer : Brain
    {
        public override void Update()
        {
        }

        public void MoveOrder(Coords targetPixel)
        {
        }

        public BrainPlayer()
            : base()
        {
        }
    }

    public class BrainDead : Brain
    {
        public override void Update()
        {
        }

        public BrainDead()
            : base()
        {
        }
    }


    /// <summary>
    /// Contains the logic for one of the actions a player can do.
    /// </summary>
    public abstract class Action
    {
        protected bool _finished = false;
        public bool Finished
        {
            get
            {
                return _finished;
            }
            set
            {
                _finished = value;
            }
        }

        protected Footballer _owner;
        public Footballer Owner
        {
            get
            {
                return _owner;
            }
        }

        public abstract void Update();

        public override string ToString()
        {
            return this.GetType().ToString();
        }

        public Action(Footballer owner)
        {
            _owner = owner;
        }
    }

    public enum PassType : sbyte
    {
        Short = 0,
        Lob
    }

    /// <summary>
    /// Passes the ball to the target; for now assuming static target.
    /// </summary>
    public class ActionPassToSpot : Action
    {

        private Vector2d _target;
        public Vector2d Target
        {
            get
            {
                return _target;
            }
        }

        private PassType _typeOfPass;

        private Int32 _entryWait = 0;
        private Int32 _exitWait = 0;


        // NOTE: The waiter is c/p from the shot class. Fix so the two classes inherit a common class.

        /// <summary>
        /// Takes care of the pre/post waits. Returns true if currently waiting.
        /// </summary>
        private bool Waiter()
        {
            if (_entryWait < Constants.DefaultPreWait)
            {
                ++_entryWait;
                return true;
            }

            if (_exitWait > 0)
            {
                if (_exitWait < Constants.DefaultPostPause)
                {
                    ++_exitWait;
                }
                else
                {
                    _finished = true;
                }
                return true;
            }

            return false;
        }


        /// <summary>
        /// Passes the ball to the cursor by rolling it around the ground
        /// The ball's velocity at arrival is given by Constants.ShortPassDesiredVelocityAtArrival.
        /// </summary>
        private void KickShortPass(Ball ball, Vector3d target, double velocityAtArrival)
        {
            // NOTE: Works, but isn't optimized. Fix as necessary.

            // Take cursor location and obtain the vector pointing toward (relative to the position of the ball).
            Vector3d clickedVector = new Vector3d(target.X, target.Y, 0);
            Vector2d test0 = new Vector2d(target.X - ball.Position3d.X, target.Y - ball.Position3d.Y);

            // Proximity check.
            if (test0.Length() < Constants.TargetCheckTolerance)
            {
                // target too close. just nudge the ball.
                ball.Kick(new Vector3d(test0.X, test0.Y, 0), Constants.ShortPassDefaultSpin, _owner);
            }

            // Scale velocity vector
            test0.ScaleToLength(1);

            // NOTE:
            // The calibration works as follows:
            // We start with a velocity vector of magnitude 1 aimed in the correct direction.
            // We obtain the magnitude of the ball's velocity at arrival at the target (0 = didn't arrive).
            // We scale the velocity vector as necessary. The scaling is linear, defined by Constants.ShortPassCalibrationSensitivity.
            // If the target is too far, we use the Constants.ShortPassMaxVelocity loop stopping condition.
            // WARMING: This algo assumes the ShortPassDefaultSpin is 0. With spin, once has to alter the algo
            // in a way similar to the alteration for the Lob calibration.

            // Definet he calibrator ball and its helper variables, and do the first cycle of the loop
            BallCalibrator calibrator = new BallCalibrator(ball);
            calibrator.Kick(new Vector3d(test0.X, test0.Y, 0), Constants.ShortPassDefaultSpin);
            double testResult = calibrator.VelocityMagnitudeAtTarget(clickedVector);
            double difference = velocityAtArrival - testResult;
            double length = test0.Length();

            // Loop until the desired velocity has been found.
            while (difference > Constants.ShortPassDesiredVelocityAtArrivalTolerance &&
                length < Constants.ShortPassMaxVelocity)
            {
                calibrator.SpawnAt(new Vector2d(ball.Position3d.X, ball.Position3d.Y));
                length = test0.Length();
                test0.ScaleToLength(test0.Length() + Constants.ShortPassCalibrationSensitivity);
                calibrator.Kick(new Vector3d(test0.X, test0.Y, 0), Constants.ShortPassDefaultSpin);
                testResult = calibrator.VelocityMagnitudeAtTarget(clickedVector);
                difference = velocityAtArrival - testResult;
            }

            // Kick ball.
            _owner.KickBall( new Vector3d(test0.X, test0.Y, 0), Constants.ShortPassDefaultSpin);
        }

        /// <summary>
        /// Lobs the ball so it bounces at the cursor. Spin is defined by Constants.LobDefaultSpin.
        /// </summary>
        private void KickLob(Ball ball, Vector3d target, Vector3d spinner)
        {
            Vector3d spin = new Vector3d(0, spinner.Y, spinner.Z);
            //Vector3d spin = Constants.LobDefaultSpin;

            Vector3d clickedVector = new Vector3d(target.X, target.Y, 0);
            Vector2d test0 = new Vector2d(target.X - ball.Position3d.X, target.Y - ball.Position3d.Y);
            double distanceToTarget = test0.Length();

            // Proximity check.
            if (distanceToTarget < Constants.TargetCheckTolerance)
            {
                // target too close. just nudge the ball.
                ball.Kick(new Vector3d(test0.X, test0.Y, Constants.LobLiftCoefficient * test0.Length()), spin, _owner);
            }

            // Scale velocity vector
            test0.ScaleToLength(1);

            // NOTE:
            // The calibration works as follows:
            // We start with a velocity vector of magnitude 1 aimed toward the goal.
            // We obtain the position where the ball bounces.
            // We compare the distance to bounce with the distance to target.
            // We scale the velocity vector as necessary. The scaling is linear, defined by Constants.ShortPassCalibrationSensitivity.
            // If the target is too far, we use the Constants.ShortPassMaxVelocity loop stopping condition.
            // After we have the desired kick strength, we rotate the kick vector to align the bounce position with the target.

            // Definet he calibrator ball and its helper variables, and do the first cycle of the loop
            BallCalibrator calibrator = new BallCalibrator(ball);
            calibrator.Kick(new Vector3d(test0.X, test0.Y, Constants.LobLiftCoefficient * test0.Length()), spin);
            Vector3d landing = calibrator.BallPositionAtBounce();
            Vector3d vectorToLanding = landing - ball.Position3d;
            double distanceMagnitude = vectorToLanding.Length();

            double difference = distanceMagnitude - distanceToTarget;
            // Loop until the desired velocity has been found.
            while (Math.Abs(difference) > Constants.LobTargetCheckTolerance &&
                test0.Length() < Constants.LobMaxVelocity)
            {
                calibrator.SpawnAt(new Vector2d(ball.Position3d.X, ball.Position3d.Y));
                //length = difference.Length();
                test0.ScaleToLength(test0.Length() + Constants.LobCalibrationSensitivity);
                calibrator.Kick(new Vector3d(test0.X, test0.Y, Constants.LobLiftCoefficient * test0.Length()), spin);
                landing = calibrator.BallPositionAtBounce();
                vectorToLanding = landing - ball.Position3d;
                distanceMagnitude = vectorToLanding.Length();
                difference = distanceMagnitude - distanceToTarget;
            }

            // If necessary, rotate to align with target.
            if (spin.Z != 0)
            {
                double angleOfRotation = (new Vector2d(landing.X - ball.Position3d.X, landing.Y - ball.Position3d.Y)).
                AngleBetween(new Vector2d(clickedVector.X - ball.Position3d.X, clickedVector.Y - ball.Position3d.Y));
                test0 = test0.Rotate(angleOfRotation);
            }

            // Kick the ball.
            _owner.KickBall(new Vector3d(test0.X, test0.Y, Constants.LobLiftCoefficient * test0.Length()), spin);
        }

        public override void Update()
        {
            _owner.UpdateMovement();

            // see if owner is 'waiting' (i.e., in motion).
            bool waiting = Waiter();

            if (waiting)
            {
                return;
            }

            if (!_owner.BallInControlFacingRange())
            {
                ++_exitWait;
            }

            if (_owner.FacingDesiredDirection())
            {
                // facing correct direction; pass and exit
                switch (_typeOfPass)
                {
                    case PassType.Short:
                        this.KickShortPass(_owner.GameBall, new Vector3d(_target.X, _target.Y, 0), Constants.ShortPassDesiredVelocityAtArrival);
                        break;
                    case PassType.Lob:
                        this.KickLob(_owner.GameBall, new Vector3d(_target.X, _target.Y, 0), new Vector3d(0, 0, 0));
                        break;
                }

                ++_exitWait;
            }
        }

        public ActionPassToSpot(Footballer owner, Vector2d target, PassType typeOfPass)
            : base(owner)
        {
            _target = target;
            _typeOfPass = typeOfPass;

            // begin to turn to target.
            _owner.TurnToFace(target- _owner.PositionDouble);
        }
    }

    /// <summary>
    /// Takes a shot; for now implicitly at the left goal; later implicitly at the enemy goal.
    /// </summary>
    public class ActionTakeAShot : Action
    {

        private Int32 _entryWait = 0;
        private Int32 _exitWait = 0;

        /// <summary>
        /// Takes care of the pre/post waits. Returns true if currently waiting.
        /// </summary>
        private bool Waiter()
        {
            if (_entryWait < Constants.DefaultPreWait)
            {
                ++_entryWait;
                return true;
            }

            if (_exitWait > 0)
            {
                if (_exitWait < Constants.DefaultPostPause)
                {
                    ++_exitWait;
                }
                else
                {
                    _finished = true;
                }
                return true;
            }

            return false;
        }

        public override void Update()
        {
            _owner.UpdateMovement();

            // see if owner is 'waiting' (i.e., in motion).
            bool waiting = Waiter();

            if (waiting)
            {
                return;
            }

            // if ball has left control rage, stop.
            if (!_owner.BallInControlFacingRange())
            {
                _finished = true;
                return;
            }

            // if facing the goal, shoot
            Vector2d goal = new Vector2d(Constants.GoalLeft, 0.5*(Constants.GoalTop+Constants.GoalBottom));

            Vector2d vectorToGoal = goal - _owner.PositionDouble;

            double angle = _owner.FacingDirection.AngleBetween(vectorToGoal);
            angle = Math.Min(angle, 2 * Math.PI - angle);

            if (angle < Math.PI / 2)
            {
                // take the shot and wrap things up
                Vector3d shotVelocity = new Vector3d(vectorToGoal.X, vectorToGoal.Y, 50);
                shotVelocity.ScaleToLength(60);

                _owner.KickBall(shotVelocity, new Vector3d(0, 0, 0));
                ++_exitWait;
                //this._finished = true;
            }
        }

        public ActionTakeAShot(Footballer owner)
            : base(owner)
        {
            // turn to face goal
            //_owner.TurnToFace(new Vector2d(Constants.GoalLeft, Constants.GoalTop) - _owner.PositionDouble);
        }
    }

    /// <summary>
    /// Dribbles to target spot.
    /// </summary>
    public class ActionDribbleTo : Action
    {
        private Vector2d _target;

        private bool _doneDribbling = false;

        public override void Update()
        {
            _owner.UpdateMovement();

            if (_doneDribbling)
            {
                if (_owner.BallInControlRange())
                {
                    _owner.ControlBall();
                    _finished = true;
                    return;
                }
                else
                {
                    _owner.AdvanceTowardBallStoppingPlace();
                    return;
                }
            }


            if ((_owner.GameBall.Position3d.ProjectionXY() - _target).Length() < Constants.BallControlRadius) // && _owner.BallInDribblingRange())
            {
                // mission compelte; stop ball and exit
                _doneDribbling = true;
                return;
            }



            if (_owner.BallInControlRange())
            {
                // turn to ball and prod ball onward
                if (!_owner.FacingBallStoppingPlace())
                {
                    _owner.TurnToFaceBallStoppingPlace();
                    return;
                }

                Vector2d kickDir = _target - _owner.PositionDouble;
                kickDir.ScaleToLength(20);
                _owner.KickBall(new Vector3d(kickDir.X,
                    kickDir.Y, 0), new Vector3d(0, 0, 0));

                //_owner.AdvanceInDirection(kickDir);

                _owner.AdvanceTowardBallStoppingPlace();
                return;
            }

            if (_owner.BallInDribblingRange())
            {
                _owner.AdvanceTowardBallStoppingPlace();

                /*
                // ball is not near enough to prod, but is near enough to chase
                if (!_owner.FacingBall())
                {
                    _owner.AdvanceTowardBall();
                }

                _owner.Advance();*/
                return;
            }

            // ball has eluded the player
            this._finished = true;
        
        }

        public ActionDribbleTo(Footballer owner, Vector2d target) : base(owner)
        {
            _target = target;
        }
    }

    /// <summary>
    /// Anticipates ball and controls it if passes by.
    /// </summary>
    public class ActionAnticipate : Action
    {
        private Int32 _counter;

        public override void Update()
        {
            _owner.UpdateMovement();

            ++_counter;

            if (_owner.BallInControlRange())
            {
                _owner.ControlBall();
                this._finished = true;
                return;
            }

            if (_counter > 10)
            {
                this._finished = true;
                return;
            }
        }

        public ActionAnticipate(Footballer owner)
            : base(owner)
        {
            _owner.Stop();
        }
    }

    /// <summary>
    /// Makes a challenge in the given direction.
    /// </summary>
    public class ActionChallenge : Action
    {
        private Vector2d _direction;

        private Int32 _entryWait = 0;
        private Int32 _exitWait = 0;

        /// <summary>
        /// Takes care of the pre/post waits. Returns true if currently waiting.
        /// </summary>
        private bool Waiter()
        {
            if (_entryWait < Constants.DefaultPreWait/2)
            {
                ++_entryWait;
                return true;
            }

            if (_exitWait > 0)
            {
                if (_exitWait < Constants.DefaultPostPause/2)
                {
                    ++_exitWait;
                }
                else
                {
                    _finished = true;
                }
                return true;
            }

            return false;
        }

        public override void Update()
        {
            _owner.UpdateMovement();

            // see if owner is 'waiting' (i.e., in motion).
            bool waiting = Waiter();

            if (waiting)
            {
                return;
            }

            // Action time - check if ball is in range
            ++_exitWait;

            if(!_owner.BallInControlRange())
            {
                // ball not in range - we're done
                return;
            }

            // Ball is in range - prod it in _direction
            _direction.ScaleToLength(40);
            _owner.KickBall(new Vector3d(_direction.X, _direction.Y, 0), new Vector3d(0,0,0));
        }

        public ActionChallenge(Footballer owner, Vector2d direction)
            : base(owner)
        {
            _direction = direction;
        }
    }

    /// <summary>
    /// Player stumbles (and so is inactive for a brief time).
    /// </summary>
    public class ActionStumble : Action
    {

        private Int32 _entryWait = 0;
        private Int32 _exitWait = 0;

        /// <summary>
        /// Takes care of the pre/post waits. Returns true if currently waiting.
        /// </summary>
        private bool Waiter()
        {
            if (_entryWait < Constants.DefaultPreWait)
            {
                ++_entryWait;
                return true;
            }

            if (_exitWait > 0)
            {
                if (_exitWait < Constants.DefaultPostPause)
                {
                    ++_exitWait;
                }
                else
                {
                    _finished = true;
                }
                return true;
            }

            return false;
        }

        public override void Update()
        {
            _owner.UpdateMovement();

            Waiter();
        }

        public ActionStumble(Footballer owner)
            : base(owner)
        {
        }
    }

    public class ActionGoto : Action
    {
        private Vector2d _target;
        private Int32 _counter;

        public override void Update()
        {
            _owner.UpdateMovement();
            ++_counter;

            if (_owner.IsNear(_target))
            {
                _finished = true;
                return;
            }

            if (_counter > 10)
            {
                _finished = true;
                return;
            }
        }

        public override string ToString()
        {
            return this.GetType().ToString() + _target;
        }

        public ActionGoto(Footballer owner, Vector2d target) : base(owner)
        {
            _target = target;
            _owner.MoveTo(_target);
        }
    }

    public class ActionWait : Action
    {
        UInt16 _timer = 0;
        UInt16 _releaseTime;

        public override void Update()
        {
            _owner.UpdateMovement();

            ++_timer;

            if (_timer >= _releaseTime)
            {
                _finished = true;
            }
        }

        public ActionWait(Footballer owner, UInt16 releaseTime)
            : base(owner)
        {
            _releaseTime = releaseTime;
        }
    }
}