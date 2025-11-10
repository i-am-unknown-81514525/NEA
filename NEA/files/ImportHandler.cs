using System;
using System.Text;
using System.IO;
using NEA.math;
using System.Linq;
using System.Collections.Generic;
using ui.math;

namespace NEA.files
{
    public class ImportException : Exception
    {
        public ImportException(string message) : base(message) { }
    }

    public class ImportFormatException : ImportException
    {
        public ImportFormatException(string message) : base(message) { }
    }

    public static class ImportHandler
    {
        public static SimplexInterationRunner ImportWithContent(string content)
        {
            string[] splitted = content.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0).ToArray();
            if (splitted.Length < 2)
            {
                throw new ImportException("Invalid content: Not enough lines.");
            }
            string[] row0 = splitted[0].Split(';').Where(s => s.Length > 0).ToArray();
            if (row0.Length < 2)
            {
                throw new ImportFormatException("Invalid format: First line must contain at least two entries separated by semicolons.");
            }
            if (row0[0] == "A" && row0.Length < 3)
            {
                throw new ImportFormatException("Invalid format: First 2 variable A and P and one final variable for RHS.");
            }
            if (row0[0] == "A" && row0[1] != "P")
            {
                throw new ImportFormatException("Invalid format: Second variable name must be 'P' if the first is 'A'.");
            }
            if (row0[0] != "A" && row0[0] != "P")
            {
                throw new ImportFormatException("Invalid format: First variable name must be 'A' or 'P'.");
            }
            if (row0[row0.Length - 1] != "RHS")
            {
                throw new ImportFormatException("Invalid format: Last variable name must be 'RHS'.");
            }
            bool is_two_stage = row0[0] == "A";
            if (row0.Length != new HashSet<string>(row0).Count)
            {
                throw new ImportFormatException("Invalid format: Variable names must be unique.");
            }
            string[] vars = row0.Take(row0.Length - 1).ToArray(); // remove RHS
            Fraction[,] expressions = new Fraction[splitted.Length - 1, row0.Length];
            for (int i = 1; i < splitted.Length; i++)
            {
                string[] row = splitted[i].Split(';').Where(s => s.Length > 0).ToArray();
                if (row.Length != row0.Length)
                {
                    throw new ImportFormatException($"Invalid format: Line {i + 1} does not have the correct number of entries.");
                }
                for (int j = 0; j < row.Length; j++)
                {
                    if (!Fraction.TryParse(row[j], out Fraction value))
                    {
                        throw new ImportFormatException($"Invalid format: Entry '{row[j]}' in line {i + 1}, column {j + 1} is not a valid fraction.");
                    }
                    expressions[i - 1, j] = value;
                }
            }
            return new SimplexInterationRunner(
                is_two_stage ? SimplexStage.TWO_STAGE_MAX : SimplexStage.ONE_STAGE,
                is_two_stage ? SimplexMode.MIN : SimplexMode.MAX,
                expressions,
                vars
            );
        }

        public static SimplexInterationRunner ImportFromFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new ImportException($"File not found: {filepath}");
            }
            string content = File.ReadAllText(filepath);
            return ImportWithContent(content);
        }
    }
}