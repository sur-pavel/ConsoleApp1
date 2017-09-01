using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;



namespace ConsoleApp1
{
    class FileHandler
    {

        internal List<string> producers = new List<string>();
        string path = @"d:\TestDir\0\";
        //string path = @"z:\_Для прикрепа\Новь\";
        //string path = @"z:\_Для прикрепа\Новь\3_,4_\";
        


        internal FileInfo[] GetOldFileNames()
        {
            DirectoryInfo info = new DirectoryInfo(path);

            FileInfo[] files = info.GetFiles();

            return files;
        }

        internal FileInfo[] GetOldFileNames2()
        {
            string path2 = @"z:\_Для прикрепа\ОПАК-Глобал\";
            DirectoryInfo info = new DirectoryInfo(path2);

            FileInfo[] files = info.GetFiles();

            return files;
        }

        internal void CreateLogFile()
        {
            FileStream fs = File.Create(@"d:\TestDir\" + "search.log");
            fs.Close();            
        }
        internal int GetPages(FileInfo file)
        {           
            PdfReader pdfReader = new PdfReader(file.FullName);
            int pages = pdfReader.NumberOfPages;

            float width = pdfReader.GetPageSize(1).Width;
            float height = pdfReader.GetPageSize(1).Height;
            if (width > height)
            {
                pages = pages * 2;
            }
            pdfReader.Close();
            return pages;
        }
        internal void RenameFile(FileInfo file, string newFileName)
        {
            //PdfReader pdfReader = new PdfReader(file.FullName);
            StringBuilder builder = new StringBuilder();
            //string num = "3";
            //if (ValidPDFProducer(pdfReader))
            //{
            //    num = "4";
            //}
            string pages = "";
            Match m = Regex.Match(newFileName, @"_[\d]+[\,с]");
            if (m.Success)
            {
                pages = m.Value.Replace("_", "").Replace("с", "").Replace(",", "");
                // Log.WriteLine("'{0}' found at position {1}", pages, m.Index);

            }


            if (newFileName.Contains(" НПРК.pdf"))
            {
               // pdfReader.Close();
                File.Move(file.FullName, file.FullName.Replace(".pdf", " НПРК.pdf"));
            }





            //if ((!pages.Equals("") && pdfReader.NumberOfPages >= int.Parse(pages)) || newFileName.Contains(" НПРК.pdf"))
            //{
            //    if (!newFileName.Contains(" НПРК.pdf"))
            //    {
            //        builder
            //            .Append(num)
            //            .Append("_")
            //            .Append(newFileName)
            //            .Append(".pdf");
            //        newFileName = builder.ToString();
            //    }
            Log.WriteLine("New file name: " + newFileName);
            //    pdfReader.Close();
            //    try
            //    {
            //     //   File.Move(file.FullName, path + newFileName);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.WriteLine("EXCEPTION: " + ex);
            //        if (File.Exists(path + newFileName))
            //        {
            //       //     File.Move(file.FullName, path + newFileName.Replace(".pdf"," НПРК.pdf"));
            //        }

            //    }
            //}
            //if (!pages.Equals("") && pdfReader.NumberOfPages < int.Parse(pages))
            //{
            //    Log.WriteLine("Too few pages in scan");
            //}
            //if (pages.Equals(""))
            //{
            //    Log.WriteLine("No pages in fileName");
            //}

            Log.WriteLine("------------------------------------\n");
        }

        internal void RenameFile2(FileInfo file, FileInfo file2)
        {
            try
            {
                File.Move(file.FullName, path + file2.Name.Replace(".pdf", " НПРК.pdf"));
            }
            catch (Exception ex)
            {
                Log.WriteLine("EXCEPTION: " + ex);
                //if (File.Exists(path + newFileName))
                //{
                //    //     File.Move(file.FullName, path + newFileName.Replace(".pdf"," НПРК.pdf"));
                //}

            }
        }

        internal void RenameNotAttached(FileInfo file)
        {
            try
            {
                File.Move(file.FullName, file.FullName.Replace(".pdf", " ПРИКРЕПИТЬ.pdf"));
            }
            catch (Exception ex)
            {
                Log.WriteLine("EXCEPTION: " + ex);              
            }
        }

        private bool ValidPDFProducer(PdfReader pdfReader)
        {
            bool validProducer = false;

            try
            {
                string producer = pdfReader.Info["Producer"];
                Log.WriteLine(producer);
                producers.Add(producer);
                if (
                    producer.Contains("Google")
                    // || (producer.Contains("PDFScanLib 1.2.2") && producer.Contains("Adobe Acrobat 10.1.3")) ||
                    //producer.Contains("Adobe Acrobat 9.5.5 Image Conversion Plug-in") ||
                    //producer.Contains("iText 1.4.6(by lowagie.com)")
                    )
                {
                    validProducer = true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
            }
            return validProducer;
        }
    }

}


