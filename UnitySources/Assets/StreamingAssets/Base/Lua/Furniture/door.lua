function Clamp01(value)
  if(value > 1) then
    return 1
  end
  
  if(value < 0) then
    return 0
  end
  
  return value
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
  
  if(furniture.cbOnChanged != nil) then
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
