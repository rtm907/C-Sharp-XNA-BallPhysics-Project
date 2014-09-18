using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BallPhysics
{
    // Form class. Deals with the window the game plays in, takes input, etc.
    // Should move some of the code here to a 'Game' class
    public partial class MainFrame : Form
    {
        #region Fields

        // Framework
        private Scheduler _scheduler;
        private Ledger _ledger;
        private Painter _painter;

        // Display / Form
        private PointF _screenAnchor;
        private Timer _timer;
        private UInt16 _invalidateCounter = 0;
        private float _zoom;
        private UInt64 _tickCounter = 0;

        // Map related
        private Map _currentMap;
        private Ball _ball;

        // Interface
        protected bool MouseRightButtonIsDown;
        private List<Keys> _pressedKeys = new List<Keys>();
        private KickMode _kickMode = KickMode.Free;
        public KickMode ModeOfKicking
        {
            get
            {
                return _kickMode;
            }
        }
        private double _heightAccumulator = 0;
        private double _spinY = 0;
        public double SpinY
        {
            get
            {
                return _spinY;
            }
        }
        private double _spinZ = 0;
        public double SpinZ
        {
            get
            {
                return _spinZ;
            }
        }
        private double _shotPower = 40d;
        public double ShotPower
        {
            get
            {
                return _shotPower;
            }
        }


        #endregion

        #region Form logic

        public MainFrame()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // reduce flicker
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            this.MouseWheel += new MouseEventHandler(MainFrame_MouseWheel);

            this.MouseRightButtonIsDown = false;
        }

        private void MainFrame_Load(object sender, System.EventArgs e)
        {
            _timer = new Timer();
            _timer.Interval = Constants.defaultTimerPeriod;
            _timer.Tick += new System.EventHandler(MainFrame_Tick);

            _zoom = Constants.ZoomDefault;

            //WorldGeneration generator = new WorldGeneration(907);

            _currentMap = new Map(907);
            _painter = new Painter(_currentMap, this, _zoom);

            _ball = new Ball(_currentMap);
            _currentMap.MatchOnMap = new Match(_ball);
            _currentMap.BallReference = _ball;
            _currentMap.MatchOnMap.TeamLeft = _currentMap.SpawnTeam(true, Color.Blue);
            _currentMap.MatchOnMap.TeamRight = _currentMap.SpawnTeam(false, Color.Red);
            _currentMap.MatchOnMap.KickOff(true);

            _screenAnchor = this.TransformCenterAtBall();
            
            //_currentMap.SpawnFootballer(new Vector2d(500, 500), Constants.DefaultPositions[2], FootballerTeam.TeamA);
            //_currentMap.SpawnFootballer(new Vector2d(900, 2000), Constants.DefaultPositions[3], FootballerTeam.TeamA);
            //_currentMap.SpawnFootballer(new Vector2d(1300, 1250), Constants.DefaultPositions[4], FootballerTeam.TeamA);
            



            _scheduler = new Scheduler(_currentMap);
            _ledger = new Ledger(_scheduler);
            _timer.Start();
        }

        private void MainFrame_Paint(object sender, PaintEventArgs e)
        {

            Graphics graphicsObj = e.Graphics;

            if (Constants.Scrolling == ScrollingType.Ball)
            {
                _screenAnchor = this.TransformCenterAtBall();
            }

            graphicsObj.TranslateTransform(_screenAnchor.X, _screenAnchor.Y);

            // Find the coords of the tile lying under the (0,0) Form pixel
            Coords topLeft = TransformInverseScreenpointToCoords(0, 0);
            // Bounds correction:
            topLeft = new Coords(Math.Max(0, topLeft.X), Math.Max(0, topLeft.Y));

            Coords bottomRight = TransformInverseScreenpointToCoords(this.Width, this.Height);
            // Bounds correction:
            bottomRight = new Coords(Math.Min(bottomRight.X, _currentMap.BoundX - 1),
                Math.Min(bottomRight.Y, _currentMap.BoundY - 1));

            this._currentMap.MyPainter.Paint(graphicsObj, topLeft, bottomRight);

            //graphicsObj.Dispose();
        }

        private void MainFrame_Tick(object source, EventArgs e)
        {
             if (_pressedKeys.Count > 0)
            {
                this.HandlerKeysHeld();
            }

            if (MouseRightButtonIsDown)
            {
                this.HandlerMouseRightButtonHeld();
            }

            this._scheduler.Update();

            this._invalidateCounter = (UInt16)((_invalidateCounter + Constants.defaultTimerPeriod) % Constants.redrawPeriod);
            if (_invalidateCounter <= Constants.defaultTimerPeriod)
            {
                this.Invalidate();
            }

            ++this._tickCounter;
        }

        #endregion

        #region Display / Game-logic coordinate transforms

        // Returns the transform that centers the form on the player
        private PointF TransformCenterAtBall()
        {
            //PointF topDownPoint = new PointF(_ball.PositionPixel.X * this._zoom, this._ball.PositionPixel.Y * this._zoom);
            return new PointF((float)(0.5f * this.Width - _ball.Position3d.X * this._zoom), (float)(0.5f * this.Height - _ball.Position3d.Y * this._zoom));
            //return new PointF();
        }

        private Vector2d TransformInverseScreenpointToVector(Int32 x, Int32 y)
        {
            // maybe should store the translation instead of recalcualting it
            float xf = (x - _screenAnchor.X) / this._zoom;
            float yf = (y - _screenAnchor.Y) / this._zoom;

            //Int32 xcoord = (Int32)(Math.Floor(xf));
            //Int32 ycoord = (Int32)(Math.Floor(yf));

            return new Vector2d(xf, yf);
        }

        // Returns the coords of the clicked tile assuming the center of the screen is the 
        // player's position
        private Coords TransformInverseScreenpointToCoords(Int32 x, Int32 y)
        {
            // maybe should store the translation instead of recalcualting it
            float xf = (x - _screenAnchor.X) / this._zoom;
            float yf = (y - _screenAnchor.Y) / this._zoom;

            Int32 xcoord = (Int32)(Math.Floor(xf));
            Int32 ycoord = (Int32)(Math.Floor(yf));

            return new Coords(xcoord, ycoord);
        }

        private Coords PixelUnderMousecursor()
        {
            Point mousePosition = this.PointToClient(Cursor.Position);
            //return TransformInverseScreenpointToCoords(mousePosition.X, mousePosition.Y);
            //Vector pointOnDisplay = TransformInverseScreenpointToVector(mousePosition.X, mousePosition.Y);
            Coords pointOnDisplay = TransformInverseScreenpointToCoords(mousePosition.X, mousePosition.Y);

            Coords returnVal = new Coords(StaticMathFunctions.DisplayPointToSpacePoint(pointOnDisplay));
            return new Coords(StaticMathFunctions.DisplayPointToSpacePoint(pointOnDisplay));

        }

        #endregion

        #region User Input

        private void MainFrame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                this.HandlerPauseUnpause();
                return;
            }

            if (!_pressedKeys.Contains(e.KeyCode))
            {
                _pressedKeys.Add(e.KeyCode);
            }

            e.Handled = true;
        }

        private void MainFrame_KeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.KeyCode);
            e.Handled = true;
        }

        private void MainFrame_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.MouseRightButtonIsDown = true;
            }
        }

        private void MainFrame_MouseUp(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                HandlerMouseLeftClick();
            }

            if (e.Button == MouseButtons.Right)
            {
                this.MouseRightButtonIsDown = false;

                HandlerMouseRightClick();
            }
            // Add button-click contextual handling here.
        }

        private void MainFrame_MouseWheel(object sender, MouseEventArgs e)
        {
            this.HandlerMousewheel(e);
        }

        private void HandlerKeysHeld()
        {
            foreach (Keys key in _pressedKeys)
            {
                switch (key)
                {
                    case Constants.KickModeFree:
                        this._kickMode = KickMode.Free;
                        break;
                    case Constants.KickModePass:
                        this._kickMode = KickMode.Pass;
                        break;
                    case Constants.KickModeLob:
                        this._kickMode = KickMode.Lob;
                        break;

                    case Constants.SpinYAdd:
                        this._spinY += Constants.SpinDelta;
                        break;
                    case Constants.SpinYSub:
                        this._spinY -= Constants.SpinDelta;
                        break;
                    case Constants.SpinZAdd:
                        this._spinZ += Constants.SpinDelta;
                        break;
                    case Constants.SpinZSub:
                        this._spinZ -= Constants.SpinDelta;
                        break;

                    case Constants.ShotPowerAdd:
                        this._shotPower += Constants.ShotPowerDelta;
                        break;
                    case Constants.ShotPowerSub:
                        this._shotPower = Math.Max(0, this._shotPower - Constants.ShotPowerDelta);
                        break;
                }


                if (Constants.Scrolling == ScrollingType.Free)
                {
                    switch (key)
                    {
                        case Keys.Right:
                            _screenAnchor = new PointF(Math.Max(-this._currentMap.PixelBoundX * this._zoom + this.Width, _screenAnchor.X - Constants.FreeScrollingSpeed), _screenAnchor.Y);
                            break;
                        case Keys.Left:
                            _screenAnchor = new PointF(Math.Min(0, _screenAnchor.X + Constants.FreeScrollingSpeed), _screenAnchor.Y);
                            break;
                        case Keys.Down:
                            _screenAnchor = new PointF(_screenAnchor.X, Math.Max(-this._currentMap.PixelBoundY * this._zoom + this.Height, _screenAnchor.Y - Constants.FreeScrollingSpeed));
                            break;
                        case Keys.Up:
                            _screenAnchor = new PointF(_screenAnchor.X, Math.Min(0, _screenAnchor.Y + Constants.FreeScrollingSpeed));
                            break;
                    }
                }
            }
        }

        private void HandlerMouseRightButtonHeld()
        {
            _heightAccumulator += 4d;

            if (Constants.GhostBallAllowed && this._kickMode == KickMode.Free)
            {
                if (_painter.Ghost == null)
                {
                    _painter.Ghost = new BallGhost(_ball);
                }

                BallGhost ghost = _painter.Ghost;
                ghost.Position3d = _ball.Position3d;
                this.KickBallTowardMouse(ghost);
            }
        }

        private void HandlerPauseUnpause()
        {
            this._timer.Enabled = (!this._timer.Enabled);
            //this._ball.Stop();
        }

        private void HandlerMousewheel(MouseEventArgs e)
        {
            if (Constants.ZoomingAllowed)
            {
                this._zoom = Math.Min(Math.Max(Constants.ZoomMin, _zoom + e.Delta * Constants.ZoomSpeed), Constants.ZoomMax);
                this._painter.Zoom = _zoom;
            }
        }

        private void HandlerMouseRightClick()
        {
            switch (this._kickMode)
            {
                case KickMode.Free:
                    this.KickBallTowardMouse(this._ball);
                    if (Constants.GhostBallAllowed)
                    {
                        _painter.Ghost = null;
                    }
                    break;

                case KickMode.Pass:
                    this.KickShortPass();
                    break;

                case KickMode.Lob:
                    this.KickLob();
                    break;
            }

            _heightAccumulator = 0;
        }

        private void HandlerMouseLeftClick()
        {
            Coords clicked = PixelUnderMousecursor();

            this._ball.SpawnAt(new Vector2d(clicked.X, clicked.Y));
        }

        #endregion

        #region Kicking

        /// <summary>
        /// Passes the ball to the cursor by rolling it around the ground
        /// The ball's velocity at arrival is given by Constants.ShortPassDesiredVelocityAtArrival.
        /// </summary>
        private void KickShortPass()
        {
            // NOTE: Works, but isn't optimized. Fix as necessary.

            // Take cursor location and obtain the vector pointing toward (relative to the position of the ball).
            Coords clicked = PixelUnderMousecursor();
            Vector3d clickedVector = new Vector3d(clicked.X, clicked.Y, 0);
            Vector2d test0 = new Vector2d(clicked.X - _ball.Position3d.X, clicked.Y - _ball.Position3d.Y);

            // Proximity check.
            if (test0.Length() < Constants.TargetCheckTolerance)
            {
                // target too close. just nudge the ball.
                _ball.Kick(new Vector3d(test0.X, test0.Y, 0), Constants.ShortPassDefaultSpin, null);
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
            BallCalibrator calibrator = new BallCalibrator(_ball);
            calibrator.Kick(new Vector3d(test0.X, test0.Y, 0), Constants.ShortPassDefaultSpin);
            double testResult = calibrator.VelocityMagnitudeAtTarget(clickedVector);
            double difference = this._shotPower - testResult;
            double length = test0.Length();
            
            // Loop until the desired velocity has been found.
            while (difference > Constants.ShortPassDesiredVelocityAtArrivalTolerance &&
                length < Constants.ShortPassMaxVelocity)
            {
                calibrator.SpawnAt(new Vector2d(_ball.Position3d.X, _ball.Position3d.Y));
                length = test0.Length();
                test0.ScaleToLength(test0.Length() + Constants.ShortPassCalibrationSensitivity);
                calibrator.Kick(new Vector3d(test0.X, test0.Y, 0), Constants.ShortPassDefaultSpin);
                testResult = calibrator.VelocityMagnitudeAtTarget(clickedVector);
                difference = this._shotPower - testResult;
            }

            // Kick ball.
            _ball.Kick(new Vector3d(test0.X, test0.Y, 0), Constants.ShortPassDefaultSpin, null);
        }

        /// <summary>
        /// Lobs the ball so it bounces at the cursor. Spin is defined by Constants.LobDefaultSpin.
        /// </summary>
        private void KickLob()
        {
            Vector3d spin = new Vector3d(0, _spinY, _spinZ);
            //Vector3d spin = Constants.LobDefaultSpin;

            Coords clicked = PixelUnderMousecursor();
            Vector3d clickedVector = new Vector3d(clicked.X, clicked.Y, 0);
            Vector2d test0 = new Vector2d(clicked.X - _ball.Position3d.X, clicked.Y - _ball.Position3d.Y);
            double distanceToTarget = test0.Length();

            // Proximity check.
            if (distanceToTarget < Constants.TargetCheckTolerance)
            {
                // target too close. just nudge the ball.
                _ball.Kick(new Vector3d(test0.X, test0.Y, Constants.LobLiftCoefficient * test0.Length()), spin, null);
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
            BallCalibrator calibrator = new BallCalibrator(_ball);
            calibrator.Kick(new Vector3d(test0.X, test0.Y, Constants.LobLiftCoefficient * test0.Length()), spin);
            Vector3d landing = calibrator.BallPositionAtBounce();
            Vector3d vectorToLanding = landing - _ball.Position3d;
            double distanceMagnitude = vectorToLanding.Length();

            double difference = distanceMagnitude - distanceToTarget;
            // Loop until the desired velocity has been found.
            while (Math.Abs(difference) > Constants.LobTargetCheckTolerance &&
                test0.Length() < Constants.LobMaxVelocity)
            {
                calibrator.SpawnAt(new Vector2d(_ball.Position3d.X, _ball.Position3d.Y));
                //length = difference.Length();
                test0.ScaleToLength(test0.Length() + Constants.LobCalibrationSensitivity);
                calibrator.Kick(new Vector3d(test0.X, test0.Y, Constants.LobLiftCoefficient * test0.Length()), spin);
                landing = calibrator.BallPositionAtBounce();
                vectorToLanding = landing - _ball.Position3d;
                distanceMagnitude = vectorToLanding.Length();
                difference = distanceMagnitude - distanceToTarget;
            }

            // If necessary, rotate to align with target.
            if (spin.Z != 0)
            {
                double angleOfRotation = (new Vector2d(landing.X-_ball.Position3d.X, landing.Y-_ball.Position3d.Y)).
                AngleBetween(new Vector2d(clickedVector.X-_ball.Position3d.X, clickedVector.Y-_ball.Position3d.Y));
                test0 = test0.Rotate(angleOfRotation);
            }

            // Kick the ball.
            _ball.Kick(new Vector3d(test0.X, test0.Y, Constants.LobLiftCoefficient * test0.Length()), spin, null);
        }

        /// <summary>
        /// Kicks the ball toward the mouse with the height collected in the height accumulator.
        /// </summary>
        private void KickBallTowardMouse(Ball ball)
        {
            Coords clicked = PixelUnderMousecursor();

            Vector2d XYvelocity = new Vector2d(clicked.X - ball.Position3d.X, clicked.Y - ball.Position3d.Y);
            //XYvelocity.ScaleToLength(this._shotPower);

            Vector3d velocity = new Vector3d(XYvelocity.X, XYvelocity.Y, _heightAccumulator);
            velocity.ScaleToLength(this._shotPower);

            Vector3d spin = new Vector3d(0, _spinY, _spinZ);
            //spin.ScaleByFactor(Math.Min(1, 0.01 * velocity.Length()));

            ball.Kick(velocity, spin, null);
        }

        #endregion
    }
}
