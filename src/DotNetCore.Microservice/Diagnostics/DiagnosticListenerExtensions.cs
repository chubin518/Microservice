using DotNetCore.Microservice.Owin;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCore.Microservice.Diagnostics
{
    public static class DiagnosticListenerExtensions
    {
        public const string DiagnosticListenerName = "DotNetCore.Microservice";

        public const string DiagnosticHostingBeginRequest = "DotNetCore.Microservice.Hosting.BeginRequest";
        public const string DiagnosticHostingEndRequest = "DotNetCore.Microservice.Hosting.EndRequest";
        public const string DiagnosticHostingUnhandledException = "DotNetCore.Microservice.Hosting.UnhandledException";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeginRequest(this DiagnosticListener listener, OwinContext context)
        {
            if (listener.IsEnabled(DiagnosticHostingBeginRequest))
            {
                listener.Write(DiagnosticHostingBeginRequest, new
                {
                    Context = context,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndRequest(this DiagnosticListener listener, OwinContext context)
        {
            if (listener.IsEnabled(DiagnosticHostingEndRequest))
            {
                listener.Write(DiagnosticHostingEndRequest, new
                {
                    Context = context,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnhandledException(this DiagnosticListener listener, OwinContext context, Exception exception)
        {
            if (listener.IsEnabled(DiagnosticHostingUnhandledException))
            {
                listener.Write(DiagnosticHostingUnhandledException, new
                {
                    Context = context,
                    Exception = exception,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }
    }
}
