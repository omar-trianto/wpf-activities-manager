using System;
using System.Collections.Generic;
using System.Text;
using activities_wpf.DataModel;

namespace activities_wpf.Repositories
{
    public interface IRepo
    {

        // returns all activities
        List<Activity> GetAllActivities();

        // search for an activity
        List<Activity> SearchActivity(string searchTerm, string searchTime);

        // add an Entertainment activity
        void AddEntertainmentActivity(List<EntertainmentActivity> a);

        // add a Fitness activity
        void AddFitnessActivity(List<FitnessActivity> a);
    }
}
