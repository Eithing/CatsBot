using CatsBot.Modules;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CatsBot.Services
{
    public class DiscordStartupService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private bool _commandsRegistered = false;  // Ajouter un flag pour vérifier l'enregistrement

        public DiscordStartupService(DiscordSocketClient discord, InteractionService commands, IServiceProvider services)
        {
            _discord = discord;
            _commands = commands;
            _services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Connexion du bot
            await _discord.LoginAsync(TokenType.Bot, ""); // ⚠️ Remplace par ton token
            await _discord.StartAsync();

            // Désabonne l'événement pour éviter les abonnements multiples
            _discord.InteractionCreated -= HandleInteractionAsync;
            _discord.InteractionCreated += HandleInteractionAsync;

            _discord.Ready += async () =>
            {
                ulong guildId = 0; // Remplace par ton Guild ID

                var _interactionService = new InteractionService(_discord);
                await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
                await _interactionService.RegisterCommandsToGuildAsync(guildId);

                _discord.InteractionCreated += async interaction =>
                {
                    var scope = _services.CreateScope();
                    var ctx = new SocketInteractionContext(_discord, interaction);
                    await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
                };

                _commandsRegistered = true;  // Marque les commandes comme enregistrées

                Console.WriteLine("✅ Les slash commands sont enregistrées !");
            };
        }
        // Gestion des interactions
        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                Console.WriteLine($"🔔 Interaction reçue : {interaction.Type}");

                var context = new SocketInteractionContext(_discord, interaction);
                var result = await _commands.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"⚠️ Erreur lors de l'exécution de la commande : {result.ErrorReason}");

                    // Envoie un message d'erreur si la commande échoue
                    if (interaction.Type == InteractionType.ApplicationCommand)
                    {
                        await interaction.RespondAsync("❌ Erreur lors de l'exécution de la commande.", ephemeral: true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Exception: {ex.Message}");

                // Gestion d'une erreur générale
                if (interaction.Type == InteractionType.ApplicationCommand && !interaction.HasResponded)
                {
                    await interaction.RespondAsync("🚨 Une erreur est survenue.", ephemeral: true);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.LogoutAsync();
            await _discord.StopAsync();
        }
    }
}

