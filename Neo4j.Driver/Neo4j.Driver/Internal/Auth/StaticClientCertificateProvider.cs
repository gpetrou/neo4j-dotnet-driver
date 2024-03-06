﻿// Copyright (c) "Neo4j"
// Neo4j Sweden AB [https://neo4j.com]
// 
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Neo4j.Driver.Preview.Auth;

namespace Neo4j.Driver.Internal.Auth;

internal class StaticClientCertificateProvider : IClientCertificateProvider
{
    private readonly X509Certificate2 _certificate;

    public StaticClientCertificateProvider(X509Certificate2 certificate)
    {
        _certificate = certificate;
    }

    public ValueTask<X509Certificate2> GetCertificateAsync()
    {
        return new ValueTask<X509Certificate2>(_certificate);
    }
}
