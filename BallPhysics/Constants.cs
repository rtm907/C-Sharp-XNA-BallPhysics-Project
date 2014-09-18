using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace BallPhysics
{
    // This class contains the global constants
    public static class Constants
    {
        //private static String Version = "0.009";

        #region Game - related

        // Epsilon for numerical purposes.
        // Read a numerical book to see how this should be done.
        public static double Epsilon = Math.Pow(10, -6);

        public static UInt16 defaultTimerPeriod = 20;

        public static UInt16 redrawPeriod = 40; // 25 frames per second

        public static Int32 defaultMapRandomatorSeed = 907;

        public static bool RecordInput = false;

        // Moving in a diagonal direction takes root(2) time (with movement left/right/top/bottom
        // taking one unit time).
        public static float diagonalCoefficient = (float)Math.Sqrt(2);

        public static float[] MovementCost =
        { Constants.diagonalCoefficient, 1, 
           Constants.diagonalCoefficient, 1, 
           Constants.diagonalCoefficient, 1, 
           Constants.diagonalCoefficient, 1
        };

        #endregion

        #region Drawing - related

        // Scrolling
        public static ScrollingType Scrolling = ScrollingType.Free;
        public static float FreeScrollingSpeed = 20f;

        public static bool ShowTrajectory = false;

        // Zooming
        public static bool ZoomingAllowed = true;
        public static float ZoomSpeed = 0.0004f; // use 2^(-10) in hexadecimal, or something
        public static float ZoomMin = 0.1f;
        public static float ZoomMax = 1f;

        public static Color BoundingCircleColor = Color.Red;

        // nets
        public static Color NetsColor = Color.Gray;
        public static UInt16 NetsSpacing = 20;

        // Ghost Ball
        public static bool GhostBallAllowed = true;
        public static Int32 GhostBallTimeLength = 20;
        public static Int32 GhostBallDrawInterval = 5;

        // Pseudo-3D transform data
        public static Int32 ActualXMax = 6000;
        public static Int32 ActualYMax = 4147;

        private static Coords ActualTopLeft = new Coords(0, 0);
        private static Coords ActualTopRight = new Coords(ActualXMax, 0);
        private static Coords ActualBottomLeft = new Coords(0, ActualYMax);
        private static Coords ActualBottomRight = new Coords(ActualXMax, ActualYMax);

        // these are the offsets in the X-direction of the upper-left and upper-right corners of the
        // slanted field
        public static Int32 DisplayLeftOffset = 1150;
        public static Int32 DisplayRightOffset = 4873;
        public static Int32 DisplayDeltaOffset = DisplayRightOffset - DisplayLeftOffset;
        // width/height of transformed image
        public static Int32 DisplayXMax = 5989;
        public static Int32 DisplayYMax = 2067;

        private static Coords DisplayTopLeft = new Coords(DisplayLeftOffset, 0);
        private static Coords DisplayTopRight = new Coords(DisplayRightOffset, 0);
        private static Coords DisplayBottomLeft = new Coords(0, DisplayYMax);
        private static Coords DisplayBottomRight = new Coords(DisplayXMax, DisplayYMax);

        // pixel addresses for props:
        public static Coords[] PropLocations = new Coords[] { 
            new Coords(750, 555), //goal left
            new Coords(5122, 555) // goal right
        };

        #endregion

        #region Map - related

        //the size of the smallest grid member in pixels. must be a divisor of _tileSize.
        //public static UInt16 TileBitmapSize = 32;

        //public static UInt16 TileSizePixels = 32;

        public static UInt16[] TileSizesX = new UInt16[] { 1156, 614, 1236, 1253, 601, 1129 };
        public static UInt16[] TileSizesY = new UInt16[] { 293, 1161, 613 };

        public static double BallRadius = 7.5d;
        public static double PostRadius = 4d;
        
        public static float ZoomDefault = 0.5f;

        // Default map size
        public static UInt16 MapSizeX = (UInt16) TileSizesX.Length;
        public static UInt16 MapSizeY = (UInt16) TileSizesY.Length;
        
        #endregion

        #region Collider

        public static UInt16 BoxesInX = 50;
        public static UInt16 BoxesInY = 30;

        public static UInt16 PixelsPerBoxX = (UInt16)(ActualXMax / BoxesInX);
        public static UInt16 PixelsPerBoxY = (UInt16)(ActualYMax / BoxesInY);

        #endregion

        #region Goals and Nets and Lines

        // Goals
        public static UInt16 GoalTop = 1394;
        public static UInt16 GoalBottom = 1875;
        public static UInt16 GoalLeft = 160;
        public static UInt16 GoalRight = 5840;

        // Ought to be 163 from my calculations... For esthetic reasons I've set it higher.
        public static UInt16 GoalHeight = 163;

        private static Vector2d PostLeftTop = new Vector2d(Constants.GoalLeft, Constants.GoalTop);
        private static Vector2d PostLeftBottom = new Vector2d(Constants.GoalLeft, Constants.GoalBottom);
        private static Vector2d PostRightTop = new Vector2d(Constants.GoalRight, Constants.GoalTop);
        private static Vector2d PostRightBottom = new Vector2d(Constants.GoalRight, Constants.GoalBottom);

        public static Vector2d[] Uprights = new Vector2d[4] { PostLeftTop, PostLeftBottom, PostRightTop, PostRightBottom };

        // Be careful with this, keep in mind it's in the XZ plane!
        private static Vector2d CrossbarLeft = new Vector2d(Constants.GoalLeft, Constants.GoalHeight);
        private static Vector2d CrossbarRight = new Vector2d(Constants.GoalRight, Constants.GoalHeight);

        public static Vector2d[] Crossbars = new Vector2d[2] { CrossbarLeft, CrossbarRight };

        // Nets
        // For now, rectangular nets, so most of data is recycled from the goal-posts data.
        // We just add an x-value for the back of the net.
        public static UInt16 NetsDepth = 80;
        public static UInt16 NetsBackLeft = (UInt16) (GoalLeft - NetsDepth);
        public static UInt16 NetsBackRight = (UInt16) (GoalRight + NetsDepth);

        // lines   
        public static UInt16 GoalLineLeft = GoalLeft;
        public static UInt16 GoalLineRight = GoalRight;
        public static UInt16 TouchLineTop = 100;
        public static UInt16 TouchLineBottom = 3888;

        public static Vector2d PenaltyLeft = new Vector2d(906, 1586);
        public static Vector2d PenaltyRight = new Vector2d(5097, 1586);

        public static Vector2d CenterPoint = new Vector2d(3000, 1586);

        public static Vector2d[] CornerPoint = new Vector2d[]
        {
            new Vector2d(GoalLineLeft +20, TouchLineTop + 20), // left-top
            new Vector2d(GoalLineLeft +20, TouchLineBottom - 20), // left-bottom
            new Vector2d(GoalLineRight -20, TouchLineTop + 20), // right-top
            new Vector2d(GoalLineRight -20, TouchLineBottom - 20) // right-bottom
        };

        public static Vector2d[] GoalKickPoint = new Vector2d[]
        {
            new Vector2d(700,1300),
            new Vector2d(5500,1300)
        };

        #endregion

        #region Footballer - related

        public static UInt16 defaultUnitSpeed = 5;

        public static UInt16 StatsMin = 1;
        public static UInt16 StatsMax = 20;

        // The stats are generated from the normal(mu, sigma) distribution and are between 1 and 20.
        public static double[,] GoblinStatGenerationMuSigmaDefaults = 
        {{10, 4}, // strength mu and sigma    
        {12, 3}, // speed
        {7, 4} // eyesight
        };

        public static UInt16[,] GoblinStatGenerationMinMaxDefaults =
        {{5,15}, //strength
        {7,16}, //speed
        {2,14} //eyesight
        };

        // minimum number of ticks an action is supposed to take.
        private static UInt16 MinSpeed = 1;
        private static UInt16 SpeedConversionFactor = 4;

        // coverts the 1-20 speed stat to the FootballerSpeed in pixels/tick (the smaller the slower).
        public static UInt16 StatSpeedToMovementTicksTakenCoverter(UInt16 speed)
        {
            return (UInt16) ( MinSpeed + (speed / Constants.SpeedConversionFactor));
        }

        public static UInt16 CollisionRadiusX = 30;
        public static UInt16 CollisionRadiusY = 30;

        public static double BaseTurningCoefficient = 0.2d;
        public static double BaseTurningSpeed = Math.PI / 4;

        public static double BallControlRadius = 100d;
        public static double MaxControlHeight = 100d;
        public static double BallControlDefaultHalfArc = Math.PI / 12d;

        public static double BallDribbleRadius = 500d;
        public static double DirectionCheckTolerance = Math.PI / 10d;

        #endregion

        #region Physics

        // Ball motion parameters
        public static double BouncingGrassConstant = 0.425d;
        public static double BouncingNetConstant = 0.1d;
        public static double BouncingSpinSoak = 0.3d;

        public static double FrictionGrass = 0.3d;
        public static double FrictionAIr = 0.1d;

        public static double GravityDeceleration = 2d;

        public static double MinBounceVelocity = 1d;

        public static double SpinDecayAir = 0.05d;
        public static double SpinDecayGrass = 0.2d;
        public static double SpinMinimalThreshold = 0.001d;

        public static Vector3d SpinAxis = new Vector3d(1, 0, 0);

        // Passing parameters
        public static double ShortPassMaxVelocity = 100d;
        public static double ShortPassDesiredVelocityAtArrival = 40d;
        public static double ShortPassDesiredVelocityAtArrivalTolerance = 5d;
        public static double ShortPassCalibrationSensitivity = 1d;
        public static Vector3d ShortPassDefaultSpin = new Vector3d(0, 0, 0);

        public static Vector3d LobDefaultSpin = new Vector3d(0,-1,-2);
        public static double LobLiftCoefficient = 1d;
        public static double LobCalibrationSensitivity = 1d;
        public static double LobMaxVelocity = 100;
        public static double LobTargetCheckTolerance = 50d;

        public static double TargetCheckTolerance = 5d;

        //public static double FootaballerPositionCheckTolerance = 20d;

        public static double GeneralMaxShotStrength = 200d;

        #endregion

        #region Interface

        public const Keys KickModeFree = Keys.A;
        public const Keys KickModePass = Keys.S;
        public const Keys KickModeLob = Keys.D;

        public const Keys SpinYAdd = Keys.P;
        public const Keys SpinYSub = Keys.O;
        public const Keys SpinZAdd = Keys.L;
        public const Keys SpinZSub = Keys.K;

        public const Keys ShotPowerAdd = Keys.N;
        public const Keys ShotPowerSub = Keys.M;

        public static double SpinDelta = 0.05;
        public static double ShotPowerDelta = 1d;

        public static float MatchTimeDelta = 45f / (300f*1000f/Constants.defaultTimerPeriod); // this gives 5 min halftime

        #endregion

        #region AI - related

        public static double CollisionAvoidanceRotation = Math.PI / 6;

        public static UInt16 ProjectionMaxLength = 500;

        public static Int32 DefaultPostPause = 5;
        public static Int32 DefaultPreWait = 5;

        public static Vector2d[] DefaultPositions = new Vector2d[]
        {
            new Vector2d(0.5,0.05), // GK
            
            new Vector2d(0.1, 0.2), // LB
            new Vector2d(0.4, 0.2), // CD
            new Vector2d(0.6, 0.2), // CD
            new Vector2d(0.9, 0.2), // RB

            new Vector2d(0.1, 0.6), // LW
            new Vector2d(0.4, 0.5), // CMF
            new Vector2d(0.6, 0.5), // CMF
            new Vector2d(0.9, 0.6), // RW

            new Vector2d(0.4, 0.8), // ST
            new Vector2d(0.6, 0.9) // ST
        }; 
        
        
        #endregion

        #region Influence Maps

        public static UInt16 InfMapDefaultX = 30;
        public static UInt16 InfMapDefaultY = 18;

        public static UInt16 InfMapDefaultBoxSizeX = (UInt16) (ActualXMax / InfMapDefaultX);
        public static UInt16 InfMapDefaultBoxSizeY = (UInt16)(ActualYMax / InfMapDefaultY);

        public static float InfMapLinearCoefficient = 0.2f;

        // Max distance threshold for recursive influence generation algorithm (InfluenceSourceMap).
        private static UInt16 _influenceMapMaxDistance = (UInt16) (1f/InfMapLinearCoefficient);
        public static UInt16 InfluenceMapMaxDistance
        {
            get
            {
                return _influenceMapMaxDistance;
            }
        }

        private static float _influenceMapMinThreshold = (float)Math.Pow(10, (-5));
        public static float InfluenceMapMinThreshold
        {
            get
            {
                return _influenceMapMinThreshold;
            }
        }

        public static UInt16 InfMapUpdatePeriod = 50;

        #endregion

        #region Strings

        public static Int32 FontSize = 12;

        public static String[] QuipsGreetings = 
        {"Yo, man!",
            "Hello!",
            "How's it going.",
            "Hey.",
            "*nods*"
        };

        public static String[] GnomeNamebits = 
        {"ka", "kri", "kyu", "khe", "ko",
            "sam", "sir", "suk", "sech", "soj",
            "bik", "trom", "shrok", "jem", "kop"
        };

        #endregion
    }
}
