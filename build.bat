@echo off

rem clear directories
if exist build RD /S /Q build
mkdir build
if exist bin RD /S /Q bin
mkdir bin

rem build llama.cpp
cd build
cmake ..
cmake --build . --config Release
cd ..

rem copy llama.cpp dll
xcopy build\bin\Release\* bin

rem build wrapper
dotnet publish llama.NET.sln -c Release -f net7.0 -o bin

echo done!
pause