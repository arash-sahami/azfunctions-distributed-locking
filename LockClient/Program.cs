﻿using CosmosDistributedLock;
using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;

namespace LockClient
{
    class Program
    {
        public static string lockId = "Lock 1";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var cosmosClient = new CosmosClient("AccountEndpoint=https://localhost:9000/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
                    //, new CosmosClientOptions
                    //{
                    //    SerializerOptions = new CosmosSerializationOptions
                    //    {
                    //        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    //    }
                    //}
                    );


            CosmosDBThrottledGate instance = new CosmosDBThrottledGate(cosmosClient, "Pluralsight", "LockTest");
            await instance.RunAsync(lockId, TimeSpan.FromSeconds(20), new ExecutionContext { InvocationId = Guid.NewGuid() }, () =>
             {
                 return Task.Run(async () =>
                 {
                     Console.WriteLine(args[0] + " Waiting");
                     await Task.Delay(30000);
                     Console.WriteLine(args[0] + "Executed ");
                 });
             }, () =>
             {
                 return Task.Run(() =>
                 {
                     Console.WriteLine(args[0] + "Trottled");
                 });
             });
        }
    }
}
