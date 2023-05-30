﻿// Copyright (c) "Neo4j"
// Neo4j Sweden AB [http://neo4j.com]
// 
// This file is part of Neo4j.
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

using System.Collections.Generic;
using Neo4j.Driver.Internal.Auth;
using Neo4j.Driver.Internal.IO;
using Neo4j.Driver.Internal.IO.MessageSerializers;

namespace Neo4j.Driver.Internal.Messaging;

internal sealed class LogonMessage : IRequestMessage
{
    public LogonMessage(BoltProtocolVersion _, IAuthToken authToken)
    {
        Auth = authToken.AsDictionary();
    }

    public IDictionary<string, object> Auth { get; }
    public IPackStreamSerializer Serializer => LogonMessageSerializer.Instance;

    public override string ToString()
    {
        var authToken = new Dictionary<string, object>(Auth);

        if (authToken.ContainsKey(AuthToken.CredentialsKey))
        {
            authToken[AuthToken.CredentialsKey] = "******";
        }

        return $"LOGON {authToken.ToContentString()}";
    }
}
