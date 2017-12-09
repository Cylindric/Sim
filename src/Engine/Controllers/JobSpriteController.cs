using System.Collections.Generic;
using Engine.Models;
// using UnityEngine;

namespace Engine.Controllers
{
    public class JobSpriteController// : MonoBehaviour
    {
        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        private FurnitureSpriteController fsc;
        private Dictionary<Job, GameObject> _jobGameObjectMap;

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        private void Start()
        {
            _jobGameObjectMap = new Dictionary<Job, GameObject>();
            fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
            WorldController.Instance.World.JobQueue.RegisterJobCreationCallback(OnJobCreated);
        }

        private void Update()
        {

        }

        private void OnJobCreated(Job job)
        {
            if (job.JobObjectType == null)
            {
                // This job doesn't have a sprite associated with it.
                return;
            }

            if (_jobGameObjectMap.ContainsKey(job))
            {
                // Debug.LogError("OnJobCreated called for a JobGO that already exists.");
                return;
            }

            var jobGo = new GameObject();

            _jobGameObjectMap.Add(job, jobGo);

            jobGo.name = "JOB_" + job.JobObjectType + "_" + job.Tile.X + "_" + job.Tile.Y;

            var posOffset = new Vector3((float)(job.FurniturePrototype.Width - 1) / 2, (float)(job.FurniturePrototype.Height - 1) / 2, 0);
            jobGo.transform.position = new Vector3(job.Tile.X, job.Tile.Y, 0) + posOffset;
            jobGo.transform.SetParent(this.transform, true);

            var sr = jobGo.AddComponent<SpriteRenderer>();
            sr.sprite = fsc.GetSpriteForFurniture(job.JobObjectType);
            sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
            sr.sortingLayerName = "Jobs";

            job.RegisterOnJobCompletedCallback(OnJobEnded);
            job.RegisterOnJobStoppedCallback(OnJobEnded);
        }

        private void OnJobEnded(Job job)
        {
            var jobGo = _jobGameObjectMap[job];
            job.UnregisterOnJobStoppedCallback(OnJobEnded);
            job.UnregisterOnJobCompletedCallback(OnJobEnded);
            Destroy(jobGo);
        }
    }
}
