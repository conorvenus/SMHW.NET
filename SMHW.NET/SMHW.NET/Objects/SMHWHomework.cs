using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMHW.NET.Objects
{
	public class SMHWHomework
	{
		private readonly JObject HomeworkJson;

		public bool Completed { get; private set; }
		public string Title { get; private set; }
		public string Type { get; private set; }
		public int Id { get; private set; }
		public string Description { get; private set; }
		public string GroupName { get; private set; }
		public string Subject { get; private set; }
		public string TeacherName { get; private set; }
		public string DueOn { get; private set; }
		public string IssuedOn { get; private set; }
		public string Url => "https://www.showmyhomework.co.uk/homeworks/" + Id.ToString();

		public SMHWHomework(JObject homeworkJson)
		{
			HomeworkJson = homeworkJson;
			SetHomework();
		}
		private void SetHomework()
		{
			DueOn = HomeworkJson["due_on"].ToString();
			Completed = HomeworkJson["completed"].ToObject<bool>();
			Description = HomeworkJson["class_task_description"].ToString();
			IssuedOn = HomeworkJson["issued_on"].ToString();
			Id = HomeworkJson["id"].ToObject<int>();
			TeacherName = HomeworkJson["teacher_name"].ToString();
			Subject = HomeworkJson["subject"].ToString();
			Title = HomeworkJson["class_task_title"].ToString();
			GroupName = HomeworkJson["class_group_name"].ToString();
		}
	}
}
