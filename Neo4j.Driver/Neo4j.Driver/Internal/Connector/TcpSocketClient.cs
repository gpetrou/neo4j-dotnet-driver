// Copyright (c) 2002-2017 "Neo Technology,"
// Network Engine for Objects in Lund AB [http://neotechnology.com]
// 
// This file is part of Neo4j.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace Neo4j.Driver.Internal.Connector
{
    internal class TcpSocketClient : ITcpSocketClient
    {
        private readonly TcpClient _client;
        private Stream _stream;
        private ILogger _logger;

        private readonly EncryptionManager _encryptionManager;

        public TcpSocketClient(EncryptionManager encryptionManager, ILogger logger = null)
        {
            _encryptionManager = encryptionManager;
            _client = new TcpClient();
            _logger = logger;
        }

        public Stream ReadStream => _stream;
        public Stream WriteStream => _stream;

        public async Task DisconnectAsync()
        {
            Close();
        }

        public async Task ConnectAsync(Uri uri, bool useTls)
        {
            await _client.ConnectAsync(uri.Host, uri.Port).ConfigureAwait(false);

            if (!useTls)
            {
                _stream = _client.GetStream();
            }
            else
            {
                _stream = new SslStream(_client.GetStream(), true,
                    (sender, certificate, chain, errors) =>
                        _encryptionManager.TrustStrategy.ValidateServerCertificate(uri, certificate, errors));

                await ((SslStream)_stream)
                    .AuthenticateAsClientAsync(uri.Host, null, System.Security.Authentication.SslProtocols.Tls12, false)
                    .ConfigureAwait(false);
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing)
                return;

            Close();
        }

        private void Close()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}