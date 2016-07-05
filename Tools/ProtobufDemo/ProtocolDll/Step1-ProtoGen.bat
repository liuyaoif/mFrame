@echo Step1/3:把*.proto文件转换成*.cs文件

set protoFiles=-i:loginServer.proto
set CSharpFile=ProtocolCSharp.cs

del %CSharpFile%
ProtoGen\protogen %protoFiles% -o:%CSharpFile% -ns:com.pwrd.wuxiaprg
pause