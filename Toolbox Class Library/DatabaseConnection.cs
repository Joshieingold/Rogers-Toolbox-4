

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System;
using System.ComponentModel.Design;
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
        public async Task PushCTRData(string ctrName, List<Device> DeviceList)
        {
            DateTime TimeOfTransaction = DateTime.Now;
            DateTime utcDateTime = TimeOfTransaction.ToUniversalTime();
            string docTitle = $"{ctrName} - {utcDateTime:yyyy-MM-dd}";
            DocumentReference docRef = _db.Collection("CTR-Reports").Document(docTitle);

            // Create a dictionary to map device names to their counts
            var deviceCounts = DeviceList.ToDictionary(device => device.Name, device => device.Counter);

            // Create a list of devices with counts set to 0
            var deviceOrders = DeviceList.ToDictionary(device => device.Name, device => 0);

            var data = new
            {
                ctrID = ctrName,
                dateSubmitted = Timestamp.FromDateTime(utcDateTime),
                deviceCounts = deviceCounts,
                deviceOrders = deviceOrders
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
        public async Task PushTechData(string techName, List<Device> DeviceList)
        {
            DateTime TimeOfTransaction = DateTime.Now;
            DateTime utcDateTime = TimeOfTransaction.ToUniversalTime();
            string docTitle = $"{techName} - {utcDateTime:yyyy-MM-dd}";
            DocumentReference docRef = _db.Collection("Tech-Reports").Document(docTitle);

            // Create a dictionary to map device names to their counts
            var deviceCounts = DeviceList.ToDictionary(device => device.Name, device => device.Counter);

            // Create a list of devices with counts set to 0
            var deviceOrders = DeviceList.ToDictionary(device => device.Name, device => 0);

            var data = new
            {
                TechID = techName,
                dateSubmitted = Timestamp.FromDateTime(utcDateTime),
                deviceCounts = deviceCounts,
                deviceOrders = deviceOrders
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

        public async Task<Dictionary<string, int>> PullDeviceData(string monthString)
        {
            int monthInt = DetermineMonth(monthString);
            int year = DateTime.Now.Year;
            int month = DetermineMonth(monthString);
            if (DetermineMonth(monthString) == 12)
            {
                year = 2024;
            }
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            DateTime startDateUtc = startDate.ToUniversalTime();
            DateTime endDateUtc = endDate.ToUniversalTime();

            Google.Cloud.Firestore.Query query = _db.Collection("bom-wip")
                    .WhereGreaterThanOrEqualTo("Date", Timestamp.FromDateTime(startDateUtc))
                    .WhereLessThanOrEqualTo("Date", Timestamp.FromDateTime(endDateUtc));

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            Dictionary<string, int> actuals = new Dictionary<string, int> // Creates a dictionary to sum up the data.
                {
                    { "CGM4981COM", 0 },
                    { "CGM4331COM", 0 },
                    { "TG4482A", 0 },
                    { "IPTVTCXI6HD", 0 },
                    { "IPTVARXI6HD", 0 },
                    { "SCXI11BEI", 0 },
                    { "XE2SGROG1", 0 }
                };

            foreach (var document in snapshot.Documents) // sums all data into the dictionary.
            {
                var data = document.ToDictionary();
                string device = data["Device"]?.ToString();
                if (int.TryParse(data["Quantity"]?.ToString(), out int quantity) && actuals.ContainsKey(device))
                {
                    actuals[device] += quantity;
                }
            }

            return actuals;

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

            return deviceGoals;
        }

        public async Task<(List<DataRecord>, Dictionary<string, int>, Dictionary<string, int>)> PullDatabaseData(DateTime StartDate, DateTime EndDate)
        {
            DateTime startDateUtc = StartDate.ToUniversalTime();
            DateTime endDateUtc = EndDate.ToUniversalTime();

            Google.Cloud.Firestore.Query query = _db.Collection("bom-wip")
                .WhereGreaterThanOrEqualTo("Date", Timestamp.FromDateTime(startDateUtc))
                .WhereLessThanOrEqualTo("Date", Timestamp.FromDateTime(endDateUtc));

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            TimeZoneInfo astTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");

            List<DataRecord> records = new List<DataRecord>();
            Dictionary<string, int> deviceTotals = new Dictionary<string, int>();
            Dictionary<string, int> userTotals = new Dictionary<string, int>();

            foreach (var document in snapshot.Documents) 
            {
                var data = document.ToDictionary();

                // Extract & Convert Date
                DateTime dateValue = DateTime.MinValue;
                if (data.TryGetValue("Date", out object dateObj) && dateObj is Timestamp timestamp)
                {
                    dateValue = timestamp.ToDateTime();
                    dateValue = TimeZoneInfo.ConvertTimeFromUtc(dateValue, astTimeZone);
                }

                // Extract Fields
                string device = data.ContainsKey("Device") ? data["Device"]?.ToString() ?? "Unknown" : "Unknown";
                string name = data.ContainsKey("Name") ? data["Name"]?.ToString() ?? "Unknown" : "Unknown";
                int quantity = data.ContainsKey("Quantity") && int.TryParse(data["Quantity"]?.ToString(), out int qty) ? qty : 0;

                // Add to List
                var record = new DataRecord { Device = device, Name = name, Quantity = quantity, Date = dateValue };
                records.Add(record);

                // Aggregate Totals
                if (deviceTotals.ContainsKey(device))
                    deviceTotals[device] += quantity;
                else
                    deviceTotals[device] = quantity;

                if (userTotals.ContainsKey(name))
                    userTotals[name] += quantity;
                else
                    userTotals[name] = quantity;
            }

            return (records, deviceTotals, userTotals);
        }

        public async Task PushSerialsData(string deviceName, int quantity, DateTime TimeOfTransaction, string user, List<string> serialList)
        {
            // ensure time is in UTC
            DateTime utcDateTime = TimeOfTransaction.ToUniversalTime();
            DocumentReference docRef = _db.Collection("SerialDatabase").Document();
            var data = new
            {
                Device = deviceName,
                Name = user,
                Quantity = quantity,
                Date = Timestamp.FromDateTime(utcDateTime), // Convert DateTime to Firestore Timestamp
                Serials = serialList
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


        // Pushes device data to Firestore (to be implemented)
        public async Task PushDeviceData(string deviceName, int quantity, DateTime TimeOfTransaction, string user, List<string> serialList)
        {
            // ensure time is in UTC
            DateTime utcDateTime = TimeOfTransaction.ToUniversalTime();
            DocumentReference docRef = _db.Collection("bom-wip").Document();
            var data = new
            {
                Device = deviceName,
                Name = user,
                Quantity = quantity,
                Date = Timestamp.FromDateTime(utcDateTime), // Convert DateTime to Firestore Timestamp
                Serials = serialList
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

        public async Task<List<SerialRecord>> PullSerialDataByDate(DateTime startDate, DateTime endDate)
        {
            DateTime startDateUtc = startDate.ToUniversalTime();
            DateTime endDateUtc = endDate.ToUniversalTime();
            TimeZoneInfo astTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");

            var records = new List<SerialRecord>();

            // Local helper method with type override
            async Task<List<SerialRecord>> QueryCollection(string collectionName, string type)
            {
                var query = _db.Collection(collectionName)
                    .WhereGreaterThanOrEqualTo("Date", Timestamp.FromDateTime(startDateUtc))
                    .WhereLessThanOrEqualTo("Date", Timestamp.FromDateTime(endDateUtc));

                var snapshot = await query.GetSnapshotAsync();
                var list = new List<SerialRecord>();

                foreach (var document in snapshot.Documents)
                {
                    var data = document.ToDictionary();
                    DateTime dateValue = DateTime.MinValue;

                    if (data.TryGetValue("Date", out object dateObj) && dateObj is Timestamp timestamp)
                    {
                        dateValue = timestamp.ToDateTime();
                        dateValue = TimeZoneInfo.ConvertTimeFromUtc(dateValue, astTimeZone);
                    }

                    if (data.ContainsKey("Serials"))
                    {
                        var serials = data["Serials"] as List<object>;
                        if (serials != null)
                        {
                            foreach (var serial in serials)
                            {
                                if (serial is string serialString)
                                {
                                    list.Add(new SerialRecord
                                    {
                                        Device = data.ContainsKey("Device") ? data["Device"]?.ToString() ?? "Unknown" : "Unknown",
                                        SerialNumber = serialString,
                                        User = data.ContainsKey("Name") ? data["Name"]?.ToString() ?? "Unknown" : "Unknown",
                                        Date = dateValue,
                                        Type = type // <-- Set type dynamically
                                    });
                                }
                            }
                        }
                    }
                }

                return list;
            }

            // Query each collection with appropriate type
            var bomWipRecords = await QueryCollection("bom-wip", "Production");
            var serialDatabaseRecords = await QueryCollection("SerialDatabase", "General Import");

            // Merge and return
            records.AddRange(bomWipRecords);
            records.AddRange(serialDatabaseRecords);

            return records;
        }

        public async Task<List<SerialRecord>> PullSerialDataByList(string[] serialNumbers)
        {
            TimeZoneInfo astTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
            var records = new List<SerialRecord>();

            // Local reusable function for querying a collection and assigning the type
            async Task<List<SerialRecord>> QueryCollection(string collectionName, string type)
            {
                var query = _db.Collection(collectionName);
                var snapshot = await query.GetSnapshotAsync();
                var list = new List<SerialRecord>();

                foreach (var document in snapshot.Documents)
                {
                    var data = document.ToDictionary();

                    if (data.ContainsKey("Serials"))
                    {
                        var serials = data["Serials"] as List<object>;
                        if (serials != null)
                        {
                            foreach (var serial in serials)
                            {
                                if (serial is string serialString && serialNumbers.Contains(serialString))
                                {
                                    DateTime dateValue = DateTime.MinValue;
                                    if (data.TryGetValue("Date", out object dateObj) && dateObj is Timestamp timestamp)
                                    {
                                        dateValue = timestamp.ToDateTime();
                                        dateValue = TimeZoneInfo.ConvertTimeFromUtc(dateValue, astTimeZone);
                                    }

                                    list.Add(new SerialRecord
                                    {
                                        Device = data.ContainsKey("Device") ? data["Device"]?.ToString() ?? "Unknown" : "Unknown",
                                        SerialNumber = serialString,
                                        User = data.ContainsKey("Name") ? data["Name"]?.ToString() ?? "Unknown" : "Unknown",
                                        Date = dateValue,
                                        Type = type // <- Set correct type
                                    });
                                }
                            }
                        }
                    }
                }

                return list;
            }

            // Query both collections with appropriate type
            var bomWipRecords = await QueryCollection("bom-wip", "Production");
            var serialDbRecords = await QueryCollection("SerialDatabase", "General Import");

            // Merge and return
            records.AddRange(bomWipRecords);
            records.AddRange(serialDbRecords);

            return records;
        }

        public class DataRecord
        {
            public string Device { get; set; }     // Name of the device
            public string Name { get; set; }       // Name of the user
            public int Quantity { get; set; }      // Quantity completed
            public DateTime Date { get; set; }     // Date of completion
        }
        public class SerialRecord
        {
            public string Device { get; set; }     // Name of the device
            public string SerialNumber { get; set; } // Serial number of the device
            public string User { get; set; }       // Name of the user
            public DateTime Date { get; set; }     // Date of completion
            public string Type { get; set; }       // The database it came from
        }
    }
}