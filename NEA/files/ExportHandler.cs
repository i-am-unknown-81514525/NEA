using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NEA.math;

namespace NEA.files
{
    public class ExportException : Exception
    {
        public ExportException(string message) : base(message) { }
    }

    public class ExportIoException : ExportException
    {
        public ExportIoException(string message) : base(message) { }
    }

    public static class ExportHandler
    {
        public static string ExportToContent(SimplexInterationRunner runner)
        {
            SimplexInterationRunner exportingRunner = runner.Start;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(";", exportingRunner.Vars) + ";RHS");
            for (int j = 0; j < exportingRunner.Expressions.GetLength(1); j++)
            {
                List<string> row = new List<string>();
                for (int i = 0; i < exportingRunner.Expressions.GetLength(0); i++)
                {
                    row.Add(exportingRunner.Expressions[i, j].ToString());
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
                throw new ExportIoException("Failed to write to file: " + ex.Message);
            }
        }
    }
}