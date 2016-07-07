function OnUpdate_MiningStation(furniture, deltaTime)
        --{
        --    var spawnSpot = furn.GetSpawnSpotTile();

        --    if (furn.GetJobCount() > 0)
        --    {
        --        -- If the destination Tile is full of Iron, stop the job.
        --        if (spawnSpot.inventory != null && (spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize))
        --        {
        --            -- the job spot is full, so cancel
        --            furn.CancelJobs();
        --        }

        --        -- There's already a Job, so nothing to do.
        --        return;
        --    }

        --    -- If we get here, then we have no current Job. Check to see if our destination is full
        --    if (spawnSpot.inventory != null && (spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize))
        --    {
        --        -- the job spot is full!
        --        return;
        --    }

        --    var jobSpot = furn.GetJobSpotTile();

        --    var j = new Job(
        --        name: "MiningConsole_Work",
        --        tile: jobSpot, 
        --        jobObjectType: null,
        --        cb: MiningConsole_JobComplete,
        --        jobTime: 0.3f,
        --        requirements: null,
        --        repeats: true);
        --    --j.RegisterOnJobStoppedCallback(MiningConsole_JobStopped);

        --    furn.AddJob(j);
        --}

end
