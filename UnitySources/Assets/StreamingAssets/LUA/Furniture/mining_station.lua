function OnUpdate_MiningStation(furniture, deltaTime)
  
  spawnSpot = furniture.GetSpawnSpotTile()

  if (furniture.GetJobCount() > 0) then
    -- If the destination Tile is full of Iron, stop the job.
    if (spawnSpot.Inventory != nil and (spawnSpot.Inventory.StackSize >= spawnSpot.Inventory.MaxStackSize)) then
      -- the job spot is full, so cancel
      furniture.CancelJobs()
    end

    -- There's already a Job, so nothing to do.
    return
  end

  -- If we get here, then we have no current Job. Check to see if our destination is full
  if (spawnSpot.Inventory != nil and (spawnSpot.Inventory.StackSize >= spawnSpot.Inventory.MaxStackSize)) then
    -- the job spot is full!
    return
  end

  jobSpot = furniture.GetJobSpotTile()

  j = Job.__new()
  
--  j = Job.__new(
--        "MiningConsole_Work", -- name
--        jobSpot,  -- tile
--        nil, -- jobObjectType
--        nil, -- cb
--        1, -- jobTime
--        nil, -- requirements
--        true -- repeats
--  )

  return jobSpot -- "[" .. jobSpot.x .. "," .. jobSpot.y .. "]"
end

function JobComplete_MiningStation(j)
  --return "test"
  World.Current.InventoryManager.PlaceInventory(j.Furniture.GetSpawnSpotTile(), Inventory.__new("steel_plate", 50, 20))
end
