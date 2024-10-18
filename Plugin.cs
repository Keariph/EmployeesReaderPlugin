using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PhoneApp.Domain;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;

namespace EmployeesReaderPlugin
{
    [Author(Name = "Anna Ryhlinskaya")]
    public class Plugin : PhoneApp.Domain.Interfaces.IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Application reading the data");
            var employeesList = args.Cast<EmployeesDTO>().ToList();

            //--------------From file--------------//

            //Console.WriteLine("Write a path");
            //Console.Write("> ");
            //string path = Console.ReadLine();

            /*if (File.Exists(path))
            {
                IEnumerable<EmployeesDTO> employees = JsonConvert.DeserializeObject<IEnumerable<EmployeesDTO>>(File.ReadAllText(path));
                employeesList.AddRange(employees);
                logger.Info($"The {employees.Count()} employees were add");
            }
            else
            {
                logger.Error($"The file on the path {path} is not exist");
            }*/

            //----------------From Api-----------------//

            string responce = getEmployees().Result;

            if (String.IsNullOrEmpty(responce))
            {
                logger.Error($"Failed to upload data");
            }
            else
            {
                DummyListJson dummyListJson = JsonConvert.DeserializeObject<DummyListJson>(responce);
                foreach( DummyUserJsonDto userJsonDto in dummyListJson.Users)
                {
                    string name = userJsonDto.FirstName + " " + userJsonDto.LastName;
                    EmployeesDTO employeeDto = new EmployeesDTO();
                    employeeDto.Name = name;
                    employeeDto.AddPhone(userJsonDto.Phone);
                    employeesList.Add(employeeDto);

                }
                logger.Info($"The {dummyListJson.Users.Count()} employees were add");
            }

            return employeesList.Cast<DataTransferObject>();
        }

        private async Task<string> getEmployees()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            var responce = await httpClient.GetAsync("https://dummyjson.com/users");

            if (responce.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return responce.Content.ReadAsStringAsync().Result.ToString();
            }

            return null;
        }
    }
}
