using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.Services.ServiceProxy;

namespace TestConsole
{
    class ConnectionHelper
    {
        /// <summary>
        /// Credentials read from a file
        /// </summary>
        private readonly string _baseUrl;
        private readonly string _user;
        private readonly string _password;

        private readonly Constants.Enums.AuthType _authType;

        public string BaseRelativityUrl
        {
            get
            {
                string retVal = null;
                if (!String.IsNullOrEmpty(_baseUrl))
                {
                    // check if URL ends with a slash
                    if (_baseUrl[_baseUrl.Length - 1].Equals('/'))
                    {
                        retVal = _baseUrl + "Relativity";
                    }
                    else
                    {
                        retVal = _baseUrl + "/Relativity";
                    }
                }
                return retVal;
            }
        }

        public Uri ServicesUri => new Uri(BaseRelativityUrl + ".Services");

        public Uri RestUri => new Uri(BaseRelativityUrl + ".REST/api");


        /// <summary>
        /// Instantiate with path to credentials file in the format:
        /// InstanceUrl
        /// Relativity username
        /// Relativity password
        /// </summary>
        /// <param name="filePath"></param>
        public ConnectionHelper(string filePath)
        {
            try
            {
                string[] creds = File.ReadAllLines(filePath);

                if (creds.Length == 1)
                {
                    Console.WriteLine("Using IntegratedAuth");
                    _baseUrl = creds[0];
                    _authType = Constants.Enums.AuthType.Integrated;
                }
                else if (creds.Length == 3)
                {
                    _baseUrl = creds[0];
                    _user = creds[1];
                    _password = creds[2];
                    Console.WriteLine("Using UsernamePassword");
                    _authType = Constants.Enums.AuthType.UsernamePassword;
                }

                else
                {
                    throw new ApplicationException($"File at {filePath} does not contain the correct number of lines.");
                }
            }

            catch (IOException)
            {
                Console.WriteLine("Specified credentials file not found.");
                throw;
            }
        }


        /// <summary>
        /// Instantiates the connection helper with user credentials
        /// </summary>
        /// <param name="instanceUrl"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public ConnectionHelper(string instanceUrl, string username, string password)
        {
            _baseUrl = instanceUrl;
            _user = username;
            _password = password;
            _authType = Constants.Enums.AuthType.UsernamePassword;
        }

        /// <summary>
        /// Tests if one can login
        /// </summary>
        /// <returns></returns>
        public bool TestLogin()
        {
            try
            {
                IRSAPIClient client = GetRsapiClient();
                client.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the RSAPIClient with the given workspace
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        public IRSAPIClient GetRsapiClient(int workspaceId = -1)
        {
            switch (_authType)
            {
                case Constants.Enums.AuthType.Integrated:
                    return GetRsapiIntegrated(workspaceId);

                case Constants.Enums.AuthType.UsernamePassword:
                    IRSAPIClient proxy = new RSAPIClient(
                        ServicesUri,
                        new kCura.Relativity.Client.UsernamePasswordCredentials(_user, _password));
                    //proxy.APIOptions.WorkspaceID = DefaultWorkspaceId;
                    proxy.APIOptions.WorkspaceID = workspaceId;
                    proxy.Login();
                    return proxy;

                default:
                    return GetRsapiIntegrated(workspaceId);
            }
        }


        /// <summary>
        /// Gets the RSAPIClient with IntegratedAuth
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        private IRSAPIClient GetRsapiIntegrated(int workspaceId = -1)
        {
            var creds = new kCura.Relativity.Client.IntegratedAuthCredentials();
            IRSAPIClient proxy = new RSAPIClient(
                ServicesUri,
                creds);
            proxy.APIOptions.WorkspaceID = workspaceId;
            proxy.Login();
            return proxy;
        }


        /// <summary>
        /// Gets the service factory with username/password
        /// </summary>
        /// <returns></returns>
        public ServiceFactory GetServiceFactory()
        {
            Credentials credentials;
            switch (_authType)
            {
                case Constants.Enums.AuthType.UsernamePassword:
                    credentials = new Relativity.Services.ServiceProxy.UsernamePasswordCredentials(_user, _password);
                    break;
                case Constants.Enums.AuthType.Integrated:
                    credentials = new Relativity.Services.ServiceProxy.IntegratedAuthCredentials();
                    break;
                default:
                    credentials = new Relativity.Services.ServiceProxy.IntegratedAuthCredentials();
                    break;
            }

            ServiceFactory factory
                = new ServiceFactory(new ServiceFactorySettings(ServicesUri, RestUri, credentials));
            return factory;
        }
    }
}
