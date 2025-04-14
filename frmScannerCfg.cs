using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Dynamsoft;
using Dynamsoft.TWAIN;

namespace SRDocScanIDP
{
    public partial class frmScannerCfg : Form
    {
        private string sScanProj;
        private string sAppNum;
        private string sUserID;

        EnumBarcodeFormat mEmBarcodeFormat = 0;
        EnumBarcodeFormat_2 mEmBarcodeFormat_2 = 0;

        public frmScannerCfg()
        {
            InitializeComponent();
            string sProdKey = MDIMain.strProductKey;
            MDIMain.oTMgr = new TwainManager(sProdKey);

        }

        private void frmScannerCfg_Load(object sender, EventArgs e)
        {
            staMain.sessionStop(false);
            sScanProj = MDIMain.strScanProject;
            sAppNum = MDIMain.intAppNum.ToString("000");

            sUserID = MDIMain.strUserID;

            loadScannerSrc();

            InitCbxResolution();

            loadCustomSettingsDB();

            if (cbxResolution.Text == "")
                cbxResolution.SelectedIndex = 2;

            this.Focus();
        }

        private void loadCustomSettingsDB()
        {
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 source,pixeltype,resolution,barcodefmt,barcodefmt2,recognitionmode,duplex,removeblank,ocrbarcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TCustomScanner ";
                sSQL += "WHERE scanproj='" + sScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + sAppNum.Trim().Replace("'", "") + "' ";

                mGlobal.LoadAppDBCfg();

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    if (cbxScannerSrc.Items.Count > 0)
                    {
                        int i = 0;
                        while (i < cbxScannerSrc.Items.Count)
                        {
                            if (cbxScannerSrc.Items[i].ToString().Trim() == dr[0].ToString().Trim())
                                cbxScannerSrc.SelectedIndex = i;

                            i += 1;
                        }
                    }

                    rdbBW.Checked = false; rdbGray.Checked = false; rdbColor.Checked = false;
                    if (dr[1].ToString().Trim() == rdbBW.Text.Replace("&",""))
                        rdbBW.Checked = true;
                    else if (dr[1].ToString().Trim() == rdbGray.Text)
                        rdbGray.Checked = true;
                    else if (dr[1].ToString().Trim() == rdbColor.Text)
                        rdbColor.Checked = true;

                    if (cbxResolution.Items.Count > 0)
                    {
                        int i = 0;
                        while (i < cbxResolution.Items.Count)
                        {
                            if (cbxResolution.Items[i].ToString().Trim() == dr[2].ToString().Trim())
                                cbxResolution.SelectedIndex = i;

                            i += 1;
                        }
                    }

                    cbxCode39.Checked = false; cbxCode93.Checked = false; cbxBCode128.Checked = false;
                    if (dr[3].ToString().Trim().Split(',').Length > 1)
                    {
                        int i = 0;
                        while (i < dr[3].ToString().Trim().Split(',').Length)
                        {
                            if (dr[3].ToString().Trim().Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_39.ToString())
                                cbxCode39.Checked = true;
                            if (dr[3].ToString().Trim().Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_93.ToString())
                                cbxCode93.Checked = true;
                            if (dr[3].ToString().Trim().Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_128.ToString())
                                cbxBCode128.Checked = true;

                            i += 1;
                        }
                    }
                    else if (dr[3].ToString().Trim() != "")
                    {
                        if (dr[3].ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_39.ToString())
                            cbxCode39.Checked = true;
                        if (dr[3].ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_93.ToString())
                            cbxCode93.Checked = true;
                        if (dr[3].ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_128.ToString())
                            cbxBCode128.Checked = true;
                    }

                    if (dr[5].ToString().Trim() == rdbBestSpeed.Text)
                        rdbBestSpeed.Checked = true;
                    else if (dr[5].ToString().Trim() == rdbBalance.Text)
                        rdbBalance.Checked = true;
                    else if (dr[5].ToString().Trim() == rdbBestCoverage.Text)
                        rdbBestCoverage.Checked = true;

                    if (dr[6].ToString().Trim().ToUpper() == "Y")
                        cbxDuplex.Checked = true;
                    else
                        cbxDuplex.Checked = false;

                    txtBlank.Text = dr[7].ToString();

                    if (dr[8].ToString().Trim().ToUpper() == "S")
                    {
                        rdbSingle.Checked = true;
                        rdbMultiple.Checked = false;
                    }
                    else
                    {
                        rdbSingle.Checked = false;
                        rdbMultiple.Checked = true;
                    }
                }                
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
        }

