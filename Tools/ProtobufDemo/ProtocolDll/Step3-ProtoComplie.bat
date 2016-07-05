@echo Step3/3:编译产生ProtobufSerializer.dll。
@echo 拷贝ProtocolDll.dll和ProtobufSerializer.dll到Assets下面。
@echo 一定要看到all done才编译成功。

set protoDll=bin\Release\ProtocolDll.dll
set ProtobufSerializer=ProtobufSerializer.dll

::编译产生ProtobufSerializer.dll，-t是类名
Precompile\precompile %protoDll% -o:%ProtobufSerializer% -t:ProtobufSerializer
copy %protoDll% ..\Assets\
copy %ProtobufSerializer% ..\Assets\

::清理操作
rmdir /s /q bin
del ProtocolDll.dll
del %ProtobufSerializer%
pause