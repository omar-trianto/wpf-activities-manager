using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using activities_wpf.DataModel;
using activities_wpf.Mapper;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace activities_wpf.Repositories
{
    // implementation of IRepo using CSV
    public class ActivityRepoCSV : IRepo
    {
        // method implementation to add an Entertainment activity
        public void AddEntertainmentActivity(List<EntertainmentActivity> a)
        {
            string filePath = "EntertainmentActivities.CSV";
            // use AddCSVHelper to enter a new entry into the given CSV
            AddCSVHelper<EntertainmentActivity, EntertainmentActivityMapper>(filePath, a);
        }

        // method implementation to add a Fitness activity
        public void AddFitnessActivity(List<FitnessActivity> a)
        {
            string filePath = "FitnessActivities.CSV";
            // use AddCSVHelper to enter a new entry into the given CSV
            AddCSVHelper<FitnessActivity, FitnessActivityMapper>(filePath, a);
        }

        // method implementation to returns all activities
        public List<Activity> GetAllActivities()
        {
            var activities = new List<Activity>();
            // we call GetCSVHelper twice because there are 2 separate source files
            activities.AddRange(GetCSVHelper("EntertainmentActivities.CSV"));
            activities.AddRange(GetCSVHelper("FitnessActivities.CSV"));
            // return a list containing all activities from both CSVs
            return activities;
        }

        // helper method to read data from CSV
        private List<Activity> GetCSVHelper(string filePath)
        {
            // error-handling to check if file exists
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"CSV file not found: {filePath}");

            // configuration for CsvHelper; HasHeaderRecord is set to true since we know the CSV has a header
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            // grab reader file path
            using var reader = new StreamReader(filePath);
            // convert to CSV over reader
            var csv = new CsvReader(reader, config);

            // auto map to Activity data from CSV file and set it to list
            return csv.GetRecords<Activity>().ToList();
        }

        // helper method to add data into CSV
        // we make it generic so it works for both Fitness and Entertainment
        public static void AddCSVHelper<T, TMap>(string filePath, List<T> a) 
            where TMap : ClassMap
        {
            // configuration for CsvHelper; HasHeaderRecord set to false to avoid duplicating header when writing
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };

            using (var writer = new StreamWriter(filePath, append: true))
            using (var csv = new CsvWriter(writer, config))
            {
                // write our objects to the CSV in append mode using the corresponding mapper
                csv.Context.RegisterClassMap(Activator.CreateInstance<TMap>());
                csv.WriteRecords(a);
            }

        }

        // method implementation to search for an activity
        public List<Activity> SearchActivity(string searchTerm, string searchTime)
        {
            // first we need a list of all the activities to search through, and an empty list to return later
            var allActivities = GetAllActivities();
            var matchingActivities = new List<Activity>();

            // set the format for DateTime conversion since we know the format of the CSV
            string format = "dd/MM/yyyy HH:mm";
            // convert the searched date into DateTime
            DateTime searchedDate = DateTime.ParseExact(searchTerm, format, CultureInfo.InvariantCulture);

            // loop through and search through all the activities
            foreach (var activity in allActivities) {
                // convert the current activity's date into DateTime as well to allow for manipulation
                //DateTime activityDate = DateTime.ParseExact(activity.DateStartTime, format, CultureInfo.InvariantCulture);
                DateTime activityDate = activity.DateStartTime;

                // perform different search logic depending on what the radio button is set to (before, on, or after)
                if (searchTime == "before")
                {
                    if (activityDate < searchedDate)
                    {
                        matchingActivities.Add(activity);
                    }
                }
                else if (searchTime == "on")
                {
                    if (activityDate == searchedDate)
                    {
                        matchingActivities.Add(activity);
                    }
                }
                else if (searchTime == "after")
                {
                    if (activityDate > searchedDate)
                    {
                        matchingActivities.Add(activity);
                    }
                }
            }
            // return a list with all activities that matches the search criteria, if any
            return matchingActivities;
        }
    }
}
    
