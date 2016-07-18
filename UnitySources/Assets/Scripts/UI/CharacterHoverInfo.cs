using System.Collections;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CharacterHoverInfo : MonoBehaviour
    {
        public Character Character { get; set; }

        public bool Enable
        {
            get { return this._box.activeInHierarchy; }
            set
            {
                this._box.SetActive(value);
            }
        }

        private GameObject _box;
        private Text[] _labels;

        void Start()
        {
            _box = GameObject.Find("CharacterInfoBox");
            _labels = _box.GetComponentsInChildren<Text>();
            this._box.SetActive(false);
        }

        void Update()
        {
            if (this.Character == null) return;

            foreach (var label in _labels)
            {
                switch (label.name)
                {
                    case "Character.Name":
                        label.text = this.Character.Name;
                        break;

                    case "Character.Job":
                        if (this.Character.CurrentJob == null)
                        {
                            label.text = "idle";
                        } else {
                            label.text = this.Character.CurrentJob.Description;
                        }
                        break;

                    case "Character.Inventory":
                        if (this.Character.Inventory == null)
                        {
                            label.text = "none";
                        }
                        else
                        {
                            label.text = string.Format("{0} {1}", Character.Inventory.StackSize, Character.Inventory.ObjectType);
                        }
                        break;

                    case "Character.ShieldStatus":
                        if (this.Character.CanBreathe())
                        {
                            label.text = "Disabled";
                            label.color = Color.green;
                        }
                        else
                        {
                            label.text = "Enabled";
                            label.color = Color.yellow;
                        }
                        break;
                }
            }
        }

    }
}
