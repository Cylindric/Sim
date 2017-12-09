using Engine.Models;
// using Engine.UI;
// using UnityEngine;

namespace Engine.Controllers
{
    public class CharacterCollider// : MonoBehaviour
    {
        public Character Character;
        private CharacterHoverInfo _info;

        void Start ()
        {
            _info = GameObject.FindObjectOfType<CharacterHoverInfo>();
        }
	
        void Update () {
	
        }

        void OnMouseEnter()
        {
            _info.Character = this.Character;
            _info.Enable = true;
        }
    }
}
