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
            const string template = "{0}: {1:P0}  "; // Name, Percentage
            _myText.text = "Unknown Atmosphere";

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

            var text = string.Empty;
            foreach (var name in t.Room.GetGasNames())
            {
                var percentage = t.Room.GetGasPercentage(name);
                var amount = t.Room.GetGasAmount(name);
                text += string.Format(template, name, percentage);
            }

            if (!string.IsNullOrEmpty(text))
            {
                text = string.Format("Air Pressure: {0:#0.0} BAR\n{1}", t.Room.GetTotalAtmosphericPressure(), text);
                _myText.text = text;
            }
        }
    }
}
