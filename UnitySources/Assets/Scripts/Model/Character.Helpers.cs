using System;
using System.Globalization;
using System.Xml;
using Assets.Scripts.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Model
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

        private bool RoomIsSafe(Room room)
        {
            if (room == null) return false;
            if (room.Atmosphere.IsBreathable() == false) return false;
            return true;
        }

        private Tile FindNearestSafeRoom()
        {
            var rf = new RoomFinder();
            var room = rf.FindClosestRoom(CurrentTile, RoomIsSafe);
            return room;
        }

        public bool CanBreathe()
        {
            if (CurrentTile == null) return false;
            if (CurrentTile.Room == null) return false;
            return CurrentTile.Room.Atmosphere.IsBreathable();
        }

        /// <summary>
        /// Perform some gas-exchange calculations and apply to the local environment.
        /// </summary>
        /// <remarks>
        /// It's not as simple as some people think...
        /// Some numbers at https://en.wikipedia.org/wiki/Breathing#Composition
        /// Alan Boyd has helpfully put some calculations up at http://biology.stackexchange.com/questions/5642/how-much-gas-is-exchanged-in-one-human-breath
        /// </remarks>
        /// <param name="deltaTime">Frame-time</param>
        public void Breathe(float deltaTime)
        {
            if (CurrentTile == null) return;
            if (CurrentTile.Room == null) return;

            // hack the deltaTime to speed up the simulation a bit
            deltaTime *= 10;

            // We can assume an at-rest breathing rate of about 15 breaths per minute (https://en.wikipedia.org/wiki/Lung_volumes)
            var breaths = (15f / 60) * deltaTime; // Breaths-per-second (this frame) 

            // We can assume an average "tidal volume" of air moving in and out of a person is 0.5L (https://en.wikipedia.org/wiki/Lung_volumes)
            var consumedO2Volume = 0.5f * breaths * 0.001f; // Cubic Metres

            // Consume some oxygen.
            CurrentTile.Room.Atmosphere.ChangeGas("O2", -consumedO2Volume);

            // In each breath in, we take in about 18mg of O2, and release back out 36mg of CO2 and 20mg of H2O, which is 0.8 molecules of CO2 for every molecule of O2.
            // I'm not sure how to convert that into a sensible "CO2 produced" number, so this is MADE UP. TODO: Don't make this up.
            CurrentTile.Room.Atmosphere.ChangeGas("CO2", consumedO2Volume);
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

            DestTile = tile;
        }
    }
}
