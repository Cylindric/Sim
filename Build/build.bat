@ECHO OFF

IF EXIST PC RMDIR /S /Q PC

PUSHD ..\UnitySources
Unity.exe -quit -batchmode -buildWindows64Player ..\Build\PC\Sim.exe
POPD
ERASE PC\*.pdb