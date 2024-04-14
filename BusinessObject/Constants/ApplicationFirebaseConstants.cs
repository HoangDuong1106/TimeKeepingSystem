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
    }

}
