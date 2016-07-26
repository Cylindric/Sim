local function Clamp01(value)
  if(value > 1) then
    return 1
  end
  
  if(value < 0) then
    return 0
  end
  
  return value
end

local function EqualizeGasses(furniture, deltaTime)
  if(furniture.Tile == nil) then
    return
  end
  
  -- If one side of the door has more of a gas than the other, "leak" it across.
  local rate = 0.1 * deltaTime
  
  -- Test to see if this is a horizontal door with N/S rooms.
  local northTile = furniture.Tile.NorthNeighbour()
  if(northTile ~= nil and northTile.Room ~= nil and northTile.Room.IsOutsideRoom() == false) then
    -- north of the door is a room.
    local southTile = furniture.Tile.SouthNeighbour()
    if(southTile ~= nil and southTile.Room ~= nil and southTile.Room.IsOutsideRoom() == false) then
      -- south of the door is a room too.
      
      local northAtmos = northTile.Room.Atmosphere
      local southAtmos = southTile.Room.Atmosphere
      northAtmos.EqualiseAtmosphere(southAtmos, rate)
      
    end
  else
    
    -- Test to see if this is a vertical door with E/W rooms.
    local eastTile = furniture.Tile.EastNeighbour()
    if(eastTile ~= nil and eastTile.Room ~= nil and eastTile.Room.IsOutsideRoom() == false) then
      -- east of the door is a room.
      local westTile = furniture.Tile.WestNeighbour()
      if(westTile ~= nil and westTile.Room ~= nil and westTile.Room.IsOutsideRoom() == false) then
        -- west of the door is a room.
        local eastAtmos = eastTile.Room.Atmosphere
        local westAtmos = westTile.Room.Atmosphere
        eastAtmos.EqualiseAtmosphere(westAtmos, rate)

      end
    end
  end

end
function OnUpdate_Door(furniture, deltaTime)
  
  if (furniture.GetParameter("is_opening") >= 1) then
    furniture.OffsetParameter("openness", deltaTime * 8)

    if (furniture.GetParameter("openness") >= 1) then
      furniture.SetParameter("is_opening", 0)
    end
  
  else
    furniture.OffsetParameter("openness", deltaTime * -4)
  end

  furniture.SetParameter("openness", Clamp01(furniture.GetParameter("openness")))

  EqualizeGasses(furniture, deltaTime)

  if(furniture.cbOnChanged ~= nil) then
    furniture.cbOnChanged(furniture)
  end
end

function IsEnterable_Door(furniture)
  furniture.SetParameter("is_opening", 1)

  if (furniture.GetParameter("openness") >= 1) then
    return 0 -- Enterability.Yes
  end

  return 2 -- Enterability.Soon
end

