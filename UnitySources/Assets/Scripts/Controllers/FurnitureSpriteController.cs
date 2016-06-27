using System.Collections.Generic;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class FurnitureSpriteController : MonoBehaviour
    {
        public static WorldController Instance { get; protected set; }

        private readonly Dictionary<Furniture, GameObject> _furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();
        private readonly Dictionary<string, Sprite> _furnitureSprites = new Dictionary<string, Sprite>();

        /// <summary>
        /// This is just a helper property to make it easier to access World.
        /// </summary>
        private static World World { get { return WorldController.Instance.World; } }

        private void Start()
        {
            LoadSprites();
            World.RegisterFurnitureCreatedCb(OnFurnitureCreated);

            // Go through any existing furniture (i.e. from save) call their onCreate.
            foreach (var furn in World._furnitures)
            {
                OnFurnitureCreated(furn);
            }
        }

        private void LoadSprites()
        {
            var sprites = Resources.LoadAll<Sprite>("Furniture/orange_walls");
            foreach (var sprite in sprites)
            {
                _furnitureSprites.Add(sprite.name, sprite);
            }
        }

        public void OnFurnitureCreated(Furniture furn)
        {
            var furnGo = new GameObject();
            _furnitureGameObjectMap.Add(furn, furnGo);

            furnGo.name = furn.ObjectType + "_" + furn.Tile.X + "_" + furn.Tile.Y;
            furnGo.transform.position = new Vector3(furn.Tile.X, furn.Tile.Y, 0);
            furnGo.transform.SetParent(this.transform, true);
        
            var sr = furnGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForFurniture(furn);
            sr.sortingLayerName = "Furniture";

            furn.RegisterOnChangedCallback(OnFurnitureChanged);
        }

        public Sprite GetSpriteForFurniture(Furniture obj)
        {
            var spriteName = obj.ObjectType;

            if (obj.LinksToNeighbour == true)
            {
                spriteName = spriteName + "_";

                // check for neighbours NESW
                var x = obj.Tile.X;
                var y = obj.Tile.Y;

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

            if (_furnitureSprites.ContainsKey(spriteName) == false)
            {
                Debug.LogErrorFormat("Attempt to load missing sprite [{0}] failed!", spriteName);
                return null;
            }

            return _furnitureSprites[spriteName];
        }

        public Sprite GetSpriteForFurniture(string objectType)
        {
            if (_furnitureSprites.ContainsKey(objectType))
            {
                return _furnitureSprites[objectType];
            }

            if (_furnitureSprites.ContainsKey(objectType + "_"))
            {
                return _furnitureSprites[objectType + "_"];
            }

            return null;
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
        }
    }
}