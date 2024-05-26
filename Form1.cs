using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

namespace KenBelvestre
{
    public partial class Form1 : Form
    {
        public class PostData
        {
            public string Name { get; set; }
            public string Location { get; set; }
            public string Mail { get; set; }
            public long Number { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void btnpost_Click(object sender, EventArgs e)
        {
            // Check and parse the contact number
            if (!long.TryParse(numberTxt.Text, out long contactNumber))
            {
                MessageBox.Show("Please enter a valid number.");
                return;
            }

            // Create a new instance of PostData with the collected data
            PostData newData = new PostData
            {
                Name = nameTxt.Text, // Assign TextBox values to PostData properties
                Location = addTxt.Text,
                Mail = mailTxt.Text,
                Number = contactNumber // Use the parsed contact number
            };

            // Generate URL with query parameters
            string url = GenerateUrl(newData);

            // Log the URL for debugging
            Console.WriteLine("Generated URL: " + url);
            MessageBox.Show("Generated URL: " + url);

            // Open the URL in the default browser
            OpenUrlInBrowser(url);

            // Add the data to the DataGridView
            dataGridView1.Rows.Add(newData.Name, newData.Location, newData.Mail, newData.Number);
        }

        private string GenerateUrl(PostData data)
        {
            // Base address of the server
            string baseUrl = "http://localhost/New%20Folder/post_handler.php";

            // Create a dictionary to hold the query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "Name", data.Name },
                { "Location", data.Location },
                { "Mail", data.Mail },
                { "Number", data.Number.ToString() }
            };

            // Convert the dictionary to a query string
            string queryString = string.Join("&", queryParams.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

            // Combine the base URL and the query string
            return $"{baseUrl}?{queryString}";
        }

        private void OpenUrlInBrowser(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open the URL in browser. Error: {ex.Message}");
            }
        }

        private async Task<List<PostData>> GetDataAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost/New%20Folder/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    HttpResponseMessage response = await client.GetAsync("get_handler.php"); // Change this to your actual GET endpoint
                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var postDataList = JsonConvert.DeserializeObject<List<PostData>>(data);

                        // Check if the data is valid and not null
                        if (postDataList != null && postDataList.Count > 0)
                        {
                            return postDataList;
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"Request error: {e.Message}");
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Unexpected error: {e.Message}");
                }

                return new List<PostData>(); // Return an empty list if no data is found or an error occurs
            }
        }

        private async void getbtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Fetch data from the server
                List<PostData> data = await GetDataAsync();

                // Check if data is available
                if (data.Count > 0)
                {
                    // Clear existing rows
                    dataGridView1.Rows.Clear();

                    // Add fetched data to DataGridView
                    foreach (var item in data)
                    {
                        dataGridView1.Rows.Add(item.Name, item.Location, item.Mail, item.Number);
                    }
                }
                else
                {
                    MessageBox.Show("No data available.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
