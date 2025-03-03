using Google.Cloud.Firestore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Toolbox_Class_Library.Firestore;

namespace Toolbox_Class_Library
{
    public class DatabaseConnection
    {
        private FirestoreDb _db;

        public DatabaseConnection(string connectionPath)
        {
            _db = FirestoreService.GetFirestoreDb();
        }

        // Asynchronously checks if the application can connect to Firestore
        private async Task InitializeConnection()
        {
            bool isOnline = await CheckIsOnline();

        }

        // Pushes CTR data to Firestore (to be implemented)
        public void PushCTRData(string ctrName, string devices)
        {

        }

        // Pulls CTR data from Firestore (to be implemented)
        public void PullCTRData()
        {

        }

        // Pulls device data based on month
        public void PullDeviceData(string month)
        {

        }

        // Pushes device data to Firestore (to be implemented)
        public void PushDeviceData(string deviceName, int quantity, DateTime TimeOfTransaction)
        {

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