using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Q.Examples
{
    class Program
    {
        private static HttpClient client = new HttpClient();
        private static string _token = "access_token";

        static void Main(string[] args)
        {
            // configure some default settings
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_token}");

            PrintNote("==> First of all => Get users of your team");
            var users = GetEntities<QUsers>($"https://api.myHQ.io/api/v1/users");

            PrintNote("==> Next step, filter users");
            var allJamies = GetEntities<QUsers>($"https://api.myHQ.io/api/v1/users?filterby=FirstName eq 'Jimmie'");

            PrintNote("==> Next step, filter users which have birthday in february");
            var  birthdays = GetEntities<QUsers>($"https://api.myHQ.io/api/v1/users?filterby=BirthDate le datetime'{new DateTime(2018,2,1).ToString("s")}' and BirthDate ge datetime'{new DateTime(2018, 3, 1).AddDays(-1).ToString("s")}'");

            PrintNote("#########################################");
            PrintNote("Start with a more complex example");
            PrintNote("==> Create your first project.");

            PrintNote("==> Check if the project status exist, else create a new.");
            var projectStatus = GetEntities<DefaultResponseObject>("https://api.myHQ.io/api/v1/projectstatuses?filterby=Name eq 'KickOff'").FirstOrDefault();
            if (projectStatus == null)
            {
                var projectStatusPostModel = @"{
                ""name"": ""KickOff"",
                ""type"": ""planning""
                }";

                projectStatus = Create<DefaultResponseObject>("https://api.myHQ.io/api/v1/projectstatuses", projectStatusPostModel);
            }

            PrintNote("==> Check if the project type exist, else create a new.");
            var projectType = GetEntities<DefaultResponseObject>("https://api.myHQ.io/api/v1/projecttypes?filterby=Name eq 'Mission'").FirstOrDefault();
            if (projectType == null)
            {
                string projectTypePostModel = @"{
                ""isSalesType"": false,
                ""name"": ""Mission"",
                ""description"": ""Project type for special missions."",
                ""icon"": ""star""
                }";

                projectType = Create<DefaultResponseObject>("https://api.myHQ.io/api/v1/projecttypes", projectTypePostModel);

            }

            PrintNote("==> Check if status is linked to type, else link.");
            var link = GetEntities<DefaultResponseObject>($"https://api.myHQ.io/api/v1/projecttypes/{projectType.Id.ToString()}/projectstatuses").FirstOrDefault(m => m.Id == projectType.Id);
            if (link == null)
            {
                string linkPostModel = $@"{{
                ""projectStatusId"": ""{projectStatus.Id.ToString()}"",
                ""order"": 1
                }}";

                link = Create<DefaultResponseObject>($"https://api.myHQ.io/api/v1/projecttypes/{projectType.Id.ToString()}/linkprojectstatus", linkPostModel);
            }

            PrintNote("==> Now create the new project.");
            string internalProjectPostModel = $@"{{
                ""name"": ""Space-Project"",
                ""description"": ""Top Secret Mission"",
                ""startDate"": ""2018-03-01T16:16:32.0450375Z"",
                ""dueDate"": ""2018-03-15T16:16:32.0450377Z"",
                ""projectTypeId"": ""{projectType.Id.ToString()}"",
                ""timeBudget"": 5,
                ""projectStatusId"": ""{projectStatus.Id.ToString()}""
            }}";

            DefaultResponseObject newProject = Create<DefaultResponseObject>("https://api.myHQ.io/api/v1/projects", internalProjectPostModel);
            PrintNote($"Awesome😎 just create the new project {newProject}.");


            PrintNote($"==> Time to leaf a comment on your new project");
            string commentPostModel = @"{
                ""message"": ""🚀 Start with my mission 👽""               
            }";

            DefaultResponseObject newComment = Create<DefaultResponseObject>($"https://api.myHQ.io/api/v1/projects/{newProject.Id.ToString()}/comments", commentPostModel);

            PrintNote("Press enter to close");
            var x = Console.ReadLine();
        }


        #region helper        

        /// <summary>
        /// Show the message in the console in another color.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public static void PrintNote(string message)
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message); 
            Console.ResetColor();
        }

        /// <summary>
        /// Returns a list of objects from type T.
        /// </summary>
        /// <param name="url">The url to fetch the data.</param>
        /// <returns></returns>
        public static List<T> GetEntities<T>(string url)
        {
            HttpResponseMessage apiResponse = client.GetAsync(url).GetAwaiter().GetResult();
            var result = apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var entities = JsonConvert.DeserializeObject<List<T>>(result);
            entities.ForEach(u => Console.WriteLine(u));
            return entities;
        }

        /// <summary>
        /// Returns an objects from type T.
        /// </summary>
        /// <param name="url">The url to fetch the data.</param>
        /// <returns></returns>
        public static T GetEntity<T>(string url)
        {
            HttpResponseMessage apiResponse = client.GetAsync(url).GetAwaiter().GetResult();
            var result = apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var entity = JsonConvert.DeserializeObject<T>(result);
            if (entity == null)
                Console.WriteLine(result);
            else
                Console.WriteLine(entity);
            return entity;
        }

        /// <summary>
        /// Create a new objects from type T.
        /// </summary>
        /// <param name="url">The url to send the data.</param>
        /// <param name="formJson">The data as json string.</param>
        /// <returns></returns>
        public static T Create<T>(string url, string formJson)
        {
            HttpResponseMessage apiResponse = client
                .PostAsync(url, new StringContent(formJson, Encoding.UTF8, "application/json"))
                .GetAwaiter()
                .GetResult();

            var result = apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var createdEntity = JsonConvert.DeserializeObject<T>(result);
            Console.WriteLine(createdEntity);
            return createdEntity;
        }

        #endregion      
    }
}
