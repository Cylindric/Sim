using Engine.Models;
using System;

namespace Engine.Controllers
{
    public class SoundController : IController
    {
        #region Singleton
        private static readonly Lazy<SoundController> _instance = new Lazy<SoundController>(() => new SoundController());

        public static SoundController Instance { get { return _instance.Value; } }

        private SoundController()
        {
        }
        #endregion

        private float _soundCooldown = 0f;
        // private AudioClip _tileChangeAudioClip;

        public void Start()
        {
            WorldController.Instance.World.RegisterFurnitureCreatedCb(OnFurnitureCreated);
            WorldController.Instance.World.RegisterTileChanged(OnTileChanged);
            //_tileChangeAudioClip = Resources.Load<AudioClip>("Sounds/Tile_OnChanged");
        }

        public void Update()
        {
            _soundCooldown -= TimeController.Instance.DeltaTime;
        }

        private void OnTileChanged(Tile tileData)
        {
            if (_soundCooldown > 0) return;
            /// AudioSource.PlayClipAtPoint(_tileChangeAudioClip, Camera.main.transform.position);
            _soundCooldown = 0.5f;
        }

        public void OnFurnitureCreated(Furniture furn)
        {
            if (_soundCooldown > 0) return;
            //var ac = Resources.Load<AudioClip>("Sounds/" + furn.ObjectType + "_OnCreated");
            //if (ac == null)
            //{
            //    ac = Resources.Load<AudioClip>("Sounds/Furniture_OnCreated");
            //}
            //AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
            _soundCooldown = 0.5f;
        }

        public void Render()
        {
            throw new NotImplementedException();
        }
    }
}
