using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BallPhysics
{
    static class StaticMathFunctions
    {
        /*
        public static Direction OppositeDirection(Direction d)
        {
            return (Direction)(((byte)d + 4) % 8);
        }
        */
        /*
        public static Direction DirectionToTheRight(Direction d)
        {
            return (Direction)(((byte)d + 1) % 8);
        }
        */

        /*
        public static Direction DirectionToTheLeft(Direction d)
        {
            return (Direction)(((byte)d + 7) % 8);
        }
        */

        /*
        public static Coords CoordsAverage(Coords c1, Coords c2)
        {
            return new Coords((Int32)0.5 * (c1.X + c2.X), (Int32)0.5 * (c1.Y + c2.Y));
        }
        */

        /*
        public static bool CoordinateIsInBox(Coords c, Coords boxTopLeft, Coords boxBottomRight)
        {
            return (((c.X >= boxTopLeft.X) && (c.X <= boxBottomRight.X)) && ((c.Y >= boxTopLeft.Y) && (c.Y <= boxBottomRight.Y)));
        }
        */

        
        /// <summary>
        /// Returns the eucledean distance between two Coords
        /// </summary>
        public static float DistanceBetweenTwoCoordsEucledean(Coords c1, Coords c2)
        {
            return (float) Math.Sqrt(Math.Pow((c1.X - c2.X),2) + Math.Pow((c1.Y - c2.Y),2));
        }

        /// <summary>
        /// returns the distance between two Coords
        /// </summary>
        public static float DistanceBetweenTwoCoordss(Coords c1, Coords c2)
        {
            return Math.Max(Math.Abs(c1.X - c2.X), Math.Abs(c1.Y - c2.Y));
        }

        public static Int32 DistanceBetweenTwoCoordsEucledeanSquared(Coords c1, Coords c2)
        {
            Int32 dx = c1.X - c2.X;
            Int32 dy = c1.Y - c2.Y;
            return (dx * dx + dy * dy);
        }

        // should really be done with a matrix.
        /// <summary>
        /// Returns the point on the distorted screen image of the field give the actual point in the 2d plane.
        /// </summary>
        public static Coords SpacePointToDisplayPointTransform(Vector2d v)
        {
            double yProportion = v.Y / Constants.ActualYMax;
            double xProportion = v.X / Constants.ActualXMax;

            Int32 y = (Int32) (Constants.DisplayYMax * yProportion);
            Int32 x = (Int32)((1 - yProportion) * Constants.DisplayLeftOffset +
                xProportion * (Constants.DisplayDeltaOffset + yProportion * (Constants.DisplayXMax - Constants.DisplayDeltaOffset)));

            return new Coords(x,y);
        }

        /// <summary>
        /// Returns the point on the distorted screen image of the field give the actual point in the 2d plane.
        /// </summary>
        public static Coords SpacePointToDisplayPointTransform(double xval, double yval)
        {
            double yProportion = yval / Constants.ActualYMax;
            double xProportion = xval / Constants.ActualXMax;

            Int32 y = (Int32)(Constants.DisplayYMax * yProportion);
            Int32 x = (Int32)((1 - yProportion) * Constants.DisplayLeftOffset +
                xProportion * (Constants.DisplayDeltaOffset + yProportion * (Constants.DisplayXMax - Constants.DisplayDeltaOffset)));

            return new Coords(x, y);
        }

        /// <summary>
        /// Returns the actual point in the 2d plane given a point from the distorted display image.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Vector2d DisplayPointToSpacePoint(Coords c)
        {
            double yProportion = c.Y / (double) Constants.DisplayYMax;
            
            double currentXmin = (1-yProportion)*Constants.DisplayLeftOffset;
            double currentXmax = Constants.DisplayRightOffset + yProportion*(Constants.DisplayXMax - Constants.DisplayRightOffset);

            double xProportion = (c.X - currentXmin) / (currentXmax - currentXmin);
            xProportion = Math.Min(Math.Max(0, xProportion), 1);

            double xVal = xProportion * Constants.ActualXMax;
            double yVal = yProportion * Constants.ActualYMax;

            return new Vector2d(xProportion*Constants.ActualXMax, yProportion*Constants.ActualYMax);
        }

        /// <summary>
        ///  returns factor for the z-value to fix the POV issue
        /// </summary>
        public static double SpacePointToDisplayPointZAxis(double y)
        {
            double yProportion = y / Constants.ActualYMax;

            double scaleMin = ((double)Constants.DisplayYMax) / Constants.ActualYMax;

            return (scaleMin+yProportion*(1-scaleMin));
        }

        /// <summary>
        /// Returns POV coordinates given the real-space ones.
        /// </summary>
        public static Vector3d Space3dPointToDisplay3dPointTransform(Vector3d v)
        {
            double yProportion = v.Y / Constants.ActualYMax;
            double xProportion = v.X / Constants.ActualXMax;

            double y = (Constants.DisplayYMax * yProportion);
            double x = ((1 - yProportion) * Constants.DisplayLeftOffset +
                xProportion * (Constants.DisplayDeltaOffset + yProportion * (Constants.DisplayXMax - Constants.DisplayDeltaOffset)));

            double scaleMin = ((double)Constants.DisplayYMax) / Constants.ActualYMax;
            double z = (scaleMin + yProportion * (1 - scaleMin));

            return new Vector3d(x, y, z);
        }

        /*
        /// <summary>
        /// Returns the Direction in which a vector is pointing.
        /// </summary>
        public static Nullable<Direction> DirectionVectorToDirection(Coords dirvector)
        {
            if (dirvector.X == 0 & dirvector.Y == 0)
            {
                return null;
            }

            // The angle is clockwise from the negative X, Y=0 axis. Note the positive Y-axis points down.
            double angle;
            angle = Math.Atan2(dirvector.Y, dirvector.X) + Math.PI;

            Direction moveDir = (Direction)
               (byte)((((angle + 0.125 * Math.PI) / (0.25 * Math.PI)) + 5) % 8);

            return moveDir;
        }
        */

        public static Coords PositionOnFieldToInfMapCoords(Vector2d position)
        {
            return new Coords((Int32)(position.X / Constants.InfMapDefaultBoxSizeX),
                        (Int32)(position.Y / Constants.InfMapDefaultBoxSizeY));
        }
        
        public static float InfluenceDecayFunction1(UInt32 a)
        {
            return (float)1 / (a + 1);
        }

        public static float InfluenceDecayFunctionLinear(UInt32 a)
        {
            return (float)Math.Max(0, 1 - Constants.InfMapLinearCoefficient * a);
        }
        
        /// <summary>
        /// Returns the Coords that neighbour 'here' in 'direction'.
        /// Note C# forms coordinate system has origin at the top-left
        /// </summary>
        public static Coords CoordsNeighboringInDirection(Coords here, Direction direction)
        {
            switch (direction)
            {
                case (Direction.Northeast):
                    return new Coords(here.X + 1, here.Y - 1);
                case (Direction.East):
                    return new Coords(here.X + 1, here.Y);
                case (Direction.Southeast):
                    return new Coords(here.X + 1, here.Y + 1);
                case (Direction.South):
                    return new Coords(here.X, here.Y + 1);
                case (Direction.Southwest):
                    return new Coords(here.X - 1, here.Y + 1);
                case (Direction.West):
                    return new Coords(here.X - 1, here.Y);
                case (Direction.Northwest):
                    return new Coords(here.X - 1, here.Y - 1);
                case (Direction.North):
                    return new Coords(here.X, here.Y - 1);
            }

            // This code should be unreachable. Added because compiler wants it.
            return here;
        }
        

        /*
        // Returns the coordinate-wise representation of a Direction
        public static Coords DirectionToCoords(Direction dir)
        {
            switch (dir)
            {
                case (Direction.Northeast):
                    return new Coords(1, -1);
                case (Direction.East):
                    return new Coords(1, 0);
                case (Direction.Southeast):
                    return new Coords(1, 1);
                case (Direction.South):
                    return new Coords(0, 1);
                case (Direction.Southwest):
                    return new Coords(-1, 1);
                case (Direction.West):
                    return new Coords(-1, 0);
                case (Direction.Northwest):
                    return new Coords(-1, -1);
                case (Direction.North):
                    return new Coords(0, -1);
            }

            return new Coords(0, 0);
        }
        */
    }
}
