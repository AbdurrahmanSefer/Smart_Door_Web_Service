
using System;
using System.Text;

using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;

namespace Sms_recive_web_service.Controllers
{
    public class PushNotification
    {
        public PushNotification(string to)
        {
            try
            {
                var applicationID = "AAAA83ECa9c:APA91bHp7rvgAhvk_uA8ubzVNhvVIl_FwnKogBBjBOerReLuRmdmvOUTbaQns88S30xIoFU0uFY_4bBHzXy6xetlwWeJGyp1hltQ9d82O9oCqA16bsa-qAnB4Iv77DIRtUuQLsFy2cvp";

                var senderId = "1045573037015";

                string deviceId = to;

                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");

                tRequest.Method = "post";

                tRequest.ContentType = "application/json";

                var data = new

                {

                    to = deviceId,

                    notification = new

                    {

                        body ="Hello Deneme",

                        title = "Heloo",

                        icon = "myicon"

                    }
                };

                var serializer = new JavaScriptSerializer();

                var json = serializer.Serialize(data);

                Byte[] byteArray = Encoding.UTF8.GetBytes(json);

                tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));

                tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));

                tRequest.ContentLength = byteArray.Length;


                using (Stream dataStream = tRequest.GetRequestStream())
                {

                    dataStream.Write(byteArray, 0, byteArray.Length);


                    using (WebResponse tResponse = tRequest.GetResponse())
                    {

                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {

                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {

                                String sResponseFromServer = tReader.ReadToEnd();

                                string str = sResponseFromServer;

                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {

                string str = ex.Message;

            }

        }

        public async Task<bool> NotifyAsync(string to, string title, string body)
        {
            try
            {
                // Get the server key from FCM console
                var serverKey = string.Format("key={0}", "AAAA83ECa9c:APA91bHp7rvgAhvk_uA8ubzVNhvVIl_FwnKogBBjBOerReLuRmdmvOUTbaQns88S30xIoFU0uFY_4bBHzXy6xetlwWeJGyp1hltQ9d82O9oCqA16bsa-qAnB4Iv77DIRtUuQLsFy2cvp");
                // Get the sender id from FCM console
                var senderId = string.Format("id={0}", "1045573037015");
                var data = new
                {
                    to, // Recipient device token
                    notification = new { title, body }
                };

                // Using Newtonsoft.Json
                var jsonBody = JsonConvert.SerializeObject(data);

                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send"))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", serverKey);
                    httpRequest.Headers.TryAddWithoutValidation("Sender", senderId);
                    httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        var result = await httpClient.SendAsync(httpRequest);

                        if (result.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            // Use result.StatusCode to handle failure
                            // Your custom error handler here
                           
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               
            }

            return false;
        }

    }
}