        private void InitCbxResolution()
        {
            cbxResolution.Items.Clear();
            cbxResolution.Items.Insert(0, "150");
            cbxResolution.Items.Insert(1, "200");
            cbxResolution.Items.Insert(2, "300");
            cbxResolution.Items.Insert(3, "512");
        }

        private void loadScannerSrc()
        {
            if (cbxScannerSrc.Items.Count > 0)
            {
                cbxScannerSrc.Items.Clear();
            }

            for (var i = 0; i < MDIMain.oTMgr.SourceCount; i++)
            {
                cbxScannerSrc.Items.Add(MDIMain.oTMgr.SourceNameItems((short)i));
            }
            if (cbxScannerSrc.Items.Count > 0)
            {
                cbxScannerSrc.SelectedIndex = 0;
            }

            MDIMain.oTMgr.CloseSource();
            MDIMain.oTMgr.CloseSourceManager();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool bExist = true;
            string sSQL = "";
            try
            {
                if (cbxScannerSrc.Text.Trim() == "")
                {
                    MessageBox.Show("Please select a scanner source!","Error Message");                    
                    return;
                }                

                if (!mGlobal.IsInteger(txtBlank.Text.Trim()))
                {
                    MessageBox.Show("Remove blank page is not a number!", "Error Message");
                    return;
                }
                else if ((Convert.ToInt32(txtBlank.Text.Trim()) > 0 && Convert.ToInt32(txtBlank.Text.Trim()) < 10) || Convert.ToInt32(txtBlank.Text.Trim()) > 20)
                {
                    MessageBox.Show("Remove blank page entry must be between 10 to 20! 0 for disabled.", "Error Message");
                    return;
                }

                string sSource = cbxScannerSrc.Text;
                string sPixelType = rdbBW.Text.Replace("&",""); //"BW";
                int iResol = Convert.ToInt32(cbxResolution.Text.Trim());
                string sBarFmt = getBarcodeFormat();
                string sBarFmt2 = getBarcodeFormat2();
                string sRecogMode = rdbBalance.Text; //"Balance";
                string sCreatedBy = sUserID.Trim();
                string sModifiedBy = sUserID.Trim();
                string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string sDuplex = "N";
                int iBlank = 0;
                string sOCRBarcode = "S";

                if (rdbGray.Checked)
                    sPixelType = rdbGray.Text; //"Gray";
                else if (rdbColor.Checked)
                    sPixelType = rdbColor.Text; //"Color";

                MDIMain.iRecognitionMode = 1; //default "Balance".

                if (rdbBestSpeed.Checked)
                {
                    sRecogMode = rdbBestSpeed.Text; //"Speed";
                    MDIMain.iRecognitionMode = 0;
                }                    
                else if (rdbBestCoverage.Checked)
                {
                    sRecogMode = rdbBestCoverage.Text; //"Coverage";
                    MDIMain.iRecognitionMode = 2;
                }

                if (cbxDuplex.Checked)
                    sDuplex = "Y";

                iBlank = Convert.ToInt32(txtBlank.Text.Trim());

                if (rdbMultiple.Checked)
                    sOCRBarcode = "M";
                
                bExist = checkCustomExist(sScanProj, sAppNum);

                if (bExist)
                {
                    if (MessageBox.Show("Scanner custom settings already exist! Overwrite?","Custom Settings", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    {                        
                        return;
                    }
                }

                mGlobal.LoadAppDBCfg();

                if (bExist)
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TCustomScanner ";
                    sSQL += "SET [source]='" + sSource.Trim().Replace("'", "") + "' ";
                    sSQL += ",[pixeltype]='" + sPixelType.Trim().Replace("'", "") + "' ";
                    sSQL += ",[resolution]=" + iResol + " ";
                    sSQL += ",[barcodefmt]='" + sBarFmt.Trim().Replace("'", "") + "' ";
                    sSQL += ",[barcodefmt2]='" + sBarFmt2.Trim().Replace("'", "") + "' ";
                    sSQL += ",[recognitionmode]='" + sRecogMode.Trim().Replace("'", "") + "' ";
                    sSQL += ",[duplex]='" + sDuplex.Trim().Replace("'", "") + "' ";
                    sSQL += ",[modifiedby]='" + sUserID.Trim().Replace("'", "") + "' ";
                    sSQL += ",[modifieddate]='" + sModifiedDate.Trim().Replace("'", "") + "' ";
                    sSQL += ",[removeblank]='" + iBlank + "' ";
                    sSQL += ",[ocrbarcode]='" + sOCRBarcode.Trim().Replace("'", "") + "' ";
                    sSQL += "WHERE scanproj='" + sScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + sAppNum.Trim().Replace("'", "") + "' ";

                    bool bOK = mGlobal.objDB.UpdateRows(sSQL, true);

                    if (bOK)
                        MessageBox.Show("Custom settings updated successfully.");
                    else
                        MessageBox.Show("Custom settings update unsuccessful!");

                    if (bOK) //Reset for Scan form.
                    {
                        if (mGlobal.checkOpenedForms("frmScan1") || mGlobal.checkOpenedForms("frmRescan1"))
                        {
                            MessageBox.Show("Document Scan screen is opened currently. \nPlease close and reopen the screen to allow the settings to take effect.", "Scanner Settings");
                        }
                    }
                }
                else
                {
                    sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TCustomScanner ";
                    sSQL += "([scanproj]";
                    sSQL += ",[appnum]";
                    sSQL += ",[source]";
                    sSQL += ",[pixeltype]";
                    sSQL += ",[resolution]";
                    sSQL += ",[barcodefmt]";
                    sSQL += ",[barcodefmt2]";
                    sSQL += ",[recognitionmode]";
                    sSQL += ",[duplex]";
                    sSQL += ",[removeblank]";
                    sSQL += ",[ocrbarcode]";
                    sSQL += ",[createdby]";
                    sSQL += ") VALUES (";
                    sSQL += "'" + sScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += ",'" + sAppNum.Trim().Replace("'", "") + "' ";
                    sSQL += ",'" + sSource.Trim().Replace("'", "") + "' ";
                    sSQL += ",'" + sPixelType.Trim().Replace("'", "") + "' ";
                    sSQL += "," + iResol + " ";
                    sSQL += ",'" + sBarFmt.Trim().Replace("'", "") + "' ";
                    sSQL += ",'" + sBarFmt2.Trim().Replace("'", "") + "' ";
                    sSQL += ",'" + sRecogMode.Trim().Replace("'", "") + "' ";
                    sSQL += ",'" + sDuplex.Trim().Replace("'", "") + "' ";
                    sSQL += ",'" + iBlank + "' ";
                    sSQL += ",'" + sOCRBarcode.Trim().Replace("'", "") + "' ";
                    sSQL += ",'" + sUserID.Trim().Replace("'", "") + "') ";

                    bool bOK = mGlobal.objDB.UpdateRows(sSQL, true);

                    if (bOK)
                        MessageBox.Show("Custom settings saved successfully.");
                    else
                        MessageBox.Show("Custom settings save unsuccessful!");

                    if (bOK) //Reset for Scan form.
                    {
                        if (mGlobal.checkOpenedForms("frmScan1") || mGlobal.checkOpenedForms("frmRescan1"))
                        {
                            MessageBox.Show("Document Scan or Rescan screen is opened currently. \nPlease close and reopen the screen to allow the settings to take effect.", "Scanner Settings");
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving scanner custom settings! Save aborted.\n" + ex.Message);
            }
        }

        private string getBarcodeFormat()
        {
            string sFormat = "";
            
            mEmBarcodeFormat = 0;
            //mEmBarcodeFormat = this.cbxAZTEC.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_AZTEC) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxDataMatrix.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_DATAMATRIX) : mEmBarcodeFormat;

            //mEmBarcodeFormat = this.cbxQRcode.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_QR_CODE) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxMicroQR.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_MICRO_QR) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxAllQRCode.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_QR_CODE | EnumBarcodeFormat.BF_MICRO_QR) : mEmBarcodeFormat;

            //mEmBarcodeFormat = this.cbxPDF417.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_PDF417) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxMicroPDF.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_MICRO_PDF417) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxAllPDF417.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_PDF417 | EnumBarcodeFormat.BF_MICRO_PDF417) : mEmBarcodeFormat;

            //mEmBarcodeFormat = this.cbxMaxicode.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_MAXICODE) : mEmBarcodeFormat;
            
            //mEmBarcodeFormat = this.cbxINDUSTRIAL25.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_INDUSTRIAL_25) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxUPCE.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_UPC_E) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxUPCA.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_UPC_A) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxEAN8.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_EAN_8) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxEAN13.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_EAN_13) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxCODABAR.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_CODABAR) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxITF.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_ITF) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxCODE93_.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_CODE_93) : mEmBarcodeFormat;
            mEmBarcodeFormat = this.cbxCode93.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_CODE_93) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxCODE128.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_CODE_128) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxCOD39.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_CODE_39) : mEmBarcodeFormat;
            mEmBarcodeFormat = this.cbxCode39.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_CODE_39) : mEmBarcodeFormat;
            //mEmBarcodeFormat = this.cbxMSICODE.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_MSI_CODE) : mEmBarcodeFormat;
            mEmBarcodeFormat = this.cbxBCode128.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_CODE_128) : mEmBarcodeFormat;

