using System;
using System.Collections.Generic;
using System.Text;

namespace SMHW.NET.Objects
{
	public class SMHWProfile
	{
		public string Username { get; internal set; }
		public int SchoolId { get; internal set; }
		public string SMHWToken { get; internal set; }
		public string AccessToken { get; internal set; }
		public string RefreshToken { get; internal set; }
		public int TokenExpiresIn { get; internal set; }
		public string UserType { get; internal set; }
		public int StudentId { get; internal set; }
		public string Avatar { get; internal set; }
		public string FirstName { get; internal set; }
		public string Surname { get; internal set; }
		public string SchoolYear { get; internal set; }
		public string Gender { get; internal set; }
		public string SimsId { get; internal set; }
		public int[] ClassGroupIds { get; internal set; }
	}
}
