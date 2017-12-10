using System;
using System.Globalization;
using System.Xml;
using Engine.Pathfinding;
using Engine.Utilities;

namespace Engine.Models
{
    public partial class Character
    {
        public void RegisterOnChangedCallback(Action<Character> cb)
        {
            _cbCharacterChanged += cb;
        }

        public void UnregisterOnChangedCallback(Action<Character> cb)
        {
            _cbCharacterChanged -= cb;
        }

        public void ReadXml(XmlNode xml)
        {
            if (xml.Attributes != null && xml.Attributes["name"] != null && xml.Attributes["name"].ToString().Length > 0)
            {
                this.Name = xml.Attributes["name"].Value;
            }

            var condsNode = xml.SelectSingleNode("./Conditions");
            if (condsNode != null)
            {
                var condNodes = condsNode.SelectNodes("./Condition");
                if (condNodes != null)
                {
                    foreach (XmlElement condNode in condNodes)
                    {
                        var name = condNode.Attributes["name"].Value;
                        var value = float.Parse(condNode.InnerText);
                        this.SetCondition(name, value);
                    }
                }
            }

            var inventoryNode = xml.SelectSingleNode("./Inventory");
            if (inventoryNode != null)
            {
                this.Inventory = new Inventory();
                this.Inventory.Character = this;
                this.Inventory.ReadXml(inventoryNode);
            }
        }

        public XmlElement WriteXml(XmlDocument xml)
        {
            var character = xml.CreateElement("Character");
            character.SetAttribute("x", CurrentTile.X.ToString());
            character.SetAttribute("y", CurrentTile.Y.ToString());
            character.SetAttribute("name", this.Name);

            if (_conditions.Count > 0)
            {
                var condsXml = xml.CreateElement("Conditions");
                foreach (var cond in _conditions)
                {
                    var condXml = xml.CreateElement("Condition");
                    condXml.SetAttribute("name", cond.Key);
                    condXml.InnerText = cond.Value.ToString(CultureInfo.InvariantCulture);
                    condsXml.AppendChild(condXml);
                }
                character.AppendChild(condsXml);
            }

            if (this.Inventory != null)
            {
                character.AppendChild(this.Inventory.WriteXml(xml));
            }

            return character;
        }

        private bool RoomIsSafe(Tile tile)
        {
            if (tile == null) return false;
            if (tile.Room == null) return false;
            return RoomIsSafe(tile.Room);
        }

        private bool RoomIsInside(Room room)
        {
            if (room == null) return false;
            return !room.IsOutsideRoom();
        }

        private bool RoomIsSafe(Room room)
        {
            if (room == null) return false;
            if (room.Atmosphere.IsBreathable() == false) return false;
            return true;
        }

        private bool TileIsAirRecharger(Tile tile)
        {
            if (tile == null) return false;
            if (tile.Furniture == null) return false;
            if (Mathf.Approximately(tile.Furniture.GetParameter("air_recharger"), 0)) return false;
            if (tile.Furniture.WorkingCharacter != null) return false;
            return true;
        }

        private Tile FindNearestRoom()
        {
            var rf = new RoomFinder();
            var room = rf.FindClosestRoom(CurrentTile, RoomIsInside);
            return room;
        }

        private Tile FindNearestSafeRoom()
        {
            var rf = new RoomFinder();
            var room = rf.FindClosestRoom(CurrentTile, RoomIsSafe);
            return room;
        }

        private Tile FindNearestReplenisher()
        {
            var rf = new RoomFinder();
            var tile = rf.FindClosestTile(CurrentTile, TileIsAirRecharger);
            return tile;
        }

        public bool CanBreathe()
        {
            if (CurrentTile == null) return false;
            if (CurrentTile.Room == null) return false;
            return CurrentTile.Room.Atmosphere.IsBreathable();
        }

        public float GetCondition(string name)
        {
            return _conditions.ContainsKey(name) ? _conditions[name] : 0f;
        }

        public float SetCondition(string name, float value, bool clamp = true)
        {
            if (clamp)
            {
                value = Mathf.Clamp01(value);
            }

            if (_conditions.ContainsKey(name) == false)
            {
                _conditions.Add(name, value);
            }
            else
            {
                _conditions[name] = value;
            }

            return _conditions[name];
        }

        public float ChangeCondition(string name, float delta, bool clamp = true)
        {
            return this.SetCondition(name, this.GetCondition(name) + delta, clamp);
        }

        public void SetDestination(Tile tile)
        {
            if (CurrentTile.IsNeighbour(tile, true) == false)
            {
                Debug.Log("Character::SetDestination -- Our destination Tile isn't actually our neighbour.");
            }

            DestinationTile = tile;
        }

        private float BreathVolume()
        {
            // hack the deltaTime to speed up the simulation a bit
            var speedModifier = 10;

            // We can assume an at-rest breathing rate of about 15 breaths per minute (https://en.wikipedia.org/wiki/Lung_volumes)
            var breaths = (15f / 60) * speedModifier; // Breaths-per-second (this frame) 

            // We can assume an average "tidal volume" of air moving in and out of a person is 0.5L (https://en.wikipedia.org/wiki/Lung_volumes)
            var consumedO2Volume = 0.5f * breaths * 0.001f; // Cubic Metres

            return consumedO2Volume;
        }
    }
}
