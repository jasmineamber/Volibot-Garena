﻿/*
 * Hello and welcome to the VoliBot AutoQueuer Project!
 * Credits to: shalzuth, Maufeat, imsosharp
 * Find assemblies for this AutoQueuer on LeagueSharp's official forum at:
 * http://www.joduska.me/
 * You are allowed to copy, edit and distribute this project,
 * as long as you don't touch this notice and you release your project with source.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ini;
using System.Collections;
using System.IO;
using System.Threading;
using System.Net;
using System.Management;
using LoLLauncher;
using System.Windows.Forms;
using System.Security.Principal;

namespace RitoBot
{
    public class Program
    {
        /*| VoliBot is an open source League of Legends Auto Queue Bot
         *| Thanks to: Maufeat, Fulcrum and shalzuth.
         *| Website: www.volibot.com
         */

        public static string Path2;
        public static string Region;
        public static ArrayList accounts = new ArrayList();
        public static ArrayList accounts2 = new ArrayList();
        public static int maxBots = 1;
        public static bool replaceConfig =  false;
        public static int connectedAccs = 0;
        public static string championId = "";
        public static List<String> championId2 = new List<string>();
        public static int lastPick = -1;
        public static int maxLevel = 31;
        public static int stopHour = 24;
        public static bool buyBoost = false;
        public static bool rndSpell = true;
        public static string spell1 = "flash";
        public static string spell2 = "ignite";
        public static string cversion = "5.6.15_01_09_17_50";
        public static bool AutoUpdate = false;
        public static bool LoadGUI = false;
        public static frm_MainWindow MainWindow = new frm_MainWindow();

        public static bool IsGameCreated = false;
        public static double GameID = 0;
        public static int LobbyPlayers = 0;
        public static string LobbyPassword = "Paruru";
        public static string LobbyOwner = "";
        public static string leader = "";
        private static bool gettoken = false;

        static void Main(string[] args)
        {
            InitChecks();
            loadVersion();
            Console.Title = "Volibot For Garena";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetWindowSize(Console.WindowWidth + 5, Console.WindowHeight);
            Console.WriteLine("=======================================");
            Console.WriteLine("Garena Volibot up-to-date for v" + cversion.Substring(0,4));
            Console.WriteLine("-----------by nongnoobjung-------------");   
            Console.WriteLine("=======================================");
            Console.WriteLine();
            WindowsIdentity winIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal winPrincipal = new WindowsPrincipal(winIdentity);
            if (!winPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine(getTimestamp() + "Error to Launch Volibot Make Sure Run As Admin");   
                return;
            }

            Console.WriteLine(getTimestamp() + "Loading config\\settings.ini");
            loadConfiguration();
            string herolist = "";
            foreach (string s in championId2)
            {
                herolist = herolist + s + ",";
            }
            herolist = herolist.Substring(0, herolist.Count() - 1);
            Console.WriteLine(getTimestamp() + "HeroList: " + herolist);
            Console.WriteLine(getTimestamp() + "StopHour: " + stopHour);
            if (replaceConfig)
            {
                Console.WriteLine(getTimestamp() + "Replacing Config");
                gamecfg();
            }
            while (!File.Exists(Path2 + "lol.exe"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Wrong LauncherPath. the path should look like this: C:\\GarenaLoL\\GameData\\Apps\\LoL\\ \n Please check config\\settings.ini, otherwise your LoL won't start.");
                Console.WriteLine();
                System.Threading.Thread.Sleep(5000);
                loadConfiguration();
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(getTimestamp() + "Loading config\\accounts.txt");
            ReloadAccounts:
            loadAccounts();
            int curRunning = 0;
            //if (LoadGUI) MainWindow.ShowDialog();

            if (!LoadGUI)
            {
                foreach (string acc in accounts)
                {
                    try
                    {
                        accounts2.RemoveAt(0);
                        string Accs = acc;
                        string[] stringSeparators = new string[] { "|" };
                        bool lead = false;
                        string token = "";
                        var result = Accs.Split(stringSeparators, StringSplitOptions.None);
                        curRunning += 1;
                        if (result[0].Contains("username"))
                        {
                            Console.WriteLine("Please add your accounts into config\\accounts.txt");
                            goto ReloadAccounts;
                        }
                       /* if (result[3].Contains("Leader") || result.Contains("leader"))
                        {
                            lead = true;
                        }*/
                        Console.WriteLine(getTimestamp()+"Bot "+curRunning+" Waiting Launch Game From Garena");
                        token = GetGarenaToken();
                        if (result[1] != null)
                        {
                            QueueTypes queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[1]);
                            RiotBot ritoBot = new RiotBot(result[0], token, Region, Path2, curRunning, queuetype, lead);
                        }
                        else
                        {
                            QueueTypes queuetype = QueueTypes.ARAM;
                            RiotBot ritoBot = new RiotBot(result[0], token, Region, Path2, curRunning, queuetype, lead);
                        }
                        Console.Title = "RitoBot Console | Currently connected: " + connectedAccs;
                        /*if (result[1] == "CUSTOM")
                        {
                            while (!Program.IsGameCreated)
                                System.Threading.Thread.Sleep(1000);
                        }*/

                        if (curRunning == maxBots)
                            break;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("CountAccError: You may have an issue in your accounts.txt");
                        Application.Exit();
                    }
                }
                Console.ReadKey();
            }
        }
        public static void loadVersion()
        {

            var versiontxt = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + @"config\\version.txt");
            cversion = versiontxt.ReadLine();               
        }
        public static void lognNewAccount()
        {
            accounts2 = accounts;
            accounts.RemoveAt(0);
            int curRunning = 0;
            if (accounts.Count == 0)
            {
                Console.WriteLine(getTimestamp() + "No more accounts to login.");
            }
            foreach (string acc in accounts)
            {
                string Accs = acc;
                string[] stringSeparators = new string[] { "|" };
                var result = Accs.Split(stringSeparators, StringSplitOptions.None);
                curRunning += 1;
                bool lead = false;
                string token = "";
              /*  if (result[3].Contains("Leader") || result.Contains("leader"))
                {
                    lead = true;
                }*/
                Console.WriteLine("Waiting Launch Game From Garena");
                token = GetGarenaToken();
                Console.WriteLine("Get Token!");
                if(result[1] != null)
                {
                    QueueTypes queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[1]);
                    RiotBot ritoBot = new RiotBot(result[0], token, Region, Path2, curRunning, queuetype, lead);
                } else {
                    QueueTypes queuetype = QueueTypes.ARAM;
                    RiotBot ritoBot = new RiotBot(result[0], token, Region, Path2, curRunning, queuetype, lead);
                }
                Console.Title = "RitoBot Console | Currently connected: " + connectedAccs;
                if (curRunning == maxBots)
                    break;
            }
        }
        public static void loadConfiguration()
        {
            try
            {
                IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "config\\settings.ini");
                //General
                Path2 = iniFile.IniReadValue("General", "LauncherPath");
                LoadGUI = Convert.ToBoolean(iniFile.IniReadValue("General", "LoadGUI"));
                maxBots = Convert.ToInt32(iniFile.IniReadValue("General", "MaxBots"));
                maxLevel = Convert.ToInt32(iniFile.IniReadValue("General", "MaxLevel"));
                stopHour = Convert.ToInt32(iniFile.IniReadValue("General", "StopHour"));
                championId = iniFile.IniReadValue("General", "ChampionPick").ToUpper();
                championId2 = new List<string>(iniFile.IniReadValue("General", "ChampionPick2").ToUpper().Split(','));
                spell1 = iniFile.IniReadValue("General", "Spell1").ToUpper();
                spell2 = iniFile.IniReadValue("General", "Spell2").ToUpper();
                rndSpell = Convert.ToBoolean(iniFile.IniReadValue("General", "RndSpell"));
                replaceConfig = Convert.ToBoolean(iniFile.IniReadValue("General", "ReplaceConfig"));
                AutoUpdate = Convert.ToBoolean(iniFile.IniReadValue("General", "AutoUpdate"));
                //Account
                Region = iniFile.IniReadValue("Account", "Region").ToUpper();
                buyBoost = Convert.ToBoolean(iniFile.IniReadValue("Account", "BuyBoost"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Thread.Sleep(10000);
                Application.Exit();
            }
        }
        public static void loadAccounts()
        {
            var accountsTxtPath = AppDomain.CurrentDomain.BaseDirectory + "config\\accounts.txt";
            TextReader tr = File.OpenText(accountsTxtPath);
            string line;
            while ((line = tr.ReadLine()) != null)
            {
                accounts.Add(line);
                accounts2.Add(line);
            }
            tr.Close();
        }
        public static String getTimestamp()
        {
           return "[" + DateTime.Now + "] ";
        }
        public static void getColor(ConsoleColor color)
        {
           Console.ForegroundColor = color;
        }
        public static void gamecfg()
        {
            try
            {

                string path = Path2 + @"Game\\Config\\game.cfg";
                FileInfo fileInfo = new FileInfo(path);
                fileInfo.IsReadOnly = false;
                fileInfo.Refresh();
                string str = "[General]\nGameMouseSpeed=9\nEnableAudio=0\nUserSetResolution=1\nBindSysKeys=0\nSnapCameraOnRespawn=1\nOSXMouseAcceleration=1\nAutoAcquireTarget=1\nEnableLightFx=0\nWindowMode=1\nShowTurretRangeIndicators=0\nPredictMovement=0\nWaitForVerticalSync=0\nColors=16\nHeight=200\nWidth=300\nSystemMouseSpeed=0\nCfgVersion=4.13.265\n\n[HUD]\nShowNeutralCamps=0\nDrawHealthBars=0\nAutoDisplayTarget=0\nMinimapMoveSelf=0\nItemShopPrevY=19\nItemShopPrevX=117\nShowAllChannelChat=0\nShowTimestamps=0\nObjectTooltips=0\nFlashScreenWhenDamaged=0\nNameTagDisplay=1\nShowChampionIndicator=0\nShowSummonerNames=0\nScrollSmoothingEnabled=0\nMiddleMouseScrollSpeed=0.5000\nMapScrollSpeed=0.5000\nShowAttackRadius=0\nNumericCooldownFormat=3\nSmartCastOnKeyRelease=0\nEnableLineMissileVis=0\nFlipMiniMap=0\nItemShopResizeHeight=47\nItemShopResizeWidth=455\nItemShopPrevResizeHeight=200\nItemShopPrevResizeWidth=300\nItemShopItemDisplayMode=1\nItemShopStartPane=1\n\n[Performance]\nShadowsEnabled=0\nEnableHUDAnimations=0\nPerPixelPointLighting=0\nEnableParticleOptimizations=0\nBudgetOverdrawAverage=10\nBudgetSkinnedVertexCount=10\nBudgetSkinnedDrawCallCount=10\nBudgetTextureUsage=10\nBudgetVertexCount=10\nBudgetTriangleCount=10\nBudgetDrawCallCount=1000\nEnableGrassSwaying=0\nEnableFXAA=0\nAdvancedShader=0\nFrameCapType=3\nGammaEnabled=1\nFull3DModeEnabled=0\nAutoPerformanceSettings=0\n=0\nEnvironmentQuality=0\nEffectsQuality=0\nShadowQuality=0\nGraphicsSlider=0\n\n[Volume]\nMasterVolume=1\nMusicMute=0\n\n[LossOfControl]\nShowSlows=0\n\n[ColorPalette]\nColorPalette=0\n\n[FloatingText]\nCountdown_Enabled=0\nEnemyTrueDamage_Enabled=0\nEnemyMagicalDamage_Enabled=0\nEnemyPhysicalDamage_Enabled=0\nTrueDamage_Enabled=0\nMagicalDamage_Enabled=0\nPhysicalDamage_Enabled=0\nScore_Enabled=0\nDisable_Enabled=0\nLevel_Enabled=0\nGold_Enabled=0\nDodge_Enabled=0\nHeal_Enabled=0\nSpecial_Enabled=0\nInvulnerable_Enabled=0\nDebug_Enabled=1\nAbsorbed_Enabled=1\nOMW_Enabled=1\nEnemyCritical_Enabled=0\nQuestComplete_Enabled=0\nQuestReceived_Enabled=0\nMagicCritical_Enabled=0\nCritical_Enabled=1\n\n[Replay]\nEnableHelpTip=0";
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(str);
                using (StreamWriter writer = new StreamWriter(Path2 + @"Game\\Config\game.cfg"))
                {
                    writer.Write(builder.ToString());
                }
                fileInfo.IsReadOnly = true;
                fileInfo.Refresh();
            }
            catch (Exception exception2)
            {
                Console.WriteLine("game.cfg Error: If using VMWare Shared Folder, make sure it is not set to Read-Only.\nException:" + exception2.Message);
            }
        }
        private static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
        private static void InitChecks()
        {
            var theFolder = AppDomain.CurrentDomain.BaseDirectory + @"config\\";
            var accountsTxtLocation = AppDomain.CurrentDomain.BaseDirectory + @"config\\accounts.txt";
            var configTxtLocation = AppDomain.CurrentDomain.BaseDirectory + @"config\\settings.ini";
            var versionTxtLocation = AppDomain.CurrentDomain.BaseDirectory + @"config\\version.txt";

            if (!Directory.Exists(theFolder))
            {
                Directory.CreateDirectory(theFolder);
            }

            if (!File.Exists(configTxtLocation))
            {
                
                var newfile = File.Create(configTxtLocation);
                newfile.Close();
                var content = "[General]\nLauncherPath=C:\\GarenaLoL\\GameData\\Apps\\LoL\\\nLoadGUI=false\nMaxBots=1\nMaxLevel=31\nChampionPick=Annie\nSpell1=Flash\nSpell2=Exhaust\nRndSpell=false\nReplaceConfig=false\nAutoUpdate=false\n\n[Account]\nRegion=TH\nBuyBoost=false\n\n[Custom]\nLobbyPassword=Paruru";
                var separator = new string[] { "\n" };
                string[] contentlines = content.Split(separator,StringSplitOptions.None);
                File.WriteAllLines(configTxtLocation, contentlines);
            }
            if (!File.Exists(versionTxtLocation))
            {
                var newfile = File.CreateText(versionTxtLocation);
                newfile.Close();
                string[] content = { cversion };
                File.WriteAllLines(versionTxtLocation, content);
            }
            if (!File.Exists(accountsTxtLocation))
            {
                var newfile = File.CreateText(accountsTxtLocation);
                newfile.Close();
                string[] content = { "username|QueueType" };
                File.WriteAllLines(accountsTxtLocation, content);
            }
        }


        private static string GetGarenaToken()
        {
           string s1 = "";
           bool token = false;
           do
           {
               foreach (var process in Process.GetProcessesByName("lol"))
               {
                   try
                   {

                       s1 = GetCommandLine(process);
                       foreach (var p1 in Process.GetProcessesByName("lolclient"))
                       {
                           p1.Kill();
                       }
                       process.Kill();
                       s1 = s1.Substring(1);
                       token = true;
                   }
                   catch (Win32Exception ex)
                   {
                       Console.WriteLine("Error Get Garena Token");
                       if ((uint)ex.ErrorCode != 0x80004005)
                       {
                           throw;
                       }
                   }
               }
           } while (!token);
            

            return s1;

        }

        private static string GetCommandLine(Process process)
        {
            var commandLine = new StringBuilder("");

            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                }
            }

            return commandLine.ToString();
        }

   }
}
