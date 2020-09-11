using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using SMHW.NET.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SMHW.NET
{
	public class ReadyEventArgs : EventArgs
	{
		public SMHWClient Client { get; set; }
	}

	public class HomeworkSetEventArgs : EventArgs
	{
		public SMHWClient Client { get; set; }
		public SMHWHomework Homework { get; set; }
	}

	public class TokenRefreshedEventArgs : EventArgs
	{
		public SMHWClient Client { get; set; }
		public SMHWProfile Before { get; set; }
		public SMHWProfile After { get; set; }
	}

	public class HomeworkChangedEventArgs : EventArgs
	{
		public SMHWClient Client { get; set; }
		public SMHWHomework Before { get; set; }
		public SMHWHomework After { get; set; }
	}

	public class SMHWClient
	{
		public event EventHandler<ReadyEventArgs> OnReady;
		public event EventHandler<HomeworkSetEventArgs> OnHomeworkSet;
		public event EventHandler<TokenRefreshedEventArgs> OnTokenRefreshed;
		public event EventHandler<HomeworkChangedEventArgs> OnHomeworkChanged;
		protected virtual void OnReadyInvoke(ReadyEventArgs e)
		{
			EventHandler<ReadyEventArgs> handler = OnReady;
			if (handler != null)
				handler(this, e);
		}
		protected virtual void OnTokenRefreshedInvoke(TokenRefreshedEventArgs e)
		{
			EventHandler<TokenRefreshedEventArgs> handler = OnTokenRefreshed;
			if (handler != null)
				handler(this, e);
		}
		protected virtual void OnHomeworkChangedInvoke(HomeworkChangedEventArgs e)
		{
			EventHandler<HomeworkChangedEventArgs> handler = OnHomeworkChanged;
			if (handler != null)
				handler(this, e);
		}
		protected virtual void OnHomeworkSetInvoke(HomeworkSetEventArgs e)
		{
			EventHandler<HomeworkSetEventArgs> handler = OnHomeworkSet;
			if (handler != null)
				handler(this, e);
		}

		public SMHWProfile Profile;
		private bool loggedIn = false;
		private const string productionUrl = "https://api.showmyhomework.co.uk";
		private const string clientId = "55283c8c45d97ffd88eb9f87e13f390675c75d22b4f2085f43b0d7355c1f";
		private const string clientSecret = "c8f7d8fcd0746adc50278bc89ed6f004402acbbf4335d3cb12d6ac6497d3";
		private readonly HttpClient Client = new HttpClient();

		private readonly string Username;
		private readonly string Password;
		private readonly int schoolId;

		public SMHWClient(string username, string password, int schoolid)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
				throw new ArgumentException("A username or password cannot be null, empty or whitespace.");

			Username = username;
			Password = password;
			schoolId = schoolid;
		}

		public async Task LoginAsync()
		{
			var formContent = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("grant_type", "password"),
				new KeyValuePair<string, string>("username", Username),
				new KeyValuePair<string, string>("password", Password),
				new KeyValuePair<string, string>("school_id", schoolId.ToString())
			});
			var oauthRequest = await Client.PostAsync($"{productionUrl}/oauth/token?client_id={clientId}&client_secret={clientSecret}", formContent);
			if (!oauthRequest.IsSuccessStatusCode)
				throw new Exception("Could not login. One of the following is most likely incorrect: Username, Password or School Id!");
			var oauthResponseJson = JObject.Parse(await oauthRequest.Content.ReadAsStringAsync());
			Profile = new SMHWProfile() { Username = Username, SchoolId = schoolId, SMHWToken = oauthResponseJson["smhw_token"].ToString(), AccessToken = oauthResponseJson["access_token"].ToString(), RefreshToken = oauthResponseJson["refresh_token"].ToString(), TokenExpiresIn = int.Parse(oauthResponseJson["expires_in"].ToString()), UserType = oauthResponseJson["user_type"].ToString() };
			await GetProfileDataAsync();
			loggedIn = true;
			OnReadyInvoke(new ReadyEventArgs() { Client = this });
			await CheckForHomework();
			await RefreshToken();
		}

		private async Task GetProfileDataAsync()
		{
			Client.SetHeaders(Profile.SMHWToken);
			var profileDataRequest = await Client.GetAsync($"{productionUrl}/api/students");
			if (!profileDataRequest.IsSuccessStatusCode)
				throw new Exception("Could not get the profile data of the student.");
			var profileDataJson = JObject.Parse(await profileDataRequest.Content.ReadAsStringAsync());
			var studentJson = profileDataJson["students"][0];
			Profile.StudentId = int.Parse(studentJson["id"].ToString());
			Profile.Avatar = studentJson["avatar"].ToString();
			Profile.FirstName = studentJson["forename"].ToString();
			Profile.Surname = studentJson["surname"].ToString();
			Profile.SchoolYear = studentJson["year"].ToString();
			Profile.Gender = studentJson["gender"].ToString();
			Profile.SimsId = studentJson["sims_id"].ToString();
			Profile.ClassGroupIds = studentJson["class_group_ids"].Select(x => (int)x).ToArray();
		}

		private Task CheckForHomework()
		{
			var _ = Task.Run(async () =>
			{
				Client.SetHeaders(Profile.SMHWToken);
				var homeworkRequest = await Client.GetAsync($"{productionUrl}/api/todos");
				if (!homeworkRequest.IsSuccessStatusCode)
					throw new Exception("Could not get the list of homework for the student.");
				var homeworks = (JArray)JObject.Parse(await homeworkRequest.Content.ReadAsStringAsync())["todos"];
				while (true)
				{
					// If the user is logged in, it will check for homework every 5 seconds.
					if (loggedIn)
					{
						Client.SetHeaders(Profile.SMHWToken);
						homeworkRequest = await Client.GetAsync($"{productionUrl}/api/todos");
						if (!homeworkRequest.IsSuccessStatusCode)
							throw new Exception("Could not get the list of homework for the student.");
						var homeworkJson = (JArray)JObject.Parse(await homeworkRequest.Content.ReadAsStringAsync())["todos"];
						// If a new homework has been added to TODOS.
						if (homeworkJson.Count() - homeworks.Count() >= 1)
						{
							var homeworksAdded = homeworkJson.Where(x => !homeworks.Any(y => int.Parse(x["class_task_id"].ToString()) == int.Parse(y["class_task_id"].ToString())));
							foreach (var HomeworkSet in homeworksAdded)
							{
								OnHomeworkSetInvoke(new HomeworkSetEventArgs() { Client = this, Homework = new SMHWHomework((JObject)HomeworkSet) });
							}
						}
						// If an old homework has been removed from TODOS.
						else if (homeworks.Count() - homeworkJson.Count() >= 1)
						{

						}
						// If a homework has been modified.
						else if (homeworks.ToString() != homeworkJson.ToString())
						{
							foreach (var (homework, index) in homeworks.OrderBy(x => x["id"].ToObject<int>()).Select((x, i) => (x, i)))
							{
								if (homework.ToString() != ((JObject)homeworkJson.OrderBy(x => x["id"].ToObject<int>()).ToArray()[index]).ToString())
								{
									// This homework was modified.
									OnHomeworkChangedInvoke(new HomeworkChangedEventArgs() { Client = this, Before = new SMHWHomework((JObject)homework), After = new SMHWHomework((JObject)homeworkJson.OrderBy(x => x["id"].ToObject<int>()).ToArray()[index]) });
								}
							}
						}
						homeworks = homeworkJson;
					}
					await Task.Delay(5 * 1000);
				}
			});
			return Task.CompletedTask;
		}


		private Task RefreshToken()
		{
			var _ = Task.Run(async () =>
			{

				// If the user is logged in, it will wait until the token needs refreshing.
				// When the token is refreshed, this loop then reiterates and waits until it needs refreshing again.
				if (loggedIn)
				{
					// Waits until 30 seconds before the token expiry time.
					await SMHWUtilities.LongDelay((long)Profile.TokenExpiresIn * 1000 - 30);
					var formContent = new FormUrlEncodedContent(new[]
					{
							new KeyValuePair<string, string>("grant_type", "refresh_token"),
							new KeyValuePair<string, string>("refresh_token", Profile.RefreshToken),
							new KeyValuePair<string, string>("school_id", schoolId.ToString())
						});
					Client.SetHeadersNoAuth();
					var oauthRequest = await Client.PostAsync($"{productionUrl}/oauth/token?client_id={clientId}&client_secret={clientSecret}", formContent);
					if (!oauthRequest.IsSuccessStatusCode)
						throw new Exception("Could not refresh the token for some reason.");
					var oauthResponseJson = JObject.Parse(await oauthRequest.Content.ReadAsStringAsync());
					SMHWProfile Before = Profile;
					Profile.SMHWToken = oauthResponseJson["smhw_token"].ToString();
					Profile.AccessToken = oauthResponseJson["access_token"].ToString();
					Profile.RefreshToken = oauthResponseJson["refresh_token"].ToString();
					Profile.TokenExpiresIn = oauthResponseJson["expires_in"].ToObject<int>();
					OnTokenRefreshedInvoke(new TokenRefreshedEventArgs() { Client = this, Before = Before, After = Profile });
				}

			});
			return Task.CompletedTask;
		}
	}
}
