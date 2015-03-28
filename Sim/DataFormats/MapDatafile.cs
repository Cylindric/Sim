using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sim.DataFormats
{
    class MapDatafile
    {
        public string Spritesheet;
        public int Width;
        public int Height;
        public List<int> TileIds = new List<int>();
        public Dictionary<char, int> Aliases = new Dictionary<char, int>();

        public MapDatafile()
        {

        }

        public void LoadFromFile(string filename)
        {
            filename = ResourceController.GetDataFilename("map.{0}.txt", filename);

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
                                Aliases.Add(c, Aliases.Count());
                            }
                            continue;
                        }
                        if (line.Equals("Tiles"))
                        {
                            // Read all basic tile data
                            var id = 0;
                            bool keepScanning = true;
                            while (keepScanning && ((line = file.ReadLine()) != null))
                            {
                                // Empty line is the end of the block
                                if(string.IsNullOrEmpty(line))
                                {
                                    keepScanning = false;
                                    break;
                                }

                                // If we're using aliases, we won't have spaces
                                if (Aliases.Count == 0)
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
                                            TileIds.Add(id);
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
                                            TileIds.Add(id);
                                        }
                                    }
                                }
                            }
                            continue;
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
            if(Aliases.ContainsKey(c))
            {
                id = Aliases[c];
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
            if (TileIds.Count != (Width * Height)) return false;

            return true;
        }

    }
}
