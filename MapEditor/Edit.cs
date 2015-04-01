using OpenTK;
using OpenTK.Graphics;
using Sim;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MapEditor
{
    public partial class Edit : Form
    {
        private const int TileIconSpacing = 2;
        private const int TileIconColumns = 3;

        private bool _mapLoaded = false;
        private bool _toolsLoaded = false;
        private string _filename;
        private Map _map;
        private readonly GraphicsController _mapGraphics = new GraphicsController();
        private readonly GraphicsController _toolGraphics = new GraphicsController();
        private SpritesheetController _toolSprites;

        private int _selectedSprite = 1;

        public Edit()
        {
            InitializeComponent();
            // Don't touch GLControl in here.
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Don't touch GLControl in here.
            Application.Idle += Application_Idle;
        }

        private void mapDisplay_Load(object sender, EventArgs e)
        {
            _mapLoaded = true;
            _mapGraphics.Load(Color.SkyBlue);
            SetupMapViewport();
            MapDisplay.Invalidate();
        }

        private void ToolDisplay_Load(object sender, EventArgs e)
        {
            _toolsLoaded = true;
            _toolGraphics.Load(Color.SandyBrown);
            SetupToolViewport();
            ToolDisplay.Invalidate();
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            MapDisplay.Invalidate();
            ToolDisplay.Invalidate();
        }

        private void mapDisplay_Paint(object sender, PaintEventArgs e)
        {
            if (!_mapLoaded)
                return;

            MapDisplay.MakeCurrent();

            _mapGraphics.BeginRender();

            if (_map != null)
            {
                _map.Render();
                for (var row = 0; row < MapDisplay.Height; row += _map.Spritesheet.SpriteHeight)
                {
                    for (var col = 0; col < MapDisplay.Width; col += _map.Spritesheet.SpriteWidth)
                    {
                        var pos = new Vector2(col, row);
                        _mapGraphics.SetColour(new Color4(1f, 1f, 1f, 0.5f));
                        _mapGraphics.RenderRectangle(pos,
                            Vector2.Add(pos, new Vector2(_map.Spritesheet.SpriteWidth, _map.Spritesheet.SpriteHeight)));
                        _mapGraphics.ClearColour();
                    }
                }
            }
            MapDisplay.SwapBuffers();
        }

        private void ToolDisplay_Paint(object sender, PaintEventArgs e)
        {
            if (!_toolsLoaded)
                return;

            ToolDisplay.MakeCurrent();

            _toolGraphics.BeginRender();

            if (_toolSprites != null)
            {
                var sprite = 0;
                for (var row = 0; row < _toolSprites.Count / TileIconColumns; row++)
                {
                    for (var col = 0; col < TileIconColumns; col++)
                    {
                        var pos = new Vector2(TileIconSpacing + (_toolSprites.SpriteWidth + TileIconSpacing) * col, TileIconSpacing + (_toolSprites.SpriteHeight + TileIconSpacing) * row);

                        _toolSprites.Render(sprite, pos, _toolGraphics);

                        if (_selectedSprite == sprite)
                        {
                            _toolGraphics.SetColour(Color.DarkRed);
                            _toolGraphics.RenderRectangle(pos, Vector2.Add(pos, new Vector2(_toolSprites.SpriteWidth, _toolSprites.SpriteHeight)));
                            _toolGraphics.ClearColour();
                        }

                        sprite++;
                    }
                }
            }

            ToolDisplay.SwapBuffers();
        }

        private void mapDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_mapLoaded)
                return;
            if (e.KeyCode == Keys.Space)
            {
                MapDisplay.Invalidate();
            }
        }

        private void mapDisplay_Resize(object sender, EventArgs e)
        {
            SetupMapViewport();
        }

        private void ToolDisplay_Resize(object sender, EventArgs e)
        {
            SetupToolViewport();
        }

        private void SetupMapViewport()
        {
            MapDisplay.MakeCurrent();
            _mapGraphics.ResetDisplay(0, 0, MapDisplay.Width, MapDisplay.Height);
        }

        private void SetupToolViewport()
        {
            ToolDisplay.MakeCurrent();
            _toolGraphics.ResetDisplay(0, 0, ToolDisplay.Width, ToolDisplay.Height);
        }

        private void FileOpenButton_Click(object sender, EventArgs e)
        {
            using (
                var dlg = new OpenFileDialog
                {
                    Filter = @"Map Files (*.txt)|*.txt",
                    FilterIndex = 1,
                    Multiselect = false
                })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _filename = dlg.FileName;
                    _map = new Map(_filename, _mapGraphics, true);
                    _toolSprites = new SpritesheetController(_map.Spritesheet.Filename, _toolGraphics);

                    MapDisplay.Invalidate();
                    ToolDisplay.Invalidate();
                }
            }
        }

        private void ToolDisplay_MouseClick(object sender, MouseEventArgs e)
        {
            _selectedSprite = ToolClickToTileId(new Vector2(e.X, e.Y));
        }

        private int ToolClickToTileId(Vector2 pos)
        {
            var column = (int)pos.X / (_toolSprites.SpriteWidth + TileIconSpacing);
            var row = (int)pos.Y / (_toolSprites.SpriteHeight + TileIconSpacing);
            var tile = (row * TileIconColumns) + column;
            if (tile > _toolSprites.Count)
            {
                return -1;
            }
            return tile;
        }

        private void MapDisplay_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Y > _map.MapSize.Y)
            {
                var rowsToAdd = (int)(e.Y - _map.MapSize.Y) / _map.Spritesheet.SpriteHeight;
                _map.AddRows(rowsToAdd + 1, _selectedSprite);
            }
            if (e.X > _map.MapSize.X)
            {
                var columnsToAdd = (int)(e.X - _map.MapSize.X) / _map.Spritesheet.SpriteWidth;
                _map.AddColumns(columnsToAdd + 1, _selectedSprite);
            }
            _map.SetTileSprite(new Vector2(e.X, e.Y), _selectedSprite);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var data = new Sim.DataFormats.MapDatafile(_map);
            data.Save(_filename);
        }
    }
}
