using DocumentFormat.OpenXml.Wordprocessing;
using Google.Cloud.Firestore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Toolbox_Class_Library.CtrUpdate;
using Toolbox_Class_Library.Firestore;

namespace Toolbox_Class_Library
{
    public class DatabaseConnection
    {
        private FirestoreDb _db;

        public DatabaseConnection()
        {
            _db = FirestoreService.GetFirestoreDb();
        }

        // Helper Functions:
        
        private int DetermineMonth(string monthString)
        {
            int month = 0;
            switch (monthString)
            {
                case "January":
                    month = 1;
                    break;
                case "February":
                    month = 2;
                    break;
                case "March":
                    month = 3;
                    break;
                case "April":
                    month = 4;
                    break;
                case "May":
                    month = 5;
                    break;
                case "June":
                    month = 6;
                    break;
                case "July":
                    month = 7;
                    break;
                case "August":
                    month = 8;
                    break;
                case "September":
                    month = 9;
                    break;
                case "October":
                    month = 10;
                    break;
                case "November":
                    month = 11;
                    break;
                case "December":
                    month = 12;
                    break;
            }
            return month;
        } // Pulls device data based on month

        // Main Functions:

        // Pushes CTR data to Firestore (to be implemented)
        public void PushCTRData(string ctrName, string devices)
        {

        }

        // Pulls CTR data from Firestore (to be implemented)
        public void PullCTRData()
        {

        }

        public async Task<Dictionary<string, int>> PullGoalsData(string monthString)
        {
            int month = DetermineMonth(monthString);
            DocumentReference goalsRef = _db.Collection("MonthlyGoals").Document(month.ToString());
            DocumentSnapshot goalsSnapshot = await goalsRef.GetSnapshotAsync();

            Dictionary<string, int> deviceGoals = new Dictionary<string, int>();

            if (goalsSnapshot.Exists) // If it is found then update the dictionary with actual values.
            {
                Dictionary<string, object> goalsData = goalsSnapshot.ToDictionary();
                deviceGoals["XB8Required"] = Convert.ToInt32(goalsData["CGM4981COM"]);
                deviceGoals["XB7fcRequired"] = Convert.ToInt32(goalsData["CGM4331COM"]);
                deviceGoals["XB7FCRequired"] = Convert.ToInt32(goalsData["TG4482A"]);
                deviceGoals["Xi6tRequired"] = Convert.ToInt32(goalsData["IPTVTCXI6HD"]);
                deviceGoals["Xi6ARequired"] = Convert.ToInt32(goalsData["IPTVARXI6HD"]);
                deviceGoals["XiOneRequired"] = Convert.ToInt32(goalsData["SCXI11BEI"]);
                deviceGoals["PodsRequired"] = Convert.ToInt32(goalsData["XE2SGROG1"]);
            }
            else // Default values for blue charts.
            {
                deviceGoals["XB8Required"] = 1;
                deviceGoals["XB7fcRequired"] = 1;
                deviceGoals["XB7FCRequired"] = 1;
                deviceGoals["Xi6tRequired"] = 1;
                deviceGoals["Xi6ARequired"] = 1;
                deviceGoals["XiOneRequired"] = 1;
                deviceGoals["PodsRequired"] = 1;
            }

            Console.WriteLine("Device Goals: ");
            foreach (var goal in deviceGoals)
            {
                Console.WriteLine($"{goal.Key}: {goal.Value}");
            }
            return deviceGoals;
        }


        // Pushes device data to Firestore (to be implemented)
        public async Task PushDeviceData(string deviceName, int quantity, DateTime TimeOfTransaction, string user)
        {
            // ensure time is in UTC
            DateTime utcDateTime = TimeOfTransaction.ToUniversalTime();
            DocumentReference docRef = _db.Collection("bom-wip").Document("Test");
            var data = new
            {
                Device = deviceName,
                Name = user,
                Quantity = quantity,
                Date = Timestamp.FromDateTime(utcDateTime) // Convert DateTime to Firestore Timestamp
            };
            try
            {
                await docRef.SetAsync(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pushing data to Firestore: {ex.Message}");
                throw;
            }
        }

        // Checks if the Firebase Firestore connection is available
        public async Task<bool> CheckIsOnline()
        {
            // Reference the "OnlineStatus" collection and specifically the "Status" document
            DocumentReference docRef = _db.Collection("OnlineStatus").Document("Status");
            try
            {
                // Log the time before the Firestore request
                var startTime = DateTime.Now;

                // Retrieve the document directly
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                // Log the time taken for the Firestore request
                var endTime = DateTime.Now;
                Console.WriteLine("Connection Authenticated in: " + (endTime - startTime).TotalSeconds + " seconds.");

                // Check if the document exists
                if (snapshot.Exists)
                {
                    // Check if the document contains the 'IsOnline' field
                    if (snapshot.ContainsField("IsOnline"))
                    {
                        bool isOnline = snapshot.GetValue<bool>("IsOnline");
                        return isOnline;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false; // Return false if the document doesn't exist
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while Authenticating: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return false;
            }
        }
    }
}