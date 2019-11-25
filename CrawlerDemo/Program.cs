using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CrawlerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            startCrawlerasync();
            Console.ReadLine();
        }

        private static async Task startCrawlerasync() {
            var url = "http://www.automobile.tn/neuf/bmw.3";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var elements = htmlDocument.DocumentNode.Descendants("span")
                .Where(node => node.ParentNode.GetAttributeValue("class", "").Equals("articles")).ToList();

            var cars = new List<Car>();            
            Regex charsToDestroy = new Regex(@"[^\d|\.\-]");            

            foreach (var item in elements) 
            {
                var car = new Car
                {
                    Model = item.Descendants("h2").FirstOrDefault().InnerText,                    
                    Price = Convert.ToInt32(charsToDestroy.Replace(item.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("price")).FirstOrDefault().InnerText, "")),
                    Link = "http://www.automobile.tn/" + item.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value,
                    ImageUrl = item.Descendants("img").FirstOrDefault().ChildAttributes("src").FirstOrDefault().Value
                };

                cars.Add(car);
            }

            string stringConn = "datasource=127.0.0.1;port=3306;username=root;password=;database=crawlerdemo";
            MySqlConnection conexion = new MySqlConnection(stringConn);

            conexion.Open();

            try
            {
                int count = cars.Count;
                foreach (var item in cars)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string query = "insert into automoviles(Model,Price,Link,ImageUrl) value(?,?,?,?);";
                        MySqlCommand cmd = new MySqlCommand(query, conexion);
                        cmd.Parameters.Add("?Model", MySqlDbType.VarChar).Value = cars[i].Model;
                        cmd.Parameters.Add("?Price", MySqlDbType.Int32).Value = cars[i].Price;
                        cmd.Parameters.Add("?Link", MySqlDbType.VarChar).Value = cars[i].Link;
                        cmd.Parameters.Add("?ImageUrl", MySqlDbType.VarChar).Value = cars[i].ImageUrl;
                        cmd.ExecuteNonQuery();
                    }

                    count = 0;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            conexion.Close();
            Console.WriteLine("Successful....");
            Console.WriteLine("Press Enter to exit the program...");


            Console.ReadKey();
        }
    }
}
