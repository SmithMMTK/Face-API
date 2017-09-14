using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace Face_API
{
    class Program
    {

        static string subscriptionkey;
        static string uribase;
        static string choice;
        static bool inputMode; // if True = wati for input

        /// <summary>
        /// Configuration file structure in Json format
        /// </summary>
        public class Configjson
        {
            public string subscriptionkey { get; set; }
            public string uriBase { get; set; }
        }

       

        static void Main()
        {
            inputMode = true;
            

            Console.WriteLine("     #####################################################################################");
            Console.WriteLine("     :Starting Azure Face API console application");
            Console.WriteLine("     #####################################################################################");
            if (loadConfig())
            {
                choice = "default";
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("     :Loading configuration file success");
                Console.WriteLine("     :Subscription Key: " + subscriptionkey);
                Console.WriteLine("     :Uri Base: " + uribase);
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Gray;
                // Load faceServiceClient from parameter in json 'config.json'
                FaceServiceClient faceServiceClient = new FaceServiceClient(subscriptionkey,uribase);


                paintMenu();

                
                if (inputMode)
                {
                    Console.Write("> ");
                    choice = Console.ReadLine();
                }


                //while(!string.ReferenceEquals(choice.ToLower(),"q"))
                while (choice.ToLower() != ".exit")
                {
                    switch (choice.ToLower())
                    {
                        case ".f":
                            choice = "default";
                            FaceDetection(faceServiceClient);
                            break;
                        case ".c":
                            choice = "default";
                            CreateEmptyPersonGroup(faceServiceClient);
                            break;
                        case ".l":
                            choice = "default";
                            ListPersonGroup(faceServiceClient);
                            break;
                        case ".d":
                            choice = "default";
                            DeletePersonGroup(faceServiceClient);
                            break;
                        case ".a":
                            choice = "default";
                            AddFaceIntoPersonGroup(faceServiceClient);
                            break;
                        case ".m":
                            choice = "default";
                            paintMenu();
                            if (inputMode)
                            {
                                Console.Write("> ");
                                choice = Console.ReadLine();
                            }
                            break;
                        default:
                            
                            if (inputMode)
                            {
                                Console.Write("> ");
                                choice = Console.ReadLine();
                            }
                            break;
                    }
                                        
                }
            }
            else
            {
                Console.WriteLine(":Loading configuration file failure");
            }
            
                        
        }

        static async void AddFaceIntoPersonGroup(FaceServiceClient faceCli)
        {

            inputMode = false;

            Console.WriteLine(":Add face:");
            Console.WriteLine(":Enter the path to an image with faces that you wish to analzye: ");
            Console.Write("$ ");
            //string imageFilePath = "c:\\temp\\smithm1.jpg";

            string imageFilePath = Console.ReadLine();
            try
            {
                using (Stream s = File.OpenRead(imageFilePath))
                {
                    
                    Console.WriteLine(":Enter the person name");
                    string personName = Console.ReadLine();
                    var persons = await faceCli.ListPersonsAsync(personName);
                    Guid personid;
                    foreach (var person in persons)
                    {
                        if (person.Name == personName)
                            personid = person.PersonId;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Group Id: " + person.PersonId);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        await faceCli.AddPersonFaceAsync(personName, person.PersonId, s);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Add " + personName + " face completed");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    //await Task.Delay(1000);

                    await faceCli.TrainPersonGroupAsync(personName);

                    TrainingStatus trainingStatus = null;
                    while (true)
                    {
                        trainingStatus = await faceCli.GetPersonGroupTrainingStatusAsync(personName);
                        
                        if (trainingStatus.Status.ToString() != "running")
                        {
                            break;
                        }
                        
                        await Task.Delay(1000);
                    }

                    inputMode = true;
                    paintMenu();
                }

            }
            catch (Exception ex)
            {
                inputMode = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                paintMenu();
            }
          


            // Execute the REST API call.
            // MakeAnalysisRequest(imageFilePath);

            

        }

        /// <summary>
        ///  Create Empty Person Group
        /// </summary>
        /// <param name="faceCli"></param>
        static async void CreateEmptyPersonGroup(FaceServiceClient faceCli)
        {
            Console.WriteLine("");
            Console.Write("Enter the name of person you wish to create: ");
            string personGroupId = Console.ReadLine();

            inputMode = false; 

            try
            {
                // Create Person Group e.g. My friend
                await faceCli.CreatePersonGroupAsync(personGroupId, "Custom group");

                // Create Person in Person Group e.g. My friend -> SmithM
                // in this example we use same e.g. Smith -> SmithM
                CreatePersonResult friend1 = await faceCli.CreatePersonAsync(personGroupId, personGroupId);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Person Name within Person Group : " + personGroupId + " successfully created");
                Console.ForegroundColor = ConsoleColor.Gray;
                inputMode = true;
                paintMenu();
            }
            catch (FaceAPIException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                inputMode = true;
                Console.WriteLine(ex.ErrorMessage);
                Console.ForegroundColor = ConsoleColor.Gray;
                paintMenu();
            }

        }

        static async void DeletePersonGroup(FaceServiceClient faceCli)
        {
            inputMode = false;

            var groups = await faceCli.ListPersonGroupsAsync();
           

            

            foreach (var group in groups)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Group Id: " + group.PersonGroupId);
                Console.ForegroundColor = ConsoleColor.Gray;
            }


            Console.WriteLine("");
            Console.WriteLine("Enter the name of person you wish to delete: ");
            string personGroupId = Console.ReadLine();
            
            try
            {
                await faceCli.DeletePersonGroupAsync(personGroupId);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Group : " + personGroupId + " successfully deleted");
                Console.ForegroundColor = ConsoleColor.Gray;
                inputMode = true; 
                paintMenu();

            }
            catch (FaceAPIException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ErrorMessage);
                Console.ForegroundColor = ConsoleColor.Gray;
                inputMode = true;
                paintMenu();
            }
            
        }


        static async void ListPersonGroup(FaceServiceClient faceCli)
        {
            // Lock input
            inputMode = false; 
            Console.WriteLine("");
            Console.WriteLine("Getting Person Group:");
            var groups = await faceCli.ListPersonGroupsAsync();

            foreach(var group in groups)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Group Id: " + group.PersonGroupId);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            paintMenu();
            // release input
            inputMode = true;
        }

        /// <summary>
        /// Face Detection to get face object that return person face in picture 
        /// </summary>
        /// <param name="faceCli"></param>
        static async void FaceDetection(FaceServiceClient faceCli)
        {
            // Lock input
            inputMode = false;
            bool fileNotFound = true;
            bool faceFound = false;
            var requiredFaceAttributes = new FaceAttributeType[] {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                FaceAttributeType.Smile,
                FaceAttributeType.FacialHair,
                FaceAttributeType.HeadPose,
                FaceAttributeType.Glasses
            };

            // Get the path and filename to process from the user.
            Console.WriteLine(":Detect faces:");
            Console.WriteLine(":Enter the path to an image with faces that you wish to analzye: ");
            Console.Write("$ ");
           // string imageFilePath = "c:\\temp\\smithm1.jpg";
            string imageFilePath = Console.ReadLine();

            Console.WriteLine("\n:Please wait a moment for the results to appear ...\n");

            // Execute the REST API call.
            // MakeAnalysisRequest(imageFilePath);
            try
            {
                using (Stream s = File.OpenRead(imageFilePath))
                {
                    var faces = await faceCli.DetectAsync(s, returnFaceLandmarks: true, returnFaceAttributes: requiredFaceAttributes);
                    fileNotFound = false;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(":Found " + faces.Count() + " face(s)." );
                    Console.ForegroundColor = ConsoleColor.Gray;

                    foreach (var face in faces)
                    {
                        var rect = face.FaceRectangle;
                        var landmarks = face.FaceLandmarks;
                        var id = face.FaceId;
                        var attributes = face.FaceAttributes;
                        var age = attributes.Age;
                        var gender = attributes.Gender;
                        var smile = attributes.Smile;
                        var facialHair = attributes.FacialHair;
                        var headPose = attributes.HeadPose;
                        var glasses = attributes.Glasses;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("     Id: " + face.FaceId);
                        Console.WriteLine("     Age: " + attributes.Age);
                        Console.WriteLine("     gender: " + attributes.Gender);
                        Console.WriteLine("     smile: " + attributes.Smile);
                        Console.WriteLine("     glasses: " + attributes.Glasses);
                        Console.WriteLine(" ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    var faceIds = faces.Select(face => face.FaceId).ToArray();

                    var groups = await faceCli.ListPersonGroupsAsync();

                    foreach (var group in groups)
                    {
                        var results = await faceCli.IdentifyAsync(group.PersonGroupId, faceIds);
                        foreach (var identifyResult in results)
                        {
                            
                            if (identifyResult.Candidates.Length == 0)
                            {

                            }
                            else
                            {
                                // Get top 1 among all candidates returned
                                var candidateId = identifyResult.Candidates[0].PersonId;
                                var person = await faceCli.GetPersonAsync(group.PersonGroupId, candidateId);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Found : {0}", person.Name);
                                Console.WriteLine("      : {0} trained picture(s) in database",person.PersistedFaceIds.Count());
                                Console.ForegroundColor = ConsoleColor.Gray;
                                faceFound = true;
                            }
                        }


                    }

                    if (!faceFound)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No face match in database.");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    Console.WriteLine("");
                    inputMode = true;
                    paintMenu();
                }
            }
            catch (Exception ex)
            {
                inputMode = true;
                if (fileNotFound)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    if (!faceFound)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No face match in database.");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                                      
                }
                
                paintMenu();
            }
           
            
           


        }


        /// <summary>
        /// Loading configuration from config.json into parameter
        /// 
        /// </summary>
        /// <returns></returns>
        static bool loadConfig()
        {
            string configFile = "config.json";
            // Request body. Posts a locally stored JPEG image.

            Console.WriteLine("");
            Console.WriteLine("     :Loading configuration file 'config.json'");

            try
            {
                configFile = System.IO.File.ReadAllText(@"config.json");
                Configjson configdata = JsonConvert.DeserializeObject<Configjson>(configFile);
                subscriptionkey = configdata.subscriptionkey;
                uribase = configdata.uriBase;
                return true;
            }
            catch
            {
                return false;
            }


        }

        static void paintMenu()
        {
            choice = "default";
            Console.WriteLine(" ");

            Console.WriteLine("     (.F) Face Detection");
            Console.WriteLine("     (.A) Add Face into Person        (.D) Delete Person          (.L) List All Person(s)");
            Console.WriteLine("");
            Console.WriteLine("     (.M) Menu");
            Console.WriteLine("     (.Exit) for Exit");
            Console.WriteLine("     --------------------------------------------------------------------------------------");
            Console.WriteLine("     Enter command:");
            Console.WriteLine(" ");

            
        }
    }
}
