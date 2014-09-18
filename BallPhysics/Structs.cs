using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BallPhysics
{
    /// <summary>
    /// Coords struct. Similar to 'Point', but I chose a different name to 
    /// distinguish this structure from Drawing.Point
    /// </summary>
    public struct Coords : IComparable
    {
        private Int32 _X;
        public Int32 X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        private Int32 _Y;
        public Int32 Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        public Coords NeighborInDirection(Direction dir)
        {
            switch (dir)
            {
                case (Direction.Northeast):
                    return new Coords(this.X + 1, this.Y - 1);
                case (Direction.East):
                    return new Coords(this.X + 1, this.Y);
                case (Direction.Southeast):
                    return new Coords(this.X + 1, this.Y + 1);
                case (Direction.South):
                    return new Coords(this.X, this.Y + 1);
                case (Direction.Southwest):
                    return new Coords(this.X - 1, this.Y + 1);
                case (Direction.West):
                    return new Coords(this.X - 1, this.Y);
                case (Direction.Northwest):
                    return new Coords(this.X - 1, this.Y - 1);
                case (Direction.North):
                    return new Coords(this.X, this.Y - 1);
            }

            // This code should be unreachable. Added because compiler wants it.
            return this;
        }

        public float DistanceTo(Coords c)
        {
            return (float)Math.Sqrt(Math.Pow((this.X - c.X), 2) + Math.Pow((this.Y - c.Y), 2));
        }

        #region Operators
        public Int32 CompareTo(object obj)
        {
            if (!(obj is Coords))
            {
                throw new Exception("Bad Coords comparison.");
            }

            Coords compared = (Coords)obj;

            Int32 score = 0;

            score = (this.X + this.Y) - (compared.X + compared.Y);

            if (score == 0)
            {
                score = this.X - compared.X;
            }

            return score;
        }

        public override bool Equals(object obj)
        {
            return (obj is Coords) && this == (Coords)obj;
        }

        public override Int32 GetHashCode()
        {
            return _X ^ _Y;
        }

        public override string ToString()
        {
            return (String)"(" + this._X + ", " + this._Y + ")";
        }

        public static bool operator ==(Coords c1, Coords c2)
        {
            return (c1.X == c2.X && c1.Y == c2.Y);
        }

        public static bool operator !=(Coords c1, Coords c2)
        {
            return (c1.X != c2.X || c1.Y != c2.Y);
        }

        public static Coords operator -(Coords c1, Coords c2)
        {
            return new Coords(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static Coords operator +(Coords c1, Coords c2)
        {
            return new Coords(c1.X + c2.X, c1.Y + c2.Y);
        }

        #endregion

        public Coords(Int32 Xval, Int32 Yval)
        {
            this._X = Xval;
            this._Y = Yval;
        }

        public Coords(Coords c)
        {
            _X = c.X;
            _Y = c.Y;
        }

        public Coords(Vector2d v)
            : this((Int32)Math.Floor(v.X), (Int32)Math.Floor(v.Y))
        {
        }
    }

    public struct Vector2d
    {
        private double _X;
        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        private double _Y;
        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        #region Operators

        public override bool Equals(object obj)
        {
            return (obj is Vector2d) && this == (Vector2d)obj;
        }

        public override Int32 GetHashCode()
        {
            return (byte)_X ^ (byte)_Y;
        }

        public override string ToString()
        {
            return (String)"(" + this._X + ", " + this._Y + ")";
        }

        public static bool operator ==(Vector2d c1, Vector2d c2)
        {
            return (c1.X == c2.X && c1.Y == c2.Y);
        }

        public static bool operator !=(Vector2d c1, Vector2d c2)
        {
            return (c1.X != c2.X || c1.Y != c2.Y);
        }

        public static Vector2d operator -(Vector2d c1, Vector2d c2)
        {
            return new Vector2d(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static Vector2d operator +(Vector2d c1, Vector2d c2)
        {
            return new Vector2d(c1.X + c2.X, c1.Y + c2.Y);
        }

        #endregion

        public double Length()
        {
            return Math.Sqrt(_X * _X + _Y * _Y);
        }

        public Vector2d Rotate(double angle)
        {
            Double sinA = Math.Sin(angle);
            Double cosA = Math.Cos(angle);

            Vector2d result = new Vector2d(_X * cosA - _Y * sinA, _X * sinA + _Y * cosA);

            return result;
        }

        /// <summary>
        /// Returns the counter-clockwise [0,2Pi) angle between this vector (acting as reference) and Vector v.
        /// </summary>
        public double AngleBetween(Vector2d v)
        {
            if ((this.X == 0 && this.Y == 0) || (v.X == 0 && v.Y == 0))
            {
                throw new Exception("Zero-length vectors passed for angle-finding.");
                //return 0;
            }

            /*
            // Check to see if the vectors are unidirectional with tolerance Epsilon.
            if ((this - v).Length() < Constants.Epsilon)
            {
                return 0d;
            }
            */

            double thisLength = this.Length();
            double vLength = v.Length();

            double cosA = (this.X * v.X + this.Y * v.Y) / (thisLength * vLength);
            cosA = Math.Min(Math.Max(cosA, -1), 1); // numerical correction
            double sinA = (this.X * v.Y - this.Y * v.X) / (thisLength * vLength);
            sinA = Math.Min(Math.Max(sinA, -1), 1); // numerical correction

            double angle1 = Math.Acos(cosA);
            double angle2 = Math.Asin(sinA);

            double returnangle;
            if (cosA >= 0)
            {
                if (sinA >= 0)
                {
                    // 1ST QUARTER
                    // the A-COS produces the correct result
                    returnangle = angle1;
                }
                else
                {
                    // 4TH QUARTER
                    // 2*Pi - the cosine is correct
                    returnangle = 2 * Math.PI - angle1;
                }
            }
            else
            {
                if (sinA > 0)
                {
                    // 2ND QUARTER
                    // the cosine is correct
                    returnangle = angle1;
                }
                else
                {
                    // 3RD QUARTER
                    // 2 * Pi - the angle is correct
                    returnangle = 2 * Math.PI - angle1;
                }
            }

            // This arises when the vectors are supposed to be unidirectional, but aren't, due to numerical pertubation.
            // I don't know how to do this properly.
            // Perhaps I ought to throw exceptions and make sure the method never gets called?
            if (double.IsNaN(returnangle))
            {
                throw new Exception("Angle sought between roughly equivalent vectors.");
            }

            return returnangle;
        }

        public void ScaleByFactor(double factor)
        {
            _X = _X * factor;
            _Y = _Y * factor;
        }

        public void ScaleToLength(double length)
        {
            double currentLength = this.Length();

            if (currentLength > 0)
            {
                double scale = length / this.Length();
                _X = _X * scale;
                _Y = _Y * scale;
            }
        }

        public double DistanceTo(Vector2d v)
        {
            return Math.Sqrt(Math.Pow(this.X - v.X, 2) + Math.Pow(this.Y - v.Y, 2));
        }

        public Vector2d(double x, double y)
        {
            _X = x;
            _Y = y;
        }

        public Vector2d(Coords c)
        {
            _X = c.X;
            _Y = c.Y;
        }
    }

    public struct Vector3d
    {
        private double _X;
        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        private double _Y;
        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        private double _Z;
        public double Z
        {
            get
            {
                return _Z;
            }
            set
            {
                _Z = value;
            }
        }

        #region Operators

        public override bool Equals(object obj)
        {
            return (obj is Vector3d) && this == (Vector3d)obj;
        }

        public override Int32 GetHashCode()
        {
            return (byte)_X ^ (byte)_Y ^ (byte) _Z;
        }

        public override string ToString()
        {
            return (String)"(" + this._X + ", " + this._Y + ", " + this._Z + ")";
        }

        public static bool operator ==(Vector3d c1, Vector3d c2)
        {
            return (c1.X == c2.X && c1.Y == c2.Y && c1.Z == c2.Z);
        }

        public static bool operator !=(Vector3d c1, Vector3d c2)
        {
            return (c1.X != c2.X || c1.Y != c2.Y || c1.Z != c2.Z);
        }

        public static Vector3d operator -(Vector3d c1, Vector3d c2)
        {
            return new Vector3d(c1.X - c2.X, c1.Y - c2.Y, c1.Z - c2.Z);
        }

        public static Vector3d operator +(Vector3d c1, Vector3d c2)
        {
            return new Vector3d(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z);
        }

        public static Vector3d operator *(Vector3d v, double d)
        {
            return new Vector3d(v._X * d, v.Y * d, v.Z * d);
        }

        #endregion

        public Vector2d ProjectionXY()
        {
            return new Vector2d(this._X, this._Y);
        }

        public Vector2d ProjectionXZ()
        {
            return new Vector2d(this._X, this._Z);
        }

        public double Length()
        {
            return Math.Sqrt(_X * _X + _Y * _Y + _Z * _Z);
        }

        public double AngleBetween(Vector3d v)
        {
            double thisLength = this.Length();
            double vLength = v.Length();

            if (thisLength == 0 || vLength == 0)
            {
                // what now? exception?
                return 0;
            }

            double dotProduct = this._X * v.X + this._Y * v.Y + this._Z * v.Z;
            double cosAngle = dotProduct / (thisLength * vLength);
            double angle = Math.Acos(cosAngle);

            return angle;
        }

        public Vector3d CrossProductWith(Vector3d v)
        {
            return new Vector3d(this._Y * v.Z - this._Z * v.Y, this._Z * v.X - this._X * v.Z, this.X * v.Y - this._Y * v.X);
        }

        /// <summary>
        /// Do this with matrices.
        /// http://en.wikipedia.org/wiki/Rotation_matrix#Three_dimensions
        /// </summary>
        public Vector3d RotateAroundAxis(double angle, Vector3d u)
        {
            double axisLength = u.Length();

            if(axisLength == 0)
            {
                // throw exception?
                return new Vector3d(0,0,0);
            }

            // make sure axis is normalized!
            if(u.Length() != 1)
            {
                u.ScaleToLength(1);
            }

            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);

            double ux2 = u.X*u.X;
            double uy2 = u.Y*u.Y;
            double uz2 = u.Z*u.Z;

            double uxy = u.X*u.Y;
            double uxz = u.X*u.Z;
            double uyz = u.Y*u.Z;

            //Matrix:
            // a11 a12 a13
            // a21 a22 a23
            // a31 a32 a33

            double a11 = ux2 + (1-ux2)*cosA;
            double a12 = uxy * (1 - cosA) - u.Z * sinA;
            double a13 = uxz * (1 - cosA) + u.Y * sinA;
            double a21 = uxy * (1 - cosA) + u.Z * sinA;
            double a22 = uy2 + (1 - uy2) * cosA;
            double a23 = uyz * (1 - cosA) - u.X * sinA;
            double a31 = uxz * (1 - cosA) - u.Y * sinA;
            double a32 = uyz * (1 - cosA) + u.X * sinA;
            double a33 = uz2 + (1 - uz2) * cosA;

            double x = this._X * a11 + this._Y * a12 + this._Z * a13;
            double y = this._X * a21 + this._Y * a22 + this._Z * a23;
            double z = this._X * a31 + this._Y * a32 + this._Z * a33;

            return new Vector3d(x, y, z);
        }

        public Vector3d RotateXY(double angle)
        {
            Double sinA = Math.Sin(angle);
            Double cosA = Math.Cos(angle);
            return new Vector3d(_X * cosA + _Y * sinA, -_X * sinA + _Y * cosA, _Z);
        }

        public Vector3d RotateXZ(double angle)
        {
            Double sinA = Math.Sin(angle);
            Double cosA = Math.Cos(angle);
            return new Vector3d(_X * cosA + _Z * sinA, _Y, -_X * sinA + _Z * cosA);
        }

        public void ScaleByFactor(double factor)
        {
            _X = _X * factor;
            _Y = _Y * factor;
            _Z = _Z * factor;
        }

        public void ScaleToLength(double length)
        {
            double currentLength = this.Length();

            if (currentLength > 0)
            {
                double scale = length / this.Length();
                _X = _X * scale;
                _Y = _Y * scale;
                _Z = _Z *scale;
            }
        }

        public double DistanceTo(Vector3d v)
        {
            return Math.Sqrt(Math.Pow(this.X - v.X, 2) + Math.Pow(this.Y - v.Y, 2) + Math.Pow(this.Z - v.Z,2));
        }

        public Vector3d(double x, double y, double z)
        {
            _X = x;
            _Y = y;
            _Z = z;
        }
    }

    public enum Stats : sbyte
    {
        Strength = 0,
        Speed = 1,
        Eyesight = 2
    }

    public enum CollisionType
    {
        None = 0,
        Terrain,
        Footballer
    }

    /// <summary>
    /// The compass directions, clockwise, Northeast is 1.
    /// </summary>
    public enum Direction : sbyte
    {
        Northeast = 0,
        East = 1,
        Southeast = 2,
        South = 3,
        Southwest = 4,
        West = 5,
        Northwest = 6,
        North = 7
    }

    /// <summary>
    /// Scrolling type flag.
    /// </summary>
    public enum ScrollingType : sbyte
    {
        /// <summary>
        /// Ball centered scrolling
        /// </summary>
        Ball = 0,
        /// <summary>
        /// Free scrolling.
        /// Fits RTS-type games.
        /// </summary>
        Free
    }

    public enum KickMode : sbyte
    {
        Free = 0,
        Pass,
        Lob,
        Shot
    }

    public enum BallPosition : sbyte
    {
        Visible = 0, // no clipping
        InsideGoal,
        BehindGoal
    }

    public enum FootballerTeam : sbyte
    {
        TeamLeft = 0,
        TeamRight
    }
}
