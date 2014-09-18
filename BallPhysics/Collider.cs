using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallPhysics
{
    public class Collider
    {
        private UInt16 _sizeX;
        public UInt16 SizeX
        {
            get
            {
                return _sizeX;
            }
        }

        private UInt16 _sizeY;
        public UInt16 SizeY
        {
            get
            {
                return _sizeY;
            }
        }

        private UInt16 _pixelsPerBoxX;
        private UInt16 _pixelsPerBoxY;

        #region collision / bounds / etc -checkers

        private Coords VertexTopLeft(Coords box)
        {
            return new Coords(_pixelsPerBoxX * box.X, _pixelsPerBoxY * box.Y);
        }
        private Coords VertexTopRight(Coords box)
        {
            return new Coords(_pixelsPerBoxX * (box.X + 1) - 1, _pixelsPerBoxY * box.Y);
        }
        private Coords VertexBottomLeft(Coords box)
        {
            return new Coords(_pixelsPerBoxX * box.X, _pixelsPerBoxY * (box.Y + 1) - 1);
        }
        private Coords VertexBottonRight(Coords box)
        {
            return new Coords(_pixelsPerBoxX * (box.X + 1) - 1, _pixelsPerBoxY * (box.Y + 1) - 1);
        }

        private List<Footballer>[,] _clippingBoxes;
        private SortedList<Footballer, LinkedList<Coords>> _occupiedBoxes = new SortedList<Footballer,LinkedList<Coords>>();

        public void RegisterFootballer(Footballer someGuy)
        {
            LinkedList<Coords> steppedOn = TilesCoveredByEllipse(new Coords(someGuy.PositionDouble), someGuy.RadiusX, someGuy.RadiusY);

            foreach (Coords c in steppedOn)
            {
                this.ClippingBoxesAddTo(c, someGuy);
            }

            _occupiedBoxes.Add(someGuy, steppedOn);
        }

        public void UpdateFootballerBoxes(Footballer someGuy, Vector2d oldPosition)
        {
            Coords oldCoords = new Coords((Int32)(oldPosition.X / _pixelsPerBoxX), (Int32)(oldPosition.Y / _pixelsPerBoxY));
            Coords newCoords = new Coords((Int32)(someGuy.PositionDouble.X / _pixelsPerBoxX), (Int32)(someGuy.PositionDouble.Y / _pixelsPerBoxY));

            if (oldCoords == newCoords)
            {
                // nothing to do
                return;
            }

            LinkedList<Coords> steppedOn = TilesCoveredByEllipse(new Coords(someGuy.PositionDouble), someGuy.RadiusX, someGuy.RadiusY);
            foreach (Coords c in _occupiedBoxes[someGuy])
            {
                this.ClippingBoxesRemoveFrom(c, someGuy);
            }
            foreach (Coords c in steppedOn)
            {
                this.ClippingBoxesAddTo(c, someGuy);
            }

            _occupiedBoxes[someGuy] = steppedOn;
        }

        public void HandleCollisions(Footballer someGuy, Vector2d oldPosition)
        {
            this.UpdateFootballerBoxes(someGuy, oldPosition);

            List<Footballer> collisions = this.PotentialFootballerToFootballerCollision(someGuy, new Coords(someGuy.PositionDouble));

            for(int i=0; i< collisions.Count; ++i)
            {
                this.HandleCollisionBetweenTwoPlayers(someGuy, collisions[i]);
            }
        }

        private void HandleCollisionBetweenTwoPlayers(Footballer first, Footballer second)
        {
            double distance = first.PositionDouble.DistanceTo(second.PositionDouble);
            double preferredDistance = Math.Sqrt(Math.Pow(first.RadiusX + second.RadiusX, 2) + Math.Pow(first.RadiusY + second.RadiusY, 2));

            double deltaOver2 = 0.5 * (preferredDistance - distance);

            if (deltaOver2 < 0)
            {
                return;
            }

            // push both actors in opposite direction, by a vector of length delta/2.
            Vector2d mover = first.PositionDouble - second.PositionDouble;
            mover.ScaleToLength(deltaOver2);
            first.PositionDouble += mover;
            second.PositionDouble -= mover;

            // this might have cause other collisions. the algo needs fixing.
        }

        public void ClippingBoxesAddTo(Coords box, Footballer newGuy)
        {
            _clippingBoxes[box.X, box.Y].Add(newGuy);
        }

        public void ClippingBoxesRemoveFrom(Coords box, Footballer removeMe)
        {
            _clippingBoxes[box.X, box.Y].Remove(removeMe);
        }

        /// <summary>
        ///  Returns footballers overlapped by agent.
        /// </summary>
        public List<Footballer> PotentialFootballerToFootballerCollision(Footballer critter, Coords potentialPosition)
        {
            List<Footballer> returnVal = new List<Footballer>();

            UInt16 critterRadiusX = critter.RadiusX;
            UInt16 critterRadiusY = critter.RadiusY;

            LinkedList<Coords> checkList = this.TilesCoveredByEllipse(potentialPosition, critterRadiusX, critterRadiusY);

            foreach (Coords checkme in checkList)
            {
                foreach (Footballer obstacle in _clippingBoxes[checkme.X, checkme.Y])
                {
                    // ignore self
                    if (obstacle == critter)
                    {
                        continue;
                    }

                    if (CollisionCheckEllipses(potentialPosition, critterRadiusX, critterRadiusY,
                        new Coords(obstacle.PositionDouble), obstacle.RadiusX, obstacle.RadiusY))
                    {
                        returnVal.Add(obstacle);
                    }
                }
            }

            return returnVal;
        }

        /*
        /// <summary>
        /// Returns all footballers clipped by the agent; empty if none.
        /// </summary>
        public List<Footballer> CreatureClippingCheck(Footballer critter, Coords potentialPosition, bool creaturesClipCheck)
        {
            // obtain new entry tiles
            LinkedList<Coords> newEntries = this.TilesCoveredByEllipse(potentialPosition, critter.RadiusX, critter.RadiusY);

            // if the flags demands it, check if there is a creature in the way
            if (creaturesClipCheck)
            {
                if (this.PotentialCreatureToCreatureCollision(critter, potentialPosition))
                {
                    return CollisionType.Footballer;
                }
            }

            return CollisionType.None;
        }
        */

        public LinkedList<Coords> TilesCoveredByEllipse(Coords center, UInt16 radiusX, UInt16 radiusY)
        {
            if (PotentialOutOfBoundsEllipse(center, radiusX, radiusY))
            {
                return null;
            }

            LinkedList<Coords> returnValue = new LinkedList<Coords>();

            Coords current = new Coords((Int32)(center.X / _pixelsPerBoxX), (Int32)(center.Y / _pixelsPerBoxY));
            // add tile on which center-pixel lies (if it's passable)
            returnValue.AddLast(current);

            #region overlap to the right check
            if ((center.X + radiusX) / _pixelsPerBoxX > current.X) // overlap to the right
            {
                Coords tileRight = new Coords(current.X + 1, current.Y);

                returnValue.AddLast(tileRight);

                if ((center.Y + radiusY) / _pixelsPerBoxY > current.Y) // overlap also to the bottom
                {
                    Coords tileBottom = new Coords(current.X, current.Y + 1);

                    returnValue.AddLast(tileBottom);

                    Coords tileBottomRight = new Coords(current.X + 1, current.Y + 1); // bottom-right inspection
                    if (CollisionCheckPixelInEllipse(VertexTopLeft(tileBottomRight), center, radiusX, radiusY)) // we're inside!
                    {
                        returnValue.AddLast(tileBottomRight);
                    }
                }
                else if ((center.Y - radiusY) / _pixelsPerBoxY < current.Y) // overlap also to the top
                {
                    Coords tileTop = new Coords(current.X, current.Y - 1);

                    returnValue.AddLast(tileTop);

                    Coords tileTopRight = new Coords(current.X + 1, current.Y - 1); // top-right inspection
                    if (CollisionCheckPixelInEllipse(VertexBottomLeft(tileTopRight), center, radiusX, radiusY)) // we're inside!
                    {
                        returnValue.AddLast(tileTopRight);
                    }
                }
            }
            #endregion
            #region overlap to the left check
            else if ((center.X - radiusX) / _pixelsPerBoxX < current.X) // overlap to the left
            {
                Coords tileLeft = new Coords(current.X - 1, current.Y);

                returnValue.AddLast(tileLeft);

                if ((center.Y + radiusY) / _pixelsPerBoxY > current.Y) // overlap also to the bottom
                {
                    Coords tileBottom = new Coords(current.X, current.Y + 1);

                    returnValue.AddLast(tileBottom);

                    Coords tileBottomLeft = new Coords(current.X - 1, current.Y + 1); // bottom-left inspection
                    if (CollisionCheckPixelInEllipse(VertexTopRight( tileBottomLeft), center, radiusX, radiusY)) // we're inside!
                    {
                        returnValue.AddLast(tileBottomLeft);
                    }
                }
                else if ((center.Y - radiusY) / _pixelsPerBoxY < current.Y) // overlap also to the top
                {
                    Coords tileTop = new Coords(current.X, current.Y - 1);

                    returnValue.AddLast(tileTop);

                    Coords tileTopLeft = new Coords(current.X - 1, current.Y - 1); // top-left inspection
                    if (CollisionCheckPixelInEllipse(VertexBottonRight( tileTopLeft), center, radiusX, radiusY)) // we're inside!
                    {
                        returnValue.AddLast(tileTopLeft);
                    }
                }
            }
            #endregion
            #region in between
            else // still have to check Y
            {
                if ((center.Y + radiusY) / _pixelsPerBoxY > current.Y) // overlap also to the bottom
                {
                    Coords tileBottom = new Coords(current.X, current.Y + 1);

                    returnValue.AddLast(tileBottom);
                }
                else if ((center.Y - radiusY) / _pixelsPerBoxY < current.Y) // overlap also to the top
                {
                    Coords tileTop = new Coords(current.X, current.Y - 1);

                    returnValue.AddLast(tileTop);
                }
            }
            #endregion

            // clean-up

            foreach (Coords c in returnValue)
            {
                if (!CheckInBounds(c))
                {
                    returnValue.Remove(c);
                }
            }


            return returnValue;
        }

        /// <summary>
        /// checks whether Coords are at all on the map
        /// </summary> 
        public bool CheckInBounds(Coords point)
        {
            if ((point.X < 0) || (point.X >= this._sizeX))
                return false;
            if ((point.Y < 0) || (point.Y >= this._sizeY))
                return false;

            return true;
        }

        public bool CollisionCheckPixelInEllipse(Coords pixel, Coords center, UInt16 radiusX, UInt16 radiusY)
        {
            Int32 asquare = radiusX * radiusX;
            Int32 bsquare = radiusY * radiusY;
            return ((pixel.X - center.X) * (pixel.X - center.X) * bsquare + (pixel.Y - center.Y) * (pixel.Y - center.Y) * asquare) < (asquare * bsquare);
        }

        // returns true if the two circles collide
        private bool CollisionCheckEllipses(Coords center1, UInt16 radius1X, UInt16 radius1Y, Coords center2, UInt16 radius2X, UInt16 radius2Y)
        {
            UInt16 radiusSumX = (UInt16)(radius1X + radius2X);
            UInt16 radiusSumY = (UInt16)(radius1Y + radius2Y);
            // IS this even correct? It is for circles. Should be for ellipses.
            return CollisionCheckPixelInEllipse(center1, center2, radiusSumX, radiusSumY);
        }

        /// <summary>
        /// Checks if a pixel move in the given direction can send the Footballer out of bounds.
        /// </summary>
        public bool PotentialOutOfBoundsEllipse(Coords potentialPosition, UInt16 radiusX, UInt16 radiusY)
        {
            if ((potentialPosition.X - radiusX < 0) || (potentialPosition.Y - radiusY < 0) ||
                 (potentialPosition.X + radiusX >= _sizeX * _pixelsPerBoxX) || (potentialPosition.Y + radiusY >= _sizeY * _pixelsPerBoxY))
            {
                return true;
            }

            return false;
        }

        #endregion


        public Collider(UInt16 sizeX, UInt16 sizeY, UInt16 pixelsPerBoxX, UInt16 pixelsPerBoxY)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;
            _pixelsPerBoxX = pixelsPerBoxX;
            _pixelsPerBoxY = pixelsPerBoxY;

            _clippingBoxes = new List<Footballer>[_sizeX, _sizeY];
            for (int i = 0; i < _sizeX; ++i)
            {
                for (int j = 0; j < _sizeY; ++j)
                {
                    _clippingBoxes[i, j] = new List<Footballer>();
                }
            }

        }
    }
}
