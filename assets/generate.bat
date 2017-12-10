@ECHO OFF
CD %~dp0

SET BUILDROOT=%1
SET BUILDROOT=%BUILDROOT:"=%

SET TP="C:\Program Files\CodeAndWeb\TexturePacker\bin\TexturePacker.exe"
SET BASEOUTPUTPATH=%BUILDROOT%\assets\base
SET OPTIONS=--format xml --trim-sprite-names --extrude 0 --algorithm Basic 
SET OPTIONS=%OPTIONS% --trim-mode None --png-opt-level 0 --disable-auto-alias 
SET OPTIONS=%OPTIONS% --disable-rotation --size-constraints POT --force-word-aligned

ECHO ###########################################################################
ECHO # Generating Assets
ECHO ###########################################################################
ECHO Generating into %BASEOUTPUTPATH%
IF NOT EXIST "%BASEOUTPUTPATH%" MD %BASEOUTPUTPATH%
ECHO.


ECHO ###########################################################################
ECHO # Data
ECHO ###########################################################################
SET OUTPUTPATH=%BASEOUTPUTPATH%\data
XCOPY /Y /I Data\names_*.* %OUTPUTPATH%
XCOPY /Y /I Data\*.xml %OUTPUTPATH%
ECHO.


ECHO ###########################################################################
ECHO # LUA Scripts
ECHO ###########################################################################
SET OUTPUTPATH=%BASEOUTPUTPATH%\scripts
XCOPY /S /Y /I Scripts %OUTPUTPATH%
ECHO.


ECHO ###########################################################################
ECHO # Images
ECHO ###########################################################################

SET OUTPUTPATH=%BASEOUTPUTPATH%\images\characters
%TP% %OPTIONS% --data %OUTPUTPATH%\colonist.xml --sheet %OUTPUTPATH%\colonist.png characters\colonist-assets

SET OUTPUTPATH=%BASEOUTPUTPATH%\images\cursors
%TP% %OPTIONS% --data %OUTPUTPATH%\cursors.xml --sheet %OUTPUTPATH%\cursors.png Cursors\cursor-assets

SET OUTPUTPATH=%BASEOUTPUTPATH%\images\furniture
%TP% %OPTIONS% --data %OUTPUTPATH%\furn_wall_steel.xml --sheet %OUTPUTPATH%\furn_wall_steel.png furniture\furn_wall_steel-assets
%TP% %OPTIONS% --data %OUTPUTPATH%\furn_door_heavy.xml --sheet %OUTPUTPATH%\furn_door_heavy.png furniture\furn_door_heavy-assets
%TP% %OPTIONS% --data %OUTPUTPATH%\furn_door.xml --sheet %OUTPUTPATH%\furn_door.png furniture\furn_door-assets
%TP% %OPTIONS% --data %OUTPUTPATH%\furn_oxygen.xml --sheet %OUTPUTPATH%\furn_oxygen.png furniture\furn_oxygen-assets
%TP% %OPTIONS% --data %OUTPUTPATH%\furn_mining_station.xml --sheet %OUTPUTPATH%\furn_mining_station.png furniture\furn_mining_station-assets

SET OUTPUTPATH=%BASEOUTPUTPATH%\images\backgrounds
IF NOT EXIST "%OUTPUTPATH%" MD %OUTPUTPATH%
XCOPY /Y Backgrounds\starfield.jpg %OUTPUTPATH%

SET OUTPUTPATH=%BASEOUTPUTPATH%\images\tiles
IF NOT EXIST "%OUTPUTPATH%" MD %OUTPUTPATH%
%TP% %OPTIONS% --data %OUTPUTPATH%\floor.xml --sheet %OUTPUTPATH%\floor.png Tiles\floor-assets
ECHO.

ECHO ###########################################################################
ECHO # Fonts
ECHO ###########################################################################
SET OUTPUTPATH=%BASEOUTPUTPATH%\fonts
IF NOT EXIST "%OUTPUTPATH%" MD %OUTPUTPATH%
XCOPY /Y /I Fonts\*.ttf %OUTPUTPATH%
ECHO.



:finish