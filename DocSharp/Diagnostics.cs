﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DocSharp
{
    /// <summary>
    /// Represents the kind of the diagnostic.
    /// </summary>
    public enum DiagnosticKind
    {
        Debug,
        Message,
        Warning,
        Error
    }

    /// <summary>
    /// Keeps information related to a single diagnostic.
    /// </summary>
    public struct DiagnosticInfo
    {
        public DiagnosticKind Kind;
        public string Message;
        public string File;
        public int Line;
        public int Column;
    }

    public interface IDiagnostics
    {
        void Emit(DiagnosticInfo info);
        void PushIndent(int level);
        void PopIndent();
    }

    public static class DiagnosticExtensions
    {
        public static void Debug(this IDiagnostics consumer,
            string msg, params object[] args)
        {
            var diagInfo = new DiagnosticInfo
            {
                Kind = DiagnosticKind.Debug,
                Message = string.Format(msg, args)
            };

            consumer.Emit(diagInfo);
        }

        public static void Message(this IDiagnostics consumer,
            string msg, params object[] args)
        {
            var diagInfo = new DiagnosticInfo
            {
                Kind = DiagnosticKind.Message,
                Message = string.Format(msg, args)
            };

            consumer.Emit(diagInfo);
        }

        public static void Warning(this IDiagnostics consumer,
            string msg, params object[] args)
        {
            var diagInfo = new DiagnosticInfo
            {
                Kind = DiagnosticKind.Warning,
                Message = string.Format(msg, args)
            };

            consumer.Emit(diagInfo);
        }

        public static void Error(this IDiagnostics consumer,
            string msg, params object[] args)
        {
            var diagInfo = new DiagnosticInfo
            {
                Kind = DiagnosticKind.Error,
                Message = string.Format(msg, args)
            };

            consumer.Emit(diagInfo);
        }
    }

    public class TextDiagnosticPrinter : IDiagnostics
    {
        public bool Verbose;
        public Stack<int> Indents;

        public TextDiagnosticPrinter()
        {
            Indents = new Stack<int>();
        }

        public void Emit(DiagnosticInfo info)
        {
            if (info.Kind == DiagnosticKind.Debug && !Verbose)
                return;

            var currentIndent = Indents.Sum();
            var message = new string(' ', currentIndent) + info.Message;

            Console.WriteLine(message);
            Debug.WriteLine(message);
        }

        public void PushIndent(int level)
        {
            Indents.Push(level);
        }

        public void PopIndent()
        {
            Indents.Pop();
        }
    }
}
