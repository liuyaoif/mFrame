using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace excel2json
{
    /// <summary>
    /// 将DataTable对象，转换成JSON string，并保存到文件中
    /// </summary>
    class JsonGenerator
    {
        private Dictionary<string, Dictionary<string, object>> m_data;

        public string FileComment
        {
            get;
            set;
        }

        /// <summary>
        /// 构造函数：完成内部数据创建
        /// </summary>
        /// <param name="sheet">ExcelReader创建的一个表单</param>
        /// <param name="headerRows">表单中的那几行是表头</param>
        public JsonGenerator(DataTable sheet, int headerRows, bool lowcase)
        {
            try
            {
                if (sheet.Columns.Count <= 0)
                {
                    return;
                }
                if (sheet.Rows.Count <= 0)
                {
                    return;
                }

                m_data = new Dictionary<string, Dictionary<string, object>>();

                //类型定义行
                DataRow typeRow = sheet.Rows[0];

                //--以第一列为ID，转换成ID->Object的字典
                int firstDataRow = headerRows - 1;
                for (int i = firstDataRow; i < sheet.Rows.Count; i++)
                {
                    DataRow row = sheet.Rows[i];
                    string ID = row[sheet.Columns[0]].ToString();
                    if (string.IsNullOrEmpty(ID))
                    {
                        continue;
                    }

                    var rowData = new Dictionary<string, object>();
                    foreach (DataColumn column in sheet.Columns)
                    {
                        object type = typeRow[column];
                        object value = row[column];

                        if (type.GetType() == typeof(DBNull))
                        {
                            continue;
                        }

                        switch ((string)type)
                        {
                            case "int":
                                {
                                    if (value.GetType() == typeof(double))
                                    {
                                        double doubleValue = (double)value;
                                        value = (int)doubleValue;
                                    }
                                    else if (value.GetType() == typeof(string))
                                    {
                                        int intValue = int.Parse((string)value);
                                        value = intValue;
                                    }
                                }
                                break;

                            case "int[]":
                                {
                                    List<int> intList = new List<int>();
                                    string strValue = ToStringValue(value);

                                    if (strValue != "-1")
                                    {
                                        string[] strArr = (strValue).Split(";".ToCharArray());
                                        for (int index = 0; index < strArr.Length; index++)
                                        {
                                            if (string.IsNullOrEmpty(strArr[index]))
                                            {
                                                continue;
                                            }

                                            intList.Add(int.Parse(strArr[index]));
                                        }
                                    }

                                    value = (intList.Count == 0) ? null : intList;
                                }
                                break;

                            case "bool":
                                {
                                    double doubleValue = (double)value;
                                    value = (doubleValue == 0) ? false : true;
                                }
                                break;

                            case "bool[]":
                                {
                                    List<int> boolArr = new List<int>();
                                    string strValue = ToStringValue(value);
                                    if (strValue != "-1")
                                    {
                                        string[] strArr = (strValue).Split(";".ToCharArray());
                                        for (int index = 0; index < strArr.Length; index++)
                                        {
                                            if (string.IsNullOrEmpty(strArr[index]))
                                            {
                                                continue;
                                            }

                                            boolArr.Add(int.Parse(strArr[index]) == 0 ? 0 : 1);
                                        }
                                    }
                                    value = (boolArr.Count == 0) ? null : boolArr;
                                }
                                break;

                            case "string":
                                {
                                    string strValue = ToStringValue(value);
                                    value = strValue;
                                }
                                break;

                            case "string[]":
                                {
                                    List<string> strArray = new List<string>();
                                    string strValue = ToStringValue(value);
                                    if (strValue != "-1")
                                    {
                                        string[] strs = (strValue).Split(";".ToCharArray());
                                        for (int index = 0; index < strs.Length; index++)
                                        {
                                            if (string.IsNullOrEmpty(strs[index]))
                                            {
                                                continue;
                                            }
                                            strArray.Add(strs[index]);
                                        }
                                    }

                                    value = (strArray.Count == 0) ? null : strArray;
                                }
                                break;

                            case "float":
                                {
                                    double doubleValue = (double)value;
                                    value = (float)doubleValue;
                                }
                                break;

                            case "float[]":
                                {
                                    List<float> floatArr = new List<float>();
                                    string strValue = ToStringValue(value);
                                    if (strValue != "-1")
                                    {
                                        string[] strArr = strValue.Split(";".ToCharArray());
                                        for (int index = 0; index < strArr.Length; index++)
                                        {
                                            if (string.IsNullOrEmpty(strArr[index]))
                                            {
                                                continue;
                                            }

                                            floatArr.Add(float.Parse(strArr[index]));
                                        }
                                    }

                                    value = (floatArr.Count == 0) ? null : floatArr;
                                }
                                break;
                        }

                        string fieldName = column.ToString();

                        // 表头自动转换成小写
                        if (lowcase)
                        {
                            fieldName = fieldName.ToLower();
                        }

                        if (!string.IsNullOrEmpty(fieldName) &&
                            fieldName.IndexOf("Column") == -1 &&
                            value != null)
                        {
                            rowData[fieldName] = value;
                        }
                    }

                    if (m_data.ContainsKey(ID))
                    {
                        throw new Exception(sheet.TableName + " " + "ID: " + ID.ToString() + " 重叠");
                    }
                    m_data[ID] = rowData;
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(" Json generating error: " + sheet.TableName);
            }
        }

        public string ToStringValue(object value)
        {
            string strValue = "";

            if (value.GetType() == typeof(DBNull))
            {
                return "";
            }
            else if (value.GetType() == typeof(double))
            {
                strValue = value.ToString();
            }
            else
            {
                strValue = (string)value;
            }
            return strValue;
        }

        /// <summary>
        /// 将内部数据转换成Json文本，并保存至文件
        /// </summary>
        /// <param name="jsonPath">输出文件路径</param>
        public void SaveToFile(string filePath, Encoding encoding)
        {
            if (m_data == null)
                throw new Exception("JsonExporter内部数据为空。");

            //-- 转换为JSON字符串
            string json = JsonConvert.SerializeObject(m_data, Formatting.Indented);

            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                {
                    writer.Write(json);
                }
            }
        }
    }
}
