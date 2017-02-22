-- Time (in seconds) it takes to go from 100% to 0% condition.
local wearRate = 600



function OnUpdate_MiningDroneStation( furniture, deltaTime )
 
 -- First we see if we need to damage this object a little bit.
  local newCondition = furniture.OffsetParameter("condition", -(1/wearRate) * deltaTime, 0, 1);
  local active = true
  
  -- If it's knackered, don't do any work
  if(newCondition <= 0.0) then
   active = false
  end
  
  if(active) then
    local spawnSpot = furniture.GetSpawnSpotTile()

    if( furniture.JobCount() > 0 ) then

      -- Check to see if the Metal Plate destination tile is full.
      if( spawnSpot.Inventory ~= nil and spawnSpot.Inventory.StackSize >= spawnSpot.Inventory.MaxStackSize ) then
        -- We should stop this job, because it's impossible to make any more items.
        furniture.CancelJobs()
      end

      active = false
    end

    -- If we get here, then we have no current job. Check to see if our destination is full.
    if(active) then
      if( spawnSpot.Inventory ~= nil and spawnSpot.Inventory.StackSize >= spawnSpot.Inventory.MaxStackSize ) then
        -- We are full! Don't make a job!
        active = false
      end
    end
  end
  
  -- If we get here, we need to CREATE a new job.
  if(active) then
    local jobSpot = furniture.GetJobSpotTile()

    j = Job.__new(
      jobSpot,
      nil,
      nil,
      1,
      nil,
      true	-- This job repeats until the destination tile is full.
    )
    j.Name = "replicating_iron"
    j.Description = "Replicating iron"
    j.RegisterOnJobCompletedCallback("MiningDroneStation_JobComplete")

    furniture.AddJob( j )
  end

  if(furniture.cbOnChanged != nil) then
    furniture.cbOnChanged(furniture)
  end
  
end


function MiningDroneStation_JobComplete(j)
  local req = Inventory.__new("steel_plate", 50, 20)
  World.Instance.InventoryManager.TransferInventory(j.Furniture.GetSpawnSpotTile(), req)
end
