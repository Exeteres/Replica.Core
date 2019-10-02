using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Replica.Core.Utils
{
    public class SimpleHttpServer
    {
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public delegate string UpdateHandler(string json);
        public delegate void StartHandler();

        public event StartHandler OnStart;

        private class HttpHandler
        {
            public string Endpoint { get; set; }
            public UpdateHandler Handler { get; set; }
        }

        private readonly List<HttpHandler> _handlers = new List<HttpHandler>();

        public void AddHandler(string endpoint, UpdateHandler handler)
        {
            _handlers.Add(new HttpHandler { Endpoint = endpoint, Handler = handler });
        }

        public SimpleHttpServer(int port)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:" + port + "/");
        }

        public void Listen()
        {
            _cts = new CancellationTokenSource();
            _listener.Start();
            OnStart?.Invoke();
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    var ctx = await _listener.GetContextAsync();

                    // request
                    using var reader = new StreamReader(ctx.Request.InputStream);
                    var handler = _handlers.FirstOrDefault(x => ctx.Request.Url.PathAndQuery.StartsWith(x.Endpoint));
                    if (handler == null)
                    {
                        ctx.Response.StatusCode = 404;
                        ctx.Response.Close();
                        continue;
                    }
                    var resp = handler.Handler.Invoke(reader.ReadToEnd());
                    // response
                    ctx.Response.StatusCode = 200;
                    var buffer = Encoding.ASCII.GetBytes(resp);
                    ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    ctx.Response.Close();
                }
                _listener.Stop();
            });
        }

        public void Stop()
        {
            _cts.Cancel();
        }
    }
}