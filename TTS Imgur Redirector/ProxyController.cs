using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
namespace TTS_Imgur_Redirector
{
    public class ProxyController
    {
        public readonly ProxyServer proxyServer;

        public Action<string> OnRequestIntercepted;

        public ProxyController()
        {
            proxyServer = new ProxyServer();
        }

        public void Start()
        {
            proxyServer.BeforeResponse += ProxyServer_BeforeResponse;

            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000, false)
            {
                IncludedHttpsHostNameRegex = new List<string> { "i.imgur.com" }
            };
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();
            
            proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
        }

        public void Stop()
        {
            proxyServer.BeforeResponse -= ProxyServer_BeforeResponse;
            proxyServer.Stop();
        }

        private async Task ProxyServer_BeforeResponse(object sender, SessionEventArgs e)
        {
            if (e.WebSession.Request.RequestUri.Host.Contains("i.imgur.com"))
            {
                if (e.WebSession.Response.ResponseStatusCode != "200")
                {
                    OnRequestIntercepted?.Invoke(e.WebSession.Request.Url);

                    var image_name = e.WebSession.Request.RequestUri.LocalPath.Substring(1);
                    var redirect_url = $"http://kageurufu.net/imgur/?{image_name}";
                    await e.Redirect(redirect_url);
                }
            }
        }
    }
}