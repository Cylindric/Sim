using System.Collections.Generic;
using Assets.Model;
using UnityEngine;

namespace Assets.Controllers
{
    public class FurnitureSpriteController : MonoBehaviour
    {
        public static WorldController Instance { get; protected set; }

        private readonly Dictionary<Furniture, GameObject> _furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();
        private readonly Dictionary<string, Sprite> _furnitureSprites = new Dictionary<string, Sprite>();

        private static World _world { get { return WorldController.Instance.World; } }

        private void Start()
        {
            // Cache some sprite stuff.
            var sprites = Resources.LoadAll<Sprite>("Furniture/Stone Walls");
            foreach (var sprite in sprites)
            {
                _furnitureSprites.Add(sprite.name, sprite);
            }

            _world.RegisterFurnitureCreatedCb(OnFurnitureCreated);
        }

        public void OnFurnitureCreated(Furniture furn)
        {
            var furnGo = new GameObject();
            _furnitureGameObjectMap.Add(furn, furnGo);

            furnGo.name = furn.ObjectType + "_" + furn.Tile.X + "_" + furn.Tile.Y;
            furnGo.transform.position = new Vector3(furn.Tile.X, furn.Tile.Y, 0);
            furnGo.transform.SetParent(this.transform, true);
        
            furnGo.AddComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

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

                t = _world.GetTileAt(x, y + 1);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
                {
                    spriteName += "N";
                }
                t = _world.GetTileAt(x + 1, y);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
                {
                    spriteName += "E";
                }
                t = _world.GetTileAt(x, y - 1);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
                {
                    spriteName += "S";
                }
                t = _world.GetTileAt(x - 1, y);
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