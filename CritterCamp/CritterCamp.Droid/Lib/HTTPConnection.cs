using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Net;
using System.Threading.Tasks;
using Java.IO;
using System.Net;
using System.IO;

namespace CritterCamp.Droid.Lib {
    class HTTPConnection : IHTTPConnection {
        public async Task<HTTPConnectionResult> GetPostResult(string url, string postData) {
            System.Diagnostics.Debug.WriteLine("HTTP Sent: " + url + "/" + postData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            bool error = false;
            request.AllowWriteStreamBuffering = true;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            // create the task for the actual post request
            TaskCompletionSource<HttpWebResponse> taskComplete = new TaskCompletionSource<HttpWebResponse>();
            request.BeginGetRequestStream(dataAsyncResponse => {
                // add in the postData into the request
                HttpWebRequest dataResponseRequest = (HttpWebRequest)dataAsyncResponse.AsyncState;
                Stream postStream = dataResponseRequest.EndGetRequestStream(dataAsyncResponse);

                // Convert the string into a byte array. 
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Write to the request stream.
                postStream.Write(byteArray, 0, postData.Length);
                postStream.Close();


                // make the request
                dataResponseRequest.BeginGetResponse(getAsyncResponse => {
                    try {
                        HttpWebRequest getResponseRequest = (HttpWebRequest)getAsyncResponse.AsyncState;
                        HttpWebResponse someResponse = (HttpWebResponse)getResponseRequest.EndGetResponse(getAsyncResponse);
                        taskComplete.TrySetResult(someResponse);
                    } catch(WebException webExc) {
                        HttpWebResponse failedResponse = (HttpWebResponse)webExc.Response;
                        error = true;
                        taskComplete.TrySetResult(failedResponse);
                    }
                }, request);
            }, request);


            // get the response and wait for it to complete
            HttpWebResponse response = (HttpWebResponse)await taskComplete.Task;

            // return the message as an HTTPConnectionResult
            using(StreamReader sr = new StreamReader(response.GetResponseStream())) {
                string message = sr.ReadToEnd();
                System.Diagnostics.Debug.WriteLine("HTTP Receive: " + message);
                return new HTTPConnectionResult(error, message);
            }
        }
        /*
        public Task<HTTPConnectionResult> GetPostResult(string urlRegister, string postData) {
            URL url = new URL(urlRegister); 
            HttpURLConnection conn = (HttpURLConnection) url.OpenConnection();           
            conn.DoOutput = true;
            conn.DoInput = true;
            conn.InstanceFollowRedirects = false;
            conn.RequestMethod = "POST";
            conn.SetRequestProperty("Content-Type", "application/x-www-form-urlencoded"); 
            conn.SetRequestProperty("charset", "utf-8");
            //connection.SetRequestProperty("Content-Length", "" + Integer.toString(urlParameters.getBytes().length));
            conn.UseCaches = false;

            // create the task for the actual post request
            //TaskCompletionSource<HttpWebResponse> taskComplete = new TaskCompletionSource<HttpWebResponse>();
            //request.BeginGetRequestStream(dataAsyncResponse => {

            //});
            OutputStreamWriter writer = new OutputStreamWriter(conn.OutputStream);

            writer.Write(postData);
            writer.Flush();

            String line;
            BufferedReader reader = new BufferedReader(new InputStreamReader(conn.InputStream));

            while ((line = reader.ReadLine()) != null) {
                //System.out.println(line);
            }
            writer.Close();
            reader.Close();      
        }*/
    }
}