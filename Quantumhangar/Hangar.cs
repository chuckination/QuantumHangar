﻿using NLog;
using QuantumHangar.HangarMarket;
using QuantumHangar.UI;
using QuantumHangar.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers;
using Torch.Managers.PatchManager;
using Torch.Session;

namespace QuantumHangar
{
    public class Hangar : TorchPluginBase, IWpfPlugin
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static Settings Config => _config?.Data;
        private static Persistent<Settings> _config;

        public static Dictionary<long, CurrentCooldown> ConfirmationsMap { get; } =
            new Dictionary<long, CurrentCooldown>();


        public TorchSessionManager TorchSession { get; private set; }
        public static bool ServerRunning { get; private set; }

        public static string MainPlayerDirectory { get; private set; }


        public enum ErrorType
        {
            Debug,
            Fatal,
            Trace,
            Warn
        }


        public UserControl Control;

        public UserControl GetControl()
        {
            return Control ??= new UserControlInterface();
        }

        private HangarMarketController _controller;


        public override void Init(ITorchBase torch)
        {
            //Settings S = new Settings();

            base.Init(torch);
            //Grab Settings
            var path = Path.Combine(StoragePath, "QuantumHangar.cfg");

            _config = Persistent<Settings>.Load(path);

            if (string.IsNullOrEmpty(Config.FolderDirectory))
            {
                Config.FolderDirectory = Path.Combine(StoragePath, "QuantumHangar");
                Directory.CreateDirectory(Config.FolderDirectory);
            }

            MainPlayerDirectory = Path.Combine(Config.FolderDirectory, "PlayerHangars");
            Directory.CreateDirectory(MainPlayerDirectory);


            TorchSession = Torch.Managers.GetManager<TorchSessionManager>();
            if (TorchSession != null)
                TorchSession.SessionStateChanged += SessionChanged;


            if (Config.GridMarketEnabled)
                _controller = new HangarMarketController();
            else
                Log.Info("Starting plugin WITHOUT the Hangar Market!");

            MigrateHangar();
        }

        private void MigrateHangar()
        {
            var dirs = Directory.GetDirectories(Config.FolderDirectory, "*", SearchOption.TopDirectoryOnly);

            foreach (var dir in dirs)
            {
                var info = new DirectoryInfo(dir);

                if (!ulong.TryParse(info.Name, out _)) continue;
                Log.Warn("Moving!");

                var destination = Path.Combine(MainPlayerDirectory, info.Name);
                Log.Warn($"Destination: {destination}");

                Directory.Move(dir, destination);
            }
        }


        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loading:
                    Config.RefreshModel();
                    break;

                case TorchSessionState.Loaded:

                    //MP = Torch.CurrentSession.Managers.GetManager<MultiplayerManagerBase>();
                    //ChatManager = Torch.CurrentSession.Managers.GetManager<ChatManagerServer>();
                    var plugins = Torch.CurrentSession.Managers.GetManager<PluginManager>();
                    PluginDependencies.InitPluginDependencies(plugins);
                    ServerRunning = true;
                    AutoHangar.StartAutoHangar();
                    _controller?.ServerStarted();
                    break;


                case TorchSessionState.Unloading:
                    ServerRunning = false;
                    PluginDispose();
                    break;
            }
        }

        // 60 frames =~ 1 sec, run update about every min
        private readonly int _maxUpdateTime = 60 * 60;
        private int _currentFrameCount = 0;

        public override void Update()
        {
            if (_currentFrameCount >= _maxUpdateTime)
            {
                Update1Min();
                _currentFrameCount = 0;
                return;
            }

            _currentFrameCount++;
        }

        public void Update1Min()
        {
            AutoHangar.UpdateAutoHangar();
        }


        public void PluginDispose()
        {
            _controller?.Close();
            AutoHangar.Dispose();
            PluginDependencies.Dispose();
        }
    }


    public class CurrentCooldown
    {
        private long _startTime;
        //private long _currentCooldown;

        private string _grid;

        public void StartCooldown(string command)
        {
            _grid = command;
            _startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public bool CheckCommandStatus(string command)
        {
            if (_grid != command)
                return true;

            var elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _startTime;

            return elapsedTime >= 30000;
        }
    }
}