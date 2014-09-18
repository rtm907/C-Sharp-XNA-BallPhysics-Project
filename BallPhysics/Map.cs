using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;

namespace BallPhysics
{
    /// <summary>
    /// Map class. The map is a double array of Tiles.
    /// Contains the Pathfinding algos, such as A* and Bresenheim lines.
    /// Contains Checking methods for tile visibility/ walkability/ in-bounds.
    /// </summary>
    public class Map
    {
        #region Properties

        // Map dimensions
        private UInt16 _xMax;
        /// <summary>
        /// The X bound of this map.
        /// </summary>
        public UInt16 BoundX
        {
            get
            {
                return _xMax;
            }
        }
        private UInt16 _yMax;
        /// <summary>
        /// The Y bound of this map.
        /// </summary>
        public UInt16 BoundY
        {
            get
            {
                return _yMax;
            }
        }

        private UInt32 _pixelMaxX;
        public UInt32 PixelBoundX
        {
            get
            {
                return _pixelMaxX;
            }
        }
        private UInt32 _pixelMaxY;
        public UInt32 PixelBoundY
        {
            get
            {
                return _pixelMaxY;
            }
        }

        private Tile[,] _tiles;
        /// <summary>
        /// Double Tile array representing the map
        /// </summary>
        public Tile[,] Tiles
        {
            get
            {
                return this._tiles;
            }
        }

        // Painter reference
        private Painter _myPainter;
        public Painter MyPainter
        {
            get
            {
                return _myPainter;
            }
            set
            {
                this._myPainter = value;
            }
        }

        private Collider _myCollider;
        public Collider MyCollider
        {
            get
            {
                return _myCollider;
            }
        }

        private InfluenceMapGenerator _infMapGenerator;
        public InfluenceMapGenerator InfMapGenerator
        {
            get
            {
                return _infMapGenerator;
            }
            set
            {
                _infMapGenerator = value;
            }
        }

        private InfluenceSourceMap _defaultInfSourceMap;
        public InfluenceSourceMap DefaultInfSourceMap
        {
            get
            {
                return _defaultInfSourceMap; 
            }
        }

        private Match _matchOnMap;
        public Match MatchOnMap
        {
            get
            {
                return _matchOnMap;
            }
            set
            {
                _matchOnMap = value;
            }
        }

        /*
        private InfluenceMap[] _teamInfluenceMaps = new InfluenceMap[2];
        public InfluenceMap TeamInfluenceMap(FootballerTeam team)
        {
            return _teamInfluenceMaps[(sbyte)team];
        }
        */

        // Counts spawned Footballers. Used to issue unique IDs. Consider moving this to some kind of a 
        // 'world' or 'game' type.
        private UInt32 _FootballerCount = 0;
        public UInt32 FootballerCount
        {
            get
            {
                return _FootballerCount;
            }
        }

        private UInt32 _itemCount = 0;
        public UInt32 ItemCount
        {
            get
            {
                return this._itemCount;
            }
        }

        private Ball _ballReference;
        public Ball BallReference
        {
            get
            {
                return this._ballReference;
            }
            set
            {
                this._ballReference = value;
            }
        }

        private RandomStuff _randomator;
        /// <summary>
        /// Reference to the random number generator for this map
        /// </summary>
        public RandomStuff Randomator
        {
            get
            {
                return this._randomator;
            }
            set
            {
                this._randomator = value;
            }
        }

        private SortedDictionary<UInt32, Footballer> _roster = new SortedDictionary<UInt32, Footballer>();
        /// <summary>
        /// Monsters belonging to the map.
        /// </summary>
        public SortedDictionary<UInt32, Footballer> Roster
        {
            get
            {
                return this._roster;
            }
            set
            {
                this._roster = value;
            }
        }

        //public SortedList<UInt32, Footballer>[] _teamRoster = new SortedList<uint, Footballer>[2];

        /*
        public SortedList<UInt32, Footballer> TeamRoster(FootballerTeam team)
        {
            return _teamRoster[(sbyte)team];
        }*/

        #endregion

        #region Methods
        public Tile GetTile(Coords coords)
        {
            return _tiles[coords.X, coords.Y];
        }
        public Tile GetTile(Int32 X, Int32 Y)
        {
            return _tiles[X, Y];
        }
        public void SetTile(Coords coords, Tile newValue)
        {
            _tiles[coords.X, coords.Y] = newValue;
        }

        #region Footballer / Item registers

