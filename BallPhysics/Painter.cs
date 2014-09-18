using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace BallPhysics
{
    /// <summary>
    /// Deals with the screen display algo.
    /// </summary>
    public class Painter
    {
        // What the painter draws is a portion of some map.
        private Map _currentMap;
        // MainFrame reference for access to user interface
        private MainFrame _mainFrame;

        private float _zoom;
        public float Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = value;
                RescaleSprites();
            }
        }

        private Bitmap[] DefaultTiles = new Bitmap[(Int32) SpriteTile.COUNT];
        private Bitmap[] DefaultFootballers = new Bitmap[(Int32)SpriteBatchFootballer.COUNT];
        private Bitmap[] DefaultProps = new Bitmap[(Int32) SpriteProp.COUNT];
        private Bitmap[] DefaultBalls = new Bitmap[(Int32) SpriteBall.COUNT];

        private Bitmap[] Tiles = new Bitmap[(Int32)SpriteTile.COUNT];
        private Bitmap[] Footballers = new Bitmap[(Int32)SpriteBatchFootballer.COUNT];
        private Bitmap[] Props = new Bitmap[(Int32)SpriteProp.COUNT];
        private Bitmap[] Balls = new Bitmap[(Int32) SpriteBall.COUNT];

        private BallGhost _ghost;
        public BallGhost Ghost
        {
            get
            {
                return _ghost;
            }
            set
            {
                _ghost = value;
            }
        }

        private void ImportSprites()
        {
            for (sbyte i = 0; i < this.DefaultTiles.Length; ++i)
            {
                Bitmap current = new Bitmap(Bitmaps.Tiles[i]);
                DefaultTiles[i] = current;
            }

            for (sbyte i = 0; i < this.DefaultFootballers.Length; ++i)
            {
                Bitmap current = new Bitmap(Bitmaps.Footballers[i]);
                current.MakeTransparent(current.GetPixel(0, 0));
                DefaultFootballers[i] = current;
            }

            for (sbyte i = 0; i < this.DefaultProps.Length; ++i)
            {
                Bitmap current = new Bitmap(Bitmaps.Props[i]);
                current.MakeTransparent(current.GetPixel(0, 0));
                DefaultProps[i] = current;
            }

            for (sbyte i = 0; i < this.DefaultBalls.Length; ++i)
            {
                Bitmap current = new Bitmap(Bitmaps.Balls[i]);
                current.MakeTransparent(current.GetPixel(0, 0));
                DefaultBalls[i] = current;
            }
        }

        private void RescaleSprites()
        {
            for (sbyte i = 0; i < this.DefaultTiles.Length; ++i)
            {
                int ind1 = i % Constants.TileSizesX.Length;
                int ind2 = i / Constants.TileSizesX.Length;

                Bitmap current = new Bitmap(DefaultTiles[i], (Int32)Math.Ceiling((Constants.TileSizesX[i % Constants.TileSizesX.Length]) * _zoom),
                    (Int32)Math.Ceiling((Constants.TileSizesY[i / Constants.TileSizesX.Length]) * _zoom));
                Tiles[i] = current;
            }

            for (sbyte i = 0; i < this.DefaultFootballers.Length; ++i)
            {
                Footballers[i] = new Bitmap(DefaultFootballers[i], (Int32)Math.Ceiling(DefaultFootballers[i].Width * _zoom),
                    (Int32)Math.Ceiling(DefaultFootballers[i].Height * _zoom));
            }

            for (sbyte i = 0; i < this.DefaultProps.Length; ++i)
            {
                Props[i] = new Bitmap(DefaultProps[i], (Int32)Math.Ceiling(DefaultProps[i].Width * _zoom),
                    (Int32)Math.Ceiling(DefaultProps[i].Height * _zoom));
            }

            for (sbyte i = 0; i < this.DefaultBalls.Length; ++i)
            {
                Balls[i] = new Bitmap(DefaultBalls[i], (Int32)Math.Ceiling(DefaultBalls[i].Width * _zoom),
                    (Int32)Math.Ceiling(DefaultBalls[i].Height * _zoom));
            }
        }

        // List of Footballers to be painted on a particular tick.
        private List<Footballer> _FootballersToDraw;
        public void AddForPaintingFootballer(Footballer critter)
        {
            this._FootballersToDraw.Add(critter);
        }

        private void IsometricTransform(Graphics g)
        {
            g.RotateTransform(45f);
            g.ScaleTransform((float)Math.Sqrt(1.5), (float)Math.Sqrt(0.5));
        }

        /// <summary>
        /// Coords the painting of the portion of the map between the two Coords parameters.
        /// </summary>
        public void Paint(Graphics g, Coords topLeft, Coords bottomRight)
        {
            // The algorithm is as follows:
            // 1) AT INIT: Imports and resizes the bitmaps.
            // 2) AT ZOOM: Resizes bitmaps.
            // 3) AT PAINT: The Form determines what portion of the map should be painted and calls Painter.
            // Painter goes through the Tiles and draws them, row by row, from left-to-right from 
            // top-to-bottom.

            #region Tiles

            Int32 pixelsX = 0;
            // We assume the validity check for the two coords has been done in the caller class.
            for (Int32 i = 0; i < Constants.MapSizeX; ++i)
            {
                if (i > 0)
                {
                    pixelsX += Constants.TileSizesX[i-1];
                }


                Int32 pixelsY = 0;
                for (Int32 j = 0; j < Constants.MapSizeY; ++j)
                {
                    Coords currentCoords = new Coords(i, j);
                    Tile currentTile = this._currentMap.GetTile(i,j);

                    if (j > 0)
                    {
                        pixelsY += Constants.TileSizesY[j-1];
                    }

                    this.TileDrawBitmap(g, new Coords(pixelsX, pixelsY), this.Tiles[(sbyte)currentTile.MyBitmap]);


                    foreach (Footballer critter in _currentMap.Roster.Values)
                    {
                        this.AddForPaintingFootballer(critter);
                    }
                }
            }

            #endregion

            #region Props and Footballers

            // The tiles have informed the painter about the Footballers he's supposed to draw.
            foreach (Footballer critter in this._FootballersToDraw)
            {
                this.FillPlayerEllipse(g, critter.PositionDouble, 10, 8, critter.Team.TeamColor);

                this.DrawBitmapAtPixel(g, new Coords((Int32) critter.PositionDouble.X, (Int32)critter.PositionDouble.Y), 
                    this.Footballers[(sbyte)critter.MyBitmap]);
                
                Vector2d delta = critter.FacingDirection;
                delta.ScaleToLength(50); 
                this.DrawLine(g, critter.PositionDouble, critter.PositionDouble + delta);
            }

            // Draw the labels
            foreach (Footballer critter in this._FootballersToDraw)
            {
                //FIX
                // draw labels
                if ((critter.LabelUpper != null) && (critter.LabelUpper.Length > 0))
                {
                    this.DrawLabel(g, critter.PositionDouble + new Vector2d(0, -200), critter.LabelUpper);
                }

                if ((critter.LabelLower != null) && (critter.LabelLower.Length > 0))
                {
                    //this.DrawLabel(g, new Coords(CoordsType.Pixel, critter.PositionPixel.X, critter.PositionPixel.Y + critter.RadiusY), critter.LabelLower);
                }
            }

            for (int i = 0; i < Props.Length; ++i)
            {
                DrawProp(g, Constants.PropLocations[i], Props[i]);
            }

            #endregion

            #region Nets, Ball, and Info

            // DrawGhostBall if option is enabled
            if (this._ghost !=null)
            {
                //BallGhost ghost = new BallGhost(this._currentMap.BallReference);
                for (int i = 0; i < Constants.GhostBallTimeLength; ++i)
                {
                    for (int j = 0; j < Constants.GhostBallDrawInterval; ++j)
                    {
                        _ghost.UpdateMotion3D();
                    }
                    this.DrawGhostBall(g, _ghost);
                }
            }

            if (Constants.ShowTrajectory)
            {
                if (this._currentMap.BallReference.Projection.Count > 0)
                {

                    foreach (Vector3d v in this._currentMap.BallReference.Projection)
                    {
                        BallGhost ghost = new BallGhost(this._currentMap.BallReference);
                        //ghost.SpawnAt(v.ProjectionXY());
                        ghost.Stop();
                        ghost.Position3d = v;
                        this.DrawGhostBall(g, ghost);
                    }
                }
            }

            BallPosition ballPos = DetermineBallPosition(_currentMap.BallReference);

            // Nets
            if (ballPos == BallPosition.BehindGoal)
            {
                this.DrawSomeBall(g, _currentMap.BallReference, this.Balls[(sbyte)SpriteBall.Standard]);
            }
            DrawNetYAxis(g, Constants.NetsBackLeft);
            DrawNetYAxis(g, Constants.NetsBackRight);
            DrawNetXAxis(g, Constants.NetsBackLeft, Constants.GoalTop);
            DrawNetXAxis(g, Constants.GoalRight, Constants.GoalTop);
            if (ballPos == BallPosition.InsideGoal)
            {
                this.DrawSomeBall(g, _currentMap.BallReference, this.Balls[(sbyte)SpriteBall.Standard]);
            }
            DrawNetXAxis(g, Constants.NetsBackLeft, Constants.GoalBottom);
            DrawNetXAxis(g, Constants.GoalRight, Constants.GoalBottom);
            DrawNetZAxis(g, Constants.NetsBackLeft);
            DrawNetZAxis(g, Constants.GoalRight);
            DrawGoals(g);
            if (ballPos == BallPosition.Visible)
            {
                this.DrawSomeBall(g, _currentMap.BallReference, this.Balls[(sbyte)SpriteBall.Standard]);
            }


            DrawBallParameters(g, topLeft);
            DrawShotParameters(g, topLeft);

            this.DrawInfluenceMap(g, _currentMap.MatchOnMap.TeamLeft.TeamInfluenceMap, topLeft, _currentMap.MatchOnMap.TeamLeft.TeamColor, true);
            this.DrawInfluenceMap(g, _currentMap.MatchOnMap.TeamRight.TeamInfluenceMap, topLeft, _currentMap.MatchOnMap.TeamRight.TeamColor, false);

            #endregion

            // Clean up the ID list.
            this._FootballersToDraw.Clear();
        }

        private BallPosition DetermineBallPosition(Ball theBall)
        {
            if (theBall.Position3d.Y < Constants.GoalTop && 
                (theBall.Position3d.X < Constants.GoalLeft || theBall.Position3d.X > Constants.GoalRight))
            {
                return BallPosition.BehindGoal;
            }

            if (theBall.Position3d.Y < Constants.GoalBottom && 
                (theBall.Position3d.X < Constants.NetsBackLeft || theBall.Position3d.X > Constants.NetsBackRight))
            {
                return BallPosition.BehindGoal;
            }

            if ((theBall.Position3d.Y < Constants.GoalBottom && theBall.Position3d.Y > Constants.GoalTop) &&
                ((theBall.Position3d.X > Constants.NetsBackLeft && theBall.Position3d.X < Constants.GoalLeft) ||
                (theBall.Position3d.X > Constants.GoalRight && theBall.Position3d.X < Constants.NetsBackRight)))
            {
                return BallPosition.InsideGoal;
            }

            return BallPosition.Visible;
        }

        private void DrawInfluenceMap(Graphics g, InfluenceMap map, Coords topLeft, Color teamColor, bool left)
        {
            float[,] actualMap = map.Map;

            sbyte teamVal = left ? (sbyte)0 : (sbyte)1;

            PointF anchor = new PointF(teamVal * 0.6f * _mainFrame.Width + topLeft.X * this._zoom, topLeft.Y * this._zoom + 0.6f * _mainFrame.Height);

            PointF boxSize = new PointF(0.4f * _mainFrame.Width / actualMap.GetLength(0), 0.4f * _mainFrame.Height / actualMap.GetLength(1));

            for (int i = 0; i < actualMap.GetLength(0); ++i)
            {
                for (int j = 0; j < actualMap.GetLength(1); ++j)
                {
                    Color TransparentColor = Color.FromArgb((Int32)(actualMap[i, j] * 50), teamColor.R, teamColor.G, teamColor.B);

                    Brush myBrush = new SolidBrush(TransparentColor);

                    g.FillRectangle(myBrush, anchor.X + i * boxSize.X, anchor.Y + j * boxSize.Y, boxSize.X, boxSize.Y);
                }
            }
        }

        private void DrawProp(Graphics g, Coords topLeft, Bitmap image)
        {
            Point anchor = new Point((Int32)(topLeft.X * _zoom), (Int32)(topLeft.Y * _zoom));
            g.DrawImageUnscaled(image, new Point(anchor.X, anchor.Y));
        }

        private void DrawSomeBall(Graphics g, Ball someball, Image ballImage)
        {
            Coords ballPos = StaticMathFunctions.SpacePointToDisplayPointTransform(someball.Position3d.X, someball.Position3d.Y);

            Point anchor = new Point((Int32)(ballPos.X * _zoom), (Int32)(ballPos.Y * _zoom));

            Int32 radiusX = (Int32)(0.5 * ballImage.Width);
            Int32 radiusY = (Int32)(0.5 * ballImage.Height);

            double zmultiplier = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(someball.Position3d.Y);

            Int32 Zoffset = (Int32)(someball.Position3d.Z * _zoom * zmultiplier);

            Color color = Color.Black;
            Color TransparentColor = Color.FromArgb(150, color.R, color.G, color.B);

            // draw shadow.
            g.FillEllipse(new SolidBrush(TransparentColor), (Int32)(anchor.X - radiusX * zmultiplier), (Int32)(anchor.Y),
                (Int32)(radiusX * 2 * zmultiplier), (Int32)(radiusY * zmultiplier));

            // draw ball
            g.DrawImage(ballImage, (float)(anchor.X - radiusX * zmultiplier), (float)(anchor.Y - (radiusY) * zmultiplier) - Zoffset,
                (float)(2 * radiusX * zmultiplier), (float)(2 * radiusX * zmultiplier));
        }

        private void FillPlayerEllipse(Graphics g, Vector2d center, double radx, double rady, Color teamColor)
        {
            Coords transformedCenter = StaticMathFunctions.SpacePointToDisplayPointTransform(center.X, center.Y);

            Point anchor = new Point((Int32)(transformedCenter.X * _zoom), (Int32)(transformedCenter.Y * _zoom));

            Color TransparentColor = Color.FromArgb(80, teamColor.R, teamColor.G, teamColor.B);
            Brush myBrush = new SolidBrush(TransparentColor);

            g.FillEllipse(myBrush, (Int32)(anchor.X - radx), (Int32)(anchor.Y - rady),
                (Int32)(radx * 2), (Int32)(rady * 2));

            myBrush.Dispose();
        }

        // The 'position' is supposed to be topLeft
        private void DrawBallParameters(Graphics g, Coords position)
        {
            Ball ballRef = _currentMap.BallReference;
            StringFormat strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Near;
            PointF anchor = new PointF(position.X * this._zoom, position.Y * this._zoom);
            Font font = new Font("Tahoma", Constants.FontSize);
            Brush b = Brushes.Black;

            Int32 space = 3;

            g.DrawString("POSITION X:" + ballRef.Position3d.X, font, b, anchor, strFormat);
            g.DrawString("POSITION Y:" + ballRef.Position3d.Y, font, b, anchor.X, anchor.Y + (Constants.FontSize + space), strFormat);
            g.DrawString("POSITION Z:" + ballRef.Position3d.Z, font, b, anchor.X, anchor.Y + 2*(Constants.FontSize + space), strFormat);
            g.DrawString("VELOCITY X,Y:" + ballRef.Velocity, font, b, anchor.X, anchor.Y + 3 * (Constants.FontSize + space), strFormat);
            g.DrawString("SPIN:" + ballRef.Spin3d, font, b, anchor.X, anchor.Y + Constants.FontSize + 4 * (Constants.FontSize + space), strFormat);
        }

        // 'Position' should be top Right
        private void DrawShotParameters(Graphics g, Coords position)
        {
            StringFormat strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Near;
            PointF anchor = new PointF(position.X * this._zoom + _mainFrame.Width - 200, position.Y * this._zoom);
            Font font = new Font("Tahoma", Constants.FontSize);
            Brush b = Brushes.Black;

            Int32 space = 3;
            g.DrawString("TOP SPIN / SLICE: " + _mainFrame.SpinY, font, b, anchor, strFormat);
            g.DrawString("LEFT / RIGHT SPIN: " + _mainFrame.SpinZ, font, b, anchor.X, anchor.Y + (Constants.FontSize + space), strFormat);
            g.DrawString("SHOT POWER: " + _mainFrame.ShotPower, font, b, anchor.X, anchor.Y + 2 * (Constants.FontSize + space), strFormat);
            g.DrawString("SHOT MODE: " + _mainFrame.ModeOfKicking, font, b, anchor.X, anchor.Y + 3 * (Constants.FontSize + space), strFormat);
            //g.DrawString("SPIN:" + ballRef.Spin3d, font, b, anchor.X, anchor.Y + Constants.FontSize + 4 * (Constants.FontSize + space), strFormat);
        }

        private void DrawGhostBall(Graphics g, BallGhost ghost)
        {
            this.DrawSomeBall(g, ghost, this.Balls[(sbyte) SpriteBall.Ghost]);
        }

        private void DrawGoals(Graphics g)
        {
            Point[] uprights = new Point[8];
            for (int i = 0; i < 4; ++i)
            {
                Coords temp = StaticMathFunctions.SpacePointToDisplayPointTransform(Constants.Uprights[i]);
                uprights[i] = new Point((Int32)(temp.X * _zoom),(Int32) (temp.Y*_zoom));
            }

            double zmultiplierTop = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Constants.GoalTop);
            double zmultiplierBottom = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Constants.GoalBottom);

            uprights[4] = new Point(uprights[0].X, (Int32)(uprights[0].Y - Constants.GoalHeight*zmultiplierTop*_zoom));
            uprights[5] = new Point(uprights[1].X, (Int32)(uprights[1].Y - Constants.GoalHeight * zmultiplierBottom * _zoom));
            uprights[6] = new Point(uprights[2].X, (Int32)(uprights[2].Y - Constants.GoalHeight * zmultiplierTop * _zoom));
            uprights[7] = new Point(uprights[3].X, (Int32)(uprights[3].Y - Constants.GoalHeight * zmultiplierBottom * _zoom));

            Pen drawPen = new Pen(Color.Red);
            // Uprights
            g.DrawLine(drawPen, uprights[0], uprights[4]);
            g.DrawLine(drawPen, uprights[1], uprights[5]);
            g.DrawLine(drawPen, uprights[2], uprights[6]);
            g.DrawLine(drawPen, uprights[3], uprights[7]);
            // Crossbars
            g.DrawLine(drawPen, uprights[4], uprights[5]);
            g.DrawLine(drawPen, uprights[6], uprights[7]);
            drawPen.Dispose();
        }

        private void DrawNetYAxis(Graphics g, UInt16 Xposition)
        {
            Pen drawPen = new Pen(Constants.NetsColor);

            // Vertical
            for (sbyte i = 0; i <= (Constants.GoalBottom - Constants.GoalTop) / Constants.NetsSpacing; ++i)
            {
                Int32 yval = Constants.GoalTop + i * Constants.NetsSpacing;
                Coords threadStart = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition, yval);
                PointF anchor = new PointF((threadStart.X * _zoom), (threadStart.Y * _zoom));
                //Coords threadEnd = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition, Constants.GoalTop + i * Constants.NetsSpacing);
                double zmultiplier = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(yval);
                float Zoffset = (float)(Constants.GoalHeight * _zoom * zmultiplier);

                g.DrawLine(drawPen, anchor.X, anchor.Y, anchor.X, anchor.Y - Zoffset);
            }

            // Horizontal
            Coords postNear = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition, Constants.GoalBottom);
            Coords postFar = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition, Constants.GoalTop);

            PointF anchorNear = new PointF((postNear.X * _zoom), (postNear.Y * _zoom));
            PointF anchorFar = new PointF((postFar.X * _zoom), (postFar.Y * _zoom));

            double zmultiplierNear = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Constants.GoalBottom);
            double zmultiplierFar = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Constants.GoalTop);

            for (sbyte i = 0; i <= Constants.GoalHeight / Constants.NetsSpacing; ++i)
            {
                float ZoffsetNear = (float)(i * Constants.NetsSpacing * _zoom * zmultiplierNear);
                float ZoffsetFar = (float)(i * Constants.NetsSpacing * _zoom * zmultiplierFar);
                g.DrawLine(drawPen, anchorNear.X, anchorNear.Y - ZoffsetNear, anchorFar.X, anchorFar.Y - ZoffsetFar);
            }

            drawPen.Dispose();
        }

        private void DrawNetXAxis(Graphics g, UInt16 Xposition, UInt16 Yposition)
        {
            Pen drawPen = new Pen(Constants.NetsColor);

            double zmultiplier = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Yposition);
            float Zoffset = (float)(Constants.GoalHeight * _zoom * zmultiplier);

            // Vertical
            for (sbyte i = 0; i <= (Constants.NetsDepth) / Constants.NetsSpacing; ++i)
            {
                //Int32 yval = Constants.GoalTop + i * Constants.NetsSpacing;
                Coords threadStart = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition+i*Constants.NetsSpacing, Yposition);
                PointF anchor = new PointF((threadStart.X * _zoom), (threadStart.Y * _zoom));
                //Coords threadEnd = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition, Constants.GoalTop + i * Constants.NetsSpacing);
                
                

                g.DrawLine(drawPen, anchor.X, anchor.Y, anchor.X, anchor.Y - Zoffset);
            }

            // Horizontal
            Coords leftEnd = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition, Yposition);
            Coords rightEnd = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition+Constants.NetsDepth, Yposition);

            PointF anchorLeft = new PointF((leftEnd.X * _zoom), (leftEnd.Y * _zoom));
            PointF anchorRight = new PointF((rightEnd.X * _zoom), (rightEnd.Y * _zoom));

            //double zmultiplierLeft = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Constants.GoalBottom);
            //double zmultiplierRight = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Constants.GoalTop);

            for (sbyte i = 0; i <= Constants.GoalHeight / Constants.NetsSpacing; ++i)
            {
                float delataZoffset = (float)(i * Constants.NetsSpacing * _zoom * zmultiplier);
                //float ZoffsetFar = (float)(i * Constants.NetsSpacing * _zoom * zmultiplier);
                g.DrawLine(drawPen, anchorLeft.X, anchorLeft.Y - delataZoffset, anchorRight.X, anchorRight.Y - delataZoffset);
            }

            drawPen.Dispose();
        }

        private void DrawNetZAxis(Graphics g, UInt16 Xposition)
        {
            Pen drawPen = new Pen(Constants.NetsColor);

            // y -axis 
            for (sbyte i = 0; i <= (Constants.GoalBottom - Constants.GoalTop) / Constants.NetsSpacing; ++i)
            {
                Int32 yval = Constants.GoalTop + i * Constants.NetsSpacing;
                Coords leftEnd = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition, yval);
                Coords rightEnd = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition + Constants.NetsDepth, yval);
                PointF anchorLeft = new PointF((leftEnd.X * _zoom), (leftEnd.Y * _zoom));
                PointF anchorRight = new PointF((rightEnd.X * _zoom), (rightEnd.Y * _zoom));
                //Coords threadEnd = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition, Constants.GoalTop + i * Constants.NetsSpacing);
                double zmultiplier = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(yval);
                float Zoffset = (float)(Constants.GoalHeight * _zoom * zmultiplier);

                g.DrawLine(drawPen, anchorLeft.X, anchorLeft.Y - Zoffset, anchorRight.X, anchorRight.Y - Zoffset);
            }

            // x-axis

            double zmultiplierNear = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Constants.GoalBottom);
            double zmultiplierFar = 0.5 + 0.5 * StaticMathFunctions.SpacePointToDisplayPointZAxis(Constants.GoalTop);

            float ZoffsetNear = (float)(Constants.GoalHeight * _zoom * zmultiplierNear);
            float ZoffsetFar = (float)(Constants.GoalHeight * _zoom * zmultiplierFar);

            for (sbyte i = 0; i <= Constants.NetsDepth / Constants.NetsSpacing; ++i)
            {
                Coords postNear = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition + i * Constants.NetsSpacing, Constants.GoalBottom);
                Coords postFar = StaticMathFunctions.SpacePointToDisplayPointTransform(Xposition + i * Constants.NetsSpacing, Constants.GoalTop);

                PointF anchorNear = new PointF((postNear.X * _zoom), (postNear.Y * _zoom));
                PointF anchorFar = new PointF((postFar.X * _zoom), (postFar.Y * _zoom));

                g.DrawLine(drawPen, anchorNear.X, anchorNear.Y - ZoffsetNear, anchorFar.X, anchorFar.Y - ZoffsetFar);
            }

            drawPen.Dispose();
        }

        private void DrawLine(Graphics g, Vector2d a, Vector2d b)
        {
            Coords start = StaticMathFunctions.SpacePointToDisplayPointTransform(a.X, a.Y);
            PointF anchorA = new PointF((start.X * _zoom), (start.Y * _zoom));

            Coords end = StaticMathFunctions.SpacePointToDisplayPointTransform(b.X, b.Y);
            PointF anchorB = new PointF((end.X * _zoom), (end.Y * _zoom));

            Pen drawPen = new Pen(Color.Red);

            g.DrawLine(drawPen, anchorA, anchorB);
            drawPen.Dispose();
        }

        /*
        private RectangleF GetRectangle(Coords positionTile)
        {
            return new RectangleF(positionTile.X * Constants.TileBitmapSize * this._zoom, positionTile.Y * Constants.TileBitmapSize * this._zoom,
                (Constants.TileBitmapSize) * this._zoom, (Constants.TileBitmapSize) * this._zoom);
        }
        */
        #region Tile drawers

        // Draw tile bitmap
        private void TileDrawBitmap(Graphics g, Coords position, Bitmap image)
        {
            Point anchor = new Point((Int32)(position.X * _zoom), (Int32)(position.Y * _zoom));
            g.DrawImageUnscaled(image, anchor);
        }

        /*
        // Used for displaying the grid.
        private void TileDrawRectangle(Graphics graphicsObj, Coords position)
        {
            RectangleF box = GetRectangle(position);
            graphicsObj.DrawRectangle(new Pen(Color.Black), box.X, box.Y, box.Width, box.Height);
        }
        */
        /*
        private void TileDrawCoordinates(Graphics g, Coords position)
        {
            StringFormat strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Center;


            g.DrawString(position.ToString(), new Font("Tahoma", Constants.FontSize / 2), Brushes.Black,
                new PointF((position.X + 0.5f) * Constants.TileBitmapSize * this._zoom, (position.Y + 0.5f) *
                    Constants.TileBitmapSize * this._zoom), strFormat);
        }
        */

        #endregion

        #region Pixel drawers

        private void DrawBitmapAtPixel(Graphics g, Coords pixel, Bitmap image)
        {
            Coords screenPos = StaticMathFunctions.SpacePointToDisplayPointTransform(pixel.X, pixel.Y);

            Point anchor = new Point((Int32)(screenPos.X * _zoom - 0.5f * image.Width), (Int32)(screenPos.Y * _zoom - image.Height));

            // This should be at the middle of the bottom of the image.
            //Point anchor = new Point((Int32)(pixel.X * this._zoom - 0.5f * image.Width), (Int32)(pixel.Y * this._zoom - image.Height));
            g.DrawImageUnscaled(image, anchor);
        }

        private void DrawEllipseAtPixel(Graphics g, Coords pixel, UInt16 radiusX, UInt16 radiusY)
        {
            g.DrawEllipse(new Pen(Constants.BoundingCircleColor), (pixel.X - radiusX) * this._zoom, (pixel.Y - radiusY) * this._zoom,
                radiusX * 2 * this._zoom, radiusY * 2 * this._zoom);
        }

        #endregion

        #region LabelDrawers

        // Draws an upper label on a tile (or rather Footballer). Should make it 
        // nicer later with color choice, etc.
        private void DrawLabel(Graphics graphicsObj, Vector2d position, String label)
        {
            Coords transformed = StaticMathFunctions.SpacePointToDisplayPointTransform(position.X, position.Y);

            StringFormat strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Center;

            graphicsObj.DrawString(label, new Font("Tahoma", Constants.FontSize), Brushes.Black,
                new PointF(transformed.X * this._zoom, transformed.Y * this._zoom), strFormat);
        }
        
        #endregion

        public Painter(Map assignedMap, MainFrame frame, float zoom)
        {
            this._zoom = zoom;
            this._currentMap = assignedMap;
            this._mainFrame = frame;
            assignedMap.MyPainter = this;
            this._FootballersToDraw = new List<Footballer>();

            this.ImportSprites();
            this.RescaleSprites();
        }
    }
        
}
