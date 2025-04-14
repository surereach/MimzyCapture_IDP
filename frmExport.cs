using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using IronBarCode;


namespace SRDocScanIDP
{
    public partial class frmExport : Form
    {
        //private Dynamsoft.DBR.BarcodeReader oBarcodeRdr = null;
        //private PublicRuntimeSettings mNormalRuntimeSettings;

        //EnumBarcodeFormat mEmBarcodeFormat = 0;
        //EnumBarcodeFormat_2 mEmBarcodeFormat_2 = 0;

        string sErrMsg;
        //EnumErrorCode enmErrCode;

        private static string _currScanProj;
        private static string _appNum;
        private static string _docDefId;

        private static staMain._stcExportInfo stcXmlInfo;
        //private static staMain._stcIndexFieldValues stcIndexValues;

        private bool bCompleted;

        //private delegate void OCRPdfThread();

        private static string sSetNumFmt;

        //private bool _isDocSep; //Document Separator.

        private static clsDocuFile oDF = new clsDocuFile();

        private bool mbCustom = false;
        private static string sCustBarcodeFmt;
        private static string sCustBarcodeFmt2;
        private static string sCustRecog;

        private static string sStatusIn;
        private static string sSet;

        private static bool bDone;
        private static List<Task> lTasks;
        private static List<Task> lSearchTasks;

        public frmExport()
        {
            InitializeComponent();

            //oBarcodeRdr = new Dynamsoft.DBR.BarcodeReader();
            //mNormalRuntimeSettings = oBarcodeRdr.GetRuntimeSettings();
            //mEmBarcodeFormat = EnumBarcodeFormat.BF_CODE_128;
            //mEmBarcodeFormat_2 = EnumBarcodeFormat_2.BF2_DOTCODE;

            _currScanProj = MDIMain.strScanProject; //this.MdiParent.Controls["toolStrip"].Controls[0].Text;
            _appNum = MDIMain.intAppNum.ToString("000");
            _docDefId = staMain.stcProjCfg.DocDefId;

            staMain.loadIndexConfigDB(_currScanProj, _appNum, _docDefId);

            if (staMain.stcProjCfg.BatchType.Trim().ToLower() == "set")
                sSetNumFmt = "000000";
            else
                sSetNumFmt = "000";

            IronBarCode.License.LicenseKey = MDIMain.strIronBcLic;
        }

        private void frmExport_Load(object sender, EventArgs e)
        {
            this.Text = this.Text + ": " + MDIMain.strScanProject;

            sErrMsg = "";
            //EnumErrorCode enmErrCode = BarcodeReader.InitLicense(sLicense, out errorMsg);

            //DMDLSConnectionParameters dlsConnParam = Dynamsoft.DBR.BarcodeReader.InitDLSConnectionParameters();
            //dlsConnParam.MainServerURL = mGlobal.GetAppCfg("BCLServer");
            //dlsConnParam.HandshakeCode = mGlobal.GetAppCfg("BCLic");
            ////dlsConnParam.OrganizationID = "100916317";
            //enmErrCode = Dynamsoft.DBR.BarcodeReader.InitLicenseFromDLS(dlsConnParam, out sErrMsg);

            //if (enmErrCode != EnumErrorCode.DBR_SUCCESS)
            //{
            //    mGlobal.Write2Log(sErrMsg);
            //}
            if (!IronBarCode.License.IsValidLicense(IronBarCode.License.LicenseKey))
            {
                mGlobal.Write2Log("Iron Barcode license is invalid!");
            }
            sErrMsg = "";

            int appNum = MDIMain.intAppNum;
            _currScanProj = MDIMain.strScanProject; //this.MdiParent.Controls["toolStrip"].Controls[0].Text;
            _appNum = appNum.ToString("000");
            _docDefId = staMain.stcProjCfg.DocDefId;

            bCompleted = false;

            getBarcodeCustomSettings();

            initExportInfo(1);

            loadDocSetStatus();
            if (cbxStatus.Items.Count > 0)
                cbxStatus.SelectedIndex = 0;

            sStatusIn = "'5'";
            sSet = "";
            staMain.sessionRestart();
        }

        private void frmExport_Resize(object sender, EventArgs e)
        {
            try
            {
                this.Width = btnCancel.Location.X + btnCancel.Width + 55;
                this.Height = btnOpenFolder.Location.Y + btnOpenFolder.Height + 70;
            }
            catch (Exception ex)
            {
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            bCompleted = true;
            this.Close();
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                sSet = cbxStatus.Text.Trim();
                string sFileExt = rdbPDF.Text;
                bool bOK = true;
                int iExpCnt = 0;

                if (rdbTIFF.Checked)
                {
                    sFileExt = rdbTIFF.Text;
                }

                if (MessageBox.Show("Confirm to export " + cbxStatus.Text + " document sets to " + sFileExt.ToUpper() + "?","Confirmation", 
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }

                cbxStatus.Enabled = false;
                btnExport.Enabled = false;
                btnCancel.Enabled = false;
                btnOpenFolder.Enabled = false;

                staMain.sessionStop(false);

                Cursor.Current = Cursors.WaitCursor;
                bDone = false;
                lTasks = new List<Task>();
                lSearchTasks = new List<Task>();

                if (sSet.Trim() == "")
                    MessageBox.Show("Document set is blank! Please select a document set.");
                else
                {
                    sStatusIn = "'5','7'";
                    if (sSet.ToLower() == "all")
                        sStatusIn = "'1','2','5','7'";
                    else if (sSet.ToLower() == "scanned")
                        sStatusIn = "'1','7'";
                    else if (sSet.ToLower() == "indexed")
                        sStatusIn = "'2','7'";

                    if (rdbPDF.Checked)
                    {
                        BackgroundWorker bg = new BackgroundWorker();
                        bg.DoWork += new DoWorkEventHandler(bg_DoWork);
                        bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
                        bg.RunWorkerAsync();

                        do
                        {
                            if (!bg.IsBusy) bDone = true;
                        }
                        while (!bDone);

                        Task tTasks = Task.WhenAll(lTasks.ToArray());
                        try
                        {
                            await tTasks;
                        }
                        catch (Exception ex)
                        {
                        }

                        tTasks.Dispose();
                        bg.Dispose();

                        Task tSearchTasks = Task.WhenAll(lSearchTasks.ToArray());
                        try
                        {
                            await tSearchTasks;
                        }
                        catch (Exception ex)
                        {
                        }
                        tSearchTasks.Dispose();
                    }
                    else if (rdbTIFF.Checked)
                    {
                        if (staMain.stcProjCfg.ExpMerge.Trim().ToUpper() == "Y")
                            saveTIFFByDocTypeMerge(sSet);
                        else
                            saveTIFFByDocTypeMany(sSet);
                    }

                    if (rdbXML.Checked)
                    {
                        clsXML oGenXml = new clsXML();
                        oGenXml._sCurrProj = _currScanProj;
                        oGenXml._sAppNum = _appNum;
                        oGenXml._sDocDefId = _docDefId;
                        oGenXml._stcXmlInfo = stcXmlInfo;
                        bOK = oGenXml.generateXML(sFileExt);
                    }
                    else
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        bOK = generateText();
                    }

                    if (bOK)
                    {
                        if (stcXmlInfo.BatchCodes.Count == 0)
                        {
                            MessageBox.Show(this, "No " + sFileExt + " files are exported!");
                            return;
                        }

                        int i = 0; int iCnt = 0;
                        while (i < stcXmlInfo.BatchCodes.Count)
                        {
                            if (stcXmlInfo.BatchCodes[i] == null)
                                break;
                            else if (stcXmlInfo.BatchCodes.Count == 1 && stcXmlInfo.BatchCodes[0] == string.Empty)
                            {
                                iCnt = 0;
                                break;
                            }
                            else
                            {
                                iExpCnt = getExportBatchDB(stcXmlInfo.BatchCodes[i].ToString(), sStatusIn);

                                if (sSet.ToLower() == "scanned")
                                    bOK = updateExportScannedDB(stcXmlInfo.BatchCodes[i].ToString(), "'S','TF'", "Y");
                                else
                                    bOK = updateExportIndexedDB(stcXmlInfo.BatchCodes[i].ToString(), "'I','V','TF'", "Y");

                                if (bOK)
                                    bOK = updateExportBatchDB(stcXmlInfo.BatchCodes[i].ToString(), sStatusIn, iExpCnt + 1);

                                if (bOK) iCnt += 1;
                            }

                            i += 1;
                        }

                        if (iCnt == 0)
                            MessageBox.Show(this, "No " + sFileExt + " files are exported!" + Environment.NewLine
                                + "Or it have been exported before.");
                        else if (iCnt > 0)
                            MessageBox.Show(this, "Export to " + sFileExt + " files successfully.");
                        else
                            MessageBox.Show(this, "Export to " + sFileExt + " files unsuccessful.");
                    }

                    //lastly clear the Xml Info.
                    stcXmlInfo = new staMain._stcExportInfo();
                    initExportInfo(1);
                }
                
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + ".." + ex.StackTrace.ToString());
                MessageBox.Show("Export error! " + ex.Message);
            }
            finally
            {
                bCompleted = true;
                Cursor.Current = Cursors.Default;
                cbxStatus.Enabled = true;
                btnExport.Enabled = true;
                btnCancel.Enabled = true;
                btnOpenFolder.Enabled = true;
                staMain.sessionRestart();
            }    
        }

        private void loadDocSetStatus()
        {
            try
            {
                cbxStatus.Items.Clear();
                //cbxStatus.Items.Add("All");
                if (staMain.stcProjCfg.IndexEnable.Trim().ToUpper() == "N")
                    cbxStatus.Items.Add("Scanned");
                else
                    cbxStatus.Items.Add("Indexed");

                if (staMain.stcProjCfg.VerifyEnable.Trim().ToUpper() == "Y")
                    cbxStatus.Items.Add("Verified");
            }
            catch (Exception)
            {
            }
        }

