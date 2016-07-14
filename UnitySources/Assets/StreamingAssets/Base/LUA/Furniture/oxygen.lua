local timeSinceLastPump = 0

-- The Oxygen Generator adds Nitrogen and Oxygen to try and maintain an 80/20 balance.
function OnUpdate_GasGenerator(furniture, deltaTime)
  actionTaken = ""
  
  timeSinceLastPump = timeSinceLastPump + deltaTime
  
  if (timeSinceLastPump < 0.5) then
    return
  end
    
  -- Speed up the deltaTime a bit to make it more interesting
  deltaTime = timeSinceLastPump * 1
  timeSinceLastPump = 0

  actionTaken = actionTaken .. deltaTime .. ".  "
  
  targetPressure = 1.000 -- 101.3 kPa is the same as they use for the ISS, so should be good enough for us.
  targetN2 =  0.78090 -- 78% Nitrogen
  targetO2 =  0.20950 -- 21% Oxygen
  targetAr =  0.00930 --  1% Argon
  targetCO2 = 0.00039 -- trace CO2

  -- The base fill-rate for the O2 Generator.
  -- TODO: Will need a way to both add Nitrogen and O2, and scrub them too, probably with different rates for all combos.
  baseRate = 1 -- m³ per second

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
    return "Oxygen Generator at [" .. furniture.Tile.X .. "," .. furniture.Tile.Y .. "] is in a Room with no Tiles!"
  end
 
  -- Always remove CO2, even if that's all there is. It's never a good thing.
  if (furniture.Tile.Room.Atmosphere.GetGasPercentage("CO2") > 0.0) then
    furniture.Tile.Room.Atmosphere.ChangeGas("CO2", -baseRate * deltaTime)
    actionTaken = actionTaken .. "Added " .. (-baseRate * deltaTime) .. " CO2.  "
  end
 
  totalPressure = furniture.Tile.Room.Atmosphere.GetTotalAtmosphericPressure()
  
  if(totalPressure > targetPressure) then
    -- Need to remove atmosphere, so take out Nitrogen
    furniture.Tile.Room.Atmosphere.ChangeGas("N2", -baseRate * deltaTime)
    actionTaken = actionTaken .. "Removed " .. (-baseRate * deltaTime) .. " N2.  "
  
  else
    -- Adding atmosphere
  
    if (furniture.Tile.Room.Atmosphere.GetGasPercentage("O2") < targetO2) then
      -- Pump Oxy!
      furniture.Tile.Room.Atmosphere.ChangeGas("O2", baseRate * deltaTime)
      actionTaken = "Added " .. (baseRate * deltaTime) .. " O2.  "
    else
      -- Pump Nitro!
      furniture.Tile.Room.Atmosphere.ChangeGas("N2", baseRate * deltaTime)
      actionTaken = "Added " .. (baseRate * deltaTime) .. " N2.  "
    end
  end
  
  return -- actionTaken
end
