using Assets.Scripts.Model;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    public class CharacterCollider : MonoBehaviour
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

        void OnMouseExit()
        {
            _info.Character = null;
            _info.Enable = false;
        }
    }
}
