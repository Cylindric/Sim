using Engine.Utilities;
using System.Diagnostics;
using static Engine.Engine;

namespace Engine.Models
{
    [DebuggerDisplay("{Name}")]
    public class GameObject
    {
        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */
        public Sprite Sprite { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// The position of this GameObject in World coordinates (Tile XY)
        /// </summary>
        public WorldCoord Position { get; set; }
        public SpriteSheet SpriteSheet { get; set; }
        public Sprite ActiveSprite { get; set; }
        public float Rotation { get; set; }
        public bool IsActive { get; set; }
        public SimplePool.Pool Pool { get; set; }
        public LAYER SortingLayerName { get; internal set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */

        /// <summary>
        /// Creates a copy of the supplied GameObject prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static GameObject Instantiate(GameObject prefab)
        {
            return new GameObject()
            {
                Sprite = prefab.Sprite,
                Name = prefab.Name,
                Position = prefab.Position,
                SpriteSheet = prefab.SpriteSheet,
                ActiveSprite = prefab.ActiveSprite,
                Rotation = prefab.Rotation,
                IsActive = prefab.IsActive
            };
        }

        /// <summary>
        /// Creates a copy of the supplied GameObject prefab.
        /// </summary>
        /// <param name="prefab">The GameObject to spawn.</param>
        /// <param name="worldPos">The world-space coordinates for this object.</param>
        /// <returns></returns>
        public static GameObject Instantiate(GameObject prefab, WorldCoord worldPos)
        {
            var go = Instantiate(prefab);
            go.Position = worldPos;
            return go;
        }

        /// <summary>
        /// Creates a copy of the supplied GameObject prefab.
        /// </summary>
        /// <param name="prefab">The GameObject to spawn.</param>
        /// <param name="worldPos">The world-space coordinates for this object.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        /// <returns></returns>
        public static GameObject Instantiate(GameObject prefab, WorldCoord worldPos, float rotation)
        {
            var go = Instantiate(prefab, worldPos);
            go.Rotation = rotation;
            return go;
        }

    }
}