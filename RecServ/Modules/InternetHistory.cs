using Microsoft.Data.Sqlite;
using RecServ.Global_Classes;

namespace RecServ.Modules
{
    internal class InternetHistory : IDisposable
    {
        public void GetHistory()
        {
            this.siteIcon.Clear();
            this.siteTitle.Clear();
            this.siteUrl.Clear();
            this.siteVisitTime.Clear();

            PathManager.Instance.CheckFolder(PathManager.Instance.Database);
            PathManager.Instance.CheckFolder(PathManager.Instance.FetchedDatabaseFiles);

            CheckChromeFilePaths();

            FetchDatabaseFiles(FaviconsSrcFile, FaviconFile);
            FetchDatabaseFiles(HistorySrcFile, HistoryFile);

            GetChromeHistory(DateParse());
            SaveToDatabase();

            Dispose();
        }


#region > Constants & Other Initializers:

        private readonly string CurrentDate = $"{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}";

        //File Locations
        private string FaviconsSrcFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\Favicons");
        private readonly string FaviconFile = $@"{PathManager.Instance.FetchedDatabaseFiles}\Favicons";

        private string HistorySrcFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\History");
        private readonly string HistoryFile = $@"{PathManager.Instance.FetchedDatabaseFiles}\History";

        private readonly string FetchedListCount = $@"{PathManager.Instance.TemporaryFiles}\Tmp-{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}";

        private readonly string InternetHistoryDB = $@"{PathManager.Instance.Database}\InternetHistoryDB";

        //Lists to store the Fatched Values
        private readonly List<byte[]> siteIcon = new();
        private readonly List<string> siteTitle = new();
        private readonly List<string> siteUrl = new();
        private readonly List<DateTimeOffset> siteVisitTime = new();

#endregion


#region > Other Helper Methods:

        //Sets Paths to get files from chrome
        private void CheckChromeFilePaths()
        {
            var FaviconFilePath = FaviconsSrcFile;
            var HistoryFilePath = HistorySrcFile;

            if (!File.Exists(FaviconFilePath) && !File.Exists(HistoryFilePath))
            {
                FaviconsSrcFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome Dev\User Data\Default\Favicons");
                HistorySrcFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome Dev\User Data\Default\History");
            }
        }

        //Converts Date to chrome Specific Format
        private DateTimeOffset DateParse()
        {
            var Date = CurrentDate;
            //var Date = "1-4-2023";

            var dateParts = Date.Split('-');
            var day = int.Parse(dateParts[0]);
            var month = int.Parse(dateParts[1]);
            var year = int.Parse(dateParts[2]);

            DateTimeOffset date = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);

            

            return date;
        }

        //Defines Date Range (i.e 24 Hours)
        public (long startTime, long endTime) DataFetchRange(DateTimeOffset date)
        {
            var startOfDay = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
            var startOfDayChromeEpoch = (startOfDay.ToUnixTimeSeconds() + 11644473600) * 1000000L;

            var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);
            var endOfDayChromeEpoch = (endOfDay.ToUnixTimeSeconds() + 11644473600) * 1000000L;

            // Return start and end of day as long integers
            return (startOfDayChromeEpoch, endOfDayChromeEpoch);
        }

        //Converts Chroms's Time (Epoch) to UTC Format
        public DateTimeOffset EpochToUTC(long chromeTime)
        {
            //Converts ChromeTime to Local time
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromFileTime(chromeTime);

            //Converts to UTC
            DateTimeOffset utcTime = dateTimeOffset.ToUniversalTime();

            //Sets offset of +0:00
            DateTimeOffset fixedUtcTime = new DateTimeOffset(utcTime.DateTime, TimeSpan.Zero);

            //Adds 8 minutes to the time
            DateTimeOffset adjustedUtcTime = fixedUtcTime.AddMinutes(8);

            return adjustedUtcTime;
        }

        
        public static void FetchDatabaseFiles(string Source, string Destination)
        {
            try
            {
                if (!File.Exists(Source))
                {
                    throw new FileNotFoundException("The source file does not exist.");
                }

                File.Copy(Source, Destination, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying file: {ex.Message}");
            }
        }


        private void SaveFetchedListCount()
        {
            File.WriteAllText(FetchedListCount, siteTitle.Count.ToString());
        }

        #region - Handles Favicons

        //Fetches Favicons From Chrome's SqLite File
        private byte[] GetFavicons(string url)
        {
            var siteUri = new Uri(url);
            string siteUrl = siteUri.GetLeftPart(UriPartial.Authority);

            var iconPath = FaviconFile;
            byte[]? faviconData = null;
            if (File.Exists(iconPath))
            {
                using (var connection = new SqliteConnection($"Data Source={iconPath};"))
                {
                    connection.Open();
                    using (var command = new SqliteCommand("SELECT icon_mapping.page_url, favicon_bitmaps.image_data FROM icon_mapping JOIN favicon_bitmaps ON icon_mapping.icon_id = favicon_bitmaps.icon_id WHERE icon_mapping.page_url LIKE '%' || @url || '%'", connection))
                    {
                        command.Parameters.AddWithValue("@url", siteUrl);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                faviconData = GetBytes(reader);
                            }
                        }
                    }
                }
            }
            return faviconData;
        }

        //Converts Fatched Icons (PNG) to Byte[]
        private byte[] GetBytes(SqliteDataReader reader)
        {
            const int CHUNK_SIZE = 2 * 1024;
            var buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = reader.GetBytes(1, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }

        //Stores  Byte[] in list
        private void StoreFavicon(string url)
        {
            var faviconData = GetFavicons(url);
            if (faviconData != null)
            {
                siteIcon.Add(faviconData);
            }
            else
            {
                System.Drawing.Image image = Properties.Resources.DefaultSiteIcon;
                byte[] DefaultIcon;
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    DefaultIcon = ms.ToArray();
                }
                siteIcon.Add(DefaultIcon);
            }
        }

        #endregion