        static void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            Task tMerge = null;
            try
            {
                mGlobal.Write2Log("ID.." + Thread.CurrentThread.ManagedThreadId.ToString());

                if (staMain.stcProjCfg.ExpMerge.Trim().ToUpper() == "Y" && staMain.stcProjCfg.ExpSearchPDF.Trim().ToUpper() == "Y")
                    tMerge = Task.Factory.StartNew(() => savePDFByDocTypeMergeFromTiff(sSet));
                else if (staMain.stcProjCfg.ExpMerge.Trim().ToUpper() == "N" && staMain.stcProjCfg.ExpSearchPDF.Trim().ToUpper() == "Y")
                    tMerge = Task.Factory.StartNew(() => savePDFByDocTypeManyFromTiff(sSet));
                else if (staMain.stcProjCfg.ExpMerge.Trim().ToUpper() == "Y")
                    tMerge = Task.Factory.StartNew(() => savePDFByDocTypeMerge(sSet));
                else
                    tMerge = Task.Factory.StartNew(() => savePDFByDocTypeMany(sSet));

                Task.WaitAll(tMerge); //this will wait till tMerge get finished.
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Work.." + ex.Message);
                mGlobal.Write2Log("Work.." + ex.StackTrace.ToString());
                MessageBox.Show("Error! " + ex.Message, "Message");
            }
            finally
            {
                if (tMerge != null) tMerge.Dispose();
                bDone = true;
            }
        }

        static void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                mGlobal.Write2Log("Completed, tid " + Thread.CurrentThread.ManagedThreadId.ToString());
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Completed.." + ex.Message);
            }
        }

        private static void initExportInfo(int pTot)
        {
            try
            {
                if (pTot == 0) pTot = 1;

                stcXmlInfo.BatchCodes = new List<string>(pTot);
                stcXmlInfo.Setnum = new List<string>(pTot);
                stcXmlInfo.DocTypes = new List<string>(pTot);
                stcXmlInfo.TotPages = new List<int>(pTot);
                stcXmlInfo.ExpFiles = new List<string>(pTot);
                //this.stcXmlInfo.DocNames = new string[pTot];

                if (pTot == 1)
                {
                    stcXmlInfo.BatchCodes.Add("");
                    stcXmlInfo.Setnum.Add("");
                    stcXmlInfo.DocTypes.Add("");
                    stcXmlInfo.TotPages.Add(0);
                    stcXmlInfo.ExpFiles.Add("");
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
            }
        }

        private void saveTIFFByDocTypeMerge(string sSet)
        {
            string[] sFiles = null;
            List<string> sDocTypes = null;
            byte[] byTiff = null;
            string sStatus = "'V','TF'";
            string TIFFFilename = "";
            string sBatchCode = "";
            string sDocType = "";
            string sSetNum = "";
            string sName = "";
            int iFldId = 0;
            string sValidPath = "";
            try
            {
                if (sSet.ToLower() == "all")
                    sStatus = "'I','V','TF'";
                else if (sSet.ToLower() == "scanned")
                    sStatus = "'S','TF'";
                else if (sSet.ToLower() == "indexed")
                    sStatus = "'I','TF'";

                initExportInfo(1);

                if (sSet.ToLower() == "scanned")
                    sFiles = getDocuSetScanImageFiles(_currScanProj, _appNum, sStatus);
                else
                    sFiles = getDocuSetIndexImageFiles(_currScanProj, _appNum, sStatus);
                if (sFiles != null)
                {
                    initExportInfo(sFiles.Length);

                    Tif2TiffMerge.clsMergeTiff oMerge = new Tif2TiffMerge.clsMergeTiff(MDIMain.strIronPdfLic);
                    sDocTypes = new List<string>();
                    string sFmt = "", sValue = ""; int i = 0, iHdr = 0; string sDir = ""; FileInfo fi = null;

                    int iF = 0; int iType = 0; int iTotType = 1; int iTotPages = 0; int iSet = 0;
                    while (iType < sFiles.Length)
                    {
                        sBatchCode = sFiles[iType].Split('|').GetValue(0).ToString().Trim();
                        sDocType = sFiles[iType].Split('|').GetValue(1).ToString().Trim();
                        sSetNum = sFiles[iType].Split('|').GetValue(3).ToString().Trim();

                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                        {
                            sSetNum = sFiles[iType].Split('|').GetValue(5).ToString().Trim().PadLeft(sSetNumFmt.Length, '0');
                        }

                        //stcXmlInfo.BatchCodes[iType] = sBatchCode;
                        //stcXmlInfo.DocTypes[iType] = sDocType;
                        //stcXmlInfo.Setnum[iType] = sSetNum;

                        if (iType + 1 < sFiles.Length)
                        {
                            if (staMain.stcProjCfg.NoSeparator.Trim() == "2" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim()
                                && sDocType.Trim() == sFiles[iType + 1].Split('|').GetValue(1).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else if (staMain.stcProjCfg.NoSeparator.Trim() == "1" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim()
                                && sSetNum.Trim() == sFiles[iType + 1].Split('|').GetValue(3).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else if (staMain.stcProjCfg.NoSeparator.Trim() == "0" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else //Different doc type group exists.
                            {
                                iTotPages = 0;

                                while (iF < sFiles.Length)
                                {
                                    iTotPages += 1;
                                    fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                                    if (fi != null)
                                    {
                                        if (fi.Exists)
                                        {
                                            if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                            {
                                                if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                                {
                                                    if (!ReadFromImageBarcode(fi.FullName))
                                                    {
                                                        sDocTypes.Add(fi.FullName);
                                                    }
                                                }
                                                else
                                                    sDocTypes.Add(fi.FullName);
                                            }
                                        }
                                        else
                                            mGlobal.Write2Log("Image file does not exists." + fi.FullName);

                                    }
                                    else
                                        mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                                    iF += 1;
                                    if (iTotPages == iTotType)
                                        break;
                                }

                                iF = iType + 1;

                                if (sDocTypes != null)
                                {
                                    //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                                    if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                                    {
                                        sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                        iHdr = sFmt.Split('\\').Length;
                                        i = 0;
                                        while (i < iHdr)
                                        {
                                            iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                            sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            if (i == 0)
                                                sValidPath = sValue;
                                            else
                                                sValidPath += @"\" + sValue;

                                            i += 1;
                                        }

                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                    }
                                    else
                                    {
                                        sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");
                                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, 1.ToString(sSetNumFmt), sBatchCode, _docDefId, iFldId); //Take the first set index value.
                                            //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sValidPath + "_" + sName + ".tiff");
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                        }
                                        else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sValidPath + "_" + sName + "_" + sDocType + ".tiff");
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".tiff");
                                        }
                                        else
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sValidPath + "_" + sName + "_" + sDocType + ".tiff");
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".tiff");
                                        }
                                    }

                                    sDir = mGlobal.getDirectoriesFromPath(TIFFFilename);
                                    if (!Directory.Exists(sDir))
                                        Directory.CreateDirectory(sDir);

                                    if (sFiles.Length == 1)
                                    {
                                        stcXmlInfo.ExpFiles[0] = TIFFFilename;
                                        stcXmlInfo.BatchCodes[0] = sBatchCode;
                                        stcXmlInfo.Setnum[0] = sSetNum;
                                        stcXmlInfo.DocTypes[0] = sDocType;
                                    }
                                    else
                                    {
                                        stcXmlInfo.ExpFiles.Add(TIFFFilename);

                                        stcXmlInfo.BatchCodes.Add(sBatchCode);
                                        stcXmlInfo.Setnum.Add(sSetNum);
                                        stcXmlInfo.DocTypes.Add(sDocType);
                                    }
                                    //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                                    //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                                    //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                                    iSet += 1;

                                    byTiff = oMerge.tif2tiff(sDocTypes.ToArray());

                                    if (oMerge.getPageCount(byTiff) > 0)
                                        File.WriteAllBytes(TIFFFilename, byTiff);
                                    else
                                        mGlobal.Write2Log("TIFF file is zero pages! The file is not saved. " + TIFFFilename);

                                    stcXmlInfo.TotPages.Add(oMerge.getPageCount(byTiff));

                                    sDocTypes = new List<string>();
                                    iTotType = 1;
                                }

                            }
                        }
                        else //Different doc type group exists.
                        {
                            iTotPages = 0;

                            while (iF < sFiles.Length)
                            {
                                iTotPages += 1;
                                fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                                if (fi != null)
                                {
                                    if (fi.Exists)
                                    {
                                        if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                        {
                                            if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                            {
                                                if (!ReadFromImageBarcode(fi.FullName))
                                                {
                                                    sDocTypes.Add(fi.FullName);
                                                }
                                            }
                                            else
                                                sDocTypes.Add(fi.FullName);
                                        }
                                    }
                                    else
                                        mGlobal.Write2Log("Image file does not exists." + fi.FullName);
                                }
                                else
                                    mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                                iF += 1;
                                if (iTotPages == iTotType)
                                    break;
                            }

                            iF = iType + 1;

                            if (sDocTypes != null)
                            {
                                //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                                if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                                {
                                    sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                    iHdr = sFmt.Split('\\').Length;
                                    i = 0;
                                    while (i < iHdr)
                                    {
                                        iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                        sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        if (i == 0)
                                            sValidPath = sValue;
                                        else
                                            sValidPath += @"\" + sValue;

                                        i += 1;
                                    }

                                    sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                    sName = mGlobal.replaceValidChars(sName, "_");
                                    TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                }
                                else
                                {
                                    sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");
                                    if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, 1.ToString(sSetNumFmt), sBatchCode, _docDefId, iFldId); //Take the first set index value.
                                        //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sValidPath + "_" + sName + ".tiff");
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                    }
                                    else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sValidPath + "_" + sName + "_" + sDocType + ".tiff");
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".tiff");
                                    }
                                    else
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sValidPath + "_" + sName + "_" + sDocType + ".tiff");
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".tiff");
                                    }
                                }

                                sDir = mGlobal.getDirectoriesFromPath(TIFFFilename);
                                if (!Directory.Exists(sDir))
                                    Directory.CreateDirectory(sDir);

                                if (sFiles.Length == 1)
                                {
                                    stcXmlInfo.ExpFiles[0] = TIFFFilename;
                                    stcXmlInfo.BatchCodes[0] = sBatchCode;
                                    stcXmlInfo.Setnum[0] = sSetNum;
                                    stcXmlInfo.DocTypes[0] = sDocType;
                                }
                                else
                                {
                                    stcXmlInfo.ExpFiles.Add(TIFFFilename);

                                    stcXmlInfo.BatchCodes.Add(sBatchCode);
                                    stcXmlInfo.Setnum.Add(sSetNum);
                                    stcXmlInfo.DocTypes.Add(sDocType);
                                }
                                //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                                //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                                //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                                iSet += 1;

                                byTiff = oMerge.tif2tiff(sDocTypes.ToArray());

                                if (oMerge.getPageCount(byTiff) > 0)
                                    File.WriteAllBytes(TIFFFilename, byTiff);
                                else
                                    mGlobal.Write2Log("TIFF file is zero pages! The file is not saved. " + TIFFFilename);

                                stcXmlInfo.TotPages.Add(oMerge.getPageCount(byTiff));

                                sDocTypes = new List<string>();
                                iTotType = 1;
                            }
                        }

                        iType += 1;
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        private void saveTIFFByDocTypeMany(string sSet)
        {
            string[] sFiles = null;
            string sStatus = "'V','TF'";
            string TIFFFilename = "";
            string sBatchCode = "";
            string sDocType = "";
            string sSetNum = "";
            string sName = "";
            int iFldId = 0;
            string sValidPath = "";
            try
            {
                if (sSet.ToLower() == "all")
                    sStatus = "'I','V','TF'";
                else if (sSet.ToLower() == "scanned")
                    sStatus = "'S','TF'";
                else if (sSet.ToLower() == "indexed")
                    sStatus = "'I','TF'";

                initExportInfo(1);

                if (sSet.ToLower() == "scanned")
                    sFiles = getDocuSetScanImageFiles(_currScanProj, _appNum, sStatus);
                else
                    sFiles = getDocuSetIndexImageFiles(_currScanProj, _appNum, sStatus);
                if (sFiles != null)
                {
                    initExportInfo(sFiles.Length);
                    string sFmt = "", sValue = ""; int i = 0, iHdr = 0; string sDir = ""; FileInfo fi = null;

                    int iF = 0; int iType = 0; int iTotPages = 1; bool isExist = true;
                    while (iType < sFiles.Length)
                    {
                        sBatchCode = sFiles[iType].Split('|').GetValue(0).ToString().Trim();
                        sDocType = sFiles[iType].Split('|').GetValue(1).ToString().Trim();
                        sSetNum = sFiles[iType].Split('|').GetValue(3).ToString().Trim();

                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                        {
                            sSetNum = sFiles[iType].Split('|').GetValue(5).ToString().Trim().PadLeft(sSetNumFmt.Length, '0');
                        }

                        //stcXmlInfo.BatchCodes[iType] = sBatchCode;
                        //stcXmlInfo.DocTypes[iType] = sDocType;
                        //stcXmlInfo.Setnum[iType] = sSetNum;

                        //iTotPages += 1;
                        fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());
                        
                        if (fi != null)
                        {
                            if (fi.Exists)
                            {
                                if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                {
                                    isExist = true;

                                    if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                    {
                                        if (ReadFromImageBarcode(fi.FullName))
                                        {
                                            isExist = false;
                                        }
                                    }
                                }
                                else
                                    isExist = false;
                            }
                            else
                                mGlobal.Write2Log("Image file does not exists." + fi.FullName);

                        }
                        else
                            mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                        if (isExist)
                        {
                            //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iF + 1);
                            sName = mGlobal.replaceValidChars(sName, "_");
                            if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                            {
                                sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                iHdr = sFmt.Split('\\').Length;
                                i = 0;
                                while (i < iHdr)
                                {
                                    iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                    sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                    if (i == 0)
                                        sValidPath = sValue;
                                    else
                                        sValidPath += @"\" + sValue;

                                    i += 1;
                                }

                                TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                            }
                            else
                            {
                                sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");
                                if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                {
                                    sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iF + 1);
                                    sName = mGlobal.replaceValidChars(sName, "_");
                                    TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                }
                                else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                {
                                    TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".tiff");
                                }
                                else
                                    TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".tiff");
                            }

                            sDir = mGlobal.getDirectoriesFromPath(TIFFFilename);
                            if (!Directory.Exists(sDir))
                                Directory.CreateDirectory(sDir);

                            if (sFiles.Length == 1)
                            {
                                stcXmlInfo.ExpFiles[0] = TIFFFilename;
                                stcXmlInfo.BatchCodes[0] = sBatchCode;
                                stcXmlInfo.Setnum[0] = sSetNum;
                                stcXmlInfo.DocTypes[0] = sDocType;
                            }
                            else
                            {
                                stcXmlInfo.ExpFiles.Add(TIFFFilename);

                                stcXmlInfo.BatchCodes.Add(sBatchCode);
                                stcXmlInfo.Setnum.Add(sSetNum);
                                stcXmlInfo.DocTypes.Add(sDocType);
                            }

                            //if (stcXmlInfo.ExpFiles.Count > 1)
                            //{
                            //    if (stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 2] != sBatchCode)
                            //        iTotPages = 1;
                            //}
                            //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                            //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                            //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;
                            stcXmlInfo.TotPages.Add(iTotPages);

                            File.Copy(fi.FullName, TIFFFilename, true);                            
                        }

                        iType += 1;
                        iF = iType;
                    }
                    
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        private void savePDFByDocType1(string sSet)
        {
            //PdfSharp.Pdf.PdfDocument docPDF = null;
            Syncfusion.Pdf.PdfDocument docPDF = null;
            string[] sFiles = null;
            string sStatus = "'V'";
            string PDFFilename = "";
            string sBatchCode = "";
            string sDocType = "";
            string sSetNum = "";
            string sName = "";
            int iFldId = 0;

            try
            {
                if (sSet.ToLower() == "all")
                    sStatus = "'I','V'";
                else if (sSet.ToLower() == "scanned")
                    sStatus = "'S'";
                else if (sSet.ToLower() == "indexed")
                    sStatus = "'I'";

                initExportInfo(1);

                if (sSet.ToLower() == "scanned")
                    sFiles = getDocuSetScanImageFiles(_currScanProj, _appNum, sStatus);
                else
                    sFiles = getDocuSetIndexImageFiles(_currScanProj, _appNum, sStatus);
                if (sFiles != null)
                {
                    initExportInfo(sFiles.Length);

                    Tiff2PdfConverter.clsTiffSplitter oTiff = new Tiff2PdfConverter.clsTiffSplitter(MDIMain.strIronLic);
                    //docPDF = new PdfSharp.Pdf.PdfDocument();
                    docPDF = new Syncfusion.Pdf.PdfDocument();

                    int iF = 0; int iType = 0; int iTotType = 1; int iTotPages = 0;
                    while (iType < sFiles.Length)
                    {
                        sBatchCode = sFiles[iType].Split('|').GetValue(0).ToString().Trim();
                        sDocType = sFiles[iType].Split('|').GetValue(1).ToString().Trim();
                        sSetNum = sFiles[iType].Split('|').GetValue(3).ToString().Trim();

                        //stcXmlInfo.BatchCodes[iType] = sBatchCode;
                        //stcXmlInfo.DocTypes[iType] = sDocType;
                        //stcXmlInfo.Setnum[iType] = sSetNum;

                        if (iType + 1 < sFiles.Length)
                        {
                            if (sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim()
                                && sDocType.Trim() == sFiles[iType + 1].Split('|').GetValue(1).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else //Different doc type group exists.
                            {
                                iTotPages = 0;

                                while (iF < sFiles.Length)
                                {
                                    iTotPages += 1;
                                    FileInfo fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                                    if (fi != null)
                                    {
                                        if (fi.Exists)
                                        {
                                            if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                            {
                                                //docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);                                                
                                                docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);                                                
                                            }
                                        }
                                        else
                                            mGlobal.Write2Log("Image file does not exists." + fi.FullName);
                                    }
                                    else
                                        mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                                    iF += 1;
                                    if (iTotPages == iTotType)
                                        break;
                                }

                                iF = iType + 1;

                                if (docPDF != null)
                                {
                                    iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                                    sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                    PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sBatchCode + "_" + sName + "_" + sDocType + ".pdf");

                                    string sDir = mGlobal.getDirectoriesFromPath(PDFFilename);
                                    if (!Directory.Exists(sDir))
                                        Directory.CreateDirectory(sDir);

                                    if (sFiles.Length == 1)
                                        stcXmlInfo.ExpFiles[0] = PDFFilename;
                                    else
                                        stcXmlInfo.ExpFiles.Add(PDFFilename);

                                    stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                                    stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                                    stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                                    //if (docPDF.PageCount > 0)
                                    //    docPDF.Save(PDFFilename);
                                    //else
                                    //    mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);

                                    //stcXmlInfo.TotPages.Add(docPDF.PageCount);
                                    if (docPDF.Pages.Count > 0)
                                    {
                                        docPDF.Save(PDFFilename);                                       
                                    }
                                    else
                                        mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);

                                    stcXmlInfo.TotPages.Add(docPDF.Pages.Count);

                                    docPDF.Close();
                                    docPDF.Dispose();

                                    //docPDF = new PdfSharp.Pdf.PdfDocument();
                                    docPDF = new Syncfusion.Pdf.PdfDocument();
                                    iTotType = 1;
                                }
                            }
                        }
                        else //Different doc type group exists.
                        {
                            iTotPages = 0;

                            while (iF < sFiles.Length)
                            {
                                iTotPages += 1;
                                FileInfo fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                                if (fi != null)
                                {
                                    if (fi.Exists)
                                    {
                                        if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                        {
                                            //docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);
                                            docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);
                                        }
                                    }
                                    else
                                        mGlobal.Write2Log("Image file does not exists." + fi.FullName);
                                }
                                else
                                    mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                                iF += 1;
                                if (iTotPages == iTotType)
                                    break;
                            }

                            iF = iType + 1;

                            if (docPDF != null)
                            {
                                iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                                sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sBatchCode + "_" + sName + "_" + sDocType + ".pdf");

                                string sDir = mGlobal.getDirectoriesFromPath(PDFFilename);
                                if (!Directory.Exists(sDir))
                                    Directory.CreateDirectory(sDir);

                                if (sFiles.Length == 1)
                                    stcXmlInfo.ExpFiles[0] = PDFFilename;
                                else
                                    stcXmlInfo.ExpFiles.Add(PDFFilename);

                                stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                                stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                                stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                                //if (docPDF.PageCount > 0)
                                //    docPDF.Save(PDFFilename);
                                //else
                                //    mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);
                                if (docPDF.Pages.Count > 0)
                                {
                                    docPDF.Save(PDFFilename);
                                    //oTiff.OCR2PdfSearchable(PDFFilename);
                                }
                                else
                                    mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);

                                //stcXmlInfo.TotPages.Add(docPDF.PageCount);
                                stcXmlInfo.TotPages.Add(docPDF.Pages.Count);

                                docPDF.Close();
                                docPDF.Dispose();

                                //docPDF = new PdfSharp.Pdf.PdfDocument();
                                docPDF = new Syncfusion.Pdf.PdfDocument();
                                iTotType = 1;
                            }

                        }

                        iType += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(PDFFilename + ".." + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
                if (docPDF != null)
                {
                    docPDF.Close();
                    docPDF.Dispose();
                }
            }
        }

        private static void savePDFByDocTypeMany(string sSet)
        {
            //PdfSharp.Pdf.PdfDocument docPDF = null;
            Syncfusion.Pdf.PdfDocument docPDF = null;
            //IronPdf.PdfDocument docPDF = null;
            string[] sFiles = null;
            string sStatus = "'V','TF'";
            string PDFFilename = "";
            string sBatchCode = "";
            string sDocType = "";
            string sSetNum = "";
            string sName = "";
            int iFldId = 0;
            string sValidPath = "";
            try
            {
                if (sSet.ToLower() == "all")
                    sStatus = "'S','I','V','TF'";
                else if (sSet.ToLower() == "scanned")
                    sStatus = "'S','TF'";
                else if (sSet.ToLower() == "indexed")
                    sStatus = "'I','TF'";

                initExportInfo(1);
                //Task<IronPdf.PdfDocument> t1;
                Task<Syncfusion.Pdf.PdfDocument> t1;
                Task st1;

                if (sSet.ToLower() == "scanned")
                    sFiles = getDocuSetScanImageFiles(_currScanProj, _appNum, sStatus);
                else
                    sFiles = getDocuSetIndexImageFiles(_currScanProj, _appNum, sStatus);
                if (sFiles != null)
                {
                    initExportInfo(sFiles.Length);

                    IronPdf.License.LicenseKey = mGlobal.GetAppCfg("IRPDFLicKey");
                    Tiff2PdfConverter.clsTiffSplitter oTiff = new Tiff2PdfConverter.clsTiffSplitter(MDIMain.strIronLic);
                    //docPDF = new PdfSharp.Pdf.PdfDocument();
                    docPDF = new Syncfusion.Pdf.PdfDocument();
                    docPDF.EnableMemoryOptimization = true;
                    docPDF.Compression = Syncfusion.Pdf.PdfCompressionLevel.Best;

                    string sFmt = "", sValue = ""; int i = 0, iHdr = 0; string sDir = ""; FileInfo fi = null;

                    int iF = 0; int iType = 0; int iTotPages = 0;
                    while (iType < sFiles.Length)
                    {
                        sBatchCode = sFiles[iType].Split('|').GetValue(0).ToString().Trim();
                        sDocType = sFiles[iType].Split('|').GetValue(1).ToString().Trim();
                        sSetNum = sFiles[iType].Split('|').GetValue(3).ToString().Trim();

                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                        {
                            sSetNum = sFiles[iType].Split('|').GetValue(5).ToString().Trim().PadLeft(sSetNumFmt.Length, '0');
                        }

                        //stcXmlInfo.BatchCodes[iType] = sBatchCode;
                        //stcXmlInfo.DocTypes[iType] = sDocType;
                        //stcXmlInfo.Setnum[iType] = sSetNum;

                        iTotPages += 1;
                        fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                        if (fi != null)
                        {
                            if (fi.Exists)
                            {
                                if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                {
                                    if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                    {
                                        if (ReadFromImageBarcode(fi.FullName))
                                        {
                                            docPDF = null;
                                        }
                                        else
                                        {
                                            //docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);
                                            //t1 = Task<IronPdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                            t1 = Task<Syncfusion.Pdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                            lTasks.Add(t1);
                                            Task.WaitAll(t1); //this will wait till t1 get finished.
                                            docPDF = t1.Result; //get the result of tiff2PDF method like this.

                                            if (t1 != null) t1.Dispose();
                                        }
                                    }
                                    else
                                    {
                                        //docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);
                                        //t1 = Task<IronPdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                        t1 = Task<Syncfusion.Pdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                        lTasks.Add(t1);
                                        Task.WaitAll(t1); //this will wait till t1 get finished.
                                        docPDF = t1.Result; //get the result of tiff2PDF method like this.

                                        if (t1 != null) t1.Dispose();
                                    }
                                }
                            }
                            else
                                mGlobal.Write2Log("Image file does not exists." + fi.FullName);
                        }
                        else
                            mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                        if (docPDF != null)
                        {
                            //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iF + 1);
                            sName = mGlobal.replaceValidChars(sName, "_");

                            if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                            {
                                sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                iHdr = sFmt.Split('\\').Length;
                                i = 0;
                                while (i < iHdr)
                                {
                                    iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                    sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                    if (i == 0)
                                        sValidPath = sValue;
                                    else
                                        sValidPath += @"\" + sValue;

                                    i += 1;
                                }

                                PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".pdf");
                            }
                            else
                            {
                                sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");

                                if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                {
                                    sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iF + 1);
                                    sName = mGlobal.replaceValidChars(sName, "_");
                                    PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".pdf");
                                }
                                else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                {
                                    PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".pdf");
                                }
                                else
                                    PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".pdf");
                            }

                            sDir = mGlobal.getDirectoriesFromPath(PDFFilename);
                            if (!Directory.Exists(sDir))
                                Directory.CreateDirectory(sDir);

                            if (sFiles.Length == 1)
                            {
                                stcXmlInfo.ExpFiles[0] = PDFFilename;
                                stcXmlInfo.BatchCodes[0] = sBatchCode;
                                stcXmlInfo.Setnum[0] = sSetNum;
                                stcXmlInfo.DocTypes[0] = sDocType;
                            }
                            else
                            {
                                stcXmlInfo.ExpFiles.Add(PDFFilename);

                                stcXmlInfo.BatchCodes.Add(sBatchCode);
                                stcXmlInfo.Setnum.Add(sSetNum);
                                stcXmlInfo.DocTypes.Add(sDocType);
                            }

                            //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                            //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                            //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                            //if (docPDF.PageCount > 0)
                            //    docPDF.Save(PDFFilename);
                            //else
                            //    mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);

                            //stcXmlInfo.TotPages.Add(docPDF.PageCount);
                            if (docPDF.Pages.Count > 0)
                            {
                                docPDF.Save(PDFFilename);
                                //docPDF.SaveAs(PDFFilename);

                                if (staMain.stcProjCfg.ExpSearchPDF.Trim().ToUpper() == "Y")
                                {
                                    //frmExport oExp = new frmExport();
                                    //if (!oExp.IsHandleCreated)
                                    //    oExp.CreateHandle();

                                    //while (!oExp.IsHandleCreated)
                                    //    System.Threading.Thread.Sleep(100);

                                    //OCRPdfThread ocrDelegate = delegate ()
                                    //{
                                    //    oTiff.OCR2PdfSearchable(PDFFilename);
                                    //};
                                    //oExp.Invoke(ocrDelegate);
                                    st1 = Task.Factory.StartNew(() => oTiff.OCR2PdfSearchable(PDFFilename));
                                    lSearchTasks.Add(st1);
                                    if (st1.IsCompleted) st1.Dispose();
                                }
                            }
                            else
                                mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);

                            stcXmlInfo.TotPages.Add(docPDF.Pages.Count);

                            docPDF.Close();
                            docPDF.Dispose();

                            //docPDF = new PdfSharp.Pdf.PdfDocument();
                            docPDF = new Syncfusion.Pdf.PdfDocument();
                            docPDF.EnableMemoryOptimization = true;
                            docPDF.Compression = Syncfusion.Pdf.PdfCompressionLevel.Best;
                            //docPDF = null; //uncomment this line for IronPDF.
                        }
                        else
                        {
                            docPDF = new Syncfusion.Pdf.PdfDocument(); //Init.
                            docPDF.EnableMemoryOptimization = true;
                            docPDF.Compression = Syncfusion.Pdf.PdfCompressionLevel.Best;
                            //docPDF = null; //uncomment this line for IronPDF.
                        }

                        iType += 1;
                        iF = iType;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
                mGlobal.Write2Log(PDFFilename + ".." + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
                if (docPDF != null)
                {
                    docPDF.Close();
                    docPDF.Dispose();
                }
            }
        }

        private static void savePDFByDocTypeMerge(string sSet)
        {
            //PdfSharp.Pdf.PdfDocument docPDF = null;
            Syncfusion.Pdf.PdfDocument docPDF = null;
            //IronPdf.PdfDocument docPDF = null;
            string[] sFiles = null;
            string sStatus = "'V','TF'";
            string PDFFilename = "";
            string sBatchCode = "";
            string sDocType = "";
            string sSetNum = "";
            string sName = "";
            int iFldId = 0;
            string sValidPath = "";
            try
            {
                if (sSet.ToLower() == "all")
                    sStatus = "'S','I','V','TF'";
                else if (sSet.ToLower() == "scanned")
                    sStatus = "'S','TF'";
                else if (sSet.ToLower() == "indexed")
                    sStatus = "'I','TF'";

                initExportInfo(1);
                //Task<IronPdf.PdfDocument> t1;
                Task<Syncfusion.Pdf.PdfDocument> t1;
                Task st1;

                if (sSet.ToLower() == "scanned")
                    sFiles = getDocuSetScanImageFiles(_currScanProj, _appNum, sStatus);
                else
                    sFiles = getDocuSetIndexImageFiles(_currScanProj, _appNum, sStatus);
                if (sFiles != null)
                {
                    initExportInfo(sFiles.Length);

                    IronPdf.License.LicenseKey = mGlobal.GetAppCfg("IRPDFLicKey");
                    Tiff2PdfConverter.clsTiffSplitter oTiff = new Tiff2PdfConverter.clsTiffSplitter(MDIMain.strIronLic);
                    //docPDF = new PdfSharp.Pdf.PdfDocument();
                    docPDF = new Syncfusion.Pdf.PdfDocument();
                    docPDF.EnableMemoryOptimization = true;
                    docPDF.Compression = Syncfusion.Pdf.PdfCompressionLevel.Best;
                    string sFmt = "", sValue = ""; int i = 0, iHdr = 0; string sDir = ""; FileInfo fi = null;

                    int iF = 0; int iType = 0; int iTotType = 1; int iTotPages = 0;
                    while (iType < sFiles.Length)
                    {
                        sBatchCode = sFiles[iType].Split('|').GetValue(0).ToString().Trim();
                        sDocType = sFiles[iType].Split('|').GetValue(1).ToString().Trim();
                        sSetNum = sFiles[iType].Split('|').GetValue(3).ToString().Trim();

                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                        {
                            sSetNum = sFiles[iType].Split('|').GetValue(5).ToString().Trim().PadLeft(sSetNumFmt.Length, '0');
                        }

                        //stcXmlInfo.BatchCodes[iType] = sBatchCode;
                        //stcXmlInfo.DocTypes[iType] = sDocType;
                        //stcXmlInfo.Setnum[iType] = sSetNum;

                        if (iType + 1 < sFiles.Length)
                        {
                            if (staMain.stcProjCfg.NoSeparator.Trim() == "2" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim()
                                && sDocType.Trim() == sFiles[iType + 1].Split('|').GetValue(1).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else if (staMain.stcProjCfg.NoSeparator.Trim() == "1" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim()
                                && sSetNum.Trim() == sFiles[iType + 1].Split('|').GetValue(3).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else if (staMain.stcProjCfg.NoSeparator.Trim() == "0" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else //Different doc type group exists.
                            {
                                iTotPages = 0;

                                while (iF < sFiles.Length)
                                {
                                    iTotPages += 1;
                                    fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                                    if (fi != null)
                                    {
                                        if (fi.Exists)
                                        {
                                            if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                            {
                                                if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                                {
                                                    if (!ReadFromImageBarcode(fi.FullName))
                                                    {
                                                        //docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);
                                                        //t1 = Task<IronPdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                                        t1 = Task<Syncfusion.Pdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                                        lTasks.Add(t1);
                                                        Task.WaitAll(t1); //this will wait till t1 get finished.
                                                        //docPDF = t1.Result; //get the result of tiff2PDF method like this.
                                                        docPDF = t1.Result;

                                                        if (t1 != null) t1.Dispose();
                                                    }
                                                }
                                                else
                                                {
                                                    //docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);
                                                    //t1 = Task<IronPdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                                    t1 = Task<Syncfusion.Pdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                                    lTasks.Add(t1);                                                    
                                                    Task.WaitAll(t1); //this will wait till t1 get finished.
                                                    //docPDF = t1.Result; //get the result of tiff2PDF method like this.
                                                    docPDF = t1.Result;

                                                    if (t1 != null) t1.Dispose();
                                                }                                                
                                            }
                                        }
                                        else
                                            mGlobal.Write2Log("Image file does not exists." + fi.FullName);
                                    }
                                    else
                                        mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());                                    

                                    iF += 1;
                                    if (iTotPages == iTotType)
                                        break;
                                }

                                iF = iType + 1;

                                if (docPDF != null)
                                {
                                    //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                                    if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                                    {
                                        sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                        iHdr = sFmt.Split('\\').Length;
                                        i = 0;
                                        while (i < iHdr)
                                        {
                                            iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                            sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            if (i == 0)
                                                sValidPath = sValue;
                                            else
                                                sValidPath += @"\" + sValue;

                                            i += 1;
                                        }

                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".pdf");
                                    }
                                    else
                                    {
                                        sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");
                                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, 1.ToString(sSetNumFmt), sBatchCode, _docDefId, iFldId);
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".pdf");
                                        }
                                        else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".pdf");
                                        }
                                        else
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".pdf");
                                        }
                                    }

                                    sDir = mGlobal.getDirectoriesFromPath(PDFFilename);
                                    if (!Directory.Exists(sDir))
                                        Directory.CreateDirectory(sDir);

                                    if (sFiles.Length == 1)
                                    {
                                        stcXmlInfo.ExpFiles[0] = PDFFilename;
                                        stcXmlInfo.BatchCodes[0] = sBatchCode;
                                        stcXmlInfo.Setnum[0] = sSetNum;
                                        stcXmlInfo.DocTypes[0] = sDocType;
                                    }
                                    else
                                    {
                                        stcXmlInfo.ExpFiles.Add(PDFFilename);

                                        stcXmlInfo.BatchCodes.Add(sBatchCode);
                                        stcXmlInfo.Setnum.Add(sSetNum);
                                        stcXmlInfo.DocTypes.Add(sDocType);
                                    }
                                    //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                                    //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                                    //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                                    //if (docPDF.PageCount > 0)
                                    //    docPDF.Save(PDFFilename);
                                    //else
                                    //    mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);

                                    //stcXmlInfo.TotPages.Add(docPDF.PageCount);
                                    if (docPDF.Pages.Count > 0)
                                    {
                                        docPDF.Save(PDFFilename);
                                        //docPDF.SaveAs(PDFFilename);

                                        if (staMain.stcProjCfg.ExpSearchPDF.Trim().ToUpper() == "Y")
                                        {
                                            //frmExport oExp = new frmExport();                                            
                                            //if (!oExp.IsHandleCreated)
                                            //    oExp.CreateHandle();

                                            //while (!oExp.IsHandleCreated)
                                            //    System.Threading.Thread.Sleep(100);

                                            //OCRPdfThread ocrDelegate = delegate ()
                                            //{
                                            //    oTiff.OCR2PdfSearchable(PDFFilename);
                                            //};
                                            //oExp.Invoke(ocrDelegate); //Sync
                                            //oExp.BeginInvoke(ocrDelegate); //Async
                                            st1 = Task.Factory.StartNew(() => oTiff.OCR2PdfSearchable(PDFFilename));
                                            lSearchTasks.Add(st1);
                                            if (st1.IsCompleted) st1.Dispose();
                                        }
                                    }
                                    else
                                        mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);

                                    stcXmlInfo.TotPages.Add(docPDF.Pages.Count);

                                    docPDF.Close();
                                    docPDF.Dispose();

                                    //docPDF = new PdfSharp.Pdf.PdfDocument();
                                    docPDF = new Syncfusion.Pdf.PdfDocument();
                                    docPDF.EnableMemoryOptimization = true;
                                    docPDF.Compression = Syncfusion.Pdf.PdfCompressionLevel.Best;
                                    //docPDF = null; //uncomment this line for IronPDF.

                                    iTotType = 1;
                                }
                                else
                                {
                                    //docPDF = null; //uncomment this line for IronPDF.
                                    docPDF = new Syncfusion.Pdf.PdfDocument();
                                    docPDF.EnableMemoryOptimization = true;
                                    docPDF.Compression = Syncfusion.Pdf.PdfCompressionLevel.Best;
                                }
                            }
                        }
                        else //Different doc type group exists.
                        {
                            iTotPages = 0;

                            while (iF < sFiles.Length)
                            {
                                iTotPages += 1;
                                fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                                if (fi != null)
                                {
                                    if (fi.Exists)
                                    {
                                        if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                        {
                                            if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                            {
                                                if (!ReadFromImageBarcode(fi.FullName))
                                                {
                                                    //docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);
                                                    //t1 = Task<IronPdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                                    t1 = Task<Syncfusion.Pdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                                    lTasks.Add(t1);
                                                    Task.WaitAll(t1); //this will wait till t1 get finished.
                                                    docPDF = t1.Result; //get the result of tiff2PDF method like this.

                                                    if (t1 != null) t1.Dispose();
                                                }
                                            }
                                            else
                                            {
                                                //docPDF = oTiff.tiff2PDF(fi.FullName, docPDF);
                                                t1 = Task<Syncfusion.Pdf.PdfDocument>.Factory.StartNew(() => oTiff.tiff2PDF(fi.FullName, docPDF));
                                                lTasks.Add(t1);
                                                Task.WaitAll(t1); //this will wait till t1 get finished.
                                                docPDF = t1.Result; //get the result of tiff2PDF method like this.

                                                if (t1 != null) t1.Dispose();
                                            }
                                        }
                                    }
                                    else
                                        mGlobal.Write2Log("Image file does not exists." + fi.FullName);
                                }
                                else
                                    mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                                iF += 1;
                                if (iTotPages == iTotType)
                                    break;
                            }

                            iF = iType + 1;

                            if (docPDF != null)
                            {
                                //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                                if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                                {
                                    sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                    iHdr = sFmt.Split('\\').Length;
                                    i = 0;
                                    while (i < iHdr)
                                    {
                                        iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                        sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        if (i == 0)
                                            sValidPath = sValue;
                                        else
                                            sValidPath += @"\" + sValue;

                                        i += 1;
                                    }

                                    sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                    sName = mGlobal.replaceValidChars(sName, "_");
                                    PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".pdf");
                                }
                                else
                                {
                                    sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");
                                    if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, 1.ToString(sSetNumFmt), sBatchCode, _docDefId, iFldId);
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".pdf");
                                    }
                                    else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".pdf");
                                    }
                                    else
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        PDFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".pdf");
                                    }
                                }

                                sDir = mGlobal.getDirectoriesFromPath(PDFFilename);
                                if (!Directory.Exists(sDir))
                                    Directory.CreateDirectory(sDir);

                                if (sFiles.Length == 1)
                                {
                                    stcXmlInfo.ExpFiles[0] = PDFFilename;
                                    stcXmlInfo.BatchCodes[0] = sBatchCode;
                                    stcXmlInfo.Setnum[0] = sSetNum;
                                    stcXmlInfo.DocTypes[0] = sDocType;
                                }
                                else
                                {
                                    stcXmlInfo.ExpFiles.Add(PDFFilename);

                                    stcXmlInfo.BatchCodes.Add(sBatchCode);
                                    stcXmlInfo.Setnum.Add(sSetNum);
                                    stcXmlInfo.DocTypes.Add(sDocType);
                                }
                                //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                                //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                                //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                                //if (docPDF.PageCount > 0)
                                //    docPDF.Save(PDFFilename);
                                //else
                                //    mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);
                                if (docPDF.Pages.Count > 0)
                                {
                                    docPDF.Save(PDFFilename);
                                    //docPDF.SaveAs(PDFFilename);

                                    if (staMain.stcProjCfg.ExpSearchPDF.Trim().ToUpper() == "Y")
                                    {
                                        //frmExport oExp = new frmExport();
                                        //if (!oExp.IsHandleCreated)
                                        //    oExp.CreateHandle();

                                        //while (!oExp.IsHandleCreated)
                                        //    System.Threading.Thread.Sleep(100);

                                        //OCRPdfThread ocrDelegate = delegate ()
                                        //{
                                        //    oTiff.OCR2PdfSearchable(PDFFilename);
                                        //};
                                        //oExp.Invoke(ocrDelegate);
                                        //oExp.BeginInvoke(ocrDelegate);
                                        st1 = Task.Factory.StartNew(() => oTiff.OCR2PdfSearchable(PDFFilename));
                                        lSearchTasks.Add(st1);
                                        if (st1.IsCompleted) st1.Dispose();
                                    }
                                }
                                else
                                    mGlobal.Write2Log("PDF file is zero pages! The file is not saved. " + PDFFilename);

                                //stcXmlInfo.TotPages.Add(docPDF.PageCount);
                                stcXmlInfo.TotPages.Add(docPDF.Pages.Count);

                                docPDF.Close();
                                docPDF.Dispose();

                                //docPDF = new PdfSharp.Pdf.PdfDocument();
                                docPDF = new Syncfusion.Pdf.PdfDocument();
                                docPDF.EnableMemoryOptimization = true;
                                docPDF.Compression = Syncfusion.Pdf.PdfCompressionLevel.Best;
                                //docPDF = null; //uncomment this line for IronPDF.

                                iTotType = 1;
                            }
                            else
                            {
                                //docPDF = null; //uncomment this line for IronPDF.
                                docPDF = new Syncfusion.Pdf.PdfDocument();
                                docPDF.EnableMemoryOptimization = true;
                                docPDF.Compression = Syncfusion.Pdf.PdfCompressionLevel.Best;
                            }
                        }

                        iType += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(PDFFilename + ".." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
                if (docPDF != null)
                {
                    docPDF.Close();
                    docPDF.Dispose();
                }
            }
        }

        private static void savePDFByDocTypeManyFromTiff(string sSet)
        {
            string[] sFiles = null;
            //byte[] byTiff = null;
            string sStatus = "'V','TF'";
            string TIFFFilename = "";
            string PDFFilename = "";
            string sBatchCode = "";
            string sDocType = "";
            string sSetNum = "";
            string sName = "";
            int iFldId = 0;
            string sValidPath = "";
            try
            {
                if (sSet.ToLower() == "all")
                    sStatus = "'S','I','V','TF'";
                else if (sSet.ToLower() == "scanned")
                    sStatus = "'S','TF'";
                else if (sSet.ToLower() == "indexed")
                    sStatus = "'I','TF'";

                initExportInfo(1);
                Task t1;

                if (sSet.ToLower() == "scanned")
                    sFiles = getDocuSetScanImageFiles(_currScanProj, _appNum, sStatus);
                else
                    sFiles = getDocuSetIndexImageFiles(_currScanProj, _appNum, sStatus);
                if (sFiles != null)
                {
                    initExportInfo(sFiles.Length);
                    Tif2TiffMerge.clsMergeTiff oMerge = new Tif2TiffMerge.clsMergeTiff(MDIMain.strIronPdfLic);
                    string sFmt = "", sValue = ""; int i = 0, iHdr = 0; string sDir = ""; FileInfo oFI = null; FileInfo fi = null;

                    int iF = 0; int iType = 0; int iTotPages = 1; bool isExist = true;
                    while (iType < sFiles.Length)
                    {
                        sBatchCode = sFiles[iType].Split('|').GetValue(0).ToString().Trim();
                        sDocType = sFiles[iType].Split('|').GetValue(1).ToString().Trim();
                        sSetNum = sFiles[iType].Split('|').GetValue(3).ToString().Trim();
                        isExist = false;

                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                        {
                            sSetNum = sFiles[iType].Split('|').GetValue(5).ToString().Trim().PadLeft(sSetNumFmt.Length, '0');
                        }
                        //stcXmlInfo.BatchCodes[iType] = sBatchCode;
                        //stcXmlInfo.DocTypes[iType] = sDocType;
                        //stcXmlInfo.Setnum[iType] = sSetNum;

                        //iTotPages += 1;
                        fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                        if (fi != null)
                        {
                            if (fi.Exists)
                            {
                                if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                {
                                    isExist = true;

                                    if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                    {
                                        if (ReadFromImageBarcode(fi.FullName))
                                        {
                                            isExist = false;
                                        }
                                    }
                                }
                                else
                                    isExist = false;
                            }
                            else
                                mGlobal.Write2Log("Image file does not exists." + fi.FullName);
                        }
                        else
                            mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                        if (isExist)
                        {
                            //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iF + 1);
                            sName = mGlobal.replaceValidChars(sName, "_");
                            if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                            {
                                sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                iHdr = sFmt.Split('\\').Length;
                                i = 0;
                                while (i < iHdr)
                                {
                                    iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                    sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                    if (i == 0)
                                        sValidPath = sValue;
                                    else
                                        sValidPath += @"\" + sValue;

                                    i += 1;
                                }

                                TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                            }
                            else
                            {
                                sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");
                                if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                {
                                    sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iF + 1);
                                    sName = mGlobal.replaceValidChars(sName, "_");
                                    TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                }
                                else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                {
                                    TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".tiff");
                                }
                                else
                                    TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".tiff");
                            }

                            PDFFilename = TIFFFilename.Replace(".tiff", ".pdf").Replace(".TIFF", ".pdf");

                            //sDir = mGlobal.getDirectoriesFromPath(TIFFFilename);
                            sDir = mGlobal.getDirectoriesFromPath(PDFFilename);
                            if (!Directory.Exists(sDir))
                                Directory.CreateDirectory(sDir);

                            if (sFiles.Length == 1)
                            {
                                //stcXmlInfo.ExpFiles[0] = TIFFFilename;
                                stcXmlInfo.ExpFiles[0] = PDFFilename;
                                stcXmlInfo.BatchCodes[0] = sBatchCode;
                                stcXmlInfo.Setnum[0] = sSetNum;
                                stcXmlInfo.DocTypes[0] = sDocType;
                            }
                            else
                            {
                                //stcXmlInfo.ExpFiles.Add(TIFFFilename);
                                stcXmlInfo.ExpFiles.Add(PDFFilename);

                                stcXmlInfo.BatchCodes.Add(sBatchCode);
                                stcXmlInfo.Setnum.Add(sSetNum);
                                stcXmlInfo.DocTypes.Add(sDocType);
                            }

                            //if (stcXmlInfo.ExpFiles.Count > 1)
                            //{
                            //    if (stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 2] != sBatchCode)
                            //        iTotPages = 1;
                            //}
                            //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                            //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                            //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;
                            stcXmlInfo.TotPages.Add(iTotPages);

                            File.Copy(fi.FullName, TIFFFilename, true);                            

                            if (staMain.stcProjCfg.ExpSearchPDF.Trim().ToUpper() == "Y")
                            {
                                oFI = new FileInfo(TIFFFilename);
                                if (oFI.Exists)
                                {
                                    oFI = new FileInfo(PDFFilename);
                                    if (!oFI.Exists)
                                    {
                                        //byTiff = File.ReadAllBytes(TIFFFilename);
                                        //oMerge.OCR2PdfSearchableFromTiff(PDFFilename, TIFFFilename);
                                        //t1 = Task.Factory.StartNew(() => oMerge.OCR2PdfSearchableFromTiff(PDFFilename, TIFFFilename, true));
                                        //t1 = Task.Factory.StartNew(() => oMerge.OCR2PdfSearchableFromTiff(PDFFilename, byTiff));
                                        oMerge.OCR2PdfSearchableFromTiff(PDFFilename, TIFFFilename, true);
                                        //lSearchTasks.Add(t1);
                                        //Task.WaitAll(t1); //this will wait till t1 get finished.
                                        //if (t1.IsCompleted) t1.Dispose();
                                    }
                                }
                            }
                        }

                        iType += 1;
                        iF = iType;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("PDF many.." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
                //throw new Exception(ex.Message);
            }
        }

        private static void savePDFByDocTypeMergeFromTiff(string sSet)
        {
            string[] sFiles = null;
            List<string> sDocTypes = null;
            byte[] byTiff = null;
            string sStatus = "'V','TF'";
            string TIFFFilename = "";
            string PDFFilename = "";
            string sBatchCode = "";
            string sDocType = "";
            string sSetNum = "";
            string sName = "";
            int iFldId = 0;
            string sValidPath = "";
            try
            {
                if (sSet.ToLower() == "all")
                    sStatus = "'S','I','V','TF'";
                else if (sSet.ToLower() == "scanned")
                    sStatus = "'S','TF'";
                else if (sSet.ToLower() == "indexed")
                    sStatus = "'I','TF'";

                initExportInfo(1);
                Task<byte[]> t1; Task t2;

                if (sSet.ToLower() == "scanned")
                    sFiles = getDocuSetScanImageFiles(_currScanProj, _appNum, sStatus);
                else
                    sFiles = getDocuSetIndexImageFiles(_currScanProj, _appNum, sStatus);
                if (sFiles != null)
                {
                    initExportInfo(sFiles.Length);

                    Tif2TiffMerge.clsMergeTiff oMerge = new Tif2TiffMerge.clsMergeTiff(MDIMain.strIronPdfLic);
                    sDocTypes = new List<string>();
                    string sFmt = "", sValue = ""; int i = 0, iHdr = 0; string sDir = ""; FileInfo oFI = null; FileInfo fi = null;

                    int iF = 0; int iType = 0; int iTotType = 1; int iTotPages = 0; int iSet = 0;
                    while (iType < sFiles.Length)
                    {
                        sBatchCode = sFiles[iType].Split('|').GetValue(0).ToString().Trim();
                        sDocType = sFiles[iType].Split('|').GetValue(1).ToString().Trim();
                        sSetNum = sFiles[iType].Split('|').GetValue(3).ToString().Trim();

                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                        {
                            sSetNum = sFiles[iType].Split('|').GetValue(5).ToString().Trim().PadLeft(sSetNumFmt.Length, '0');
                        }

                        //stcXmlInfo.BatchCodes[iType] = sBatchCode;
                        //stcXmlInfo.DocTypes[iType] = sDocType;
                        //stcXmlInfo.Setnum[iType] = sSetNum;

                        if (iType + 1 < sFiles.Length)
                        {
                            if (staMain.stcProjCfg.NoSeparator.Trim() == "2" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim()
                                && sDocType.Trim() == sFiles[iType + 1].Split('|').GetValue(1).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else if (staMain.stcProjCfg.NoSeparator.Trim() == "1" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim()
                                && sSetNum.Trim() == sFiles[iType + 1].Split('|').GetValue(3).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else if (staMain.stcProjCfg.NoSeparator.Trim() == "0" && sBatchCode.Trim() == sFiles[iType + 1].Split('|').GetValue(0).ToString().Trim())
                            {
                                iTotType += 1;
                            }
                            else //Different doc type group exists.
                            {
                                iTotPages = 0;

                                while (iF < sFiles.Length)
                                {
                                    iTotPages += 1;
                                    fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                                    if (fi != null)
                                    {
                                        if (fi.Exists)
                                        {
                                            if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                            {
                                                if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                                {
                                                    if (!ReadFromImageBarcode(fi.FullName))
                                                    {
                                                        sDocTypes.Add(fi.FullName);
                                                    }
                                                }
                                                else
                                                    sDocTypes.Add(fi.FullName);
                                            }
                                        }
                                        else
                                            mGlobal.Write2Log("Image file does not exists." + fi.FullName);

                                    }
                                    else
                                        mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                                    iF += 1;
                                    if (iTotPages == iTotType)
                                        break;
                                }

                                iF = iType + 1;

                                if (sDocTypes != null)
                                {
                                    //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                                    if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                                    {
                                        sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                        iHdr = sFmt.Split('\\').Length;
                                        i = 0;
                                        while (i < iHdr)
                                        {
                                            iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                            sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            if (i == 0)
                                                sValidPath = sValue;
                                            else
                                                sValidPath += @"\" + sValue;

                                            i += 1;
                                        }

                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                    }
                                    else
                                    {
                                        sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");
                                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, 1.ToString(sSetNumFmt), sBatchCode, _docDefId, iFldId); //Take the first set index value.
                                            //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sValidPath + "_" + sName + ".tiff");
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                        }
                                        else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sValidPath + "_" + sName + "_" + sDocType + ".tiff");
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".tiff");
                                        }
                                        else
                                        {
                                            //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                            //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sValidPath + "_" + sName + "_" + sDocType + ".tiff");
                                            sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                            sName = mGlobal.replaceValidChars(sName, "_");
                                            TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".tiff");
                                        }
                                    }
                                    PDFFilename = TIFFFilename.Replace(".tiff", ".pdf").Replace(".TIFF", ".pdf");

                                    //sDir = mGlobal.getDirectoriesFromPath(TIFFFilename);
                                    sDir = mGlobal.getDirectoriesFromPath(PDFFilename);
                                    if (!Directory.Exists(sDir))
                                        Directory.CreateDirectory(sDir);

                                    if (sFiles.Length == 1)
                                    {
                                        //stcXmlInfo.ExpFiles[0] = TIFFFilename;
                                        stcXmlInfo.ExpFiles[0] = PDFFilename;
                                        stcXmlInfo.BatchCodes[0] = sBatchCode;
                                        stcXmlInfo.Setnum[0] = sSetNum;
                                        stcXmlInfo.DocTypes[0] = sDocType;
                                    }
                                    else
                                    {
                                        //stcXmlInfo.ExpFiles.Add(TIFFFilename);
                                        stcXmlInfo.ExpFiles.Add(PDFFilename);

                                        stcXmlInfo.BatchCodes.Add(sBatchCode);
                                        stcXmlInfo.Setnum.Add(sSetNum);
                                        stcXmlInfo.DocTypes.Add(sDocType);
                                    }
                                    //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                                    //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                                    //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                                    iSet += 1;

                                    //byTiff = oMerge.tif2tiff(sDocTypes.ToArray());
                                    t1 = Task<byte[]>.Factory.StartNew(() => oMerge.tif2tiff(sDocTypes.ToArray()));
                                    lTasks.Add(t1);
                                    Task.WaitAll(t1); //this will wait till t1 get finished.
                                    byTiff = t1.Result;
                                    if (oMerge.getPageCount(byTiff) > 0)
                                        File.WriteAllBytes(TIFFFilename, byTiff);
                                    else
                                        mGlobal.Write2Log("TIFF file is zero pages! The file is not saved. " + TIFFFilename);

                                    stcXmlInfo.TotPages.Add(oMerge.getPageCount(byTiff));

                                    if (staMain.stcProjCfg.ExpSearchPDF.Trim().ToUpper() == "Y")
                                    {
                                        oFI = new FileInfo(TIFFFilename);
                                        if (oFI.Exists)
                                        {
                                            PDFFilename = oFI.FullName.Replace(".tiff", ".pdf").Replace(".TIFF", ".pdf");
                                            //oMerge.OCR2PdfSearchableFromTiff(PDFFilename, TIFFFilename);
                                            //t2 = Task.Factory.StartNew(() => oMerge.OCR2PdfSearchableFromTiff(PDFFilename, TIFFFilename, true));
                                            t2 = Task.Factory.StartNew(() => oMerge.OCR2PdfSearchableFromTiff(TIFFFilename, PDFFilename, byTiff));
                                            lSearchTasks.Add(t2);
                                            //Task.WaitAll(t2); //this will wait till t1 get finished.
                                        }
                                    }

                                    sDocTypes = new List<string>();
                                    iTotType = 1;
                                }
                            }
                        }
                        else //Different doc type group exists.
                        {
                            iTotPages = 0;

                            while (iF < sFiles.Length)
                            {
                                iTotPages += 1;
                                fi = new FileInfo(sFiles[iF].Split('|').GetValue(2).ToString());

                                if (fi != null)
                                {
                                    if (fi.Exists)
                                    {
                                        if (fi.Extension.ToLower() == ".tif" || fi.Extension.ToLower() == ".tiff")
                                        {
                                            if (staMain.stcProjCfg.ExpRemvBarSep.ToUpper() == "Y")
                                            {
                                                if (!ReadFromImageBarcode(fi.FullName))
                                                {
                                                    sDocTypes.Add(fi.FullName);
                                                }
                                            }
                                            else
                                                sDocTypes.Add(fi.FullName);
                                        }
                                    }
                                    else
                                        mGlobal.Write2Log("Image file does not exists." + fi.FullName);
                                }
                                else
                                    mGlobal.Write2Log("File not found..!" + sFiles[iF].Split('|').GetValue(2).ToString());

                                iF += 1;
                                if (iTotPages == iTotType)
                                    break;
                            }

                            iF = iType + 1;

                            if (sDocTypes != null)
                            {
                                //iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, staMain.stcProjCfg.ExpFileFmt.Trim());
                                if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                                {
                                    sFmt = staMain.stcProjCfg.ExpSubDirFmt.Trim();
                                    iHdr = sFmt.Split('\\').Length;
                                    i = 0;
                                    while (i < iHdr)
                                    {
                                        iFldId = getKeyIndexIdDB(_currScanProj, _appNum, _docDefId, sFmt.Split('\\').GetValue(i).ToString().Replace("[", "").Replace("]", ""));
                                        sValue = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        if (i == 0)
                                            sValidPath = sValue;
                                        else
                                            sValidPath += @"\" + sValue;

                                        i += 1;
                                    }

                                    sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                    sName = mGlobal.replaceValidChars(sName, "_");
                                    TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                }
                                else
                                {
                                    sValidPath = mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\" + mGlobal.replaceValidChars(sBatchCode, "_");
                                    if (staMain.stcProjCfg.NoSeparator.Trim() == "0") //Set number = Page image sequence in TIndexFieldValue table.
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, 1.ToString(sSetNumFmt), sBatchCode, _docDefId, iFldId); //Take the first set index value.
                                        //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sValidPath + "_" + sName + ".tiff");
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, 1.ToString(sSetNumFmt), sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, sName + ".tiff");
                                    }
                                    else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sValidPath + "_" + sName + "_" + sDocType + ".tiff");
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, "Set" + sSetNum, sName + ".tiff");
                                    }
                                    else
                                    {
                                        //sName = getKeyIndexValueDB(_currScanProj, _appNum, sSetNum, sBatchCode, _docDefId, iFldId);
                                        //TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, "Set" + sSetNum, sValidPath + "_" + sName + "_" + sDocType + ".tiff");
                                        sName = oDF.getExpFilenameFmt(_currScanProj, _appNum, sBatchCode, sSetNum, sDocType, iType);
                                        sName = mGlobal.replaceValidChars(sName, "_");
                                        TIFFFilename = System.IO.Path.Combine(staMain.stcProjCfg.ExpDir, sValidPath, mGlobal.addDirSep("Set" + sSetNum) + sDocType, sName + ".tiff");
                                    }
                                }
                                PDFFilename = TIFFFilename.Replace(".tiff", ".pdf").Replace(".TIFF", ".pdf");

                                //sDir = mGlobal.getDirectoriesFromPath(TIFFFilename);
                                sDir = mGlobal.getDirectoriesFromPath(PDFFilename);
                                if (!Directory.Exists(sDir))
                                    Directory.CreateDirectory(sDir);

                                if (sFiles.Length == 1)
                                {
                                    //stcXmlInfo.ExpFiles[0] = TIFFFilename;
                                    stcXmlInfo.ExpFiles[0] = PDFFilename;
                                    stcXmlInfo.BatchCodes[0] = sBatchCode;
                                    stcXmlInfo.Setnum[0] = sSetNum;
                                    stcXmlInfo.DocTypes[0] = sDocType;
                                }
                                else
                                {
                                    //stcXmlInfo.ExpFiles.Add(TIFFFilename);
                                    stcXmlInfo.ExpFiles.Add(PDFFilename);

                                    stcXmlInfo.BatchCodes.Add(sBatchCode);
                                    stcXmlInfo.Setnum.Add(sSetNum);
                                    stcXmlInfo.DocTypes.Add(sDocType);
                                }
                                //stcXmlInfo.BatchCodes[stcXmlInfo.ExpFiles.Count - 1] = sBatchCode;
                                //stcXmlInfo.Setnum[stcXmlInfo.ExpFiles.Count - 1] = sSetNum;
                                //stcXmlInfo.DocTypes[stcXmlInfo.ExpFiles.Count - 1] = sDocType;

                                iSet += 1;

                                //byTiff = oMerge.tif2tiff(sDocTypes.ToArray());
                                t1 = Task<byte[]>.Factory.StartNew(() => oMerge.tif2tiff(sDocTypes.ToArray()));
                                lTasks.Add(t1);
                                Task.WaitAll(t1); //this will wait till t1 get finished.
                                byTiff = t1.Result;
                                if (oMerge.getPageCount(byTiff) > 0)
                                    File.WriteAllBytes(TIFFFilename, byTiff);
                                else
                                    mGlobal.Write2Log("TIFF file is zero pages! The file is not saved. " + TIFFFilename);

                                stcXmlInfo.TotPages.Add(oMerge.getPageCount(byTiff));

                                if (staMain.stcProjCfg.ExpSearchPDF.Trim().ToUpper() == "Y")
                                {
                                    oFI = new FileInfo(TIFFFilename);
                                    if (oFI.Exists)
                                    {
                                        PDFFilename = oFI.FullName.Replace(".tiff", ".pdf").Replace(".TIFF", ".pdf");
                                        //oMerge.OCR2PdfSearchableFromTiff(PDFFilename, TIFFFilename);
                                        //t2 = Task.Factory.StartNew(() => oMerge.OCR2PdfSearchableFromTiff(PDFFilename, TIFFFilename, true));
                                        t2 = Task.Factory.StartNew(() => oMerge.OCR2PdfSearchableFromTiff(TIFFFilename, PDFFilename, byTiff));
                                        lSearchTasks.Add(t2);
                                        //Task.WaitAll(t2); //this will wait till t1 get finished.                                        
                                    }
                                }

                                sDocTypes = new List<string>();
                                iTotType = 1;
                            }
                        }

                        iType += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        public static string[] getDocuSetScanImageFiles(string pScanProj, string pAppNum, string pStatus)
        {
            string[] bRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT batchcode,doctype,docimage,setnum,docdefid,imageseq ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND scanstatus IN (" + pStatus.Trim() + ") ";
                sSQL += "AND exported='N' ";
                sSQL += "GROUP BY batchcode,doctype,docimage,setnum,docdefid,imageseq ";
                sSQL += "ORDER BY batchcode,setnum,docdefid,doctype,imageseq,docimage ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);
                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        int i = 0;
                        bRet = new string[drs.Count];

                        while (i < drs.Count)
                        {
                            bRet[i] = drs[i][0].ToString() + "|" + drs[i][1].ToString() + "|" + drs[i][2].ToString() + "|"
                                + drs[i][3].ToString() + "|" + drs[i][4].ToString() + "|" + drs[i][5].ToString();

                            i += 1;
                        }
                    }
                }
                else
                    bRet = null;
            }
            catch (Exception ex)
            {
                bRet = null;
            }

            return bRet;
        }

        public static string[] getDocuSetIndexImageFiles(string pScanProj, string pAppNum, string pStatus)
        {
            string[] bRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT batchcode,doctype,docimage,setnum,docdefid,imageseq ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus IN (" + pStatus.Trim() + ") ";
                sSQL += "AND exported='N' ";
                sSQL += "GROUP BY batchcode,doctype,docimage,setnum,docdefid,imageseq ";
                sSQL += "ORDER BY batchcode,setnum,docdefid,doctype,imageseq,docimage ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);
                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        int i = 0;
                        bRet = new string[drs.Count];

                        while (i < drs.Count)
                        {
                            bRet[i] = drs[i][0].ToString() + "|" + drs[i][1].ToString() + "|" + drs[i][2].ToString() + "|" 
                                + drs[i][3].ToString() + "|" + drs[i][4].ToString() + "|" + drs[i][5].ToString();

                            i += 1;
                        }
                    }                    
                }
                else
                    bRet = null;
            }
            catch (Exception ex)
            {
                bRet = null;
            }

            return bRet;
        }

        private void frmExport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bCompleted && !MDIMain.bSessTimeout)
            {
                e.Cancel = false;
                MessageBox.Show("Export process is not completed! Export cannot close.");
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(staMain.stcProjCfg.ExpDir)) Directory.CreateDirectory(staMain.stcProjCfg.ExpDir);

            System.Diagnostics.Process.Start("explorer.exe", staMain.stcProjCfg.ExpDir);
        }

        private bool generateText()
        {
            bool bRet = true;
            string sDelim = ",";
            string[] sFiles = null;
            int iTotHdrFld = 0;
            int iTotFld = 4;
            string sDir = "";
            string sTextFilename = "";
            string[] sSetNums = null;
            string[] sExpFiles = null;

            try
            {
                if (stcXmlInfo.BatchCodes.Count >= 1 && stcXmlInfo.BatchCodes[0].Trim() != string.Empty)
                {
                    if (staMain.stcProjCfg.ExpSubDirFmt.Trim() == string.Empty)
                    {
                        sDir = mGlobal.addDirSep(staMain.stcProjCfg.ExpDir) + mGlobal.replaceValidChars(_currScanProj, "_") + @"\" + _appNum + @"\";

                        if (!Directory.Exists(sDir))
                            Directory.CreateDirectory(sDir);

                        sTextFilename = sDir + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                    }
                    else if (staMain.stcProjCfg.ExpSubDirFmt.Trim() != string.Empty)
                    {                       
                        sDir = staMain.stcProjCfg.ExpDir;

                        if (!Directory.Exists(sDir))
                            Directory.CreateDirectory(sDir);

                        sTextFilename = System.IO.Path.Combine(sDir, mGlobal.replaceValidChars(_currScanProj, "_") + _appNum + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv");
                    }

                    string sNextLines = "";
                    string sLines = "";

                    if (staMain.stcProjCfg.ExpMerge.Trim().ToUpper() == "Y")
                    {
                        sFiles = stcXmlInfo.ExpFiles.Distinct().ToArray();
                        sSetNums = stcXmlInfo.Setnum.Distinct().ToArray();
                        if (sFiles.Length != sSetNums.Length) //group by batch files
                            sSetNums = stcXmlInfo.Setnum.ToArray();
                    }
                    else
                    {
                        sFiles = stcXmlInfo.ExpFiles.ToArray();
                        sSetNums = stcXmlInfo.Setnum.ToArray();
                    }
                    sExpFiles = sFiles;

                    List<string> lVal = new List<string>();
                    DataRowCollection drs = null;
                    drs = getExportIndexHeaderDB(_currScanProj, _appNum, _docDefId);

                    int iSet = 0; int iFld = 0; int i = 0; int j = 0; 
                    while (i <= sFiles.Length)
                    {
                        //Header
                        if (i == 0)
                        {
                            
                            if (drs != null)
                            {
                                j = 0;
                                while (j < drs.Count)
                                {
                                    sNextLines += drs[j]["fldname"].ToString().Trim().Replace(" ","_").Replace("#","_Number").Replace(".","") + sDelim;
                                    j += 1;
                                }
                            }
                            
                            while (iFld < iTotFld)
                            {
                                switch (iFld)
                                {
                                    case 0: sNextLines += "Set_Name" + sDelim; break;
                                    case 1: sNextLines += "Document_Type" + sDelim; break;
                                    case 2: sNextLines += "Total_Page" + sDelim; break;
                                    case 3: sNextLines += "File"; break;
                                    default: sNextLines += ""; break;
                                }

                                iFld += 1;
                            }

                            iTotHdrFld = j;

                            sLines = sNextLines + System.Environment.NewLine;
                            sNextLines = "";
                        }
                        else
                        {
                            if (staMain.stcProjCfg.NoSeparator.Trim() == "0" && staMain.stcProjCfg.ExpMerge.Trim() == "Y") //Take the first Set number for index value.
                            {
                                lVal = getExportIndexValueDB(_currScanProj, _appNum, 1.ToString(sSetNumFmt), stcXmlInfo.BatchCodes[iSet].ToString(), _docDefId, iTotHdrFld);
                            }
                            else
                            {
                                if (sSetNums[iSet] != null)
                                {
                                    //lVal = getExportIndexValueDB(_currScanProj, _appNum, stcXmlInfo.Setnum[iSet].ToString(), stcXmlInfo.BatchCodes[iSet].ToString(), _docDefId, iTotHdrFld);
                                    lVal = getExportIndexValueDB(_currScanProj, _appNum, sSetNums[iSet].ToString(), stcXmlInfo.BatchCodes[iSet].ToString(), _docDefId, iTotHdrFld);
                                }
                                else //Skip
                                    lVal = null;
                            }

                            if (lVal != null)
                            {
                                j = 0;
                                if (lVal.Count > 0)
                                {
                                    while (j < lVal.Count)
                                    {
                                        //sNextLines += drs[j]["fldvalue"].ToString().Trim() + sDelim;
                                        sNextLines += mGlobal.sanitizeValue(lVal[j].Trim(), "") + sDelim;
                                        j += 1;
                                    }
                                }
                                else
                                {
                                    while (j < iTotHdrFld)
                                    {
                                        sNextLines += "" + sDelim;
                                        j += 1;
                                    }
                                }                                
                            }
                            else
                            {
                                j = 0;
                                while (j < iTotHdrFld)
                                {
                                    sNextLines += "" + sDelim;
                                    j += 1;
                                }
                            }

                            if (sSetNums[iSet] != null)
                            {
                                while (iFld < iTotFld)
                                {
                                    switch (iFld)
                                    {
                                        case 0: sNextLines += sSetNums[iSet] + sDelim; break; //sNextLines += stcXmlInfo.Setnum[iSet] + sDelim; break;
                                        case 1: sNextLines += stcXmlInfo.DocTypes[iSet] + sDelim; break;
                                        case 2: sNextLines += stcXmlInfo.TotPages[iSet] + sDelim; break;
                                        case 3: sNextLines += sExpFiles[iSet]; break; //stcXmlInfo.ExpFiles[iSet]; break;
                                        default: sNextLines += ""; break;
                                    }

                                    iFld += 1;
                                }

                                if (iSet != (sFiles.Length - 1))
                                    sNextLines = sNextLines + System.Environment.NewLine;
                            }

                            iSet += 1;
                        }

                        sLines += sNextLines;
                        sNextLines = "";

                        iFld = 0;
                        i += 1;
                    }

                    drs = null;
                    lVal = null;

                    mGlobal.writeText(sLines, sTextFilename);
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
                bRet = false;
            }
            return bRet;
        }

        private int getExportBatchDB(string pBatchCode, string pBatchStatusIn)
        {
            int ret = 0;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(exportcnt),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn.Trim() + ") ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    ret = Convert.ToInt32(dr[0]);
                }
            }
            catch (Exception ex)
            {
                ret = 0;
            }
            return ret;
        }

        private bool updateExportBatchDB(string pBatchCode, string pBatchStatusIn, int pExportCnt, string pNewStatus = "")
        {
            bool ret = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "SET exportcnt=" + pExportCnt + " ";
                if (pNewStatus.Trim() != "")
                    sSQL += ",batchstatus='" + pNewStatus + "' ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn.Trim() + ") ";

                ret = mGlobal.objDB.UpdateRows(sSQL);

            }
            catch (Exception ex)
            {
                ret = false;
            }
            return ret;
        }

        public static DataRowCollection getExportIndexHeaderDB(string pPrjCode, string pAppNum, string pDocDefId)
        {
            DataRowCollection ret = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT rowid,fldname FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuDefiSet a ";
                sSQL += "WHERE a.scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY a.flddispseq ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                ret = drs;
                drs = null;
            }
            catch (Exception ex)
            {
                ret = null;
                throw new Exception(ex.Message);
            }
            return ret;
        }

        //private static void initExportIndex(int pTot)
        //{
        //    if (pTot == 0) pTot = 1;

        //    stcIndexValues.FieldId = new int[pTot];
        //    stcIndexValues.FieldName = new string[pTot];
        //    stcIndexValues.FieldValue = new string[pTot];
        //}

        //public static staMain._stcIndexFieldValues getExportIndexValue(string pPrjCode, string pAppNum, string pBatchCode, string pDocDefId)
        //{
        //    DataRowCollection drs = null;
        //    try
        //    {
                
        //        initExportIndex(1);
        //        drs = getExportIndexValueDB(pPrjCode, pAppNum, pBatchCode, pDocDefId);
                
        //        if (drs != null)
        //        {
        //            initExportIndex(drs.Count);
        //            int i = 0;
        //            while (i < drs.Count)
        //            {
        //                stcIndexValues.FieldId[i] = Convert.ToInt32(drs[i][0].ToString());
        //                stcIndexValues.FieldName[i] = drs[i][1].ToString();
        //                stcIndexValues.FieldValue[i] = drs[i][2].ToString();

        //                i += 1;
        //            }
        //        }

        //        drs = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //    return stcIndexValues;
        //}

        public static List<string> getExportIndexValueDB(string pPrjCode, string pAppNum, string pSetNum, string pBatchCode, string pDocDefId, int iHdrCnt)
        {
            List<string> lval = new List<string>();
            string sSQL = "";   
            try
            {   
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP " + iHdrCnt + " rowid FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuDefiSet ";
			
                sSQL += "WHERE scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
																				  
																					  
                sSQL += "AND docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
																					   
                sSQL += "ORDER BY flddispseq,modifieddate desc,createddate desc ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                int i = 0;
                while (i < drs.Count)
                {
                    sSQL = "SELECT fldvalue FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue a ";
                    sSQL += "WHERE a.scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND a.sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND a.setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND a.docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                    sSQL += "AND a.fieldid='" + drs[i][0].ToString() + "' "; //rowid

                    DataRowCollection drs1 = mGlobal.objDB.ReturnRows(sSQL);

                    if (drs1 != null)
                    {
                        if (drs1.Count > 0)
                        {
                            lval.Add(drs1[0][0].ToString().Trim());
                        }
                        else
                            lval.Add("");
                    }

                    drs1 = null;

                    i += 1;
                }

                //sSQL = "SELECT TOP " + iHdrCnt + " fldvalue FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue a ";
                //sSQL += "INNER JOIN " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuDefiSet b ";
                //sSQL += "ON a.fieldid=b.rowid ";
                //sSQL += "WHERE a.scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                //sSQL += "AND a.sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND a.setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                //sSQL += "AND a.docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                //sSQL += "GROUP BY fldvalue,b.flddispseq,b.modifieddate,b.createddate ";
                //sSQL += "ORDER BY b.flddispseq,b.modifieddate desc,b.createddate desc ";

                drs = null;
            }
            catch (Exception ex)
            {
                lval = null;
                throw new Exception(ex.Message);
            }
            return lval;
        }

        public static int getKeyIndexIdDB(string pPrjCode, string pAppNum, string pDocDefId, string sKeyFieldName)
        {
            int ret = 0;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 a.rowid FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuDefiSet a ";               
                sSQL += "WHERE a.scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.fldname='" + sKeyFieldName + "' ";
                sSQL += "ORDER BY a.rowid desc ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null) ret = Convert.ToInt32(dr[0].ToString());

                dr = null;
            }
            catch (Exception ex)
            {
                ret = 0;
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static string getKeyIndexValueDB(string pPrjCode, string pAppNum, string pSetNum, string pBatchCode, string pDocDefId, int sKeyFieldId)
        {
            string ret = "";
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 a.rowid,fldname,fldvalue,flddispseq FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue a ";
                sSQL += "INNER JOIN " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuDefiSet b ";
                sSQL += "ON a.fieldid=b.rowid ";
                sSQL += "WHERE a.scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                sSQL += "AND fieldid=" + sKeyFieldId + " ";
                sSQL += "ORDER BY batchcode,b.flddispseq ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null) ret = dr[2].ToString();

                dr = null;
            }
            catch (Exception ex)
            {
                ret = "";
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static string getKeyIndexValueDB(string pPrjCode, string pAppNum, string pBatchCode, string pDocDefId, int sKeyFieldId)
        {
            string ret = "";
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 a.rowid,fldname,fldvalue,flddispseq FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue a ";
                sSQL += "INNER JOIN " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuDefiSet b ";
                sSQL += "ON a.fieldid=b.rowid ";
                sSQL += "WHERE a.scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                sSQL += "AND fieldid=" + sKeyFieldId + " ";
                sSQL += "ORDER BY batchcode,a.setnum,b.flddispseq ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null) ret = dr[2].ToString();

                dr = null;
            }
            catch (Exception ex)
            {
                ret = "";
                throw new Exception(ex.Message);
            }
            return ret;
        }

        private static bool ReadFromImageBarcode(string pFilename)
        {
            bool isDocSep = false;
            //MessageBox.Show(oImgCore.ImageBuffer.HowManyImagesInBuffer.ToString());
            //ShowSelectedImageArea();
            Bitmap bmp = null;
            try
            {
                //updateRuntimeSettingsWithUISetting();

                //bmp = new Bitmap(pFilename);
                //TextResult[] textResults = oBarcodeRdr.DecodeBitmap(bmp, "");
                //if (textResults.Length > 0)
                //{
                //    isDocSep = true;
                //}

                //textResults = null;
                ////this.ShowResultOnImage(bmp, textResults);

                //bmp.Dispose();

                var oOptions = new BarcodeReaderOptions
                {
                    // Choose a speed from: Faster, Balanced, Detailed, ExtremeDetail
                    // There is a tradeoff in performance as more Detail is set
                    Speed = ReadingSpeed.Balanced,

                    // Reader will stop scanning once a barcode is found, unless set to true
                    ExpectMultipleBarcodes = false,

                    // By default, all barcode formats are scanned for.
                    // Specifying one or more, performance will increase.
                    ExpectBarcodeTypes = BarcodeEncoding.AllOneDimensional, //BarcodeEncoding.AllOneDimensional, //BarcodeEncoding.Code128,

                    // Utilizes multiple threads to reads barcodes from multiple images in parallel.
                    Multithreaded = false,

                    // Maximum threads for parallel. Default is 4
                    MaxParallelThreads = 4,

                    // The area of each image frame in which to scan for barcodes.
                    // Will improve performance significantly and avoid unwanted results and avoid noisy parts of the image.
                    CropArea = new Rectangle(),

                    // Special Setting for Code39 Barcodes.
                    // If a Code39 barcode is detected. Try to use extended mode for the full ASCII Character Set
                    UseCode39ExtendedMode = true
                };

                switch (MDIMain.iRecognitionMode)
                {
                    case 0:
                        oOptions.Speed = ReadingSpeed.Faster;
                        break;
                    case 1:
                        oOptions.Speed = ReadingSpeed.Balanced;
                        break;
                    case 2:
                        oOptions.Speed = ReadingSpeed.Detailed;
                        break;
                    case 3:
                        oOptions.Speed = ReadingSpeed.ExtremeDetail;
                        break;
                }

                if (sCustBarcodeFmt.Split(',').Length > 1)
                {
                    oOptions.ExpectBarcodeTypes = BarcodeEncoding.AllOneDimensional;
                    //int i = 0;
                    //while (i < sCustBarcodeFmt.Split(',').Length)
                    //{
                    //    if (sCustBarcodeFmt.Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_39.ToString().Trim())
                    //        oOptions.ExpectBarcodeTypes = BarcodeEncoding.Code39;
                    //    else if (sCustBarcodeFmt.Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_93.ToString().Trim())
                    //        oOptions.ExpectBarcodeTypes = BarcodeEncoding.Code93;
                    //    else if (sCustBarcodeFmt.Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_128.ToString().Trim())
                    //        oOptions.ExpectBarcodeTypes = BarcodeEncoding.Code128;

                    //    break;
                    //    i += 1;
                    //}
                }
                else if (sCustBarcodeFmt.Trim() != "")
                {
                    if (sCustBarcodeFmt.Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_39.ToString().Trim())
                        oOptions.ExpectBarcodeTypes = BarcodeEncoding.Code39;
                    if (sCustBarcodeFmt.Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_93.ToString().Trim())
                        oOptions.ExpectBarcodeTypes = BarcodeEncoding.Code93;
                    if (sCustBarcodeFmt.Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_128.ToString().Trim())
                        oOptions.ExpectBarcodeTypes = BarcodeEncoding.Code128;
                }
                //updateRuntimeSettingsWithUISetting(ref oOptions);

                //System.Threading.Tasks.Task<BarcodeResults> textResults = IronBarCode.BarcodeReader.ReadAsync(bmp, oOptions);
                System.Threading.Tasks.Task<BarcodeResults> textResults = IronBarCode.BarcodeReader.ReadAsync(pFilename, oOptions);

                textResults.Wait();

                BarcodeResult[] results = textResults.Result.ToArray();

                if (results.Length > 0)
                {                    
                    isDocSep = true;
                }

                //textResults = null;
                results = null;
                textResults.Dispose();

                //this.ShowResultOnImage(bmp, textResults);
                //bmp.Dispose();
                //aByte = null;
            }
            catch (Exception exp)
            {
                mGlobal.Write2Log(exp.Message);
                //MessageBox.Show(exp.Message, "Decoding error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (bmp != null) bmp.Dispose();
            }

            return isDocSep;
        }

        private void frmExport_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (this.MdiParent != null && mGlobal.checkAnyOpenedForms() == false)
                    this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = true;
                else if (mGlobal.checkAnyOpenedForms() == false)
                    this.Owner.Controls["toolStrip"].Controls[0].Enabled = true;

                staMain.sessionRestart();
                //frmProcSumm ofProSumm = new frmProcSumm();
                //ofProSumm.Activated += ofProSumm.refreshProcSummary;
                //ofProSumm.refreshProcSummaryTreeview();
                //ofProSumm.tvwProcess.Update();
                //ofProSumm.tvwProcess.Refresh();
                //ofProSumm.Dispose();
            }
            catch (Exception ex)
            {
            }
        }

        public void getBarcodeCustomSettings()
        {
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 barcodefmt,barcodefmt2,recognitionmode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TCustomScanner ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    sCustBarcodeFmt = dr[0].ToString();
                    sCustBarcodeFmt2 = dr[1].ToString();
                    sCustRecog = dr[2].ToString();

                    if (sCustBarcodeFmt.Trim() == "" || sCustBarcodeFmt.Trim() == "BF_NULL")
                    {
                        mbCustom = false;
                    }
                    else
                    {
                        mbCustom = true;
                    }

                    if (sCustRecog.ToLower().Trim() == "speed")
                        MDIMain.iRecognitionMode = 0;
                    else if (sCustRecog.ToLower().Trim() == "balance")
                        MDIMain.iRecognitionMode = 1;
                    else
                        MDIMain.iRecognitionMode = 2; //coverage
                }
            }
            catch (Exception ex)
            {
                mbCustom = false;
            }
        }

        private bool updateExportScannedDB(string pBatchCode, string pBatchStatusIn, string sNewStatus)
        {
            bool ret = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "SET exported='" + sNewStatus + "' ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND scanstatus IN (" + pBatchStatusIn.Trim() + ") ";

                ret = mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                ret = false;
            }
            return ret;
        }

        private bool updateExportIndexedDB(string pBatchCode, string pBatchStatusIn, string sNewStatus)
        {
            bool ret = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "SET exported='" + sNewStatus + "' ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus IN (" + pBatchStatusIn.Trim() + ") ";

                ret = mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                ret = false;
            }
            return ret;
        }

        private void frmExport_Activated(object sender, EventArgs e)
        {
            try
            {
                staMain.enableScan(this.MdiParent, false);
                staMain.sessionRestart();
            }
            catch (Exception ex)
            {
            }
        }



    }
}
