using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace CentralServerConceptTest.Bussiness
{
    public class CustomHttpClient
    {
        public bool UseSSL { get; set; }
        private static string _clientCertificatePath { get; set; }
        private static X509Certificate _SSLCertificate;
        public CustomHttpClient()
        {
            UseSSL = false;
        }
        public CustomHttpClient(bool useSSL)
        {
            this.UseSSL = useSSL;
        }
        /// <summary>
        /// Custom validation of certificates
        /// ALWAYS Use it in Startup.cs
        /// </summary>
        /// <param name="SSLCertificateSubjectName"></param>
        public static void SetServerCertificateValidation (string SSLCertificateSubjectName,string clientCertificatePath)
        {
            // Configuring SecurityProtocol to Tls12
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Reading content of SSL Server Certificate
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, SSLCertificateSubjectName, false);
            _SSLCertificate = certificates[0];
            store.Close();
            // Configuring validation of certificate rules
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, error) =>
                {
                    return cert.Issuer == _SSLCertificate.Issuer;
                };
            // Loading client certificate path
            _clientCertificatePath = clientCertificatePath;
        }
        protected X509Certificate GetCertificate ()
        {
            X509Certificate certificate = new X509Certificate();
            certificate.Import(_clientCertificatePath);
            return certificate;
        }
        public HttpClient CreateHttpClient(string baseAddress)
        {
            Uri uriAddress = new Uri((UseSSL == false ? "http://" : "https://") + baseAddress);
            HttpClient client = null;
            if (UseSSL== false)
            {
                client = new HttpClient
                {
                    BaseAddress = uriAddress
                };
            }
            else
            {
                WebRequestHandler handler = new WebRequestHandler();
                handler.ClientCertificates.Add(GetCertificate());
                client = new HttpClient(handler)
                {
                    BaseAddress = uriAddress
                };
            }

            return client;
        }
        public async Task<string> InvokeGet ( string baseAddress,string relativeUri)
        {
            HttpClient client = CreateHttpClient(baseAddress);
            HttpResponseMessage response = await client.GetAsync(relativeUri);
            return await response.Content.ReadAsStringAsync();
            
        }
        public async Task<string> InvokePost ( string baseAddress, string relativeUri , string content)
        {
            HttpClient client = CreateHttpClient(baseAddress);
            HttpResponseMessage response = await client.PostAsync(relativeUri,new StringContent(content));
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> InvokePost(string baseAddress, string relativeUri, object content)
        {
            return await InvokePost(baseAddress, relativeUri, JsonConvert.SerializeObject(content));
        }
        public async Task<T> InvokePost<T>(string baseAddress, string relativeUri, object content)
        {
            return await InvokePost<T>(baseAddress, relativeUri, JsonConvert.SerializeObject(content));
        }
        public async Task<T> InvokePost<T>(string baseAddress, string relativeUri, string content)
        {
            HttpClient client = CreateHttpClient(baseAddress);
            HttpResponseMessage response = await client.PostAsync(relativeUri, new StringContent(content));
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}