#endregion



#region > Main Mudule:

        //Fetches History From Sqllite File and Stores in respactive Lists
        private void GetChromeHistory(DateTimeOffset date)
        {
            string ConnectionStr = $"Data Source={HistoryFile}";
            using var connection = new SqliteConnection(ConnectionStr);

            var Date = DataFetchRange(date);


            var query = $"SELECT title, url, last_visit_time FROM urls WHERE last_visit_time BETWEEN {Date.startTime} AND {Date.endTime} ORDER BY last_visit_time DESC";

            connection.Open();
            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var title = reader.GetString(0); //Gets Website Title From Database
                var url = reader.GetString(1);  //Gets Website URL From Database

                var visitTime = EpochToUTC(reader.GetInt64(2));

                StoreFavicon(url);

                if (string.IsNullOrEmpty(title) || title.Contains("https://"))
                {
                    siteTitle.Add("Untitled Page");
                }
                else
                {
                    siteTitle.Add(title);
                }

                    siteUrl.Add(url);
                    siteVisitTime.Add(visitTime);

            }
            connection.Close();
            MessageBox.Show($"Data Saved:\r\n\r\nIcon - \t{siteIcon.Count}\tTitle - \t{siteTitle.Count}\r\nURL - \t{siteUrl.Count}\tTime - \t{siteVisitTime.Count}");

        }

        //Saves The Captured Data to SqLite Database
        private void SaveToDatabase()
        {
            if (siteTitle.Count == siteUrl.Count && siteUrl.Count == siteIcon.Count && siteTitle.Count != 0)
            {
                using (var connection = new SqliteConnection($"Data Source={InternetHistoryDB}"))
                {
                    var CurrentDBTable = CurrentDate;

                    connection.Open();
                    using (var command = new SqliteCommand($"CREATE TABLE IF NOT EXISTS '{CurrentDBTable}' (Icon BLOB, Title TEXT, URL TEXT, Time TEXT)", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    int TotalSaved = 0;
                    int StartIndexValue = 0;
                    if(File.Exists(FetchedListCount))
                    {
                        StartIndexValue = int.Parse(File.ReadAllText(FetchedListCount));
                    }

                    for (int i = StartIndexValue; i < siteTitle.Count; i++)
                    {
                        using (var command = new SqliteCommand($"INSERT INTO '{CurrentDBTable}' (Icon, Title, URL, Time) VALUES (@Icon, @Title, @URL, @Time)", connection))
                        {
                            command.Parameters.AddWithValue("@Icon", siteIcon[i]);
                            command.Parameters.AddWithValue("@Title", siteTitle[i]);
                            command.Parameters.AddWithValue("@URL", siteUrl[i]);
                            command.Parameters.AddWithValue("@Time", siteVisitTime[i].ToString("hh:mm tt"));

                            command.ExecuteNonQuery();
                        }
                        TotalSaved++;
                    }
                    MessageBox.Show("Entries Saved: " + TotalSaved.ToString());
                    connection.Close();
                }
                SaveFetchedListCount();
            }
        }

        public void Dispose()
        {
            // Dispose any disposable objects here
            // Clear lists
            siteIcon.Clear();
            siteTitle.Clear();
            siteUrl.Clear();
            siteVisitTime.Clear();

            using (var connection = new SqliteConnection($"Data Source={InternetHistoryDB}"))
            {
                connection.Close();
            }
        }

        #endregion

    }
}
