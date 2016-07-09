using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class RoomInfoLabel : MonoBehaviour {

        private Text _myText;

        private void Start()
        {
            _myText = GetComponent<Text>();
        }

        void Update () {
            _myText.text = "";

            foreach (var room in World.Instance.Rooms)
            {
                _myText.text += string.Format("{0} {1} {2}\n", room.Id, room.Size, room.IsOutsideRoom() ? "Outside" : "Inside");
            }
        }
    }
}
