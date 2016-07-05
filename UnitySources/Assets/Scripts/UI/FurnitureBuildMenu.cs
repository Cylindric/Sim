using Assets.Scripts.Controllers;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class FurnitureBuildMenu : MonoBehaviour
    {
        public GameObject BuildButtonPrefab;

        void Start ()
        {
            var bmc = GameObject.FindObjectOfType<BuildModeController>();

            foreach (var s in World.Current._furniturePrototypes.Keys)
            {
                var go = (GameObject)Instantiate(BuildButtonPrefab);
                go.transform.SetParent(this.transform);

                go.name = string.Format("Button - Build {0}", s);
                go.transform.GetComponentInChildren<Text>().text = string.Format("Build {0}", s);
	        
                var objectId = s;
                var b = go.GetComponent<Button>();
                b.onClick.AddListener(delegate { bmc.SetMode_BuildInstalledObject(objectId); });
            }
        }
    }
}
