﻿using System;
using BotFramework;
using ImgXml;
using System.Drawing;
using System.Linq;

namespace UI
{
    class Guildwar
    {
        private static int error = 0, waittime = 0;
        private static readonly int[] guildwartime = {8, 12, 19, 22 };
        public static void Enter()
        {
            var tempEvent = PrivateVariable.Instance.VCevent;
            var Japan = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            var time = TimeZoneInfo.ConvertTime(DateTime.Now, Japan).TimeOfDay;
            var hour = time.Hours;
            while (guildwartime.Contains(hour))
            {
                if (!PrivateVariable.Instance.LocatedGuildWar)
                {
                    Variables.ScriptLog("Entering Guildwar!", Color.Lime);
                    PrivateVariable.Instance.VCevent = PrivateVariable.EventType.GuildWar;
                    for (int x = 0; x < 30; x++)
                    {
                        while (!BotCore.GameIsForeground(VCBotScript.game))
                        {
                            PrivateVariable.Instance.LocatedGuildWar = false;
                            BotCore.StartGame(VCBotScript.game + VCBotScript.activity);
                            BotCore.Delay(5000);
                            VCBotScript.LocateMainScreen();
                            x = 0;
                        }
                        var image = Screenshot.ImageCapture();
                        var point = BotCore.FindImage(image, Img.GreenButton, false, 0.9);
                        if (point != null)
                        {
                            BotCore.SendTap(point.Value);
                            BotCore.Delay(500);
                            x--;
                            continue;
                        }
                        point = BotCore.FindImage(Screenshot.CropImage(image, new Point(319, 525), new Point(588, 591)), Img.Start_Game, false, 0.8);
                        if (point != null && BotCore.RGBComparer(new Point(379, 642), Color.FromArgb(37, 37, 37), 15, image))
                        {
                            Variables.ScriptLog("Start Game Button Located!", Color.Lime);
                            BotCore.SendTap(point.Value);
                            PrivateVariable.Instance.InMainScreen = false;
                            PrivateVariable.Instance.LocatedGuildWar = false;
                            VCBotScript.LocateMainScreen();
                        }
                        if (BotCore.FindImage(image, "Img\\GuildWar\\Locate.png", false, 0.85) != null)
                        {
                            PrivateVariable.Instance.LocatedGuildWar = true;
                            break;
                        }
                        if (x > 10)
                        {
                            Variables.ScriptLog("Somehing is not right! No guild war locate found! Lets get in with different method!", Color.Yellow);
                            if (Variables.FindConfig("GuildWar", "EnterPosX", out string p_x) && Variables.FindConfig("GuildWar", "EnterPosY", out string p_y))
                            {
                                if (BotCore.FindImage(image, Img.MainScreen, true, 0.9) == null)
                                {
                                    BotCore.SendTap(point.Value);
                                    BotCore.Delay(1000, false);
                                    Variables.ScriptLog("Returning main screen", Color.Lime);
                                    BotCore.SendTap(942, 630);
                                    BotCore.Delay(5000, false);
                                    //We are not at mainscreen and having nothing to do
                                    point = BotCore.FindImage(image, Img.Back_to_Village, true, 0.9);
                                    if (point != null)
                                    {
                                        Variables.ScriptLog("Going back to Main screen", Color.Lime);
                                        BotCore.SendTap(point.Value);
                                        do
                                        {
                                            BotCore.Delay(3000);
                                            VCBotScript.image = Screenshot.ImageCapture();
                                        }
                                        while (BotCore.FindImage(VCBotScript.image, Img.MainScreen, true, 0.9) == null);
                                    }
                                }
                                Variables.ScriptLog("Screen Located", Color.Lime);
                                BotCore.SendTap(170, 655);
                                BotCore.Delay(5000, false);
                                try
                                {
                                    BotCore.SendTap(Convert.ToInt32(p_x), Convert.ToInt32(p_y));
                                }
                                catch
                                {
                                    Variables.ModifyConfig("GuildWar", "EnterPosX", "620");
                                    Variables.ModifyConfig("GuildWar", "EnterPosY", "90");
                                    continue;
                                }
                            }
                            else
                            {
                                Variables.ModifyConfig("GuildWar", "EnterPosX", "620");
                                Variables.ModifyConfig("GuildWar", "EnterPosY", "90");
                            }
                            x = 0;
                            error++;
                            if(error > 3)
                            {
                                Variables.ScriptLog("Unable to get into guildwar!",Color.Red);
                                Variables.ModifyConfig("GuildWar", "Manual", "true");
                                return;
                            }
                            continue;
                        }
                    }
                }
                GuildWar(time);
                time = TimeZoneInfo.ConvertTime(DateTime.Now, Japan).TimeOfDay;
                hour = time.Hours;
            }
            PrivateVariable.Instance.VCevent = tempEvent;
            PrivateVariable.Instance.LocatedGuildWar = false;
            return;
        }

        //Guild wars
        private static void GuildWar(TimeSpan time)
        {
            var image = Screenshot.ImageCapture();
            //Read energy
            var greenbutton = BotCore.FindImage(image, Img.GreenButton, false, 0.9);
            if (greenbutton !=null)
            {
                BotCore.SendTap(greenbutton.Value);
                BotCore.Delay(1000);
                return;
            }
            VCBotScript.energy = VCBotScript.GetEnergy();
            VCBotScript.runes = VCBotScript.GetRune();
            if(VCBotScript.energy > 0)
            {
                if(time.Minutes < 50)
                {
                    if (VCBotScript.energy < 5 && VCBotScript.runes > 3 && VCBotScript.runes < 6)
                    {
                        if (waittime == 0)
                        {
                            Variables.ScriptLog("Waiting Energy before Fever", Color.Yellow);
                        }
                        else if (waittime % 5 == 0)
                        {
                            BotCore.SendTap(797, 187);
                            BotCore.Delay(5000);
                            waittime = 0;
                        }
                        BotCore.Delay(5000);
                        waittime++;
                        return;
                    }
                }
                PrivateVariable.Instance.Battling = true;
                var redbutton = BotCore.FindImage(image, Img.Red_Button, false, 0.9);
                if (redbutton != null)
                {
                    BotCore.SendTap(redbutton.Value);
                    int x = 0;
                    do
                    {
                        if(BotCore.RGBComparer(new Point(247, 356), Color.Black, 10))
                        {
                            break;
                        }
                        else
                        {
                            BotCore.Delay(1000);
                            x++;
                        }
                    }
                    while (x < 20);
                    VCBotScript.Battle();
                }
            }
            else
            {
                if (waittime == 0)
                {
                    Variables.ScriptLog("Waiting Energy", Color.Yellow);
                }
                else if (waittime % 5 == 0)
                {
                    Variables.ScriptLog("Refreshing Map",Color.Cyan);
                    BotCore.SendTap(797, 187);
                    BotCore.Delay(5000);
                    waittime = 0;
                }
                BotCore.Delay(5000);
                waittime++;
                return;
            }
        }
    }
}
