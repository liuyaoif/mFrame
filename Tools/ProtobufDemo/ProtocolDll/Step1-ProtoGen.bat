@echo Step1/3:��*.proto�ļ�ת����*.cs�ļ�

set protoFiles=-i:loginServer.proto
set CSharpFile=ProtocolCSharp.cs

del %CSharpFile%
ProtoGen\protogen %protoFiles% -o:%CSharpFile% -ns:com.pwrd.wuxiaprg
pause