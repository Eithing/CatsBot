using Discord;
using Discord.Interactions;
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
            try
            {
                await FollowupAsync("OUI / 20");
            }
            catch (Exception ex)
            {
            }
        }
    }
}
