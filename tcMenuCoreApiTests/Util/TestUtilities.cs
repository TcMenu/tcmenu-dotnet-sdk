using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcMenu.CoreSdkTests.Util
{
    public class TestUtils
    {
        public static string GetResourceTestFile(Type ty, string file)
        {
            using (Stream stream = ty.Assembly.GetManifestResourceStream(file))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static byte[] GetResourceTestFileAsBytes(Type ty, string file)
        {
            using (Stream stream = ty.Assembly.GetManifestResourceStream(file))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return reader.ReadBytes(100000);
                }
            }
            throw new ArgumentException("Resource File not found " + file);
        }
    }

    public class SimHttpMessageHandler : HttpMessageHandler
    {
        private Dictionary<string, HttpContent> responseByRequest = new Dictionary<string, HttpContent>();

        public void AddRequest(string request, string response)
        {
            responseByRequest[request] = new StringContent(response);
        }

        public void AddRequest(string request, byte[] response)
        {
            responseByRequest[request] = new ByteArrayContent(response);
        }

        public void ClearAll()
        {
            responseByRequest.Clear();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var entry in responseByRequest)
            {
                if (request.RequestUri.ToString().Contains(entry.Key))
                {
                    HttpResponseMessage rm = new HttpResponseMessage();
                    rm.StatusCode = System.Net.HttpStatusCode.OK;
                    rm.RequestMessage = request;
                    rm.Content = entry.Value;
                    return Task.FromResult(rm);
                }
            }

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }

}
