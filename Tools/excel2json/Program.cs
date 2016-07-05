using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace excel2json
{
    sealed partial class Program
    {
        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;

            //-- 分析命令行参数
            Options options = new Options();
            var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error);

            Action Onfailed = () =>
                {
                    Console.WriteLine("Parse option failed");
                };

            if (parser.ParseArgumentsStrict(args, options, Onfailed))
            {
                //-- 执行导出操作
                Run(options);
            }

            //-- 程序计时
            DateTime endTime = DateTime.Now;
            TimeSpan dur = endTime - startTime;

            Console.WriteLine(string.Format("Finish parse. Total cost[{0}ms].", dur.Milliseconds));
        }

        /// <summary>
        /// 根据命令行参数，执行Excel数据导出工作
        /// </summary>
        /// <param name="options">命令行参数</param>
        private static void Run(Options options)
        {
            string excelPath = options.ExcelPath;

            string[] excelFiles = options.ExcelPath.Split(";".ToCharArray());

            if (excelFiles.Length == 0)//只配置了一个excel文件
            {
                excelFiles = new string[] { options.ExcelPath };
            }

            //用于产生string到
            List<string> typeMapperList = new List<string>();

            foreach (string curExcelFileName in excelFiles)
            {
                try
                {
                    if (string.IsNullOrEmpty(curExcelFileName))//文件名为空，跳过
                    {
                        continue;
                    }

                    // 加载Excel文件
                    using (FileStream curExcelFile = File.Open(curExcelFileName, FileMode.Open, FileAccess.Read))
                    {
                        // Reading from a OpenXml Excel file (2007 format; *.xlsx)
                        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(curExcelFile);

                        // The result of each spreadsheet will be created in the result.Tables
                        excelReader.IsFirstRowAsColumnNames = true;
                        DataSet book = excelReader.AsDataSet();

                        //-- 确定编码
                        Encoding cd = new UTF8Encoding(false);
                        if (options.Encoding != "utf8-nobom")
                        {
                            foreach (EncodingInfo ei in Encoding.GetEncodings())
                            {
                                Encoding e = ei.GetEncoding();
                                if (e.EncodingName == options.Encoding)
                                {
                                    cd = e;
                                    break;
                                }
                            }
                        }

                        //检查sheet个数
                        if (book.Tables.Count < 1)
                        {
                            throw new Exception("Excel文件中没有找到Sheet: " + curExcelFile);
                        }

                        //取出sheet
                        foreach (DataTable sheet in book.Tables)
                        {
                            string sheetName = sheet.TableName;
                            bool isContainChinese = System.Text.RegularExpressions.Regex.IsMatch(sheetName, @"[\u4e00-\u9fa5]");

                            if (isContainChinese)//如果Sheet是中文名，则跳过
                            {
                                continue;
                            }

                            //检查sheet是否为空
                            if (sheet.Rows.Count <= 0)
                            {
                                throw new Exception("Excel Sheet中没有数据: " + excelPath);
                            }

                            //-- 导出JSON文件
                            if (options.JsonPath != null && options.JsonPath.Length > 0)
                            {
                                string jsonOutPath = options.JsonPath + sheetName + ".json";
                                JsonGenerator exporter = new JsonGenerator(sheet, options.HeaderRows, options.Lowcase);
                                exporter.FileComment = string.Format("// Generate From {0}", curExcelFileName
                                    + ". SheetName: " + sheetName + "\n");

                                exporter.SaveToFile(jsonOutPath, cd);
                            }

                            //-- 生成C#定义文件
                            if (options.CSharpPath != null && options.CSharpPath.Length > 0)
                            {
                                string excelName = Path.GetFileName(excelPath);

                                CSDefineGenerator exporter = new CSDefineGenerator(sheet, options.HeaderRows);
                                exporter.ClassComment = string.Format("// Generate From {0}", curExcelFileName
                                     + ". SheetName: " + sheetName);
                                string cSharpOutPath = options.CSharpPath + sheetName + ".cs";
                                exporter.SaveToFile(cSharpOutPath, cd);
                                typeMapperList.Add(sheetName);
                            }

                            //生成java定义文件
                            if (options.JavaPath != null && options.JavaPath.Length > 0 &&
                                options.JavaPackage != null && options.JavaPackage.Length > 0)
                            {
                                string excelName = Path.GetFileName(excelPath);
                                JavaDefineGenerator exporter = new JavaDefineGenerator(sheet);

                                exporter.ClassComment = string.Format("// Generate From {0}", curExcelFileName
                                     + ". SheetName: " + sheetName);

                                string javaOutPath = options.JavaPath + sheetName + ".java";
                                exporter.SaveToFile(options.JavaPackage, options.javaImport, javaOutPath, cd);
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine(curExcelFileName + " Error: " + exp.Message
                        + "Stack: " + exp.StackTrace);
                }
            }

            //-- 生成C#的映射文件
            if (options.CSharpPath != null && options.CSharpPath.Length > 0)
            {
                string mapFilePath = options.CSharpPath + "ConfigMapper.cs";
                CSMapperGenerator.SaveMapFile(typeMapperList, mapFilePath, new UTF8Encoding(false));
            }
        }//Run
    }
}
