@ECHO OFF

IF EXIST PC RMDIR /S /Q PC

PUSHD ..\UnitySources
Unity.exe -quit -batchmode -buildWindows64Player ..\Build\PC\Sim.exe
Unity.exe -quit -batchmode -buildLinux64Player ..\Build\Linux64\Sim
 
POPD
ERASE PC\*.pdb