public void Main()
        {


            string datetime = DateTime.Now.ToString("yyyyMMddHHmmss");

            try
            {

                //Declare Variables
                string FileLocation = Dts.Variables["User::FileLocation"].Value.ToString();
                string LogFolder = Dts.Variables["User::LogFolder"].Value.ToString();

                //SQL Connection Start
                SqlConnection myADONETConnection = new SqlConnection();
                myADONETConnection = (SqlConnection)(Dts.Connections["DBConn"].AcquireConnection(Dts.Transaction) as SqlConnection);
                //SQL Connection End

                //Getting Files From Directroy with Extension (.txt)
                DirectoryInfo drInputFolder = new DirectoryInfo(FileLocation);
                FileInfo[] InputFilesList = drInputFolder.GetFiles("*.txt");


                //Looping each file from Directory
                foreach (FileInfo fiInputFile in InputFilesList)
                {
                    //Generating Dynamic Schema.ini File in Directory ---> START
                    using (FileStream fs = new FileStream(Path.GetDirectoryName(FileLocation) + "\\schema.ini", FileMode.Create, FileAccess.Write))
                    {
                        string strInputFileFullPath = FileLocation + "\\" + fiInputFile;
                        string strInputFilename = fiInputFile.Name.Replace(".txt", "");


                        using (StreamReader sr = new StreamReader(fiInputFile.FullName))
                        {
                            int Col = 1;
                            string[] columnheaders = sr.ReadLine().Split('|');


                            StreamWriter sw = new StreamWriter(fs);
                            sw.WriteLine("[" + Path.GetFileName(fiInputFile.Name) + "]");
                            sw.WriteLine("ColNameHeader=True");
                            sw.WriteLine("Format=Delimited(|)");
                            sw.WriteLine("CharacterSet=ANSI");

                            foreach (string column in columnheaders)
                            {
                                
                                sw.WriteLine("Col"+ Col++ +"=\""+column+"\" TEXT");

                            }
                            sw.Close();
                            sw.Dispose();

                        }
                        fs.Close();
                        fs.Dispose();
                        //Generating Dynamic Schema.ini File in Directory ---> END


                        //Connection String For Text files
                        OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + Path.GetDirectoryName(strInputFileFullPath) + ";Extended Properties = 'text;HDR=Yes;IMEX=0'");
                        conn.Open();
                        string strPrefix = strInputFilename.Substring(2);
                        string strTableName = strPrefix.Substring(0, strPrefix.LastIndexOf("_"));

                       
                        string strQuery = "SELECT * FROM [" + Path.GetFileName(strInputFileFullPath) + "]";
                        OleDbDataAdapter adapter = new OleDbDataAdapter(strQuery, conn);
                        DataTable dataTable = new DataTable
                        {
                            Locale = CultureInfo.CurrentCulture
                        };
                        adapter.Fill(dataTable);


                        string StrCreateandDropQry = string.Empty;

                        //Generating SQL Query Based on Files Columns
                        StrCreateandDropQry += "IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = ";
                        StrCreateandDropQry += "OBJECT_ID (N'[dbo].[" + strTableName + "]') AND TYPE IN (N'U'))";
                        StrCreateandDropQry += "DROP TABLE [dbo].[" + strTableName + "]";
                        StrCreateandDropQry += "Create table [" + strTableName + "]";
                        StrCreateandDropQry += "(";
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            if (i != dataTable.Columns.Count - 1)
                                StrCreateandDropQry += "[" + dataTable.Columns[i].ColumnName + "] " + "Varchar(8000)" + ",";
                            else
                                StrCreateandDropQry += "[" + dataTable.Columns[i].ColumnName + "] " + "Varchar(8000)";
                        }
                        StrCreateandDropQry += ")";


                        //MessageBox.Show(StrCreateandDropQry);


                        
                        SqlCommand myCommand = new SqlCommand(StrCreateandDropQry, myADONETConnection);
                        myCommand.ExecuteNonQuery();


                        //Loading into SQL using SqlBulkCopy
                        SqlBulkCopy blk = new SqlBulkCopy(myADONETConnection)
                        {
                            BulkCopyTimeout = Convert.ToInt32(0),
                            DestinationTableName = "[" + strTableName + "]"
                        };

                        blk.WriteToServer(dataTable);
                        conn.Close();

                    }
                }
                Dts.TaskResult = (int)ScriptResults.Success;
            }
            catch (Exception exception)
            {
                using (StreamWriter sw = File.CreateText(Dts.Variables["User::LogFolder"].Value.ToString() + "\\" + "ErrorLog_" + datetime + ".log"))
                {
                    sw.WriteLine(exception.ToString());
                    Dts.TaskResult = (int)ScriptResults.Failure;
                }


            }

        }
    }
