using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Web;
using System.Web.UI;
using MySql.Data.MySqlClient;
using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.Data;

namespace MySqlWebProfilingDemo
{
	public partial class Default : Page
	{
		protected override void OnLoad (EventArgs e)
		{
			if (!this.IsPostBack)
			{
				using (var conn = CreateConnection())
				{
					conn.Open();
					
					using (var cmd = conn.CreateCommand())
					{
						cmd.Connection = conn;

						// warm up
						cmd.CommandText = "SELECT 1 FROM world.city limit 1;";
						cmd.ExecuteScalar();

						// load actual data
						cmd.CommandText = "SELECT * FROM world.city limit 5000;";
						using (var sda = CreateDataAdapter())
						{
							sda.SelectCommand = cmd;
							using (DataTable dt = new DataTable())
							{
								sda.Fill(dt);
								GridView1.DataSource = dt;
								GridView1.DataBind();
							}
						}
					}
				}
			}
		}

		private DbConnection CreateConnection()
		{
			string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
			var mySqlConn = new MySqlConnection(constr);

			var profilingSession = ProfilingSession.Current;
			return profilingSession == null ?
				(DbConnection)mySqlConn
				:
				new ProfiledDbConnection(mySqlConn, new DbProfiler(profilingSession.Profiler));
		}

		private DbDataAdapter CreateDataAdapter()
		{
			return new MySqlDataAdapter();
		}
	}
}