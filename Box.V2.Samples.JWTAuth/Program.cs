﻿using Box.V2.Config;
using Box.V2.JWTAuth;
using Box.V2.Models;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Box.V2.Samples.JWTAuth
{
    class Program
    {
        static readonly string CLIENT_ID = ConfigurationManager.AppSettings["boxClientId"];
        static readonly string CLIENT_SECRET = ConfigurationManager.AppSettings["boxClientSecret"];
        static readonly string ENTERPRISE_ID = ConfigurationManager.AppSettings["boxEnterpriseId"];
        static readonly string JWT_PRIVATE_KEY_PASSWORD = ConfigurationManager.AppSettings["boxPrivateKeyPassword"];
        static readonly string JWT_PUBLIC_KEY_ID = ConfigurationManager.AppSettings["boxPublicKeyId"];

        static void Main(string[] args)
        {
            Task t = MainAsync();
            t.Wait();

            Console.WriteLine();
            Console.Write("Press return to exit...");
            Console.ReadLine();
        }

        static async Task MainAsync()
        {
            var privateKey = File.ReadAllText("private_key.pem");

            var boxConfig = new BoxConfig(CLIENT_ID, CLIENT_SECRET, ENTERPRISE_ID, privateKey, JWT_PRIVATE_KEY_PASSWORD, JWT_PUBLIC_KEY_ID);
            var boxJWT = new BoxJWTAuth(boxConfig);

            var adminToken = boxJWT.AdminToken();
            Console.WriteLine("Admin Token: " + adminToken);
            Console.WriteLine();

            var adminClient = boxJWT.AdminClient(adminToken);

            Console.WriteLine("Admin root folder items");
            var items = await adminClient.FoldersManager.GetFolderItemsAsync("0", 500);
            items.Entries.ForEach(i => 
            {
                Console.WriteLine("\t{0}", i.Name);
                //if (i.Type == "file")
                //{
                //    var preview_link = adminClient.FilesManager.GetPreviewLinkAsync(i.Id).Result;
                //    Console.WriteLine("\tPreview Link: {0}", preview_link.ToString());
                //    Console.WriteLine();
                //}   
            });
            Console.WriteLine();

            var userRequest = new BoxUserRequest() { Name = "test appuser", IsPlatformAccessOnly = true };
            var appUser = await adminClient.UsersManager.CreateEnterpriseUserAsync(userRequest);
            Console.WriteLine("Created App User");

            var userToken = boxJWT.UserToken(appUser.Id);
            var userClient = boxJWT.UserClient(userToken, appUser.Id);

            var userDetails = await userClient.UsersManager.GetCurrentUserInformationAsync();
            Console.WriteLine("\nApp User Details:");
            Console.WriteLine("\tId: {0}", userDetails.Id);
            Console.WriteLine("\tName: {0}", userDetails.Name);
            Console.WriteLine("\tStatus: {0}", userDetails.Status);
            Console.WriteLine();

            await adminClient.UsersManager.DeleteEnterpriseUserAsync(appUser.Id, false, true);
            Console.WriteLine("Deleted App User");
        }
    }
}