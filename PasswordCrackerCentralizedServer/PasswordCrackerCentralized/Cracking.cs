using System.Net;
using System.Net.Sockets;
using PasswordCrackerCentralized.model;
using PasswordCrackerCentralized.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace PasswordCrackerCentralized
{
    public class Cracking
    {
        /// <summary>
        /// The algorithm used for encryption.
        /// Must be exactly the same algorithm that was used to encrypt the passwords in the password file
        /// </summary>
        private readonly HashAlgorithm _messageDigest;

        private string _newuserpass;

        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
            //_messageDigest = new MD5CryptoServiceProvider();
            // seems to be same speed
        }

        /// <summary>
        /// Runs the password cracking algorithm
        /// </summary>
        public void RunCracking()
        {
            int count = 0;
            bool userlist = true;
            var serverSocket = new TcpListener(IPAddress.Any, 65080);
            serverSocket.Start();
            Socket connectionSocket = serverSocket.AcceptSocket();
            var userInfos = new List<UserInfo>();
            var result = new List<UserInfoClearText>();
            var queue = new Queue<string>();
            bool getting = true;

            using (Stream ns = new NetworkStream(connectionSocket))
            {
                var sw = new StreamWriter(ns);
                using (var sr = new StreamReader(ns))
                {
                    while (!sr.EndOfStream)
                    {
                        String dictionaryEntry = sr.ReadLine();
                        //Hvis besked er -1 skal den sætte userlist til falsk så programmet ved at den er færdig med at modtage bruger listen.
                        if (dictionaryEntry == "-1")
                        {
                            userlist = false;
                        }
                        if (userlist == false && dictionaryEntry != "-1")
                        {
                            //Hvis beskeden er -2 skal den sætte getting til falsk så programmet ved at den er færdig med at modtage ord fra ordbogen.
                            if (dictionaryEntry == "-2")
                            {
                                getting = false;
                                //Printer til console så man faktisk kan se der sker noget.
                                Console.WriteLine("Done Reciving.");
                            }
                            else
                            {
                                //Hvis den ikke er færdig med at modtage fra ordbogen skal den smide det ind i køen
                                queue.Enqueue(dictionaryEntry);
                            }
                        }
                        else
                        {
                            if (dictionaryEntry != null && userlist)
                            {
                                //Her smider den bruger listen ind i en liste til sener brug.
                                String[] parts = dictionaryEntry.Split(":".ToCharArray());
                                var userInfo = new UserInfo(parts[0], parts[1]);
                                userInfos.Add(userInfo);
                            }
                        }
                        while (!getting)
                        {
                            //myTasks.Add(Task.Run(() =>
                            // {
                            // Her går den i gangmed selve dekrypteringen
                            IEnumerable<UserInfoClearText> partialResult = CheckWordWithVariations(queue.Dequeue(),
                                userInfos);
                            result.AddRange(partialResult);
                            //Hvis den får svar tilbage skal den skrive det tilbage til clienten.
                            if (_newuserpass != null)
                            {
                                sw.WriteLine(_newuserpass);
                                sw.Flush();
                                _newuserpass = null;
                            }

                            //Hvis køen er tom giver den besked til clienten så den kan få en ny omgang ord.
                            if (queue.Count == 0)
                            {
                                count++;
                                getting = true;
                                sw.WriteLine("Done");
                                sw.Flush();
                            }
                            //  }));
                            //  Task.WaitAll(myTasks.ToArray());
                        }
                    }
                }
            }
            Console.WriteLine(count);
            Console.WriteLine(string.Join(", ", result));
        }

        /// <summary>
        /// Generates a lot of variations, encrypts each of the and compares it to all entries in the password file
        /// </summary>
        /// <param name="dictionaryEntry">A single word from the dictionary</param>
        /// <param name="userInfos">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private IEnumerable<UserInfoClearText> CheckWordWithVariations(String dictionaryEntry, List<UserInfo> userInfos)
        {
            var result = new List<UserInfoClearText>();

            String possiblePassword = dictionaryEntry;
            IEnumerable<UserInfoClearText> partialResult = CheckSingleWord(userInfos, possiblePassword);
            result.AddRange(partialResult);

            String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
            IEnumerable<UserInfoClearText> partialResultUpperCase = CheckSingleWord(userInfos, possiblePasswordUpperCase);
            result.AddRange(partialResultUpperCase);

            String possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
            IEnumerable<UserInfoClearText> partialResultCapitalized = CheckSingleWord(userInfos, possiblePasswordCapitalized);
            result.AddRange(partialResultCapitalized);

            String possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
            IEnumerable<UserInfoClearText> partialResultReverse = CheckSingleWord(userInfos, possiblePasswordReverse);
            result.AddRange(partialResultReverse);

            for (int i = 0; i < 100; i++)
            {
                String possiblePasswordEndDigit = dictionaryEntry + i;
                IEnumerable<UserInfoClearText> partialResultEndDigit = CheckSingleWord(userInfos, possiblePasswordEndDigit);
                result.AddRange(partialResultEndDigit);
            }

            for (int i = 0; i < 100; i++)
            {
                String possiblePasswordStartDigit = i + dictionaryEntry;
                IEnumerable<UserInfoClearText> partialResultStartDigit = CheckSingleWord(userInfos, possiblePasswordStartDigit);
                result.AddRange(partialResultStartDigit);
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                    IEnumerable<UserInfoClearText> partialResultStartEndDigit = CheckSingleWord(userInfos, possiblePasswordStartEndDigit);
                    result.AddRange(partialResultStartEndDigit);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks a single word (or rather a variation of a word): Encrypts and compares to all entries in the password file
        /// </summary>
        /// <param name="userInfos"></param>
        /// <param name="possiblePassword">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private IEnumerable<UserInfoClearText> CheckSingleWord(IEnumerable<UserInfo> userInfos, String possiblePassword)
        {
            char[] charArray = possiblePassword.ToCharArray();
            byte[] passwordAsBytes = Array.ConvertAll(charArray, PasswordFileHandler.GetConverter());
            byte[] encryptedPassword = _messageDigest.ComputeHash(passwordAsBytes);
            //string encryptedPasswordBase64 = System.Convert.ToBase64String(encryptedPassword);

            var results = new List<UserInfoClearText>();
            foreach (UserInfo userInfo in userInfos)
            {
                if (CompareBytes(userInfo.EntryptedPassword, encryptedPassword))
                {
                    results.Add(new UserInfoClearText(userInfo.Username, possiblePassword));
                    Console.WriteLine(userInfo.Username + " " + possiblePassword);
                    _newuserpass = userInfo.Username + ": " + possiblePassword;
                }
            }
            return results;
        }

        /// <summary>
        /// Compares to byte arrays. Encrypted words are byte arrays
        /// </summary>
        /// <param name="firstArray"></param>
        /// <param name="secondArray"></param>
        /// <returns></returns>
        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("firstArray");
            //}
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("secondArray");
            //}
            if (firstArray.Count != secondArray.Count)
            {
                return false;
            }
            for (int i = 0; i < firstArray.Count; i++)
            {
                if (firstArray[i] != secondArray[i])
                    return false;
            }
            return true;
        }

    }
}