        /// <summary>
        /// Returns the Footballer with ID 'key'
        /// </summary>
        public Footballer RosterGetFootballerFrom(UInt32 key)
        {
            return this._roster[key];
        }
        
        
        /// <summary>
        /// Add Footballer to the menagerie. They 'key' is the Footballer ID.
        /// </summary>
        public void RosterAddFootballerTo(UInt32 key, Footballer newGuy)
        {
            this._roster[key] = newGuy;
            //this._teamRoster[(sbyte)newGuy.Team][key] = newGuy;
        }
        

        /// <summary>
        /// Issues ID to a Footballer.
        /// </summary>
        public UInt32 IssueFootballerID()
        {
            return this._FootballerCount++;
        }

        #endregion

        #region Raytracers

        /// <summary>
        /// Returns the tiles under the given line.
        /// Borrowed from: http://playtechs.blogspot.com/2007/03/raytracing-on-grid.html (James McNeill)
        /// </summary>
        public List<Coords> RayTracer(Coords c1, Coords c2)
        {
            List<Coords> returnVal = new List<Coords>();

            Int32 x0 = c1.X;
            Int32 y0 = c1.Y;
            Int32 x1 = c2.X;
            Int32 y1 = c2.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int x = x0;
            int y = y0;
            int n = 1 + dx + dy;
            int x_inc = (x1 > x0) ? 1 : -1;
            int y_inc = (y1 > y0) ? 1 : -1;
            int error = dx - dy;
            dx *= 2;
            dy *= 2;

            for (; n > 0; --n)
            {
                returnVal.Add(new Coords(x, y));

                if (error > 0)
                {
                    x += x_inc;
                    error -= dy;
                }
                else
                {
                    y += y_inc;
                    error += dx;
                }
            }

            return returnVal;
        }

        /*
        /// <summary>
        /// Performs a terrain passability check betwee two points by doing pixel validity checks at interval delta.
        /// </summary>
        public CollisionType RayTracerPassabilityCheckRough(Footballer client, Vector v1, Vector v2, double delta)
        {
            Vector difference = v2-v1;
            Vector deltaV = difference;
            deltaV.ScaleToLength(delta);

            Vector currentPosition = v1;

            for (int i = 0; i < difference.Length() / deltaV.Length(); ++i)
            {
                Coords pixel = new Coords(currentPosition);
                currentPosition += deltaV;
            }

            return CollisionType.None;
        }

        /// <summary>
        /// Returns the Bresenham line between p0 and p1; Borrowed the code
        /// from some dude whose name I don't have, who in turn borrowed from Wikipedia.
        /// </summary>
        private List<Coords> BresenhamLine(Coords p0, Coords p1)
        {
            List<Coords> returnList = new List<Coords>();

            Boolean steep = Math.Abs(p1.Y - p0.Y) > Math.Abs(p1.X - p0.X);

            if (steep == true)
            {
                Coords tmpPoint = new Coords(CoordsType.Tile,p0.X, p0.Y);
                p0 = new Coords(CoordsType.Tile,tmpPoint.Y, tmpPoint.X);

                tmpPoint = p1;
                p1 = new Coords(CoordsType.Tile,tmpPoint.Y, tmpPoint.X);
            }

            Int32 deltaX = Math.Abs(p1.X - p0.X);
            Int32 deltaY = Math.Abs(p1.Y - p0.Y);
            Int32 error = 0;
            Int32 deltaError = deltaY;
            Int32 yStep = 0;
            Int32 xStep = 0;
            Int32 y = p0.Y;
            Int32 x = p0.X;

            if (p0.Y < p1.Y)
            {
                yStep = 1;
            }
            else
            {
                yStep = -1;
            }

            if (p0.X < p1.X)
            {
                xStep = 1;
            }
            else
            {
                xStep = -1;
            }

            Int32 tmpX = 0;
            Int32 tmpY = 0;

            while (x != p1.X)
            {

                x += xStep;
                error += deltaError;

                //if the error exceeds the X delta then
                //move one along on the Y axis
                if ((2 * error) > deltaX)
                {
                    y += yStep;
                    error -= deltaX;
                }

                //flip the coords if they're steep
                if (steep)
                {
                    tmpX = y;
                    tmpY = x;
                }
                else
                {
                    tmpX = x;
                    tmpY = y;
                }

                //check the point generated is legal
                //and if it is add it to the list
                if (this.CheckInBounds(new Coords(CoordsType.Tile,tmpX, tmpY)) == true)
                {
                    returnList.Add(new Coords(CoordsType.Tile,tmpX, tmpY));
                }
                else
                {   //a bad point has been found, so return the list thus far
                    return returnList;
                }

            }

            return returnList;
        }
        */
        #endregion

