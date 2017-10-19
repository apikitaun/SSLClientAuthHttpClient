using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace CentralServerConceptTest.Bussiness
{

    /// <summary>
    ///  Example:
    ///  
    ///             CustomHttpClient client = new CustomHttpClient(true, @"c:\work\client.cer");
    ///             return await client.InvokeGet("localhost", "api/WebServiceConfiguration");
    /// 
    /// 
    /// 
    /// </summary>
    public class CustomHttpClient
    {
        public bool UseSSL { get; set; }
        private string _clientCertificatePath { get; set; }
        private X509Certificate _SSLCertificate;
        public CustomHttpClient()
        {
            UseSSL = false;
        }
        public CustomHttpClient(bool useSSL)
        {
            this.UseSSL = useSSL;
        }
        public CustomHttpClient(bool useSSL , string clientCertificatePath)
        {
            this.UseSSL = useSSL;
            SetServerCertificateValidation("", clientCertificatePath);
        }
        /// <summary>
        /// Custom validation of certificates
        /// ALWAYS Use it in Startup.cs
        /// </summary>
        /// <param name="SSLCertificateSubjectName"></param>
        public void SetServerCertificateValidation (string SSLCertificateSubjectName,string clientCertificatePath)
        {
            // Configuring SecurityProtocol to Tls12
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _clientCertificatePath = clientCertificatePath;
            _SSLCertificate = GetCertificate();

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, error) =>
                {
                    return cert.Issuer == _SSLCertificate.Issuer;
                };
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