using System.Text;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using activities_wpf.DataModel;
using activities_wpf.Repositories;
using Xceed.Wpf.Toolkit;

namespace activities_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // IRepo object to interface with the system logic
        private readonly IRepo _repo;

        public MainWindow()
        {
            InitializeComponent();

            // create a new connection string to connect to the database with
            string connStr = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=ActivitiesDb;Integrated Security=True;Trust Server Certificate=True";

            //// create a new interface to work with CSV on program start
            //_repo = new ActivityRepoCSV();

            // create a new interface to work with Database on program start
            _repo = new ActivityRepoDB(connStr);
        }

        // button to retrieve all activities and display them in the datagrid
        private void DisplayAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgActivities.ItemsSource = _repo.GetAllActivities();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Could not load activities:\n{ex.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // button to search for an activity given the criteria selected on the radio button group
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (searchBefore.IsChecked == true)
                {
                    //System.Windows.MessageBox.Show("Before");
                    dgActivities.ItemsSource = _repo.SearchActivity(searchDate.Text, "before");
                }
                else if (searchOn.IsChecked == true)
                {
                    //System.Windows.MessageBox.Show("On");
                    dgActivities.ItemsSource = _repo.SearchActivity(searchDate.Text, "on");
                }
                else if (searchAfter.IsChecked == true)
                {
                    //System.Windows.MessageBox.Show("After");
                    dgActivities.ItemsSource = _repo.SearchActivity(searchDate.Text, "after");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Could not search activities:\n{ex.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // button to add a new Entertainment activity
        private void saveEntertainment_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show($"{addDate.Value}\n{addTitle.Text}\n{addCost.Text}\n{addParticipants.Text}");
            var recordsToAdd = new List<EntertainmentActivity>();
            var newRecord = new EntertainmentActivity();

            // fill in the parameters using the data typed in
            // TODO: add input validation
            try
            {
                newRecord.DateStartTime = DateTime.Parse(addDate.Text);
                newRecord.Title = addTitle.Text;
                newRecord.Cost = decimal.Parse(addCost.Text);
                newRecord.MinParticipants = int.Parse(addParticipants.Text);
                recordsToAdd.Add(newRecord);
            
                _repo.AddEntertainmentActivity(recordsToAdd);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Could not add activities:\n{ex.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // button to add a new Fitness activity
        private void saveFitness_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show($"{addDate.Value}\n{addTitle.Text}\n{addCost.Text}\n{addLocation.Text}");
            var recordsToAdd = new List<FitnessActivity>();
            var newRecord = new FitnessActivity();

            // fill in the parameters using the data typed in
            // TODO: add input validation
            try
            {
                newRecord.DateStartTime = DateTime.Parse(addDate.Text);
                newRecord.Title = addTitle.Text;
                newRecord.Cost = decimal.Parse(addCost.Text);
                newRecord.Location = addLocation.Text;
                recordsToAdd.Add(newRecord);

                _repo.AddFitnessActivity(recordsToAdd);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Could not add activities:\n{ex.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}