            //mEmBarcodeFormat = this.cbxPATCHCODE.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_PATCHCODE) : mEmBarcodeFormat;

            //mEmBarcodeFormat = this.cbxDATABAR.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_DATABAR_LIMITED | EnumBarcodeFormat.BF_GS1_DATABAR_TRUNCATED | EnumBarcodeFormat.BF_GS1_DATABAR_STACKED_OMNIDIRECTIONAL | EnumBarcodeFormat.BF_GS1_DATABAR_STACKED | EnumBarcodeFormat.BF_GS1_DATABAR_EXPANDED_STACKED | EnumBarcodeFormat.BF_GS1_DATABAR_OMNIDIRECTIONAL | EnumBarcodeFormat.BF_GS1_DATABAR_EXPANDED) : mEmBarcodeFormat;
            //if (this.cbxDATABAR.Checked)
            //{
            //    mEmBarcodeFormat = this.cbxDatabarLimited.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_DATABAR_LIMITED) : mEmBarcodeFormat;
            //    mEmBarcodeFormat = this.cbxDatabarOmnidirectional.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_DATABAR_OMNIDIRECTIONAL) : mEmBarcodeFormat;
            //    mEmBarcodeFormat = this.cbxDatabarExpanded.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_DATABAR_EXPANDED) : mEmBarcodeFormat;
            //    mEmBarcodeFormat = this.cbxDatabarExpanedStacked.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_DATABAR_EXPANDED_STACKED) : mEmBarcodeFormat;
            //    mEmBarcodeFormat = this.cbxDatabarStacked.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_DATABAR_STACKED) : mEmBarcodeFormat;
            //    mEmBarcodeFormat = this.cbxDatabarStackedOmnidirectional.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_DATABAR_STACKED_OMNIDIRECTIONAL) : mEmBarcodeFormat;
            //    mEmBarcodeFormat = this.cbxDatabarTruncated.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_DATABAR_TRUNCATED) : mEmBarcodeFormat;
            //}

