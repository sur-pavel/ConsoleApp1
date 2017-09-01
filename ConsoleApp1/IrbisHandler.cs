using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedClient;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


namespace ConsoleApp1
{
    class IrbisHandler
    {
        ManagedClient64 client = new ManagedClient64();
        IrbisRecord currentRecord = new IrbisRecord();
        internal List<string> notFounded = new List<string>();
        internal string volNum = "";
        internal int pdfPages = 0;


        internal string GetNewFileName(FileInfo file)
        {

            string reformatFileName = ReformatFileName(file);

            string author = GetAuthor(reformatFileName);
            string title = GetTitle(reformatFileName);
            string[] titleKeyWords = GetTitleKeyWords(title);
            string year = GetYear(reformatFileName);

            string newFileName = "";

            try
            {
                if (!year.Equals(""))
                {
                    //  int[] foundRecords = SimpleSearch(client, author, title, year);

                    volNum = "";
                    Match m = Regex.Match(file.Name, @"[ЧТВК].? ?\d");
                    if (m.Success)
                    {
                        string value = m.Value;
                        volNum = Regex.Match(value, @"\d+").Value;
                    }
                    int[] foundRecordsMFN = SequentialSearch(client, author, titleKeyWords, year, volNum);

                    Log.WriteLine("Found records: " + foundRecordsMFN.Length);
                    if (foundRecordsMFN.Length < 1)
                    {
                        Log.WriteLine("NO RECORDS");
                        notFounded.Add(file.ToString());
                        int recordMFN = client.SequentialSearch(
                            string.Format("\"G={0}$\"", year),
                            string.Format("v951^a:'{0}'", file.Name))[0];
                        Log.WriteLine("Real MFN:" + recordMFN);

                    }
                    else
                    {
                        foreach (int recordMFN in foundRecordsMFN)
                        {
                            currentRecord = client.ReadRecord(recordMFN);
                            int pages = NumberOfPages();
                            Log.WriteLine("PDF Pages: " + pdfPages);
                            Log.WriteLine("Record Pages: " + pages);
                            if (!(pdfPages / pages > 1))
                            {
                                Log.WriteLine(string.Format("Record MFN: {0}", recordMFN));
                                newFileName = CreateNewFileName(recordMFN);
                            }
                        }

                        //newFileName = "ПРИКРЕПИТЬ";

                        //foreach (int frecord in foundRecordsMFN)
                        //{
                        //    currentRecord = client.ReadRecord(frecord);

                        //    foreach (string subField951a in GetSubFields(951, 'a'))
                        //    {
                        //        Log.WriteLine("Field 951: " + subField951a);
                        //        //if (dublicate.Contains("pdf") && dublicate.Contains(year) && dublicate.ToLower().Contains(titleKeyWords[0].ToLower()))
                        //        if (subField951a.Contains(file.Name))
                        //        {
                        //            //newFileName = dublicate.Replace(".pdf", " НПРК.pdf");
                        //            newFileName = "Прикреплен";
                        //            break;
                        //        }                                
                        //    }
                        //}

                        Log.WriteLine("Old file name: " + file.Name);
                        Log.WriteLine("New name from Irbis: " + newFileName);
                    }
                }


            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.StackTrace);
                Log.WriteLine(ex.ToString());
            }

            //Log.WriteLine(newFileName);
            return newFileName;
        }


