using Discord;
using Discord.Interactions;
using MySql.Data.MySqlClient;
using SteamQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatsBot.Modules
{
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("info", "Information de chaque joueurs gmod depuis une base de données")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Info(string data)
        {
            await DeferAsync(); // Préviens Discord que le bot travaille sur la réponse
            string server = "51.195.100.159";
            string database = "s3_database_Cats";
            string user = "Api";
            string password = "879453216ea";
            string port = "3306";

            string connectionString = $"Server={server};Database={database};User ID={user};Password={password};Port={port};SslMode=none;";


            try
            {
                // 📡 Ouvrir connexion
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // 🐾 Exemple de requête SELECT
                    string query = $"SELECT * FROM joueurs WHERE steamid = '{data}' OR pseudo = '{data}' OR steamid64 = '{data}' LIMIT 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@data", data); // ✅ Paramétrisation pour éviter l'injection SQL

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    string pseudo = reader.GetString("pseudo");
                                    string steamid = reader.GetString("steamid");
                                    string steamid64 = reader.GetString("steamid64");

                                    var embed = new EmbedBuilder()
                                        .WithTitle("🎮 Infos du joueur")
                                        .WithColor(Color.Green)
                                        .AddField("🔹 Pseudo", pseudo)
                                        .AddField("🔹 SteamID", steamid)
                                        .AddField("🔹 SteamID64", steamid64)
                                        .WithFooter("Requête effectuée par le bot", "http://eynwa.fr/images/lucane_anime.gif")
                                        .WithTimestamp(DateTimeOffset.Now);

                                    await FollowupAsync(embed: embed.Build());
                                }
                            }
                            else
                            {
                                // 🚫 Aucun joueur trouvé
                                await FollowupAsync("❌ Aucun joueur trouvé avec ces informations.");
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (MySqlException ex)
            {
                await FollowupAsync($"❌ Erreur MySQL : {ex.Message}");
            }
            catch (Exception ex)
            {
                await FollowupAsync($"❌ Erreur générale : {ex.Message}");
            }
        }
    }
}
