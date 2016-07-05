@echo off
set excelFiles=ExampleData.xlsx
::set excelFiles=1file.xlsx
set JsonOut=Json\
set CSharpOut=CSharp\
::echo %excelFiles%

rmdir /s /q %JsonOut%
rmdir /s /q %CSharpOut%
pause
mkdir %JsonOut%
mkdir %CSharpOut%
::pause

excel2json --excel %excelFiles% --json %JsonOut% --java %CSharpOut% --header 3
pause