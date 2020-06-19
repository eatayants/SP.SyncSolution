using Microsoft.SharePoint.Administration;
using System;
using System.Diagnostics;

namespace SqlSync.Common.Helpers
{
	public sealed class LogHelper
	{
		#region Singleton

		private static readonly Lazy<LogHelper> LazyInstance = new Lazy<LogHelper>();
		public static LogHelper Instance
		{
			get { return LazyInstance.Value; }
		}

		public static LogHelper GetInstance()
		{
			return new LogHelper();
		}

		#endregion

		#region Logger


        public void Debug(string message, params object[] parameters)
		{
			Trace.WriteLine(string.Format(message, parameters));
		}


        public void Info(string message, params object[] parameters)
		{
			Trace.WriteLine(string.Format(message, parameters));
		}

        public void Warning(string message, params object[] parameters)
		{
            Trace.WriteLine(string.Format(message, parameters));
		}

		public void Error(string message, Exception ex)
		{
			if (ex == null)
			{
				Trace.WriteLine(message);
			}
			else
			{
                var arg = string.Format("{0} {4} Message: {1} {4} Type: {3}{4} SrackTrace: {2}{4}", message,
				                        ex.GetFullMessage(), ex.StackTrace, ex.GetType(), Environment.NewLine);
				Trace.WriteLine(string.Format(message, arg));
			}
		}

        public void ErrorULS(string message, Exception ex)
        {
            if (ex == null)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SqlSync.SP", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, message, null);
            }
            else
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SqlSync.SP", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected,
                    ex.GetFullMessage() + "; StackTrace: " + ex.StackTrace, null);
            }
        }

        public void InfoULS(string message)
        {
            SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SqlSync.SP", TraceSeverity.Monitorable, EventSeverity.Information), TraceSeverity.Monitorable, message, null);
        }

        public void Error(string message)
		{
            Error(message, null);
		}

		#endregion Logger
	}
}