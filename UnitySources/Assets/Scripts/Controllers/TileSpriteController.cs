﻿using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class TileSpriteController : MonoBehaviour
    {
        public static WorldController Instance { get; protected set; }

        public Sprite FloorSprite;
        public Sprite EmptySprite;

        private readonly Dictionary<Tile, GameObject> _tileGameObjectMap = new Dictionary<Tile, GameObject>();

        private World _world { get { return WorldController.Instance.World; } }

        private void Start()
        {
            // Create a game object for every Tile.
            for (var x = 0; x < _world.Width; x++)
            {
                for (var y = 0; y < _world.Height; y++)
                {
                    var tileData = _world.GetTileAt(x, y);
                    var tileGo = new GameObject();
                    _tileGameObjectMap.Add(tileData, tileGo);

                    tileGo.name = "Tile_" + x + "_" + y;
                    tileGo.transform.localScale = new Vector3(1.001f, 1.001f); // little bit of extra size to help prevent gaps between tiles. TODO: must be a cleverer way of doing this ;)
                    tileGo.transform.position = new Vector3(tileData.X, tileData.Y, 0);
                    tileGo.transform.SetParent(this.transform, true);

                    var sr = tileGo.AddComponent<SpriteRenderer>();
                    sr.sprite = EmptySprite;
                    sr.sortingLayerName = "Tiles";

                    OnTileChanged(tileData);
                }
            }

            _world.RegisterTileChanged(OnTileChanged);
        }

        private void DestroyAllTileGameObjects()
        {
            while (_tileGameObjectMap.Count > 0)
            {
                var tileData = _tileGameObjectMap.Keys.First();
                var tileGo = _tileGameObjectMap[tileData];
                _tileGameObjectMap.Remove(tileData);
                tileData.UnRegisterTileTypeChangedCallback(OnTileChanged);
                Destroy(tileGo);
            }
        }

        private void OnTileChanged(Tile tileData)
        {
            if (_tileGameObjectMap.ContainsKey(tileData) == false)
            {
                Debug.LogError("TileGameObjectMap doesn't contain the tile_data.");
                return;
            }

            var tileGo = _tileGameObjectMap[tileData];

            if (tileGo == null)
            {
                Debug.LogError("TileGameObjectMap returned a null GameObject.");
                return;
            }

            if (tileData.Type == TileType.Floor)
            {
                tileGo.GetComponent<SpriteRenderer>().sprite = FloorSprite;
            }
            else if (tileData.Type == TileType.Empty)
            {
                tileGo.GetComponent<SpriteRenderer>().sprite = EmptySprite;
            }
            else
            {
                Debug.LogError("OnTileChanged - Unrecognised Tile type");
            }
        }

    }
}