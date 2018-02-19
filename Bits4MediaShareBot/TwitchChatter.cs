using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bits4MediaShareBot
{
    class TwitchChatter
    {

        private Thread writeThread;
        private Thread readThread;
        private Queue<String> _messagesToSend;

        TcpClient IRCSocket;
        String _serverAddress;
        int _serverPort;

        private StreamReader _chatReader;
        private StreamWriter _chatWriter;
        private NetworkStream _networkStream;

        private static Logger logger = LogManager.GetCurrentClassLogger();


        public TwitchChatter()
        {

        }

        public void Start(string username, string password)
        {
            LoginToTwitch(username, password);
        }

        private void LoginToTwitch(string username, string password)
        {

            try
            {
                _networkStream = IRCSocket.GetStream();
                var utf8withoutBOM = new UTF8Encoding(false);
                _chatReader = new StreamReader(_networkStream, utf8withoutBOM);
                _chatWriter = new StreamWriter(_networkStream, utf8withoutBOM);


                _chatWriter.Flush();

                String loginMessage = "PASS " + password;
                loginMessage += Environment.NewLine + "NICK " + username;

                EnqueueMessage(loginMessage);

            }
            catch (Exception ex)
            {
                logger.Error("Exception Thrown Authenticating With Twitch");

                throw;
            }
        }

        private void ConnectToTwitch()
        {
            IRCSocket = new TcpClient();
            try
            {
                var task = IRCSocket.ConnectAsync(_serverAddress, _serverPort);
                task.Wait();
                if (!IRCSocket.Connected)
                {
                    throw new Exception("Tried to connect but couldnt");
                }
            }
            catch (Exception ex)
            {
                logger.Error("An exception was thrown while trying to connect to Twitch");
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw;
            }
        }



        private void EnqueueMessage(String message)
        {
            lock (_messagesToSend)
            {
                _messagesToSend.Enqueue(message);
            }
        }

        private void WriteToStream(String message)
        {
            logger.Trace("Writing {0} to twitch", message);

            _chatWriter.WriteLine(message);
            _chatWriter.Flush();
        }
    }
}
