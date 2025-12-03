using Client.Managers;
using Common.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Common.Utils;
using System.IO;
using Client.Services;

namespace Client.SignalR
{
    public static class CommandHandler
    {
        private static readonly Dictionary<string, Func<CommandEnvelope, Task>> Handlers =
            new(StringComparer.OrdinalIgnoreCase);

        public static void Create()
        {
            Handlers["AtcChat"] = AtcChat;
            Handlers["OnExportAllAirplanes"] = OnExportAllAirplanes;

            SignalRClient.Connection?.On<string>("ReceiveEnvelope", json =>
            {
                var env = JsonConvert.DeserializeObject<CommandEnvelope>(json);
                if (env == null) return;

                if (Handlers.TryGetValue(env.Command, out var handler))
                    handler(env);
            });
        }

        private static Task AtcChat(CommandEnvelope env)
        {
            var token = env.Payload as JToken;
            ChatMessage message = token?.ToObject<ChatMessage>();
            if (message == null) return Task.CompletedTask;
            ViewManager.MessagesViewModel.AddMessageFromATC(message.Sender, message.Text, message.Channel);
            return Task.CompletedTask;
        }

        private static Task OnExportAllAirplanes(CommandEnvelope env)
        {
            var token = env.Payload as JToken;
            JArray airplanes = token?.ToObject<JArray>();
            SurveillanceService.Airplanes.Clear();
            foreach (JObject airplane in airplanes)
            {
                SurveillanceService.Airplanes.Add(airplane);
            }
            App.RequestRender();
            return Task.CompletedTask;
        }
    }
}
