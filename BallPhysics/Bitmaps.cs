using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BallPhysics
{
    public static class Bitmaps
    {
        public static string HomeDirectory = @"C:\Users\rado\Documents\Visual Studio 2010\Projects\BallPhysics\BallPhysics\";

        #region Tiles

        public static String[] Tiles = new String[] 
        { 
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_01.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_02.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_03.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_04.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_05.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_06.png",

        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_07.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_08.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_09.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_10.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_11.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_12.png",

        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_13.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_14.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_15.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_16.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_17.png",
        Bitmaps.HomeDirectory + @"Bitmaps\soccer-field-slanted-v2_18.png",
        };

        #endregion

        #region Footballers

        public static String[] Footballers = new String[] 
        { 
        Bitmaps.HomeDirectory + @"Bitmaps\Player.bmp",

        };

        #endregion

        #region Props
        
        public static String[] Props = new String[]
        {
            Bitmaps.HomeDirectory + @"Bitmaps\goal-left.bmp",
            Bitmaps.HomeDirectory + @"Bitmaps\goal-right.bmp"
        };

        public static String[] Balls = new String[]    
        {
            Bitmaps.HomeDirectory + @"Bitmaps\Ball.bmp",
            Bitmaps.HomeDirectory + @"Bitmaps\BallGhost.bmp"
        };

        #endregion
    }

    public enum SpriteTile : sbyte
    {
        Top1 = 0,
        Top2,
        Top3,
        Top4,
        Top5,
        Top6,

        Middle1,
        Middle2,
        Middle3,
        Middle4,
        Middle5,
        Middle6,

        Bottom1,
        Bottom2,
        Bottom3,
        Bottom4,
        Bottom5,
        Bottom6,
        COUNT
    }

    public enum SpriteBatchFootballer : sbyte
    {
        Player = 0,
        COUNT
    }

    public enum SpriteProp : sbyte
    {
        GoalLeft = 0,
        GoalRight,
        COUNT
    }

    public enum SpriteBall : sbyte
    {
        Standard = 0,
        Ghost,
        COUNT
    }
}
