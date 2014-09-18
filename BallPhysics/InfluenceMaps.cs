using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallPhysics
{
    // NOTE: Might be wiser to use integers instead of floats, for more accurate and faster arithmetic.
    // Let's see how it goes with floats.


    public class InfluenceMap
    {
        // The map itself. 2D array of floats
        private float[,] _map;
        public float[,] Map
        {
            get
            {
                return this._map;
            }

            // who gets responsibility for copying? Perhaps 'set' should be banned.
            set
            {
                this._map = value;
            }
        }

        public float GetMapValue(Coords number)
        {
            // Perhaps should throw exception
            return this._map[number.X, number.Y];
        }

        public void Substract(InfluenceMap substractor)
        {
            if (this.Map.GetLength(0) != substractor.Map.GetLength(0) || this.Map.GetLength(1) != substractor.Map.GetLength(1))
            {
                throw new Exception("attempted substraction of influence maps of different dimensions");
            }

            //InfluenceMap returnVal = new InfluenceMap((UInt16)map1.Map.GetLength(0), (UInt16)map1.Map.GetLength(1));

            for (int i = 0; i < this.Map.GetLength(0); ++i)
            {
                for (int j = 0; j < this.Map.GetLength(1); ++j)
                {
                    this.Map[i, j] -= substractor.Map[i, j];
                }
            }
        }

        public void Add(InfluenceMap summant)
        {
            if (this.Map.GetLength(0) != summant.Map.GetLength(0) || this.Map.GetLength(1) != summant.Map.GetLength(1))
            {
                throw new Exception("attempted summation of influence maps of different dimensions");
            }

            //InfluenceMap returnVal = new InfluenceMap((UInt16)map1.Map.GetLength(0), (UInt16)map1.Map.GetLength(1));

            for (int i = 0; i < this.Map.GetLength(0); ++i)
            {
                for (int j = 0; j < this.Map.GetLength(1); ++j)
                {
                    this.Map[i, j] += summant.Map[i, j];
                }
            }
        }

        public float SumInArea(Coords center, UInt16 radius)
        {
            float sum = 0;

            for (int i = center.X - radius; i <= center.X + radius; ++i)
            {
                for (int j = center.Y - radius; j <= center.Y + radius; ++j)
                {
                    if (i >= 0 && i < _map.GetLength(0) && j >= 0 && j < _map.GetLength(1))
                    {
                        sum += _map[i, j];
                    }
                }
            }

            return sum;
        }

        // Self-explanatory
        public void SetMapValue(Coords number, float newValue)
        {
            // Perhaps should throw exception
            this._map[number.X, number.Y] = newValue;
        }

        public InfluenceMap(UInt16 sizex, UInt16 sizey)
        {
            _map = new float[sizex, sizey];
        }
    }

    public class InfluenceSourceMap : InfluenceMap
    {
        private Coords _source;
        public Coords Source
        {
            get
            {
                return _source;
            }
        }

        private UInt16 _effectiveDistance;
        public UInt16 EffectiveDistance
        {
            get
            {
                return _effectiveDistance;
            }
        }

        public InfluenceSourceMap(UInt16 sizex, UInt16 sizey, Coords source, UInt16 distance) : base(sizex, sizey)
        {
            _source = source;
            _effectiveDistance = distance;
        }
    }


    /// <summary>
    /// Generates an influence map from a given source, using a given spread function
    /// There is no 'strength' parameter - let the objects associated with the influence map
    /// have their own parameter for this. Influence is 1 at the source and decreasing as one moves away.
    /// </summary>
    public class InfluenceMapGenerator
    {
        /// <summary>
        /// Calculates influence 'distance' units from the source. Should be a [0,1] 
        /// monotonic decreasing function. Perhaps should provide some default, in
        /// case the constructor fails to initialize one.
        /// </summary>
        public delegate float InfluenceSpreadFunction(UInt32 distance);

        /// <summary>
        /// Generates the influence map.
        /// Uses a silly recursive algorithm.
        /// Stopping conditions: Let's use two, to avoid stupid infinite loops.
        /// One is a distance threshold check.
        /// Second is a min influence threshold check.
        /// </summary>
        public float[,] GenerateInfluenceMap(UInt16 sizex, UInt16 sizey, Coords source, InfluenceSpreadFunction f)
        {
            float[,] influenceMap = new float[sizex, sizey];

            // boolean array to keep note of which tiles have been processed
            BitArray[] takenCareOf = new BitArray[sizex];
            for (int i = 0; i < sizex; ++i)
            {
                takenCareOf[i] = new BitArray(sizey);
            }
            takenCareOf[source.X][source.Y] = true;

            // sets up two queues - one for the current pass, one for the next one
            // distance increments by one at each pass
            // if too slow, the process should be broken up so it does a number of passes each tick
            Queue<Coords> currentQueue = new Queue<Coords>();
            Queue<Coords> nextQueue = new Queue<Coords>();

            currentQueue.Enqueue(source);

            UInt32 currentDistance = 0;

            // main loop
            // Stopping conditions: the two queues are exhausted, OR InfluenceMapMaxDistance is reached
            while
                (
                ((currentQueue.Count > 0) & (nextQueue.Count > 0))
                |
                (currentDistance < Constants.InfluenceMapMaxDistance)
                )
            {
                // Checks if it's time to start the next pass
                if (currentQueue.Count == 0)
                {
                    currentQueue = nextQueue;
                    nextQueue = new Queue<Coords>();
                    currentDistance++;
                    continue;
                }

                Coords currentCoords = currentQueue.Peek();

                // Analyzes the neighbors of the current Tile for possible additions to nextQueue
                for (byte i = 0; i < 8; i++)
                {
                    Direction currentDir = (Direction)i;
                    Coords toCheck = StaticMathFunctions.CoordsNeighboringInDirection(currentCoords, currentDir);
                    if (toCheck.X >= 0 && toCheck.X < sizex && toCheck.Y >= 0 && toCheck.Y < sizey)
                    {
                        if (!takenCareOf[toCheck.X][toCheck.Y])
                        {
                            nextQueue.Enqueue(toCheck);
                            takenCareOf[toCheck.X][toCheck.Y] = true;
                        }
                    }
                }

                float newVal = f(currentDistance);

                // Check to avert infnite / excessively deep loop
                if (newVal > Constants.InfluenceMapMinThreshold)
                {
                    influenceMap[currentCoords.X, currentCoords.Y] = newVal;
                }

                currentQueue.Dequeue();
            }

            return influenceMap;
        }

        /// <summary>
        /// The passed 'updater' is generic. It was added to the 'map' at oldsource. We want to substract it, and add it at
        /// newSource.
        /// </summary>
        public void UpdateMapViaSourceMap(InfluenceMap map, InfluenceSourceMap updater, Coords oldSource, Coords newSource)
        {
            if (oldSource == newSource)
            {
                return;
            }

            InfluenceSourceMap oldInfMap = ShiftInfluenceSourceMap(updater, oldSource);
            InfluenceSourceMap newInfMap = ShiftInfluenceSourceMap(updater, newSource);

            map.Substract(oldInfMap);
            map.Add(newInfMap);
        }


        public InfluenceSourceMap ShiftInfluenceSourceMap(InfluenceSourceMap map, Coords newSource)
        {
            Int32 lengthX = map.Map.GetLength(0);
            Int32 lengthY = map.Map.GetLength(1);

            Coords delta = newSource - map.Source;

            float[,] returnMap = new float[lengthX, lengthY];

            InfluenceSourceMap returnVal = new InfluenceSourceMap((UInt16)lengthX, (UInt16)lengthY, newSource, map.EffectiveDistance);

            for (int i = 0; i < lengthX; ++i)
            {
                for (int j = 0; j < lengthY; ++j)
                {
                    Int32 shiftedX = i - delta.X;
                    Int32 shiftedY = j - delta.Y;

                    if (shiftedX >= 0 && shiftedX < lengthX && shiftedY >= 0 && shiftedY < lengthY)
                    {
                        returnMap[i, j] = map.Map[i - delta.X, j - delta.Y];
                    }
                }
            }

            returnVal.Map = returnMap;
            return returnVal;
        }

        public InfluenceMapGenerator()
        {            
        }

    }
}
