@ECHO OFF

SET BASEOUTPUTPATH=..\UnitySources\Assets\StreamingAssets\Base\Images
SET OPTIONS=--format xml --trim-sprite-names --extrude 0 --algorithm Basic 
SET OPTIONS=%OPTIONS% --trim-mode None --png-opt-level 0 --disable-auto-alias 
SET OPTIONS=%OPTIONS% --disable-rotation --size-constraints POT --force-word-aligned



SET OUTPUTPATH=%BASEOUTPUTPATH%\Characters
TexturePacker %OPTIONS% --data %OUTPUTPATH%\colonist.xml --sheet %OUTPUTPATH%\colonist.png characters\colonist-assets

SET OUTPUTPATH=%BASEOUTPUTPATH%\Furniture
TexturePacker %OPTIONS% --data %OUTPUTPATH%\furn_wall_steel.xml --sheet %OUTPUTPATH%\furn_wall_steel.png furniture\furn_wall_steel-assets
TexturePacker %OPTIONS% --data %OUTPUTPATH%\furn_door_heavy.xml --sheet %OUTPUTPATH%\furn_door_heavy.png furniture\furn_door_heavy-assets
TexturePacker %OPTIONS% --data %OUTPUTPATH%\furn_door.xml --sheet %OUTPUTPATH%\furn_door.png furniture\furn_door-assets
TexturePacker %OPTIONS% --data %OUTPUTPATH%\furn_oxygen.xml --sheet %OUTPUTPATH%\furn_oxygen.png furniture\furn_oxygen-assets
TexturePacker %OPTIONS% --data %OUTPUTPATH%\furn_mining_station.xml --sheet %OUTPUTPATH%\furn_mining_station.png furniture\furn_mining_station-assets

