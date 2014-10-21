using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace RestLibrary
{
    public class RequestClient
    {
        private CookieContainer cookieContainer;
        private String baseUriString = "https://my.vonagebusiness.com/";
        private Uri baseUri;
        private String user;
        private String password;
        private DateTime lastSuccessfulRequest;
        private DateTime lastAttempted;
        private Object syncLock;
        private String applicationVersion;

        //internal void MarkLastAttempted()
        public void MarkLastAttempted()
        {
            this.lastAttempted = DateTime.Now;
        }

        public class LastServerResponseArgs : EventArgs
        {
            public HttpStatusCode HttpStatusCode { get; set; }
            public DateTime Timestamp { get; set; }
        }

        //internal void MarkSuccess(HttpStatusCode statusCode)
        public void MarkSuccess(HttpStatusCode statusCode)
        {
            this.lastSuccessfulRequest = DateTime.Now;

            if (LastServerResponse != null)
            {
                LastServerResponse(this, new LastServerResponseArgs() { HttpStatusCode = statusCode, Timestamp = DateTime.Now });
            }
        }

        public CookieContainer GetCookies()
        {
            lock (this.syncLock)
            {
                return this.cookieContainer;
            }
        }

        void HackCookiesRemoveVersion(HttpWebRequest request)
        {
            CookieContainer result = request.CookieContainer;
            foreach (Cookie cookie in request.CookieContainer.GetCookies(request.RequestUri))
            {
                if (cookie.Version > 0)
                {
                    Debug.WriteLine("Bad version!");
                    cookie.Version = 0;
                }
            }
        }

        public HttpWebRequest CreateRequest(Uri requestUri, String accept, String method)
        {
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.CookieContainer = GetCookies();

            HackCookiesRemoveVersion(request);

            request.Accept = accept;
            request.Method = method;

            // this throws an exception
            // request.Headers["Accept-Encoding"] = "gzip";
            request.Headers["X-VPF-Version"] = this.applicationVersion;

            return request;
        }


        public void BeginInitialize(Action onCompleted, Action<Exception> onFailed)
        {
            // get account
            // presence/rest/getAccount/<AN>
            // presence/rest/directory
            HttpWebRequest httpWebRequest = CreateRequest(@"[dashboard]/presence/", "application/json", "GET");

            httpWebRequest.BeginGetResponse((result) =>
                {
                    MarkLastAttempted();

                    try
                    {
                        using (HttpWebResponse response = httpWebRequest.EndGetResponse(result) as HttpWebResponse)
                        {
                            MarkSuccess();

                            if (onCompleted != null)
                            {
                                onCompleted();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (onFailed != null)
                        {
                            onFailed(ex);
                        }
                    }
                }, null);
        }




    }
}
