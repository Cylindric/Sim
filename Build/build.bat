@ECHO OFF

PUSHD ..\UnitySources
Unity.exe -quit -batchmode -buildWindows64Player ..\Build\PC\Sim.exe
POPD
ERASE PC\*.pdb