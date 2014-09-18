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
    /// Tile class. Represents one square of the map.
    /// </summary>
    public class Tile
    {
        #region Properties
        
        // Coords of the Tile
        private Coords _position;
        public Coords Position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
            }
        }
        
        // Reference to the Map. 
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

        protected SpriteTile _myBitmap;
        public SpriteTile MyBitmap
        {
            get
            {
                return _myBitmap;
            }
        }

        #endregion

        #region Constructors

        private Tile(Map home, Coords position)
        {
            this.InhabitedMap = home;
            this.Position = position;
        }

        public Tile(Map home, Coords position, SpriteTile tileBitmap)
            : this(home, position)
        {
            this._myBitmap = tileBitmap;
        }

        /*
        public Tile(Map home, Coords position, TileGenerator generator) : this(home, position)
        {
            this._myBitmap = generator.tileBitmap;
        }
        */

        #endregion
    }


}
