﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System.Linq;
using SchoolDiscordBot.Persistent;
using System.IO;
using System.Web;
using SchoolDiscordBot.Modules.Preconditions;
using System.Collections.Concurrent;
using SchoolDiscordBot.Helpers;
using System.Reflection;
using SchoolDiscordBot.Modules.References;
using System.Net;
using Newtonsoft.Json;
using NReco;

namespace SchoolDiscordBot.Modules
{
    public sealed class BasicCommands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            // spend time here developing system to display all the commands properly and allow the user to specify depth and detail.

        }

        [Command("hello")]
        public async Task Hello()
        {
            string html = File.ReadAllText("Modules/References/hello.html");
            await Context.Channel.SendMessageAsync(html);
            html = string.Format(html, Context.User.Username);
            var api = new HtmlToPdfOrImage.Api("99433eae-569c-470d-a763-0a3d0b28ad21", "vmZ2Qryg");
            var format = new HtmlToPdfOrImage.GenerateSettings()
            {
                OutputType = HtmlToPdfOrImage.OutputType.Image
            };
            var result = api.Convert(html, format);
            await Context.Channel.SendFileAsync(new MemoryStream((byte[])result.model), "hello.png");
        }

        [Command("person")]
        [Cooldown(10)]
        public async Task Person()
        {
            EmbedBuilder embed = new EmbedBuilder();
            string json = "";
            using (WebClient client = new WebClient())
            {
                json = client.DownloadString("https://randomuser.me/api/1.1/");
            }
            var dataObject = JsonConvert.DeserializeObject<dynamic>(json);

            string gender = dataObject.results[0].gender.ToString();
            string portrait = dataObject.results[0].picture.large.ToString();
            string name = $"{dataObject.results[0].name.title.ToString()}. {dataObject.results[0].name.first.ToString()} {dataObject.results[0].name.last.ToString()}";
            string nationality = dataObject.results[0].nat.ToString();
            var locationinfo = dataObject.results[0].location;
            string street = locationinfo.street.ToString();
            string city = locationinfo.city.ToString();
            string state = locationinfo.state.ToString();
            string postcode = locationinfo.postcode.ToString();
            string location = $"{street}, {city} {postcode}, {state}.";
            string dob = dataObject.results[0].dob.ToString();
            string phone = dataObject.results[0].phone.ToString();
            string cell = dataObject.results[0].cell.ToString();

            EmbedFooterBuilder embedfoot = new EmbedFooterBuilder();

            embedfoot.WithText(dataObject.results[0].email.ToString());

            embed.WithThumbnailUrl(portrait)
                .WithTitle("Random person: ")
                .AddInlineField("Name", name)
                .WithDescription($"{gender}, from {nationality} in {dob.TrimEnd()}.")
                .AddInlineField("Location", location)
                .WithAuthor(Context.User)
                .AddInlineField("Login", $"Username : {dataObject.results[0].login.username.ToString()} \nPassword : {dataObject.results[0].login.password.ToString()}")
                .WithFooter(embedfoot)
                .AddInlineField("Phone", $"Home {phone}\nCell {cell}");


            await Context.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("purge")]
        [Cooldown(3600)]
        public async Task Purge([Remainder]string Message)
        {

        }

        [Command("8ball"), Remarks("answers any question with a stunning level of wiseness")]
        public async Task EightBall([Remainder]string message)
        {
            EightBall Ball = new EightBall();
            await Context.Channel.SendMessageAsync(Ball.GrabRandomAnswer());
        }

        [Command("ping")]
        public async Task Ping()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($":ping_pong:  Pong!");
            await ReplyAsync("", false, builder.Build());
        }

        [Command("echo")]
        public async Task Echo([Remainder]string message)
        {
            var embed = new EmbedBuilder();
            string r = Utilities.GetFormattedAlert("WELCOME_&NAME", Context.User.Username);
            embed.WithTitle("Echoed message")
                .WithDescription(r)
                .WithColor(new Color(0, 255, 0));


            await Context.Channel.SendMessageAsync("", false, embed);
        }

    }
}