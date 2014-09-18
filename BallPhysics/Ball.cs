using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallPhysics
{
    /// <summary>
    /// The game ball.
    /// </summary>
    public class Ball
    {
        private Map _mapReference;
        public Map MapReference
        {
            get
            {
                return this._mapReference;
            }
            set
            {
                this._mapReference = value;
            }
        }

        protected Vector3d _position3dOld;
        public Vector3d Position3dOld
        {
            get
            {
                return _position3dOld;
            }
            set
            {
                _position3dOld = value;
            }
        }

        protected Vector3d _position3d;
        public Vector3d Position3d
        {
            get
            {
                return _position3d;
            }
            set
            {
                _position3d = value;
            }
        }

        protected Vector3d _velocity;
        public Vector3d Velocity
        {
            get
            {
                return _velocity;
            }
        }

        /// <summary>
        /// The spin gives you the axis of rotation. The left/right bends are on the Z-axis.
        /// The Y-axis gives you top-spins and slices. X-axis gives bullet spin? Don't
        /// know how it translates (or if it is physically attainable) in real football.
        /// </summary>
        protected Vector3d _spin3d;
        public Vector3d Spin3d
        {
            get
            {
                return _spin3d;
            }
            set
            {
                _spin3d = value;
            }
        }

        private List<Vector3d> _projection = new List<Vector3d>();
        public List<Vector3d> Projection
        {
            get
            {
                return _projection;
            }
            set
            {
                _projection = value;
            }
        }

        private Team _lastTouch;
        public Team LastTouch
        {
            get
            {
                return _lastTouch;
            }
            set
            {
                _lastTouch = value;
            }
        }

        /// <summary>
        /// Projects the ball's travel path until stopping.
        /// </summary>
        private void ConstructProjection()
        {
            // clear old projection
            _projection.Clear();

            BallCalibrator calibrator = new BallCalibrator(this);
            calibrator.Kick(this._velocity, new Vector3d(0,0,0));
            calibrator.Spin3d = _spin3d; // to bypass the spin realignment. do this more cleverly later.

            int j = 0;

            while (calibrator.Velocity.Length() > 0 && j<Constants.ProjectionMaxLength)
            { 
                calibrator.UpdateMotion3D();

                _projection.Add(calibrator.Position3d);
                ++j;
            }

            _projection.Reverse();
        }

        /// <summary>
        /// Returns true if the segment interesects the circle.
        /// Used for goal-frame collision detection.
        /// </summary>
        private bool SegmentToCircleCollision(Vector2d ptBtoPtAVector, Vector2d circleCentertoPtAVector, double radius)
        {
            // Helpful link: http://doswa.com/blog/2009/07/13/circle-segment-intersectioncollision/
            // Project the to-center vector on the segment vector:
            double segmentLength = ptBtoPtAVector.Length();
            
            double projectionScalar = (circleCentertoPtAVector.X * ptBtoPtAVector.X + circleCentertoPtAVector.Y * ptBtoPtAVector.Y) / segmentLength;

            Vector2d nearestPoint;

            if (projectionScalar < 0)
            {
                // nearest point to circle is origin
                nearestPoint = new Vector2d(0, 0);
            }
            else if (projectionScalar > segmentLength)
            {
                // nearest point to circle is segment endpoint
                nearestPoint = ptBtoPtAVector;
            }
            else
            {
                // nearest point is the projection
                nearestPoint =  new Vector2d(ptBtoPtAVector.X / segmentLength, ptBtoPtAVector.Y / segmentLength);
                nearestPoint.ScaleByFactor(projectionScalar);
            }

            return (nearestPoint - circleCentertoPtAVector).Length() < radius;
        }

        /// <summary>
        /// Corrects ball position if it is out of bounds.
        /// In the XY plane the ball bounces off the wall.
        /// Along the Z axis, it bounces off the ground. Gravity takes care of the upper bound.
        /// </summary>
        protected virtual Vector3d BoundsCorrection3d(Vector3d correctMe)
        {
            double xval = correctMe.X;
            double yval = correctMe.Y;
            double zval = correctMe.Z;

            #region Goal frame collision detection

            // NOTE: these should not be done unless the ball actually is near the frame of goal.

            // uprights
            for (int i = 0; i < 4; ++i)
            {
                Vector2d positionXY = _position3d.ProjectionXY();
                Vector2d toPost = Constants.Uprights[i] - positionXY;
                Vector2d delta = correctMe.ProjectionXY() - positionXY;
                if (this.SegmentToCircleCollision(delta, toPost,  (Constants.BallRadius + Constants.PostRadius))
                    && (_position3d.Z < Constants.GoalHeight))
                {
                    // collision with the post
                    // the axis of reflection is given by the line passing through the center of the ball and center of the post at the moment of collision.
                    Vector2d velocityXY = _velocity.ProjectionXY();
                    double alpha = velocityXY.AngleBetween(Constants.Uprights[i] - positionXY);
                    velocityXY = velocityXY.Rotate(Math.PI + alpha);
                    _velocity.X = velocityXY.X;
                    _velocity.Y = velocityXY.Y;

                    delta = delta.Rotate(Math.PI + alpha);
                    xval += delta.X;
                    yval += delta.Y;
                }
            }

            // Crossbars
            for (int j = 0; j < 2; ++j)
            {
                //Vector2d toCrossbar = Constants.Crossbars[j] -correctMe.ProjectionXZ();

                Vector2d positionXZ = _position3d.ProjectionXZ();
                Vector2d toCrossbar = Constants.Crossbars[j] - positionXZ;
                Vector2d delta = correctMe.ProjectionXZ() - positionXZ;

                if (this.SegmentToCircleCollision(delta, toCrossbar, (Constants.BallRadius + Constants.PostRadius))
                    && (_position3d.Y < Constants.GoalBottom) && (_position3d.Y > Constants.GoalTop))
                {
                    // collision with the post
                    // the axis of reflection is given by the line passing through the center of the ball and center of the post at the moment of collision.
                    Vector2d velocityXZ = _velocity.ProjectionXZ();
                    double alpha = velocityXZ.AngleBetween(Constants.Crossbars[j] - positionXZ);
                    velocityXZ = velocityXZ.Rotate(Math.PI + alpha);
                    _velocity.X = velocityXZ.X;
                    _velocity.Z = velocityXZ.Y;

                    delta = delta.Rotate(Math.PI + alpha);
                    xval += delta.X;
                    zval += delta.Y;
                }
            }

            #endregion

            #region Net collision detection
            // x-axis left:
            if (_position3d.Z <= Constants.GoalHeight && _position3d.Y >= Constants.GoalTop && _position3d.Y <= Constants.GoalBottom)
            {
                if (_position3d.X >= Constants.NetsBackLeft && correctMe.X < Constants.NetsBackLeft)
                {
                    // this is INSIDE the net
                    // kill x-velocity
                    _velocity.X *= (-Constants.BouncingNetConstant);
                    xval = Constants.NetsBackLeft + Constants.BallRadius;
                }
                else if (_position3d.X < Constants.NetsBackLeft && correctMe.X >= Constants.NetsBackLeft)
                {
                    //outside
                    _velocity.X *= (-Constants.BouncingNetConstant);
                    xval = Constants.NetsBackLeft - Constants.BallRadius;
                }
            }

            // x-axis right:
            if (_position3d.Z <= Constants.GoalHeight && _position3d.Y >= Constants.GoalTop && _position3d.Y <= Constants.GoalBottom)
            {
                if (_position3d.X > Constants.NetsBackRight && correctMe.X <= Constants.NetsBackRight)
                {
                    // outside of net
                    // kill x-velocity
                    _velocity.X *= (-Constants.BouncingNetConstant);
                    xval = Constants.NetsBackRight + Constants.BallRadius;
                }
                else if (_position3d.X <= Constants.NetsBackRight && correctMe.X > Constants.NetsBackRight)
                {
                    //inside
                    _velocity.X *= (-Constants.BouncingNetConstant);
                    xval = Constants.NetsBackRight - Constants.BallRadius;
                }
            }

            // y-axis top :
            if (
                _position3d.Z <= Constants.GoalHeight && ((_position3d.X >= Constants.NetsBackLeft && _position3d.X <= Constants.GoalLeft)
                || (_position3d.X <= Constants.NetsBackRight && _position3d.X >= Constants.GoalRight))
                )
            {
                if (_position3d.Y > Constants.GoalTop && correctMe.Y <= Constants.GoalTop)
                {
                    // outside
                    _velocity.Y *= (-Constants.BouncingNetConstant);
                    yval = Constants.GoalTop+ Constants.BallRadius;
                }
                else if (_position3d.Y <= Constants.GoalTop && correctMe.Y > Constants.GoalTop)
                {
                    //inside 
                    _velocity.Y *= (-Constants.BouncingNetConstant);
                    yval = Constants.GoalTop - Constants.BallRadius;
                }
            }

            // y-axis bottom :
            if (
                _position3d.Z <= Constants.GoalHeight && ((_position3d.X >= Constants.NetsBackLeft && _position3d.X <= Constants.GoalLeft)
                || (_position3d.X <= Constants.NetsBackRight && _position3d.X >= Constants.GoalRight))
                &&
                ((_position3d.Y >= Constants.GoalBottom && correctMe.Y < Constants.GoalBottom) ||
                (_position3d.Y <= Constants.GoalBottom && correctMe.Y > Constants.GoalBottom))
                )
            {
                if (_position3d.Y < Constants.GoalBottom && correctMe.Y >= Constants.GoalBottom)
                {
                    // outside
                    _velocity.Y *= (-Constants.BouncingNetConstant);
                    yval = Constants.GoalBottom - Constants.BallRadius;
                }
                else if (_position3d.Y >= Constants.GoalBottom && correctMe.Y < Constants.GoalBottom)
                {
                    //inside 
                    _velocity.Y *= (-Constants.BouncingNetConstant);
                    yval = Constants.GoalBottom + Constants.BallRadius;
                }
            }

            // z-axis
            if (
                (_position3d.Y >= Constants.GoalTop && _position3d.Y <= Constants.GoalBottom) && 
                ((_position3d.X >= Constants.NetsBackLeft && _position3d.X <= Constants.GoalLeft)
                || (_position3d.X <= Constants.NetsBackRight && _position3d.X >= Constants.GoalRight))
                )
            {
                if (_position3d.Z > Constants.GoalHeight && correctMe.Z <= Constants.GoalHeight)
                {
                    // outside
                    _velocity.Z *= (-Constants.BouncingNetConstant);
                    zval = Constants.GoalHeight + Constants.BallRadius;
                }
                else if (_position3d.Z <= Constants.GoalHeight && correctMe.Z > Constants.GoalHeight)
                {
                    // inside
                    _velocity.Z *= (-Constants.BouncingNetConstant);
                    zval = Constants.GoalHeight - Constants.BallRadius;
                }

            }

            #endregion

            /*
            if (correctMe.X < 0)
            {
                xval = -correctMe.X;
                //flip x-dir velocity
                _velocity.X = -_velocity.X;
                _spin3d.X = -_spin3d.X;
            }
            else if (correctMe.X > Constants.ActualXMax)
            {
                xval = 2 * Constants.ActualXMax - correctMe.X;
                //flip x-dir velocity
                _velocity.X = -_velocity.X;
                _spin3d.X = -_spin3d.X;
            }

            if (correctMe.Y < 0)
            {
                yval = -correctMe.Y;
                //flip y-dir velocity
                _velocity.Y = -_velocity.Y;
                //_spin3d.Y = -_spin3d.Y;
            }
            else if (correctMe.Y > Constants.ActualYMax)
            {
                yval = 2 * Constants.ActualYMax - correctMe.Y;
                //flip y-dir velocity
                _velocity.Y = -_velocity.Y;
                //_spin3d.Y = -_spin3d.Y;
            }
            */
            if (zval < 0)
            {
                // negative zvalue. Bounce the ball. 
                zval = -zval;
                // flip z-velocity
                _velocity.Z = -_velocity.Z * Constants.BouncingGrassConstant;
                // correct spin
                _spin3d.ScaleByFactor(Constants.BouncingSpinSoak);

                // check to see if bouncing threshold is attained
                if (_velocity.Z < Constants.GravityDeceleration)
                {
                    // rest the ball
                    zval = 0;
                    _velocity.Z = 0;
                    _spin3d.Z = 0;
                }
            }

            return new Vector3d(xval, yval, zval);
        }

        /// <summary>
        /// Updates the ball's position according to the current velocity. Checks for collisions.
        /// </summary>
        public void UpdateMotion3D()
        {
            _position3dOld = _position3d;

            if (_velocity.Length() == 0 && _position3d.Z == 0)
            {
                return;
            }

            // --- Update 3d position ---
            this._position3d = this.BoundsCorrection3d(this._position3d + _velocity);

            // --- Apply spinning ---
            // Getting this to work was a bit tricky. See reference:
            // http://en.wikipedia.org/wiki/Magnus_effect
            
            if (_spin3d.Length() > 0 && _velocity.Length() > 0)
            {
                Vector3d spinEffect = _spin3d.CrossProductWith(_velocity);
                Vector3d spinEffectCorrected = spinEffect;
                spinEffectCorrected.ScaleByFactor(Math.Pow(Math.Max(_velocity.Length(), _spin3d.Length()), -1));

                _velocity += spinEffectCorrected;
            }

            // -- Update velocity vector ---
            if (_position3d.Z == 0)
            {
                // ball on the ground
                Vector2d velocityXY = new Vector2d(_velocity.X, _velocity.Y);
                velocityXY.ScaleToLength(Math.Max(velocityXY.Length() - Constants.FrictionGrass, 0));
                _velocity.X = velocityXY.X;
                _velocity.Y = velocityXY.Y;
            }
            else
            {
                // ball is in the air
                _velocity.ScaleToLength(Math.Max(_velocity.Length() - Constants.FrictionAIr, 0));
            }

            if (this._position3d.Z > 0 || Math.Abs(_velocity.Z) > 0)
            {
                // We're in the air or bouncing. Update for gravity effects.
                _velocity.Z -= Constants.GravityDeceleration;
            }

            // --- Update spin vector ---
            if (_spin3d.Length() > 0)
            {
                if (this._position3d.Z > 0)
                {
                    // ball in the air
                    _spin3d.ScaleToLength(_spin3d.Length() - Constants.SpinDecayAir);
                }
                else
                {
                    // ball on the grass
                    _spin3d.ScaleByFactor(_spin3d.Length() - Constants.SpinDecayGrass);
                }

                if (_spin3d.Length() < Constants.SpinMinimalThreshold)
                {
                    // spin is so small we zero it out
                    _spin3d.ScaleByFactor(0);
                }
            }

            // --- Update projection ---
            if (_projection.Count > 0)
            {                  
                _projection.RemoveAt(_projection.Count - 1);
            }
        }

        /// <summary>
        /// Gives the ball a velocity vector and a spin vector.
        /// </summary>
        public virtual void Kick(Vector3d velocity, Vector3d spin, Footballer kicker)
        {
            if (kicker != null)
            {
                _lastTouch = kicker.Team;
            }

            this._velocity = velocity;
            this._spin3d = spin;
            if (spin.Length() > 0 && (spin.X != 0 || spin.Y != 0) && (velocity.X != 0 || velocity.Y !=0))
            {
                this.RealignSpin();
            }

            ConstructProjection();
        }

        /// <summary>
        /// Allings the spin vectors with the velocity vector.
        /// </summary>
        protected void RealignSpin()
        {
            Vector2d spinXY = new Vector2d(_spin3d.X, _spin3d.Y);
            Vector2d velocity2d = new Vector2d(_velocity.X, _velocity.Y);
            double alpha = (new Vector2d(1, 0)).AngleBetween(velocity2d);
            Vector2d spinner = spinXY.Rotate(alpha);
            this._spin3d = new Vector3d(spinner.X, spinner.Y, _spin3d.Z);
        }

        /// <summary>
        /// Halts the ball.
        /// </summary>
        public void Stop()
        {
            this._velocity.ScaleByFactor(0);
            this._position3d.Z = 0;
            this._spin3d.ScaleByFactor(0);
        }

        /// <summary>
        /// Spawns the ball at its start position, in rest.
        /// </summary>
        public void Reset()
        {
            this._position3d = new Vector3d(0.5 * Constants.ActualXMax, 0.5 * Constants.ActualYMax, 0);
            this.Stop();
        }

        /// <summary>
        /// Spawns the ball, at rest, at the given position.
        /// </summary>
        public void SpawnAt(Vector2d place)
        {
            this.Stop();
            this.Position3d = new Vector3d(place.X, place.Y, 0);
            this.ConstructProjection();
        }

        private Ball()
        {
            this.Position3d = new Vector3d(0.5 * Constants.ActualXMax, 0.5 * Constants.ActualYMax, 0);
        }

        public Ball(Map mapRef) : this()
        {
            this._mapReference = mapRef;
        }
    }

    /// <summary>
    /// Used to display the flight trajectory in Painter.
    /// </summary>
    public class BallGhost : Ball
    {
        public void SetParameters(Vector3d position, Vector3d velocity, Vector3d spin)
        {
            this._position3d = position;
            this._velocity = velocity;
            this._spin3d = spin;
        }

        public void Kick(Vector3d velocity, Vector3d spin)
        {
            this._velocity = velocity;
            this._spin3d = spin;
            if (spin.Length() > 0 && (spin.X != 0 || spin.Y != 0) && (velocity.X != 0 || velocity.Y != 0))
            {
                base.RealignSpin();
            }
        }

        public BallGhost(Ball original): base(original.MapReference)
        {
            this.Position3d = original.Position3d;
            this._spin3d = original.Spin3d;
            this._velocity = original.Velocity;
        }
    }

    /// <summary>
    /// Used to calibrate ball kicking.
    /// </summary>
    public class BallCalibrator : Ball
    {
        public Vector3d BallPositionAtBounce()
        {
            bool stopAtBounce = true;
            while (this.Velocity.Length() > 0 || this._position3d.Z != 0)
            {
                this.UpdateMotion3D(stopAtBounce);
            }

            return this._position3d;
        }

        public double VelocityMagnitudeAtTarget(Vector3d target)
        {
            while 
                (
                ((this._position3d - target).Length() > Constants.TargetCheckTolerance) 
                && 
                (this._velocity.Length() > 0)
                )
            {
                this.UpdateMotion3D(false);
            }

            if (_velocity.Length() > 0)
            {

            }

            return this._velocity.Length();
        }

        public void Kick(Vector3d velocity, Vector3d spin)
        {
            this._velocity = velocity;
            this._spin3d = spin;
            if (spin.Length() > 0 && (spin.X != 0 || spin.Y != 0) && (velocity.X != 0 || velocity.Y != 0))
            {
                base.RealignSpin();
            }
        }

        /// <summary>
        /// Corrects ball position if it is out of bounds.
        /// In the XY plane the ball bounces off the wall.
        /// Along the Z axis, it bounces off the ground. Gravity takes care of the upper bound.
        /// </summary>
        private Vector3d BoundsCorrection3d(Vector3d correctMe, bool stopAtBounce)
        {
            double xval = correctMe.X;
            double yval = correctMe.Y;
            double zval = correctMe.Z;
            
            // No off-the-wall bounces!

            // Z-axis check
            if (zval < 0)
            {
                // negative zvalue. Bounce the ball. 
                zval = -zval;
                // flip z-velocity
                _velocity.Z = -_velocity.Z * Constants.BouncingGrassConstant;
                // correct spin
                _spin3d.ScaleByFactor(Constants.BouncingSpinSoak);

                if (stopAtBounce)
                {
                    // halt the ball
                    this.Stop();
                }

                // check to see if bouncing threshold is attained
                if (_velocity.Z < Constants.GravityDeceleration)
                {
                    // rest the ball
                    zval = 0;
                    this.Stop();
                }
            }

            return new Vector3d(xval, yval, zval);
        }

        private void UpdateMotion3D(bool stopAtBounce)
        {
            if (_velocity.Length() == 0 && _position3d.Z == 0)
            {
                return;
            }

            // --- Update 3d position ---
            this._position3d = this.BoundsCorrection3d(this._position3d + _velocity, stopAtBounce);

            // --- Apply spinning ---
            // Getting this to work was a bit tricky. See reference:
            // http://en.wikipedia.org/wiki/Magnus_effect

            if (_spin3d.Length() > 0 && _velocity.Length() > 0)
            {
                Vector3d spinEffect = _spin3d.CrossProductWith(_velocity);
                Vector3d spinEffectCorrected = spinEffect;
                spinEffectCorrected.ScaleByFactor(Math.Pow(Math.Max(_velocity.Length(), _spin3d.Length()), -1));

                _velocity += spinEffectCorrected;
            }

            // -- Update velocity vector ---
            if (_position3d.Z == 0)
            {
                // ball on the ground
                Vector2d velocityXY = new Vector2d(_velocity.X, _velocity.Y);
                velocityXY.ScaleToLength(Math.Max(velocityXY.Length() - Constants.FrictionGrass, 0));
                _velocity.X = velocityXY.X;
                _velocity.Y = velocityXY.Y;
            }
            else
            {
                // ball is in the air
                _velocity.ScaleToLength(Math.Max(_velocity.Length() - Constants.FrictionAIr, 0));
            }

            if (this._position3d.Z > 0 || Math.Abs(_velocity.Z) > 0)
            {
                // We're in the air or bouncing. Update for gravity effects.
                _velocity.Z -= Constants.GravityDeceleration;
            }

            // --- Update spin vector ---
            if (this._position3d.Z > 0)
            {
                // ball in the air
                _spin3d.ScaleToLength(_spin3d.Length() - Constants.SpinDecayAir);
            }
            else
            {
                // ball on the grass
                _spin3d.ScaleByFactor(_spin3d.Length() - Constants.SpinDecayGrass);
            }

            if (_spin3d.Length() < Constants.SpinMinimalThreshold)
            {
                // spin is so small we zero it out
                _spin3d.ScaleByFactor(0);
            }
        }

        public BallCalibrator(Ball original) : base(original.MapReference)
        {
            this.Position3d = original.Position3d;
            this._spin3d = original.Spin3d;
            this._velocity = original.Velocity;
        }
    }
}
