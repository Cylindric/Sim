using Assets.Scripts.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class MouseOverTileTypeLabel : MonoBehaviour
    {
        private Text _myText;
        private MouseController _mouseController;

        private void Start()
        {
            _myText = GetComponent<Text>();
            _mouseController = GameObject.FindObjectOfType<MouseController>();
        }

        private void Update()
        {
            _myText.text = "Tile Type: none";

            var t = _mouseController.GetTileUnderMouse();
            if (t == null)
            {
                return;
            }

            _myText.text = "Tile Type: " + t.Type;
        }
        
    }
}
