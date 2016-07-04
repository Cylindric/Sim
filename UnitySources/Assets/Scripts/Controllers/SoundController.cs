using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class SoundController : MonoBehaviour
    {
        private float _soundCooldown = 0f;
        private AudioClip _tileChangeAudioClip;

        private void Start()
        {
            WorldController.Instance.World.RegisterFurnitureCreatedCb(OnFurnitureCreated);
            WorldController.Instance.World.RegisterTileChanged(OnTileChanged);
            _tileChangeAudioClip = Resources.Load<AudioClip>("Sounds/Tile_OnChanged");
        }

        private void Update()
        {
            _soundCooldown -= Time.deltaTime;
        }

        private void OnTileChanged(Tile tileData)
        {
            if (_soundCooldown > 0) return;
            AudioSource.PlayClipAtPoint(_tileChangeAudioClip, Camera.main.transform.position);
            _soundCooldown = 0.5f;
        }

        public void OnFurnitureCreated(Furniture furn)
        {
            if (_soundCooldown > 0) return;
            var ac = Resources.Load<AudioClip>("Sounds/" + furn.ObjectType + "_OnCreated");
            if (ac == null)
            {
                ac = Resources.Load<AudioClip>("Sounds/Furniture_OnCreated");
            }
            AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
            _soundCooldown = 0.5f;
        }
    }
}
