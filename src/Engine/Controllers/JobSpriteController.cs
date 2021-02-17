using System.Collections.Generic;
using Engine.Models;
using Engine.Utilities;

namespace Engine.Controllers
{
    public class JobSpriteController
    {
        /* #################################################################### */
        /* #                           FIELDS                                 # */
        /* #################################################################### */

        private Dictionary<Job, GameObject> _jobGameObjectMap;

        /* #################################################################### */
        /* #                           METHODS                                # */
        /* #################################################################### */

        private void Start()
        {
            _jobGameObjectMap = new Dictionary<Job, GameObject>();
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

            jobGo.Name = "JOB_" + job.JobObjectType + "_" + job.Tile.X + "_" + job.Tile.Y;

            var posOffset = new Vector2<float>((float)(job.FurniturePrototype.Width - 1) / 2, (float)(job.FurniturePrototype.Height - 1) / 2);
            jobGo.Position = new WorldCoord(job.Tile.X + posOffset.X, job.Tile.Y + posOffset.Y);
            jobGo.Sprite = FurnitureSpriteController.Instance.GetSpriteForFurniture(job.JobObjectType);
            jobGo.Sprite.Colour = new Colour(0.5f, 1f, 0.5f, 0.25f);
            jobGo.SortingLayerName = Engine.LAYER.JOBS;

            job.RegisterOnJobCompletedCallback(OnJobEnded);
            job.RegisterOnJobStoppedCallback(OnJobEnded);
        }

        private void OnJobEnded(Job job)
        {
            var jobGo = _jobGameObjectMap[job];
            job.UnregisterOnJobStoppedCallback(OnJobEnded);
            job.UnregisterOnJobCompletedCallback(OnJobEnded);
        }
    }
}
