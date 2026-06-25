using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using activities_wpf.DataModel;
using Microsoft.Data.SqlClient;
using Xceed.Wpf.Toolkit;

namespace activities_wpf.Repositories
{
    // implementation of IRepo using Database
    public class ActivityRepoDB : IRepo
    {
        // variable to store the connection string
        private readonly string _conn;

        // constructor for ActivityRepoDB that takes a connection string
        public ActivityRepoDB(string connectionString) => _conn = connectionString;

        // method implementation to add an Entertainment activity
        public void AddEntertainmentActivity(List<EntertainmentActivity> a)
        {
            if (!CheckActivityExists(a))
            {
                // setup the connection
                using var conn = new SqlConnection(_conn);
                conn.Open();
                using var tx = conn.BeginTransaction(); // start to ensure insert

                // use the stored procedure to insert all the activities in the given list
                foreach (var activity in a)
                {
                    // setup the stored procedure
                    using var cmd = new SqlCommand("AddEntertainmentActivity", conn, tx);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // match the params of the procedure with the model
                    cmd.Parameters.Add(new SqlParameter("@DateStartTime", SqlDbType.DateTime) { Value = activity.DateStartTime });
                    cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50) { Value = activity.Title ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@Cost", SqlDbType.Decimal, 10) { Value = activity.Cost });
                    cmd.Parameters.Add(new SqlParameter("@MinParticipants", SqlDbType.Int) { Value = activity.MinParticipants });

                    // execute the procedure
                    cmd.ExecuteNonQuery();
                }
                // commit the transaction
                tx.Commit();
                MessageBox.Show("Activities successfully added.");
            }
            else
            {
                MessageBox.Show("An activity already exists at this date. Please select a different time.");
            }
        }

        // method implementation to add a Fitness activity
        public void AddFitnessActivity(List<FitnessActivity> a)
        {
            if (!CheckActivityExists(a))
            {
                //MessageBox.Show(CheckActivityExists(a).ToString());

                // setup the connection
                using var conn = new SqlConnection(_conn);
                conn.Open();
                using var tx = conn.BeginTransaction(); // start to ensure insert

                // use the stored procedure to insert all the activities in the given list
                foreach (var activity in a)
                {
                    // setup the stored procedure
                    using var cmd = new SqlCommand("AddFitnessActivity", conn, tx);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // match the params of the procedure with the model
                    cmd.Parameters.Add(new SqlParameter("@DateStartTime", SqlDbType.DateTime) { Value = activity.DateStartTime });
                    cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50) { Value = activity.Title ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@Cost", SqlDbType.Decimal, 10) { Value = activity.Cost });
                    cmd.Parameters.Add(new SqlParameter("@Location", SqlDbType.NVarChar, 100) { Value = activity.Location ?? (object)DBNull.Value });

                    // execute the procedure
                    cmd.ExecuteNonQuery();
                }
                // commit the transaction
                tx.Commit();
                MessageBox.Show("Activities successfully added.");
            }
            else
            {
                MessageBox.Show("An activity already exists at this date. Please select a different time.");
            }
        }

