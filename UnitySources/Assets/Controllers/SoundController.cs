using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Controllers
{
    class SoundController : MonoBehaviour
    {
        private float _soundCooldown = 0f;

        void Start()
        {
            WorldController.Instance.World.RegisterFurnitureCreatedCb(OnFurnitureCreated);
            WorldController.Instance.World.RegisterTileChanged(OnTileChanged);
        }

        void Update()
        {
            _soundCooldown -= Time.deltaTime;
        }

        void OnTileChanged(Tile tile_data)
        {
            if (_soundCooldown > 0) return;
            AudioClip ac = Resources.Load<AudioClip>("Sounds/Tile_OnChanged");
            AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
            _soundCooldown = 0.5f;
        }

        public void OnFurnitureCreated(Furniture furn)
        {
            if (_soundCooldown > 0) return;
            AudioClip ac = Resources.Load<AudioClip>("Sounds/" + furn.ObjectType + "_OnCreated");
            if (ac == null)
            {
                ac = Resources.Load<AudioClip>("Sounds/Furniture_OnCreated");
            }
            AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
            _soundCooldown = 0.5f;
        }
    }
}
