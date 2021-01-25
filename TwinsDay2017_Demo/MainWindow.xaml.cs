using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics.Gui;
using Neurotec.Images;
using Neurotec.IO;
using Neurotec.Licensing;
using Neurotec.Media;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace TwinsDay2017_Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables

            #region Neurotec Variables

            public const int Port = 5000;
            public const string Address = "/local";
            private string[] faceLicenseComponents = { "Biometrics.FaceExtractionFast", "Biometrics.FaceMatchingFast", "Biometrics.FaceSegmentsDetection" };
            private static NBiometricClient mainClient;
            private NBiometricTask enrollment = new NBiometricTask(NBiometricOperations.Enroll);

            #endregion

            #region Subject Variables

            private NSubject identSubject;
            private NFaceView faceIdent;
            private NSubject[] referenceSubject;

            private NSubject verSubjectLeft;
            private NSubject verSubjectRight;
            private NFaceView faceVerLeft;
            private NFaceView faceVerRight;

            private NSubject verifySubject;
            private NFaceView faceVerify;

            #endregion

            #region Matching Lists/Dictionaries

            private List<Match> matchList;
            private HashSet<string> recordList;

            #endregion

            #region Subject Scan Lists

            private List<string> identScanFiles;
            private List<string> leftScanFiles;
            private List<string> rightScanFiles;
            private List<string> verifyScanFiles;

            #endregion

            #region Settings and Default Variables

            private string[] defaultMatches = { "5", "10", "15", "20", "30" };
            private double[] defaultFAR = { 0.01, 0.001, 0.0001, 0.00001 };
            private Settings curSettings;
            private Settings defaultSettings;

            private const double defMatchFAR = 0.01;
            private const int defNumMatches = 5;
            private const string defExt = ".jpg";
            private const string FileFilter = "Images|*.jpg;*.png;*.bmp|Templates|*.dat";
            private static string TWD17_Server = @"D:\TWD17_SERVER\";
            private static string DataFolder = @"D:\TWD17_SERVER\TWD17 Applications (Deployable)\DEPLOY\Demo Data";
            private string TEMPLATEFOLDER = DataFolder + @"\Face";
            private string IMGFOLDER = DataFolder + @"\Face Img";
            private List<string> defaultDatabases = new List<string>{ @"\\192.168.5.10\R\TwinsDay 2015\TriRig\08092015",
                                                                      @"\\192.168.5.10\R\TwinsDay 2015\TriRig\08082015",
                                                                      @"\\192.168.5.10\X\Previous_Collections\Twins2016\FACE",
                                                                      @"\\192.168.5.10\R\TwinsDay2014\Face\Canon EOS 5D Mark III",
                                                                      TWD17_Server + "FACE"};

            #endregion

            #region Backgroundworker/Process Variables

            private BackgroundWorker progress;
            private BackgroundWorker identifyProgress;
            private BackgroundWorker matchProgress;
            private int templateCount;
            private int matchCount;
            private int identifyCount;
            private int identifyMax;

            #endregion

            #region Other Variables

            // Static Elements
            private static int lineNum = 1;

            // Semaphores
            private Semaphore templateWriteHold;

            #endregion

        #endregion

        #region MainWindow Constructor

            public MainWindow()
            {
                /* Default Settings Construction */
                defaultSettings = new Settings(defMatchFAR * 100, defaultDatabases, defNumMatches, GetScore(defMatchFAR));

                /* Current Settings Construction */
                curSettings = new Settings(defaultSettings);

                // Start the component initialization for the WPF/XAML application
                InitializeComponent();

                // Initialize the match list and set the corresponding listbox item source to the match list
                matchList = new List<Match>();
                TopMatchList.ItemsSource = matchList;

                // Initialize the subject record dictionary
                recordList = new HashSet<string>();

                // Initialize the scanner file record list(s)
                identScanFiles = new List<string>();
                leftScanFiles = new List<string>();
                rightScanFiles = new List<string>();
                verifyScanFiles = new List<string>();

                // Initialize all of the progress bar/background workers
                progress = new BackgroundWorker();
                progress.WorkerReportsProgress = true;
                progress.WorkerSupportsCancellation = true;
                progress.DoWork += new DoWorkEventHandler(template_DoWork);
                progress.ProgressChanged += new ProgressChangedEventHandler(template_ProgressChanged);
                progress.RunWorkerCompleted += new RunWorkerCompletedEventHandler(template_RunWorkerCompleted);

                identifyProgress = new BackgroundWorker();
                identifyProgress.WorkerReportsProgress = true;
                identifyProgress.WorkerSupportsCancellation = true;
                identifyProgress.DoWork += new DoWorkEventHandler(identify_DoWork);
                identifyProgress.ProgressChanged += new ProgressChangedEventHandler(identify_ProgressChanged);
                identifyProgress.RunWorkerCompleted += new RunWorkerCompletedEventHandler(identify_RunWorkerCompleted);

                matchProgress = new BackgroundWorker();
                matchProgress.WorkerReportsProgress = true;
                matchProgress.WorkerSupportsCancellation = true;
                matchProgress.DoWork += new DoWorkEventHandler(matchProgress_DoWork);
                matchProgress.ProgressChanged += new ProgressChangedEventHandler(matchProgress_ProgressChanged);
                matchProgress.RunWorkerCompleted += new RunWorkerCompletedEventHandler(matchProgress_RunWorkerCompleted);

                // Initialize any semaphores/thread holds we may use
                templateWriteHold = new Semaphore(1, 1);

                // Initialize each table layout used in the WPF form
                System.Windows.Forms.TableLayoutPanel leftPanel = this.LeftImagePanel.Child as System.Windows.Forms.TableLayoutPanel;
                System.Windows.Forms.TableLayoutPanel rightPanel = this.RightImagePanel.Child as System.Windows.Forms.TableLayoutPanel;
                System.Windows.Forms.TableLayoutPanel probePanel = this.ProbeImagePanel.Child as System.Windows.Forms.TableLayoutPanel;
                System.Windows.Forms.TableLayoutPanel verifyPanel = this.VerifyImagePanel.Child as System.Windows.Forms.TableLayoutPanel;
                leftPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                rightPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                probePanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                verifyPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

                // Initialize each face view object
                faceVerLeft = new NFaceView();
                faceVerRight = new NFaceView();
                faceIdent = new NFaceView();
                faceVerify = new NFaceView();

                // Add each face view object to its corresponding winforms panel (aka our TableLayoutPanel)
                leftPanel.Controls.Add(faceVerLeft, 0, 0);
                rightPanel.Controls.Add(faceVerRight, 0, 0);
                probePanel.Controls.Add(faceIdent, 0, 0);
                verifyPanel.Controls.Add(faceVerify, 0, 0);

                // Left Face View Properties
                faceVerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
                faceVerLeft.Face = null;
                faceVerLeft.FaceIds = null;
                faceVerLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

                // Right Face View Properties
                faceVerRight.Dock = System.Windows.Forms.DockStyle.Fill;
                faceVerRight.Face = null;
                faceVerRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

                // Probe Image Face View Properties
                faceIdent.Dock = System.Windows.Forms.DockStyle.Fill;
                faceIdent.Face = null;
                faceIdent.FaceIds = null;
                faceIdent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

                // Verify Image Face View Properties
                faceVerify.Dock = System.Windows.Forms.DockStyle.Fill;
                faceVerify.Face = null;
                faceVerify.FaceIds = null;
                faceVerify.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

                /* Loading all of the settings tab items */

                // # of matches ComboBox
                for (int index = 0; index < defaultMatches.Length; ++index)
                    NumOfMatches.Items.Add(defaultMatches[index]);

                // FAR ComboBox
                for (int i = 0; i < defaultFAR.Length; i++)
                    setMatchFAR.Items.Add(defaultFAR[i] * 100);

                // Check access of each of the default databases
                foreach (string uncPath in defaultDatabases)
                    GetAccess(uncPath);

                // Set each of the settings text boxes to their respective values
                NumOfMatches.Text = defNumMatches.ToString();
                setMatchFAR.Text = (defMatchFAR * 100).ToString();
                setThreshold.Text = GetScore(defMatchFAR).ToString();
                ImportDatabases(defaultDatabases);

                // Make sure buttons are not enabled after loading the changes
                RevertSetting.IsEnabled = false;
                DefaultSetting.IsEnabled = false;

                Log("");
                Log("Window Initialized");

                // Begin the license obtaining process and the initilization of the biometric client
                Start();
            }

        #endregion

        #region Identification Functions

            #region Import/Clear/Reset Functions

                /// <summary>
                /// Starts the template import process for the probe image
                /// </summary>
                private void ImportFile(object sender, RoutedEventArgs e)
                {
                    faceIdent.Face = null;
                    //faceIdent.ShowEmotions = true;
                    //faceIdent.ShowGender = true;
                    //faceIdent.ShowExpression = true;
                    //faceIdent.ShowProperties = true;
                    //faceIdent.ShowEmotions = true;
                    //faceIdent.ShowAge = true;
                    identSubject = null;

                    matchList.Clear();
                    TopMatchList.Items.Refresh();
                    IdentificationReset.IsEnabled = false;

                    Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

                    openPicker.DefaultExt = defExt;
                    openPicker.Filter = FileFilter;

                    if (openPicker.ShowDialog() == true)
                    {
                        IdentificationImportPath.Text = openPicker.FileName;
                        GetTemplate(faceIdent, openPicker.FileName, out identSubject);

                        ParticipantLabel.Content = "Participant: " + identSubject.Id;

                        // For testing purposes
                        //string testID = "1330344";
                        //ParticipantLabel.Content = string.Format("Participant: {0}({1})", testID, identSubject.Id);
                        //identSubject.Id = testID;

                        EnableIdentify();

                        Log("Probe Image Imported");

                        IdentificationClearWindow.IsEnabled = true;
                        IdentifyScan.IsEnabled = false;
                        IdentificationScanPath.IsEnabled = false;
                    }

                }

                /// <summary>
                /// Clears the identification tab/window
                /// </summary>
                private void IdentificationClearWindow_Click(object sender, RoutedEventArgs e)
                {
                    IdentificationImportPath.Text = string.Empty;
                    ParticipantLabel.Content = "Waiting for Import";

                    faceIdent.Face = null;
                    identSubject = null;
                    matchList.Clear();
                    TopMatchList.Items.Refresh();

                    GenerateMatches.IsEnabled = false;
                    IdentificationClearWindow.IsEnabled = false;
                    IdentificationReset.IsEnabled = false;

                    IdentifyImport.IsEnabled = true;
                    IdentificationImportPath.IsEnabled = true;

                    IdentifyScan.IsEnabled = true;
                    IdentificationScanPath.IsEnabled = true;
                    IdentificationScanPath.Text = "";

                    identScanFiles.Clear();
                    IdentifyImageSelect.Items.Clear();
                    IdentifyImageSelect.IsEnabled = false;
                    IdentifySelect.IsEnabled = false;
                    IdentifyDeselect.IsEnabled = false;

                    Log("");
                    Log("Cleared Identification Page");
                }

                /// <summary>
                /// When the reset button is clicked; resets the match list
                /// </summary>
                private void IdentificationReset_Click(object sender, RoutedEventArgs e)
                {
                    matchList.Clear();
                    TopMatchList.Items.Refresh();

                    IdentificationReset.IsEnabled = false;

                    Log("");
                    Log("Reset Identification Results");
                }

            #endregion

            #region Main Process Functions

                /// <summary>
                /// The work method for the identification background process
                /// </summary>
                private void identify_DoWork(object sender, DoWorkEventArgs e)
                {
                    // Get all face templates
                    string[] allTemplates = Directory.GetFiles(TEMPLATEFOLDER, "*.dat");

                    if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                    else
                    {
                        identifyMax = allTemplates.Length;

                        this.Dispatcher.Invoke(() =>
                            {
                                // Clear list
                                matchList.Clear();
                                TopMatchList.Items.Refresh();

                                // Set the maximum for the identification progress bar
                                IdentificationStatus.Maximum = allTemplates.Length + 10;
                            });


                        if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                        else
                        {
                            // Perform enrollment of all templates
                            EnrollTemplates(allTemplates);

                            // Update our identification background worker/progress bar
                            identifyCount += 10;
                            (sender as BackgroundWorker).ReportProgress(identifyCount, "Enrollment Completed");
                            Thread.Sleep(5);

                            if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; }
                            else if (identSubject != null && referenceSubject != null && referenceSubject.Length > 0)
                            {
                                // Matching threshold will be specified by the user input FAR/Threshold value
                                mainClient.MatchingThreshold = (byte)curSettings.GetThreshold();

                                // Start identification
                                mainClient.BeginIdentify(identSubject, AsyncIdentification, null);
                            }
                        }
                    }
                }

                /// <summary>
                /// Method called when the identification background process reports progress
                /// </summary>
                private void identify_ProgressChanged(object sender, ProgressChangedEventArgs e)
                {
                    IdentificationStatus.Value = e.ProgressPercentage;
                    IdentStatusInfo.Text = "Progress: " + (string)e.UserState;
                }

                /// <summary>
                /// Method called when the identification background process finishes
                /// </summary>
                private void identify_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
                {
                    GenerateMatches.IsEnabled = true;
                    //IdentifyImport.IsEnabled = true;
                    //IdentificationImportPath.IsEnabled = true;
                    IdentificationReset.IsEnabled = true;

                    IdentificationStatus.Value = 0;
                    IdentStatusInfo.Text = "Progress: ";

                    if (e.Error != null)
                    {
                        System.Windows.Forms.MessageBox.Show(e.Error.Message);
                        Log(string.Format("Error occurred in the identification process: {0}", e.Error.Message));
                    }
                    else if (e.Cancelled)
                    {
                        //System.Windows.Forms.MessageBox.Show("Identification Canceled");
                        Log("Identification Canceled");
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("Identification Completed");
                        Log("Identification Completed");
                    }
                }

                /// <summary>
                /// Begins the identification process
                /// </summary>
                private void GenerateMatches_Click(object sender, RoutedEventArgs e)
                {
                    if(!CheckData())
                        System.Windows.MessageBox.Show("The data folder has not been appropriately set.\nPlease check the settings before continuing","Folder Not Set", MessageBoxButton.OK);

                    // Check if background worker already has a job
                    else if (!identifyProgress.IsBusy)
                    {
                        mainClient.Clear(); // Clear the biometric client and start from scratch

                        Log("");
                        Log("Starting Identification");

                        GenerateMatches.IsEnabled = false;
                        //IdentifyImport.IsEnabled = false;
                        //IdentificationImportPath.IsEnabled = false;
                
                        IdentificationStatus.Value = 0;

                        // Start Asynchronous Process
                        identifyProgress.RunWorkerAsync();
                    }
                }

            #endregion

            #region Other Functions

                /// <summary>
                /// Checks to see if the Identify/Generate Matches button should be enabled
                /// </summary>
                private void EnableIdentify()
                {
                    GenerateMatches.IsEnabled = ValidSubject(identSubject);
                }

                /// <summary>
                /// Opens a new image window/popup
                /// </summary>
                private void OpenImage_Click(object sender, RoutedEventArgs e)
                {
                    int index = ((int)((System.Windows.Controls.Button)sender).Tag) - 1;

                    // Open image if there exists a face image/no placeholder image
                    if (!matchList[index].LargeFile.Equals("/Images/NoImage.Png"))
                    {
                        ImageDisplay resultDisplay = new ImageDisplay(matchList[index].LargeFile, matchList[index].ID);
                        this.Dispatcher.BeginInvoke(new Action(() => resultDisplay.Show()));
                    }
                }

            #endregion

        #endregion

        #region Matching Progress/Backgroundworker Functions

            /// <summary>
            /// Action Performed when the match background worker starts
            /// </summary>
            private void matchProgress_DoWork(object sender, DoWorkEventArgs e)
            {
                this.Dispatcher.Invoke(() =>
                {
                    // Reset progress bar to 0 and set maximum to # of matches and the # of matches we will display
                    IdentificationStatus.Value = 0;
                    IdentificationStatus.Maximum = identSubject.MatchingResults.Count + curSettings.GetNumMatches();

                    Log("");
                    Log("Adding Matches");
                });

                // Var to record overall progress over various methods
                matchCount = 0;

                // Loop through all results
                foreach (NMatchingResult result in identSubject.MatchingResults)
                {
                    if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                    else
                    {
                        // Add result
                        AddMatch(result);

                        // Increment progress by 1 and reupdate progress bar
                        matchCount++;
                        matchProgress.ReportProgress(matchCount, "Adding Match - " + result.Id);
                        Thread.Sleep(1);
                    }
                }

                this.Dispatcher.Invoke(() =>
                {
                    Log("All Matches Added");
                    Log("");
                    Log("Displaying Matches");
                });

                if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                else { DisplayMatches(); }
            }

            /// <summary>
            /// Action Performed when the match background worker finishes
            /// </summary>
            private void matchProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                GenerateMatches.IsEnabled = true;
                IdentifyImport.IsEnabled = true;
                IdentificationImportPath.IsEnabled = true;
                IdentificationReset.IsEnabled = true;

                IdentificationStatus.Value = 0;
                IdentStatusInfo.Text = "Progress: ";
            }

            /// <summary>
            /// Action Performed when the match background worker reports progress
            /// </summary>
            private void matchProgress_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                IdentificationStatus.Value = e.ProgressPercentage;
                IdentStatusInfo.Text = "Progress: " + (string)e.UserState;
            }

            /// <summary>
            /// Sorts and adds a matching result to the top match list
            /// </summary>
            /// <param name="newMatch">The NMatchingResult match</param>
            private void AddMatch(NMatchingResult newMatch)
            {
                int maxDisplay = curSettings.GetNumMatches();

                // Check each thumbnail for existance (or get default 'No Image' thumbnail)
                string file = GetThumbnailPath(IMGFOLDER + "\\" + newMatch.Id + @"\Thumbnail\Thumbnail.jpg");
                string largefile = GetThumbnailPath(IMGFOLDER + "\\" + newMatch.Id + @"\Thumbnail\LargeThumbnail.jpg");

                double far = GetFAR(newMatch.Score);
                double falseProb = GetFalseProbability(far);


                if (matchList.Count > 0)
                {
                    bool found = false;
                    for (int i = 0; found != true && i < matchList.Count && i < maxDisplay; i++)
                    {
                        if (newMatch.Score >= matchList[i].Score)
                        {
                            Match temp = new Match(file, largefile, newMatch.Id, newMatch.Score, far * 100, falseProb);
                            matchList.Insert(i, temp);
                            found = true;
                        }
                    }

                    // If loop ended and score was not higher than anything already in the list, insert at the end
                    if (found != true)
                    {
                        Match temp = new Match(file, largefile, newMatch.Id, newMatch.Score, far * 100, falseProb);
                        matchList.Add(temp);
                    }
                }
                else
                {
                    // The list is empty, so insert into list

                    Match temp = new Match(file, largefile, newMatch.Id, newMatch.Score, far * 100, falseProb);
                    matchList.Add(temp);
                }

                // If the number of elements in the list is higher than the maximum amount, then remove the last element
                if (matchList.Count > maxDisplay)
                {
                    matchList.RemoveAt(maxDisplay);
                }
            }

            /// <summary>
            /// Displays the list of top n matches to the GUI
            /// </summary>
            private void DisplayMatches()
            {

                int maxDisplay = curSettings.GetNumMatches();

                for (int i = 0; i < matchList.Count && i < maxDisplay; i++)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        matchList[i].GetImage();
                        matchList[i].Rank = i + 1;
                    });

                    matchCount++;
                    matchProgress.ReportProgress(matchCount, "Displaying Matches");
                    Thread.Sleep(1);
                }

                this.Dispatcher.Invoke(() =>
                {
                    TopMatchList.Items.Refresh();

                    Log("Matches Displayed");
                });
            }

            /// <summary>
            /// Tests a file path and returns itself or a default thumbnail
            /// </summary>
            /// <param name="file">The string path of the file</param>
            /// <returns>A default thumbnail or the file itself</returns>
            private string GetThumbnailPath(string file)
            {
                string path = "/Images/NoImage.Png";

                if (File.Exists(file))
                    path = file;

                return path;
            }

        #endregion

        #region Verification (1:1) Functions

            #region Import Functions

                /// <summary>
                /// Starts the template import process for the left image
                /// </summary>
                private void VerficationImportLeft_Click(object sender, RoutedEventArgs e)
                {
                    //faceVerLeft.Face = null;
                    verSubjectLeft = null;

                    Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

                    openPicker.DefaultExt = defExt;
                    openPicker.Filter = FileFilter;

                    if (openPicker.ShowDialog() == true)
                    {

                        LeftImportPath.Text = openPicker.FileName;
                        GetTemplate(faceVerLeft, openPicker.FileName, out verSubjectLeft);

                        EnableVerify();

                        Log("Left Verification Image Imported");

                        VerificationClearLeft.IsEnabled = true;
                        VerificationClear.IsEnabled = true;
                        LeftScan.IsEnabled = false;
                        LeftScanPath.IsEnabled = false;
                    }
                }

                /// <summary>
                /// Starts the template import process for the right image
                /// </summary>
                private void VerficationImportRight_Click(object sender, RoutedEventArgs e)
                {
                    //faceVerRight.Face = null;
                    verSubjectRight = null;

                    Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

                    openPicker.DefaultExt = defExt;
                    openPicker.Filter = FileFilter;

                    if (openPicker.ShowDialog() == true)
                    {

                        RightImportPath.Text = openPicker.FileName;
                        GetTemplate(faceVerRight, openPicker.FileName, out verSubjectRight);

                        EnableVerify();

                        Log("Left Verification Image Imported");

                        VerificationClearRight.IsEnabled = true;
                        VerificationClear.IsEnabled = true;
                        RightScan.IsEnabled = false;
                        RightScanPath.IsEnabled = false;
                    }
                }

            #endregion

            #region Clear and Reset Functions

                /// <summary>
                /// Clears the left subject/image
                /// </summary>
                private void VerificationClearLeft_Click(object sender, RoutedEventArgs e)
                {
                    VerificationClearLeft.IsEnabled = false;

                    // Reset verification buttons
                    VerifyImages.IsEnabled = false;

                    faceVerLeft.Face = null;
                    verSubjectLeft = null;
                    LeftImportPath.Text = string.Empty;

                    // Reset verification buttons
                    VerificationClear.IsEnabled = false;
                    VerifyImages.IsEnabled = false;

                    // Reset file import buttons
                    LeftImport.IsEnabled = true;
                    LeftImportPath.IsEnabled = true;

                    // Clear ComboBoxes
                    LeftImageSelect.Items.Clear();
                    LeftImageSelect.IsEnabled = false;

                    // Clear lists
                    leftScanFiles.Clear();

                    // Reset left image buttons
                    LeftScan.IsEnabled = true;
                    LeftScanPath.IsEnabled = true;
                    LeftScanPath.Text = string.Empty;
                    LeftSelect.IsEnabled = false;
                    LeftDeselect.IsEnabled = false;

                    //Log("");
                    Log("Cleared the Left Verification Subject");

                    //if ((RightImportPath.Text == string.Empty) && (RightScanPath.Text == string.Empty))
                    if (!RightScanPath.IsEnabled)
                        VerificationClear.IsEnabled = false;
                }

                /// <summary>
                /// Clears the right subject/image
                /// </summary>
                private void VerificationClearRight_Click(object sender, RoutedEventArgs e)
                {
                    VerificationClearRight.IsEnabled = false;

                    // Reset verification buttons
                    VerifyImages.IsEnabled = false;

                    faceVerRight.Face = null;
                    verSubjectRight = null;
                    RightImportPath.Text = string.Empty;

                    // Reset file import buttons
                    RightImport.IsEnabled = true;
                    RightImportPath.IsEnabled = true;

                    // Clear ComboBoxes
                    RightImageSelect.Items.Clear();
                    RightImageSelect.IsEnabled = false;

                    // Clear lists
                    rightScanFiles.Clear();

                    // Reset right image buttons
                    RightScan.IsEnabled = true;
                    RightScanPath.IsEnabled = true;
                    RightScanPath.Text = string.Empty;
                    RightSelect.IsEnabled = false;
                    RightDeselect.IsEnabled = false;

                    //Log("");
                    Log("Cleared the Right Verification Subject");

                    //if ((LeftImportPath.Text == string.Empty) && (LeftScanPath.Text == string.Empty))
                    if (!LeftScanPath.IsEnabled)
                        VerificationClear.IsEnabled = false;
                }

                /// <summary>
                /// Clears all verification information from the GUI
                /// </summary>
                private void VerificationClear_Click(object sender, RoutedEventArgs e)
                {
                    Log("");

                    VerificationClearLeft_Click(null, null);
                    VerificationClearRight_Click(null, null);

                    VerificationClear.IsEnabled = false;

                    Log("Cleared Verification Page");
                }

                /// <summary>
                /// Resets the verification results box
                /// </summary>
                private void VerificationReset_Click(object sender, RoutedEventArgs e)
                {
                    VerifyResults.Text = string.Empty;
                    VerificationReset.IsEnabled = false;
                    VerificationStore.IsEnabled = false;
                }

            #endregion

            /// <summary>
            /// Checks to see if the Verify button should be enabled
            /// </summary>
            private void EnableVerify()
            {
                VerifyImages.IsEnabled = ValidSubject(verSubjectLeft) && ValidSubject(verSubjectRight);
            }

            /// <summary>
            /// Starts the verification process
            /// </summary>
            private void Verification_Click(object sender, RoutedEventArgs e)
            {
                if (verSubjectLeft != null && verSubjectRight != null)
                {
                    Log("");
                    Log("Starting Verification");

                    mainClient.BeginVerify(verSubjectLeft, verSubjectRight, AsyncVerification, null);
                }

                // Add potential checks or messages for user if verification can't be completed
            }

        #endregion

        #region Verification (1:N) Functions

            /// <summary>
            /// Imports the subject for verification
            /// </summary>
            private void VerifyImport_Click(object sender, RoutedEventArgs e)
            {
                verifySubject = null;

                Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

                openPicker.DefaultExt = defExt;
                openPicker.Filter = FileFilter;

                if (openPicker.ShowDialog() == true)
                {
                    // Clear the import path and determine new path
                    VerifyImportPath.Text = openPicker.FileName;
                    GetTemplate(faceVerify, openPicker.FileName, out verifySubject);

                    if (openPicker.FileName != string.Empty)
                    {

                        EnableVerifyN();

                        Log("Verification Image Imported");

                        VerifySubject_Clear.IsEnabled = true;
                        VerifyScan.IsEnabled = false;
                        VerifyScanPath.IsEnabled = false;
                    }
                }
            }

            /// <summary>
            /// Clears the verification subject
            /// </summary>
            private void VerifyNClear_Click(object sender, RoutedEventArgs e)
            {
                VerifySubjectImage.IsEnabled = false;
                VerifySubject_Clear.IsEnabled = false;

                verifySubject = null;
                faceVerify.Face = null;
                VerifyImportPath.Text = string.Empty;

                // Clear ComboBoxes
                VerifyImageSelect.Items.Clear();
                VerifyImageSelect.IsEnabled = false;

                // Clear lists
                verifyScanFiles.Clear();

                // Reset Import Buttons
                VerifyImport.IsEnabled = true;
                VerifyImportPath.IsEnabled = true;

                // Reset Scan Buttons
                VerifyScan.IsEnabled = true;
                VerifyScanPath.IsEnabled = true;
                VerifyScanPath.Text = string.Empty;
                VerifySelect.IsEnabled = false;
                VerifyDeselect.IsEnabled = false;

                Log("Cleared Verification (1:N) Page");
            }

            /// <summary>
            /// Resets the verication results
            /// </summary>
            private void VerifyNReset_Click(object sender, RoutedEventArgs e)
            {
                VerifyN_Results.Text = string.Empty;
                VerifySubject_Reset.IsEnabled = false;
                VerifyStoreResult.IsEnabled = false;
            }

            /// <summary>
            /// Enables the verification button if the subject is valid
            /// </summary>
            private void EnableVerifyN()
            {
                VerifySubjectImage.IsEnabled = ValidSubject(verifySubject);
            }

            /// <summary>
            /// Event occurred that starts the verification (1:N) process
            /// </summary>
            private void VerifyN_Click(object sender, RoutedEventArgs e)
            {
                if (!CheckData())
                    System.Windows.MessageBox.Show("The data folder has not been appropriately set.\nPlease check the settings before continuing", "Folder Not Set", MessageBoxButton.OK);

                else
                {
                    // Clear the biometric client and start from scratch
                    mainClient.Clear();

                    // Get all face templates
                    string[] allTemplates = Directory.GetFiles(TEMPLATEFOLDER, "*.dat");

                    // Enroll templates
                    EnrollTemplates(allTemplates);

                    if (verifySubject != null)
                    {
                        Log("");
                        Log("Starting Verification (1:N)");

                        // Start verification
                        mainClient.BeginVerify(verifySubject, AsyncVerifyAll, null);
                    }
                }
            }

        #endregion

        #region Generate Template Functions

            /// <summary>
            /// Generates all template files for every directory/database in the settings
            /// </summary>
            private void GenerateAll_Click(object sender, RoutedEventArgs e)
            {
                if(!CheckData())
                        System.Windows.MessageBox.Show("The data folder has not been appropriately set.\nPlease check the settings before continuing","Folder Not Set", MessageBoxButton.OK);

                // Check if background worker already has a job
                else if (!progress.IsBusy)
                {
                    // Set generate buttons so that they cannot be clicked and activate the cancel button
                    GenerateAll.IsEnabled = false;
                    GenerateSelected.IsEnabled = false;
                    CancelTemplate.IsEnabled = true;

                    TemplateStatus.Maximum = GetNumberSubjects();
                    TemplateStatus.Value = 0;

                    Log("");
                    Log("Generating all templates");

                    progress.RunWorkerAsync(GetAccessibleDirectories(curSettings.GetDatabases()));
                }

            }

            /// <summary>
            /// Generates all template files for each directory/database specified by the user
            /// </summary>
            private void GenerateSelected_Click(object sender, RoutedEventArgs e)
            {
                if(!CheckData())
                        System.Windows.MessageBox.Show("The data folder has not been appropriately set.\nPlease check the settings before continuing","Folder Not Set", MessageBoxButton.OK);

                // Check if background worker already has a job and if there are actually any items selected
                else if (!progress.IsBusy && DatabaseDisplay.SelectedItems.Count > 0)
                {
                    string[] selectionList = GetAccessibleDirectories(DatabaseDisplay.SelectedItems.OfType<string>().ToArray());

                    // See if # of accessible directories is greater than 0
                    if (selectionList.Length > 0)
                    {
                        // Set generate buttons so that they cannot be clicked and activate the cancel button
                        GenerateAll.IsEnabled = false;
                        GenerateSelected.IsEnabled = false;
                        CancelTemplate.IsEnabled = true;

                        // Create a copy of the selections, since they may potentially change

                        TemplateStatus.Maximum = GetNumberSubjects(selectionList);

                        Log("");
                        Log("Generating templates for selected directories");

                        progress.RunWorkerAsync(selectionList);
                    }
                }
            }

            /// <summary>
            /// Creates a template for a given subject
            /// </summary>
            /// <param name="RID">The RID identifier for a subject</param>
            private void CreateNewTemplate(string RID)
            {
                using (NSubject newSubject = new NSubject())
                {
                    newSubject.Id = RID;

                    // Get all recorded images we have for the given subject
                    string[] allFiles = Directory.GetFiles(IMGFOLDER + "\\" + RID, "*.jpg", SearchOption.TopDirectoryOnly);

                    // Loop through all images
                    foreach (string img in allFiles)
                    {
                        // Create a new face and set filename to the image
                        NFace face = new NFace { FileName = img };

                        // Add the face to the subject
                        newSubject.Faces.Add(face);
                    }

                    mainClient.CreateTemplate(newSubject);

                    WriteTemplate(newSubject);
                }
            }

            /// <summary>
            /// Records all data for subjects in each directory
            /// </summary>
            /// <param name="allDirectory">An array of directories to search through</param>
            private void RecordSubjects(string[] allDirectory)
            {
                // Clear our record list
                recordList.Clear();

                // Loop through every directory
                for (int i = 0; i < allDirectory.Length && !progress.CancellationPending; i++)
                {
                    string[] ridDirectory = Directory.GetDirectories(allDirectory[i]);

                    // Loop through each sub directory (RID folders)
                    for (int j = 0; j < ridDirectory.Length && !progress.CancellationPending; j++)
                    {
                        string RID = new DirectoryInfo(ridDirectory[j]).Name;

                        // Check if RID has already been recorded and if equal to ERRORS (which has a completely different file structure than other RID's)
                        if (!recordList.Contains(RID) && !RID.Equals("ERRORS"))
                        {
                            string imgFolder = IMGFOLDER + "\\" + RID;

                            // Keep count of the number of images that we find
                            int numImageCount = 0;

                            /* **************************** RID ALREADY HAS RECORD **************************** */
                            if (Directory.Exists(imgFolder))
                            {
                                // Check if the thumbnails have already been created
                                bool thumbnailMade = File.Exists(imgFolder + @"\Thumbnail\Thumbnail.jpg") && File.Exists(imgFolder + @"\Thumbnail\LargeThumbnail.jpg");

                                // Go through all specified directories to get their images
                                foreach (string dir in allDirectory)
                                {
                                    // Since we're re-looping through everything, we need to double check if the RID is in the directory
                                    if (!progress.CancellationPending && Directory.Exists(dir + "\\" + RID))
                                    {
                                        // Get all images in directory
                                        List<string> faceImages = GetAllImages(dir + "\\" + RID);

                                        // See if the image is already in the folder, if not, resize and store image
                                        for (int k = 0; k < faceImages.Count && !progress.CancellationPending; k++)
                                        {
                                            numImageCount++;
                                            if (!File.Exists(imgFolder + "\\" + Path.GetFileName(faceImages[k])))
                                                SaveImage(faceImages[k], imgFolder);
                                        }

                                        // Check to see if there were already images in the subject folder that weren't accounted for
                                        // Add those images if none have been recorded
                                        if (numImageCount == 0)
                                        {
                                            //int numFiles = Directory.GetFiles(dir + "\\" + RID, "*.jpg", SearchOption.TopDirectoryOnly).Length;
                                            //
                                            //if (numFiles > 0)
                                            //    numImageCount += numFiles;

                                            // Add count of files already within image folder
                                            numImageCount += Directory.GetFiles(dir + "\\" + RID, "*.jpg", SearchOption.TopDirectoryOnly).Length;
                                        }

                                        // If thumbnail hasn't been made, create it
                                        if (!thumbnailMade)
                                        {
                                            string thumbImg = GetOpenEyeImage(dir + "\\" + RID);
                                            if (thumbImg != string.Empty)
                                            {
                                                //SaveImage(thumbImg, imgFolder + @"\Thumbnail", "LargeThumbnail.jpg");
                                                SaveThumbnail(thumbImg, imgFolder + @"\Thumbnail");
                                                thumbnailMade = true;
                                            }
                                        }
                                    }
                                }
                            }

                            /* **************************** NO RID IN RECORD **************************** */
                            else
                            {
                                // Create their appropriate directories
                                Directory.CreateDirectory(imgFolder);
                                Directory.CreateDirectory(imgFolder + @"\Thumbnail");

                                bool thumbnailMade = false;
                                

                                // Go through every (accessible) directory to get every image that we can get
                                foreach (string dir in GetAccessibleDirectories(curSettings.GetDatabases()))
                                {
                                    if (!progress.CancellationPending && Directory.Exists(dir + "\\" + RID))
                                    {
                                        List<string> faceImages = GetAllImages(dir + "\\" + RID);

                                        // Save every image that we find
                                        for (int k = 0; k < faceImages.Count && !progress.CancellationPending; k++)
                                        {
                                            SaveImage(faceImages[k], imgFolder);
                                            numImageCount++;
                                        }

                                        // If thumbnail hasn't been made yet, create it 
                                        if (!thumbnailMade)
                                        {
                                            string thumbImg = GetOpenEyeImage(dir + "\\" + RID);
                                            if (thumbImg != string.Empty)
                                            {
                                                //SaveImage(thumbImg, imgFolder + @"\Thumbnail", "LargeThumbnail.jpg");
                                                SaveThumbnail(thumbImg, imgFolder + @"\Thumbnail");
                                                thumbnailMade = true;
                                            }
                                        }
                                    }
                                }
                            }

                            //// Check to see if no images have been found at all
                            //if (numImageCount == 0)
                            //{
                            //    // No image were found, so delete the directory
                            //    Directory.Delete(imgFolder, true);
                            //}

                            // Some images were found, so record them for possible template processing
                            if(numImageCount != 0)
                            {
                                // Add RID/Subject to our record
                                recordList.Add(RID);
                            }

                            // Update the template background worker
                            templateCount++;
                            progress.ReportProgress(templateCount, "Recorded Subject Files - " + RID);
                            Thread.Sleep(1);
                        }
                    }
                }
            }

            #region Main Template Processes

                /// <summary>
                /// The work method for the template background process
                /// </summary>
                private void template_DoWork(object sender, DoWorkEventArgs e)
                {
                    templateCount = 0;

                    // Repopulate our dictionary with all of the appropriate subjects/records
                    RecordSubjects((string[])e.Argument);

                    templateCount = 0;

                    if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                    else if (recordList != null && recordList.Count > 0)
                    {
                        // Loop through our records
                        foreach (string subject in recordList)
                        {
                            if ((sender as BackgroundWorker).CancellationPending) // Check if process has been canceled
                            {
                                e.Cancel = true;
                                return;
                            }
                            else
                            {
                                // Try to get a template file to a subject
                                string template = Directory.GetFiles(TEMPLATEFOLDER, subject + ".dat").FirstOrDefault();

                                // No template found for subject, so start creating a template
                                if (template == null)
                                {
                                    CreateNewTemplate(subject);
                                }

                                // Template Found for subject
                                else
                                {
                                    // Read in the template
                                    NSubject templateSubject = NSubject.FromFile(template);

                                    // If template has 0 images within it, it will throw a nullpointerexception. So, we'll want to overwrite the original file anyways.
                                    try
                                    {
                                        // Check if number of face records from template is less than the number of faces/images for a given RID
                                        // If less, then we know we must recreate the template to include all face data
                                        // If not, nothing happens

                                        string[] recordImages = Directory.GetFiles(IMGFOLDER + "\\" + subject, "*.jpg", SearchOption.TopDirectoryOnly);

                                        if (recordImages.Length != 0 && templateSubject.GetTemplate().Faces.Records.Count < recordImages.Length) //subject.Value.imageList.Count
                                            CreateNewTemplate(subject);
                                    }
                                    catch
                                    {
                                        CreateNewTemplate(subject);
                                    }
                                }

                                // Update our template background worker/progress bar
                                templateCount++;
                                (sender as BackgroundWorker).ReportProgress(templateCount, "Saved Subject - " + subject);
                                Thread.Sleep(1);
                            }
                        }
                    }
                }

                /// <summary>
                /// Method called when the template background process reports progress
                /// </summary>
                private void template_ProgressChanged(object sender, ProgressChangedEventArgs e)
                {
                    TemplateStatus.Value = e.ProgressPercentage;
                    TemplateStatusInfo.Text = "Progress: " + (string)e.UserState;
                }

                /// <summary>
                /// Method called when the template background process finishes
                /// </summary>
                private void template_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
                {
                    GenerateAll.IsEnabled = true;
                    GenerateSelected.IsEnabled = true;
                    CancelTemplate.IsEnabled = false;
                    TemplateStatus.Value = 0;
                    TemplateStatusInfo.Text = "Progress: ";

                    if (e.Error != null)
                    {
                        //e.Error.Message
                        System.Windows.Forms.MessageBox.Show(e.Error.Message,"Error in Template Process");
                        Log(e.Error.Message);
                    }
                    else if (e.Cancelled)
                    {
                        //System.Windows.Forms.MessageBox.Show("Template Process Canceled");
                        Log("Template Process Canceled");
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("Operation Completed");
                        Log("Template Process Completed");
                    }
                }

            #endregion

            #region Image Resizing and Saving

                /// <summary>
                /// Resizes and saves an image
                /// </summary>
                /// <param name="file">The original image to be resized</param>
                /// <param name="savePath">The file path where the resized image will be saved</param>
                private void SaveImage(string file, string savePath)
                {
                    BitmapImage img = new BitmapImage();
                    //RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.None;
                    img.DecodePixelHeight = 960;
                    img.DecodePixelWidth = 1440;
                    img.UriSource = new Uri(file);
                    img.Rotation = Rotation.Rotate90;
                    img.EndInit();

                    if (img.CanFreeze)
                        img.Freeze();

                    BmpBitmapEncoder encode = new BmpBitmapEncoder();
                    encode.Frames.Add(BitmapFrame.Create(img));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encode.Save(ms);
                        System.Drawing.Image.FromStream(ms).Save(savePath + "\\" + Path.GetFileName(file));
                    }
                }

                /// <summary>
                /// Resizes and saves an image
                /// </summary>
                /// <param name="file">The original image to be resized</param>
                /// <param name="savePath">The file path where the resized image will be saved</param>
                /// <param name="saveName">A specific filename (with extension) for the new image</param>
                private void SaveImage(string file, string savePath, string saveName)
                {
                    BitmapImage img = new BitmapImage();
                    //RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.None;
                    img.DecodePixelHeight = 960;
                    img.DecodePixelWidth = 1440;
                    img.UriSource = new Uri(file);
                    img.Rotation = Rotation.Rotate90;
                    img.EndInit();

                    if (img.CanFreeze)
                        img.Freeze();

                    BmpBitmapEncoder encode = new BmpBitmapEncoder();
                    encode.Frames.Add(BitmapFrame.Create(img));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encode.Save(ms);
                        System.Drawing.Image.FromStream(ms).Save(savePath + "\\" + saveName);
                    }
                }

                /// <summary>
                /// Reads an image and saves a thumbnail sized version
                /// </summary>
                /// <param name="file">The original image to be resized</param>
                /// <param name="savePath">The file path where the resized image will be saved</param>
                private void SaveThumbnail(string file, string savePath)
                {
                    BitmapImage img = new BitmapImage();
                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.None;
                    img.DecodePixelHeight = 320;
                    img.DecodePixelWidth = 480;
                    img.UriSource = new Uri(file);
                    img.Rotation = Rotation.Rotate90;
                    img.EndInit();

                    if (img.CanFreeze)
                        img.Freeze();

                    BmpBitmapEncoder encode = new BmpBitmapEncoder();
                    encode.Frames.Add(BitmapFrame.Create(img));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encode.Save(ms);
                        System.Drawing.Image.FromStream(ms).Save(savePath + @"\Thumbnail.jpg");
                    }

                    SaveImage(file, savePath, "LargeThumbnail.jpg");
                }

            #endregion

            /// <summary>
            /// Writes a subject template to a file
            /// </summary>
            /// <param name="subject">The subject of the template</param>
            /// <param name="directory">The directory to be saved to</param>
            private void WriteTemplate(NSubject subject)
            {
                // Semaphore used to make sure only one thread is writing at a time
                templateWriteHold.WaitOne();

                using (NBuffer subjectBuffer = subject.GetTemplateBuffer())
                {
                    File.WriteAllBytes(TEMPLATEFOLDER + "\\" + subject.Id + ".dat", subjectBuffer.ToArray());
                }

                templateWriteHold.Release();
            }

            /// <summary>
            /// Prompts the user to cancel the template generation process
            /// </summary>
            private void CancelTemplate_Click(object sender, RoutedEventArgs e)
            {
                MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Are you sure you want to cancel the template generation?",
                                                                               "Cancel Template", System.Windows.MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                    if (progress.IsBusy)
                        progress.CancelAsync();
            }

        #endregion

        #region Neurotec Asynch Functions

            /// <summary>
            /// Asynchronously ends the template creation process
            /// </summary>
            private void AsyncCreation(IAsyncResult r)
            {
                // Based loosely on the Neurotechnology Simple Faces Demo

                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(new AsyncCallback(AsyncCreation), r);
                }
                else
                {
                    try
                    {
                        NBiometricStatus status = mainClient.EndCreateTemplate(r);
                        if (status != NBiometricStatus.Ok)
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("The template was not extracted: {0}.", status), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Log(string.Format("The template was not extracted: {0}.", status));
                        }
                        else
                        {
                            Log("Template creation was successful");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(string.Format("Template creation failed: {0}.", ex.ToString()));
                        //Log("Template creation failed");
                    }
                }
                //threadHold.Release();
            }

            /// <summary>
            /// Asynchronously ends the verification (1:1) process
            /// </summary>
            private void AsyncVerification(IAsyncResult r)
            {
                // Based loosely on the Neurotechnology Simple Faces Demo

                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(new AsyncCallback(AsyncVerification), r);
                }
                else
                {
                    try
                    {
                        NBiometricStatus status = mainClient.EndVerify(r);
                        if (status == NBiometricStatus.Ok || status == NBiometricStatus.MatchNotFound)
                        {
                            int score = verSubjectLeft.MatchingResults[0].Score;

                            // Print results/information into the VerifyResults text box
                            // We can change this later to display whatever we want to plug in
                            VerifyResults.Text += string.Format("Left RID: {0}\nRight RID: {1}\nScore: {2}\n\n", verSubjectLeft.Id, verSubjectRight.Id, score);

                            VerificationStore.IsEnabled = true;
                            VerificationReset.IsEnabled = true;

                            Log("Verification successfully completed");
                        }
                        else
                        {
                            Log(string.Format("Verification completed, but not successful: {0}", status));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Error: {0}.", ex.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log("Error Occurred in Verification Process");
                    }
                }
            }

            /// <summary>
            /// Asynchronously ends the verification (1:N) process
            /// </summary>
            private void AsyncVerifyAll(IAsyncResult r)
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(new AsyncCallback(AsyncVerifyAll), r);
                }
                else
                {
                    try
                    {
                        NBiometricStatus status = mainClient.EndVerify(r);
                        if (status == NBiometricStatus.Ok || status == NBiometricStatus.MatchNotFound)
                        {
                            int score = verifySubject.MatchingResults[0].Score;

                            // Print results/information into the VerifyResults text box
                            // We can change this later to display whatever we want to plug in
                            VerifyN_Results.Text += string.Format("Subject RID: {0}\nScore: {1}\n\n", verifySubject.Id, score);

                            VerifySubject_Reset.IsEnabled = true;
                            VerifyStoreResult.IsEnabled = true;
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Incomplete Verification: {0}", status), "Incomplete Verification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Log(string.Format("Incomplete Verification: {0}", status));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Error: {0}.", ex.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log("Error Occurred in Verification Process");
                    }
                }
            }

            /// <summary>
            /// Asynchronously ends the Identification process
            /// </summary>
            private void AsyncIdentification(IAsyncResult r)
            {
                // Based loosely on the Neurotechnology Simple Faces Demo
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(new AsyncCallback(AsyncIdentification), r);
                }
                else
                {
                    try
                    {
                        NBiometricStatus status = mainClient.EndIdentify(r);
                        if (status == NBiometricStatus.Ok || status == NBiometricStatus.MatchNotFound)
                        {
                            Log("Identification successfully completed");

                            matchProgress.RunWorkerAsync();
                        }
                        else
                        {
                            Log(string.Format("Identification completed, but not successful: {0}", status));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Error: {0}.", ex.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log("Error Occurred in Identification Process");
                    }
                }
            }

        #endregion

        #region Settings Action Commands

            #region Apply/Cancel/Revert/Default Settings

                /// <summary>
                /// Applies the values entered into the current settings
                /// </summary>
                private void ApplySettings(object sender, RoutedEventArgs e)
                {
                    Log("");
                    Log("Applying Settings");

                    int newMatches = 0;
                    double newFAR = 0;
                    int newThreshold = 0;
                    /*Check if input number of matches is an integer*/
                    /*Stores input into newMatches*/
                    if (Int32.TryParse(NumOfMatches.Text, out newMatches))
                    {
                        curSettings.SetNumMatches(newMatches);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Invalid Number of Matches");
                        return;
                    }

                    /*Check if far is an integer*/
                    /*Stores input into newFAR*/
                    if (Double.TryParse(setMatchFAR.Text, out newFAR))
                    {
                        curSettings.SetFar(newFAR);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Invalid FAR");
                        return;
                    }

                    //if (!DatabaseDisplayList.HasItems)
                    //{
                    //    System.Windows.MessageBox.Show("No Databases Specified!");
                    //    return;
                    //}

                    /*Stores threshold as the biometric client threshold*/
                    if (Int32.TryParse(setThreshold.Text, out newThreshold))
                    {
                        curSettings.SetThreshold(newThreshold);
                        mainClient.MatchingThreshold = (byte)newThreshold;
                    }

                    List<string> listDatabases = new List<string>();
                    listDatabases = DatabaseDisplayList.Items.Cast<string>().ToList();
                    curSettings.SetDatabases(listDatabases);

                    System.Windows.MessageBox.Show("Settings Applied!");
                    Log("New Settings Applied");

                    ApplySetting.IsEnabled = false;
                    CancelSetting.IsEnabled = false;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Cancels any settings changes to before anything was altered
                /// </summary>
                private void CancelSettings(object sender, RoutedEventArgs e)
                {
                    MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Are you sure you want to cancel any changes?",
                                                                                   "Cancel Settings", System.Windows.MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        Log("");
                        Log("Canceling Settings Changes");

                        NumOfMatches.Text = curSettings.GetNumMatches().ToString();
                        setMatchFAR.Text = curSettings.GetFar().ToString();
                        setThreshold.Text = curSettings.GetThreshold().ToString();
                        ImportDatabases(curSettings.GetDatabases());

                        ApplySetting.IsEnabled = false;
                        CancelSetting.IsEnabled = false;
                    }
                    else
                    {
                        ApplySetting.IsEnabled = true;
                        CancelSetting.IsEnabled = true;
                    }

                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Reverts the settings to the previously set values
                /// </summary>
                private void RevertSettings(object sender, RoutedEventArgs e)
                {
                    MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Are you sure you want to revert to previously applied settings?",
                                                                                   "Revert Settings", System.Windows.MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        Log("");
                        Log("Reverting Settings");

                        // Revert Settings
                        curSettings.Revert();

                        NumOfMatches.Text = curSettings.GetNumMatches().ToString();
                        setMatchFAR.Text = curSettings.GetFar().ToString();
                        setThreshold.Text = curSettings.GetThreshold().ToString();
                        ImportDatabases(curSettings.GetDatabases());

                        ApplySetting.IsEnabled = false;
                        CancelSetting.IsEnabled = false;
                    }

                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Restores the settings page back to its default state
                /// </summary>
                private void RestoreDefaultSettings(object sender, RoutedEventArgs e)
                {
                    Log("");
                    Log("Restoring Settings to Default");

                    NumOfMatches.Text = defaultSettings.GetNumMatches().ToString();
                    setMatchFAR.Text = defaultSettings.GetFar().ToString();
                    setThreshold.Text = defaultSettings.GetThreshold().ToString();
                    ImportDatabases(defaultSettings.GetDatabases());

                    // Set current settings to default
                    curSettings.SetSettings(defaultSettings);

                    ApplySetting.IsEnabled = false;
                    CancelSetting.IsEnabled = false;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = false;
                }

            #endregion

            #region Directory/Network Management Functions

                /// <summary>
                /// Lets user select a directory/database and adds it to the database list in the GUI
                /// </summary>
                private void AddDirectory_Click(object sender, RoutedEventArgs e)
                {
                    // Have to specify the Windows.Forms to avoid confusion of Windows Forms and WPF versions
                    System.Windows.Forms.FolderBrowserDialog folderPicker = new System.Windows.Forms.FolderBrowserDialog();

                    // previously used folderPicker.SelectedPath != null
                    if (folderPicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Log("");
                        Log(string.Format("Adding Database: {0}", folderPicker.SelectedPath));

                        DatabaseDisplayList.Items.Add(folderPicker.SelectedPath);
                        DatabaseDisplay.Items.Add(folderPicker.SelectedPath);

                        ApplySetting.IsEnabled = true;
                        CancelSetting.IsEnabled = true;
                        RevertSetting.IsEnabled = true;
                        DefaultSetting.IsEnabled = true;
                    }
                }

                /// <summary>
                /// Opens a user prompt to enter in a network drive path
                /// </summary>
                private void AddNetworkDrive_Click(object sender, RoutedEventArgs e)
                {
                    UserInput networkInput = new UserInput("Enter the server address", "Add Network Drive", "Address: ");
                    networkInput.Owner = this;

                    if (networkInput.ShowDialog() == true)
                    {
                        Log("");
                        Log(string.Format("Adding Network/Server: {0}", networkInput.ServerName));

                        DatabaseDisplayList.Items.Add(networkInput.ServerName);
                        DatabaseDisplay.Items.Add(networkInput.ServerName);

                        ApplySetting.IsEnabled = true;
                        CancelSetting.IsEnabled = true;
                    }

                }

                /// <summary>
                /// Removes a specific database from the GUI
                /// </summary>
                private void RemoveDatabase_Click(object sender, RoutedEventArgs e)
                {
                    Log("");
                    Log("Removing Database(s)");

                    object[] selectionList = new object[DatabaseDisplayList.SelectedItems.Count];
                    DatabaseDisplayList.SelectedItems.CopyTo(selectionList,0);
                    foreach (object selection in selectionList)
                    {
                        DatabaseDisplayList.Items.Remove(selection);
                        DatabaseDisplay.Items.Remove(selection);
                    }

                    ApplySetting.IsEnabled = true;
                    CancelSetting.IsEnabled = true;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Clears the list of databases from the GUI
                /// </summary>
                private void ClearDatabases_Click(object sender, RoutedEventArgs e)
                {
                    Log("");
                    Log("Clearing All Databases");

                    DatabaseDisplayList.Items.Clear();
                    DatabaseDisplay.Items.Clear();

                    ApplySetting.IsEnabled = true;
                    CancelSetting.IsEnabled = true;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Opens a dialog box to set the Twins Day 2017 server address
                /// </summary>
                private void SetServerData_Click(object sender, RoutedEventArgs e)
                {
                    //UserInput TWDServerInput = new UserInput("Enter the Twins Day 2017 server address:", "Add TWD17 Server", "Address: ", TWD17_Server, false);
                    //TWDServerInput.Owner = this;

                    //if (TWDServerInput.ShowDialog() == true)
                    //{
                    //    string server = TWDServerInput.ServerName;

                    //    // We want the server path to end with a backslash
                    //    // Test if last character is '\' and if not, add it
                    //    if (!server[server.Length - 1].Equals('\\'))
                    //        server += "\\";

                    //    Log("");
                    //    Log(string.Format("Setting TWD 2017 Server: {0}", server));

                    //    // If server was included in our directory list, update it accordingly
                    //    curSettings.ChangeDatabase(TWD17_Server + "FACE", server + "FACE");

                    //    TWD17_Server = server;

                    //    // Default databases have changed due to static TWD17_Server variable, so update the default settings
                    //    defaultSettings.SetDatabases(defaultDatabases);

                    //    // Update our directory and network lists
                    //    ImportDatabases(curSettings.GetDatabases());
                    //}

                    ServerSet InputServer = new ServerSet(TWD17_Server, DataFolder);
                    InputServer.Owner = this;
                    InputServer.Show();
                }

                public void SetServer(string serverPath)
                {
                    TWD17_Server = serverPath;

                    if (!TWD17_Server[TWD17_Server.Length - 1].Equals('\\'))
                        TWD17_Server += "\\";
                    
                    // If server was included in our directory list, update it accordingly
                    curSettings.ChangeDatabase(TWD17_Server + "FACE", TWD17_Server + "FACE");

                    // Default databases have changed due to static TWD17_Server variable, so update the default settings
                    defaultSettings.SetDatabases(defaultDatabases);

                    // Update our directory and network lists
                    ImportDatabases(curSettings.GetDatabases());

                    Log("");
                    Log(string.Format("Setting TWD 2017 Server: {0}", TWD17_Server));
                }

                public void SetData(string dataPath)
                {
                    // Update all paths
                    DataFolder = dataPath;
                    TEMPLATEFOLDER = DataFolder + @"\Face";
                    IMGFOLDER = DataFolder + @"\Face Img";

                    Log(string.Format("Setting Data Folder: {0}", DataFolder));
                }

                private bool CheckData()
                {
                    bool result = false;

                    if (GetAccess(DataFolder, false))
                    {
                        string tempPath = DataFolder;

                        if (!tempPath[tempPath.Length - 1].Equals('/'))
                            tempPath += "//";

                        if (Directory.Exists(tempPath + "Audio") && Directory.Exists(tempPath + "Face") && Directory.Exists(tempPath + "Face Img"))
                            result = true;
                    }

                    return result;
                }

                private bool CheckServer()
                {
                    return GetAccess(TWD17_Server, false);
                }

        #endregion

            #region User Text Input/ComboBox Functions

                /// <summary>
                /// Event when the NumOfMatches combobox text is changed
                /// </summary>
                private void NumOfMatches_TextChanged(object sender, TextChangedEventArgs e)
                {
                    ApplySetting.IsEnabled = true;
                    CancelSetting.IsEnabled = true;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                #region Threshold and FAR Functions

                    /// <summary>
                    /// Limits the set threshold text box to only enter in numbers
                    /// </summary>
                    private void setThreshold_PreviewTextInput(object sender, TextCompositionEventArgs e)
                    {
                        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
                        e.Handled = regex.IsMatch(e.Text);
                    }

                    /// <summary>
                    /// Clears the FAR text when threshold gains focus
                    /// </summary>
                    private void setThreshold_GotFocus(object sender, RoutedEventArgs e)
                    {
                        setMatchFAR.Text = string.Empty;
                    }

                    /// <summary>
                    /// Changes the FAR text if the threshold is changed
                    /// </summary>
                    private void setThreshold_LostFocus(object sender, RoutedEventArgs e)
                    {
                        if (setThreshold.Text == string.Empty)
                        {
                            setMatchFAR.Text = string.Empty;
                        }
                        else
                        {
                            int threshold = 0;
                            Int32.TryParse(setThreshold.Text, out threshold);

                            double farResult = GetFAR(threshold) * 100;
                            if(farResult >= 0.0001)
                                setMatchFAR.Text = Math.Round(farResult, 4).ToString();
                            else if(farResult >= 0.0000000001)
                                setMatchFAR.Text = string.Format("{0:0.##########}", Math.Round(farResult, 10));
                            else
                                setMatchFAR.Text = farResult.ToString("0.0###e-00");
                        }

                        ApplySetting.IsEnabled = true;
                        CancelSetting.IsEnabled = true;
                    }

                    /// <summary>
                    /// Clears the threshold text when FAR gains focus
                    /// </summary>
                    private void setMatchFAR_GotFocus(object sender, RoutedEventArgs e)
                    {
                        setThreshold.Text = string.Empty;
                    }

                    /// <summary>
                    /// Changes the threshold text if the FAR is changed
                    /// </summary>
                    private void setMatchFAR_LostFocus(object sender, RoutedEventArgs e)
                    {
                        double far = 0;

                        if (!(setMatchFAR.Text == string.Empty || setMatchFAR.Text.Equals(".") || setMatchFAR.Text.Equals("0.")) && Double.TryParse(setMatchFAR.Text, out far))
                        {
                            if (far <= 0 || far > 100)
                            {
                                setThreshold.Text = string.Empty;
                            }
                            else
                            {
                                setThreshold.Text = GetScore(far / 100).ToString();

                                ApplySetting.IsEnabled = true;
                                CancelSetting.IsEnabled = true;
                            }
                        }
                        else
                            setThreshold.Text = string.Empty;
                    }

                #endregion

            #endregion

            #region Status Button Methods

                /// <summary>
                /// Loading behavior for satus buttons
                /// </summary>
                private async void StatusButton_Load(object sender, RoutedEventArgs e)
                {
                    await Task.Run(() =>
                    {
                        string tag = string.Empty;
                        this.Dispatcher.Invoke(() => { tag = (string)(sender as System.Windows.Controls.Button).Tag; });

                        if (GetAccess(tag, false))
                            this.Dispatcher.Invoke(() => { (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightGreen; });
                        else
                            this.Dispatcher.Invoke(() => { (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightCoral; });
                    });
                }

                /// <summary>
                /// Click behavior for status buttons
                /// </summary>
                private async void StatusButton_Click(object sender, RoutedEventArgs e)
                {
                    (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightSkyBlue;

                    await Task.Run(() =>
                    {
                        Thread.Sleep(500);

                        string tag = string.Empty;
                        this.Dispatcher.Invoke(() => { tag = (string)(sender as System.Windows.Controls.Button).Tag; });

                        if (GetAccess(tag, false))
                            this.Dispatcher.Invoke(() => { (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightGreen; });
                        else
                            this.Dispatcher.Invoke(() => { (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightCoral; });
                    });
                }

            #endregion

            #region ScrollViewer Methods

                /// <summary>
                /// Event triggered when the directory list is scrolled
                /// </summary>
                private void DatabaseDisplayList_ScrollChanged(object sender, ScrollChangedEventArgs e)
                {
                    ScrollViewer ListScroller = GetDescendant(DatabaseDisplayList, typeof(ScrollViewer)) as ScrollViewer;
                    ScrollViewer StatusScroller = GetDescendant(DBStatusList, typeof(ScrollViewer)) as ScrollViewer;
                    StatusScroller.ScrollToVerticalOffset(ListScroller.VerticalOffset);
                }

                /// <summary>
                /// Event triggered when the status list is scrolled
                /// </summary>
                private void DBStatusList_ScrollChanged(object sender, ScrollChangedEventArgs e)
                {
                    ScrollViewer ListScroller = GetDescendant(DatabaseDisplayList, typeof(ScrollViewer)) as ScrollViewer;
                    ScrollViewer StatusScroller = GetDescendant(DBStatusList, typeof(ScrollViewer)) as ScrollViewer;
                    ListScroller.ScrollToVerticalOffset(StatusScroller.VerticalOffset);
                }

                /// <summary>
                /// Gets the child/descendant from a WPF element
                /// </summary>
                /// <returns>The WPF element descendant</returns>
                private Visual GetDescendant(Visual element, Type type)
                {
                    //Based on an answer on https://social.msdn.microsoft.com/Forums/vstudio/en-US/38413d0a-7388-4191-a7a6-fd66e469d502/two-listbox-scrollbar-in-synchronisation?forum=wpf

                    if (element == null)
                        return null;
                    if (element.GetType() == type)
                        return element;

                    Visual foundElement = null;
                    if (element is FrameworkElement)
                    {
                        (element as FrameworkElement).ApplyTemplate();
                    }

                    bool found = false;
                    for (int i = 0; !found && i < VisualTreeHelper.GetChildrenCount(element); i++)
                    {
                        Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                        foundElement = GetDescendant(visual, type);
                        if (foundElement != null)
                            found = true;
                    }

                    return foundElement;
                }

            #endregion

            #region Window and Keyboard Events

                /// <summary>
                /// Main window closing behavior
                /// </summary>
                private void Window_Closing(object sender, CancelEventArgs e)
                {

                    string message = string.Empty;
                    int identifier = 0;
                    bool templateBusy = false;
                    bool needConfirm = false;

                    // See if any background workers are currently running and ask if they want to really close
                    // If yes, cancel all of the running background processes
                    if (identifyProgress.IsBusy) // || matchProgress.IsBusy)
                    {
                        needConfirm = true;

                        message = "Identification is still working.\n\nDo you still want to close?";
                        identifier = 1;

                        if (matchProgress.IsBusy)
                            identifier = 0;

                        if (progress.IsBusy)
                        {
                            message = "Identification and template generation are still working.\n\nDo you still want to close?";
                            templateBusy = true;
                        }

                    }
                    else if (progress.IsBusy)
                    {
                        needConfirm = true;

                        message = "Template generation is still working.\n\nDo you still want to close?";
                        identifier = 2;
                    }

                    if (needConfirm)
                    {
                        MessageBoxResult choice = System.Windows.MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (choice == MessageBoxResult.Yes)
                        {
                            switch (identifier)
                            {
                                case 0:
                                    // Cancel matching, but don't break
                                    matchProgress.CancelAsync();
                                    goto case 1;
                                case 1:
                                    // Cancel identification/matching
                                    identifyProgress.CancelAsync();

                                    if (templateBusy)
                                        goto case 2;

                                    break;
                                case 2:
                                    // Cancel template generation
                                    progress.CancelAsync();
                                    break;
                            }
                        }
                        else
                        {
                            // Cancel the closing operation
                            e.Cancel = true;
                            return;
                        }
                    }

                    // Release all Neurotec Licenses
                    foreach (string license in faceLicenseComponents)
                    {
                        NLicense.ReleaseComponents(license);
                    }

                    // Close any subwindows we may have open
                    foreach (Window subWindow in App.Current.Windows)
                    {
                        if (subWindow != this)
                            subWindow.Close();
                    }
                }

                /// <summary>
                /// Sets focus to the window when the user clicks outside of a control
                /// </summary>
                private void Window_MouseDown(object sender, MouseButtonEventArgs e)
                {
                    MainGrid.Focus();
                }

                /// <summary>
                /// Sets focus to the window when the user presses enter within a control
                /// </summary>
                private void Settings_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        MainGrid.Focus();
                        e.Handled = true;
                    }
                }

                /// <summary>
                /// Performs the appropriate actions when tabs are changed
                /// </summary>
                private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    if (e.OriginalSource is System.Windows.Controls.TabControl)
                    {
                        if (((IdentifyTab != null && IdentifyTab.IsSelected) || (VerifyTab != null && VerifyTab.IsSelected) || (VerifyNTab != null && VerifyNTab.IsSelected)) && mainClient != null)
                        {
                            mainClient.Cancel();
                        }
                        else if (TemplateTab != null && TemplateTab.IsSelected)
                        {
                            ImportDatabases(curSettings.GetDatabases());

                            DatabaseDisplay.Items.Refresh();
                        }
                        else if (SettingsTab != null && SettingsTab.IsSelected)
                        {
                            NumOfMatches.Text = curSettings.GetNumMatches().ToString();
                            setMatchFAR.Text = curSettings.GetFar().ToString();
                            setThreshold.Text = curSettings.GetThreshold().ToString();
                            ImportDatabases(curSettings.GetDatabases());

                            DatabaseDisplayList.Items.Refresh();

                            // Make sure buttons are not enabled after loading the changes
                            ApplySetting.IsEnabled = false;
                            CancelSetting.IsEnabled = false;
                        }
                        else if (LogTab != null && LogTab.IsSelected)
                        {
                            LogText.ScrollToEnd();
                        }
                    }

                    e.Handled = true;
                }

                /// <summary>
                /// Sets the identification results to immediately handle the BringIntoView event
                /// </summary>
                private void ResultViewHandler(object sender, RequestBringIntoViewEventArgs e)
                {
                    e.Handled = true;
                }

            #endregion

        #endregion

        #region Internal Functions

            #region Neurotec and Common Identify/Verify Functions

                /// <summary>
                /// Starts the Neurotechnology license obtaining process and initializes the biometric client
                /// </summary>
                private void Start()
                {
                    try
                    {
                        Log("");
                        foreach (string license in faceLicenseComponents)
                        {
                            if (NLicense.ObtainComponents(Address, Port, license))
                            {
                                Log(string.Format("License was obtained: {0}", license));
                            }
                            else
                            {
                                Log(string.Format("License could not be obtained: {0}", license));
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("An error occurred in the license process:\n\n{0}", ex.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    Log("");
                    Log("Starting Client");
                    mainClient = new NBiometricClient();
                    mainClient.BiometricTypes = NBiometricType.Face;
                    mainClient.UseDeviceManager = true;
                    mainClient.Initialize();
                    Log("Client Was Initialized");
                }

                /// <summary>
                /// Creates a template for a subject, usually when obtaining from a file
                /// </summary>
                /// <param name="faceView">The biometric face data for the subject</param>
                /// <param name="newSubject">The biometric subject that contains the face template (Note: is an out parameter)</param>
                /// <returns>The file path to the selected image</returns>
                private void GetTemplate(NFaceView faceView, string filename, out NSubject newSubject)
                {
                    // Check if Segmentation license has been activated
                    if (NLicense.IsComponentActivated("Biometrics.FaceSegmentsDetection"))
                    {
                        // Set any additional biometric client features that we may want
                        mainClient.FacesDetectAllFeaturePoints = true;
                        mainClient.FacesDetectBaseFeaturePoints = true;
                        //mainClient.FacesDetermineGender = true;
                        //mainClient.FacesDetermineAge = true;
                        //mainClient.FacesDetectProperties = true;
                        //mainClient.FacesRecognizeEmotion = true;
                        //mainClient.FacesRecognizeExpression = true;
                    }

                    newSubject = null;
                    faceView.Face = null;

                    if (filename != string.Empty)
                    {
                        string[] splitName = System.IO.Path.GetFileNameWithoutExtension(filename).Split('_');

                        try
                        {
                            newSubject = NSubject.FromFile(filename);
                            newSubject.Id = splitName[0];
                        }
                        catch { }

                        if (newSubject == null)
                        {
                            newSubject = new NSubject();
                            NFace face = new NFace { Image = GetImage(filename) };
                            faceView.Face = face;
                            newSubject.Faces.Add(face);
                    
                            newSubject.Id = splitName[0];

                            Log("");
                            Log(string.Format("Attempting to create template ({0})", newSubject.Id));
                            mainClient.BeginCreateTemplate(newSubject, AsyncCreation, newSubject);
                        }
                    }
                }

                /// <summary>
                /// Reads an array of templates and enrolls them to the biometric client
                /// </summary>
                /// <param name="allTemplates">An array of templates</param>
                private void EnrollTemplates(string[] allTemplates)
                {
                    // Go through all templates
                    // Import subject + RID into a new NSubject

                    referenceSubject = new NSubject[allTemplates.Length];

                    for (int i = 0; i < allTemplates.Length; i++)
                    {
                        referenceSubject[i] = NSubject.FromFile(allTemplates[i]);
                        referenceSubject[i].Id = System.IO.Path.GetFileNameWithoutExtension(allTemplates[i]);

                        // Add subjects to the enrollment "task"
                        enrollment.Subjects.Add(referenceSubject[i]);

                        // Added if statement just so that we can run the method without running identification
                        if (identifyProgress.IsBusy)
                        {
                            identifyCount++;
                            identifyProgress.ReportProgress(identifyCount, "Enrolling Subject - " + referenceSubject[i].Id);
                            Thread.Sleep(1);
                        }
                    }

                    // Perform the "task" which contains each subject
                    mainClient.PerformTask(enrollment);
                }

                /// <summary>
                /// Determines if the subject is instantiated and its status is valid
                /// </summary>
                /// <param name="subject">The subject to evaluate</param>
                /// <returns>True, is subject is valid and, False, if not</returns>
                private bool ValidSubject(NSubject subject)
                {
                    return subject != null && (subject.Status == NBiometricStatus.Ok
                        || subject.Status == NBiometricStatus.MatchNotFound
                        || subject.Status == NBiometricStatus.None && subject.GetTemplateBuffer() != null);
                }

            #endregion

            #region Directory/Network Functions

                /// <summary>
                /// Displays a list of databases to the GUI
                /// </summary>
                /// <param name="databases">A list of strings with database file paths</param>
                private void ImportDatabases(List<string> databases)
                {
                    DatabaseDisplayList.Items.Clear();
                    DatabaseDisplay.Items.Clear();
                    for (int index = 0; index < databases.Count; ++index)
                    {
                        DatabaseDisplayList.Items.Add(databases[index]);
                        DatabaseDisplay.Items.Add(databases[index]);
                    }
                }

                /// <summary>
                /// Checks to see if each database/directories exists within the system
                /// </summary>
                /// <param name="list">A list of strings with database file paths</param>
                /// <returns>True, if all databases are present or, False, if not</returns>
                private bool AllDatabasesExist(List<string> list)
                {
                    Log("");
                    Log("Checking Databases");

                    bool noErrors = true;
                    for (int index = 0; index < list.Count; ++index)
                    {
                        if (GetAccess(list[index], false) && !Directory.Exists(list[index]))
                        {
                            Log(string.Format("Error - Database not recognized: {0}", list[index]));
                            noErrors = false;
                        }
                    }
                    Log("Database Check Successful");

                    return noErrors;
                }

                /// <summary>
                /// Checks to see if we are able to get access to an UNC path or directory
                /// </summary>
                /// <param name="uncPath">The UNC path or directory path</param>
                /// <returns>True if we can connect, false otherwise</returns>
                public bool GetAccess(string uncPath)
                {
                    bool result = false;

                    // Invoked main dispatcher if we decide to use this method in any background workers
                    // Dispatcher needed to return log messages to GUI (since on different thread of execution)
                    this.Dispatcher.Invoke(() =>
                    {
                        Log("");
                        try
                        {
                            Directory.GetAccessControl(uncPath);
                            result = true;
                            Log("Access granted, connected to: " + uncPath);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Could be read-only, so it still exists
                            result = true;
                            Log("Access unauthorized, connected to: " + uncPath);
                        }
                        catch
                        {
                            // Some other error, so assume it doesn't exist
                            result = false;
                            Log("Error occurred, could not access: " + uncPath);
                        }
                    });

                    return result;
                }

                /// <summary>
                /// Checks to see if we are able to get access to an UNC path or directory
                /// </summary>
                /// <param name="uncPath">The UNC path or directory path</param>
                /// <param name="wantLog">True if you want log messages, false if not</param>
                /// <returns>True if we can connect, false otherwise</returns>
                public bool GetAccess(string uncPath, bool wantLog)
                {
                    bool result = false;

                    if (wantLog)
                        return GetAccess(uncPath);
                    else
                    {
                        try
                        {
                            Directory.GetAccessControl(uncPath);
                            result = true;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Could be read-only, so it still exists
                            result = true;
                        }
                        catch
                        {
                            // Some other error, so assume it doesn't exist
                            result = false;
                        }
                    }
                    return result;
                }

                /// <summary>
                /// Evaluates an array of directory paths and returns only accessible paths
                /// </summary>
                /// <param name="directories">An array of directory paths</param>
                /// <returns>A resulting array of accessible directories</returns>
                private string[] GetAccessibleDirectories(string[] directories)
                {
                    List<string> accessible = new List<string>();

                    foreach (string dir in directories)
                        if (GetAccess(dir, false))
                            accessible.Add(dir);

                    return accessible.ToArray();
                }

                /// <summary>
                /// Evaluates a list of directory paths and returns only accessible paths
                /// </summary>
                /// <param name="directories">A list of directory paths</param>
                /// <returns>A resulting array of accessible directories</returns>
                private string[] GetAccessibleDirectories(List<string> directories)
                {
                    List<string> accessible = new List<string>();

                    foreach (string dir in directories)
                        if (GetAccess(dir, false))
                            accessible.Add(dir);

                    return accessible.ToArray();
                }

            #endregion

            #region GetNumberSubjects Functions

                /// <summary>
                /// Searches through each directory and counts the number of subjects
                /// </summary>
                /// <returns>The total number of subjects from all of the databases currently saved</returns>
                private int GetNumberSubjects()
                {
                    int subjectCount = 0;
                    HashSet<string> RIDRecord = new HashSet<string>();

                    // Loop through each directory provided
                    foreach (string dir in GetAccessibleDirectories(curSettings.GetDatabases()))
                    {
                        // Get all of the sub directories, which should be our RID folders
                        string[] ridDirectory = Directory.GetDirectories(dir);

                        // Loop through all of the RID's
                        for (int j = 0; j < ridDirectory.Length; j++)
                        {
                            // Get the RID from the directory name
                            string RID = new DirectoryInfo(ridDirectory[j]).Name;

                            // Check if RID already exists in our dictionary
                            if (!RIDRecord.Contains(RID))
                            {
                                subjectCount++;

                                RIDRecord.Add(RID);
                            }

                        }
                    }

                    return subjectCount;
                }

                /// <summary>
                /// Searches through each directory and counts the number of subjects
                /// </summary>
                /// <returns>The total number of subjects from all of the databases linked</returns>
                private int GetNumberSubjects(string directory)
                {
                    return Directory.GetDirectories(directory).Length;
                }

                /// <summary>
                /// Searches through each directory and counts the number of subjects
                /// </summary>
                /// <returns>The total number of subjects from all of the databases linked</returns>
                private int GetNumberSubjects(List<string> directories)
                {
                    int subjectCount = 0;
                    HashSet<string> RIDRecord = new HashSet<string>();

                    // Loop through each directory provided
                    for (int i = 0; i < directories.Count; i++)
                    {
                        // Get all of the sub directories, which should be our RID folders
                        string[] ridDirectory = Directory.GetDirectories(directories[i]);

                        // Loop through all of the RID's
                        for (int j = 0; j < ridDirectory.Length; j++)
                        {
                            // Get the RID from the directory name
                            string RID = new DirectoryInfo(ridDirectory[j]).Name;

                            // Check if RID already exists in our dictionary
                            if (!RIDRecord.Contains(RID))
                            {
                                subjectCount++;

                                RIDRecord.Add(RID);
                            }
                        }
                    }

                    return subjectCount;
                }

                /// <summary>
                /// Searches through each directory and counts the number of subjects
                /// </summary>
                /// <returns>The total number of subjects from all of the databases linked</returns>
                private int GetNumberSubjects(string[] directories)
                {
                    int subjectCount = 0;
                    HashSet<string> RIDRecord = new HashSet<string>();

                    // Loop through each directory provided
                    for (int i = 0; i < directories.Length; i++)
                    {
                        // Get all of the sub directories, which should be our RID folders
                        string[] ridDirectory = Directory.GetDirectories(directories[i]);

                        // Loop through all of the RID's
                        for (int j = 0; j < ridDirectory.Length; j++)
                        {
                            // Get the RID from the directory name
                            string RID = new DirectoryInfo(ridDirectory[j]).Name;

                            // Check if RID already exists in our dictionary
                            if (!RIDRecord.Contains(RID))
                            {
                                subjectCount++;

                                RIDRecord.Add(RID);
                            }
                        }
                    }

                    return subjectCount;
                }

            #endregion

            #region Get Image File Functions

                /// <summary>
                /// Finds the icon/token image for an RID
                /// </summary>
                /// <param name="RID">A subject's RID</param>
                /// <returns>A string path to the intended icon/token image</returns>
                private string GetTokenImagePath(string RID)
                {
                    string image = string.Empty;

                    List<string> tempDatabase = curSettings.GetDatabases();
            
                    bool found = false;
                    for (int i = 0; i < tempDatabase.Count && found != true; i++)
                    {
                        string tempDir = tempDatabase[i] + "\\" + RID;
                        if (Directory.Exists(tempDir))
                        {
                            image = GetOpenEyeImage(tempDir);

                            if(image != string.Empty)
                                found = true;
                        }
                    }

                    return image;
                }

                /// <summary>
                /// Gets an image from a file and resizes it into an NImage
                /// </summary>
                /// <param name="file">The string path to an image</param>
                /// <returns>An NImage object of the resized image</returns>
                private NImage GetImage(string file)
                {
                    byte[] byteImg;

                    BitmapImage img = new BitmapImage();
                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.None;
                    img.DecodePixelHeight = 960; // 1280 vs 960;
                    img.DecodePixelWidth = 1440; // 1920 vs 1440;
                    img.UriSource = new Uri(file);
                    img.Rotation = Rotation.Rotate90;
                    img.EndInit();
            
                    if (img.CanFreeze)
                        img.Freeze();

                    BmpBitmapEncoder encode = new BmpBitmapEncoder();
                    encode.Frames.Add(BitmapFrame.Create(img));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encode.Save(ms);
                        byteImg = ms.ToArray();
                    }

                    return NImage.FromMemory(byteImg);
                }

                /// <summary>
                /// Gets faces images from a specific directory
                /// </summary>
                /// <param name="currentDirectory">The directory to be searched</param>
                /// <returns>A list of file paths to front facing images</returns>
                private List<string> GetAllImages(string currentDirectory)
                {
                    List<string> imgFiles = new List<string>();
                    string mainDirectory = new DirectoryInfo(currentDirectory).Parent.FullName;

                    /* TWINS DAY 2017 */
                    if (mainDirectory.Equals(defaultDatabases[defaultDatabases.Count - 1]))
                    {
                        foreach (string childDir in Directory.GetDirectories(currentDirectory))
                        {
                            // Try, just in case there is a file structure that doesn't conform
                            try
                            {
                                imgFiles.AddRange(getAllFilePaths(childDir + @"\C5D3\RAW", "*_FACE_C5D3_RAW_?C.jpg"));
                                //string ccImg = Directory.GetFiles(childDir + @"\C5D3\RAW", "*_FACE_C5D3_RAW_CC.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                                //string ncImg = Directory.GetFiles(childDir + @"\C5D3\RAW", "*_FACE_C5D3_RAW_NC.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();

                                //if (ccImg != null)
                                    //imgFiles.Add(ccImg);

                                //if (ncImg != null)
                                    //imgFiles.Add(ncImg);
                            }
                            catch { }
                        }
                    }

                    /* TWINS DAY 2015 */
                    else if (mainDirectory.Equals(defaultDatabases[0]) || mainDirectory.Equals(defaultDatabases[1]))
                    {
                        imgFiles.AddRange(getAllFilePaths(currentDirectory + @"\Face\Canon Mark III\1\Raw", "*_0_?.jpg"));
                    }

                    /* TWINS DAY 2016 */
                    else if (mainDirectory.Equals(defaultDatabases[2])) 
                    {
                        // Gets child directories since the subfolders are not the same for each participant
                        foreach (string childDir in Directory.GetDirectories(currentDirectory))
                        {
                            try
                            {
                                // Since we know there's only one forward image, we can just grab the first
                                string img = Directory.GetFiles(childDir + @"\FACE\C5D3\RAW", "*C5D3_0.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();

                                if(img != null)
                                    imgFiles.Add(img);
                            }
                            catch { }
                        }
                    }

                    /* TWINS DAY 2014 */
                    else if (mainDirectory.Equals(defaultDatabases[3]))
                    {
                        // Gets child directories since the subfolders are not the same for each participant
                        foreach (string childDir in Directory.GetDirectories(currentDirectory))
                        {
                            try
                            {
                                imgFiles.AddRange(getAllFilePaths(childDir + @"\raw\1", "*_0_?_001_CanonEOS5DMarkIII.jpg"));
                            }
                            catch { }
                        }
                    }

                    /* OTHER */
                    else
                    {
                        // May remove, since we don't want the system to spend forever collecting all images
                        imgFiles.AddRange(getAllFilePaths(currentDirectory, "*.jpg"));
                    }

                    return imgFiles;
                }

                /// <summary>
                /// Gets a face image that only have eyes open from a specific directory
                /// </summary>
                /// <param name="currentDirectory">The directory to be searched</param>
                /// <returns>A string path to an open eye picture</returns>
                private string GetOpenEyeImage(string currentDirectory)
                {
                    string openImage = string.Empty;
                    string mainDirectory = new DirectoryInfo(currentDirectory).Parent.FullName;

                    /* TWINS DAY 2017 */
                    if (mainDirectory.Equals(defaultDatabases[defaultDatabases.Count - 1]))
                    {
                        foreach (string childDir in Directory.GetDirectories(currentDirectory))
                        {
                            if (openImage == null || openImage == string.Empty)
                            {
                                // Try, just in case there is a file structure that doesn't conform
                                try
                                {
                                    openImage = Directory.GetFiles(childDir + @"\C5D3\RAW", "*_C5D3_RAW_CC.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                                }
                                catch { }
                            }
                        }
                    }

                    /* TWINS DAY 2015 */
                    else if (mainDirectory.Equals(defaultDatabases[0]) || mainDirectory.Equals(defaultDatabases[1]))
                    {
                        openImage = Directory.GetFiles(currentDirectory + @"\Face\Canon Mark III\1\Raw", "*_0_O.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    }

                    /* TWINS DAY 2016 */
                    else if (mainDirectory.Equals(defaultDatabases[2]))
                    {
                        // Gets child directories since the subfolders are not the same for each participant
                        foreach (string childDir in Directory.GetDirectories(currentDirectory))
                        {
                            if (openImage == null || openImage == string.Empty)
                            {
                                try
                                {
                                    openImage = Directory.GetFiles(childDir + @"\FACE\C5D3\RAW", "*C5D3_0.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                                }
                                catch { }
                            }
                        }
                    }

                    /* TWINS DAY 2014 */
                    else if (mainDirectory.Equals(defaultDatabases[3]))
                    {
                        // Gets child directories since the subfolders are not the same for each participant
                        foreach (string childDir in Directory.GetDirectories(currentDirectory))
                        {
                            if (openImage == null || openImage == string.Empty)
                            {
                                try
                                {
                                    openImage = Directory.GetFiles(childDir + @"\raw\1", "*_0_O_001_CanonEOS5DMarkIII.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                                }
                                catch { }
                            }
                        }
                    }

                    /* OTHER */
                    else
                    {
                        openImage = Directory.GetFiles(currentDirectory, "*.jpg").FirstOrDefault();
                    }

                    // Make sure that, if null, to set string to empty
                    if (openImage == null)
                        openImage = string.Empty;

                    return openImage;
                }

            #endregion

            #region Conversion Functions

                /// <summary>
                /// Calculates the FAR associated with a threshold/score value
                /// </summary>
                /// <param name="score">The int value of the threshold/score</param>
                /// <returns>The decimal value of the FAR</returns>
                private double GetFAR(int score)
                {
                    return Math.Pow(10, ((double)score / -12));
                }

                /// <summary>
                /// Calculates the threshold/score associated with a FAR value
                /// </summary>
                /// <param name="far">The decimal value of the FAR</param>
                /// <returns>The int value of the threshold/score</returns>
                private int GetScore(double far)
                {
                    return (int)Math.Ceiling(Math.Log10(far) * (-12));
                }

                /// <summary>
                /// Formula from Neurotec documentation to find the False Acceptance Probability
                /// </summary>
                /// <param name="far">The FAR (False Acceptance Rate) in decimal form</param>
                /// <returns>The False Acceptance Probability in percentage</returns>
                private double GetFalseProbability(double far)
                {
                    // Formula from documentation to determine the probability of a false match
                    return (1 - Math.Pow((1 - (far / 100)), identifyMax)) * 100;
                }

            #endregion

            #region Other Functions

                /// <summary>
                /// Searches through a directory and returns a list of files
                /// </summary>
                /// <param name="baseDir">The file path of the directory to be searched</param>
                /// <param name="fileFormat">Type of format of files to be searched. So, "*.jpg" would only return .jpg files</param>
                /// <returns>A list of file paths to every matching filetype within the given directory</returns>
                private List<string> getAllFilePaths(string baseDir, string fileFormat)
                {
                    List<string> allFiles = new List<string>();
                    Queue<string> pendingFolders = new Queue<string>();
                    pendingFolders.Enqueue(baseDir);
                    string[] temp;

                    while (pendingFolders.Count > 0)
                    {
                        baseDir = pendingFolders.Dequeue();
                        temp = Directory.GetFiles(baseDir, fileFormat, SearchOption.TopDirectoryOnly);

                        for (int i = 0; i < temp.Length; i++)
                            allFiles.Add(temp[i]);

                        temp = Directory.GetDirectories(baseDir);
                        for (int i = 0; i < temp.Length; i++)
                            pendingFolders.Enqueue(temp[i]);
                    }

                    return allFiles;
                }

                /// <summary>
                /// Method used to display actions into the log page
                /// </summary>
                /// <param name="s">The string message to be logged</param>
                private void Log(string s)
                {
                    LogText.Text += string.Format("{0}\t{1}\n", lineNum, s);
                    lineNum++;
                }

            #endregion

        #endregion

        #region Scanning Functions

            #region Identify Scan/Select/Deselect Functions

                /// <summary>
                /// Scans in the subject for identification
                /// </summary>
                private void IdentifyScan_Click(object sender, RoutedEventArgs e)
                {
                    string rid = IdentificationScanPath.Text.Split('_')[0];
                    identScanFiles = new List<string>();

                    identScanFiles = ScanFiles(rid, IdentifyImageSelect);

                    if (identScanFiles.Count > 0)
                    {
                        IdentifyImport.IsEnabled = false;
                        IdentificationImportPath.IsEnabled = false;
                        IdentifyScan.IsEnabled = false;
                        IdentificationScanPath.IsEnabled = false;

                        ParticipantLabel.Content = "Participant: " + rid;

                        IdentifyImageSelect.IsEnabled = true;
                        IdentificationClearWindow.IsEnabled = true;

                        Log("");
                        Log(string.Format("Scanned in Subject ({0})", rid));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) did not contain any demo files", rid));
                    }
                }

                /// <summary>
                /// Selects the currently listed image and imports their template
                /// </summary>
                private void IdentifySelect_Click(object sender, RoutedEventArgs e)
                {
                    IdentifyImageSelect.IsEnabled = false;
                    IdentifyDeselect.IsEnabled = true;
                    IdentifySelect.IsEnabled = false;

                    GetTemplate(faceIdent, identScanFiles[IdentifyImageSelect.SelectedIndex], out identSubject);

                    EnableIdentify();
                }

                /// <summary>
                /// Clears the current image/subject template
                /// </summary>
                private void IdentifyDeselect_Click(object sender, RoutedEventArgs e)
                {
                    IdentifyImageSelect.IsEnabled = true;
                    IdentifyDeselect.IsEnabled = false;
                    IdentifySelect.IsEnabled = true;

                    faceIdent.Face = null;
                    identSubject = null;

                    GenerateMatches.IsEnabled = false;
                }

            #endregion

            #region Left Verify Scan/Select/Deselect Functions

                /// <summary>
                /// Scans in the subject for the left verification subject
                /// </summary>
                private void LeftScan_Click(object sender, RoutedEventArgs e)
                {
                    string rid = LeftScanPath.Text.Split('_')[0];
                    leftScanFiles = new List<string>();

                    leftScanFiles = ScanFiles(rid, LeftImageSelect);

                    if (leftScanFiles.Count > 0)
                    {
                        LeftImport.IsEnabled = false;
                        LeftImportPath.IsEnabled = false;
                        LeftScan.IsEnabled = false;
                        LeftScanPath.IsEnabled = false;

                        LeftImageSelect.IsEnabled = true;
                        VerificationClearLeft.IsEnabled = true;
                        VerificationClear.IsEnabled = true;

                        Log("");
                        Log(string.Format("Scanned in Subject ({0})", rid));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) did not contain any demo files", rid));
                    }
                }

                /// <summary>
                /// Selects the currently listed image and imports their template
                /// </summary>
                private void LeftSelect_Click(object sender, RoutedEventArgs e)
                {
                    LeftImageSelect.IsEnabled = false;
                    LeftDeselect.IsEnabled = true;
                    LeftSelect.IsEnabled = false;

                    GetTemplate(faceVerLeft, leftScanFiles[LeftImageSelect.SelectedIndex], out verSubjectLeft);

                    EnableVerify();
                }

                /// <summary>
                /// Clears the current image/subject template
                /// </summary>
                private void LeftDeselect_Click(object sender, RoutedEventArgs e)
                {
                    LeftImageSelect.IsEnabled = true;
                    LeftDeselect.IsEnabled = false;
                    LeftSelect.IsEnabled = true;

                    faceVerLeft.Face = null;
                    verSubjectLeft = null;

                    VerifyImages.IsEnabled = false;
                }

            #endregion

            #region Right Verify Scan/Select/Deselect Functions

                /// <summary>
                /// Scans in the subject for the right verification subject
                /// </summary>
                private void RightScan_Click(object sender, RoutedEventArgs e)
                {
                    string rid = RightScanPath.Text.Split('_')[0];
                    rightScanFiles = new List<string>();

                    rightScanFiles = ScanFiles(rid, RightImageSelect);

                    if (rightScanFiles.Count > 0)
                    {
                        RightImport.IsEnabled = false;
                        RightImportPath.IsEnabled = false;
                        RightScan.IsEnabled = false;
                        RightScanPath.IsEnabled = false;

                        RightImageSelect.IsEnabled = true;
                        VerificationClearRight.IsEnabled = true;
                        VerificationClear.IsEnabled = true;

                        Log("");
                        Log(string.Format("Scanned in Subject ({0})", rid));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) did not contain any demo files", rid));
                    }
                }

                /// <summary>
                /// Selects the currently listed image and imports their template
                /// </summary>
                private void RightSelect_Click(object sender, RoutedEventArgs e)
                {
                    RightImageSelect.IsEnabled = false;
                    RightDeselect.IsEnabled = true;
                    RightSelect.IsEnabled = false;

                    GetTemplate(faceVerRight, rightScanFiles[RightImageSelect.SelectedIndex], out verSubjectRight);

                    EnableVerify();
                }

                /// <summary>
                /// Clears the current image/subject template
                /// </summary>
                private void RightDeselect_Click(object sender, RoutedEventArgs e)
                {
                    RightImageSelect.IsEnabled = true;
                    RightDeselect.IsEnabled = false;
                    RightSelect.IsEnabled = true;

                    faceVerRight.Face = null;
                    verSubjectRight = null;

                    VerifyImages.IsEnabled = false;
                }

            #endregion

            #region Verify Scan/Select/Deselect Functions

                /// <summary>
                /// Scans in the subject for the right verification subject
                /// </summary>
                private void VerifyScan_Click(object sender, RoutedEventArgs e)
                {
                    string rid = VerifyScanPath.Text.Split('_')[0];
                    verifyScanFiles = new List<string>();
                    verifySubject = null;

                    verifyScanFiles = ScanFiles(rid, VerifyImageSelect);

                    if (verifyScanFiles.Count > 0)
                    {
                        VerifyImport.IsEnabled = false;
                        VerifyImportPath.IsEnabled = false;
                        VerifyScan.IsEnabled = false;
                        VerifyScanPath.IsEnabled = false;

                        VerifyImageSelect.IsEnabled = true;
                        VerifySubject_Clear.IsEnabled = true;

                        Log("");
                        Log(string.Format("Scanned in Subject ({0})", rid));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) did not contain any demo files", rid));
                    }
                }

                /// <summary>
                /// Selects the currently listed image and imports their template
                /// </summary>
                private void VerifySelect_Click(object sender, RoutedEventArgs e)
                {
                    VerifyImageSelect.IsEnabled = false;
                    VerifyDeselect.IsEnabled = true;
                    VerifySelect.IsEnabled = false;

                    GetTemplate(faceVerify, verifyScanFiles[VerifyImageSelect.SelectedIndex], out verifySubject);

                    EnableVerifyN();
                }

                /// <summary>
                /// Clears the current image/subject template
                /// </summary>
                private void VerifyDeselect_Click(object sender, RoutedEventArgs e)
                {
                    VerifyImageSelect.IsEnabled = true;
                    VerifyDeselect.IsEnabled = false;
                    VerifySelect.IsEnabled = true;

                    verifySubject = null;
                    faceVerify.Face = null;

                    VerifySubjectImage.IsEnabled = false;
                }

            #endregion

            #region Selection Changed Functions

                /// <summary>
                /// Actiavtes the select button when the selected image changes
                /// </summary>
                private void IdentifyImageSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    IdentifySelect.IsEnabled = true;
                }

                /// <summary>
                /// Actiavtes the select button when the selected image changes
                /// </summary>
                private void LeftImageSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    LeftSelect.IsEnabled = true;
                }

                /// <summary>
                /// Actiavtes the select button when the selected image changes
                /// </summary>
                private void RightImageSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    RightSelect.IsEnabled = true;
                }

                /// <summary>
                /// Actiavtes the select button when the selected image changes
                /// </summary>
                private void VerifyImageSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    VerifySelect.IsEnabled = true;
                }

            #endregion

            #region Enter Key Functions

                /// <summary>
                /// Activates the scanning process when the enter key is pressed
                /// </summary>
                private void LeftScanPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        LeftScan_Click(null, null);
                        e.Handled = true;
                    }
                }

                /// <summary>
                /// Activates the scanning process when the enter key is pressed
                /// </summary>
                private void RightScanPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        RightScan_Click(null, null);
                        e.Handled = true;
                    }
                }

                /// <summary>
                /// Activates the scanning process when the enter key is pressed
                /// </summary>
                private void IdentifyScanPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        IdentifyScan_Click(null, null);
                        e.Handled = true;
                    }
                }

                /// <summary>
                /// Activates the scanning process when the enter key is pressed
                /// </summary>
                private void VerifyScanPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        VerifyScan_Click(null, null);
                        e.Handled = true;
                    }
                }

            #endregion

            /// <summary>
            /// Finds all demo files for an RID and loads it into a combobox
            /// </summary>
            /// <param name="RID">The RID identifier for a subject</param>
            /// <param name="combo">The combobox object to write to</param>
            /// <returns>The list of demo files found</returns>
            private List<string> ScanFiles(string RID, System.Windows.Controls.ComboBox combo)
            {
                List<string> scannedFiles = new List<string>();
                string foundImg;

                if (Directory.Exists(defaultDatabases[defaultDatabases.Count - 1] + "\\" + RID))
                {
                    // Counters, in case we are unable to extract a date from the file path
                    int normalCount = 0;
                    int disguiseCount = 0;

                    // Gets all "Date" folders from RID
                    //string[] subDirectories = Directory.GetDirectories(defaultDatabases[defaultDatabases.Count - 1] + "\\" + RID);
                    foreach (string dateFolder in Directory.GetDirectories(defaultDatabases[defaultDatabases.Count - 1] + "\\" + RID))
                    {
                        bool dateExtracted = false;
                        int day = 0;
                        int month = 0;
                        int year = 0;

                        // Try to extract the date out of the directory name
                        try
                        {
                            string dateFolderName = new DirectoryInfo(dateFolder).Name;
                            month = Int32.Parse(dateFolderName.Substring(0, 2));
                            day = Int32.Parse(dateFolderName.Substring(2, 2));
                            year = Int32.Parse(dateFolderName.Substring(6, 2));

                            dateExtracted = true;
                        }
                        catch { }

                        try
                        {
                            // Try to get the normal face
                            foundImg = Directory.GetFiles(dateFolder + @"\C5D3\RAW", "*_C5D3_RAW_0.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                            if (foundImg != null)
                            {
                                normalCount++;

                                if (dateExtracted)
                                    combo.Items.Add(string.Format("Normal Face ({0}/{1}/{2})", month, day, year));
                                else
                                    combo.Items.Add(string.Format("Normal Face (#{0})", normalCount));

                                scannedFiles.Add(foundImg);
                            }
                        }
                        catch { }

                        try
                        {
                            // Try to get the disguised face
                            foundImg = Directory.GetFiles(dateFolder + @"\C5D3\RAW", "*_C5D3_RAW_0D.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                            if (foundImg != null)
                            {
                                disguiseCount++;

                                if (dateExtracted)
                                    combo.Items.Add(string.Format("Disguised Face ({0}/{1}/{2})", month, day, year));
                                else
                                    combo.Items.Add(string.Format("Disguised Face (#{0})", disguiseCount));

                                scannedFiles.Add(foundImg);
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) was not recognized", RID));
                }

                return scannedFiles;
            }

        #endregion

    }
}
