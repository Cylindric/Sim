using Assets.Scripts.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    ///  0 -  6 results in death.
    ///  6 - 10 nausea and possible lack of conciousness.
    /// 10 - 14 extreme exhaustion.
    /// 14 - 16 notable physical effects.
    /// 16 - 17 mental impairment.
    /// 
    /// Ideal O2 concentration is 20.9%.
    /// Optiomal is between 19.5% and 23.5%.
    /// 
    /// Very high levels can be bad too.
    /// </summary>
    class MouseOverO2Label : MonoBehaviour
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
            const string template = "O<size=6>2</size>: {0:P0}";
            _myText.text = "O<size=6>2</size>: 0";

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

            if (t.Room.GetGasPercentage("O2") < 0.06f)
            {
                _myText.color = Color.red;
            }
            else if (t.Room.GetGasPercentage("O2") < 0.1f)
            {
                _myText.color = Color.red;
            }
            else
            {
                _myText.color = Color.green;
            }

            _myText.text = string.Format(template, t.Room.GetGasAmount("O2"));
        }
    }
}
