-- The base fill-rate for the O2 Generator. Sort of in m³ per second.
-- TODO: Will need a way to both add Nitrogen and O2, and scrub them too, probably with different rates for all combos.
-- TODO: Will need a way to both add Nitrogen and O2, and scrub them too, probably with different rates for all combos.
local pumpRate = 5

-- Time (in seconds) it takes to go from 100% to 0% condition.
local wearRate = 1200

-- Condition at which pump performance degrades
local wearSlowDownLevel = 0.3



-- The Oxygen Generator adds Nitrogen and Oxygen to try and maintain an 80/20 balance.
function OnUpdate_GasGenerator(furniture, deltaTime)
  local actionTaken = ""
  
  -- First we see if we need to damage this object a little bit.
  local newCondition = furniture.OffsetParameter("condition", -(1/wearRate) * deltaTime, 0, 1);

  -- If it's knackered, don't do any work
  if(newCondition <= 0.0) then
    furniture.GasParticlesEnabled = false
    return
  end
  
  local effectiveRate = pumpRate
  if(newCondition < wearSlowDownLevel) then
    effectiveRate = effectiveRate * 0.5
  end
  
  actionTaken = actionTaken .. deltaTime .. ".  "
  
  local tolerance = 0.005
  local targetPressure = 1.000 -- 101.3 kPa is the same as they use for the ISS, so should be good enough for us.
  local targetN2 =  0.78090 -- 78% Nitrogen
  local targetO2 =  0.20950 -- 21% Oxygen
  local targetAr =  0.00930 --  1% Argon
  local targetCO2 = 0.00039 -- trace CO2


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
    furniture.Tile.Room.Atmosphere.ChangeGas("CO2", -effectiveRate * deltaTime)
    actionTaken = actionTaken .. "Removed " .. (-effectiveRate * deltaTime) .. " CO2.  "
  end
 
  local totalPressure = furniture.Tile.Room.Atmosphere.GetTotalAtmosphericPressure()
  local pressureDifference = targetPressure - totalPressure
  furniture.GasParticlesEnabled = false  
  
  if(pressureDifference < 0 and math.abs(pressureDifference) > tolerance) then
    -- Need to remove atmosphere, so take out Nitrogen
    furniture.Tile.Room.Atmosphere.ChangeGas("N2", -effectiveRate * deltaTime)
    actionTaken = actionTaken .. "Removed " .. (-effectiveRate * deltaTime) .. " N2.  "
  
  elseif(pressureDifference > tolerance) then
    -- Adding atmosphere

    if (targetO2 - furniture.Tile.Room.Atmosphere.GetGasPercentage("O2") > tolerance) then
      -- Pump Oxy!
      furniture.Tile.Room.Atmosphere.ChangeGas("O2", effectiveRate * deltaTime)
      actionTaken = "Added " .. (effectiveRate * deltaTime) .. " O2.  "
      furniture.GasParticlesEnabled = true
    else
      -- Pump Nitro!
      furniture.Tile.Room.Atmosphere.ChangeGas("N2", effectiveRate * deltaTime)
      actionTaken = "Added " .. (effectiveRate * deltaTime) .. " N2.  "
      furniture.GasParticlesEnabled = true
    end
  end
  
  if(furniture.cbOnChanged != nil) then
    furniture.cbOnChanged(furniture)
  end
  
  return -- actionTaken
end
