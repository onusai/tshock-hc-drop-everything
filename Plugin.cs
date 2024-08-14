using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using System.Text.Json;
using Terraria.GameContent.Creative;
using System.Diagnostics;
using IL.Terraria.DataStructures;
using Terraria.Net;
using IL.Terraria.Net;

namespace HCDropEverything
{
    [ApiVersion(2, 1)]
    public class HCDropEverything : TerrariaPlugin
    {

        public override string Author => "Onusai";
        public override string Description => "Upon death hardcore characters will drop consumed life crystals and items in banks";
        public override string Name => "HCDropEverything";
        public override Version Version => new Version(1, 0, 0, 0);

        public class ConfigData
        {
            public bool DropConsumedLifeCrystals { get; set; } = true;
            public bool DropItemsInBanks { get; set; } = true;
        }

        ConfigData config;

        public HCDropEverything(Main game) : base(game) { }

        public override void Initialize()
        {
            config = PluginConfig.Load("HCDropEverything");

            ServerApi.Hooks.GameInitialize.Register(this, OnGameLoad);
        }

        void OnGameLoad(EventArgs e)
        {
            TShockAPI.GetDataHandlers.KillMe += OnPlayerDeath;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnGameLoad);
                TShockAPI.GetDataHandlers.KillMe -= OnPlayerDeath;
            }
            base.Dispose(disposing);
        }

        void RegisterCommand(string name, string perm, CommandDelegate handler, string helptext)
        {
            TShockAPI.Commands.ChatCommands.Add(new Command(perm, handler, name)
            { HelpText = helptext });
        }

        void OnPlayerDeath(object sender, TShockAPI.GetDataHandlers.KillMeEventArgs args)
        {
            var player = TShock.Players[args.Player.Index];
            if (player.Difficulty != 2) return;

            int drop_amount = (player.TPlayer.statLifeMax - 100) / 20;
            if (config.DropConsumedLifeCrystals && drop_amount > 0)
            {
                Item.NewItem(null, (int)player.X, (int)player.Y, player.TPlayer.width, player.TPlayer.height, 29, drop_amount, false, 0, true);
            }

            if (config.DropItemsInBanks)
            {
                List<Chest> banks = new List<Chest>
                {
                    player.TPlayer.bank,
                    player.TPlayer.bank2,
                    player.TPlayer.bank3,
                    player.TPlayer.bank4,
                };

                foreach (Chest bank in banks)
                {
                    foreach (Item item in bank.item)
                    {
                        if (item.type == 0) continue;

                        Item.NewItem(null, (int)player.X, (int)player.Y, player.TPlayer.width, player.TPlayer.height, item.type, item.stack, false, 0, true);
                    }
                }
            }
        }

        public static class PluginConfig
        {
            public static string filePath;
            public static ConfigData Load(string Name)
            {
                filePath = String.Format("{0}/{1}.json", TShock.SavePath, Name);

                if (!File.Exists(filePath))
                {
                    var data = new ConfigData();
                    Save(data);
                    return data;
                }

                var jsonString = File.ReadAllText(filePath);
                var myObject = JsonSerializer.Deserialize<ConfigData>(jsonString);

                return myObject;
            }

            public static void Save(ConfigData myObject)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(myObject, options);

                File.WriteAllText(filePath, jsonString);
            }
        }

    }
}
