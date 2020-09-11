# SMHW.NET
An asynchronous .NET library for interacting with the Show My Homework API.

## Simple Implementation
```c#
using System;
using System.Threading.Tasks;
using SMHW.NET;

namespace SMHW.NET.Example
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			// Provides the credentials to the ctor of the SMHWClient.
			var client = new SMHWClient("email", "password", 0);

			// Hooks up events that the SMHW client automatically invokes.
			client.OnReady += OnReady;

			// Logs in to SMHW with the credentials provided in the ctor of the SMHWClient.
			await client.LoginAsync();

			// This basically allows for the asynchronous checking of homework,
			// and refreshing the token without the program ending.
			await Task.Delay(-1);
		}

		private static void OnReady(object sender, ReadyEventArgs e)
		{
			// Prints who you are logged in as.
			Console.WriteLine($"Logged in as: {e.Client.Profile.FirstName} {e.Client.Profile.Surname}\nYear: {e.Client.Profile.SchoolYear}\nGender: {char.ToUpper(e.Client.Profile.Gender[0]) + e.Client.Profile.Gender.Substring(1)}\nUser Type: {e.Client.Profile.UserType}");
		}
	}
}
```
I will hopefully be adding more events and information in the future, although for now, the current events are: `OnReady`, `OnHomeworkSet`, `OnHomeworkChanged`, `OnTokenRefreshed`. My next step for events would probably be to add some sort of `OnHomeworkRemoved` event.
