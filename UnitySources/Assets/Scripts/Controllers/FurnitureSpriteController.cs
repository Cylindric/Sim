﻿using System;
using System.Collections.Generic;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class FurnitureSpriteController : MonoBehaviour
    {
        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        private readonly Dictionary<Furniture, GameObject> _furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();
        //private readonly Dictionary<string, Sprite> _furnitureSprites = new Dictionary<string, Sprite>();

        /* #################################################################### */
        /* #                         PROPERTIES                               # */
        /* #################################################################### */

        //public static WorldController Instance { get; protected set; }

        /// <summary>
        /// This is just a helper property to make it easier to access World.
        /// </summary>
        private static World World { get { return WorldController.Instance.World; } }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        public void OnFurnitureCreated(Furniture furn)
        {
            var furnGo = new GameObject();
            _furnitureGameObjectMap.Add(furn, furnGo);

            furnGo.name = furn.ObjectType + "_" + furn.Tile.X + "_" + furn.Tile.Y;
            furnGo.transform.localScale = new Vector3(1.001f, 1.001f); // little bit of extra size to help prevent gaps between tiles. TODO: must be a cleverer way of doing this ;)

            var posOffset = new Vector3((float)(furn.Width - 1) / 2, (float)(furn.Height - 1) / 2, 0);

            furnGo.transform.position = new Vector3(furn.Tile.X, furn.Tile.Y, 0) + posOffset;
            furnGo.transform.SetParent(this.transform, true);

            var sr = furnGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForFurniture(furn);
            sr.color = furn.Tint;
            sr.sortingLayerName = "Furniture";

            if (furn.ObjectType == "furn_door")
            {
                var t = furn.Tile;
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == "furn_wall_steel")
                {
                }
                else
                {
                    sr.transform.Rotate(Vector3.forward, 90f);
                }
            }

            furn.RegisterOnChangedCallback(OnFurnitureChanged);
            furn.RegisterOnRemovedCallback(OnFurnitureRemoved);
        }

        private void OnFurnitureChanged(Furniture furn)
        {
            if (_furnitureGameObjectMap.ContainsKey(furn) == false)
            {
                Debug.LogError("OnFurnitureChanged failed - Furniture requested that is not in the map!");
                return;
            }

            var furnGo = _furnitureGameObjectMap[furn];
            furnGo.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
            furnGo.GetComponent<SpriteRenderer>().color = furn.Tint;
        }

        private void OnFurnitureRemoved(Furniture furn)
        {
            if (_furnitureGameObjectMap.ContainsKey(furn) == false)
            {
                Debug.LogError("OnFurnitureRemoved failed - Furniture requested that is not in the map!");
                return;
            }

            var furnGo = _furnitureGameObjectMap[furn];
            Destroy(furnGo);
            _furnitureGameObjectMap.Remove(furn);
        }

        public Sprite GetSpriteForFurniture(Furniture obj)
        {
            var spriteName = obj.ObjectType;
            var x = obj.Tile.X;
            var y = obj.Tile.Y;

            if (obj.LinksToNeighbour == true)
            {
                spriteName = spriteName + "_";

                // check for neighbours NESW

                Tile t;

                t = World.GetTileAt(x, y + 1);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
                {
                    spriteName += "N";
                }
                t = World.GetTileAt(x + 1, y);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
                {
                    spriteName += "E";
                }
                t = World.GetTileAt(x, y - 1);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
                {
                    spriteName += "S";
                }
                t = World.GetTileAt(x - 1, y);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
                {
                    spriteName += "W";
                }
            }

            // If it's a door, check openness and update the sprite accordingly.
            if (obj.ObjectType == "furn_door") // TODO: fix this hard-coding of types
            {
                spriteName = "furn_door_";

                if (obj.GetParameter("openness") <= 0.1)
                {
                    spriteName += "0";
                }
                else if (obj.GetParameter("openness") <= 0.2)
                {
                    spriteName += "20";
                }
                else if (obj.GetParameter("openness") <= 0.4)
                {
                    spriteName += "40";
                }
                else if (obj.GetParameter("openness") <= 0.6)
                {
                    spriteName += "60";
                }
                else if (obj.GetParameter("openness") <= 0.8)
                {
                    spriteName += "80";
                }
                else if (obj.GetParameter("openness") <= 1.0)
                {
                    spriteName += "100";
                }
            }

            if (spriteName.EndsWith("_"))
            {
                spriteName = spriteName.Substring(0, spriteName.LastIndexOf("_", StringComparison.Ordinal));
            }

            var sprite = SpriteManager.Instance.GetSprite(spriteName);
            return sprite;
        }

        public Sprite GetSpriteForFurniture(string objectType)
        {
            return SpriteManager.Instance.GetSprite(objectType);
        }

        private void Start()
        {
            World.RegisterFurnitureCreatedCb(OnFurnitureCreated);

            // Go through any existing furniture (i.e. from save) call their onCreate.
            foreach (var furn in World.Furnitures)
            {
                OnFurnitureCreated(furn);
            }
        }
    }
}