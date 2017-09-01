using System;
using System.IO;


namespace ConsoleApp1
{
    class Program
    {
        FileHandler fileHandler = new FileHandler();
        IrbisHandler irbisHandler = new IrbisHandler();

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Perform();
            //program.CompareFileNames();
        }



        private void Perform()
        {
            
            fileHandler.CreateLogFile();
            FileInfo[] files = fileHandler.GetOldFileNames();
            
            irbisHandler.ConnectToServer();
            //irbisHandler.AdvancedSearch();

            foreach (FileInfo file in files)
            {
                string newFileName = irbisHandler.GetNewFileName(file);

                irbisHandler.pdfPages = fileHandler.GetPages(file);
                if (newFileName != "")
                {
                    //fileHandler.RenameFile(file, newFileName);
                }


                //bool attached = irbisHandler.IsAttached(file);
                //if (!attached)
                //{
                //    fileHandler.RenameNotAttached(file);
                //}


                Log.WriteLine("------------------------------------\n");
            }

            irbisHandler.Disconnect();
            PrintNotFounded();

            //PrintProducers();
            //Console.ReadKey();
        }



        private void PrintNotFounded()
        {
            Log.WriteLine("\nNOT FOUNDED:");
            foreach (string fileName in irbisHandler.notFounded)
            {
                Log.WriteLine(fileName);
            }
        }

        private void PrintProducers()
        {            
            foreach (string producer in fileHandler.producers)
            {
                Log.WriteLine(producer);
            }
        }

        private void CompareFileNames()
        {

            fileHandler.CreateLogFile();
            FileInfo[] files = fileHandler.GetOldFileNames();
            FileInfo[] files2 = fileHandler.GetOldFileNames2();

            foreach (FileInfo file in files)
            {
                string reformatFileName = irbisHandler.ReformatFileName(file);
                string title = irbisHandler.GetTitle(reformatFileName);
                string year = irbisHandler.GetYear(reformatFileName);
                string[] titleKeyWords = irbisHandler.GetTitleKeyWords(title);
                if (titleKeyWords.Length <= 2)
                {
                    Log.WriteLine("Title: " + title);
                    Log.WriteLine(file.Name);
                }

                if (titleKeyWords.Length > 2)
                {
                    foreach (FileInfo file2 in files2)
                    {
                        string lowerFile2Name = file2.Name.ToLower();
                        if (lowerFile2Name.Contains(year)
                            && lowerFile2Name.Contains(titleKeyWords[0])
                            && lowerFile2Name.Contains(titleKeyWords[1])
                            && lowerFile2Name.Contains(titleKeyWords[2])
                            )
                        {
                            string log = string.Format("FILE\n{0}\nIS DUPLICATE of\n{1}\n-----------\n",
                                file.Name, file2.Name);
                            Log.WriteLine(log);
                            //fileHandler.RenameFile2(file, file2);
                        }

                    }
                }
            }
        }
    }
}