            //mEmBarcodeFormat = this.cbxGS1Composite.Checked ? (mEmBarcodeFormat | EnumBarcodeFormat.BF_GS1_COMPOSITE) : mEmBarcodeFormat;

            sFormat = mEmBarcodeFormat.ToString();

            return sFormat;
        }
        private string getBarcodeFormat2()
        {
            string sFormat = "";
            mEmBarcodeFormat_2 = 0;

            //if (this.cbxPostalCode.Checked)
            //    cbxUSPSIntelligentMail.Checked = cbxAustralianPost.Checked = cbxRM4SCC.Checked = cbxPostnet.Checked = cbxPlanet.Checked = true;
            //else
            //    cbxUSPSIntelligentMail.Checked = cbxAustralianPost.Checked = cbxRM4SCC.Checked = cbxPostnet.Checked = cbxPlanet.Checked = false;

            //mEmBarcodeFormat_2 = this.cbxUSPSIntelligentMail.Checked ? (mEmBarcodeFormat_2 | EnumBarcodeFormat_2.BF2_USPSINTELLIGENTMAIL) : mEmBarcodeFormat_2;
            //mEmBarcodeFormat_2 = this.cbxAustralianPost.Checked ? (mEmBarcodeFormat_2 | EnumBarcodeFormat_2.BF2_AUSTRALIANPOST) : mEmBarcodeFormat_2;
            //mEmBarcodeFormat_2 = this.cbxRM4SCC.Checked ? (mEmBarcodeFormat_2 | EnumBarcodeFormat_2.BF2_RM4SCC) : mEmBarcodeFormat_2;
            //mEmBarcodeFormat_2 = this.cbxPostnet.Checked ? (mEmBarcodeFormat_2 | EnumBarcodeFormat_2.BF2_POSTNET) : mEmBarcodeFormat_2;
            //mEmBarcodeFormat_2 = this.cbxPlanet.Checked ? (mEmBarcodeFormat_2 | EnumBarcodeFormat_2.BF2_PLANET) : mEmBarcodeFormat_2;
            
            mEmBarcodeFormat_2 = this.cbxDOTCODE.Checked ? (mEmBarcodeFormat_2 | EnumBarcodeFormat_2.BF2_DOTCODE) : mEmBarcodeFormat_2;

            sFormat = mEmBarcodeFormat_2.ToString();

            return sFormat;
        }

        private bool checkCustomExist(string pScanProj, string pAppNum)
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'","") + ".dbo.TCustomScanner ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'","") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'","") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;
            }
            catch (Exception ex)
            {
                bRet = false;
            }

            return bRet;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbxCode39_Click(object sender, EventArgs e)
        {
            //Added for IronBarcode for only one option.
            //if (cbxCode39.Checked)
            //{
            //    cbxCode93.Checked = false;
            //    cbxBCode128.Checked = false;
            //}
        }

        private void cbxCode93_Click(object sender, EventArgs e)
        {
            //Added for IronBarcode for only one option.
            //if (cbxCode93.Checked)
            //{
            //    cbxCode39.Checked = false;
            //    cbxBCode128.Checked = false;
            //}
        }

        private void cbxBCode128_Click(object sender, EventArgs e)
        {
            //Added for IronBarcode for only one option.
            //if (cbxBCode128.Checked)
            //{
            //    cbxCode39.Checked = false;
            //    cbxCode93.Checked = false;
            //}
        }

        private void frmScannerCfg_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                staMain.sessionRestart();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
