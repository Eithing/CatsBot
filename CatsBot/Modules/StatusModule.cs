using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;
using SteamQuery;
using System.Linq;
using System.Net.NetworkInformation;
using System.Collections.Generic;

namespace CatsBot.Modules
{
    public class StatusModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("status", "Status d'un serveur gmod")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Status(string ip, ushort portOui = 27015)
        {
            await DeferAsync(); // Préviens Discord que le bot travaille sur la réponse
            try
            {
                var server = new GameServer($"{ip}:{portOui}");
                await server.PerformQueryAsync();

                var pingResult = await GetPingAsync(ip);

                var embed = new EmbedBuilder()
                    .WithTitle($"**Serveur GMod - {ip}:{portOui}**")
                    .WithColor(Color.Green)  // Couleur de l'embed (barre latérale)
                    .AddField("Nom du serveur", server.Information.ServerName, true)  // Texte en gras
                    .AddField("Ping", "*50ms*", true)  // Texte en italique
                    .AddField("Carte", server.Information.Map, true)
                    .AddField("Nombre de joueurs", $"**{server.Information.OnlinePlayers}/{server.Information.MaxPlayers}** - *En ligne*");  // Combinaison de gras et italique

                    // Diviser la liste des joueurs en morceaux si elle dépasse la limite de 1024 caractères par champ
                    var playersList = server.Players.Any() ?
                        server.Players.Select(p => $"**{p.Name}** *Score:* {p.Score}, *Durée de connexion:* {p.Duration}")
                        .ToList() : new List<string> { "Aucun joueur connecté." };

                    string currentField = "";
                    int fieldIndex = 1;

                    foreach (var player in playersList)
                    {
                        // Ajouter le joueur au champ actuel
                        if ((currentField + player).Length <= 1024)
                        {
                            currentField += player + "\n";
                        }
                        else
                        {
                            // Si l'ajout du joueur dépasse 1024 caractères, on ajoute le champ et on crée un nouveau champ
                            embed.AddField($"Liste des joueurs (partie {fieldIndex})", currentField);
                            fieldIndex++;
                            currentField = player + "\n";  // Nouveau champ
                        }
                    }

                    // Ajouter le dernier champ (si nécessaire)
                    if (!string.IsNullOrEmpty(currentField))
                    {
                        embed.AddField($"Liste des joueurs (partie {fieldIndex})", currentField)
                        .WithFooter("Requête effectuée par le bot", "http://eynwa.fr/images/lucane_anime.gif")
                        .WithTimestamp(DateTimeOffset.Now);
                    }



                    //.AddField("Liste des joueurs", server.Players.Any() ? string.Join("\n", server.Players.Select(p => $"**{p.Name}** *Score:* {p.Score}, *durée de connexion:* {p.Duration}")) : "Aucun joueur connecté.", false)  // Utilisation du markdown pour formater le texte
                    

                server.Close();
                await FollowupAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                if(ex.Message == "Hôte inconnu.")
                {
                    await FollowupAsync($"❌ Impossible de se connecter au serveur {ip}:{portOui}\nErreur: {ex.Message}");
                }
            }
        }

        // Fonction pour effectuer un ping ICMP sur l'IP donnée
        private async Task<long> GetPingAsync(string ip)
        {
            try
            {
                using (var ping = new Ping())
                {
                    // Effectue le ping
                    var reply = await ping.SendPingAsync(ip);
                    if (reply.Status == IPStatus.Success)
                    {
                        return reply.RoundtripTime;  // Retourne le temps du ping en millisecondes
                    }
                    else
                    {
                        return -1;  // Retourne -1 si le ping échoue
                    }
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur dans le ping
                Console.WriteLine($"Erreur lors du ping : {ex.Message}");
                return -1;
            }
        }
    }
}
