using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sim.DataFormats
{
    public class MapDatafile
    {
        public string Spritesheet;
        public int Width;
        public int Height;
        public List<Map.Tile> Tiles = new List<Map.Tile>();
        readonly Dictionary<char, int> _aliases = new Dictionary<char, int>();

        public MapDatafile()
        {
        }

        public MapDatafile(Map map)
        {
            Spritesheet = map.Spritesheet.Filename;
            Width = map.Columns;
            Height = map.Rows;
            foreach (var t in map.Tiles)
            {
                Tiles.Add(t);
            }
        }

        public void Save(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (var stream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write, FileShare.Write))
            {
                using (var file = new StreamWriter(stream, Encoding.UTF8))
                {
                    file.WriteLine("Spritesheet={0}", Spritesheet);
                    file.WriteLine("Height={0}", Height);
                    file.WriteLine("Width={0}", Width);
                    file.WriteLine("Tiles");
                    for (var row = 0; row < Height; row++)
                    {
                        for (var col = 0; col < Width; col++)
                        {
                            file.Write("{0:D2} ", Tiles[row*Width + col].SpriteNum);
                        }
                        file.WriteLine();
                    }
                }
            }
        }

        public void LoadFromFile(string filename, bool fullFilename = false)
        {
            if (!fullFilename)
            {
                filename = ResourceController.GetDataFilename("map.{0}.txt", filename);
            }

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var file = new StreamReader(stream, Encoding.UTF8, true, 128))
                {
                    var line = "";
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.StartsWith("Spritesheet="))
                        {
                            Spritesheet = line.Substring(line.IndexOf("=") + 1);
                            continue;
                        }
                        if (line.StartsWith("Width="))
                        {
                            int.TryParse(line.Substring(line.IndexOf("=") + 1), out Width);
                            continue;
                        }
                        if (line.StartsWith("Height="))
                        {
                            int.TryParse(line.Substring(line.IndexOf("=") + 1), out Height);
                            continue;
                        }
                        if (line.StartsWith("Aliases="))
                        {
                            foreach(var c in line.Substring(line.IndexOf("=") + 1).ToArray())
                            {
                                _aliases.Add(c, _aliases.Count());
                            }
                            continue;
                        }
                        if (line.Equals("Tiles"))
                        {
                            // Read all basic tile data
                            bool keepScanning = true;
                            while (keepScanning && ((line = file.ReadLine()) != null))
                            {
                                // Empty line is the end of the block
                                if(string.IsNullOrEmpty(line))
                                {
                                    keepScanning = false;
                                    break;
                                }

                                // If we're using Aliases, we won't have spaces
                                var id = 0;

                                if (_aliases.Count == 0)
                                {
                                    foreach (var value in line.Trim().Split(' '))
                                    {
                                        if (int.TryParse(value.Trim(), out id) == false)
                                        {
                                            // doesn't look like a numerical value, so we must be outside of the Tile data block
                                            keepScanning = false;
                                            break;
                                        }
                                        else
                                        {
                                            var t = new Map.Tile
                                            {
                                                SpriteNum = id,
                                                Column = Tiles.Count%Width,
                                                Row = Tiles.Count/Width
                                            };
                                            Tiles.Add(t);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var value in line.Trim())
                                    {
                                        if (IdFromChar(value, out id) == false)
                                        {
                                            // doesn't look like a numerical value, so we must be outside of the Tile data block
                                            keepScanning = false;
                                            break;
                                        }
                                        else
                                        {
                                            var t = new Map.Tile
                                            {
                                                SpriteNum = id,
                                                Column = Tiles.Count%Width,
                                                Row = Tiles.Count/Width
                                            };
                                            Tiles.Add(t);
                                        }
                                    }
                                }
                                //TileIds.Add(id);
                            }
                        }
                    }
                }
            }

            if(!DataIsValid())
            {
                throw new InvalidOperationException("The data for the map is incomplete.");
            }
        }

        private bool IdFromChar(char c, out int id)
        {
            if(_aliases.ContainsKey(c))
            {
                id = _aliases[c];
                return true;
            } else
            {
                return(int.TryParse(c.ToString(), out id));
            }
        }

        private bool DataIsValid()
        {
            if (string.IsNullOrEmpty(Spritesheet)) return false;
            if (Width == 0) return false;
            if (Height == 0) return false;
            if (Tiles.Count != (Width * Height)) return false;

            return true;
        }

    }
}