        // helper method to check for the existence of a fitness activity by date
        public bool CheckActivityExists(List<FitnessActivity> a)
        {
            // value to return
            bool activityExists = false;
            // setup the connection and the stored procedure
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("CheckActivityExistsByDate", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // open the connection
            conn.Open();

            // loop through the list of activities and check if a date is found
            foreach (var activity in a)
            {
                cmd.Parameters.AddWithValue("@DateToCheck", activity.DateStartTime);
                var result = cmd.ExecuteScalar();
                // this is 0 if no activity was found (0 rows returned)
                activityExists = Convert.ToInt32(result) == 1;
            }
            // return true if activity found, otherwise false
            return activityExists;
        }

        // overloaded helper method to check for the existence of an entertainment activity by date
        // should be combined with the above method if time allows
        public bool CheckActivityExists(List<EntertainmentActivity> a)
        {
            // value to return
            bool activityExists = false;
            // setup the connection and the stored procedure
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("CheckActivityExistsByDate", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // open the connection
            conn.Open();

            // loop through the list of activities and check if a date is found
            foreach (var activity in a)
            {
                cmd.Parameters.AddWithValue("@DateToCheck", activity.DateStartTime);
                var result = cmd.ExecuteScalar();
                // this is 0 if no activity was found (0 rows returned)
                activityExists = Convert.ToInt32(result) == 1;
            }
            // return true if activity found, otherwise false
            return activityExists;
        }

        // method implementation to returns all activities
        public List<DataModel.Activity> GetAllActivities()
        {
            // result list to return
            var results = new List<DataModel.Activity>();

            // setup the SQL connection and stored procedure
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("GetAllActivities", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // open the connection to the DB
            conn.Open();
            using var rdr = cmd.ExecuteReader();

            // process the read-in data
            while (rdr.Read())
            {
                // get the activity type so we can use the correct model to parse
                string activityType = rdr.GetString(rdr.GetOrdinal("ActivityType"));

                if (activityType == "Fitness")
                {
                    results.Add(new FitnessActivity
                    {
                        ActivityID = rdr.GetInt32(rdr.GetOrdinal("ActivityID")),
                        DateStartTime = rdr.GetDateTime(rdr.GetOrdinal("DateStartTime")),
                        Title = rdr.GetString(rdr.GetOrdinal("Title")),
                        Cost = rdr.GetDecimal(rdr.GetOrdinal("Cost")),
                        ActivityType = activityType,
                        Location = rdr.IsDBNull(rdr.GetOrdinal("Location"))
                                            ? ""
                                            : rdr.GetString(rdr.GetOrdinal("Location"))
                    });
                }
                else // Entertainment
                {
                    results.Add(new EntertainmentActivity
                    {
                        ActivityID = rdr.GetInt32(rdr.GetOrdinal("ActivityID")),
                        DateStartTime = rdr.GetDateTime(rdr.GetOrdinal("DateStartTime")),
                        Title = rdr.GetString(rdr.GetOrdinal("Title")),
                        Cost = rdr.GetDecimal(rdr.GetOrdinal("Cost")),
                        ActivityType = activityType,
                        MinParticipants = rdr.IsDBNull(rdr.GetOrdinal("MinParticipants"))
                                              ? 0
                                              : rdr.GetInt32(rdr.GetOrdinal("MinParticipants"))
                    });
                }
            }
            // return the list of activities
            return results;
        }

        // method implementation to search for an activity
        public List<DataModel.Activity> SearchActivity(string searchTerm, string searchTime)
        {
            //throw new NotImplementedException();
            // result list to return
            var results = new List<DataModel.Activity>();

            // setup the SQL connection and stored procedure
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("SearchActivitiesByDate", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // open the connection to the DB
            conn.Open();

            // format the searched dateTime so we only have the date part
            DateTime dt = DateTime.Parse(searchTerm);
            DateOnly dateOnly = DateOnly.FromDateTime(dt);
            // setup the params
            cmd.Parameters.AddWithValue("@SearchDate", dateOnly);
            cmd.Parameters.AddWithValue("@Operator", searchTime);

            using var rdr = cmd.ExecuteReader();

            // process the read-in data
            while (rdr.Read())
            {
                results.Add(new DataModel.Activity
                {
                    ActivityID = rdr.GetInt32(rdr.GetOrdinal("ActivityID")),
                    DateStartTime = rdr.GetDateTime(rdr.GetOrdinal("DateStartTime")),
                    Title = rdr.GetString(rdr.GetOrdinal("Title")),
                    Cost = rdr.GetDecimal(rdr.GetOrdinal("Cost")),
                });
            }
            // return the list of activities
            return results;
        }
    }
}
