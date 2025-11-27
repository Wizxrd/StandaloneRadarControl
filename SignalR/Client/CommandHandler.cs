using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SignalR.Client
{
    public sealed class CommandEnvelope
    {
        public string Command { get; init; }
        public JsonElement Payload { get; init; }
    }

    public static class CommandHandler
    {
        private static readonly Dictionary<string, Func<CommandEnvelope, Task>> Handlers =
            new(StringComparer.OrdinalIgnoreCase);

        public static void Create()
        {
            Register("Chat", HandleChatAsync);

            SignalRClient.Connection?.On<string>("ReceiveMessage", async (message) =>
            {
                var envelope = ParseEnvelope(message);
                await DispatchAsync(envelope);
            });
        }

        public static void Register(string commandName, Func<CommandEnvelope, Task> handler)
        {
            Handlers[commandName] = handler;
        }

        private static CommandEnvelope ParseEnvelope(string rawMessage)
        {
            using var doc = JsonDocument.Parse(rawMessage);
            var root = doc.RootElement;

            return new CommandEnvelope
            {
                Command = root.GetProperty("command").GetString() ?? string.Empty,
                Payload = root.TryGetProperty("payload", out var payload) ? payload : default
            };
        }

        private static async Task DispatchAsync(CommandEnvelope envelope)
        {
            if (!Handlers.TryGetValue(envelope.Command, out var handler))
                return;

            await handler(envelope);
        }

        private static Task HandleChatAsync(CommandEnvelope envelope)
        {
            var text = envelope.Payload.GetProperty("text").GetString();
            return Task.CompletedTask;
        }
    }
}
