function MiningDroneStation_UpdateAction( furniture, deltaTime )
	
	spawnSpot = furniture.GetSpawnSpotTile()

	if( furniture.JobCount() > 0 ) then

		-- Check to see if the Metal Plate destination tile is full.
		if( spawnSpot.Inventory ~= nil and spawnSpot.Inventory.StackSize >= spawnSpot.Inventory.MaxStackSize ) then
			-- We should stop this job, because it's impossible to make any more items.
			furniture.CancelJobs()
		end

		return
	end

	-- If we get here, then we have no current job. Check to see if our destination is full.
	if( spawnSpot.Inventory ~= nil and spawnSpot.Inventory.StackSize >= spawnSpot.Inventory.MaxStackSize ) then
		-- We are full! Don't make a job!
		return
	end

	-- If we get here, we need to CREATE a new job.

	jobSpot = furniture.GetJobSpotTile()

	j = Job.__new(
		jobSpot,
		nil,
		nil,
		1,
		nil,
		true	-- This job repeats until the destination tile is full.
	)
	j.RegisterOnJobCompletedCallback("MiningDroneStation_JobComplete")

	furniture.AddJob( j )
end


function MiningDroneStation_JobComplete(j)
  --return "test"
  req = Inventory.__new("steel_plate", 50, 20)
  World.Current.InventoryManager.PlaceInventory(j.Furniture.GetSpawnSpotTile(), req)
end
