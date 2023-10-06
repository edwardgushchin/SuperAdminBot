namespace SuperAdminBot
{
	public static class Debug
	{
		public enum MessageStatus {
			INFO,
			WARN,
			FAIL,
			DEBUG
		}

		public enum Sender {
			Kernel,
			Settings,
			TGSubsystem,
			VKSubsystem,
			CacheSubsystem,
			CallbackOrder
		}

		public static void Message(string message)
		{
			Console.WriteLine(message);
		}

		private static string GetCurrentTime => $"[{DateTime.Now:HH:mm:ss}]";

		private static string GetCurrentDateLogPath => Path.Combine("logs", $"{DateTime.Now.Date:dd-MM-yy}.log");

		public static void Log(string message)
		{
			//if (!Debugger.IsAttached) return;
			
			Console.Write($"{GetCurrentTime} ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(message);
			Console.ResetColor();
			
			WriteInLogFile($"{GetCurrentTime} {message}");
		}

		public static void Log(string message, Sender sender)
		{
			//if (!Debugger.IsAttached) return;
			
			Console.Write($"{GetCurrentTime} [{sender}] ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(message);
			Console.ResetColor();
			
			WriteInLogFile($"{GetCurrentTime} [{sender}] {message}");
		}

		public static void Log(string message, MessageStatus status)
		{
			//if (!Debugger.IsAttached) return;
			
			Console.Write($"{GetCurrentTime} ");
			Console.ForegroundColor = status switch
			{
				MessageStatus.INFO => ConsoleColor.Green,
				MessageStatus.WARN => ConsoleColor.Yellow,
				MessageStatus.FAIL => ConsoleColor.Red,
				_ => Console.ForegroundColor
			};
			Console.WriteLine(message);
			Console.ResetColor();
			
			WriteInLogFile($"{GetCurrentTime} [{status}] {message}");
		}

		public static void Log(string message, Sender sender, MessageStatus status)
		{
			//if (!Debugger.IsAttached) return;
			
			Console.Write($"{GetCurrentTime} [{sender}] ");
			Console.ForegroundColor = status switch
			{
				MessageStatus.DEBUG => ConsoleColor.Green,
				MessageStatus.WARN => ConsoleColor.Yellow,
				MessageStatus.FAIL => ConsoleColor.Red,
				_ => Console.ForegroundColor
			};
			Console.WriteLine(message);
			Console.ResetColor();

			WriteInLogFile($"{GetCurrentTime} [{sender}] [{status}] {message}");
		}

		public static void NewLine()
		{
			Console.WriteLine();
		}

		private static void WriteInLogFile(string data)
		{
			try
			{
				using var file = new StreamWriter(GetCurrentDateLogPath, true);
				file.WriteLine(data);
			}
			catch (Exception e)
			{
				Log($"[WriteInLogFile] {e.Message}", Sender.Kernel, MessageStatus.FAIL);
			}
			
		}
	}
}