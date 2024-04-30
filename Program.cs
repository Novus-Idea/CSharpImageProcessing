using System;
using System.Net;
using System.IO; 

// Make a simple HTTP listener app 
// that listens on port 80 and returns
// a simple HTML page with a message
// "Hello, World!".

class Program
{
    static void Main()
    {
        // Create a new HTTP listener
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://192.168.137.1/api/machine/");

        // Start the listener
        listener.Start();
        Console.WriteLine("Listening...");

        String quitCondition = "";

        // Listen for incoming requests in a loop
        while (quitCondition != "quit")
        {
            // Wait for an incoming request
            HttpListenerContext context = listener.GetContext();
            Console.WriteLine("Request received");

            // Get the response object
            HttpListenerResponse response = context.Response;

            // Set the content type
            response.ContentType = "text/html";

            // Get the response stream
            Stream output = response.OutputStream;

            // Write the response
            string html = "<html><body><h1>Hello, World!</h1></body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
            output.Write(buffer, 0, buffer.Length);

            // Close the output stream
            output.Close();
        }

        // Stop the listener
        listener.Stop();
    }
}