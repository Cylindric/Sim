local timeSinceLastPump = 0

-- The Oxygen Generator adds Nitrogen and Oxygen to try and maintain an 80/20 balance.
function OnUpdate_GasGenerator(furniture, deltaTime)
  local actionTaken = ""
  local timeSinceLastPump = timeSinceLastPump + deltaTime
  
  if (timeSinceLastPump < 0.0) then
    return
  end
    
  -- Speed up the deltaTime a bit to make it more interesting
  deltaTime = timeSinceLastPump * 1
  timeSinceLastPump = 0

  actionTaken = actionTaken .. deltaTime .. ".  "
  
  local tolerance = 0.005
  local targetPressure = 1.000 -- 101.3 kPa is the same as they use for the ISS, so should be good enough for us.
  local targetN2 =  0.78090 -- 78% Nitrogen
  local targetO2 =  0.20950 -- 21% Oxygen
  local targetAr =  0.00930 --  1% Argon
  local targetCO2 = 0.00039 -- trace CO2

  -- The base fill-rate for the O2 Generator.
  -- TODO: Will need a way to both add Nitrogen and O2, and scrub them too, probably with different rates for all combos.
  -- TODO: Will need a way to both add Nitrogen and O2, and scrub them too, probably with different rates for all combos.
  local baseRate = 5 -- m³ per second

  if (furniture == nil) then
    return "Furn is nil!"
  end

  if (furniture.Tile == nil) then
    return "Furn Tile is null!"
  end

  if (furniture.Tile.Room == nil) then
    return "Oxygen Generator at [" .. furniture.Tile.X .. "," .. furniture.Tile.Y .. "] has no Room!"
  end 

  if (furniture.Tile.Room.Size == 0) then
    return -- "Oxygen Generator at [" .. furniture.Tile.X .. "," .. furniture.Tile.Y .. "] is in a Room with no Tiles!"
  end
 
  -- Always remove CO2, even if that's all there is. It's never a good thing.
  if (furniture.Tile.Room.Atmosphere.GetGasPercentage("CO2") > 0.0) then
    furniture.Tile.Room.Atmosphere.ChangeGas("CO2", -baseRate * deltaTime)
    actionTaken = actionTaken .. "Removed " .. (-baseRate * deltaTime) .. " CO2.  "
  end
 
  local totalPressure = furniture.Tile.Room.Atmosphere.GetTotalAtmosphericPressure()
  local pressureDifference = targetPressure - totalPressure
  furniture.GasParticlesEnabled = false  
  
  if(pressureDifference < 0 and math.abs(pressureDifference) > tolerance) then
    -- Need to remove atmosphere, so take out Nitrogen
    furniture.Tile.Room.Atmosphere.ChangeGas("N2", -baseRate * deltaTime)
    actionTaken = actionTaken .. "Removed " .. (-baseRate * deltaTime) .. " N2.  "
  
  elseif(pressureDifference > tolerance) then
    -- Adding atmosphere

    if (targetO2 - furniture.Tile.Room.Atmosphere.GetGasPercentage("O2") > tolerance) then
      -- Pump Oxy!
      furniture.Tile.Room.Atmosphere.ChangeGas("O2", baseRate * deltaTime)
      actionTaken = "Added " .. (baseRate * deltaTime) .. " O2.  "
      furniture.GasParticlesEnabled = true
    else
      -- Pump Nitro!
      furniture.Tile.Room.Atmosphere.ChangeGas("N2", baseRate * deltaTime)
      actionTaken = "Added " .. (baseRate * deltaTime) .. " N2.  "
      furniture.GasParticlesEnabled = true
    end
  end
  
  if(furniture.cbOnChanged != nil) then
    furniture.cbOnChanged(furniture)
  end
  
  return -- actionTaken
end
