using Microsoft.VisualBasic.FileIO;

namespace WinFormsApp1.Data
{
    public class CsvImporter
    {
        public static async Task<(int successCount, int errorCount)> ImportCsvToDatabaseAsync(string filePath, AppDbContext context, IProgress<int> progress)
        {
            int successCount = 0;
            int errorCount = 0;
            string[] allLines = File.ReadAllLines(filePath);
            int totalLines = allLines.Length;
            int processedLines = 0;
            var peopleBatch = new List<Person>();

            bool hasHeader = false;
            if (totalLines > 0)
            {
                string firstLine = allLines[0];
                if (firstLine.Contains("Date") || firstLine.Contains("date"))
                {
                    hasHeader = true;
                }
            }

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.Delimiters = new string[] { ";" };
                parser.HasFieldsEnclosedInQuotes = true;
                parser.TrimWhiteSpace = true;

                if (hasHeader && !parser.EndOfData)
                {
                    parser.ReadFields();
                    processedLines = 1;
                }

                while (!parser.EndOfData)
                {
                    try
                    {
                        string[] fields = parser.ReadFields();
                        processedLines++;

                        if (fields == null || fields.Length < 6)
                        {
                            errorCount++;
                            continue;
                        }

                        DateTime parsedDate;
                        if (!DateTime.TryParse(fields[0].Trim(), out parsedDate))
                        {
                            errorCount++;
                            continue;
                        }

                        var person = new Person
                        {
                            Date = parsedDate.Date,
                            FirstName = fields[1].Trim(),
                            LastName = fields[2].Trim(),
                            SurName = fields.Length > 3 ? fields[3].Trim() : "",
                            City = fields.Length > 4 ? fields[4].Trim() : "",
                            Country = fields.Length > 5 ? fields[5].Trim() : ""
                        };

                        if (string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
                        {
                            errorCount++;
                            continue;
                        }

                        peopleBatch.Add(person);
                        successCount++;

                        if (peopleBatch.Count >= 1000)
                        {
                            await context.People.AddRangeAsync(peopleBatch);
                            await context.SaveChangesAsync();
                            peopleBatch.Clear();
                        }

                        if (progress != null && totalLines > 0)
                        {
                            int percent = processedLines * 100 / totalLines;
                            progress.Report(percent);
                        }
                    }
                    catch
                    {
                        errorCount++;
                    }
                }
            }

            if (peopleBatch.Count > 0)
            {
                await context.People.AddRangeAsync(peopleBatch);
                await context.SaveChangesAsync();
            }

            return (successCount, errorCount);
        }
    }
}