@echo Step3/3:�������ProtobufSerializer.dll��
@echo ����ProtocolDll.dll��ProtobufSerializer.dll��Assets���档
@echo һ��Ҫ����all done�ű���ɹ���

set protoDll=bin\Release\ProtocolDll.dll
set ProtobufSerializer=ProtobufSerializer.dll

::�������ProtobufSerializer.dll��-t������
Precompile\precompile %protoDll% -o:%ProtobufSerializer% -t:ProtobufSerializer
copy %protoDll% ..\Assets\
copy %ProtobufSerializer% ..\Assets\

::�������
rmdir /s /q bin
del ProtocolDll.dll
del %ProtobufSerializer%
pause