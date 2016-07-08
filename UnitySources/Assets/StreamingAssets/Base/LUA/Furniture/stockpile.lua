function Stockpile_GetItemsFromFilter()
	-- TODO: This should be reading from some kind of UI for this
	-- particular stockpile

	-- Probably, this doesn't belong in Lua at all and instead we should
	-- just be calling a C# function to give us the list.

	-- Since jobs copy arrays automatically, we could already have
	-- an Inventory[] prepared and just return that (as a sort of example filter)

	return { Inventory.__new("steel_plate", 50, 0) }
end

function Stockpile_UpdateAction( furniture, deltaTime )
	-- We need to ensure that we have a job on the queue
	-- asking for either:
	--  (if we are empty): That ANY loose inventory be brought to us.
	--  (if we have something): Then IF we are still below the max stack size,
	--						    that more of the same should be brought to us.

	-- TODO: This function doesn't need to run each update.  Once we get a lot
	-- of furniture in a running game, this will run a LOT more than required.
	-- Instead, it only really needs to run whenever:
	--		-- It gets created
	--		-- A good gets delivered (at which point we reset the job)
	--		-- A good gets picked up (at which point we reset the job)
	--		-- The UI's filter of allowed items gets changed


	if( furniture.Tile.Inventory != nil and furniture.Tile.Inventory.StackSize >= furniture.Tile.Inventory.MaxStackSize ) then
		-- We are full!
		furniture.CancelJobs()
		return
	end

	-- Maybe we already have a job queued up?
	if( furniture.JobCount() > 0 ) then
		-- Cool, all done.
		return
	end


	-- We currently are NOT full, but we don't have a job either.
	-- Two possibilities: Either we have SOME inventory, or we have NO inventory.

	-- Third possibility: Something is WHACK
	if( furniture.Tile.Inventory != nil and furniture.Tile.Inventory.StackSize == 0 ) then
		furniture.CancelJobs()
		return "Stockpile has a zero-size stack. This is clearly WRONG!"
	end


	-- TODO: In the future, stockpiles -- rather than being a bunch of individual
	-- 1x1 tiles -- should manifest themselves as single, large objects.  This
	-- would respresent our first and probably only VARIABLE sized "furniture" --
	-- at what happenes if there's a "hole" in our stockpile because we have an
	-- actual piece of furniture (like a cooking stating) installed in the middle
	-- of our stockpile?
	-- In any case, once we implement "mega stockpiles", then the job-creation system
	-- could be a lot smarter, in that even if the stockpile has some stuff in it, it
	-- can also still be requestion different object types in its job creation.

	itemsDesired = {}

	if( furniture.Tile.inventory == nil ) then
		itemsDesired = Stockpile_GetItemsFromFilter()
	else
		desInv = furniture.Tile.Inventory.Clone()
		desInv.MaxStackSize = desInv.MaxStackSize - desInv.StackSize
		desInv.StackSize = 0

		itemsDesired = { desInv }
	end

	j = Job.__new(
		furniture.Tile,
		nil,
		nil,
		0,
		itemsDesired,
		false
	)

	-- TODO: Later on, add stockpile priorities, so that we can take from a lower
	-- priority stockpile for a higher priority one.
	j.CanTakeFromStockpile = false

	j.RegisterOnJobWorkedCallback("Stockpile_JobWorked")
	furniture.AddJob( j )
end

function Stockpile_JobWorked(j)
	j.CancelJob()

	-- TODO: Change this when we figure out what we're doing for the all/any pickup job.
	--values = j.GetInventoryRequirementValues();
	for k, inv in pairs(j.InventoryRequirements) do
		if(inv.StackSize > 0) then
			World.Current.InventoryManager.PlaceInventory(j.tile, inv)

			return  -- There should be no way that we ever end up with more than on inventory requirement with stackSize > 0
		end
	end
end
