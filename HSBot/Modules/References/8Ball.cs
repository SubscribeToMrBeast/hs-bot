﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HSBot.Modules.References
{
    internal sealed class EightBall
    {
        internal List<string> Answers = new List<string>
        {
            "It is certain",
            "It is decidedly so",
            "Without a doubt",
            "Yes definitely",
            "You may rely on it",
            "As I see it, yes",
            "Most likely",
            "Outlook good",
            "Signs point to yes",
            "Reply hazy try again",
            "Ask again later",
            "Better not tell you now",
            "Cannot predict now",
            "Concentrate and ask again",
            "Don't count on it",
            "My reply is no",
            "My sources say no",
            "Outlook not so good",
            "Very doubtful",
        };
        
        internal string GrabRandomAnswer()
        {
            return Answers[Global.R.Next(0, Answers.Count)];
        }
    }
}
