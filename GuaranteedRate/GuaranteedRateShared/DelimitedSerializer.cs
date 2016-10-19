using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace GuaranteedRate
{
    public class DelimitedSerializer<T>
    {
        private static string[] delimiter= {" , ", " | ", " " };
        public async Task<IEnumerable<T>> DeserializeAsync(TextReader textReader)
        {
            return await Task.Run(() => Deserialize(textReader));
        }
        public IEnumerable<T> Deserialize(TextReader textReader)
        {
            List<T> list = new List<T>();
            try
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                string line = null;
                do
                {
                    line = textReader.ReadLine();
                    if (line != null)
                    {
                        string[] entries;
                        if (TryParseRecord(line, properties.Length, out entries))//ignore invalid entries. May consider add log message to the log file(better approach) or throw Exception
                        {
                            T obj = (T)Activator.CreateInstance(typeof(T));
                            for (int i = 0; i < properties.Length; ++i)
                            {
                                properties[i].SetValue(obj, DeserializePrimitive(properties[i].PropertyType, entries[i]));
                            }
                            list.Add(obj);
                        }
                    }
                } while (line != null);
            }
            catch (Exception)
            {
                //log exception in the log file + consider sending email to all interested developers
                //somebody may like rethrow this exception(return it via function parameters) and handle it later + return error in HttpResponse
                //it depends. My preference is to log + send email
                return null;
            }
            return list;
        }

        private object DeserializePrimitive(Type type, string entry)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, entry);
            }
            else if (type == typeof(Color))
            {
                return Color.FromArgb((int)Convert.ChangeType(entry, typeof(int)));
            }
            else if (type == typeof(DateTime))
            {
                return DateTime.Parse(entry);
            }
            return Convert.ChangeType(entry, type);
        }
        public static bool TryParseRecord(string record, out string[] entries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            return TryParseRecord(record, properties.Length, out entries);
        }

        private static bool TryParseRecord(string record, int propertiesCount, out string[] entries)
        {
            entries = record.Split(delimiter, StringSplitOptions.None);
            return entries.Length == propertiesCount;
        }
    }
}
