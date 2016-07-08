-- The Oxygen Generator adds Nitrogen and Oxygen to try and maintain an 80/20 balance.
function OnUpdate_GasGenerator(furniture, deltaTime)
    
    targetPressure = 1.013 -- 101.3 kPa is the same as they use for the ISS, so should be good enough for us.
    targetN2 =  0.78090 -- 78% Nitrogen
    targetO2 =  0.20950 -- 21% Oxygen
    targetAr =  0.00930 --  1% Argon
    targetCO2 = 0.00039 -- trace CO2

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

  currentPressure = furniture.Tile.Room.GetTotalAtmosphericPressure()
  space = targetPressure - currentPressure
  
  -- The base fill-rate for the O2 Generator.
  -- TODO: Will need a way to both add Nitrogen and O2, and scrub them too, probably with different rates for all combos.
  baseRate = 1

  -- The rate depends on the size of the room being affected.
  -- Larger rooms take longer
  roomSizeMulti = 1/furniture.Tile.Room.Size
  
  -- The final rate
  rate = baseRate * roomSizeMulti -- the amount to add per-second.
  qty = rate * deltaTime -- the amount to add on this frame-tick.
  qty = math.min(qty, space) -- Try not to go into over-pressure.
  
  
  if (currentPressure >= targetPressure) then
    -- Already at target pressure, so will not add any more! Otherwise ears go pop!
    return
  
  else
    -- Add various gases from the tanks until they're reached the desired concentrations.
    if (furniture.Tile.Room.GetGasPercentage("O2") < targetO2) then
      -- Pump Oxy!
      furniture.Tile.Room.ChangeGas("O2", qty)
    else
      -- Pump Nitro!
      furniture.Tile.Room.ChangeGas("N", qty)
    end
  end
end
