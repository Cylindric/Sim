using System.Collections.Generic;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class JobSpriteController : MonoBehaviour
    {

        private FurnitureSpriteController fsc;
        private Dictionary<Job, GameObject> _jobGameObjectMap; 

        void Start ()
        {
            _jobGameObjectMap = new Dictionary<Job, GameObject>();
            fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
            WorldController.Instance.World.JobQueue.RegisterJobCreationCallback(OnJobCreated);
        }
	
        void Update () {
	
        }

        private void OnJobCreated(Job job)
        {
            if (_jobGameObjectMap.ContainsKey(job))
            {
                //Debug.LogError("OnJobCreated called for a JobGO that already exists.");
                return;
            }

           var jobGo = new GameObject();

            _jobGameObjectMap.Add(job, jobGo);

            jobGo.name = "JOB_" + job.JobObjectType + "_" + job.Tile.X + "_" + job.Tile.Y;
            jobGo.transform.position = new Vector3(job.Tile.X, job.Tile.Y, 0);
            jobGo.transform.SetParent(this.transform, true);

            var sr = jobGo.AddComponent<SpriteRenderer>();
            sr.sprite = fsc.GetSpriteForFurniture(job.JobObjectType);
            sr.color = new Color(1f, 1f, 1f, 0.5f);
            sr.sortingLayerName = "Jobs";

            job.RegisterOnCompleteCallback(OnJobEnded);
            job.RegisterOnCancelCallback(OnJobEnded);
        }

        private void OnJobEnded(Job job)
        {
            var jobGo = _jobGameObjectMap[job];
            job.UnregisterOnCancelCallback(OnJobEnded);
            job.UnregisterOnCompleteCallback(OnJobEnded);
            Destroy(jobGo);
        }
    }
}
