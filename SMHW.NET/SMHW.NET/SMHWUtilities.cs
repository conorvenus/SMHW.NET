using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SMHW.NET
{
	internal static class SMHWUtilities
	{
		internal static async Task LongDelay(long delay)
		{
			while (delay > 0)
			{
				var currentDelay = delay > int.MaxValue ? int.MaxValue : (int)delay;
				await Task.Delay(currentDelay);
				delay -= currentDelay;
			}
		}

		internal static void SetHeaders(this HttpClient Client, string smhwToken)
		{
			Client.DefaultRequestHeaders.Clear();
			Client.DefaultRequestHeaders.Add("Accept", "application/smhw.v3+json");
			Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {smhwToken}");
		}

		internal static void SetHeadersNoAuth(this HttpClient Client)
		{
			Client.DefaultRequestHeaders.Clear();
			Client.DefaultRequestHeaders.Add("Accept", "application/smhw.v3+json");
		}
	}
}