        #region Item / Footballer spawners

        /// <summary>
        /// Spawns the player on the 'ground' at 'startPoint'
        /// returns a reference to the Player so one can more easily take care of references.
        /// </summary>
        public Footballer SpawnFootballer(Vector2d startPoint, Vector2d defaultPlayingPosition, Team team)
        {
            Footballer newGuy = new Footballer(this, startPoint, defaultPlayingPosition, this.IssueFootballerID(), team);

            Coords infMapPosition = new Coords((Int32)(startPoint.X / Constants.InfMapDefaultBoxSizeX), (Int32)(startPoint.Y / Constants.InfMapDefaultBoxSizeY));
            InfluenceSourceMap updater = _infMapGenerator.ShiftInfluenceSourceMap(_defaultInfSourceMap, infMapPosition);

            newGuy.Team.TeamInfluenceMap.Add(updater);
            //_teamInfluenceMaps[(sbyte)team].Add( updater);
            newGuy.PositionAtLastInfMapUpdate = infMapPosition;

            return newGuy;
        }

        public Team SpawnTeam(bool goingLeft, Color teamColor)
        {
            List<Footballer> roster = new List<Footballer>();
            Team newTeam = new Team(roster, new InfluenceMap(Constants.InfMapDefaultX, Constants.InfMapDefaultY), _matchOnMap, goingLeft, teamColor);

            for (int i = 0; i<11; ++i)
            {
                Vector2d position = new Vector2d(Constants.GoalLeft + Constants.DefaultPositions[i].Y * (0.5 * Constants.ActualXMax - Constants.GoalLeft),
                    Constants.DefaultPositions[i].X * Constants.ActualYMax);
                if (!goingLeft)
                {
                    position = new Vector2d(Constants.ActualXMax - position.X, Constants.ActualYMax - position.Y);
                }
                SpawnFootballer(position, Constants.DefaultPositions[i], newTeam);
            }

            return newTeam;
        }



        #endregion

        #endregion

        #region Constructors

        // Constructs an xSize by ySize map. Default Tile set to TileBasicFloor.
        public Map(Int32 seed)
        {
            // creates and fills the tile array
            this._xMax = Constants.MapSizeX;
            this._yMax = Constants.MapSizeY;

            // Calculates maximal bounds in pixels.
            this._pixelMaxX = 0;
            for (Int32 i = 0; i < Constants.MapSizeX; ++i)
            {
                _pixelMaxX += Constants.TileSizesX[i];
            }
            this._pixelMaxY = (UInt32)(Constants.MapSizeY);
            for (Int32 i = 0; i < Constants.MapSizeY; ++i)
            {
                _pixelMaxY += Constants.TileSizesY[i];
            }

            // Constructs the field.
            _tiles = new Tile[Constants.MapSizeX, Constants.MapSizeY];
            for (Int32 i = 0; i < Constants.MapSizeX; i++)
            {
                for (Int32 j = 0; j < Constants.MapSizeY; j++)
                {
                    _tiles[i, j] = new Tile(this, new Coords(i, j), (SpriteTile)(i + j * Constants.MapSizeX));
                }
            }

            // initializes collider
            this._myCollider = new Collider(Constants.BoxesInX, Constants.BoxesInY, Constants.PixelsPerBoxX, Constants.PixelsPerBoxY);

            // intialize influence map generator
            this._infMapGenerator = new InfluenceMapGenerator();

            // initialize default source map
            this._defaultInfSourceMap = new InfluenceSourceMap(Constants.InfMapDefaultX, Constants.InfMapDefaultY,
                new Coords(Constants.InfMapDefaultX/2, Constants.InfMapDefaultY/2), Constants.InfluenceMapMaxDistance);
            this._defaultInfSourceMap.Map = this.InfMapGenerator.GenerateInfluenceMap(Constants.InfMapDefaultX, Constants.InfMapDefaultY,
                new Coords(Constants.InfMapDefaultX/2, Constants.InfMapDefaultY/2), StaticMathFunctions.InfluenceDecayFunctionLinear);

            //this._teamInfluenceMaps[0] = new InfluenceMap(Constants.InfMapDefaultX, Constants.InfMapDefaultY);
            //this._teamInfluenceMaps[1] = new InfluenceMap(Constants.InfMapDefaultX, Constants.InfMapDefaultY);

            // initializes the random number generator associated with this map
            this._randomator = new RandomStuff(seed);
        }

        #endregion
    }
}