        private string CreateNewFileName(int firstFoundRecord)
        {
            currentRecord = client.ReadRecord(firstFoundRecord);
            string newFileName = "";
            string lastName = "";
            string initials = "";
            string attribute = "";
            string title = "";
            string location = "";
            string year = "";
            string pages = "";
            if (volNum.Equals(""))
            {
                lastName = GetSubField(700, 'a');

                initials = GetSubField(700, 'b');
                initials = initials.Equals("") ? "" : " " + initials;
                attribute = GetAttribute(700);

                title = GetSubField(200, 'a');
                title = CheckTitleLength(title);

                location = "_" + GetSubField(210, 'a');

                year = GetSubField(210, 'd');

                pages = GetSubField(215, 'a');

            }

            else
            {
                lastName = GetSubField(961, 'a');

                initials = GetSubField(961, 'b');
                initials = initials.Equals("") ? "" : " " + initials;
                attribute = GetAttribute(961);

                title = GetSubField(461, 'c');
                title = CheckTitleLength(title);

                string volume = CleanWord(GetSubField(200, 'v')).Replace(" ", "");
                title = string.Format("{0}_{1}", title, volume);

                location = GetSubField(210, 'a');
                location = location.Equals("") ? GetSubField(461, 'd') : location;

                year = GetSubField(210, 'd');
                year = year.Equals("") ? GetSubField(461, 'h') : year;

                pages = GetSubField(215, 'a');
            }
            if (!title.Equals(""))
            {
                newFileName = string.Format(
                "{0}{1}{2}_{3}_{4}_{5}_{6}с",
                 lastName, initials, attribute, title, location.Replace(".", ""), year, pages
                );
            }

            return newFileName;
        }

        internal string ReformatFileName(FileInfo file)
        {
            string fileName = file.ToString()
                .Replace(".pdf", "")
                .Replace(".", ". ")
                .Replace("--", " ")
                .Replace("ο", "о")
                .Replace("o", "о")
                .Replace("-", " ");
            fileName = Regex.Replace(fileName, @"^\d_", "");

            Log.WriteLine("Reformated file name: " + fileName);

            return fileName;
        }

        internal bool IsAttached(FileInfo file)
        {
            string newFileName = GetNewFileName(file);
            return newFileName.Equals("Прикреплен") || newFileName.Equals("");
        }

