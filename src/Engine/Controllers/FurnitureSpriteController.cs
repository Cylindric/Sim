using System;
using System.Collections.Generic;
using Engine.Models;
using Engine.Utilities;

namespace Engine.Controllers
{
    public class FurnitureSpriteController
    {
        #region Singleton
        private static readonly Lazy<FurnitureSpriteController> _instance = new Lazy<FurnitureSpriteController>(() => new FurnitureSpriteController());

        public static FurnitureSpriteController Instance { get { return _instance.Value; } }

        private FurnitureSpriteController()
        {
        }
        #endregion

        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        private readonly Dictionary<Furniture, GameObject> _furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();
        //private readonly Dictionary<Furniture, ParticleSystem> _furnitureParticleSystemMap = new Dictionary<Furniture, ParticleSystem>();

        /* #################################################################### */
        /* #                         PROPERTIES                               # */
        /* #################################################################### */

        /// <summary>
        /// This is just a helper property to make it easier to access World.
        /// </summary>
        private static World World { get { return WorldController.Instance.World; } }

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */
        //public ParticleSystem GasParticles;

        public void OnFurnitureCreated(Furniture furn)
        {
            var furnGo = new GameObject();
            _furnitureGameObjectMap.Add(furn, furnGo);

            furnGo.Name = furn.ObjectType + "_" + furn.Tile.X + "_" + furn.Tile.Y;

            var posOffset = new Vector2<float>((float)(furn.Width - 1) / 2, (float)(furn.Height - 1) / 2);

            furnGo.Position = new WorldCoord(furn.Tile.X, furn.Tile.Y) + posOffset;
            // furnGo.transform.SetParent(this.transform, true);

            furnGo.Sprite = GetSpriteForFurniture(furn);
            furnGo.Sprite.Colour = furn.Tint;
            furnGo.SortingLayerName = Engine.LAYER.FURNITURE;

            if (furn.GetParameter("openness", -1f) >= 0f)
            {
                var t = furn.Tile.NorthNeighbour();
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == "furn_wall_steel")
                {
                    furnGo.Sprite.Rotate(90f);
                }
            }

            //if(Mathf.Approximately(furn.GetParameter("gas_generator"), 1f))
            //{
            //    _furnitureParticleSystemMap[furn] = Instantiate(GasParticles);
            //    _furnitureParticleSystemMap[furn].transform.SetParent(furnGo.transform, false);
            //    var em = _furnitureParticleSystemMap[furn].emission;
            //    em.enabled = false;
            //}

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
            furnGo.Sprite = GetSpriteForFurniture(furn);
            furnGo.Sprite.Colour = furn.Tint;
        }

        private void OnFurnitureRemoved(Furniture furn)
        {
            if (_furnitureGameObjectMap.ContainsKey(furn) == false)
            {
                Debug.LogError("OnFurnitureRemoved failed - Furniture requested that is not in the map!");
                return;
            }

            var furnGo = _furnitureGameObjectMap[furn];
            _furnitureGameObjectMap.Remove(furn);
        }

        public Sprite GetSpriteForFurniture(Furniture furn)
        {
            var spriteName = "";
            var x = furn.Tile.X;
            var y = furn.Tile.Y;

            if (furn.LinksToNeighbour == true)
            {
                // spriteName = spriteName + "_";

                // check for neighbours NESW

                Tile t;

                t = World.GetTileAt(x, y + 1);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
                {
                    spriteName += "n";
                }
                t = World.GetTileAt(x + 1, y);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
                {
                    spriteName += "e";
                }
                t = World.GetTileAt(x, y - 1);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
                {
                    spriteName += "s";
                }
                t = World.GetTileAt(x - 1, y);
                if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
                {
                    spriteName += "w";
                }
            }

            // If it's a door, check openness and update the sprite accordingly.
            if (furn.GetParameter("openness", -1) >= 0)
            {
                spriteName += "openness_";

                if (furn.GetParameter("openness") <= 0.1)
                {
                    spriteName += "0";
                }
                else if (furn.GetParameter("openness") <= 0.2)
                {
                    spriteName += "20";
                }
                else if (furn.GetParameter("openness") <= 0.4)
                {
                    spriteName += "40";
                }
                else if (furn.GetParameter("openness") <= 0.6)
                {
                    spriteName += "60";
                }
                else if (furn.GetParameter("openness") <= 0.8)
                {
                    spriteName += "80";
                }
                else if (furn.GetParameter("openness") <= 1.0)
                {
                    spriteName += "100";
                }
            }

            if (spriteName.EndsWith("_"))
            {
                spriteName = spriteName.Substring(0, spriteName.LastIndexOf("_", StringComparison.Ordinal));
            }

            if (Mathf.Approximately(furn.GetParameter("condition"), 0))
            {
                spriteName = "damaged";
            }

            // If we don't have a sprite name yet, we're idle.
            if (string.IsNullOrEmpty(spriteName))
            {
                if (furn.IdleSprites == 0)
                {
                    spriteName = "default";
                }
                else
                {
                    spriteName = "idle_" + furn.CurrentIdleFrame;
                }
            }

            var sprite = SpriteManager.Instance.GetSprite(furn.ObjectType, spriteName);
            if (sprite == null)
            {
                sprite = GetSpriteForFurniture(furn.ObjectType);
            }

            if (Mathf.Approximately(furn.GetParameter("gas_generator"), 1f))
            {
                //if (_furnitureParticleSystemMap.ContainsKey(furn))
                //{
                //    var part = _furnitureParticleSystemMap[furn].emission;
                //    part.enabled = furn.GasParticlesEnabled;
                //}
            }

            return sprite;
        }

        public Sprite GetSpriteForFurniture(string objectType)
        {
            
            return SpriteManager.Instance.GetSprite(objectType, "default");
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