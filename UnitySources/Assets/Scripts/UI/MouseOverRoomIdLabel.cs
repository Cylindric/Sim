using Assets.Scripts.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class MouseOverRoomIdLabel : MonoBehaviour
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
            _myText.text = "Room: none";

            var t = _mouseController.GetTileUnderMouse();
            if (t == null)
            {
                return;
            }

            var r = t.Room;
            if (r == null)
            {
                return;
            }

            _myText.text = "Room: " + t.World._rooms.IndexOf(t.Room);
        }
    }
}
