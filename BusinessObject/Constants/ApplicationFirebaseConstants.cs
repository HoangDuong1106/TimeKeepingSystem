using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Constants
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class ApplicationFirebaseConstants
    {
        public static readonly string URL_FIREBASE = "https://nextjs-course-f2de1-default-rtdb.firebaseio.com";
        public static readonly string PathConfig = "/configVariable";
        public static readonly string LATEST_EMPLOYEE_NUMBER = "latestEmployeeNumber";

        // Method to retrieve configuration variable from Firebase
        public static async Task<string> GetConfigVariable(string nameOfVariable)
        {
            var fullPath = $"{URL_FIREBASE}{PathConfig}/{nameOfVariable}.json";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(fullPath);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return content; // You might want to parse this JSON content
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to retrieve data from Firebase.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching configuration from Firebase: " + ex.Message);
            }
        }

        public static async Task<bool> UpdateConfigVariable(string nameOfVariable, string newValue)
        {
            var fullPath = $"{URL_FIREBASE}{PathConfig}/{nameOfVariable}.json";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var content = new StringContent($"\"{newValue}\"", Encoding.UTF8, "application/json"); // Ensure the value is properly JSON encoded
                    var response = await httpClient.PutAsync(fullPath, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Failed to update data in Firebase. Status Code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating configuration in Firebase: " + ex.Message);
            }
        }

        public static async Task<bool> UpdateLatestEmployeeNumberInFirebase()
        {
            try
            {
                // Assuming you have a method to fetch the highest current EmployeeNumber
                var newEmployeeNumber = await GetConfigVariable(LATEST_EMPLOYEE_NUMBER);

                // Update the value in Firebase
                var result = await ApplicationFirebaseConstants.UpdateConfigVariable("/configVariable/latestEmployeeNumber", newEmployeeNumber);

                if (!result)
                {
                    throw new InvalidOperationException("Failed to update the latest EmployeeNumber on Firebase.");
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as necessary
                throw new Exception($"Error updating EmployeeNumber in Firebase: {ex.Message}", ex);
            }
        }

    }

}
