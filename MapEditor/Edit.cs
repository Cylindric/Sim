using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sim;

namespace MapEditor
{
    public partial class Edit : Form
    {
        private bool _mapLoaded = false;
        private bool _toolsLoaded = false;
        private Map _map;
        private readonly GraphicsController _mapGraphics = new GraphicsController();
        private readonly GraphicsController _toolGraphics = new GraphicsController();
        private SpritesheetController _toolSprites;

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
                for (var row = 0; row < _toolSprites.Count / 3; row++)
                {
                    for (var col = 0; col < 3; col++)
                    {
                        _toolSprites.Render(sprite++,
                            new Vector2((_toolSprites.SpriteWidth + 2)*col, (_toolSprites.SpriteHeight + 2)*row),
                            _toolGraphics);

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
                    _map = new Map(dlg.FileName, _mapGraphics, true);
                    _toolSprites = new SpritesheetController(_map.Spritesheet.Filename, _toolGraphics);

                    MapDisplay.Invalidate();
                    ToolDisplay.Invalidate();
                }
            }
        }

    }
}
