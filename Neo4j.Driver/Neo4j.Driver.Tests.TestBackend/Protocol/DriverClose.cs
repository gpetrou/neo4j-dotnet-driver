﻿using System;
using System.Threading.Tasks;
using Neo4j.Driver;


namespace Neo4j.Driver.Tests.TestBackend
{ 
    internal class DriverClose : IProtocolObject
    {
        public DriverCloseType data { get; set; } = new DriverCloseType();
        
        public class DriverCloseType
        {
            public string driverId { get; set; }            
        }

        public override async Task Process()
        {
            IDriver driver = ((NewDriver)ObjManager.GetObject(data.driverId)).Driver;
            await driver.CloseAsync();                            
        }

        public override string Respond()
        {   
            return new Response("Driver", uniqueId).Encode();
        }
    }
}
