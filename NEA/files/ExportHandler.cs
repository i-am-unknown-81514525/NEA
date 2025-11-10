using System.Text;
using System.IO;
using NEA.math;
using System.Collections.Generic;
using System;

namespace ui.files
{
    public class ExportException : Exception
    {
        public ExportException(string message) : base(message) { }
    }

    public class ExportIOException : ExportException
    {
        public ExportIOException(string message) : base(message) { }
    }

    public static class ExportHandler
    {
        public static string ExportToContent(SimplexInterationRunner runner)
        {
            SimplexInterationRunner exporting_runner = runner.start;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(";", exporting_runner.vars) + ";RHS");
            for (int i = 0; i < exporting_runner.expressions.GetLength(0); i++)
            {
                List<string> row = new List<string>();
                for (int j = 0; j < exporting_runner.expressions.GetLength(1); j++)
                {
                    row.Add(exporting_runner.expressions[i, j].ToString());
                }
                sb.AppendLine(string.Join(";", row));
            }
            return sb.ToString();
        }

        public static void ExportToFile(SimplexInterationRunner runner, string filename)
        {
            string content = ExportToContent(runner);
            try
            {
                File.WriteAllText(filename, content);
            }
            catch (Exception ex)
            {
                throw new ExportIOException("Failed to write to file: " + ex.Message);
            }
        }
    }
}