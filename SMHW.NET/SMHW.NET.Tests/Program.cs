using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SMHW.NET;
using SMHW.NET.Objects;

namespace SMHW.NET.Tests
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			// Provides the credentials to the ctor of the SMHWClient.
			var client = new SMHWClient("email", "password", 0 /*School Id*/);

			// Hooks up events that the SMHW client automatically invokes.
			client.OnReady += OnReady;
			client.OnHomeworkSet += OnHomeworkSet;
			client.OnHomeworkChanged += OnHomeworkChanged;
			client.OnTokenRefreshed += OnTokenRefreshed;

			// Logs in to SMHW with the credentials provided in the ctor of the SMHWClient.
			await client.LoginAsync();

			// This basically allows for the asynchronous checking of homework,
			// and refreshing the token without the program ending.
			await Task.Delay(-1);
		}

		private static void OnHomeworkChanged(object sender, HomeworkChangedEventArgs e)
		{
			// Does a simple comparison on Before.Completed vs After.Completed.
			// This tells us if the completion has changed state, e.g. me ticking it off.
			Console.WriteLine($"Before --> Completed: {e.Before.Completed}\nAfter --> Completed: {e.After.Completed}");
		}

		private static void OnTokenRefreshed(object sender, TokenRefreshedEventArgs e)
		{
			// Dispatched when the token was refreshed.
			Console.WriteLine("Token was refreshed :)");
		}

		private static void OnHomeworkSet(object sender, HomeworkSetEventArgs e)
		{
			// Prints basic information about the homework.
			Console.WriteLine($"Title: {e.Homework.Title}\nClass: {e.Homework.GroupName}\nSubject: {e.Homework.Subject}\nTeacher: {e.Homework.TeacherName}\nURL: {e.Homework.Url}");
		}

		private static void OnReady(object sender, ReadyEventArgs e)
		{
			// Prints who you are logged in as.
			Console.WriteLine($"Logged in as: {e.Client.Profile.FirstName} {e.Client.Profile.Surname}\nYear: {e.Client.Profile.SchoolYear}\nGender: {char.ToUpper(e.Client.Profile.Gender[0]) + e.Client.Profile.Gender.Substring(1)}\nUser Type: {e.Client.Profile.UserType}");
		}
	}
}
