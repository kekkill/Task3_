using System.Text;
using System.Xml;

namespace WinFormsApp1.Data
{
    public class ExportService
    {
        public async Task ExportToExcelAsync(IQueryable<Person> query, string filePath)
        {
            await Task.Run(() =>
            {
                var people = query.ToList();

                if (people.Count == 0)
                {
                    throw new Exception("Нет данных для экспорта по выбранным фильтрам");
                }

                var csvContent = new StringBuilder();
                csvContent.Append('\uFEFF');
                csvContent.AppendLine("Date;First Name;Last Name;Surname;City;Country");

                foreach (var person in people)
                {
                    string line = $"{FormatDate(person.Date)};" +
                                  $"{EscapeField(person.FirstName)};" +
                                  $"{EscapeField(person.LastName)};" +
                                  $"{EscapeField(person.SurName)};" +
                                  $"{EscapeField(person.City)};" +
                                  $"{EscapeField(person.Country)}";
                    csvContent.AppendLine(line);
                }

                File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
            });
        }

        public async Task ExportToXmlAsync(IQueryable<Person> query, string filePath)
        {
            await Task.Run(() =>
            {
                var people = query.ToList();

                if (people.Count == 0)
                {
                    throw new Exception("Нет данных для экспорта по выбранным фильтрам");
                }

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8,
                    IndentChars = "  "
                };

                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("TestProgram");

                    int id = 1;
                    foreach (var person in people)
                    {
                        writer.WriteStartElement("Record");
                        writer.WriteAttributeString("id", id.ToString());

                        writer.WriteElementString("Date", person.Date.ToString("yyyy-MM-dd"));
                        writer.WriteElementString("FirstName", person.FirstName);
                        writer.WriteElementString("LastName", person.LastName);
                        writer.WriteElementString("SurName", person.SurName);
                        writer.WriteElementString("City", person.City);
                        writer.WriteElementString("Country", person.Country);

                        writer.WriteEndElement();
                        id++;
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            });
        }

        private string FormatDate(DateTime date)
        {
            return date.ToString("dd.MM.yyyy");
        }

        private string EscapeField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return "";
            }

            if (field.Contains(";") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }
    }
}