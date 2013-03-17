using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO;

namespace CritterCamp {
    public class HTTPConnection {
        public static async Task<HTTPConnectionResult> GetPostResult(string url, string postData) {
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
                    }
                    catch (WebException webExc) {
                        HttpWebResponse failedResponse = (HttpWebResponse)webExc.Response;
                        error = true;
                        taskComplete.TrySetResult(failedResponse);
                    }
                }, request);
            }, request);


            // get the response and wait for it to complete
            HttpWebResponse response = (HttpWebResponse)await taskComplete.Task;

            // return the message as an HTTPConnectionResult
            using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
                string message = sr.ReadToEnd();
                System.Diagnostics.Debug.WriteLine("HTTP Receive: " + message);
                return new HTTPConnectionResult(error, message);
            }
        }
    }

    public class HTTPConnectionResult {
        public bool error;
        public string message;

        public HTTPConnectionResult(bool e, string m) {
            error = e;
            message = m;
        }
    }
}
