using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace GuaranteedRate
{
    class Program
    {
        static HttpClient client;
        static void Main(string[] args)
        {
            DoFirstPartOfTheAssignment();
            DoSecondPartOfTheAssignment().Wait();
        }

        private static void DoFirstPartOfTheAssignment()
        {
            Console.WriteLine("First part of the assignment\n");
            IEnumerable<Person> list;
            DelimitedSerializer<Person> serializer = new DelimitedSerializer<Person>();
            using (TextReader textReader = new StreamReader("TestFile.txt"))
            {
                list = serializer.Deserialize(textReader);
            }
            PrintList(list.OrderByDescending(p => p.Gender).ThenBy(p => p.LastName),
                "\nPersons list sorted by gender(female before male) then Last Name(Asc)");
            PrintList(list.OrderBy(p => p.DateOfBirth), "\nPersons list sorted by birth date(Asc)");
            PrintList(list.OrderByDescending(p => p.LastName),
                "\nPersons list sorted by Last Name(Desc)\n");
        }

        private async static Task DoSecondPartOfTheAssignment()
        {
            var content = new StringContent("=Created FromCode Male 100 11/11/2011", Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:1494/");

                Console.WriteLine("\nSecond part of the assignment: Testing WebApi");
                var postTask = client.PostAsync(new Uri("api/records", UriKind.Relative), content);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var genderTask = GetRecordsAsync("gender");
                var birthDayTask = GetRecordsAsync("birthdate");
                var nameTask = GetRecordsAsync("name");
                await Task.WhenAll(postTask, genderTask, birthDayTask, nameTask);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                }
                content.Dispose();
            }
        }

        static async Task GetRecordsAsync(string recordType)
        {
            try
            {
                string uri = string.Format("api/records/{0}", recordType);
                using (HttpResponseMessage response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        var records = serializer.Deserialize<IEnumerable<Person>>(json);
                        PrintList(records, string.Format("\nResults from '{0}' WebApi call:\n", recordType));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Error occured retrieving {0}", recordType));
                    }
                }
            }
            catch(Exception ex)
            {
                //log and send email to developers
                Console.Write(ex.Message);
            }
        }

        static async Task<Person> GetPersonAsync(string path)
        {
            Person person = null;
            using (HttpResponseMessage response = await client.GetAsync("api/persons/GetAllPersons"))
            {
                if (response.IsSuccessStatusCode)
                {
                    person = await response.Content.ReadAsAsync<Person>();
                }
            }
            return person;
        }

        private static void PrintList(IEnumerable list, string header)
        {
            StringBuilder stringBuilder = new StringBuilder(header);
            foreach (var entry in list)
            {
                stringBuilder.AppendLine(entry.ToString());
            }
            Console.Write(stringBuilder.ToString());
        }
    }
}
