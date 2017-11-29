function OnUpdate_Bed( furniture, deltaTime )
  local active = true
  
  if( furniture.JobCount() > 0 ) then
    -- There is already a rest-job setup for this bed.
    active = false
  end

  if(active) then

    j = Job.__new(
      furniture.GetJobSpotTile(), -- Tile
      nil, -- JobObjectType
      nil, -- cbJobComplete
      1, -- jobTime
      nil, -- inventoryRequirements
      true	-- jobRepeats: This job repeats
    )
    j.Name = "resting"
    j.Description = "Resting"

    furniture.AddJob( j )
  end

  if(furniture.cbOnChanged != nil) then
    furniture.cbOnChanged(furniture)
  end

end
