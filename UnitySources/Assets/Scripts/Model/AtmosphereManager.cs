﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Assets.Scripts.Model
{
    /// <summary>
    /// The AtmosphereManager can be used to track and modify the composition of the atmoshpere in a room.
    /// </summary>
    [MoonSharpUserData]
    public class AtmosphereManager
    {
        public const float TileVolume = 3f;

        private Room _room;

        /// <summary>
        /// A list of Gasses identified by their name, and the amount of that gas (in m³) per Tile's volume of atmosphere.
        /// </summary>
        private Dictionary<string, float> _atmosphericGasses;
         
        public AtmosphereManager(Room room)
        {
            this._room = room;
            this._atmosphericGasses = new Dictionary<string, float>();
        }

        public AtmosphereManager Clone()
        {
            var a = new AtmosphereManager(this._room);
            a._atmosphericGasses = new Dictionary<string, float>(this._atmosphericGasses);
            return a;
        }

        /// <summary>
        /// Add the specified volume of the named gas, in m³.
        /// </summary>
        /// <param name="name">The name of the gas to alter.</param>
        /// <param name="delta">The m³ of gas to add/remove to the room.</param>
        public void ChangeGas(string name, float delta)
        {
            // The outside room can't have an atmosphere.
            if (this._room.IsOutsideRoom())
            {
                return;
            }

            // The amount passed in is the total amount of gas (at sealevel-equivalent volume) to 
            // add to the whole room, so scale it to the one-tile value we're storing.
            // For example, if we're adding 20L of air to a room of 10 tiles, only 2L get added to one tile.
            var tileDelta = delta/this._room.Size;

            if (_atmosphericGasses.ContainsKey(name))
            {
                if (float.IsNaN(_atmosphericGasses[name]))
                {
                    _atmosphericGasses[name] = 0f;
                }
                _atmosphericGasses[name] += tileDelta;
            }
            else
            {
                _atmosphericGasses.Add(name, tileDelta);
            }

            // If the delta took our total amount less than zero, just set it to zero.
            if (_atmosphericGasses[name] < 0) _atmosphericGasses[name] = 0;
        }

        /// <summary>
        /// Returns the volume of the specified gas in one tile of this room.
        /// </summary>
        /// <param name="name">The name of the gas.</param>
        /// <returns>The volume of gas.</returns>
        public float GetGasAmount(string name)
        {
            if (_atmosphericGasses.ContainsKey(name))
            {
                return _atmosphericGasses[name];
            }
            return 0f;
        }

        /// <summary>
        /// Returns the concentration of the specified gas in this room.
        /// </summary>
        /// <param name="name">The name of the gas.</param>
        /// <returns>The percentage of the atmoshpere that is made up of the specified gas.</returns>
        public float GetGasPercentage(string name)
        {
            if (_atmosphericGasses.ContainsKey(name) == false)
            {
                return 0f;
            }

            var total = GetTotalAtmosphericPressure();

            if (Mathf.Approximately(total, 0))
            {
                return 0f;
            }

            return _atmosphericGasses[name] / total;
        }

        /// <summary>
        /// Returns a list of all the gas names present in this room.
        /// </summary>
        /// <returns>The list of names.</returns>
        public IEnumerable<string> GetGasNames()
        {
            return _atmosphericGasses.Keys;
        }

        /// <summary>
        /// Returns the total atmoshperic pressure, in atmospheres.
        /// </summary>
        /// <returns>The relative atmospheric pressure.</returns>
        /// <remarks>
        /// 0 means no atmosphere present, so basically a vacuum.
        /// 1 means one atmosphere of pressure, or about 101.325 kPa or 1013.25 mbar) 
        /// >1 means higher than usual pressure.
        /// </remarks>
        public float GetTotalAtmosphericPressure()
        {
            return this.GetTotalAtmosphericVolume() / TileVolume;
        }

        /// <summary>
        /// Returns the total atmoshperic volume, in m3.
        /// </summary>
        /// <returns>The atmospheric volume.</returns>
        public float GetTotalAtmosphericVolume()
        {
            return _atmosphericGasses.Values.Sum(g => g);
        }

        /// <summary>
        /// Merge the atmospheres of multiple rooms together into this one.
        /// </summary>
        /// <param name="other">A list of other rooms to merge.</param>
        public void MergeAtmosphere(IEnumerable<Room> other)
        {
            // Spin through and get a list of all available gasses, and the total amount of it.
            var gasses = new Dictionary<string, float>();
            var totalTiles = 0;
            foreach (var room in other)
            {
                totalTiles += room.Size;
                foreach (var gas in room.Atmosphere._atmosphericGasses)
                {
                    if (gasses.ContainsKey(gas.Key))
                    {
                        gasses[gas.Key] += gas.Value * room.Size;
                    }
                    else
                    {
                        gasses.Add(gas.Key, gas.Value * room.Size);
                    }
                }
            }

            // Now divide the total volume of gas back down by the size of the new room
            foreach (var gas in gasses)
            {
                this.ChangeGas(gas.Key, gas.Value / totalTiles);
            }
        }

        public bool IsBreathable()
        {
            if (this.GetGasPercentage("O2") < 0.2f)
            {
                return false;
            }
            if (this.GetTotalAtmosphericPressure() < 0.3f)
            {
                return false;
            }
            return true;
        }

        public void ReadXml(XmlElement element)
        {
            var atmos = (XmlElement)element.SelectSingleNode("./Atmosphere");
            if (atmos != null)
            {
                var gasses = atmos.SelectNodes("./Gas");
                if (gasses != null)
                {
                    foreach (XmlNode gasElement in gasses)
                    {
                        var gasName = gasElement.Attributes["name"].Value;
                        var gasAmount = float.Parse(gasElement.InnerText);
                        this.ChangeGas(gasName, gasAmount);
                    }
                }
            }
        }

        public void WriteXml(XmlDocument xml, XmlElement room)
        {
            if (_atmosphericGasses.Count > 0)
            {   
                var gassesElement = xml.CreateElement("Atmosphere");
                foreach (var gas in _atmosphericGasses)
                {
                    var gasElement = xml.CreateElement("Gas");
                    gasElement.SetAttribute("name", gas.Key);
                    gasElement.InnerText = gas.Value.ToString(CultureInfo.InvariantCulture);
                    gassesElement.AppendChild(gasElement);
                }
                room.AppendChild(gassesElement);
            }
        }
    }
}
