using System;
using System.Net;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace weather
{
    class Program
    {
        static void Main(string[] args)
        {
            //ProcessStartInfo info = new ProcessStartInfo("weather28.dll") {Verb = "runas"};
            //info.UseShellExecute = false;
            //Process.Start(info);

            string url = @"http://+"; 
            string port = "80";
            string prefix = String.Format("{0}:{1}/", url, port);

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://+:80/");

            listener.Start();

            Console.WriteLine("Welcome to simple HttpListener.\n", port);
            Console.WriteLine("Listening on {0}...", prefix);

            while (true)
            {
                //Ожидание входящего запроса
                HttpListenerContext context = listener.GetContext();

                //Объект запроса
                HttpListenerRequest request = context.Request;

                //Объект ответа
                HttpListenerResponse response = context.Response;

                //Создаем ответ
                string requestBody;
                Stream inputStream = request.InputStream;
                Encoding encoding = request.ContentEncoding;
                StreamReader reader = new StreamReader(inputStream, encoding);
                requestBody = reader.ReadToEnd();

                Console.WriteLine("{0} request was caught: {1}",
                                   request.HttpMethod, request.Url);

                response.StatusCode = (int)HttpStatusCode.OK;

                //Возвращаем ответ
                using (Stream stream = response.OutputStream)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(ParserString(request.Url.ToString()).ToString());
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// Парсер запроса
        /// </summary>
        /// <param name="url"></param>
        static JObject ParserString(string url)
        {
            char[] znaki = { '/', '?', '=', '&' };
            string[] mass_url = url.Split(znaki);
            List<string> list_mass_url = new List<string>();
            for (int i = 0; i < mass_url.Length; i++)
            {
                list_mass_url.Add(mass_url[i]);
            }
            if (list_mass_url.Contains("current") && list_mass_url.Contains("city") && !list_mass_url.Contains("dt"))
            {
                int IndexCity = list_mass_url.IndexOf("city") + 1;
                string city = list_mass_url[IndexCity];
                return CurrentWeather(city);
            }
            else if (list_mass_url.Contains("forecast") && list_mass_url.Contains("city") && list_mass_url.Contains("dt"))
            {
                int IndexCity = list_mass_url.IndexOf("city") + 1;
                string city = list_mass_url[IndexCity];

                int IndexDt = list_mass_url.IndexOf("dt") + 1;
                string Dt = list_mass_url[IndexDt];
                // if (Dt == "current" || "minutely" || "hourly" || "daily" || "alerts")
                switch (Dt)
                {
                    case "current": return ForecastWeather(city, Dt); 
                    case "minutely": return ForecastWeather(city, Dt); 
                    case "hourly": return ForecastWeather(city, Dt); 
                    case "daily": return ForecastWeather(city, Dt); 
                    case "alerts": return ForecastWeather(city, Dt); 
                    default:
                        {
                            JArray array = new JArray();

                            JObject mainTree = new JObject();

                            mainTree["ok"] = false;

                            JObject o = new JObject();

                            o["error"] = "error";
                            o["dt"] = "current | minutely | hourly |  daily | alerts";
                            array.Add(o);

                            mainTree["response"] = array;
                            return mainTree;
                        }
                }
            }
            else
            {
                JArray array = new JArray();

                JObject mainTree = new JObject();

                mainTree["ok"] = false;

                JObject o = new JObject();

                o["error"] = "error";
                array.Add(o);

                mainTree["response"] = array;
                return mainTree;
            }
        }

        /// <summary>
        /// Текущая погода
        /// </summary>
        static JObject CurrentWeather(string city)
        {
            string Api_key = "82ab21c6e456449ebe9247c770287b35";
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={Api_key}&units=metric";
            try
            {
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                // If required by the server, set the credentials.
                myReq.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();
                // Display the status.
                Console.WriteLine(response.StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
             //   Console.WriteLine(responseFromServer);


                string temp = JObject.Parse(responseFromServer)["main"]["temp"].ToString();


                JArray array = new JArray();

                JObject mainTree = new JObject();

                JObject o = new JObject();

                o["city"] = city;
                o["unit"] = "metric";
                o["temperature"] = temp;
                array.Add(o);

                mainTree["response"] = array;

                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();

                return mainTree;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Погода на 5 дней
        /// </summary>
        static JObject ForecastWeather(string city, string Dt)
        {
            //получение координат города 

            string url_city = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{city}.json?access_token=pk.eyJ1IjoiZW1vbGluZyIsImEiOiJja2docW54ZXAwZThsMzNxbmpqczNxMjhvIn0.E0Njq_PpGrC6Vf-uXh3RWA";
            HttpWebRequest myReq_city = (HttpWebRequest)WebRequest.Create(url_city);
            // If required by the server, set the credentials.
            myReq_city.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)myReq_city.GetResponse();
            // Display the status.
          //  Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            var g = JObject.Parse(responseFromServer)["features"][0]["center"].ToArray(); //0 - долгота(lon) 1 - ширина (lat)

            //https://api.openweathermap.org/data/2.5/onecall?lat={lat}&lon={lon}&exclude={part}&appid={API key}

            //получение погоды
            string Api_key = "82ab21c6e456449ebe9247c770287b35";
            string url_weather = $"https://api.openweathermap.org/data/2.5/onecall?lat={g[1]}&lon={g[0]}&exclude={Dt}&appid={Api_key}&units=metric";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url_weather);
            // If required by the server, set the credentials.
            myReq.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response_weather = (HttpWebResponse)myReq.GetResponse();
            // Display the status.
            Console.WriteLine(response_weather.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream_weather = response_weather.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader_weather = new StreamReader(dataStream_weather);
            // Read the content.
            string responseFromServer_weather = reader_weather.ReadToEnd();
            // Display the content.
           // Console.WriteLine(responseFromServer_weather);

            JToken[] temp;
            if (Dt == "hourly")
            {
                temp = JObject.Parse(responseFromServer_weather)["daily"].ToArray();
            }
            else
                temp = JObject.Parse(responseFromServer_weather)["hourly"].ToArray();

            JArray array = new JArray();

            JObject mainTree = new JObject();

            JObject o = new JObject();
            for (int i = 0; i < temp.Length; i++)
            {
                o["dt"] = temp[i]["dt"];
                o["city"] = city;
                o["unit"] = "metric";
                o["temperature"] = temp[i]["temp"];
                array.Add(o);
                mainTree["response"] = array;
            }
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            reader_weather.Close();
            dataStream_weather.Close();
            response_weather.Close();

            return mainTree;
        }

    }
}
