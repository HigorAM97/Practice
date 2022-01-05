using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProconGAPE
{

    class Program
    {

        static void Main(string[] args)
        {
            var StartProgram = new ExportFile();
            StartProgram.FormatFiles();
        }
    }

    public class Procon_Api
    {
        public GetParameters getParameters = new GetParameters();
        public GetLoginResponse getLoginResponse = new GetLoginResponse();
        public GetAllPhonesToExportResponse getAllPhonesToExportResponse = new GetAllPhonesToExportResponse();
        public List<string> Telefone = new List<string>();
        public List<string> dataCadastramento = new List<string>();
        public List<string> evento = new List<string>();
        public List<string> aPartir = new List<string>();
        public void ReadParameters()
        {
            var readParameters = new StreamReader("Parameters.json");
            getParameters = JsonConvert.DeserializeObject<GetParameters>(readParameters.ReadToEnd());
        }

        public void PostLogin()

        {
            ReadParameters();

            var client = new RestClient("https://api.bloqueio.procon.sp.gov.br/v1/User/Login");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var body = JsonConvert.SerializeObject(getParameters.AuthenticationParameters);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            getLoginResponse = JsonConvert.DeserializeObject<GetLoginResponse>(response.Content);
            Console.WriteLine(response.Content);

            using (StreamWriter createFiles = File.CreateText(@"ApiResponse_Logs\Login_log_" + DateTime.Now.ToString().Replace(@"/", "").Replace(@":", "") + @".txt"))
            {

                createFiles.Write(response.Content);
            }

        }
        public void PostGetAllPhonesToExport()
        {
            PostLogin();
            var client = new RestClient("https://api.bloqueio.procon.sp.gov.br/v1/UserPhone/GetAllPhonesToExport");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + getLoginResponse.data.token);
            request.AddHeader("Cookie", "ROUTEID=.1");
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR");
            ApiParameters setApiParameters = new ApiParameters();
            ApiParameters_WithDateEvent setApiParameters_WithDateEvent = new ApiParameters_WithDateEvent();
            setApiParameters.DateEvent = new DateTime(getParameters.QueryPhonesParameters.DateEventYear, getParameters.QueryPhonesParameters.DateEventMonth, getParameters.QueryPhonesParameters.DateEventDay - getParameters.QueryPhonesParameters.How_Many_Days_Before_DateActivatedAtEnd);
            setApiParameters.DateEventEnd = new DateTime(getParameters.QueryPhonesParameters.DateEventEndYear, getParameters.QueryPhonesParameters.DateEventEndMonth, getParameters.QueryPhonesParameters.DateEventEndDay);
            if (getParameters.QueryPhonesParameters.DateEventYear == 9999 && getParameters.QueryPhonesParameters.DateEventMonth == 12 && getParameters.QueryPhonesParameters.DateEventDay == 31)
                setApiParameters.DateEvent = DateTime.Now.AddDays(-getParameters.QueryPhonesParameters.How_Many_Days_Before_DateEventEnd);
            if (getParameters.QueryPhonesParameters.DateEventEndYear == 9999 && getParameters.QueryPhonesParameters.DateEventEndMonth == 12 && getParameters.QueryPhonesParameters.DateEventEndDay == 31)
                setApiParameters.DateEventEnd = DateTime.Now;
            string body;
            if (getParameters.QueryPhonesParameters.ActivatedAtYear != 9999 && getParameters.QueryPhonesParameters.ActivatedAtMonth != 12 && getParameters.QueryPhonesParameters.ActivatedAtDay != 31)
            {
                setApiParameters_WithDateEvent.DateActivatedAt = new DateTime(getParameters.QueryPhonesParameters.ActivatedAtYear, getParameters.QueryPhonesParameters.ActivatedAtMonth, getParameters.QueryPhonesParameters.ActivatedAtDay - getParameters.QueryPhonesParameters.How_Many_Days_Before_DateActivatedAtEnd);
                setApiParameters_WithDateEvent.DateActivatedAtEnd = new DateTime(getParameters.QueryPhonesParameters.ActivatedAtEndYear, getParameters.QueryPhonesParameters.ActivatedAtEndMonth, getParameters.QueryPhonesParameters.ActivatedAtEndDay);
                body = JsonConvert.SerializeObject(setApiParameters_WithDateEvent);
            }
            else
            {
                body = JsonConvert.SerializeObject(setApiParameters);
            }
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            using (StreamWriter createFiles = File.CreateText(@"ApiResponse_Logs\ExportData_log_" + DateTime.Now.ToString().Replace(@"/", "").Replace(@":", "") + @".txt"))
            {

                createFiles.Write(response.Content);
            }

            getAllPhonesToExportResponse = JsonConvert.DeserializeObject<GetAllPhonesToExportResponse>(response.Content);
            if (getParameters.QueryPhonesParameters.Event == "Bloqueado") getAllPhonesToExportResponse.data.RemoveAll(remove => remove.evento == "Desbloqueado");
            if (getParameters.QueryPhonesParameters.Event == "Desbloqueado") getAllPhonesToExportResponse.data.RemoveAll(remove => remove.evento == "Bloqueado");
            for (int proconCount = 0; proconCount < getAllPhonesToExportResponse.data.Count - 1; proconCount++) { 
                Telefone.Add(getAllPhonesToExportResponse.data[proconCount].telefone);
                dataCadastramento.Add(getAllPhonesToExportResponse.data[proconCount].dataCadastramento);
                evento.Add(getAllPhonesToExportResponse.data[proconCount].evento);
                aPartir.Add(getAllPhonesToExportResponse.data[proconCount].aPartir);
            }
        }
    }
    public class AuthenticationParameters
    {
        public string Name
        {
            get;
            set;
        }
        public string Password
        {
            get;
            set;
        }
        public string Type
        {
            get;
            set;
        }
        public string typeDoc
        {
            get;
            set;
        }
    }
    public class QueryPhonesParameters
    {
        public string Event
        {
            get;
            set;
        }
        public int ActivatedAtYear
        {
            get;
            set;
        }
        public int ActivatedAtMonth
        {
            get;
            set;
        }
        public int ActivatedAtDay
        {
            get;
            set;
        }
        public int ActivatedAtEndYear
        {
            get;
            set;
        }
        public int ActivatedAtEndMonth
        {
            get;
            set;
        }
        public int ActivatedAtEndDay
        {
            get;
            set;
        }
        public int How_Many_Days_Before_DateActivatedAtEnd
        {
            get;
            set;
        }
        public int DateEventYear
        {
            get;
            set;
        }
        public int DateEventMonth
        {
            get;
            set;
        }
        public int DateEventDay
        {
            get;
            set;
        }
        public int DateEventEndYear
        {
            get;
            set;
        }
        public int DateEventEndMonth
        {
            get;
            set;
        }
        public int DateEventEndDay
        {
            get;
            set;
        }
        public int How_Many_Days_Before_DateEventEnd
        {
            get;
            set;
        }
    }
    public class GetParameters
    {
        public AuthenticationParameters AuthenticationParameters
        {
            get;
            set;
        }
        public QueryPhonesParameters QueryPhonesParameters
        {
            get;
            set;
        }
    }
    public class data
    {
        public string telefone
        {
            get;
            set;
        }
        public string dataCadastramento
        {
            get;
            set;
        }
        public string evento
        {
            get;
            set;
        }
        public string aPartir
        {
            get;
            set;
        }
    }
    public class GetAllPhonesToExportResponse
    {
        [JsonProperty("sucess")]
        public string success
        {
            get;
            set;
        }
        public string message
        {
            get;
            set;
        }
        public string innerException
        {
            get;
            set;
        }
        public string statusCode
        {
            get;
            set;
        }
        public List<data> data
        {
            get;
            set;
        }
    }
    public class userRoles { }
    public class userPhones { }
    public class userLegalRepres { }
    public class userNotes { }
    public class User
    {
        public int id
        {
            get;
            set;
        }
        public string name
        {
            get;
            set;
        }
        public string nickname
        {
            get;
            set;
        }
        public object cpf
        {
            get;
            set;
        }
        public string cnpj
        {
            get;
            set;
        }
        public object rg
        {
            get;
            set;
        }
        public string email
        {
            get;
            set;
        }
        public object password
        {
            get;
            set;
        }
        public string cep
        {
            get;
            set;
        }
        public string address
        {
            get;
            set;
        }
        public int addressNumber
        {
            get;
            set;
        }
        public string addressComplement
        {
            get;
            set;
        }
        public string city
        {
            get;
            set;
        }
        public string state
        {
            get;
            set;
        }
        public string neighborhood
        {
            get;
            set;
        }
        public string type
        {
            get;
            set;
        }
        public List<object> userRoles
        {
            get;
            set;
        }
        public bool activated
        {
            get;
            set;
        }
        public List<object> userPhones
        {
            get;
            set;
        }
        public string key
        {
            get;
            set;
        }
        public List<object> userLegalRepres
        {
            get;
            set;
        }
        public List<object> userNotes
        {
            get;
            set;
        }
        public string phonesString
        {
            get;
            set;
        }
        public object providerCompaniesString
        {
            get;
            set;
        }
        public DateTime createdAt
        {
            get;
            set;
        }
        public DateTime updatedAt
        {
            get;
            set;
        }
        public object canal
        {
            get;
            set;
        }
        public object id_sourcerequester_criacao
        {
            get;
            set;
        }
        public object id_sourcerequester_alteracao
        {
            get;
            set;
        }
    }
    public class Data
    {
        public string token
        {
            get;
            set;
        }
        public User user
        {
            get;
            set;
        }
    }
    public class GetLoginResponse
    {
        public bool sucess
        {
            get;
            set;
        }
        public string message
        {
            get;
            set;
        }
        public object innerException
        {
            get;
            set;
        }
        public int statusCode
        {
            get;
            set;
        }
        public Data data
        {
            get;
            set;
        }
    }
    public class ApiParameters
    {
        // public string PhoneNumber { get; set; }
        // public string Event { get; set; }
        // public DateTime DateActivatedAt { get; set; }
        // public DateTime DateActivatedAtEnd { get; set; }
        public DateTime DateEvent
        {
            get;
            set;
        }
        public DateTime DateEventEnd
        {
            get;
            set;
        }
    }
    public class ApiParameters_WithDateEvent
    {
        // public string PhoneNumber { get; set; }
        // public string Event { get; set; }
        public DateTime DateActivatedAt
        {
            get;
            set;
        }
        public DateTime DateActivatedAtEnd
        {
            get;
            set;
        }
        public DateTime DateEvent
        {
            get;
            set;
        }
        public DateTime DateEventEnd
        {
            get;
            set;
        }
    }


    public class ExportFile
    {
        GetFilesToExport getFiles = new GetFilesToExport();
        public void FormatFiles()

        {


            var readFile = new StreamReader("setFilesToExport.json");

            getFiles = JsonConvert.DeserializeObject<GetFilesToExport>(readFile.ReadToEnd());
            var setProconPhones = new Procon_Api();
            setProconPhones.PostGetAllPhonesToExport();

     
            List<string> ddd_Telefone = new List<string>();
            List<string> dataCadastramento = new List<string>();
            List<string> evento = new List<string>();
            List<string> aPartir = new List<string>();

            List<string> returnNull = new List<string>();

            List<string> ddd = new List<string>();
            List<string> Telefone = new List<string>();
            List<string> DateTime_Today = new List<string>();
            List<string> DateTime_Now = new List<string>();
            ddd_Telefone = setProconPhones.Telefone;
            dataCadastramento = setProconPhones.dataCadastramento;
            evento = setProconPhones.evento;
            aPartir = setProconPhones.aPartir;
            for(int phone =0;phone <= setProconPhones.Telefone.Count; phone++)
            {
                returnNull.Add(string.Empty);
            }

            foreach (string phone in ddd_Telefone)
                Telefone.Add(phone.Substring(3));
            foreach (string phone in ddd_Telefone)
                ddd.Add(phone.Substring(0, 2));

            foreach (string phone in ddd_Telefone)
                DateTime_Today.Add(DateTime.Today.ToString().Substring(0, 10));

            foreach (string phone in ddd_Telefone)
                DateTime_Now.Add(DateTime.Now.ToString());

            List<string> fileLine_1 = new List<string>();
            List<string> fileLine_2 = new List<string>();
            List<string> fileLine_3 = new List<string>();
            List<string> fileLine_4 = new List<string>();
            List<string> fileLine_5 = new List<string>();
            List<string> fileLine_6 = new List<string>();
            List<string> fileLine_7 = new List<string>();
            List<string> fileLine_8 = new List<string>();
            List<string> fileLine_9 = new List<string>();
            List<string> fileLine_10 = new List<string>();
            List<string> fileLine_11 = new List<string>();
            List<string> fileLine_12 = new List<string>();

            foreach (fileList file in getFiles.fileList)
            {
                List<string> getItem_1 = new List<string>();
                List<string> getItem_2 = new List<string>();
                List<string> getItem_3 = new List<string>();
                List<string> getItem_4 = new List<string>();
                List<string> getItem_5 = new List<string>();
                List<string> getItem_6 = new List<string>();
                List<string> getItem_7 = new List<string>();
                List<string> getItem_8 = new List<string>();
                List<string> getItem_9 = new List<string>();
                List<string> getItem_10 = new List<string>();
                List<string> getItem_11 = new List<string>();
                List<string> getItem_12 = new List<string>();

                for (int TelCount = 0; TelCount <= ddd_Telefone.Count; TelCount++)
                {
                        getItem_1.Add(file.item1);
 
                        getItem_2.Add(file.item2);

                        getItem_3.Add(file.item3);

                        getItem_4.Add(file.item4);

                        getItem_5.Add(file.item5);

                        getItem_6.Add(file.item6);

                        getItem_7.Add(file.item7);

                        getItem_8.Add(file.item8);

                        getItem_9.Add(file.item9);

                        getItem_10.Add(file.item10);

                        getItem_11.Add(file.item11);

                        getItem_12.Add(file.item12);
                }

                switch (file.item1)
                {

                    case "_tel":
                        fileLine_1 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_1 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_1 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_1 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_1 = evento;
                        break;
                    case "_apartir":
                        fileLine_1 = aPartir;
                        break;
                    case "_date":
                        fileLine_1 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_1 = DateTime_Now;
                        break;
                    case "":
                        fileLine_1 = returnNull;
                        break;
                    default:
                        fileLine_1 = getItem_1;
                        break;

                }
                switch (file.item2)
                {

                    case "_tel":
                        fileLine_2 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_2 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_2 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_2 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_2 = evento;
                        break;
                    case "_apartir":
                        fileLine_2 = aPartir;
                        break;
                    case "_date":
                        fileLine_2 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_2 = DateTime_Now;
                        break;
                    case "":
                        fileLine_2 = returnNull;
                        break;
                    default:
                        fileLine_2 = getItem_2;
                        break;
                }
                switch (file.item3)
                {

                    case "_tel":
                        fileLine_3 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_3 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_3 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_3 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_3 = evento;
                        break;
                    case "_apartir":
                        fileLine_3 = aPartir;
                        break;
                    case "_date":
                        fileLine_3 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_3 = DateTime_Now;
                        break;
                    case "":
                        fileLine_3 = returnNull;
                        break;
                    default:
                        fileLine_3 = getItem_3;
                        break;
                }
                switch (file.item4)
                {
                    case "_tel":
                        fileLine_4 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_4 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_4 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_4 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_4 = evento;
                        break;
                    case "_apartir":
                        fileLine_4 = aPartir;
                        break;
                    case "_date":
                        fileLine_4 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_4 = DateTime_Now;
                        break;
                    case "":
                        fileLine_4 = returnNull;
                        break;
                    default:
                        fileLine_4 = getItem_4;
                        break;
                }
                switch (file.item5)
                {
                    case "_tel":
                        fileLine_5 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_5 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_5 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_5 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_5 = evento;
                        break;
                    case "_apartir":
                        fileLine_5 = aPartir;
                        break;
                    case "_date":
                        fileLine_5 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_5 = DateTime_Now;
                        break;
                    case "":
                        fileLine_5 = returnNull;
                        break;
                    default:
                        fileLine_5 = getItem_5;
                        break;
                }
                switch (file.item6)
                {

                    case "_tel":
                        fileLine_6 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_6 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_6 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_6 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_6 = evento;
                        break;
                    case "_apartir":
                        fileLine_6 = aPartir;
                        break;
                    case "_date":
                        fileLine_6 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_6 = DateTime_Now;
                        break;
                    case "":
                        fileLine_6 = returnNull;
                        break;
                    default:
                        fileLine_6 = getItem_6;
                        break;
                }
                switch (file.item7)
                {
                    case "_tel":
                        fileLine_7 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_7 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_7 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_7 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_7 = evento;
                        break;
                    case "_apartir":
                        fileLine_7 = aPartir;
                        break;
                    case "_date":
                        fileLine_7 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_7 = DateTime_Now;
                        break;
                    case "":
                        fileLine_7 = returnNull;
                        break;
                    default:
                        fileLine_7 = getItem_7;
                        break;
                }
                switch (file.item8)
                {
                    case "_tel":
                        fileLine_8 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_8 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_8 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_8 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_8 = evento;
                        break;
                    case "_apartir":
                        fileLine_8 = aPartir;
                        break;
                    case "_date":
                        fileLine_8 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_8 = DateTime_Now;
                        break;
                    case "":
                        fileLine_8 = returnNull;
                        break;
                    default:
                        fileLine_8 = getItem_8;
                        break;
                }
                switch (file.item9)
                {
                    case "_tel":
                        fileLine_9 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_9 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_9 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_9 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_9 = evento;
                        break;
                    case "_apartir":
                        fileLine_9 = aPartir;
                        break;
                    case "_date":
                        fileLine_9 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_9 = DateTime_Now;
                        break;
                    case "":
                        fileLine_9 = returnNull;
                        break;
                    default:
                        fileLine_9 = getItem_9;
                        break;
                }
                switch (file.item10)
                {

                    case "_tel":
                        fileLine_10 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_10 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_10 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_10 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_10 = evento;
                        break;
                    case "_apartir":
                        fileLine_10 = aPartir;
                        break;
                    case "_date":
                        fileLine_10 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_10 = DateTime_Now;
                        break;
                    case "":
                        fileLine_10 = returnNull;
                        break;
                    default:
                        fileLine_10 = getItem_10;
                        break;
                }
                switch (file.item11)
                {
                    case "_tel":
                        fileLine_11 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_11 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_11 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_11 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_11 = evento;
                        break;
                    case "_apartir":
                        fileLine_11 = aPartir;
                        break;
                    case "_date":
                        fileLine_11 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_11 = DateTime_Now;
                        break;
                    case "":
                        fileLine_11 = returnNull;
                        break;
                    default:
                        fileLine_11 = getItem_11;
                        break;
                }
                switch (file.item12)
                {
                    case "_tel":
                        fileLine_12 = Telefone;
                        break;
                    case "_ddd":
                        fileLine_12 = ddd;
                        break;
                    case "_dddtel":
                        fileLine_12 = ddd_Telefone;
                        break;
                    case "_datacadastramento":
                        fileLine_12 = dataCadastramento;
                        break;
                    case "_evento":
                        fileLine_12 = evento;
                        break;
                    case "_apartir":
                        fileLine_12 = aPartir;
                        break;
                    case "_date":
                        fileLine_12 = DateTime_Today;
                        break;
                    case "_datetime":
                        fileLine_12 = DateTime_Now;
                        break;
                    case "":
                        fileLine_12 = returnNull;
                        break;
                    default:
                        fileLine_12 = getItem_12;
                        break;
                }

                if (file.HeadBoard != "")
                {
                    file.HeadBoard += "\r\n";
                }
                for (int TelCount = 0; TelCount < ddd_Telefone.Count; TelCount++)
                {

                    file.HeadBoard += fileLine_1[TelCount] + fileLine_2[TelCount] + fileLine_3[TelCount] + fileLine_4[TelCount] + fileLine_5[TelCount] + fileLine_6[TelCount] + fileLine_7[TelCount] + fileLine_8[TelCount] + fileLine_9[TelCount] + fileLine_10[TelCount] + fileLine_11[TelCount] + fileLine_12[TelCount] + "\r\n";

                }
                using (StreamWriter createFiles = File.CreateText(file.WhereToSave.Replace(@"//", @"\") + file.FileName + DateTime.Today.ToString().Substring(0, 10).Replace(@"/", "") + @".csv"))
                {

                    createFiles.Write(file.HeadBoard);
                }
            }
        }
        public class fileList
        {
            public string FileName
            {
                get;
                set;
            }
            public string HeadBoard
            {
                get;
                set;
            }
            public string item1
            {
                get;
                set;
            }
            public string item2
            {
                get;
                set;
            }
            public string item3
            {
                get;
                set;
            }


            public string item4
            {
                get;
                set;
            }

            public string item5
            {
                get;
                set;
            }

            public string item6
            {
                get;
                set;
            }

            public string item7
            {
                get;
                set;
            }

            public string item8
            {
                get;
                set;
            }

            public string item9
            {
                get;
                set;
            }

            public string item10
            {
                get;
                set;
            }

            public string item11
            {
                get;
                set;
            }

            public string item12
            {
                get;
                set;
            }
            public string WhereToSave
            {
                get;
                set;
            }
        }
        public class GetFilesToExport
        {
            public List<fileList> fileList
            {
                get;
                set;
            }
        }
    }

}
