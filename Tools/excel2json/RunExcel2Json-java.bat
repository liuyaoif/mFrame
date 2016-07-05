@echo off
set excelFiles=ExampleData.xlsx

set JsonOut=Json\
set CSharpOut=CSharp\
set JavaOut=Java\

rmdir /s /q %JsonOut%
rmdir /s /q %CSharpOut%
rmdir /s /q %JavaOut%

mkdir %JsonOut%
mkdir %CSharpOut%
mkdir %JavaOut%

excel2json --excel %excelFiles% --json %JsonOut% --csharp %CSharpOut% --java %JavaOut% --javaPackage com.wuxia.rpg.jsonParser --header 4
pause