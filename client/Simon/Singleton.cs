using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Simon
{
    public sealed class Singleton
    {
        private static readonly Singleton instance = new Singleton();
        HubConnection connection;

        static Singleton()
        {
        }

        private Singleton()
        {
        }



        public async System.Threading.Tasks.Task RunQueriesAsync()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:53257/chatHub")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                int i = 0;
            });

            await connection.StartAsync();

            await connection.InvokeAsync("SendMessage",
            "ycallen", "hello");
        }

        public static Singleton Instance
        {
            get
            {
                return instance;
            }
        }



    }
}
