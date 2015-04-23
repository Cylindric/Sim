using OpenTK;

namespace Sim.Primitives
{
    public class Tile
    {
        public Vector2 LocationPx;
        public Vector2 CentrePx;
        public Vector2 SizePx;
        public int Row;
        public int Column;
        public bool IsWalkable;
        public int SpriteNum;

        public Tile()
        {

        }

        public Tile(int row, int col, int width, int height)
        {
            SizePx = new Vector2(width, height);
            MoveTile(col, row);

            SpriteNum = 0;
            IsWalkable = SpriteNum == 3;
        }

        public void MoveTile(int newX, int newY)
        {
            Row = newY;
            Column = newX;
            LocationPx = new Vector2(Column * SizePx.X, Row * SizePx.Y);
            CentrePx = new Vector2(LocationPx.X + SizePx.X / 2, LocationPx.Y + SizePx.Y / 2);
        }
    }
}
