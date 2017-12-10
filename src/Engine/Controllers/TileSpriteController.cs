using System.Collections.Generic;
using System.Linq;
using Engine.Models;
using Engine.Utilities;
using System;

namespace Engine.Controllers
{
    public class TileSpriteController : IController
    {
        #region Singleton
        private static readonly Lazy<TileSpriteController> _instance = new Lazy<TileSpriteController>(() => new TileSpriteController());

        public static TileSpriteController Instance { get { return _instance.Value; } }

        private TileSpriteController()
        {
        }
        #endregion

        private readonly Dictionary<Tile, GameObject> _tileGameObjectMap = new Dictionary<Tile, GameObject>();

        private World _world { get { return WorldController.Instance.World; } }

        public void Start()
        {
            // Create a game object for every Tile.
            for (var x = 0; x < _world.Width; x++)
            {
                for (var y = 0; y < _world.Height; y++)
                {
                    var tileData = _world.GetTileAt(x, y);
                    var tileGo = new GameObject();
                    _tileGameObjectMap.Add(tileData, tileGo);

                    tileGo.Name = "Tile_" + x + "_" + y;
                    tileGo.Position = new WorldCoord(tileData.X, tileData.Y);
                    //tileGo.transform.SetParent(this.transform, true);

                    tileGo.Sprite = null;
                    tileGo.IsActive = false;
                    tileGo.SortingLayerName = "Tiles";

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
                var x = tileData.X % 3;
                var y = x + tileData.Y % 3;
                var tile = (x + y) % 3;
                tileGo.Sprite = SpriteManager.Instance.GetSprite("floor", string.Format("floor_{0}", tile));
                tileGo.IsActive = true;
            }
            else if (tileData.Type == TileType.Empty)
            {
                tileGo.Sprite = null;
                tileGo.IsActive = false;
            }
            else
            {
                Debug.LogError("OnTileChanged - Unrecognised Tile type");
            }
        }

        public void Update() { }

        public void Render()
        {
            foreach (var t in _tileGameObjectMap)
            {
                var go = t.Value;

                if (go.Sprite == null)
                {
                    continue;
                }

                var posX = go.Position.X * Engine.GRID_SIZE;
                var posY = go.Position.Y * Engine.GRID_SIZE;

                // offset render location by the camera movement
                posX -= CameraController.Instance.Position.X;
                posY += CameraController.Instance.Position.Y;

                go.Sprite.Render((int)posX, (int)posY);
            }
        }
    }
}