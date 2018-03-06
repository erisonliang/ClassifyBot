﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using Serilog;
using Serilog.Enrichers;
using Serilog.Sinks;

namespace ClassifyBot
{
    public static class ConsoleDriver
    {
        static ConsoleDriver()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        #region Properties
        static Dictionary<string, string> AppConfig { get; set; }
        static LoggerConfiguration LoggerConfiguration { get; set; }
        static ILogger L;
        static bool ExitToEnvironment { get; set; }
        public static bool LogToRollingFile { get; set; }
        public static bool LogToConsole { get; set; } = true;
        public static bool Debug { get; set; } = true;
        #endregion

        #region Methods
        public static void RunAndExit(string[] args)
        {
            ExitToEnvironment = true;
            LoggerConfiguration = new LoggerConfiguration();
            if (Debug)
            {
                LoggerConfiguration = LoggerConfiguration.MinimumLevel.Debug();
            }
            if (LogToConsole)
            {
                LoggerConfiguration = LoggerConfiguration.WriteTo.Console();
            }
            if (LogToRollingFile)
            {
                LoggerConfiguration = LoggerConfiguration.WriteTo.RollingFile(Path.Combine("logs", "ClassifyBot") + "-{Date}.log");
            }
            Log.Logger = LoggerConfiguration.CreateLogger();
            L = Log.ForContext<Stage>();
            Stage s = Driver.MarshalOptionsForStage(args, out StageResult result, out string optionsHelp);
            if (result == StageResult.INIT && s != null)
            {
                s.Run();
            }
            else if (result == StageResult.INVALID_OPTIONS && s == null && !optionsHelp.Empty())
            {
                L.Information(optionsHelp);
            }
            else
            {
                throw new Exception("Unknown stage state {0} {1}.".F( result, s));
            }
        }

        static void Exit(ExitResult result)
        {
            Log.CloseAndFlush();
            Environment.Exit((int)result);
        }
        #endregion

        #region Event handlers
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            Log.Error(exception, "An unhandled runtime exception occurred.");
            if (ExitToEnvironment)
            {
                Exit(ExitResult.UNHANDLED_RUNTIME_EXCEPTION);
            }
        }
        #endregion
    }
}
