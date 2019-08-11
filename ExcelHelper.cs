using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using CommLiby;
using Lite.ExcelLibrary.BinaryFileFormat;
using Lite.ExcelLibrary.SpreadSheet;

namespace CommonLibSL
{
    public class ExcelHelper
    {
        public static void Export<T>(IEnumerable<T> source, Stream fs, SheetColumn[] sheetColumns = null, string sheetName = "") where T : class
        {
            Type t = typeof(T);

            var columnPropertys = getColomnPropertyInfos<T>();
            var columnHeaders = new List<SheetColumn>();
            var colIndex = 0;
            if (sheetColumns == null)
            {
                columnHeaders = columnPropertys.Select(x => new SheetColumn { Header = x.Name, DataProperty = x.Name, ColIndex = colIndex++, Pi = x }).ToList();
            }
            else
            {
                columnHeaders = sheetColumns.Select(x =>
                {
                    x.ColIndex = colIndex++;
                    return x;
                }).ToList();
                columnPropertys = columnPropertys.Join(sheetColumns, x => x.Name, y => y.DataProperty, (x, y) =>
                {
                    y.Pi = x;
                    return x;
                }).ToList();
            }

            if (string.IsNullOrEmpty(sheetName))
            {
                sheetName = "Sheet1";
            }

            Workbook newWorkBook = new Workbook();
            Worksheet newWorkSheet = new Worksheet(sheetName);
            newWorkSheet.SheetType = SheetType.Worksheet;

            for (int i = 0; i < columnHeaders.Count; i++)
            {
                SheetColumn x = columnHeaders[i];
                Cell headerCell = new Cell(x.Header);
                newWorkSheet.Cells[0, i] = headerCell;
                newWorkSheet.Cells.ColumnWidth[(ushort)i] = 3000;
                newWorkSheet.Cells[0, i].Style = new CellStyle() { BackColor = Color.FromArgb(255, 91, 155, 213) };
            }

            var sourceArray = source.ToArray();
            for (int i = 0; i < sourceArray.Length; i++)
            {
                var item = sourceArray[i];
                for (int j = 0; j < columnHeaders.Count; j++)
                {
                    SheetColumn x = columnHeaders[j];
                    object value = x.Pi.GetValue(item, null);
                    string strval = null;

                    if (x.Pi.PropertyType == typeof(DateTime))
                    {
                        strval = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        strval = value?.ToString();
                    }

                    Cell dataCell = new Cell(strval);
                    newWorkSheet.Cells[i + 1, j] = dataCell;
                    if (x.Pi.PropertyType== typeof(DateTime))
                    {
                        newWorkSheet.Cells.ColumnWidth[(ushort)j] = 5100;
                    }
                }
            }

            newWorkBook.Worksheets.Add(newWorkSheet);
            newWorkBook.Save(fs);
        }


        private static List<PropertyInfo> getColomnPropertyInfos<T>()
        {
            List<PropertyInfo> columnPropertyInfos = new List<PropertyInfo>();
            Type t = typeof(T);
            if (t.IsClass)
            {
                System.Reflection.PropertyInfo[] properties = t.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (PropertyInfo item in properties)
                {
                    if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                    {
                        if (!columnPropertyInfos.Contains(item))
                        {
                            columnPropertyInfos.Add(item);
                        }
                    }
                }
            }
            return columnPropertyInfos;
        }


        private static List<string> getColomnHeaders<T>() => getColomnPropertyInfos<T>().Select(x => x.Name).ToList();

    }

    public static class ExcelLiteExtensions
    {
        public static byte[] ExportToExcel<T>(this IEnumerable<T> source, SheetColumn[] sheetColumns = null, string sheetName = "") where T : class
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                using (ms)
                {
                    ExcelHelper.Export(source, ms, sheetColumns, sheetName);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