        private static string CheckTitleLength(string title)
        {
            string[] titleWords = title.Split(' ');
            int wordsInTitle = 15;
            if (titleWords.Length > wordsInTitle)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < wordsInTitle; i++)
                {
                    builder.Append(titleWords[i]);
                    if (i != wordsInTitle - 1) builder.Append(" ");
                }
                builder.Append("...");
                title = builder.ToString();
            }
            return title;
        }

        private string GetSubField(int num, char code)
        {

            string field = num.ToInvariantString();
            string subField = currentRecord
                .Fields
                .GetField(field)
                .GetSubField(code)
                .GetSubFieldText()
                .FirstOrDefault();
            return subField == null ? "" : subField;
        }

        private string[] GetSubFields(int num, char code)
        {
            string field = num.ToInvariantString();
            string[] subFields = currentRecord
                .Fields
                .GetField(field)
                .GetSubField(code)
                .GetSubFieldText();
            return subFields;
        }
        private string GetAttribute(int field)
        {
            string subField = GetSubField(field, '1');
            string attribute = "";
            Log.WriteLine("Attribute: " + subField);
            if (subField.Contains("патр")) attribute = "патр.";
            if (subField.Contains("митр.")) attribute = "митр.";
            if (subField.Contains("архиеп")) attribute = "архиеп.";
            if (subField.Contains("еп")) attribute = "еп.";
            if (subField.Contains("архим")) attribute = "архим.";
            if (subField.Contains("игум")) attribute = "игум.";
            if (subField.Contains("иер")) attribute = "иер.";
            if (subField.Contains("иером")) attribute = "иером.";
            if (subField.Contains("мон")) attribute = "мон.";
            if (subField.Contains("прот")) attribute = "прот.";
            if (subField.Contains("протодиак")) attribute = "протодиак.";
            if (subField.Contains("архидиак")) attribute = "архидиак.";
            if (subField.Contains("прп")) attribute = "прп.";
            if (subField.Contains("свт")) attribute = "свт.";
            if (subField.Contains("мч")) attribute = "мч.";
            if (!attribute.Equals(""))
            {
                attribute = ", " + attribute;
            }
            return attribute;
        }



        private int[] SimpleSearch(ManagedClient64 client, string author, string title, string year)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("\"A={0}$\"", author);
            builder.AppendFormat("*\"T={0}$\"", title);
            builder.AppendFormat("*\"G={0}$\"", year);

            string searchTherm = builder.ToString();
            Log.WriteLine(searchTherm);

            return client.Search(searchTherm);
        }

        private int[] SequentialSearch(ManagedClient64 client, string author, string[] titleKeyWords, string year, string volNum)
        {
            List<string> filteredKeyWords = new List<string>();

            if (volNum.Equals(""))
            {
                filteredKeyWords.Add(string.Format("v700:'{0}'", author));
                int size = titleKeyWords.Length;
                if (size > 10) size = 10;
                for (int i = 0; i < size; i++)
                {
                    //byte[] asciiBytes = Encoding.ASCII.GetBytes(keyWord);
                    //foreach (byte asciiByte in asciiBytes)
                    //{
                    //    Log.WriteLine(asciiByte);
                    //}
                    string keyWord = titleKeyWords[i];

                    if (keyWord.Length > 1 && !keyWord.Contains("?"))
                    {
                        filteredKeyWords.Add(string.Format("v200:'{0}'", keyWord));
                    }
                }

                filteredKeyWords.Add("v900^b:'05'");

            }

            else
            {
                filteredKeyWords.Add(string.Format("v961:'{0}'", author));
                filteredKeyWords.Add(string.Format("v200^v:'{0}'", volNum));
                int size = titleKeyWords.Length;
                if (size > 10) size = 10;
                for (int i = 0; i < size; i++)
                {
                    //byte[] asciiBytes = Encoding.ASCII.GetBytes(keyWord);
                    //foreach (byte asciiByte in asciiBytes)
                    //{
                    //    Log.WriteLine(asciiByte);
                    //}
                    string keyWord = titleKeyWords[i];

                    if (keyWord.Length > 1 && !keyWord.Contains("?"))
                    {
                        filteredKeyWords.Add(string.Format("v461:'{0}'", keyWord));
                    }
                }

                filteredKeyWords.Add("v900^b:'03'");
            }



            string searchYear = string.Format("\"G={0}$\"", year);
            string searchTitle = String.Join(" and ", filteredKeyWords.ToArray());

            //searchTitle = "v200: 'Жизнь' and v200: 'митрополита'";

            Log.WriteLine("Therm for search title: " + searchTitle + "\n");
            Log.WriteLine("Therm for search year: " + searchYear + "\n");

            return client.SequentialSearch(
                searchYear,
                searchTitle
                );
        }




        private string GetAuthor(string fileName)
        {
            string author = fileName.Split('_')[0];
            author = CleanWord(author);
            author = author.Split(' ')[0];
            if (author != "") Log.WriteLine("Author: " + author);
            return author;
        }

        internal string GetTitle(string fileName)
        {
            string[] nameSplit = fileName.Split('_');
            string title = nameSplit.Length > 1 ? nameSplit[1] : "";
            //if (title != "") Log.WriteLine("Title: " + title + "\n");
            return title;
        }

        internal string[] GetTitleKeyWords(string title)
        {
            string titleClean = CleanWord(title);


            IStemmer stemmer = new RussianStemmer();
            string[] titleKeyWords = titleClean.Split(' ');
            List<string> keyWords = new List<string>();
            foreach (string titleKeyWord in titleKeyWords)
            {
                string keyWord = stemmer.Stem(titleKeyWord);
                if (keyWord.Length > 2)
                {
                    keyWords.Add(keyWord);
                }
            }
            //if (keyWords.Count > 2)
            //{
            //    Log.WriteLine(string.Format("{0}, {1}, {2}", keyWords.ElementAt(0), keyWords.ElementAt(1), keyWords.ElementAt(2)));
            //} 
            return keyWords.ToArray();
        }

        internal string GetYear(string fileName)
        {
            string year = "";
            Regex regex = new Regex(@"_\d\d\d\d");
            MatchCollection matches = regex.Matches(fileName);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    year = match.Value.Replace("_", "");
                }
            }
            //if (year != "") Log.WriteLine(year);
            return year;
        }

        internal void ConnectToServer()
        {
            try
            {
                //client.ParseConnectionString("host=127.0.0.1;port=8888; user=1;password=1;");
                client.ParseConnectionString("host=194.169.10.3;port=8888; user=СПА;password=1;");
                client.Connect();
                client.PushDatabase("MPDA");
                //Log.WriteLine("Записей в базе: {0}", client.GetMaxMfn() - 1);

            }
            catch (Exception ex)
            {
                Log.WriteLine("IRBIS ERROR!");
                Log.WriteLine(ex.StackTrace);
                Log.WriteLine(ex.ToString());
            }
        }

        internal void Disconnect()
        {
            try
            {
                client.Disconnect();
            }
            catch (Exception ex)
            {
                Log.WriteLine("IRBIS ERROR!");
                Log.WriteLine(ex.StackTrace);
                Log.WriteLine(ex.ToString());
            }
        }

        internal void AdvancedSearch()
        {
            //filteredKeyWords.Add("v900:'^B05'");
            //string searchYear = string.Format("\"G={0}$\"", year);
            //string searchTitle = String.Join(" and ", filteredKeyWords.ToArray());
            //string searchTitle =
            string searchYear = "";
            string searchTitle = "v961: 'ДА'";
            // |961 (&uf('Av961^z#1')='ДА') and (&uf('Av961^z#2')='ДА')
            Log.WriteLine("Therm for search title: " + searchTitle);
            Log.WriteLine("Therm for search year: " + searchYear);
            List<int> selected = new List<int>();
            int[] foundRecordsMFN = client.SequentialSearch(
                searchYear,
                searchTitle
                );
            foreach (int frecord in foundRecordsMFN)
            {
                int count = 0;
                currentRecord = client.ReadRecord(frecord);
                foreach (string subField961z in GetSubFields(961, 'z'))
                {
                    if (subField961z.Contains("ДА"))
                    {
                        count++;
                    }
                }

                if (count > 1) selected.Add(frecord);
            }

            Log.WriteLine("Founded " + selected.Count + " items.");
            foreach (int num in selected)
            {
                Log.WriteLine(num + "");
            }
        }

        private string CleanWord(string word)
        {
            return Regex.Replace(word, "[-.?!)(,:]", "");
        }

        private int NumberOfPages()
        {
            int numberOfPages = 0;
            string pages = GetSubField(215, 'a');
            try
            {
                numberOfPages = int.Parse(pages);
            }
            catch (FormatException e)
            {
                if (!pages.Contains(","))
                {
                    numberOfPages = RomanToInteger(pages);
                }
                else
                {
                    pages = pages.Replace(",", " ").Replace("  ", " ");
                    string[] pagesArr = pages.Split(' ');
                    List<int> nums = new List<int>();
                    foreach (string page in pagesArr)
                    {
                        string pageAmount = page.Replace(" ", "");
                        try
                        {
                            
                            nums.Add(int.Parse(pageAmount));
                        }
                        catch (FormatException e2)
                        {
                            nums.Add(RomanToInteger(pageAmount));
                        }
                    }

                    foreach(int num in nums)
                    {
                        numberOfPages = numberOfPages + num;
                    }
                }
            }

            return numberOfPages;
        }
        private static Dictionary<char, int> RomanMap = new Dictionary<char, int>()
        {
            { 'I', 1},
            { 'V', 5},
            { 'X', 10},
            { 'L', 50},
            { 'C', 100},
            { 'D', 500},
            { 'M', 1000}
        };

        public static int RomanToInteger(string roman)
        {
            int number = 0;
            for (int i = 0; i < roman.Length; i++)
            {
                if (i + 1 < roman.Length && RomanMap[roman[i]] < RomanMap[roman[i + 1]])
                {
                    number -= RomanMap[roman[i]];
                }
                else
                {
                    number += RomanMap[roman[i]];
                }
            }
            return number;
        }
    }

}
