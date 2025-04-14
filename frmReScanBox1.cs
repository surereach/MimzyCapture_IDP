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

using Dynamsoft;
//using Dynamsoft.DBR;
using Dynamsoft.TWAIN;
using Dynamsoft.TWAIN.Interface;
using Dynamsoft.Core;
//using Dynamsoft.PDF;
using Dynamsoft.Core.Enums;
using Dynamsoft.Core.Annotation;
using Dynamsoft.TWAIN.Enums;

using SRDocScanIDP;
using ImageInfoJson;

using IronBarCode;
using IronSoftware.Drawing;
using IronOcr;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using System.Data.Odbc;

namespace SRDocScanIDP
{
    public partial class frmRescanBox1 : Form, IAcquireCallback//, ISave
    {
        //private PDFRasterizer oPDFRasterizer = null;
        //private Dynamsoft.DBR.BarcodeReader oBarcodeRdr = null;
        //private PDFCreator oPDFCreator = null;
        private clsPDF oPDFCreator = null;

        //private string sLicense;
        string sErrMsg;
        //EnumErrorCode enmErrCode;

        private bool mbCustomScan = false;
        private bool mbCustom = false;
        //private PublicRuntimeSettings mNormalRuntimeSettings;

        //EnumBarcodeFormat mEmBarcodeFormat = 0;
        //EnumBarcodeFormat_2 mEmBarcodeFormat_2 = 0;

        private delegate void CrossThreadOperationControl();
        private delegate void CrossThreadOperationDB();

        private int _currentImageIndex = 0;
        private bool _isMultiPage = false;
        private string _currScanProj;
        private string _batchCode;
        private string _batchType;
        private string _boxNum;
        private string _boxRef;
        private string _appNum;
        private string _batchNum;
        private string _setNum;
        private string _collaNum;
        private bool _isDocSep; //Document Separator.
        private string _docDefId;
        private string _docType;
        private DateTime _scanStart;
        private DateTime _scanEnd;
        private string _scanStn;
        private string _scanUsr;
        private string _batchStatus;
        private int _totPageCnt;
        private string _noSeparator;
        private int _sepCnt;

        private DateTime _scanDStart;
        private DateTime _scanDEnd;
        private string _scanStatus;
        private string _imgFilename;
        private string _fromProc;

        private string sLastBatchNum;
        private int iBatchNum;
        private string sBatchNumFmt;
        private int iSetNum;
        private string sSetNumFmt;
        private int iCollaNum;
        private string sCollaNumFmt;

        private bool bScanStart;
        private bool bCancel;

        private bool bIsDuplexEnabled;
        private string sCustSrc;
        private string sCustPixelType;
        private int iCustResolutn;

        private string sCustBarcodeFmt;
        private string sCustBarcodeFmt2;
        private string sCustRecog;

        private clsDocuFile oDF = new clsDocuFile();

        public string sIsAddOn;

        private staMain._stcImgInfo _stcImgInfo;
        private ImageCore _oImgCore;

        private long _iCurrBatchRowid;
        private string sCurrSetNum;
        private long lCurrPgRowid;
        private string sCurrDocType;
        private string sLastDocType;

        private int iImgSeq;

        private string sImgInfoCopy;
        private bool bImgCut;
        private ImageCore oImgCut;
        private string sDrag;

        private List<string> lScanImg;

        private TreeNode selectedNode;

        private Bitmap oSepBmp;
        private bool bDocAutoOCR;
        private List<string> lAutoBarcode;
        private int iAutoBarCnt;

        private float fltBrightness;
        private float fltContrast;
        private byte[] oImgBytes;

        private string sFrontBack;
        private List<short> lHeaderIdx;
        private short sRmvBlank;
        private bool bIsBlankPage;
        private bool bMultipleBarcodeOCR;

        private const int FORM_MIN_WIDTH = 1280;
        private const int FORM_MIN_HEIGHT = 720;
        private int iFormOffset = 65;

        private string sRemarks;

        public frmRescanBox1()
        {
            InitializeComponent();
            string sProdKey = MDIMain.strProductKey;
            MDIMain.oTMgr = new TwainManager(sProdKey);
            _oImgCore = new ImageCore();
            _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;
            dsvImg.Bind(_oImgCore);
            dsvThumbnailList.Bind(_oImgCore);

            //sLicense = mGlobal.GetAppCfg("BCLic");
            //oBarcodeRdr = new Dynamsoft.DBR.BarcodeReader();

            //oPDFCreator = new PDFCreator(mGlobal.GetAppCfg("PDFLibKey"));
            //oPDFCreator = new PDFCreator(sProdKey);
            oPDFCreator = new clsPDF();

            //mNormalRuntimeSettings = oBarcodeRdr.GetRuntimeSettings();
            //mEmBarcodeFormat = EnumBarcodeFormat.BF_CODE_128;
            //mEmBarcodeFormat_2 = EnumBarcodeFormat_2.BF2_DOTCODE;

            _batchType = MDIMain.sBatchType;
            _noSeparator = staMain.stcProjCfg.NoSeparator;

            sCollaNumFmt = "000";
            if (_batchType.Trim().ToLower() == "set")
                sSetNumFmt = "000000";
            else
                sSetNumFmt = "000";

            sBatchNumFmt = "000000";

            dsvImg.IfFitWindow = true; //Windows Size if true else Actual Size set to false.
            //dsvImg.Zoom = 1; //Actual Size.

            dsvImg.MouseShape = true;
            dsvImg.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;

            dsvThumbnailList.MouseShape = true;
            dsvThumbnailList.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;

            selectedNode = null;

            IronBarCode.License.LicenseKey = MDIMain.strIronBcLic;
            IronOcr.License.LicenseKey = MDIMain.strIronLic;

            sFrontBack = "F";
            lHeaderIdx = new List<short>();

            sRemarks = "";
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams parms = base.CreateParams;
        //        parms.ClassStyle |= 0x200;  // CS_NOCLOSE
        //        return parms;
        //    }
        //}

        private void frmRescanBox_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = this.Text + ": " + MDIMain.strScanProject;
                this.timer1.Enabled = true;

                this.ShowIcon = false;
                //this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.WindowState = FormWindowState.Maximized;
                this.ScanStatusStrip.Width = this.Width;
                //this.ScanStatusBar1.Width = this.ScanStatusStrip.Width - this.ScanStatusBar.Width - this.ScanProgressBar.Width - this.CurrDateTime.Width;

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
                sErrMsg = "";

                int appNum = MDIMain.intAppNum;

                _currScanProj = MDIMain.strScanProject; //this.MdiParent.Controls["toolStrip"].Controls[0].Text;
                _appNum = appNum.ToString("000");

                _boxNum = "";
                _boxRef = "";
                _batchCode = "";
                _batchType = MDIMain.sBatchType;                          
                _isDocSep = true;
                _docDefId = MDIMain.sDocDefId;
                _docType = staMain.stcProjCfg.SeparatorText;
                _scanStart = DateTime.Now;
                _scanEnd = DateTime.Now;
                _scanStn = MDIMain.strStation;
                _scanUsr = MDIMain.strUserID;
                _batchStatus = ""; //1 - Scan, 2 - Indexing, 3 - Deleted, 4 - Rescan, 5 - Verify, 6 - Export, 7 - Transfer, 8 - Reindex.
                _totPageCnt = 0;
                _fromProc = "";
                _sepCnt = 0;

                _scanDStart = DateTime.Now;
                _scanDEnd = DateTime.Now;
                _scanStatus = "R"; //Scan,Rescan,Addon,Index,Verify,Process,Export,Transfer.
                _imgFilename = "";

                this.iBatchNum = oDF.getLastBatchNum(_currScanProj, _appNum, _batchType);
                sLastBatchNum = iBatchNum.ToString(sBatchNumFmt);

                iBatchNum = oDF.getNewBatchNum(_currScanProj, _appNum, DateTime.Now, _batchType);
                _batchNum = iBatchNum.ToString(sBatchNumFmt);

                iImgSeq = 0;
                lCurrPgRowid = 0;
                lScanImg = new List<string>();
                sCurrDocType = "";
                sLastDocType = "";

                //if (_batchType.Trim().ToLower() == "set")
                //    this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum);
                //else //batch.
                    this.iSetNum = 0;

                _setNum = this.iSetNum.ToString(sSetNumFmt);

                sCurrSetNum = _setNum;

                this.iCollaNum = oDF.getLastCollaNum(_currScanProj, _appNum, _batchCode, _setNum);
                _collaNum = this.iCollaNum.ToString(sCollaNumFmt);

                _iCurrBatchRowid = 0;
                _stcImgInfo = new staMain._stcImgInfo();
                initImgInfoCollection(1);

                sImgInfoCopy = "";
                bImgCut = false;
                oImgCut = null;

                sIsAddOn = MDIMain.sIsAddOn;
                if (sIsAddOn.Trim() != "")
                {
                    this.MinimizeBox = true;
                    ScanStatusBar1.Text = "Addon";
                    this.Text = ScanStatusBar1.Text + " " + this.Text;

                    this.iBatchNum = oDF.getLastBatchNum(_currScanProj, _appNum, _batchType);
                    sLastBatchNum = iBatchNum.ToString(sBatchNumFmt);
                    _batchNum = sLastBatchNum;

                    setInfoForAddOn(false);                                   
                }
                else
                {
                    ScanStatusBar1.Text = "Rescan";
                    //loadRejectedImagesFrmDisk(_iCurrBatchRowid);

                    //CheckImageCount();

                    //if (_fromProc.Trim().ToLower() == "verify") //From Verify rescan request.
                    //    //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                    //    loadRejectedBatchSetTreeview();
                    //else
                    //    loadBatchSetRescanTreeview(_fromProc); //loadBatchSetRescanTreeview();
                    //if (tvwBatch.Nodes.Count > 0)
                    //    tvwBatch.ExpandAll();
                }

                bScanStart = false;
                bCancel = false;

                dsvImg.IfFitWindow = true;
                //dsvImg.Zoom = 1;
                dsvImg.SetViewMode(-1, -1);

                dsvThumbnailList.IfFitWindow = true;
                dsvThumbnailList.SetViewMode(1, 10);

                getScannerCustomSettings();
                getBarcodeCustomSettings();

                ScanStatusBar.Text = "Ready";
                //loadBatchTreeview(_scanStart, _scanEnd);
                //if (tvwBatch.Nodes.Count > 0)
                //{
                //    btnDeleteStrip.Enabled = true;
                //    btnDeleteAllStrip.Enabled = true;

                //    //btnNoticeStrip.Enabled = true;
                //    //tvwBatch.ExpandAll();
                //}
                //else
                //{
                //    btnDeleteStrip.Enabled = false;
                //    btnDeleteAllStrip.Enabled = false;

                //    //btnNoticeStrip.Enabled = false;
                //}

                sDrag = string.Empty;
                tvwBatch.AllowDrop = true;
                tvwBatch.Dock = DockStyle.None;

                tvwBatch.ItemDrag += new ItemDragEventHandler(tvwBatch_ItemDrag);
                tvwBatch.DragEnter += new DragEventHandler(tvwBatch_DragEnter);
                tvwBatch.DragOver += new DragEventHandler(tvwBatch_DragOver);
                tvwBatch.DragDrop += new DragEventHandler(tvwBatch_DragDrop);

                int iMainId = oDF.getOCRAreaMainDB(_currScanProj, _appNum);
                oDF.getOCRAreaDB(iMainId);

                if (_noSeparator.Trim() == "C")
                    clsIDP.loadIDPMappingDB(_currScanProj, _appNum, _docDefId);

                staMain.loadIndexConfigDB(_currScanProj, _appNum, _docDefId);

                if (oDF.stcOCRAreaList.Count > 0 && staMain.checkIndexAutoOCREnabled())
                    bDocAutoOCR = true;

                lAutoBarcode = staMain.checkIndexAutoOCRBarcodeEnabled();

                oImgBytes = null;
                fltBrightness = 1;
                fltContrast = 1;
                txtBrighttoolStrip.Text = fltBrightness.ToString();
                txtContrtoolStrip.Text = fltContrast.ToString();

                sRmvBlank = 0;
                bIsBlankPage = false;
                bMultipleBarcodeOCR = false;

                enableScan(true);

                ScanToolStrip.Focus();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }            
        }

        private void frmRescanBox1_Paint(object sender, PaintEventArgs e)
        {
            if (tvwBatch.Nodes.Count > 0)
            {
                staMain.sessionStop(false);
            }
            if (System.Windows.Forms.Screen.AllScreens[0].Bounds.Width == FORM_MIN_WIDTH && System.Windows.Forms.Screen.AllScreens[0].Bounds.Height == FORM_MIN_HEIGHT)
            {
                iFormOffset = 60;
            }
            else
            {
                iFormOffset = 65;
            }
            formResize();
        }

        private void frmRescanBox_Resize(object sender, EventArgs e)
        {
            try
            {
                formResize();

                System.Drawing.Point oPoint = new System.Drawing.Point();
                if (tvwBatch.Width + dsvImg.Width + dsvThumbnailList.Width + 65 < this.Width)
                {
                    //tvwBatch.Height = ScanStatusStrip.Location.Y - tvwBatch.Location.Y - 10;
                    tvwBatch.Height = Height - ScanStatusStrip.Height - ScanToolStrip.Height - 23;

                    dsvImg.Width = this.Width - tvwBatch.Width - dsvThumbnailList.Width - 65;
                    //dsvImg.Height = txtInfo.Location.Y - dsvImg.Location.Y - 5;
                    dsvImg.Height = Height - ScanStatusStrip.Height - ScanToolStrip.Height - txtInfo.Height - 25;

                    txtInfo.Width = dsvImg.Width - panel1.Width - 5;
                    oPoint.Y = dsvImg.Location.Y + dsvImg.Height + 5;
                    oPoint.X = dsvImg.Location.X;
                    txtInfo.Location = oPoint;
                    dsvImg.Height = txtInfo.Location.Y - dsvImg.Location.Y - 5;

                    oPoint.Y = dsvImg.Location.Y + dsvImg.Height;
                    oPoint.X = txtInfo.Location.X + txtInfo.Width + 5;
                    panel1.Location = oPoint;
                    oPoint.Y = dsvThumbnailList.Location.Y;
                    oPoint.X = dsvImg.Location.X + dsvImg.Width + 5;
                    dsvThumbnailList.Location = oPoint;
                    dsvThumbnailList.Height = tvwBatch.Height;
                }
            }
            catch (Exception)
            {
            }
        }

        private void formResize()
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                //MessageBox.Show(this.Width.ToString() + "," + this.ScanStatusStrip.Width.ToString());
                //this.ScanStatusStrip.Width = this.Width;
                //this.ScanStatusBar1.Width = this.ScanStatusBar1.Width + 135;
                if (this.WindowState == FormWindowState.Normal)
                {
                    if (this.Width < FORM_MIN_WIDTH) this.Width = FORM_MIN_WIDTH;
                    if (this.Height < FORM_MIN_HEIGHT) this.Height = FORM_MIN_HEIGHT;
                    iFormOffset = -this.CurrDateTime.Width;
                }
                //else if (this.WindowState == FormWindowState.Maximized)
                //{
                //    iFormOffset = 0;
                //}

                if (MDIMain.sIsAddOn == "")
                    this.ScanProgressBar.Width = this.Width - this.ScanStatusBar.Width - this.ScanStatusBar1.Width - this.CurrDateTime.Width - iFormOffset;

                if (this.ScanStatusBar1.Width > 350)
                    this.ScanStatusBar1.Width = 350;
                if (this.ScanProgressBar.Width < 708)
                    this.ScanProgressBar.Width = 708;
            }
        }

        public void btnScan_Click(object sender, EventArgs e)
        {
            bool bValid = true;
            try
            {
                if (BoxNoStripTxt.Text.Trim() == "")
                {
                    MessageBox.Show("Box number is required! Please fill in box number.");
                    bValid = false;
                    return;
                }
                else if (_batchCode.Trim() == "" && staMain.stcProjCfg.BatchAuto.Trim().ToUpper() == "N")
                {
                    MessageBox.Show("Batch code is required! Please check with your system administrator.");
                    bValid = false;
                    return;
                }
                else
                {
                    if (!mGlobal.IsInteger(BoxNoStripTxt.Text.Trim()))
                    {
                        MessageBox.Show("Box number must be number! Please fill in box number with number only.");
                        bValid = false;
                        return;
                    }
                }

                btnLoadImgStrip.Enabled = false;
                //btnScan.Enabled = false;
                enableScan(false);
                btnDeleteStrip.Enabled = false;
                btnDeleteAllStrip.Enabled = false;
                btnNoticeStrip.Enabled = false;

                txtBrighttoolStrip.Enabled = false;
                txtContrtoolStrip.Enabled = false;

                //if (btnSend.Visible)
                if (SendStripBtn.Visible)
                {
                    //btnSend.Enabled = false;
                    SendStripBtn.Enabled = false;
                }

                _boxNum = BoxNoStripTxt.Text.Trim();
                _boxRef = boxLabelStripTxt.Text.Trim();
                BoxNoStripTxt.ReadOnly = true;
                boxLabelStripTxt.ReadOnly = true;

                this._currScanProj = MDIMain.strScanProject;

                Cursor.Current = Cursors.WaitCursor;

                ScanToolStrip.Enabled = false;
                //if (tvwBatch.SelectedNode != null) //document set is selected.
                //{
                //    if (_batchType.Trim().ToLower() == "batch" && tvwBatch.SelectedNode.Level == 1 && _noSeparator.Trim() != "0")
                //    {
                //        this._setNum = tvwBatch.SelectedNode.Tag.ToString().Split('|').GetValue(1).ToString();
                //    }
                //    else
                //    {
                //        //if (_batchType.Trim().ToLower() == "set")
                //        //    this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum);
                //        //else //batch.
                //        //    this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum, _batchCode);
                //        this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum, _batchCode);

                //        //this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum);
                //        this._setNum = iSetNum.ToString(sSetNumFmt);
                //    }
                //}
                //else
                //{ 
                if (tvwBatch.Nodes.Count == 0) //Set number start with 0.
                {
                    this.iBatchNum = oDF.getLastBatchNum(_currScanProj, _appNum, _batchType);
                    sLastBatchNum = iBatchNum.ToString(sBatchNumFmt);
                    this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum, _batchCode);

                    //this.iSetNum = 0;
                    this._setNum = iSetNum.ToString(sSetNumFmt);

                    //this.iCollaNum = 0;
                    //_collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                }
                else
                {
                    this.iBatchNum = Convert.ToInt32(oDF.getBatchCodeNum(_currScanProj, _appNum, _batchCode));
                    sLastBatchNum = iBatchNum.ToString(sBatchNumFmt);
                    this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum, _batchCode);

                    //this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum);
                    this._setNum = iSetNum.ToString(sSetNumFmt);

                    //this.iCollaNum = oDF.getLastCollaNum(_currScanProj, _appNum, _batchCode, _setNum);
                    //_collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                }
                //}

                if (oDF.checkDocuScanExist(_currScanProj, _appNum, _batchCode, _setNum, sLastDocType))
                {
                    this.iCollaNum = oDF.getCurrCollaNum(_currScanProj, _appNum, _batchCode, _setNum, sLastDocType);
                    _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                }
                else
                {
                    this.iCollaNum = oDF.getLastCollaNum(_currScanProj, _appNum, _batchCode, _setNum);
                    _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                }

                this._scanStatus = "R";

                int iCurrImgSeq = oDF.getLastImageSeqDB(_currScanProj, _appNum, _setNum, _collaNum, _batchCode);
                if (iCurrImgSeq == 1)
                    this.iImgSeq = 0;
                else
                    this.iImgSeq = iCurrImgSeq;

                if (this.sIsAddOn.Trim() != "")
                {
                    this.iBatchNum = oDF.getLastBatchNum(_currScanProj, _appNum, _batchType);
                    sLastBatchNum = iBatchNum.ToString(sBatchNumFmt);
                    _batchNum = sLastBatchNum;
                    setInfoForAddOn(false);                    
                }

                //_oImgCore = new ImageCore();
                //_oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;
                //dsvImg.Bind(_oImgCore); //Init.

                //if (enmErrCode != EnumErrorCode.DBR_SUCCESS)
                //{
                //    MessageBox.Show("Barcode scanning license is invalid!");
                //    bValid = false;
                //}
                if (!IronBarCode.License.IsValidLicense(MDIMain.strIronBcLic))
                {
                    MessageBox.Show("Barcode scanning license is invalid!");
                    bValid = false;
                }

                if (this._currScanProj.Trim() == "")
                {
                    MessageBox.Show("Current Scan Project cannot be blank! Please select a current scan project at the top left of the screen.");
                    bValid = false;
                }
                
                if (bValid)
                {
                    //Init.>>
                    this._isDocSep = true;
                    _sepCnt = 0;
                    iAutoBarCnt = 0;

                    if (sCurrDocType.Trim() != "")
                        _docType = sCurrDocType.Trim();
                    else
                        this._docType = _batchType.Trim().ToLower() == "set" ? _setNum : "";

                    if (_batchType.Trim().ToLower() == "set")
                        _docType = _setNum.Trim();

                    this._batchStatus = "4";
                    sFrontBack = "F";
                    bIsBlankPage = false;
                    bMultipleBarcodeOCR = false;
                    tvwBatch.Nodes.Clear();
                    //>>
                    btnCloseStrip.Enabled = false;
                    this.ScanToolStrip.Items["btnCloseStrip"].Enabled = false;

                    //Disable Scanner Source menu item.
                    mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], false, "mnuScanSettingStrip");
                    mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], false, "mnuScannerStrip");

                    _scanStart = DateTime.Now;
                    bScanStart = true;

                    dsvImg.IfFitWindow = true;
                    //dsvImg.Zoom = 1;

                    AcquireImage();

                    if (bCancel)
                    {
                        bCancel = false; //Init.
                        ScanStatusBar.Text = "Ready";
                        timerProgress.Enabled = false;
                        ScanProgressBar.Value = 0;
                    }

                    //Application.DoEvents();
                    //mGlobal.Write2Log("Scan:" + _scanEnd.ToString("HH:mm:ss"));                   
                }
                 
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
            finally
            {
                BoxNoStripTxt.ReadOnly = false;
                boxLabelStripTxt.ReadOnly = false;

                btnLoadImgStrip.Enabled = true;
                //btnScan.Enabled = true;
                enableScan(true);
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                //if (btnSend.Visible)
                if (SendStripBtn.Visible)
                {
                    //btnSend.Enabled = true;
                    SendStripBtn.Enabled = true;
                }

                btnNoticeStrip.Enabled = true;
                if (MDIMain.oTMgr.PixelType == TWICapPixelType.TWPT_RGB)
                {
                    txtBrighttoolStrip.Enabled = true;
                    txtContrtoolStrip.Enabled = true;
                }

                //oBarcodeRdr.Dispose();
                //mGlobal.Write2Log("Final:" + DateTime.Now.ToString("HH:mm:ss"));
                timerProgress.Enabled = false;
                ScanProgressBar.Value = 0;
                ScanStatusBar.Text = "Ready";
                ScanStatusBar1.Text = "";

                Cursor.Current = Cursors.Default;
                ScanToolStrip.Enabled = true;
                //try
                //{
                //    loadBatchTreeview(_scanStart, DateTime.Now.AddSeconds(60));
                //}
                //catch (Exception ex)
                //{
                //}                
                btnCloseStrip.Enabled = true;
                this.ScanToolStrip.Items["btnCloseStrip"].Enabled = true;

                //Enable Scanner Source menu item.
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], true, "mnuScanSettingStrip");
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], true, "mnuScannerStrip");
                
                if (tvwBatch.Nodes.Count > 0)
                {
                    btnDeleteStrip.Enabled = true;
                    btnDeleteAllStrip.Enabled = true;
                    //tvwBatch.ExpandAll();
                }
                else
                {
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;
                }
                //btnScan.Focus();
                ScanToolStrip.Focus();
            }
        }

        private bool AcquireImage()
        {
            bool bRet = false;
            try
            {
                if (MDIMain.oTMgr.SourceCount == 0)
                {
                    MessageBox.Show("Scanner source does not exists!");
                    //btnScan.Enabled = true;
                    enableScan(true);
                    return false;
                }

                getScannerCustomSettings();
                getBarcodeCustomSettings(); //Added for IronBarcode settings update.

                // Select the source for TWAIN
                var srcIndex = -1;
                for (short i = 0; i < MDIMain.oTMgr.SourceCount; i++)
                {                   
                    if (mbCustom && MDIMain.oTMgr.SourceNameItems(i).Trim() == sCustSrc.Trim())
                    {
                        srcIndex = i;
                        break;
                    }
                    else if (MDIMain.oTMgr.SourceNameItems(i) != "") continue;

                    srcIndex = i;
                    break;
                }

                MDIMain.oTMgr.CloseSource();
                MDIMain.oTMgr.CloseSourceManager();
                MDIMain.oTMgr.SelectSourceByIndex(srcIndex == -1 ? 0 : srcIndex);
                MDIMain.oTMgr.OpenSource();
                MDIMain.oTMgr.IfDisableSourceAfterAcquire = true;
                MDIMain.oTMgr.XferCount = -1;
                MDIMain.oTMgr.IfShowUI = false;
                MDIMain.oTMgr.IfDuplexEnabled = bIsDuplexEnabled;
                MDIMain.oTMgr.AlwaysOnTop = false;

                if (mbCustomScan)
                {
                    if (sCustPixelType.Trim() == "Color")
                    {
                        MDIMain.oTMgr.PixelType = TWICapPixelType.TWPT_RGB;
                        MDIMain.oTMgr.BitDepth = 24;

                    }
                    else if (sCustPixelType.Trim() == "Gray")
                    {
                        MDIMain.oTMgr.PixelType = TWICapPixelType.TWPT_GRAY;
                        MDIMain.oTMgr.BitDepth = 8;
                    }
                    else //Black & White
                    {
                        MDIMain.oTMgr.PixelType = TWICapPixelType.TWPT_BW;
                        MDIMain.oTMgr.BitDepth = 1;
                    }

                    MDIMain.oTMgr.Resolution = iCustResolutn;
                }
                else
                {
                    MDIMain.oTMgr.PixelType = TWICapPixelType.TWPT_BW;
                    MDIMain.oTMgr.BitDepth = 1;

                    MDIMain.oTMgr.Resolution = 512;
                }

                if (MDIMain.oTMgr.PixelType == TWICapPixelType.TWPT_RGB)
                {
                    this.txtBrighttoolStrip.Enabled = true;
                    this.txtContrtoolStrip.Enabled = true;
                }
                else
                {
                    this.txtBrighttoolStrip.Enabled = false;
                    this.txtContrtoolStrip.Enabled = false;
                }

                //btnScan.Enabled = false;
                enableScan(false);
                ScanStatusBar.Text = "Scanning";
                timerProgress.Enabled = true;

                bRet = MDIMain.oTMgr.AcquireImage(this as Dynamsoft.TWAIN.Interface.IAcquireCallback);
                if (!bRet)
                {
                    MessageBox.Show("An error occurred while scanning.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (TwainException ex)
            {
                ScanStatusBar.Text = "Ready";
                ScanProgressBar.Value = 0;
                timerProgress.Enabled = false;
                
                MessageBox.Show("An exception occurs: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MDIMain.oTMgr.CloseSource();
                MDIMain.oTMgr.CloseSourceManager();

                if (!bRet)
                {
                    //btnScan.Enabled = true;
                    enableScan(true);
                }
                ScanStatusBar1.Text = "";
            }
            return bRet;
        }

        public void getScannerCustomSettings()
        {
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 source,pixeltype,resolution,duplex,removeblank ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'","") + ".dbo.TCustomScanner ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'","") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    sCustSrc = dr[0].ToString();
                    sCustPixelType = dr[1].ToString();
                    iCustResolutn = Convert.ToInt32(dr[2].ToString());

                    if (dr[3].ToString().ToUpper() == "Y")
                        bIsDuplexEnabled = true;
                    else
                        bIsDuplexEnabled = false;

                    sRmvBlank = Convert.ToInt16(dr[4].ToString());

                    mbCustomScan = true;
                }
            }
            catch (Exception ex)
            {
                mbCustomScan = false;
            }
        }

        public void getBarcodeCustomSettings()
        {
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 barcodefmt,barcodefmt2,recognitionmode,ocrbarcode ";
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

                    if (dr[3].ToString().ToUpper() == "M")
                        bMultipleBarcodeOCR = true;
                    else
                        bMultipleBarcodeOCR = false;
                }
            }
            catch (Exception ex)
            {
                mbCustom = false;
            }
        }

        //private void updateBarcodeFormat()
        //{
        //    try
        //    {
        //        this.mEmBarcodeFormat = Dynamsoft.EnumBarcodeFormat.BF_CODE_128;
        //        this.mEmBarcodeFormat_2 = Dynamsoft.EnumBarcodeFormat_2.BF2_DOTCODE;

        //        getBarcodeCustomSettings();

        //        if (mbCustom)
        //        {
        //            if (sCustBarcodeFmt.Split(',').Length > 1)
        //            {
        //                int i = 0;
        //                while (i < sCustBarcodeFmt.Split(',').Length)
        //                {
        //                    if (sCustBarcodeFmt.Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_39.ToString())
        //                        this.mEmBarcodeFormat = this.mEmBarcodeFormat | Dynamsoft.EnumBarcodeFormat.BF_CODE_39;
        //                    if (sCustBarcodeFmt.Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_93.ToString())
        //                        this.mEmBarcodeFormat = this.mEmBarcodeFormat | Dynamsoft.EnumBarcodeFormat.BF_CODE_93;

        //                    i += 1;
        //                }
        //            }
        //            else if (sCustBarcodeFmt.Trim() != "")
        //            {
        //                if (sCustBarcodeFmt.Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_39.ToString())
        //                    this.mEmBarcodeFormat = Dynamsoft.EnumBarcodeFormat.BF_CODE_39;
        //                if (sCustBarcodeFmt.Trim() == Dynamsoft.EnumBarcodeFormat.BF_CODE_93.ToString())
        //                    this.mEmBarcodeFormat = Dynamsoft.EnumBarcodeFormat.BF_CODE_93;
        //            }

        //            if (sCustBarcodeFmt2.Split(',').Length > 1)
        //            {
        //                int i = 0;
        //                while (i < sCustBarcodeFmt2.Split(',').Length)
        //                {
        //                    if (sCustBarcodeFmt2.Split(',').GetValue(i).ToString().Trim() == Dynamsoft.EnumBarcodeFormat_2.BF2_POSTALCODE.ToString())
        //                        this.mEmBarcodeFormat_2 = this.mEmBarcodeFormat_2 | Dynamsoft.EnumBarcodeFormat_2.BF2_POSTALCODE;

        //                    i += 1;
        //                }
        //            }
        //            else if (sCustBarcodeFmt2.Trim() != "")
        //            {
        //                if (sCustBarcodeFmt2.Trim() == Dynamsoft.EnumBarcodeFormat_2.BF2_POSTALCODE.ToString())
        //                    this.mEmBarcodeFormat_2 = Dynamsoft.EnumBarcodeFormat_2.BF2_POSTALCODE;
        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        //private void updateRuntimeSettingsWithUISetting()
        //{
        //    oBarcodeRdr.ResetRuntimeSettings();
        //    updateBarcodeFormat();

        //    if (mbCustom)
        //    {
        //        PublicRuntimeSettings runtimeSettings = oBarcodeRdr.GetRuntimeSettings();
        //        runtimeSettings.BarcodeFormatIds = (int)this.mEmBarcodeFormat;
        //        runtimeSettings.BarcodeFormatIds_2 = (int)this.mEmBarcodeFormat_2;

        //        //if (!this.tbExpectedBarcodesCount.Text.Equals(""))
        //        //    runtimeSettings.ExpectedBarcodesCount = Int32.Parse(this.tbExpectedBarcodesCount.Text);
        //        runtimeSettings.ExpectedBarcodesCount = 1;
        //        runtimeSettings.DeblurLevel = 5; //cmbDeblurLevel_SelectedIndex;// this.cmbDeblurLevel.SelectedIndex;
                
        //        for (int i = 0; i < runtimeSettings.LocalizationModes.Length; i++)
        //            runtimeSettings.LocalizationModes[i] = EnumLocalizationMode.LM_SKIP;

        //        switch (5) //(this.cmbLocalizationModes_SelectedIndex)
        //        {
        //            case 0:
        //                runtimeSettings.LocalizationModes = mNormalRuntimeSettings.LocalizationModes;
        //                break;
        //            case 1:
        //                runtimeSettings.LocalizationModes[0] = EnumLocalizationMode.LM_CONNECTED_BLOCKS;
        //                break;
        //            case 2:
        //                runtimeSettings.LocalizationModes[0] = EnumLocalizationMode.LM_STATISTICS;
        //                break;
        //            case 3:
        //                runtimeSettings.LocalizationModes[0] = EnumLocalizationMode.LM_LINES;
        //                break;
        //            case 4:
        //                runtimeSettings.LocalizationModes[0] = EnumLocalizationMode.LM_SCAN_DIRECTLY;
        //                break;
        //            case 5:
        //                runtimeSettings.LocalizationModes[0] = EnumLocalizationMode.LM_CONNECTED_BLOCKS;
        //                runtimeSettings.LocalizationModes[1] = EnumLocalizationMode.LM_SCAN_DIRECTLY;
        //                break;
        //        }

        //        runtimeSettings.FurtherModes.TextFilterModes[0] = EnumTextFilterMode.TFM_SKIP; //(this.cbTextFilterMode.CheckState == CheckState.Checked) ? EnumTextFilterMode.TFM_GENERAL_CONTOUR : EnumTextFilterMode.TFM_SKIP;
        //        runtimeSettings.FurtherModes.RegionPredetectionModes[0] = EnumRegionPredetectionMode.RPM_SKIP; //(this.cbRegionPredetectionMode.CheckState == CheckState.Checked) ? EnumRegionPredetectionMode.RPM_GENERAL_RGB_CONTRAST : EnumRegionPredetectionMode.RPM_SKIP;

        //        runtimeSettings.ScaleDownThreshold = 512; // (Int32.Parse(this.tbScaleDownThreshold.Text) < 512) ? 512 : Int32.Parse(this.tbScaleDownThreshold.Text);
                
        //        switch (2) //(this.cmbGrayscaleTransformationModes_SelectedIndex)
        //        {
        //            case 0:
        //                runtimeSettings.FurtherModes.GrayscaleTransformationModes[0] = EnumGrayscaleTransformationMode.GTM_ORIGINAL;
        //                runtimeSettings.FurtherModes.GrayscaleTransformationModes[1] = EnumGrayscaleTransformationMode.GTM_INVERTED;
        //                break;
        //            case 1:
        //                runtimeSettings.FurtherModes.GrayscaleTransformationModes[0] = EnumGrayscaleTransformationMode.GTM_INVERTED;
        //                runtimeSettings.FurtherModes.GrayscaleTransformationModes[1] = EnumGrayscaleTransformationMode.GTM_SKIP;
        //                break;
        //            case 2:
        //                runtimeSettings.FurtherModes.GrayscaleTransformationModes[0] = EnumGrayscaleTransformationMode.GTM_ORIGINAL;
        //                runtimeSettings.FurtherModes.GrayscaleTransformationModes[1] = EnumGrayscaleTransformationMode.GTM_SKIP;
        //                break;
        //        }

        //        switch (0) //(this.cmbImagePreprocessingModes_SelectedIndex)
        //        {
        //            case 0:
        //                runtimeSettings.FurtherModes.ImagePreprocessingModes[0] = EnumImagePreprocessingMode.IPM_GENERAL;
        //                break;
        //            case 1:
        //                runtimeSettings.FurtherModes.ImagePreprocessingModes[0] = EnumImagePreprocessingMode.IPM_GRAY_EQUALIZE;
        //                break;
        //            case 2:
        //                runtimeSettings.FurtherModes.ImagePreprocessingModes[0] = EnumImagePreprocessingMode.IPM_GRAY_SMOOTH;
        //                break;
        //            case 3:
        //                runtimeSettings.FurtherModes.ImagePreprocessingModes[0] = EnumImagePreprocessingMode.IPM_SHARPEN_SMOOTH;
        //                break;
        //        }

        //        runtimeSettings.MinResultConfidence = 5 * 10; //this.cmbMinResultConfidence_SelectedIndex * 10;

        //        runtimeSettings.FurtherModes.TextureDetectionModes[0] = EnumTextureDetectionMode.TDM_SKIP; //(this.cmbTextureDetectionSensitivity_SelectedIndex == 0) ? EnumTextureDetectionMode.TDM_SKIP : EnumTextureDetectionMode.TDM_GENERAL_WIDTH_CONCENTRATION;

        //        oBarcodeRdr.UpdateRuntimeSettings(runtimeSettings);

        //        string strErrorMessage;
        //        //if (this.cmbTextureDetectionSensitivity_SelectedIndex != 0)
        //        //    oBarcodeRdr.SetModeArgument("TextureDetectionModes", 0, "Sensitivity", this.cmbTextureDetectionSensitivity_SelectedIndex.ToString(), out strErrorMessage);
        //        //if (!this.tbBinarizationBlockSize.Text.Equals(""))
        //        //    oBarcodeRdr.SetModeArgument("BinarizationModes", 0, "BlockSizeX", this.tbBinarizationBlockSize.Text, out strErrorMessage);
        //    }
        //    else
        //    {
        //        // 0 Best Speed. 1 Balance. 2 Best Coverage.
        //        switch (MDIMain.iRecognitionMode) //(miRecognitionMode)
        //        {
        //            case 0:
        //                PublicRuntimeSettings tempBestSpeed = oBarcodeRdr.GetRuntimeSettings();

        //                tempBestSpeed.BarcodeFormatIds = (int)this.mEmBarcodeFormat;
        //                tempBestSpeed.BarcodeFormatIds_2 = (int)this.mEmBarcodeFormat_2;

        //                tempBestSpeed.LocalizationModes[0] = EnumLocalizationMode.LM_SCAN_DIRECTLY;

        //                for (int i = 1; i < tempBestSpeed.LocalizationModes.Length; i++)
        //                    tempBestSpeed.LocalizationModes[i] = EnumLocalizationMode.LM_SKIP;

        //                tempBestSpeed.DeblurLevel = 3;
        //                tempBestSpeed.ExpectedBarcodesCount = 0;
        //                tempBestSpeed.ScaleDownThreshold = 2300;

        //                for (int i = 0; i < tempBestSpeed.FurtherModes.TextFilterModes.Length; i++)
        //                    tempBestSpeed.FurtherModes.TextFilterModes[i] = EnumTextFilterMode.TFM_SKIP;

        //                oBarcodeRdr.UpdateRuntimeSettings(tempBestSpeed);
        //                break;
        //            case 1:
        //                PublicRuntimeSettings tempBalance = oBarcodeRdr.GetRuntimeSettings();

        //                tempBalance.BarcodeFormatIds = (int)this.mEmBarcodeFormat;
        //                tempBalance.BarcodeFormatIds_2 = (int)this.mEmBarcodeFormat_2;

        //                tempBalance.LocalizationModes[0] = EnumLocalizationMode.LM_CONNECTED_BLOCKS;
        //                tempBalance.LocalizationModes[1] = EnumLocalizationMode.LM_SCAN_DIRECTLY;

        //                for (int i = 2; i < tempBalance.LocalizationModes.Length; i++)
        //                    tempBalance.LocalizationModes[i] = EnumLocalizationMode.LM_SKIP;

        //                tempBalance.DeblurLevel = 5;
        //                tempBalance.ExpectedBarcodesCount = 512;
        //                tempBalance.ScaleDownThreshold = 2300;
        //                tempBalance.FurtherModes.TextFilterModes[0] = EnumTextFilterMode.TFM_GENERAL_CONTOUR;

        //                for (int i = 1; i < tempBalance.FurtherModes.TextFilterModes.Length; i++)
        //                    tempBalance.FurtherModes.TextFilterModes[i] = EnumTextFilterMode.TFM_SKIP;

        //                oBarcodeRdr.UpdateRuntimeSettings(tempBalance);
        //                break;
        //            case 2:
        //                PublicRuntimeSettings tempCoverage = oBarcodeRdr.GetRuntimeSettings();

        //                tempCoverage.BarcodeFormatIds = (int)this.mEmBarcodeFormat;
        //                tempCoverage.BarcodeFormatIds_2 = (int)this.mEmBarcodeFormat_2;

        //                // use default value of LocalizationModes
        //                tempCoverage.DeblurLevel = 9;
        //                tempCoverage.ExpectedBarcodesCount = 512;
        //                tempCoverage.ScaleDownThreshold = 214748347;
        //                tempCoverage.FurtherModes.TextFilterModes[0] = EnumTextFilterMode.TFM_GENERAL_CONTOUR;
                        
        //                for (int i = 1; i < tempCoverage.FurtherModes.TextFilterModes.Length; i++)
        //                    tempCoverage.FurtherModes.TextFilterModes[i] = EnumTextFilterMode.TFM_SKIP;
                        
        //                tempCoverage.FurtherModes.GrayscaleTransformationModes[0] = EnumGrayscaleTransformationMode.GTM_ORIGINAL;
        //                tempCoverage.FurtherModes.GrayscaleTransformationModes[1] = EnumGrayscaleTransformationMode.GTM_INVERTED;
                        
        //                for (int i = 2; i < tempCoverage.FurtherModes.GrayscaleTransformationModes.Length; i++)
        //                    tempCoverage.FurtherModes.GrayscaleTransformationModes[i] = EnumGrayscaleTransformationMode.GTM_SKIP;
                        
        //                oBarcodeRdr.UpdateRuntimeSettings(tempCoverage);
        //                break;
        //        }
        //    }
        //}

        private bool ReadFromImageBarcode(System.Drawing.Bitmap pBmp)
        {
            bool isDocSep = false;
            //System.Drawing.Bitmap bmp = null;

            try
            {
                //MessageBox.Show(oImgCore.ImageBuffer.HowManyImagesInBuffer.ToString());
                //ShowSelectedImageArea(); //Uncomment for Dynamsoft Barcode only.

                //if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer >= 0)
                if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                {

                    //updateRuntimeSettingsWithUISetting();

                    //Bitmap bmp = (Bitmap)(_oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer));

                    //TextResult[] textResults = oBarcodeRdr.DecodeBitmap(bmp, "");   

                    //if (textResults.Length > 0)
                    //{
                    //    int i = 0;

                    //    while (i < textResults.Length)
                    //    {
                    //        _docType = textResults[i].BarcodeText;                            

                    //        i += 1;
                    //    }
                    //    isDocSep = true;
                    //}
                    //this.ShowResultOnImage(bmp, textResults);

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
                        CropArea = new System.Drawing.Rectangle(),

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

                    System.Threading.Tasks.Task<BarcodeResults> textResults = IronBarCode.BarcodeReader.ReadAsync(pBmp, oOptions);
                    BarcodeResult[] results = textResults.Result.ToArray();

                    if (results.Length > 0)
                    {
                        int i = 0;
                        //_docType = "";

                        while (i < results.Length)
                        {
                            _docType = results[i].Text;

                            i += 1;
                        }
                        isDocSep = true;
                    }

                    //textResults = null;
                    results = null;
                    textResults.Dispose();

                    //this.ShowResultOnImage(bmp, textResults);
                    //bmp.Dispose();
                    //aByte = null;
                }

            }
            catch (Exception exp)
            {
                mGlobal.Write2Log(exp.Message);
                //MessageBox.Show(exp.Message, "Decoding error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return isDocSep;
        }

        private void ShowResultOnImage(Bitmap bitmap, TextResult[] textResults)
        {
            _oImgCore.ImageBuffer.SetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation, null, true);
            if (textResults != null)
            {
                List<AnnotationData> tempListAnnotation = new List<AnnotationData>();
                int nTextResultIndex = 0;
                for (var i = 0; i < textResults.Length; i++)
                {
                    var penColor = System.Drawing.Color.Red;
                    TextResult result = textResults[i];

                    var rectAnnotation = new AnnotationData();
                    rectAnnotation.AnnotationType = AnnotationType.enumRectangle;
                    System.Drawing.Rectangle boundingrect = ConvertLocationPointToRect(result.LocalizationResult.ResultPoints);
                    rectAnnotation.StartPoint = new System.Drawing.Point(boundingrect.Left, boundingrect.Top);
                    rectAnnotation.EndPoint = new System.Drawing.Point((boundingrect.Left + boundingrect.Size.Width), (boundingrect.Top + boundingrect.Size.Height));
                    rectAnnotation.FillColor = System.Drawing.Color.Transparent.ToArgb();
                    rectAnnotation.PenColor = penColor.ToArgb();
                    rectAnnotation.PenWidth = 3;
                    rectAnnotation.GUID = Guid.NewGuid();

                    float fsize = bitmap.Width / 48.0f;
                    if (fsize < 25)
                        fsize = 25;

                    System.Drawing.Font textFont = new System.Drawing.Font("Times New Roman", fsize, System.Drawing.FontStyle.Bold);

                    string strNo = (result != null) ? "[" + (nTextResultIndex++ + 1) + "]" : "";
                    System.Drawing.SizeF textSize = Graphics.FromHwnd(IntPtr.Zero).MeasureString(strNo, textFont);

                    var textAnnotation = new AnnotationData();
                    textAnnotation.AnnotationType = AnnotationType.enumText;
                    textAnnotation.StartPoint = new System.Drawing.Point(boundingrect.Left, (int)(boundingrect.Top - textSize.Height * 1.25f));
                    textAnnotation.EndPoint = new System.Drawing.Point((textAnnotation.StartPoint.X + (int)textSize.Width * 2), (int)(textAnnotation.StartPoint.Y + textSize.Height * 1.25f));
                    if (textAnnotation.StartPoint.X < 0)
                    {
                        textAnnotation.EndPoint = new System.Drawing.Point((textAnnotation.EndPoint.X + textAnnotation.StartPoint.X), textAnnotation.EndPoint.Y);
                        textAnnotation.StartPoint = new System.Drawing.Point(0, textAnnotation.StartPoint.Y);
                    }
                    if (textAnnotation.StartPoint.Y < 0)
                    {
                        textAnnotation.EndPoint = new System.Drawing.Point(textAnnotation.EndPoint.X, (textAnnotation.EndPoint.Y - textAnnotation.StartPoint.Y));
                        textAnnotation.StartPoint = new System.Drawing.Point(textAnnotation.StartPoint.X, 0);
                    }

                    textAnnotation.TextContent = strNo;
                    AnnoTextFont tempFont = new AnnoTextFont();
                    tempFont.TextColor = System.Drawing.Color.Red.ToArgb();
                    tempFont.Size = (int)fsize;
                    tempFont.Name = "Times New Roman";
                    textAnnotation.FontType = tempFont;
                    textAnnotation.GUID = Guid.NewGuid();

                    tempListAnnotation.Add(rectAnnotation);
                    tempListAnnotation.Add(textAnnotation);
                }
                _oImgCore.ImageBuffer.SetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation, tempListAnnotation, true);
            }
        }

        private System.Drawing.Rectangle ConvertLocationPointToRect(System.Drawing.Point[] points)
        {
            int left = points[0].X, top = points[0].Y, right = points[1].X, bottom = points[1].Y;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].X < left)
                {
                    left = points[i].X;
                }

                if (points[i].X > right)
                {
                    right = points[i].X;
                }

                if (points[i].Y < top)
                {
                    top = points[i].Y;
                }

                if (points[i].Y > bottom)
                {
                    bottom = points[i].Y;
                }
            }
            System.Drawing.Rectangle temp = new System.Drawing.Rectangle(left, top, (right - left), (bottom - top));
            return temp;
        }

        private void CheckImageCount()
        {
            _currentImageIndex = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
            var currentIndex = _currentImageIndex + 1;
            int imageCount = _oImgCore.ImageBuffer.HowManyImagesInBuffer;
            if (imageCount == 0)
                currentIndex = 0;

            txtCurrImgIdx.Text = currentIndex.ToString();
            txtTotImgNum.Text = imageCount.ToString();

            if (imageCount > 0)
            {
                EnableAllFunctionButtons();
            }
            else
            {
                DisableAllFunctionButtons();
                if (sIsAddOn.Trim() == String.Empty)
                {
                    _oImgCore = new ImageCore();
                    _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;
                }

                dsvImg.Bind(new ImageCore());
            }

            if (imageCount > 1)
            {
                EnableControls(picboxFirst);
                EnableControls(picboxLast);
                EnableControls(picboxPrevious);
                EnableControls(picboxNext);

                if (currentIndex == 1)
                {
                    DisableControls(picboxPrevious);
                    DisableControls(picboxFirst);
                }
                if (currentIndex == imageCount)
                {
                    DisableControls(picboxNext);
                    DisableControls(picboxLast);
                }
            }
            else
            {
                DisableControls(picboxFirst);
                DisableControls(picboxLast);
                DisableControls(picboxPrevious);
                DisableControls(picboxNext);
            }

            //ShowSelectedImageArea();
        }

        private void ShowSelectedImageArea()
        {
            if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer >= 0)
            {
                short imgIdx = (short) (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                var recSelArea = dsvImg.GetSelectionRect(imgIdx);
                var imgCurrent = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            }
        }

        #region controls settings
        private void DisableAllFunctionButtons()
        {
            DisableControls(picboxFirst);
            DisableControls(picboxPrevious);
            DisableControls(picboxNext);
            DisableControls(picboxLast);
        }

        private void EnableAllFunctionButtons()
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 1)
            {
                EnableControls(picboxFirst);
                EnableControls(picboxPrevious);
                EnableControls(picboxNext);
                EnableControls(picboxLast);

                if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer == 0)
                {
                    DisableControls(picboxPrevious);
                    DisableControls(picboxFirst);
                }
                if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer + 1 == _oImgCore.ImageBuffer.HowManyImagesInBuffer)
                {
                    DisableControls(picboxNext);
                    DisableControls(picboxLast);
                }
            }

            //CheckZoom();
        }

        private void DisableControls(object sender)
        {
            DisableControls(sender, string.Empty);
        }

        private void DisableControls(object sender, string suffix)
        {
            if (string.IsNullOrEmpty(suffix)) suffix = "_Disabled";

            if (sender is PictureBox)
            {
                 (sender as PictureBox).Enabled = false;
            }
            else
            {
                var control = sender as Control;
                if (control != null) control.Enabled = false;
            }
        }

        private static void EnableControls(object sender)
        {
            if (sender is PictureBox)
            {                
                (sender as PictureBox).Enabled = true;
            }
            else
            {
                var control = sender as Control;
                if (control != null) control.Enabled = true;
            }
        }
        #endregion

        #region ISave Callback
        public object GetAnnotations(int iPageNumber)
        {
            if (_isMultiPage == true)
            {
                return _oImgCore.ImageBuffer.GetMetaData((short)iPageNumber, EnumMetaDataType.enumAnnotation);
            }
            else
            {
                return _oImgCore.ImageBuffer.GetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation);
            }
        }

        public Bitmap GetImage(int iPageNumber)
        {
            if (_isMultiPage == true)
            {
                return _oImgCore.ImageBuffer.GetBitmap((short)iPageNumber);
            }
            else
            {
                return _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            }
        }

        public int GetPageCount()
        {
            if (_isMultiPage == true)
                return _oImgCore.ImageBuffer.HowManyImagesInBuffer;
            else
                return 1;
        }
        #endregion ISave Callback

        #region AcquireImage Callback
        public void OnPostAllTransfers()
        {
            try
            {
                CrossThreadOperationControl crossDelegate = delegate ()
                {
                    ScanStatusBar.Text = "Processing";

                    dsvImg.Visible = true;
                    CheckImageCount();

                    try
                    {
                        //_batchCode = _appNum + "_" + _collaNum;
                        //_scanEnd = DateTime.Now;

                        _iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, _setNum, "'4'");
                        if (_fromProc.Trim().ToLower() == "verify") //Rescan request from Verify process.
                            loadRejectedImagesFrmDisk(_iCurrBatchRowid, false);
                        else
                            loadRejectedImagesFrmDisk(_iCurrBatchRowid, false);

                        if (this.sIsAddOn.Trim() != "")
                        {
                            if (sIsAddOn.Trim().ToLower() == "rejected")
                            {
                                ScanStatusBar1.Text = "Rejected Addon";
                                //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                                loadRejectedBatchSetTreeview();
                            }
                            else
                            {
                                if (_fromProc.Trim().ToLower() == "verify") //From Verify rescan request.
                                                                            //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                                    loadRejectedBatchSetTreeview();
                                else
                                    loadBatchSetRescanTreeview(_fromProc); //loadBatchSetRescanTreeview();
                            }

                            if (tvwBatch.Nodes.Count > 0)
                                tvwBatch.ExpandAll();
                        }
                        else
                        {
                            //loadBatchTreeview(_scanStart, _scanEnd);
                            if (_fromProc.Trim().ToLower() == "verify") //From Verify rescan request.
                                                                        //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                                loadRejectedBatchSetTreeview();
                            else
                                loadBatchSetRescanTreeview(_fromProc); //loadBatchSetRescanTreeview();

                            if (tvwBatch.Nodes.Count > 0)
                                tvwBatch.ExpandAll();

                            //setImgInfo(0);
                        }

                        //dsvThumbnailList.Bind(_oImgCore);

                        //_scanEnd = DateTime.Now; //For treeview last end date.
                        //mGlobal.Write2Log("Post:" + _scanEnd.ToString("HH:mm:ss"));
                    }
                    catch (Exception ex)
                    {
                        mGlobal.Write2Log(ex.Message);
                    }

                    timerProgress.Enabled = false;
                    ScanProgressBar.Value = 0;
                    ScanStatusBar.Text = "Ready";

                    //Init.>>
                    sCurrDocType = _docType;
                    sCurrSetNum = _setNum;

                    if (_batchType.Trim().ToLower() == "set" && _docType.Trim() != _setNum.Trim())
                        _docType = _setNum;

                    sLastDocType = _docType;
                    sCurrDocType = sLastDocType;
                    this._isDocSep = true;
                    if (_batchType.Trim().ToLower() != "set")
                        this._docType = "";

                    bIsBlankPage = false;
                    bMultipleBarcodeOCR = false;
                    //>>

                    ScanStatusBar.Text = "Processing";

                    //btnScan.Enabled = true;
                    enableScan(true);

                    if (mGlobal.checkOpenedForms("frmNotification"))
                        MessageBox.Show("The Notification screen already opened!", "Message");
                    else
                    {
                        frmNotification fmNotify = new frmNotification();
                        fmNotify.Show(this);
                        fmNotify.validateTreeview(_batchCode, lScanImg);
                        fmNotify.loadNotification(_batchCode);
                        fmNotify.Focus();

                        btnNoticeStrip.Enabled = true;
                    }
                };

                Invoke(crossDelegate);

                Task.Run(() => RemoveDuplexHeaderBackPage()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                btnCloseStrip.Enabled = true;
                mGlobal.Write2Log("Post all.." + ex.Message);
                mGlobal.Write2Log("Post all.." + ex.StackTrace.ToString());
            }
        }

        public bool OnPostTransfer(Bitmap bit, string info)
        {
            ScanStatusBar.Text = "Processing";

            try
            {
                var oImgInfo = clsImgInfo.FromJson(info);

                if (oImgInfo.ExtendedImageInfo.Others.TweiPageside == 2)
                    sFrontBack = "B";
                else
                    sFrontBack = "F";

                //mGlobal.Write2Log(info);
                //_oImgCore.IO.LoadImage(bit);
                //dsvImg.Bind(_oImgCore);

                //MessageBox.Show(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer.ToString());            
                if (_noSeparator.Trim() == "0")
                {
                    _oImgCore.IO.LoadImage(bit);
                    if (sRmvBlank > 0)
                    {
                        if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer > 0) //Not first page.
                        {
                            bIsBlankPage = CheckBlankPage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                            if (bIsBlankPage)
                            {
                                int idx = lHeaderIdx.FindIndex(f => f.Equals(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer));
                                if (idx == -1)
                                {
                                    lHeaderIdx.Add(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                                }
                            }
                        }

                        if (bIsBlankPage) //Skip the scanning process and goto next page.
                        {
                            throw new Exception("Blank page removed!");
                        }
                    }
                    _isDocSep = false;
                    _sepCnt = 0; //always zero for zero separator.
                }
                else
                {
                    if (bIsDuplexEnabled)
                    {
                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer == 0)
                        {
                            _oImgCore.IO.LoadImage(bit);
                            //mGlobal.Write2Log(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer.ToString() + "..." + sFrontBack + "." + _docType);
                        }
                    }
                    else
                    {
                        _oImgCore.IO.LoadImage(bit);
                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 1) //Not first page.
                        {
                            bIsBlankPage = CheckBlankPage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                            if (bIsBlankPage)
                            {
                                int idx = lHeaderIdx.FindIndex(f => f.Equals(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer));
                                if (idx == -1)
                                {
                                    lHeaderIdx.Add(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                                }
                            }
                        }
                    }

                    if (bIsBlankPage) //Skip the scanning process and goto next page.
                    {
                        throw new Exception("Blank page removed!");
                    }

                    if (_noSeparator.Trim() == "C")
                    {
                        _docType = "Document"; //default.
                        _isDocSep = idpFromImageClassify(bit);
                    }
                    else
                    {
                        _isDocSep = ReadFromImageBarcode(bit);
                    }

                    //if (bDocAutoOCR)
                    //{
                    //mGlobal.Write2Log(_setNum + ":" + _sepCnt.ToString());
                    if (_noSeparator.Trim() == "1" && _isDocSep)
                    {
                        //mGlobal.Write2Log(_docType + ":" + sCurrDocType);
                        if (_docType.ToUpper().Trim() != staMain.stcProjCfg.SeparatorText)
                        {
                            _isDocSep = false;
                            _docType = "";
                        }
                    }
                    else if (_noSeparator.Trim() == "2" && _isDocSep)
                    {
                        //mGlobal.Write2Log(_docType + ":" + sCurrDocType);
                        if (_docType.ToUpper().Trim() == staMain.stcProjCfg.SeparatorText)
                        {
                            _sepCnt = 0;
                        }
                        else if (_sepCnt > 1)
                        {
                            _isDocSep = true; //false; //true for the continue of same doc type scanning, but will cause duplicate doc type notification.
                            //_docType = sCurrDocType; //Last doc. type. Commented for the continue of same doc type scanning, but will cause duplicate doc type notification.
                        }
                        else if (_sepCnt == 1 && _docType.ToUpper().Trim() != staMain.stcProjCfg.SeparatorText)
                        {
                            sCurrDocType = _docType; //Current DocType is scanned doctype.
                        }
                    }
                    //mGlobal.Write2Log("Sep?" + _isDocSep.ToString() + ",Current Doc.Type:" + sCurrDocType);
                    //}
                    //else //Normal scanning
                    //{
                    //    mGlobal.Write2Log(_setNum + ":" + _sepCnt.ToString());
                    //    mGlobal.Write2Log(_docType + ":" + sCurrDocType);
                    //    if (_noSeparator.Trim() == "1" && _sepCnt > 0 && _docType.Trim() != staMain.stcProjCfg.SeparatorText)
                    //    {
                    //        _isDocSep = false;
                    //    }
                    //    else if (_noSeparator.Trim() == "2" && _sepCnt > 1 && _docType.Trim() != staMain.stcProjCfg.SeparatorText)
                    //    {
                    //        _isDocSep = false;
                    //        _docType = sCurrDocType; //Last doc. type.
                    //    }
                    //    mGlobal.Write2Log("Sep?" + _isDocSep.ToString() + ",Current Doc.Type:" + sCurrDocType);
                    //}

                    if (bIsDuplexEnabled)
                    {
                        if (_docType.ToUpper().Trim() == "" && _oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                        {
                            _oImgCore.IO.LoadImage(bit);
                            //mGlobal.Write2Log(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer.ToString() + ".." + sFrontBack);
                        }
                        else
                        {
                            if (_isDocSep && sFrontBack == "F")
                            {
                                sFrontBack = "H";
                                if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer == 0)
                                    lHeaderIdx.Add((short)(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer + 1));
                                else
                                {
                                    _oImgCore.IO.LoadImage(bit);
                                    //mGlobal.Write2Log(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer.ToString() + "..." + sFrontBack + ".." + _docType);
                                    lHeaderIdx.Add((short)(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer + 1));
                                }
                            }
                            else if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                            {
                                _oImgCore.IO.LoadImage(bit);
                                //mGlobal.Write2Log(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer.ToString() + "..." + sFrontBack + "." + _docType);
                            }
                        }

                        short sCurIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                        //if (sRmvBlank > 0 && _oImgCore.ImageBuffer.CurrentImageIndexInBuffer > 0) //Not first page.
                        if (sRmvBlank > 0)
                        {
                            bIsBlankPage = CheckBlankPage(sCurIdx);

                            if (bIsBlankPage)
                            {
                                int idx = lHeaderIdx.FindIndex(f => f.Equals(sCurIdx));
                                if (idx == -1)
                                {
                                    lHeaderIdx.Add(sCurIdx);
                                }
                            }
                        }

                        if (bIsBlankPage) //Skip the scanning process and goto next page.
                        {
                            throw new Exception(sCurIdx + " Blank page removed!");
                        }
                    }

                    //mGlobal.Write2Log("Sep?" + _isDocSep.ToString() + ",Page:" + sFrontBack);

                    if (bIsDuplexEnabled)
                    {
                        if (_noSeparator.Trim() == "1")
                        {
                            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer == 1)
                                sErrMsg = validateScan();
                        }
                        else if (_noSeparator.Trim() == "2")
                        {
                            if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer == 0)
                                sErrMsg = validateScan();
                        }
                    }
                    else
                    {
                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer == 1)
                            sErrMsg = validateScan();
                    }                        

                    if (sErrMsg.Trim() != "")
                    {
                        _oImgCore.ImageBuffer.RemoveAllImages();
                        tvwBatch.Nodes.Clear();
                        _isDocSep = true; //Init.
                        _sepCnt = 0;
                        iAutoBarCnt = 0;
                        _docType = ""; //Init.
                        iSetNum = 1;
                        this.OnTransferCancelled();
                        return false;
                    }
                }

                string sDir = "";
                string sFileName = "";
                string sFileExt = "tiff";
                string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (sIsAddOn.Trim() == "")
                {
                    if (staMain.stcProjCfg.BatchAuto.Trim().ToUpper() == "Y")
                    {
                        //if (_batchType.Trim().ToLower() == "set")
                        //    _batchCode = _appNum + "_" + oDF.getFormatBatchNum(_setNum, DateTime.Now);
                        //else
                        //    _batchCode = _appNum + "_" + oDF.getFormatBatchNum(_batchNum, DateTime.Now);
                        _batchCode = staMain.getBatchCodeByProj(_batchType, _appNum, _setNum, _batchNum);
                    }
                    else if (_stcImgInfo.BatchCode != null)
                        _batchCode = _stcImgInfo.BatchCode.Trim();

                    if (_batchCode.Trim() == string.Empty)
                    {
                        sErrMsg = "Post..batch code is blank in box rescan.";
                        mGlobal.Write2Log(sErrMsg);
                        this.OnTransferCancelled();
                        return false;
                    }
                }

                if (_isDocSep == false)
                {
                    if (iSetNum == 0)
                    {
                        iSetNum = 1;
                        _setNum = iSetNum.ToString(sSetNumFmt);
                    }

                    if (_docType.Trim() == "") _docType = sCurrDocType; //Last doc. type.

                    string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");

                    if (_batchType.Trim().ToLower() == "set")
                    {
                        if (_fromProc.Trim().ToLower() == "verify")
                        {
                            sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            //if (_docType.Trim() == "")
                            //{
                            //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            //}
                            //else
                            //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }
                        else
                        {
                            sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            //if (_docType.Trim() == "")
                            //{
                            //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            //}
                            //else
                            //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }
                    } 
                    else //Batch.
                    {
                        if (_fromProc.Trim().ToLower() == "verify")
                        {
                            if (_docType.Trim() == "")
                            {
                                sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            }
                            else
                                sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }
                        else
                        {
                            if (_docType.Trim() == "")
                            {
                                sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            }
                            else
                                sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }
                    }                    

                    sFileName = sBatchPath.Trim() + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;

                    //if (!bIsDuplexEnabled)
                    if (sFrontBack == "F" || sFrontBack == "B")
                    {
                        CrossThreadOperationDB crossDelegateDB = delegate ()
                        {
                            if (saveImageToFile(sDir, sFileName, sFileExt.ToUpper()))
                            {
                                _scanEnd = DateTime.Now;

                                if (this.sIsAddOn.Trim().ToLower() == "rejected") //Rejected index addon..
                                {
                                    //if (_fromProc.Trim().ToLower() == "verify")
                                    //{
                                    //    if (_batchType.Trim().ToLower() == "set")
                                    //        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                    //    else
                                    //        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                    //}
                                    //else
                                    //{
                                    //    if (_batchType.Trim().ToLower() == "set")
                                    //        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                                    //    else
                                    //        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                    //}
                                    if (_batchType.Trim().ToLower() == "set")
                                        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                    else
                                        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);

                                    if (_batchType.Trim().ToLower() == "set")
                                        _totPageCnt = _totPageCnt + oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                                    else
                                        _totPageCnt = _totPageCnt + oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);

                                    _totPageCnt = _totPageCnt + 1;

                                    if (_batchType.Trim().ToLower() == "set")
                                    {
                                        updateBatchDB(_batchCode, "4", "4", "", _setNum);
                                    }
                                    else
                                        updateBatchDB(_batchCode, "4", "4");

                                    _scanDEnd = DateTime.Now;

                                    if (oDF.checkDocuIndexExist(_currScanProj, _appNum, _batchCode, _setNum, _docType))
                                    {
                                        this.iCollaNum = oDF.getCurrCollaNumIndex(_currScanProj, _appNum, _batchCode, _setNum, _docType);
                                        _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                                    }

                                    if (Convert.ToInt32(_collaNum) == 0)
                                    {
                                        iCollaNum = 1;
                                        _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                                    }

                                    iImgSeq = oDF.getLastDocTypeImageSeqDB(_currScanProj, _appNum, _setNum, _docType, _batchCode);
                                    iImgSeq += 1;

                                    //mGlobal.Write2Log(_imgFilename);
                                    saveDocIndexDB("R", "R", _imgFilename);
                                }
                                else
                                {
                                    //if (_fromProc.Trim().ToLower() == "verify")
                                    //{
                                    //    if (_batchType.Trim().ToLower() == "set")
                                    //        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                    //    else
                                    //        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                    //}
                                    //else
                                    //{
                                    //    if (_batchType.Trim().ToLower() == "set")
                                    //        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                                    //    else
                                    //        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                    //}
                                    if (_batchType.Trim().ToLower() == "set")
                                        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                    else
                                        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);

                                    if (_batchType.Trim().ToLower() == "set")
                                        _totPageCnt = _totPageCnt + oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                                    else
                                        _totPageCnt = _totPageCnt + oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);

                                    _totPageCnt = _totPageCnt + 1;
                                    sModifiedDate = _scanEnd.ToString("yyyy-MM-dd HH:mm:ss");

                                    if (_fromProc.Trim().ToLower() == "verify") //Rescan request from Verify process. 
                                    {
                                        string sSetNo = "";
                                        if (_batchType.Trim().ToLower() == "set")
                                            sSetNo = _setNum;

                                        //updateBatchDB(_batchCode, _batchStatus, "4", sModifiedDate);
                                        if (_noSeparator.Trim() == "C")
                                        {
                                            if (_totPageCnt == 1)
                                            {
                                                sCurrDocType = _docType;
                                                _docType = staMain.stcProjCfg.SeparatorText;
                                                saveBatchDB();
                                                _docType = sCurrDocType;
                                            }
                                            else
                                                updateBatchDB(_batchCode, _batchStatus, _batchStatus, _setNum);
                                        }
                                        else if (_noSeparator.Trim() == "0")
                                        {
                                            if (_totPageCnt == 1)
                                            {
                                                _docType = staMain.stcProjCfg.SeparatorText;
                                                saveBatchDB();
                                                _docType = "";
                                            }
                                            else
                                                updateBatchDB(_batchCode, _batchStatus, "4", sModifiedDate, sSetNo);
                                        }
                                        else
                                            updateBatchDB(_batchCode, _batchStatus, "4", sModifiedDate, sSetNo);

                                        _scanDEnd = DateTime.Now;

                                        if (oDF.checkDocuIndexExist(_currScanProj, _appNum, _batchCode, _setNum, _docType))
                                        {
                                            this.iCollaNum = oDF.getCurrCollaNumIndex(_currScanProj, _appNum, _batchCode, _setNum, _docType);
                                            _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                                        }

                                        if (Convert.ToInt32(_collaNum) == 0)
                                        {
                                            iCollaNum = 1;
                                            _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                                        }

                                        iImgSeq = oDF.getLastDocTypeImageSeqDB(_currScanProj, _appNum, _setNum, _docType, _batchCode);
                                        iImgSeq += 1;

                                        saveRescanDocIndexDB(_scanStatus, "R", _imgFilename);
                                    }
                                    else //Rescan request from Index process. 
                                    {
                                        string sSetNo = "";
                                        if (_batchType.Trim().ToLower() == "set")
                                            sSetNo = _setNum;

                                        //updateBatchDB(_batchCode, _batchStatus, "4", sModifiedDate);
                                        if (_noSeparator.Trim() == "C")
                                        {
                                            if (_totPageCnt == 1)
                                            {
                                                sCurrDocType = _docType;
                                                _docType = staMain.stcProjCfg.SeparatorText;
                                                saveBatchDB();
                                                _docType = sCurrDocType;
                                            }
                                            else
                                                updateBatchDB(_batchCode, _batchStatus, _batchStatus, _setNum);
                                        }
                                        else if (_noSeparator.Trim() == "0")
                                        {
                                            if (_totPageCnt == 1)
                                            {
                                                _docType = staMain.stcProjCfg.SeparatorText;
                                                saveBatchDB();
                                                _docType = "";
                                            }
                                            else
                                                updateBatchDB(_batchCode, _batchStatus, "4", sModifiedDate, sSetNo);
                                        }
                                        else
                                            updateBatchDB(_batchCode, _batchStatus, "4", sModifiedDate, sSetNo);

                                        _scanDEnd = DateTime.Now;

                                        if (_batchType.Trim().ToLower() == "set")
                                            _docType = _setNum;

                                        if (oDF.checkDocuScanExist(_currScanProj, _appNum, _batchCode, _setNum, _docType))
                                        {
                                            this.iCollaNum = oDF.getCurrCollaNum(_currScanProj, _appNum, _batchCode, _setNum, _docType);
                                            _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                                        }

                                        if (Convert.ToInt32(_collaNum) == 0)
                                        {
                                            iCollaNum = 1;
                                            _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                                        }

                                        iImgSeq = oDF.getLastDocTypeImageSeqDB(_currScanProj, _appNum, _setNum, _docType, _batchCode);
                                        iImgSeq += 1;

                                        saveDocScanDB("R");
                                    }

                                    string sOCRFilename = "";
                                    if (mGlobal.verifyFileName(sFileName)) //Most current filename in case of IronOCR Exception..One or more errors occurred.
                                    {
                                        sOCRFilename = mGlobal.addDirSep(sDir) + sFileName;

                                        //1 or 2 separator Only first page will be auto read barcode and save in TIndexFieldValue table.
                                        if (lAutoBarcode.Count > 0 && iAutoBarCnt == 0 && iImgSeq == 1)
                                        {
                                            //mGlobal.Write2Log(_setNum + "::" + sCurrSetNum);
                                            Thread ocrThread = new Thread(new ThreadStart(() => AutoReadBarcodeAndSave(_setNum, iImgSeq, sOCRFilename)));
                                            ocrThread.IsBackground = true;
                                            ocrThread.Start();
                                        }
                                    }
                                    else
                                        mGlobal.Write2Log("Invalid filename.." + sFileName);

                                    //1 or 2 separator Only first page will be OCR and save in TIndexFieldValue table.
                                    if (bDocAutoOCR && (_noSeparator.Trim() == "0" || ((_noSeparator.Trim() == "1" || _noSeparator.Trim() == "2")
                                        && iImgSeq == 1)))
                                    {
                                        //mGlobal.Write2Log(_setNum + "::" + sCurrSetNum);
                                        Thread ocrThread = new Thread(new ThreadStart(() => OCRAndSave(_setNum, iImgSeq)));
                                        ocrThread.IsBackground = true;
                                        ocrThread.Start();
                                    }
                                }

                                lScanImg.Add(_imgFilename);
                            }
                            else
                                mGlobal.Write2Log("Save file failed.." + sFileName);
                        };

                        if (bIsDuplexEnabled == false)
                            Invoke(crossDelegateDB);
                        else
                        {
                            //if (sRmvBlank == 0) //Remove blank page option is disabled.
                            //{
                            //    int iHIdx = lHeaderIdx.FindIndex(i => i.Equals(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer));
                            //    if (iHIdx == -1)
                            //        Invoke(crossDelegateDB);
                            //}
                            //else
                            //    Invoke(crossDelegateDB);
                            int iHIdx = lHeaderIdx.FindIndex(i => i.Equals(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer));
                            if (iHIdx == -1)
                                Invoke(crossDelegateDB);
                        }
                    }                    
                }
                else //is separator.
                {
                    //if (!bIsDuplexEnabled)
                    if (sFrontBack == "F" || sFrontBack == "H")
                    {
                        CrossThreadOperationDB crossDelegateDBSep = delegate ()
                        {
                            if (_docType.Trim().ToUpper() == staMain.stcProjCfg.SeparatorText)
                            {
                                if (tvwBatch.Nodes.Count == 0) //load for once only due to increase performance.
                                {
                                    //loadBatchTreeview(_scanStart, _scanEnd);
                                    if (_fromProc.Trim().ToLower() == "verify") //From Verify rescan request.
                                        //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                                        loadRejectedBatchSetTreeview();
                                    else
                                        loadBatchSetRescanTreeview(_fromProc); //loadBatchSetRescanTreeview();
                                }
                                //_totPageCnt = 0;

                                if (_batchType.Trim().ToLower() == "set")
                                {
                                    if (_noSeparator.Trim() == "1") //2 separators is not possible for SET batch type.
                                        iSetNum = iSetNum + 1;  //Increase document set number else remain the set number.
                                }
                                else
                                    iSetNum += 1;  //Increase document set number.

                                if (iSetNum == 0) iSetNum = 1;
                                _setNum = iSetNum.ToString(sSetNumFmt);

                                //mGlobal.Write2Log(_setNum + ".." + _sepCnt.ToString());
                                if (iSetNum == 1) //is start first set separator.
                                {
                                    _sepCnt = 1; //One separator counter for batch header.
                                    iAutoBarCnt = 0; //Init auto barcode count.
                                }
                                else if (_setNum != sCurrSetNum)
                                {
                                    _sepCnt = 1; //Init separator counter for batch header.
                                    iAutoBarCnt = 0; //Init auto barcode count.
                                }

                                iCollaNum = 0;  //Reset document collaction number.
                                _collaNum = iCollaNum.ToString(sCollaNumFmt);

                                //if (_fromProc.Trim().ToLower() == "verify") //From Verify rescan request.
                                //{
                                //    if (_batchType.Trim().ToLower() == "set")
                                //        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                //    else
                                //        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                //}
                                //else
                                //{
                                //    if (_batchType.Trim().ToLower() == "set")
                                //        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                                //    else
                                //        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                //}
                                if (_batchType.Trim().ToLower() == "set")
                                    _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                else
                                    _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);

                                if (_batchType.Trim().ToLower() == "set")
                                    _totPageCnt = _totPageCnt + oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                                else
                                    _totPageCnt = _totPageCnt + oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);

                                _scanEnd = DateTime.Now;

                                saveBatchDB(); //for new set if any.

                                lScanImg.Add(_docType);

                                //Init for new set in the same tray at the same time.>>
                                if (_batchType.Trim().ToLower() == "set")
                                    _docType = _setNum;
                                else
                                    _docType = "";

                                _isDocSep = true;
                                //>>
                            }
                            else //Not Batch Header.
                            {
                                //if (_noSeparator.Trim() == "2")
                                //{
                                _sepCnt += 1; //Increase separator counter for non batch header per set.
                                              //}

                                //if (_fromProc.Trim().ToLower() == "verify") //From Verify rescan request.
                                //{
                                //    if (_batchType.Trim().ToLower() == "set")
                                //        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                //    else
                                //        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                //}
                                //else
                                //{
                                //    if (_batchType.Trim().ToLower() == "set")
                                //        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                                //    else
                                //        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                //}
                                if (_batchType.Trim().ToLower() == "set")
                                    _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                else
                                    _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);

                                if (_batchType.Trim().ToLower() == "set")
                                    _totPageCnt = _totPageCnt + oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                                else
                                    _totPageCnt = _totPageCnt + oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);

                                if (tvwBatch.Nodes.Count == 0 && _docType.Trim().ToUpper() != staMain.stcProjCfg.SeparatorText) //Create first set automatically.
                                {
                                    //loadBatchTreeview(_scanStart, _scanEnd);
                                    if (_fromProc.Trim().ToLower() == "verify") //From Verify rescan request.
                                        //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                                        loadRejectedBatchSetTreeview();
                                    else
                                        loadBatchSetRescanTreeview(_fromProc); //loadBatchSetRescanTreeview();

                                    _totPageCnt = 0;

                                    if (tvwBatch.Nodes.Count == 0)
                                    {
                                        iSetNum = 1;  //Reset document set number.
                                        _setNum = iSetNum.ToString(sSetNumFmt);
                                    }

                                    _scanEnd = DateTime.Now;

                                    sCurrDocType = _docType;
                                    _docType = staMain.stcProjCfg.SeparatorText;
                                    saveBatchDB();

                                    if (lScanImg.Count == 1) //Check and not to add duplicate BATCH HEADER in the list.
                                    {
                                        if (lScanImg[0] != _docType)
                                        {
                                            lScanImg.Add(_docType);
                                            //if (_noSeparator.Trim() == "2") //for rescan only.
                                            //    lScanImg.Add(sCurrDocType);
                                        }
                                        else
                                        {
                                            if (_noSeparator.Trim() == "2") //for rescan only.
                                            {
                                                //if (lScanImg.FindIndex(lScanImg.Count - 1, a => a.Contains(staMain.stcProjCfg.SeparatorText)) == -1)
                                                //    lScanImg.Add(_docType);
                                                lScanImg.Add(sCurrDocType);
                                            }
                                            else
                                                lScanImg.Add(sCurrDocType);
                                        }
                                    }
                                    else
                                    {
                                        if (_noSeparator.Trim() == "2") //for rescan only.
                                        {
                                            //if (lScanImg.FindIndex(lScanImg.Count - 1, a => a.Contains(staMain.stcProjCfg.SeparatorText)) == -1)
                                            //    lScanImg.Add(_docType);
                                            lScanImg.Add(sCurrDocType);
                                        }
                                        else
                                            //lScanImg.Add(_docType);
                                            lScanImg.Add(sCurrDocType);
                                    }

                                    _docType = sCurrDocType;
                                }
                                else if (_docType.Trim().ToUpper() != staMain.stcProjCfg.SeparatorText)
                                {
                                    //if (_noSeparator.Trim() == "2") //for rescan only.
                                    //{
                                    //    if (lScanImg.FindIndex(lScanImg.Count - 1, a => a.Contains(staMain.stcProjCfg.SeparatorText)) == -1)
                                    //        lScanImg.Add(staMain.stcProjCfg.SeparatorText);
                                    //    lScanImg.Add(_docType);
                                    //}
                                    //else
                                    lScanImg.Add(_docType);
                                }

                                if (_batchType.Trim().ToLower() == "set")
                                    _docType = _setNum;

                                sCurrDocType = _docType;

                                if (tvwBatch.SelectedNode != null)
                                {
                                    if ((_batchType.Trim().ToLower() == "batch" && tvwBatch.SelectedNode.Level == 1)
                                    || (_batchType.Trim().ToLower() == "box" && tvwBatch.SelectedNode.Level == 1)
                                    || (_batchType.Trim().ToLower() == "set" && tvwBatch.SelectedNode.Level == 0)) //Document set level node selected.
                                    {
                                        //iCollaNum += 1;  //Increase document collaboration number.
                                        //_collaNum = iCollaNum.ToString(sCollaNumFmt);
                                        if (_fromProc.Trim().ToLower() == "verify")
                                        {
                                            if (oDF.checkDocuIndexExist(_currScanProj, _appNum, _batchCode, _setNum, _docType))
                                            {
                                                if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                                    iCollaNum = oDF.getCurrCollaNumIndex(_currScanProj, _appNum, _batchCode, _setNum, _docType);
                                                else
                                                    iCollaNum = oDF.getLastCollaNumIndex(_currScanProj, _appNum, _batchCode, _setNum);

                                                _collaNum = iCollaNum.ToString(sCollaNumFmt);
                                            }
                                            else
                                            {
                                                iCollaNum += 1;  //Increase document collaboration number.
                                                _collaNum = iCollaNum.ToString(sCollaNumFmt);

                                                //iImgSeq = 0; //Init.
                                            }
                                        }
                                        else
                                        {
                                            if (oDF.checkDocuScanExist(_currScanProj, _appNum, _batchCode, _setNum, _docType))
                                            {
                                                iCollaNum = oDF.getLastCollaNum(_currScanProj, _appNum, _batchCode, _setNum);
                                                _collaNum = iCollaNum.ToString(sCollaNumFmt);
                                            }
                                            else
                                            {
                                                iCollaNum += 1;  //Increase document collaboration number.
                                                _collaNum = iCollaNum.ToString(sCollaNumFmt);

                                                //iImgSeq = 0; //Init.
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //iCollaNum += 1;  //Increase document collaboration number.
                                    //_collaNum = iCollaNum.ToString(sCollaNumFmt);
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        if (oDF.checkDocuIndexExist(_currScanProj, _appNum, _batchCode, _setNum, _docType))
                                        {
                                            iCollaNum = oDF.getLastCollaNumIndex(_currScanProj, _appNum, _batchCode, _setNum);
                                            _collaNum = iCollaNum.ToString(sCollaNumFmt);
                                        }
                                        else
                                        {
                                            iCollaNum += 1;  //Increase document collaboration number.
                                            _collaNum = iCollaNum.ToString(sCollaNumFmt);

                                            //iImgSeq = 0; //Init.
                                        }
                                    }
                                    else
                                    {
                                        if (oDF.checkDocuScanExist(_currScanProj, _appNum, _batchCode, _setNum, _docType))
                                        {
                                            iCollaNum = oDF.getLastCollaNum(_currScanProj, _appNum, _batchCode, _setNum);
                                            _collaNum = iCollaNum.ToString(sCollaNumFmt);
                                        }
                                        else
                                        {
                                            iCollaNum += 1;  //Increase document collaboration number.
                                            _collaNum = iCollaNum.ToString(sCollaNumFmt);

                                            //iImgSeq = 0; //Init.
                                        }
                                    }
                                }

                                //iImgSeq = 0; //Init.
                                if (_noSeparator.Trim().ToUpper() == "C" && sCurrDocType.Trim() != _docType.Trim())
                                {
                                    iSetNum += 1;
                                    _setNum = iSetNum.ToString(sSetNumFmt);
                                }

                                sCurrSetNum = _setNum;
                            }
                        };
                        Invoke(crossDelegateDBSep);
                    }
                    else //Skip the back page for Seperator.
                    {
                    }
                    
                }
            }
            catch (ImageCoreException icex)
            {
                mGlobal.Write2Log("Rescan Box Post Image.." + ScanStatusBar1.Text + ": " + icex.Message + Environment.NewLine 
                + icex.StackTrace.ToString());
                return false;
            }
            catch (Exception ex)
            {
                if (bIsBlankPage)
                {
                    //mGlobal.Write2Log(_docType + ".." + ex.Message);
                    return true;
                }
                else
                {
                    mGlobal.Write2Log("Rescan Box Post.." + ScanStatusBar1.Text + ": " + ex.Message + Environment.NewLine
                    + ex.StackTrace.ToString());
                    return false;
                }
            }            

            return true;
        }

        public void OnPreAllTransfers()
        {
            try
            {
                //Init.>>
                if (_fromProc.Trim().ToLower() == "verify")
                {
                    if (_batchType.Trim().ToLower() == "set")
                        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                    else
                        _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                }
                else
                {
                    if (_batchType.Trim().ToLower() == "set")
                        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                    else
                        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                }
                _isDocSep = true;
                _sepCnt = 0;
                iAutoBarCnt = 0;
                sErrMsg = "";
                bIsBlankPage = false;
                //>>        
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Pre all.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }

            //updateRuntimeSettingsWithUISetting();
        }

        public bool OnPreTransfer()
        {
            ScanStatusBar.Text = "Scanning";
            _scanDStart = DateTime.Now;
            _isDocSep = true;
            bIsBlankPage = false;
            //if (!bDocAutoOCR) _sepCnt = 0;

            return true;
        }

        public void OnSourceUIClose()
        {
            try
            {
                btnCloseStrip.Enabled = true;
                //btnScan.Enabled = true;
                enableScan(true);
                MDIMain.oTMgr.CloseSource();
                MDIMain.oTMgr.CloseSourceManager();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Close.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        public void OnTransferCancelled()
        {
            try
            {
                btnCloseStrip.Enabled = true;
                bCancel = true;
                timerProgress.Stop();
                timerProgress.Enabled = false;

                //ScanProgressBar.Value = 0; //Error!
                ScanStatusBar.Text = "Ready";

                if (sErrMsg.Trim() != "")
                {
                    MessageBox.Show(this, "Scan process stopped with error! " + Environment.NewLine + sErrMsg, "Process Cancel");
                    sErrMsg = "";
                }
                //else
                //    MessageBox.Show(sErrMsg, "Process Cancel");
                if (tvwBatch.Nodes.Count == 0)
                {
                    staMain.sessionRestart();
                }
            }
            catch (Exception)
            {
            }
        }

        public void OnTransferError()
        {
            try
            {
                btnCloseStrip.Enabled = true;
                ScanStatusBar.Text = "Error";
                timerProgress.Enabled = false;
                if (tvwBatch.Nodes.Count == 0)
                {
                    staMain.sessionStop(false);
                }
            }
            catch (Exception)
            {
            }
        }

        public bool IfGetImageInfo
        {
            get
            {
                return true;
            }
        }

        public bool IfGetExtImageInfo
        {
            get
            {
                return true;
            }
        }
        #endregion AcquireImage Callback

        private bool updateBatchDB(string pBatchCode, string pBatchStatus, string pNewBatchStatus, string pModifiedDate = "", string pSetNum = "")
        {
            bool bUpdated = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();
                
                if (mGlobal.objDB == null)
                {
                    mGlobal.objDB = new clsDatabase();
                }
                
                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "SET scanend='" + _scanEnd.ToString("yyyy-MM-dd HH:mm:ss") + "', ";
                sSQL += "totpagecnt=" + _totPageCnt.ToString() + ", ";
                sSQL += "batchstatus='" + pNewBatchStatus.Trim() + "', ";
                sSQL += "fromproc='" + _fromProc.Trim().Replace("'", "") + "' ";

                if (pModifiedDate.Trim() != "")
                    sSQL += ",modifieddate='" + pModifiedDate.Trim() + "' ";

                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != String.Empty)
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                sSQL += "AND batchstatus='" + pBatchStatus.Trim() + "' ";
                //sSQL += "AND scanstation='" + _scanStn.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanuser='" + _scanUsr.Trim().Replace("'", "") + "' ";

                //mGlobal.Write2Log(sSQL);

                if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                {
                    bUpdated = false;
                }
            }
            catch (Exception ex)
            {
                bUpdated = false;
                //throw new Exception(ex.Message);                            
            }

            return bUpdated;
        }

        private bool saveBatchDB()
        {
            bool bSaved = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "([scanproj]";
                sSQL += ",[appnum]";
                sSQL += ",[batchnum]";
                sSQL += ",[setnum]";
                sSQL += ",[batchcode]";
                sSQL += ",[batchtype]";
                sSQL += ",[batchstatus]"; 
                sSQL += ",[totpagecnt]";
                sSQL += ",[boxnum]";
                sSQL += ",[boxlabel]";
                sSQL += ",[docdefid]";
                sSQL += ",[doctype]";
                sSQL += ",[scanstation]";
                sSQL += ",[scanuser]";
                sSQL += ",[scanstart]";
                sSQL += ",[scanend]";
                sSQL += ",[fromproc]";
                sSQL += ") VALUES (";
                sSQL += "'" + _currScanProj.Trim().Replace("'","") + "'";
                sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _batchNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _setNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _batchType.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _batchStatus.Trim().Replace("'", "") + "'";
                sSQL += "," + _totPageCnt + "";
                sSQL += ",'" + _boxNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _boxRef.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _docType.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _scanStn.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _scanUsr.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _scanStart.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + _scanEnd.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + _fromProc.Trim().Replace("'", "") + "')";

                if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                {
                    bSaved = false;
                    throw new Exception("Saving data failure!");
                }
                
            }
            catch (Exception ex)
            {
                bSaved = false;
                mGlobal.Write2Log("Save batch.." + ex.Message);
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool saveDocScanDB(string pNewScanStatus, string pNewImgFilename)
        {
            bool bSaved = true;
            try
            {
                if (_fromProc.Trim().ToLower() == "verify") //Rescan request from Verify process. 
                {
                    saveRescanDocIndexDB(_scanStatus, pNewScanStatus, pNewImgFilename);
                }
                else //Rescan request from Index process. 
                {                    
                    saveDocScanDB(pNewScanStatus);
                }
            }
            catch (Exception ex)
            {
            }
            return bSaved;
        }

        private bool saveDocScanDB(string pNewScanStatus)
        {
            bool bSaved = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "([scanproj]";
                sSQL += ",[appnum]";
                sSQL += ",[setnum]";
                sSQL += ",[collanum]";
                sSQL += ",[batchcode]";
                sSQL += ",[boxnum]";
                sSQL += ",[docdefid]";
                sSQL += ",[doctype]";
                sSQL += ",[scanstation]";
                sSQL += ",[scanuser]";
                sSQL += ",[scanstart]";
                sSQL += ",[scanend]";
                sSQL += ",[scanstatus]";
                sSQL += ",[imageseq]";
                sSQL += ",[docimage]";
                sSQL += ",[page]";
                sSQL += ") VALUES (";
                sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _setNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _collaNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _boxNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _docType.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _scanStn.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _scanUsr.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _scanDStart.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + _scanDEnd.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + pNewScanStatus.Trim().Replace("'", "") + "'";
                sSQL += "," + iImgSeq + "";
                sSQL += ",'" + _imgFilename.Trim().Replace("'", "") + "'";
                sSQL += ",'" + sFrontBack.Trim().Replace("'", "") + "')";

                if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                {
                    bSaved = false;
                    throw new Exception("Saving data failure!");
                }

            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool saveDocIndexDB(string pDocuStatus, string pNewDocuStatus, string pNewFilename)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            try
            {
                bExist = oDF.checkDocuSetIndexExist(_currScanProj, _appNum, _batchCode, _setNum, _docType, pDocuStatus, pNewFilename);

                mGlobal.LoadAppDBCfg();

                if (bExist)
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "SET [indexstatus]='" + pNewDocuStatus + "'";
                    sSQL += ",[indexstation]='" + MDIMain.strStation.Trim().Replace("'", "") + "'";
                    sSQL += ",[indexuser]='" + MDIMain.strUserID.Trim().Replace("'", "") + "' ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND setnum='" + _setNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND doctype='" + _docType.Trim().Replace("'", "") + "' ";
                    sSQL += "AND indexstatus='" + pDocuStatus + "' ";
                    sSQL += "AND docimage='" + pNewFilename.Trim().Replace("'", "") + "' ";

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                        throw new Exception("Update document index data failure!");
                    }
                }
                else
                {
                    sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "([scanproj]";
                    sSQL += ",[appnum]";
                    sSQL += ",[setnum]";
                    sSQL += ",[collanum]";
                    sSQL += ",[batchcode]";
                    sSQL += ",[doctype]";
                    sSQL += ",[docdefid]";
                    sSQL += ",[indexstatus]";
                    sSQL += ",[imageseq]";
                    sSQL += ",[docimage]";
                    sSQL += ",[branchcode]";
                    sSQL += ",[page]";
                    sSQL += ") VALUES (";
                    sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _setNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _collaNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _docType.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pNewDocuStatus + "'";
                    sSQL += "," + iImgSeq + "";
                    sSQL += ",'" + pNewFilename.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + mGlobal.secDecrypt(mGlobal.GetAppCfg("Branch"), mGlobal.objSecData).Trim().Replace("'", "") + "'";
                    sSQL += ",'" + sFrontBack.Trim().Replace("'", "") + "')";

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                        throw new Exception("Saving document index data failure!");
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Save docidx.." + ex.Message);
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool saveRescanDocIndexDB(string pDocuStatus, string pNewDocuStatus, string pNewFilename)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            try
            {
                bExist = oDF.checkDocuSetIndexExist(_currScanProj, _appNum, _batchCode, _setNum, _docType, pDocuStatus, pNewFilename);

                mGlobal.LoadAppDBCfg();

                if (bExist)
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "SET [indexstatus]='" + pNewDocuStatus + "' ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND setnum=" + _setNum.Trim().Replace("'", "") + " ";
                    sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND doctype='" + _docType.Trim().Replace("'", "") + "' ";
                    sSQL += "AND indexstatus='" + pDocuStatus + "' ";
                    sSQL += "AND docimage='" + pNewFilename.Trim().Replace("'", "") + "' ";

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                        throw new Exception("Update document index data failure!");
                    }
                }
                else
                {
                    sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "([scanproj]";
                    sSQL += ",[appnum]";
                    sSQL += ",[setnum]";
                    sSQL += ",[collanum]";
                    sSQL += ",[batchcode]";
                    sSQL += ",[doctype]";
                    sSQL += ",[docdefid]";
                    sSQL += ",[indexstatus]";
                    sSQL += ",[imageseq]";
                    sSQL += ",[docimage]";
                    sSQL += ",[branchcode]";
                    sSQL += ",[page]";
                    sSQL += ") VALUES (";
                    sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _setNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _collaNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _docType.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pNewDocuStatus + "'";
                    sSQL += "," + iImgSeq + "";
                    sSQL += ",'" + pNewFilename.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + mGlobal.secDecrypt(mGlobal.GetAppCfg("Branch"), mGlobal.objSecData).Trim().Replace("'", "") + "'";
                    sSQL += ",'" + sFrontBack.Trim().Replace("'", "") + "')";

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                        throw new Exception("Saving document index data failure!");
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Save doc idx.." + ex.Message);
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private void loadRejectedBatchSetTreeview()
        {
            staMain._ScanProj = _currScanProj;
            staMain._AppNum = _appNum;

            if (_fromProc.Trim().ToLower() == "verify")
            {
                if (_noSeparator.Trim() == "0")
                {
                    if (_batchType.Trim().ToLower() == "set")
                        staMain.loadIndexSetTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    else
                        staMain.loadIndexBatchTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                }
                else
                {
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        staMain.loadIndexSetTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    }
                    else //Batch process type.
                    {
                        staMain.loadIndexBatchTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    }
                }
            }
            else
            {
                //staMain.loadBatchTreeviewUnion(this.tvwBatch, _batchType, _batchCode, "'4'", "'R','S'");
                if (_noSeparator.Trim() == "0")
                    staMain.loadBatchTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                else
                {
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        staMain.loadDocuSetTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    }
                    else //Batch process type.
                    {
                        staMain.loadBatchTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    }
                }

                if (staMain.checkImageNodesAvailable(tvwBatch) == false) //try load from TDocuIndex.
                {
                    if (_noSeparator.Trim() == "0")
                    {
                        if (_batchType.Trim().ToLower() == "set")
                            staMain.loadIndexSetTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                        else
                            staMain.loadIndexBatchTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    }
                    else
                    {
                        if (_batchType.Trim().ToLower() == "set")
                        {
                            staMain.loadIndexSetTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                        }
                        else //Batch process type.
                        {
                            staMain.loadIndexBatchTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                        }
                    }
                }
            }
            
        }

        private void loadRejectedBatchSetTreeview(DateTime pDateStart, DateTime pDateEnd)
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                sSQL += "AND batchcode='" + _batchCode.Trim() + "' ";
                sSQL += "AND batchstatus='4' ";
                //sSQL += "AND scanstation='" + _station.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanuser='" + _userId.Trim().Replace("'", "") + "' ";
                //sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                //sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables.Count > 0)
                {
                    if (dsTVw.Tables[0].Rows.Count > 0)
                    {
                        tvwBatch.Nodes.Clear();

                        int i = 0;
                        TreeNode node; DataRow dr = null;
                        while (i < dsTVw.Tables[0].Rows.Count)
                        {
                            dr = dsTVw.Tables[0].Rows[i];

                            sTag = "Sep_" + dr["batchcode"].ToString();
                            node = new TreeNode("Document Set " + dr["batchcode"].ToString().Replace("_", "-"));
                            node.Tag = new String(sTag.ToCharArray());
                            tvwBatch.Nodes.Add(node);

                            i += 1;
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Document Type
                int nc = 0;
                sTag = "";

                while (nc < tvwBatch.Nodes.Count)
                {
                    sNodeText = tvwBatch.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct doctype ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND indexstatus IN ('R') ";
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                    //sSQL += "AND indexstation='" + _station.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND indexuser='" + _userId.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                    //sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep1_" + dr["doctype"].ToString();
                                node = new TreeNode(dr["doctype"].ToString());
                                node.Tag = new String(sTag.ToCharArray());
                                tvwBatch.Nodes[nc].Nodes.Add(node);

                                i += 1;
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

                //Load Document.
                nc = 0;
                int nnc = 0; string sNodeText1 = "";
                sTag = "";

                while (nc < tvwBatch.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwBatch.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwBatch.Nodes[nc].Nodes.Count)
                    {
                        sNodeText1 = tvwBatch.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);

                        sSQL = "SELECT Distinct rowid,batchcode,doctype,indexstart ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                        sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText + "' ";
                        sSQL += "AND indexstatus IN ('R') ";
                        sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                        sSQL += "AND doctype='" + sNodeText1 + "' ";
                        //sSQL += "AND scanstation='" + _station.Trim().Replace("'", "") + "' ";
                        //sSQL += "AND scanuser='" + _userId.Trim().Replace("'", "") + "' ";
                        //sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        //sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        sSQL += "ORDER BY indexstart ";

                        dsTVw = new DataSet();
                        dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Doc_" + dr["batchcode"].ToString() + "_" + dr["doctype"].ToString() + "_" + dr["rowid"].ToString();
                                    node = new TreeNode("Page " + (i + 1));
                                    node.Tag = new String(sTag.ToCharArray());
                                    tvwBatch.Nodes[nc].Nodes[nnc].Nodes.Add(node);

                                    i += 1;
                                }
                            }
                        }
                        dsTVw.Dispose();
                        dsTVw = null;

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void initImgInfoCollection(int pTot)
        {
            try
            {
                if (pTot == 0) pTot = 1;

                _stcImgInfo.CurrImgIdx = 0;
                _stcImgInfo.SetNum = new string[pTot];
                _stcImgInfo.CollaNum = new string[pTot];
                _stcImgInfo.DocType = new string[pTot];
                _stcImgInfo.ImgIdx = new int[pTot];
                _stcImgInfo.ImgFile = new string[pTot];
                _stcImgInfo.ImgSeq = new int[pTot];
                _stcImgInfo.Rowid = new int[pTot];
                _stcImgInfo.Page = new string[pTot];

                int i = 0;
                while (i < pTot)
                {
                    _stcImgInfo.SetNum[i] = "";
                    _stcImgInfo.CollaNum[i] = "";
                    _stcImgInfo.DocType[i] = "";
                    _stcImgInfo.ImgIdx[i] = 0;
                    _stcImgInfo.ImgFile[i] = "";
                    _stcImgInfo.ImgSeq[i] = 0;
                    _stcImgInfo.Rowid[i] = 0;
                    _stcImgInfo.Page[i] = "";

                    i += 1;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void initImageAllInfo()
        {
            try
            {
                txtInfo.Text = "";
                txtCurrImgIdx.Text = "0";
                txtTotImgNum.Text = "0";

                _batchType = MDIMain.sBatchType;

                _stcImgInfo = new staMain._stcImgInfo();

                initImgInfoCollection(1);


                _oImgCore = new ImageCore(); //Init.
                _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                dsvImg.IfFitWindow = true;
                dsvImg.Bind(_oImgCore); //Clear.
                dsvThumbnailList.Bind(_oImgCore);

                tvwBatch.Nodes.Clear();

                ScanStatusBar.Text = "";
                ScanStatusBar1.Text = "";
            }
            catch (Exception ex)
            {
            }
        }

        private void loadRejectedImagesFrmDisk(long iCurrBatchRowId, bool pLoadImage = true)
        {
            try
            {
                DataRow bdr = oDF.getDocBatchDB(_currScanProj, _appNum, _batchType, "'3','4'", MDIMain.strStation, MDIMain.strUserID, iCurrBatchRowId);
                initImgInfoCollection(1);

                _totPageCnt = 0; //Init.

                if (bdr != null)
                {
                    _iCurrBatchRowid = Convert.ToInt32(bdr["rowid"].ToString());
                    _fromProc = bdr["fromproc"].ToString();

                    _batchNum = bdr["batchnum"].ToString();
                    _setNum = bdr["setnum"].ToString();
                    _batchCode = bdr["batchcode"].ToString();
                    _totPageCnt = Convert.ToInt32(bdr["totpagecnt"].ToString());

                    BoxNoStripTxt.Text = bdr["boxnum"].ToString();
                    boxLabelStripTxt.Text = bdr["boxlabel"].ToString();

                    sRemarks = bdr["remarks"].ToString();

                    initImgInfoCollection(_totPageCnt);

                    _stcImgInfo.ScanPrj = _currScanProj;
                    _stcImgInfo.AppNum = _appNum;
                    _stcImgInfo.BatchCode = _batchCode;
                    _stcImgInfo.SetNum[0] = _setNum;                    

                    DataRowCollection drs = null;

                    if (_fromProc.Trim().ToLower() == "verify") //Rescan request from Verify process.
                        drs = oDF.getIndexedDocFileByBatchCodeDB(_currScanProj, _appNum, _batchCode, "'R'");
                    else
                        drs = oDF.getDocFileByBatchCodeDB(_currScanProj, _appNum, _batchCode, "'R'");

                    if (drs != null)
                    {
                        if (drs.Count == 0)
                            drs = oDF.getIndexedDocFileByBatchCodeDB(_currScanProj, _appNum, _batchCode, "'R'");
                    }

                    string sFilePath = "";

                    if (drs != null)
                    {
                        if (_totPageCnt != drs.Count)
                            initImgInfoCollection(drs.Count);

                        int iTot = _totPageCnt;
                        if (_batchType.Trim().ToLower() == "set")
                            iTot = drs.Count;

                        int i = 0;
                        while (i < drs.Count)
                        {

                            sFilePath = drs[i]["docimage"].ToString().Trim();
                            _stcImgInfo.SetNum[i] = drs[i]["setnum"].ToString().Trim();

                            if (i < iTot) //In case index is out of bound of the array.
                            {
                                _stcImgInfo.CollaNum[i] = drs[i]["collanum"].ToString().Trim();
                                _stcImgInfo.DocType[i] = drs[i]["doctype"].ToString().Trim();
                                _stcImgInfo.ImgIdx[i] = i;
                                _stcImgInfo.ImgFile[i] = sFilePath;
                                _stcImgInfo.ImgSeq[i] = Convert.ToInt32(drs[i]["imageseq"].ToString());
                                _stcImgInfo.Rowid[i] = Convert.ToInt32(drs[i]["rowid"].ToString());
                                _stcImgInfo.Page[i] = drs[i]["page"].ToString().Trim();

                                if (!lScanImg.Contains(sFilePath))
                                    lScanImg.Add(sFilePath);
                            }

                            if (pLoadImage)
                            {
                                try
                                {
                                    if (File.Exists(sFilePath))
                                        _oImgCore.IO.LoadImage(sFilePath);
                                }
                                catch (Exception ex)
                                {
                                    mGlobal.Write2Log("Rescanning.." + sFilePath + ".." + ex.Message);
                                }
                            }

                            i += 1;
                        }

                        if (i > 0)
                        {
                            sCurrSetNum = _stcImgInfo.SetNum[0].ToString();
                        }
                    }

                    if (pLoadImage || sIsAddOn.Trim() != "")
                    {
                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                        {
                            _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = 0;
                            _stcImgInfo.CurrImgIdx = 0;
                            dsvImg.Bind(_oImgCore);
                            dsvThumbnailList.Bind(_oImgCore);

                            //dsvImg.IfFitWindow = true; //Windows Size if true else Actual Size set to false.
                            //dsvImg.Zoom = 1; //Actual Size.
                        }
                    }
                    else
                    {
                        dsvImg.Bind(_oImgCore);
                        dsvThumbnailList.Bind(_oImgCore);
                    }
                    //setImgInfo(0);
                }
                //else
                //{
                //    MessageBox.Show("Currently there are no reverted sets from " + _fromProc + " process for rescanning!", "Message");
                //    initImageAllInfo();
                //}
                CheckImageCount();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Rescanning.." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
            }
        }

        private void setImgInfo(short sImageIndex)
        {
            try
            {
                if (_stcImgInfo.ImgIdx.Length > 0 && _stcImgInfo.ImgIdx.Length > sImageIndex)
                {
                    if (_stcImgInfo.BatchCode != null)
                    {
                        _stcImgInfo.CurrImgIdx = sImageIndex;
                        txtInfo.Text = "Page " + (sImageIndex + 1).ToString(); 
                        ScanStatusBar1.Text = "Rescan: Current Total Pages " + _totPageCnt.ToString();
                    }
                }
                else
                {
                    if (_stcImgInfo.BatchCode != null)
                    {
                        txtInfo.Text = "Page " + (sImageIndex + 1).ToString();
                        ScanStatusBar1.Text = _stcImgInfo.BatchCode + " Rescan: Current Total Pages " + _totPageCnt.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Rescan.." + ex.Message);
            }

        }

        private void loadBatchSetRescanTreeview(string pFromProc)
        {
            staMain._ScanProj = _currScanProj;
            staMain._AppNum = _appNum;

            if (pFromProc.Trim().ToLower() == "verify")
            {
                if (_noSeparator.Trim() == "0")
                {
                    if (_batchType.Trim().ToLower() == "set")
                        staMain.loadIndexSetTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    else
                        staMain.loadIndexBatchTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                }
                else
                {
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        staMain.loadIndexSetTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    }
                    else //Batch process type.
                    {
                        staMain.loadIndexBatchTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    }
                }
            }
            else
            {
                if (_noSeparator.Trim() == "0")
                    staMain.loadBatchTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                else
                {
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        staMain.loadDocuSetTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                    }
                    else //Batch process type.
                    {
                        staMain.loadBatchTreeview(this.tvwBatch, _batchType, _batchCode, "'4','8'", "'R','G'");
                    }
                }

                //if (staMain.checkImageNodesAvailable(tvwBatch) == false) //try load from TDocuIndex.
                //{
                //    if (_noSeparator.Trim() == "0")
                //    {
                //        if (_batchType.Trim().ToLower() == "set")
                //            staMain.loadIndexSetTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                //        else
                //            staMain.loadIndexBatchTreeviewNoSep(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                //    }
                //    else
                //    {
                //        if (_batchType.Trim().ToLower() == "set")
                //        {
                //            staMain.loadIndexSetTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                //        }
                //        else //Batch process type.
                //        {
                //            staMain.loadIndexBatchTreeview(this.tvwBatch, _batchType, _batchCode, "'4'", "'R'");
                //        }
                //    }
                //}
            }
        }

        private void loadBatchSetRescanTreeview()
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                sSQL += "AND batchcode='" + _batchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN ('2','4') ";
                //sSQL += "AND scanstation='" + _scanStn.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanuser='" + _scanUsr.Trim().Replace("'", "") + "' ";
                //sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                //sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables.Count > 0)
                {
                    if (dsTVw.Tables[0].Rows.Count > 0)
                    {
                        tvwBatch.Nodes.Clear();

                        int i = 0;
                        TreeNode node; DataRow dr = null;
                        while (i < dsTVw.Tables[0].Rows.Count)
                        {
                            dr = dsTVw.Tables[0].Rows[i];

                            sTag = "Sep_" + dr["batchcode"].ToString();
                            node = new TreeNode("Document Set " + dr["batchcode"].ToString().Replace("_", "-"));
                            node.Tag = new String(sTag.ToCharArray());
                            tvwBatch.Nodes.Add(node);

                            i += 1;
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Document Type
                int nc = 0;
                sTag = "";

                while (nc < tvwBatch.Nodes.Count)
                {
                    sNodeText = tvwBatch.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct doctype ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND scanstatus IN ('S','A','R') ";
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                    sSQL += "AND scanstation='" + _scanStn.Trim().Replace("'", "") + "' ";
                    sSQL += "AND scanuser='" + _scanUsr.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                    //sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep1_" + dr["doctype"].ToString();
                                node = new TreeNode(dr["doctype"].ToString());
                                node.Tag = new String(sTag.ToCharArray());
                                tvwBatch.Nodes[nc].Nodes.Add(node);

                                i += 1;
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

                //Load Document.
                nc = 0;
                int nnc = 0; string sNodeText1 = "";
                sTag = "";

                while (nc < tvwBatch.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwBatch.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwBatch.Nodes[nc].Nodes.Count)
                    {
                        sNodeText1 = tvwBatch.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);

                        sSQL = "SELECT Distinct rowid,batchcode,doctype ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                        sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText + "' ";
                        sSQL += "AND scanstatus IN ('S','A','R') ";
                        sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                        sSQL += "AND doctype='" + sNodeText1 + "' ";
                        sSQL += "AND scanstation='" + _scanStn.Trim().Replace("'", "") + "' ";
                        sSQL += "AND scanuser='" + _scanUsr.Trim().Replace("'", "") + "' ";
                        //sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        //sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                        dsTVw = new DataSet();
                        dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Doc_" + dr["batchcode"].ToString() + "_" + dr["doctype"].ToString() + "_" + dr["rowid"].ToString();
                                    node = new TreeNode("Page " + (i + 1));
                                    node.Tag = new String(sTag.ToCharArray());
                                    tvwBatch.Nodes[nc].Nodes[nnc].Nodes.Add(node);
                                    tvwBatch.Nodes[nc].Nodes[nnc].LastNode.Name = tvwBatch.Nodes[nc].Nodes[nnc].LastNode.FullPath;

                                    i += 1;
                                }
                            }
                        }
                        dsTVw.Dispose();
                        dsTVw = null;

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void loadBatchTreeview(DateTime pDateStart, DateTime pDateEnd)
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                sSQL += "AND batchstatus='" + _batchStatus.Trim() + "' ";
                sSQL += "AND scanstation='" + _scanStn.Trim().Replace("'", "") + "' ";
                sSQL += "AND scanuser='" + _scanUsr.Trim().Replace("'", "") + "' ";
                sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables.Count > 0)
                {
                    if (dsTVw.Tables[0].Rows.Count > 0)
                    {
                        tvwBatch.Nodes.Clear();

                        int i = 0;
                        TreeNode node; DataRow dr = null;
                        while (i < dsTVw.Tables[0].Rows.Count)
                        {
                            dr = dsTVw.Tables[0].Rows[i];

                            sTag = "Sep_" + dr["batchcode"].ToString();
                            node = new TreeNode("Document Set " + (i + 1));
                            node.Tag = new String(sTag.ToCharArray());
                            tvwBatch.Nodes.Add(node);
                            
                            i += 1;
                        }                        
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Document Type
                int nc = 0;
                sTag = "";

                while (nc < tvwBatch.Nodes.Count)
                {
                    sNodeText = tvwBatch.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct doctype ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND scanstatus IN ('S','A') "; //Scan and Addon.
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                    sSQL += "AND scanstation='" + _scanStn.Trim().Replace("'", "") + "' ";
                    sSQL += "AND scanuser='" + _scanUsr.Trim().Replace("'", "") + "' ";
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep1_" + dr["doctype"].ToString();
                                node = new TreeNode(dr["doctype"].ToString());
                                node.Tag = new String(sTag.ToCharArray());
                                tvwBatch.Nodes[nc].Nodes.Add(node);
                                
                                i += 1;
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

                //Load Document.
                nc = 0;
                int nnc = 0; string sNodeText1 = "";
                sTag = "";

                while (nc < tvwBatch.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwBatch.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwBatch.Nodes[nc].Nodes.Count)
                    {
                        sNodeText1 = tvwBatch.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);

                        sSQL = "SELECT Distinct rowid,batchcode,doctype ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                        sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText + "' ";
                        sSQL += "AND scanstatus IN ('S','A') ";
                        sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                        sSQL += "AND doctype='" + sNodeText1 + "' ";
                        sSQL += "AND scanstation='" + _scanStn.Trim().Replace("'", "") + "' ";
                        sSQL += "AND scanuser='" + _scanUsr.Trim().Replace("'", "") + "' ";
                        sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                        dsTVw = new DataSet();
                        dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Doc_" + dr["batchcode"].ToString() + "_" + dr["doctype"].ToString() + "_" + dr["rowid"].ToString();
                                    node = new TreeNode("Page " + (i + 1));
                                    node.Tag = new String(sTag.ToCharArray());
                                    tvwBatch.Nodes[nc].Nodes[nnc].Nodes.Add(node);

                                    i += 1;
                                }
                            }
                        }
                        dsTVw.Dispose();
                        dsTVw = null;

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        private bool saveImageToFile(string pDir, string pFileName, string pFileExt)
        {            
            try
            {
                txtInfo.Text = "";

                if (mGlobal.verifyFileName(pFileName))
                {
                    this._imgFilename = mGlobal.addDirSep(pDir) + pFileName;

                    oDF._isMultiPage = false;
                    oDF.oImgCore = _oImgCore;
                    oDF.oPDFCreator = this.oPDFCreator;

                    Thread myThread = new Thread(new ThreadStart(() => oDF.saveImageToFile(pDir, pFileName, pFileExt.ToUpper())));
                    myThread.IsBackground = true;
                    myThread.Start();
                    //oDF.saveImageToFile(pDir, pFileName, pFileExt.ToUpper());

                }
                else
                {
                    mGlobal.Write2Log("Invalid filename!");
                    //this.btnScan.Focus();
                    ScanToolStrip.Focus();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
                return false;
                throw new Exception(ex.Message);
            }

            return true;
        }

        private void frmRescanBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (!btnScan.Enabled)
            if (!btnCloseStrip.Enabled)
            {
                MDIMain.bIsComplete = false;
                MessageBox.Show("The scanning process is running! Screen not able to close.", "Scan Process");
                e.Cancel = true;
            }
            else
                MDIMain.bIsComplete = true;
        }

        private void frmRescanBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (mGlobal.checkOpenedForms("frmNotification"))
                {
                    frmNotification oNote = (frmNotification)mGlobal.getOpenedForms("frmNotification");
                    if (oNote != null)
                    {
                        oNote.Close();
                        oNote.Dispose();
                    }
                }

                if (this.MdiParent != null && mGlobal.checkAnyOpenedForms() == false)
                    this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = true;

                enableScan(false);

                if (_oImgCore != null)
                    _oImgCore.Dispose();

                //if (oBarcodeRdr != null) oBarcodeRdr.Dispose();

                if (this.sIsAddOn.Trim().ToLower() == "rejected")
                {
                    if (this.MdiParent != null)
                        this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                    //if (bScanStart)
                    //{
                    //    frmIndexingReject oIndex = (frmIndexingReject)mGlobal.getOpenedForms("frmIndexingReject");
                    //    if (oIndex != null)
                    //    {
                    //        oIndex.reloadForAddOn();
                    //    }
                    //}                                      
                }
                else if (this.sIsAddOn.Trim().ToLower() == "indexing")
                {
                    if (this.MdiParent != null)
                        this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                    if (bScanStart)
                    {
                        frmIndexing oIndex = (frmIndexing)mGlobal.getOpenedForms("frmIndexing");
                        if (oIndex != null)
                        {
                            oIndex.reloadForAddOn();
                        }
                    }                    
                }                

                this.sIsAddOn = "";
                MDIMain.sIsAddOn = "";

                if (lScanImg != null) lScanImg.Clear();
                if (lHeaderIdx != null) lHeaderIdx.Clear();

                timer1.Enabled = false;
            }
            catch (Exception ex)
            {
                this.sIsAddOn = "";
                MDIMain.sIsAddOn = "";

                if (_oImgCore != null)
                    _oImgCore.Dispose();
                
                if (MDIMain.oTMgr != null)
                    MDIMain.oTMgr.Dispose();

                //if (oBarcodeRdr != null) oBarcodeRdr.Dispose();

                timer1.Enabled = false;
            }            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CurrDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
        }

        private void timerProgress_Tick(object sender, EventArgs e)
        {
            if (bCancel)
                ScanProgressBar.Value = 0;
            else
                ScanProgressBar.Value = ScanProgressBar.Value < ScanProgressBar.Maximum ? ScanProgressBar.Value + 1 : ScanProgressBar.Minimum;
        }

        private void tvwBatch_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string nodeInfo = "";
            string sep = "";
            try
            {
                if (e.Node.Level == 0)
                    txtInfo.Text = e.Node.Text;
                else
                    txtInfo.Text = "";
                mnuDeleteSetStrip.Enabled = false;
                mnuImpStrip.Enabled = false;
                mnuImpRepStrip.Enabled = false;
                mnuScanRepStrip.Enabled = false;

                mnuCopyStrip.Enabled = false;
                mnuCutStrip.Enabled = false;
                mnuDeleteImgStrip.Enabled = false;
                mnuReplaceStrip.Enabled = false;
                mnuPasteStrip.Enabled = false;
                mnuInsStrip.Enabled = false;

                nodeInfo = e.Node.Text.Trim().Substring(0, 4);
                sep = e.Node.Tag.ToString().Trim().Substring(0, 4);

                if (sep.Trim().ToLower() == "sep|")
                {
                    sCurrSetNum = 1.ToString(sSetNumFmt); //First set.
                    if (_batchType.ToLower().Trim() == "set")
                    {
                        if (_noSeparator.Trim() == "2")
                        {
                            ScanStatusBar1.Text = "Set " + _stcImgInfo.BatchCode.Replace("_", "-") + " : " + e.Node.Text.Trim();
                            mnuDeleteSetStrip.Enabled = true;
                        }
                        else
                        {
                            ScanStatusBar1.Text = "Batch " + _stcImgInfo.BatchCode.Replace("_", "-");
                            mnuDeleteSetStrip.Enabled = true;
                        }
                    }
                    else
                        ScanStatusBar1.Text = "Batch " + _stcImgInfo.BatchCode.Replace("_", "-");

                    txtInfo.Text = "Batch " + _stcImgInfo.BatchCode.Replace("_", "-") + ""; //Page 1.
                }
                else if (sep.Trim().ToLower() == "sep1")
                {
                    txtInfo.Text = "Set " + e.Node.Text.Trim();
                    if (_batchType.ToLower().Trim() == "set") //Doc. Type.
                    {
                        ScanStatusBar1.Text = "Set " + _stcImgInfo.BatchCode + " : " + e.Node.Text.Trim();
                        sCurrSetNum = e.Node.Text.Trim();

                        //Load the first image for doc set selected.
                        if (e.Node.Nodes.Count > 0)
                        {
                            short sCurrIdx = -1;
                            string rowid = e.Node.Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                            _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuScan");
                            if (_imgFilename.Trim() == string.Empty)
                                _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuIndex");
                            lCurrPgRowid = Convert.ToInt64(rowid);

                            short i = 0;
                            while (i < _stcImgInfo.ImgIdx.Length)
                            {
                                if (rowid == _stcImgInfo.Rowid[i].ToString())
                                {
                                    sCurrIdx = (short)_stcImgInfo.ImgIdx[i];
                                    _stcImgInfo.CurrImgIdx = sCurrIdx;
                                    break;
                                }

                                i += 1;
                            }

                            if (sCurrIdx != -1)
                            {
                                if (File.Exists(_stcImgInfo.ImgFile[sCurrIdx].ToString()))
                                {
                                    i = 0; int iAdv = 0;
                                    if (sIsAddOn.Trim() != "") //Calculate number of non loaded image if Addon Scanning process.
                                    {
                                        iAdv = lScanImg.Count - _oImgCore.ImageBuffer.HowManyImagesInBuffer;
                                    }

                                    while (i < _oImgCore.ImageBuffer.HowManyImagesInBuffer)
                                    {
                                        if (i + iAdv < lScanImg.Count)
                                        {
                                            if (lScanImg[i + iAdv].ToString().Trim() == _stcImgInfo.ImgFile[sCurrIdx].ToString())
                                            {
                                                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = i;

                                                iImgSeq = _stcImgInfo.ImgSeq[sCurrIdx];
                                                break;
                                            }
                                        }
                                        i += 1;
                                    }

                                    oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
                                }
                                else
                                    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                            }
                        }
                        //End.

                        mnuInsStrip.Enabled = true;
                        mnuImpStrip.Enabled = true;
                        if (sImgInfoCopy.Trim() != "")
                        {
                            mnuPasteStrip.Enabled = true;
                        }
                    }
                    else if (_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box")
                    {
                        //_setNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                        //_setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);

                        mnuInsStrip.Enabled = false;
                        mnuImpStrip.Enabled = false;
                        mnuDeleteSetStrip.Enabled = true;

                        //Load the first image for doc type for doc set selected.
                        if (e.Node.Nodes.Count > 0)
                        {
                            if (_noSeparator.Trim() == "1")
                            {
                                short sCurrIdx = -1;
                                string rowid = e.Node.Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuScan");
                                if (_imgFilename.Trim() == string.Empty)
                                    _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuIndex");
                                lCurrPgRowid = Convert.ToInt64(rowid);

                                short i = 0;
                                while (i < _stcImgInfo.ImgIdx.Length)
                                {
                                    if (rowid == _stcImgInfo.Rowid[i].ToString())
                                    {
                                        sCurrIdx = (short)_stcImgInfo.ImgIdx[i];
                                        _stcImgInfo.CurrImgIdx = sCurrIdx;
                                        break;
                                    }

                                    i += 1;
                                }

                                if (sCurrIdx != -1)
                                {
                                    if (File.Exists(_stcImgInfo.ImgFile[sCurrIdx].ToString()))
                                    {
                                        i = 0; int iAdv = 0;
                                        if (sIsAddOn.Trim() != "") //Calculate number of non loaded image if Addon Scanning process.
                                        {
                                            iAdv = lScanImg.Count - _oImgCore.ImageBuffer.HowManyImagesInBuffer;
                                        }

                                        while (i < _oImgCore.ImageBuffer.HowManyImagesInBuffer)
                                        {
                                            if (i + iAdv < lScanImg.Count)
                                            {
                                                if (lScanImg[i + iAdv].ToString().Trim() == _stcImgInfo.ImgFile[sCurrIdx].ToString())
                                                {
                                                    _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = i;

                                                    iImgSeq = _stcImgInfo.ImgSeq[sCurrIdx];
                                                    break;
                                                }
                                            }
                                            i += 1;
                                        }
                                        oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
                                    }
                                    else
                                        MessageBox.Show(this, "The selected document page does not exists!", "Message");
                                }
                            }
                            else
                            {
                                if (e.Node.Nodes[0].Nodes.Count > 0)
                                {
                                    short sCurrIdx = -1;
                                    string rowid = e.Node.Nodes[0].Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                    _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuScan");
                                    if (_imgFilename.Trim() == string.Empty)
                                        _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuIndex");
                                    lCurrPgRowid = Convert.ToInt64(rowid);

                                    short i = 0;
                                    while (i < _stcImgInfo.ImgIdx.Length)
                                    {
                                        if (rowid == _stcImgInfo.Rowid[i].ToString())
                                        {
                                            sCurrIdx = (short)_stcImgInfo.ImgIdx[i];
                                            _stcImgInfo.CurrImgIdx = sCurrIdx;
                                            break;
                                        }

                                        i += 1;
                                    }

                                    if (sCurrIdx != -1)
                                    {
                                        if (File.Exists(_stcImgInfo.ImgFile[sCurrIdx].ToString()))
                                        {
                                            i = 0; int iAdv = 0;
                                            if (sIsAddOn.Trim() != "") //Calculate number of non loaded image if Addon Scanning process.
                                            {
                                                iAdv = lScanImg.Count - _oImgCore.ImageBuffer.HowManyImagesInBuffer;
                                            }

                                            while (i < _oImgCore.ImageBuffer.HowManyImagesInBuffer)
                                            {
                                                if (i + iAdv < lScanImg.Count)
                                                {
                                                    if (lScanImg[i + iAdv].ToString().Trim() == _stcImgInfo.ImgFile[sCurrIdx].ToString())
                                                    {
                                                        _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = i;

                                                        iImgSeq = _stcImgInfo.ImgSeq[sCurrIdx];
                                                        break;
                                                    }
                                                }
                                                i += 1;
                                            }
                                            oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
                                        }
                                        else
                                            MessageBox.Show(this, "The selected document page does not exists!", "Message");
                                    }
                                }
                            }
                        }
                        //End.

                        sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();

                        ScanStatusBar1.Text = "Set " + sCurrSetNum;
                    }                    
                }
                else if (sep.Trim().ToLower() == "sep2") //Doc. Type.
                {
                    if (_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box")
                    {
                        //_docType = e.Node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                        //_setNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        //_setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);
                        //_collaNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                        //iImgSeq = oDF.getLastImageSeqDB(_currScanProj, _appNum, _setNum, _collaNum, _batchCode);

                        mnuInsStrip.Enabled = true;
                        mnuImpStrip.Enabled = true;
                        if (sImgInfoCopy.Trim() != "")
                        {
                            mnuPasteStrip.Enabled = true;
                        }                        

                        //Load the first image for doc type selected.
                        if (e.Node.Nodes.Count > 0)
                        {
                            short sCurrIdx = -1;
                            string rowid = e.Node.Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                            _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuScan");
                            if (_imgFilename.Trim() == string.Empty)
                                _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuIndex");
                            lCurrPgRowid = Convert.ToInt64(rowid);

                            short i = 0;
                            while (i < _stcImgInfo.ImgIdx.Length)
                            {
                                if (rowid == _stcImgInfo.Rowid[i].ToString())
                                {
                                    sCurrIdx = (short)_stcImgInfo.ImgIdx[i];
                                    _stcImgInfo.CurrImgIdx = sCurrIdx;
                                    break;
                                }

                                i += 1;
                            }

                            if (sCurrIdx != -1)
                            {
                                if (File.Exists(_stcImgInfo.ImgFile[sCurrIdx].ToString()))
                                {
                                    i = 0; int iAdv = 0;
                                    if (sIsAddOn.Trim() != "") //Calculate number of non loaded image if Addon Scanning process.
                                    {
                                        iAdv = lScanImg.Count - _oImgCore.ImageBuffer.HowManyImagesInBuffer;
                                    }

                                    while (i < _oImgCore.ImageBuffer.HowManyImagesInBuffer)
                                    {
                                        if (i + iAdv < lScanImg.Count)
                                        {
                                            if (lScanImg[i + iAdv].ToString().Trim() == _stcImgInfo.ImgFile[sCurrIdx].ToString())
                                            {
                                                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = i;

                                                iImgSeq = _stcImgInfo.ImgSeq[sCurrIdx];
                                                break;
                                            }
                                        }
                                        i += 1;
                                    }
                                    oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
                                }
                                else
                                    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                            }
                        }
                        //End.

                        sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        sCurrDocType = e.Node.Text.Trim();
                        ScanStatusBar1.Text = "Set " + sCurrSetNum + " : " + sCurrDocType;
                    }
                }
                else if (nodeInfo.Trim().ToLower() == "page")
                {
                    txtInfo.Text = "Page view : " + e.Node.Text;
                    mnuImpStrip.Enabled = true;
                    mnuImpRepStrip.Enabled = true;
                    mnuScanRepStrip.Enabled = true;

                    mnuCopyStrip.Enabled = true;
                    mnuCutStrip.Enabled = true;
                    mnuDeleteImgStrip.Enabled = true;
                    mnuReplaceStrip.Enabled = true;
                    mnuPasteStrip.Enabled = true;
                    mnuInsStrip.Enabled = true;

                    //sTag = "Doc_" + dr["batchcode"].ToString() + "_" + dr["doctype"].ToString() + "_" + dr["rowid"].ToString();
                    if (e.Node.Tag.ToString().Trim().Split('|').Length > 1)
                    {
                        short sCurrIdx = -1;
                        string rowid = e.Node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                        sCurrDocType = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        if (_fromProc.Trim().ToLower() == "verify")
                        {
                            _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuIndex");
                        }
                        else
                        {
                            _imgFilename = oDF.getDocFilenameDB(rowid, "TDocuScan");
                        }                        
                        lCurrPgRowid = Convert.ToInt64(rowid);

                        short i = 0;
                        while (i < _stcImgInfo.ImgIdx.Length)
                        {
                            if (rowid == _stcImgInfo.Rowid[i].ToString())
                            {
                                sCurrIdx = (short)_stcImgInfo.ImgIdx[i];
                                _stcImgInfo.CurrImgIdx = sCurrIdx;
                                break;
                            }

                            i += 1;
                        }

                        if (sCurrIdx != -1)
                        {
                            if (File.Exists(_stcImgInfo.ImgFile[sCurrIdx].ToString()))
                            {
                                i = 0; int iAdv = 0;
                                if (sIsAddOn.Trim() != "") //Calculate number of non loaded image if Addon Scanning process.
                                {
                                    iAdv = lScanImg.Count - MDIMain.oImgCore.ImageBuffer.HowManyImagesInBuffer;
                                }

                                while (i < _oImgCore.ImageBuffer.HowManyImagesInBuffer)
                                {
                                    if (i + iAdv < lScanImg.Count)
                                    {
                                        if (lScanImg[i + iAdv].ToString().Trim() == _stcImgInfo.ImgFile[sCurrIdx].ToString())
                                        {
                                            if (lScanImg.Count == _stcImgInfo.ImgFile.Length)
                                                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                            else
                                                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = i;

                                            iImgSeq = _stcImgInfo.ImgSeq[sCurrIdx];
                                            break;
                                        }
                                    }
                                    i += 1;
                                }

                                if (_stcImgInfo.Page[sCurrIdx] == "F")
                                    txtInfo.Text = "Front " + txtInfo.Text;
                                else
                                    txtInfo.Text = "Back " + txtInfo.Text;

                                oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
                            }
                            else
                                MessageBox.Show(this, "The selected document page does not exists!", "Message");
                        }
                        
                        CheckImageCount();
                        if (_noSeparator.Trim() == "0")
                        {
                            sCurrSetNum = (e.Node.Index + 1).ToString(sSetNumFmt); //Set number = Page number                            
                        }
                        else
                            sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();

                        if (sCurrSetNum.Trim() == sCurrDocType.Trim())
                            ScanStatusBar1.Text = "Set " + sCurrSetNum;
                        else
                            ScanStatusBar1.Text = "Set " + sCurrSetNum + " : " + sCurrDocType;
                    }
                    else
                        ScanStatusBar1.Text = "Batch " + _batchCode;

                    //1 or 2 separator Only first page will be shown OCR Zone.
                    if (_noSeparator.Trim() == "0" || ((_noSeparator.Trim() == "1" || _noSeparator.Trim() == "2")
                        && e.Node.Text.ToLower().Trim() == "page 1"))
                    {
                        if (bDocAutoOCR)
                        {
                            Bitmap bmp = (Bitmap)Bitmap.FromFile(_imgFilename);
                            List<System.Drawing.Rectangle> cropAreas = new List<System.Drawing.Rectangle>();
                            System.Drawing.Rectangle oRect = new System.Drawing.Rectangle();

                            int i = 0;
                            while (i < oDF.stcOCRAreaList.Count)
                            {
                                oRect = new System.Drawing.Rectangle();

                                oRect.X = oDF.stcOCRAreaList[i].left;
                                oRect.Y = oDF.stcOCRAreaList[i].top;
                                oRect.Width = oDF.stcOCRAreaList[i].width;
                                oRect.Height = oDF.stcOCRAreaList[i].height;

                                cropAreas.Add(oRect);

                                i += 1;
                            }

                            try
                            {
                                ShowResultOnImage(bmp, cropAreas.ToArray()); //May have Parameter is invalid exception related to OnPostTransfer..
                            }
                            catch (Exception ex)
                            {
                                mGlobal.Write2Log(ex.Message + ".." + ex.StackTrace.ToString());
                            }

                            bmp.Dispose();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }

        private void picboxNext_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 &&
                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer < _oImgCore.ImageBuffer.HowManyImagesInBuffer - 1)
                ++_oImgCore.ImageBuffer.CurrentImageIndexInBuffer;

            CheckImageCount();
        }

        private void picboxPrevious_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 && _oImgCore.ImageBuffer.CurrentImageIndexInBuffer > 0)
                --_oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
            CheckImageCount();
        }

        private void picboxFirst_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = 0;
            CheckImageCount();
        }

        private void picboxLast_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = (short)(_oImgCore.ImageBuffer.HowManyImagesInBuffer - 1);
            CheckImageCount();
        }

        private void dsvThumbnailList_OnMouseClick(short sImageIndex)
        {
            txtInfo.Text = "Thumbnail view";
            CheckImageCount();
        }

        private void btnCloseStrip_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDeleteStrip_Click(object sender, EventArgs e)
        {
            bool bOK = true;
            bool bFrmIndex = true;
            try
            {
                btnCloseStrip.Enabled = true;
                int iNodes = 0;
                if (tvwBatch.Nodes.Count > 0)
                {
                    iNodes = tvwBatch.Nodes[0].Nodes.Count;
                }

                if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 || iNodes > 0)
                {
                    int i = 0; int rowid = 0;
                    bool bConfirm = false;

                    if (_fromProc.Trim().ToLower() == "verify")
                        bFrmIndex = false;

                    if (MessageBox.Show(this, "Confirm to delete the selected image of set " + sCurrSetNum + "?", "Delete Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        bConfirm = true;

                    if (bConfirm)
                    {
                        string sBatchPath = mGlobal.replaceValidChars(_batchCode, "_");
                        string sDirRoot = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";

                        if (!bFrmIndex) //From Verify rescan request.
                            sDirRoot = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";

                        ScanStatusBar.Text = "Deleting an image";
                        btnCloseStrip.Enabled = false;
                        //btnScan.Enabled = false;
                        enableScan(false);
                        btnDeleteStrip.Enabled = false;
                        btnDeleteAllStrip.Enabled = false;

                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                            i = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;

                        if (i < _stcImgInfo.ImgIdx.Length)
                        {
                            _batchCode = _stcImgInfo.BatchCode;
                            _collaNum = _stcImgInfo.CollaNum[i];
                            _docType = _stcImgInfo.DocType[i];
                            _imgFilename = _stcImgInfo.ImgFile[i];
                            rowid = _stcImgInfo.Rowid[i];

                            try
                            {
                                if (File.Exists(_imgFilename))
                                    File.Delete(_imgFilename);

                                if (_docType != "")
                                {
                                    string sDir = mGlobal.addDirSep(sDirRoot) + _docType + @"\";

                                    if (Directory.Exists(sDir) && Directory.GetFiles(sDir).Length == 0)
                                        Directory.Delete(sDir);
                                }                                
                            }
                            catch (Exception ex)
                            {
                                bOK = false;
                                mGlobal.Write2Log("Image delete exception!" + ex.Message);
                            }

                            if (bOK || iNodes > 0)
                            {
                                if (!bFrmIndex) //From Verify rescan request.
                                    bOK = oDF.deleteRecordDB(rowid, "TDocuIndex");
                                else
                                    bOK = oDF.deleteRecordDB(rowid, "TDocuScan");

                                if (bOK) //Reload all.
                                {
                                    if (_batchType.Trim().ToLower() == "set")
                                    {
                                        _docType = sCurrSetNum;
                                        if (!bFrmIndex) //From Verify rescan request.
                                            _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                        else
                                            _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                    }
                                    else
                                    {
                                        if (!bFrmIndex) //From Verify rescan request.
                                            _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                        else
                                            _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                    }

                                    //_totPageCnt = _totPageCnt - 1;
                                    if (_totPageCnt < 0) _totPageCnt = 0;

                                    if (_totPageCnt == 0)
                                    {
                                        if (_noSeparator.Trim() == "C")
                                        {
                                            oDF.deleteAllIDPKeyFieldValuesDB(_currScanProj, _appNum, _batchCode); //delete indexed key field value if any.
                                            oDF.deleteAllIDPKeyValuesDB(_currScanProj, _appNum, _batchCode); //delete indexed key value if any.
                                            oDF.deleteAllIDPTableValuesDB(_currScanProj, _appNum, _batchCode); //delete table value if any.
                                            oDF.deleteAllIDPTableColsDB(_currScanProj, _appNum, _batchCode); //delete table columns structure if any.
                                            oDF.deleteAllIDPModelDB(_currScanProj, _appNum, _batchCode); //delete IDP Model if any.
                                        }

                                        bOK = deleteBatchSetRecordDB(_batchCode, sCurrSetNum);
                                    }
                                    else
                                    {
                                        if (_batchType.Trim().ToLower() == "set")
                                        {
                                            _docType = sCurrSetNum;
                                            bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "", sCurrSetNum);
                                            //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "");
                                        }
                                        else
                                            bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "");
                                    }

                                    string sStatus = "4";
                                    string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                    if (_batchType.Trim().ToLower() == "set")
                                    {
                                        _docType = sCurrSetNum;
                                        bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "4", _totPageCnt, sStatus, "", "", sModifiedDate, sCurrSetNum);
                                        //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "4", _totPageCnt, sStatus, "", "", sModifiedDate);
                                    }
                                    else
                                        bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "4", _totPageCnt, sStatus, "", "", sModifiedDate);

                                    if (bOK)
                                    {
                                        if (_totPageCnt == 0)
                                        {
                                            try
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                {
                                                    sDirRoot = mGlobal.addDirSep(sDirRoot) + sCurrSetNum + @"\";
                                                }

                                                if (Directory.Exists(sDirRoot) && Directory.GetFiles(sDirRoot).Length == 0)
                                                    Directory.Delete(sDirRoot);
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }

                                        if (_totPageCnt != 0)
                                            oDF.reorderDocTypeScanImageSeq(_currScanProj, _appNum, sCurrSetNum, _docType, _batchCode, iImgSeq);

                                        if (_totPageCnt == 0)
                                        {
                                            List<short> lSet = new List<short>();
                                            if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer - 2 > -1)
                                            {
                                                if (lScanImg[_oImgCore.ImageBuffer.CurrentImageIndexInBuffer - 2].ToUpper() == staMain.stcProjCfg.SeparatorText)
                                                    lSet.Add((short)(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer - 2));

                                                if (lSet.Count == 1) //Found a header separator.
                                                    lSet.Add((short)(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer - 1));
                                            }

                                            lSet.Add(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                                            lSet.Sort();

                                            _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = lSet[0];
                                            _oImgCore.ImageBuffer.RemoveImages(lSet);
                                            lScanImg.RemoveRange(lSet[0], lSet.Count);

                                            lSet.Clear();
                                        }
                                        else
                                        {
                                            short sCur = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                            List<short> lType = new List<short>();

                                            if (_fromProc.Trim().ToLower() == "verify")
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                {
                                                    if (oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sCurrSetNum) == 0)
                                                    {
                                                        lType.Add((short)(sCur - 1));
                                                    }
                                                }
                                                else
                                                {
                                                    if (oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrDocType) == 0)
                                                    {
                                                        lType.Add((short)(sCur - 1));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                {
                                                    if (oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum) == 0)
                                                    {
                                                        lType.Add((short)(sCur - 1));
                                                    }
                                                }
                                                else
                                                {
                                                    if (oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrDocType) == 0)
                                                    {
                                                        lType.Add((short)(sCur - 1));
                                                    }
                                                }
                                            }

                                            lType.Add(sCur);

                                            _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = lType[0];
                                            _oImgCore.ImageBuffer.RemoveImages(lType);
                                            lScanImg.RemoveRange(lType[0], lType.Count);
                                        }
                                    }

                                    bScanStart = true;

                                    selectedNode = tvwBatch.SelectedNode;

                                    if (bOK)
                                        MessageBox.Show("Selected image is deleted successfully!");
                                    else
                                        MessageBox.Show("Selected image delete unsuccessful!");

                                    initImageAllInfo();

                                    _iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, _setNum, "'4'");

                                    loadRejectedImagesFrmDisk(_iCurrBatchRowid);                                    

                                    CheckImageCount();

                                    if (!bFrmIndex) //From Verify rescan request.
                                        //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                                        loadRejectedBatchSetTreeview();
                                    else
                                        loadBatchSetRescanTreeview(_fromProc); //loadBatchSetRescanTreeview();

                                    if (tvwBatch.Nodes.Count > 0)
                                    {
                                        tvwBatch.ExpandAll();

                                        //mGlobal.Write2Log("Node Name.." + tvwBatch.Nodes[0].Nodes[0].Nodes[0].Nodes[0].Name);
                                        TreeNode[] oNodes = tvwBatch.Nodes.Find(selectedNode.Name, true);
                                        if (oNodes.Length > 0)
                                        {
                                            tvwBatch.SelectedNode = oNodes[0];
                                            tvwBatch.SelectedNode.EnsureVisible();
                                        }

                                        //tvwBatch.Refresh();
                                    }

                                    setImgInfo(0);
                                }
                            }
                            else
                                MessageBox.Show("Selected image delete unsuccessful!");
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Delete.." + ex.Message);
            }
            finally
            {
                ScanStatusBar.Text = "Ready";
                btnCloseStrip.Enabled = true;
                //btnScan.Enabled = true;
                enableScan(true);
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;
            }
        }

        private void btnDeleteAllStrip_Click(object sender, EventArgs e)
        {
            bool bOK = true;
            bool bFrmIndex = true;
            try
            {
                btnCloseStrip.Enabled = true;
                int iNodes = 0;
                if (tvwBatch.Nodes.Count > 0)
                {
                    iNodes = tvwBatch.Nodes[0].Nodes.Count;
                }

                if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 || iNodes > 0)
                {
                    int i = 0;
                    bool bConfirm = false;

                    if (_fromProc.Trim().ToLower() == "verify")
                        bFrmIndex = false;

                    if (MessageBox.Show(this, "Confirm to delete ALL sets and all the images?", "Delete All Images Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        bConfirm = true;

                    if (bConfirm)
                    {
                        string sBatchPath = mGlobal.replaceValidChars(_batchCode, "_");
                        string sDirRoot = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";

                        if (!bFrmIndex) //From Verify rescan request.
                            sDirRoot = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";

                        ScanStatusBar.Text = "Deleting all images";
                        btnCloseStrip.Enabled = false;
                        //btnScan.Enabled = false;
                        enableScan(false);
                        btnDeleteStrip.Enabled = false;
                        btnDeleteAllStrip.Enabled = false;

                        int j = 0;
                        if (Directory.Exists(sDirRoot))
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(sDirRoot);

                            FileInfo file;
                            while (i < dirInfo.GetFiles().Length)
                            {
                                file = (FileInfo) dirInfo.GetFiles().GetValue(i);
                                file.Delete();

                                i += 1;
                            }

                            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                            {
                                dir.Delete(true);
                            }

                            Directory.Delete(sDirRoot);
                        }

                        if (i > 0 || j > 0 || iNodes > 0)
                        {                         
                            if (bOK)
                            {
                                deleteAllImagesRecordDB(_batchCode, "TDocuIndex");
                                deleteAllImagesRecordDB(_batchCode, "TDocuScan");
                                //if (!bFrmIndex) //From Verify rescan request.
                                //    bOK = deleteAllImagesRecordDB(_batchCode, "TDocuIndex");
                                //else
                                //    bOK = deleteAllImagesRecordDB(_batchCode, "TDocuScan");

                                if (bOK) //Reload all.
                                {
                                    oDF.deleteAllIndexedRecordDB(_currScanProj, _appNum, _batchCode); //delete indexed value if any.

                                    if (_noSeparator.Trim() == "C")
                                    {
                                        oDF.deleteAllIDPKeyFieldValuesDB(_currScanProj, _appNum, _batchCode); //delete indexed key field value if any.
                                        oDF.deleteAllIDPKeyValuesDB(_currScanProj, _appNum, _batchCode); //delete indexed key value if any.
                                        oDF.deleteAllIDPTableValuesDB(_currScanProj, _appNum, _batchCode); //delete table value if any.
                                        oDF.deleteAllIDPTableColsDB(_currScanProj, _appNum, _batchCode); //delete table columns structure if any.
                                        oDF.deleteAllIDPModelDB(_currScanProj, _appNum, _batchCode); //delete IDP Model if any.
                                    }

                                    frmNotification oNotify = new frmNotification();
                                    oNotify.deleteAllNotificationDB(_batchCode);
                                    oNotify.Dispose();

                                    _totPageCnt = 0;
                                    //string sStatus = "4";
                                    //string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "4", _totPageCnt, sStatus, "", "", sModifiedDate);
                                    
                                    bOK = deleteBatchRecordDB(_batchCode);

                                    if (bOK)
                                        MessageBox.Show("All image are deleted successfully!");
                                    else
                                        MessageBox.Show("All image delete unsuccessful!");

                                    initImageAllInfo();
                                    //_batchCode = "";
                                    _oImgCore = new ImageCore(); //Init.
                                    _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                                    dsvImg.Bind(_oImgCore); //Clear.
                                    dsvThumbnailList.Bind(_oImgCore);

                                    lScanImg.Clear();
                                    lScanImg = new List<string>();

                                    tvwBatch.Nodes.Clear();

                                    //Init batch number ready for next scan.>>
                                    this.iBatchNum = oDF.getLastBatchNum(_currScanProj, _appNum, _batchType);
                                    sLastBatchNum = iBatchNum.ToString(sBatchNumFmt);

                                    iBatchNum = oDF.getNewBatchNum(_currScanProj, _appNum, DateTime.Now, _batchType);
                                    _batchNum = iBatchNum.ToString(sBatchNumFmt);

                                    if (_batchType.Trim().ToLower() == "set" || staMain.stcProjCfg.BatchAuto.Trim().ToUpper() == "N")
                                        this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum);
                                    else //batch.
                                        this.iSetNum = 1;

                                    _setNum = this.iSetNum.ToString(sSetNumFmt);
                                    //>>

                                    //_iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, _setNum, "'4'");

                                    //loadRejectedImagesFrmDisk(_iCurrBatchRowid);                                    

                                    //CheckImageCount();

                                    //if (!bFrmIndex) //From Verify rescan request.
                                    //    //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                                    //    loadRejectedBatchSetTreeview();
                                    //else
                                    //    loadBatchSetRescanTreeview(_fromProc); //loadBatchSetRescanTreeview();

                                    //if (tvwBatch.Nodes.Count > 0)
                                    //    tvwBatch.ExpandAll();

                                    setImgInfo(0);
                                }
                            }
                            else
                                MessageBox.Show("All images delete unsuccessful!");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Delete all..." + ex.Message);
            }
            finally
            {
                ScanStatusBar.Text = "Ready";
                btnCloseStrip.Enabled = true;
                //btnScan.Enabled = true;
                enableScan(true);
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;
            }
        }

        public bool deleteBatchRecordDB(string pBatchCode)
        {
            bool bUpdated = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                //sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                //sSQL += "SET batchstatus='3' "; //Delete.
                //sSQL += ",totpagecnt=0 ";
                //sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                //sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                bUpdated = mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                bUpdated = false;
                //throw new Exception(ex.Message);                            
            }

            return bUpdated;
        }

        public bool deleteAllImagesRecordDB(string pBatchCode, string pDBTableName)
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + pDBTableName + " ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL, false) == false)
                {
                    bDeleted = false;
                }

            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            bool bConfirm = false;
            try
            {
                if (_batchCode.Trim() == "" && staMain.stcProjCfg.BatchAuto.Trim().ToUpper() == "N")
                {
                    MessageBox.Show("Batch code is required! Please check with your system administrator.");
                    return;
                }

                if (MessageBox.Show(this, "Confirm to send this batch " + _batchCode + " to indexing from " + _fromProc.ToLower() + "?", "Send Confirmation", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    bConfirm = true;

                if (bConfirm)
                {
                    string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    bool bOK = true;
                    if (_fromProc.Trim().ToLower() == "verify") //Rescan request from Verify process. 
                    {
                        //bOK = updateRescanDocDB(_batchCode, "R", "V", _fromProc); //Send back to verify.
                        bOK = updateRescanDocDB(_batchCode, "R", "S", _fromProc); //Send back to index.  

                        if (bOK)
                        {
                            if (_batchType.Trim().ToLower() == "set")
                            {
                                int i = 0; string sSetNum = "";
                                while (i < _stcImgInfo.SetNum.Length)
                                {
                                    sSetNum = _stcImgInfo.SetNum[i];
                                    _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sSetNum);
                                    //updateBatchDB(_batchCode, "4", "5", sModifiedDate, sSetNum); //Send back to verify.
                                    updateBatchDB(_batchCode, "4", "1", sModifiedDate, sSetNum); //Send back to index.

                                    i += 1;
                                }

                                if (i > 0) bOK = true;
                            }
                            else
                                //bOK = updateBatchDB(_batchCode, "4", "5", sModifiedDate); //Send back to verify.
                                bOK = updateBatchDB(_batchCode, "4", "1", sModifiedDate); //Send back to index.  
                        }
                    }
                    else if (_fromProc.Trim().ToLower() == "index") //Rescan request from Index process. 
                    {
                        //bOK = updateRescanDocDB(_batchCode, "R", "S", _fromProc); //Send back to verify.
                        bOK = updateRescanDocDB(_batchCode, "R", "S", _fromProc); //Send back to index.

                        if (bOK)
                        {
                            if (_batchType.Trim().ToLower() == "set")
                            {
                                int i = 0; string sSetNum = "";
                                while (i < _stcImgInfo.SetNum.Length)
                                {
                                    sSetNum = _stcImgInfo.SetNum[i];
                                    _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sSetNum);
                                    //updateBatchDB(_batchCode, "4", "1", sModifiedDate, sSetNum); //Send back to verify.
                                    updateBatchDB(_batchCode, "4", "1", sModifiedDate, sSetNum); //Send back to index.

                                    i += 1;
                                }

                                if (i > 0) bOK = true;
                            }
                            else
                                //bOK = updateBatchDB(_batchCode, "4", "1", sModifiedDate); //Send back to verify.
                                bOK = updateBatchDB(_batchCode, "4", "1", sModifiedDate); //Send back to index.
                        }
                    }
                    else
                    {
                        bOK = false;
                        MessageBox.Show("Batch " + _batchCode + " from process is unknown error!", "Message");
                    }

                    if (bOK)
                    {
                        MessageBox.Show("Batch " + _batchCode + " sent successfully.", "Message");
                        //initImageAllInfo();
                        //ScanStatusBar.Text = "Ready";
                        this.Close();
                    }
                    else
                        MessageBox.Show("Batch " + _batchCode + " send unsuccessful.", "Message");
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Send.." + ex.Message);
                MessageBox.Show("Send error found!\n" + ex.Message, "Error");
            }
        }

        private bool updateRescanDocDB(string pBatchCode, string pDocuStatus, string pNewDocuStatus, string pFromProc)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            try
            {
                mGlobal.LoadAppDBCfg();

                string sDBTable = "TDocuScan";

                if (pFromProc.Trim().ToLower() == "verify")
                {
                    sDBTable = "TDocuIndex";
                    bExist = true;
                }                    

                if (bExist)
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + sDBTable.Trim().Replace("'", "") + " ";
                    sSQL += "SET [indexstatus]='" + pNewDocuStatus + "' ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";                    
                    sSQL += "AND indexstatus='" + pDocuStatus + "' "; 

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = updateRescanDocDB(pBatchCode, pDocuStatus, pNewDocuStatus, "Verify");
                        if (bSaved) return bSaved;

                        bSaved = false;
                        throw new Exception("Update document index data failed!");
                    }
                }
                else
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + sDBTable.Trim().Replace("'", "") + " ";
                    sSQL += "SET [scanstatus]='" + pNewDocuStatus + "' ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND scanstatus='" + pDocuStatus + "' ";

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = updateRescanDocDB(pBatchCode, pDocuStatus, pNewDocuStatus, "Verify");
                        if (bSaved) return bSaved;

                        bSaved = false;
                        throw new Exception("Update document scan data failed!");
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Update "+_fromProc+".." + ex.Message);
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        //public void reloadForEdit()
        //{
        //    _oImgCore = new ImageCore();
        //    _stcImgInfo = new staMain._stcImgInfo();

        //    loadRejectedImagesFrmDisk(0);
        //    loadRejectedBatchSetTreeview();
        //    if (tvwBatch.Nodes.Count > 0)
        //        tvwBatch.ExpandAll();

        //    setImgInfo(0);

        //    CheckImageCount();
        //}

        public void reloadForCurrRow(long pCurrRowId, string pBatchCode)
        {
            try
            {
                this._oImgCore = new ImageCore();
                _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;
                _stcImgInfo = new staMain._stcImgInfo();

                _iCurrBatchRowid = pCurrRowId;

                loadRejectedImagesFrmDisk(_iCurrBatchRowid);
                //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                loadRejectedBatchSetTreeview();
                if (tvwBatch.Nodes.Count > 0)
                    tvwBatch.ExpandAll();

                if (tvwBatch.Nodes.Count > 0)
                {
                    btnDeleteStrip.Enabled = true;
                    btnDeleteAllStrip.Enabled = true;

                    //btnNoticeStrip.Enabled = true;
                    //tvwBatch.ExpandAll();
                }
                else
                {
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;

                    //btnNoticeStrip.Enabled = false;
                }

                _stcImgInfo.BatchCode = pBatchCode;
                _batchCode = pBatchCode;

                //if (_stcImgInfo.ImgIdx.Length > 1)
                //    setImgInfo(0);
                //else
                    ScanStatusBar1.Text = "Batch " + _stcImgInfo.BatchCode + " rescan";

                CheckImageCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reload error!\n" + ex.Message, "Error");
            }            
        }

        private void btnActualStrip_Click(object sender, EventArgs e)
        {
            dsvImg.IfFitWindow = false;
            dsvImg.Zoom = 1;
        }

        private void btnFitStrip_Click(object sender, EventArgs e)
        {
            dsvImg.IfFitWindow = true;
        }

        private void btnZoomInStrip_Click(object sender, EventArgs e)
        {
            float zoom = dsvImg.Zoom + 0.1F;
            dsvImg.IfFitWindow = false;
            dsvImg.Zoom = zoom;
        }

        private void btnZoomOutStrip_Click(object sender, EventArgs e)
        {
            float zoom = dsvImg.Zoom - 0.1F;
            dsvImg.IfFitWindow = false;
            dsvImg.Zoom = zoom;
        }

        private void btnRotateRightStrip_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
            {
                int iImgWidth = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer).Width;
                int iImgHeight = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer).Height;
                List<AnnotationData> tempListAnntn = (List<AnnotationData>)_oImgCore.ImageBuffer.GetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation);

                foreach (AnnotationData tempAnnotn in tempListAnntn)
                {
                    int x = iImgHeight - (tempAnnotn.Location.Y + tempAnnotn.Size.Height);
                    int y = tempAnnotn.Location.X;
                    int iWidth = tempAnnotn.Size.Height;
                    int iHeight = tempAnnotn.Size.Width;
                    switch (tempAnnotn.AnnotationType)
                    {
                        case AnnotationType.enumEllipse:
                        case AnnotationType.enumRectangle:
                        case AnnotationType.enumText:
                            tempAnnotn.StartPoint = new System.Drawing.Point(x, y);
                            tempAnnotn.EndPoint = new System.Drawing.Point((tempAnnotn.StartPoint.X + iWidth), (tempAnnotn.StartPoint.Y + iHeight));
                            break;
                        case AnnotationType.enumLine:
                            System.Drawing.Point startPoint = tempAnnotn.StartPoint;
                            x = iImgHeight - startPoint.Y;
                            y = startPoint.X;
                            tempAnnotn.StartPoint = new System.Drawing.Point(x, y);
                            System.Drawing.Point endPoint = tempAnnotn.EndPoint;
                            x = iImgHeight - endPoint.Y;
                            y = endPoint.X;
                            tempAnnotn.EndPoint = new System.Drawing.Point(x, y);
                            break;
                    }
                }
                _oImgCore.ImageProcesser.RotateRight(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            }
        }

        private void btnRotateLeftStrip_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
            {
                int iImgWidth = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer).Width;
                int iImgHeight = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer).Height;
                List<AnnotationData> tempListAnnotn = (List<AnnotationData>)_oImgCore.ImageBuffer.GetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation);

                if (tempListAnnotn != null && tempListAnnotn.Count != 0)
                {
                    foreach (AnnotationData tempAnnotn in tempListAnnotn)
                    {
                        int x = tempAnnotn.Location.Y;
                        int y = iImgWidth - (tempAnnotn.EndPoint.X);
                        int iWidth = (tempAnnotn.EndPoint.Y - tempAnnotn.StartPoint.Y);
                        int iHeight = (tempAnnotn.EndPoint.X - tempAnnotn.StartPoint.X);
                        switch (tempAnnotn.AnnotationType)
                        {
                            case AnnotationType.enumEllipse:
                            case AnnotationType.enumRectangle:
                            case AnnotationType.enumText:
                                tempAnnotn.StartPoint = new System.Drawing.Point(x, y);
                                tempAnnotn.EndPoint = new System.Drawing.Point((tempAnnotn.StartPoint.X + iWidth), (tempAnnotn.StartPoint.Y + iHeight));
                                break;
                            case AnnotationType.enumLine:
                                System.Drawing.Point startPoint = tempAnnotn.StartPoint;
                                x = startPoint.Y;
                                y = iImgWidth - startPoint.X;
                                tempAnnotn.StartPoint = new System.Drawing.Point(x, y);
                                System.Drawing.Point endPoint = tempAnnotn.EndPoint;
                                x = endPoint.Y;
                                y = iImgWidth - endPoint.X;
                                tempAnnotn.EndPoint = new System.Drawing.Point(x, y);
                                break;
                        }
                    }
                }
                _oImgCore.ImageBuffer.SetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation, tempListAnnotn, true);
                _oImgCore.ImageProcesser.RotateLeft(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            }
        }

        private bool updateImageFile(short pCurrImgIdx)
        {
            try
            {
                if (pCurrImgIdx < _stcImgInfo.ImgFile.Length)
                {
                    string sFile = _stcImgInfo.ImgFile[pCurrImgIdx];

                    //List<short> lIdx = new List<short>(1);
                    //lIdx.Add(pCurrImgIdx);
                    //_oImgCore.IO.SaveAsTIFF(sFile, lIdx);
                    byte[] bFile = _oImgCore.IO.SaveImageToBytes(pCurrImgIdx, EnumImageFileFormat.WEBTW_TIF);
                    File.WriteAllBytes(sFile, bFile);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private void mnuCopyStrip_Click(object sender, EventArgs e)
        {
            try
            {
                copyImageNode(tvwBatch.SelectedNode);
                //string nodeInfo = "";
                //string sep = "";

                //string sDBTable = "TDocuScan";

                //if (_fromProc.Trim().ToLower() == "verify")
                //{
                //    sDBTable = "TDocuIndex";
                //}

                //if (tvwBatch.SelectedNode != null)
                //{
                //    TreeNode node = tvwBatch.SelectedNode;
                //    bImgCut = false;

                //    nodeInfo = node.Text.Trim().Substring(0, 4);
                //    sep = node.Tag.ToString().Trim().Substring(0, 4);

                //    if (sep.Trim().ToLower() == "sep1")
                //    {
                //    }
                //    else if (sep.Trim().ToLower() == "sep2")
                //    {
                //    }
                //    else if (nodeInfo.Trim().ToLower() == "page")
                //    {
                //        if (node.Tag.ToString().Trim().Split('|').Length > 1)
                //        {
                //            string rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                //            string docType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                //            string collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                //            sImgInfoCopy = rowid + "|" + oDF.getDocFilenameDB(rowid, sDBTable) + "|" + docType + "|" + collaNum;
                //        }

                //    }
                //}
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuCutStrip_Click(object sender, EventArgs e)
        {
            try
            {
                cutImageNode(tvwBatch.SelectedNode);
                //string msg = "";
                //string nodeInfo = "";
                //string sep = "";

                //string sDBTable = "TDocuScan";

                //if (_fromProc.Trim().ToLower() == "verify")
                //{
                //    sDBTable = "TDocuIndex";
                //}

                //if (tvwBatch.SelectedNode != null)
                //{
                //    TreeNode node = tvwBatch.SelectedNode;

                //    nodeInfo = node.Text.Trim().Substring(0, 4);
                //    sep = node.Tag.ToString().Trim().Substring(0, 4);

                //    if (nodeInfo.Trim().ToLower() == "page")
                //    {
                //        if (MessageBox.Show(this, "Cut an image cannot undo. Confirm to cut the selected image?", "Confirmation", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                //            return;
                //    }
                //    else
                //        return;

                //    bImgCut = true;

                //    if (sep.Trim().ToLower() == "sep1")
                //    {
                //    }
                //    else if (sep.Trim().ToLower() == "sep2")
                //    {
                //    }
                //    else if (nodeInfo.Trim().ToLower() == "page")
                //    {
                //        if (node.Tag.ToString().Trim().Split('|').Length > 1)
                //        {
                //            string rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                //            string docType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                //            string collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                //            string srcFilename = oDF.getDocFilenameDB(rowid, sDBTable);
                //            sImgInfoCopy = rowid + "|" + srcFilename + "|" + docType + "|" + collaNum;

                //            if (File.Exists(srcFilename))
                //            {
                //                oImgCut = new ImageCore();
                //                oImgCut.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                //                oImgCut.IO.LoadImage(srcFilename);

                //                //_oImgCore.IO.CopyToClipborad(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                //                _oImgCore.ImageBuffer.RemoveImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                //                lScanImg.RemoveAt(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                //            }
                //            else
                //            {
                //                msg = "Cut image failed for image source does not exists!";
                //                MessageBox.Show(this, msg, "Message");
                //                return;
                //            }

                //            msg = cutImageFile();

                //            if (msg.Trim() == "")
                //            {
                //                //reloadForEdit();
                //                reloadForCurrRow(_iCurrBatchRowid, _batchCode);
                //            }
                //            else
                //                mGlobal.Write2Log("Cut image.." + msg);
                            
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuReplaceStrip_Click(object sender, EventArgs e)
        {
            bool bOK = true;
            try
            {
                string nodeInfo = "";
                string sep = "";
                bool bValid = false;
                string sExistRowid = "";
                string sExistFilename = "";

                string sDBTable = "TDocuScan";

                if (_fromProc.Trim().ToLower() == "verify")
                {
                    sDBTable = "TDocuIndex";
                }

                if (tvwBatch.SelectedNode != null)
                {
                    TreeNode node = tvwBatch.SelectedNode;
                    selectedNode = node;

                    nodeInfo = node.Text.Trim().Substring(0, 4);
                    sep = node.Tag.ToString().Trim().Substring(0, 4);

                    if (sep.Trim().ToLower() == "sep1")
                    {
                    }
                    else if (sep.Trim().ToLower() == "sep2")
                    {
                    }
                    else if (nodeInfo.Trim().ToLower() == "page")
                    {
                        if (node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            if (sImgInfoCopy.Trim() != "") //Page treeview node image info.
                            {
                                _docType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                                _setNum = node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();
                                _setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);
                                _collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                                iImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                                sExistRowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                sExistFilename = oDF.getDocFilenameDB(sExistRowid, sDBTable);
                                sFrontBack = "F";

                                bValid = true;
                            }
                            else
                                MessageBox.Show(this, "You have not copy or cut an image! Please select a image and copy or cut an image.", "Message");
                        }
                    }

                    if (bValid)
                    {
                        if ((_noSeparator.Trim() == "2" && _docType.Trim() == "") == false)
                        {
                            string sMsg = replaceImageFile(sExistRowid, sExistFilename);

                            if (sMsg.Trim() == "")
                            {
                                reloadForEdit(); //Refresh.

                                TreeNode[] oNodes = tvwBatch.Nodes.Find(selectedNode.Name, true);
                                if (oNodes.Length > 0)
                                {
                                    tvwBatch.SelectedNode = oNodes[0];
                                    tvwBatch.SelectedNode.EnsureVisible();
                                }

                                //tvwBatch.Refresh();
                            }
                            else
                                MessageBox.Show(this, "Paste image error! " + Environment.NewLine + sMsg, "Message");
                        }
                        else
                            MessageBox.Show(this, "Paste image failed for image paste to document type is blank!", "Message");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Replace image error! " + ex.Message, "Message");
            }
        }

        private void mnuPasteStrip_Click(object sender, EventArgs e)
        {
            try
            {
                string nodeInfo = "";
                string sep = "";
                bool bValid = false;

                if (tvwBatch.SelectedNode != null)
                {
                    TreeNode node = tvwBatch.SelectedNode;
                    selectedNode = node;

                    nodeInfo = node.Text.Trim().Substring(0, 4);
                    sep = node.Tag.ToString().Trim().Substring(0, 4);

                    if (sep.Trim().ToLower() == "sep1")
                    {
                        if (_batchType.Trim().ToLower() == "batch" && node.Tag.ToString().Trim().Split('|').Length > 1) //Doc Set.
                        {
                            if (sImgInfoCopy.Trim() != "") //Page treeview node image info.
                            {
                                if (sImgInfoCopy.Split('|').Length == 4)
                                {
                                    _setNum = node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                                    _setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);

                                    _docType = sImgInfoCopy.Split('|').GetValue(2).ToString();
                                    _collaNum = sImgInfoCopy.Split('|').GetValue(3).ToString();
                                    iImgSeq = oDF.getLastImageSeqDB(_currScanProj, _appNum, _setNum, _collaNum, _batchCode);

                                    bValid = true;
                                }
                                else
                                    MessageBox.Show(this, "You have not copy or cut an image!! Please select an image and copy or cut an image.", "Message");
                            }
                            else
                                MessageBox.Show(this, "You have not copy or cut an image! Please select an image and copy or cut an image.", "Message");
                        }
                    }
                    else if (sep.Trim().ToLower() == "sep2")
                    {
                        if (_batchType.Trim().ToLower() == "batch" && node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            if (sImgInfoCopy.Trim() != "") //Page treeview node image info.
                            {
                                _docType = node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                                _setNum = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                                _setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);
                                _collaNum = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                iImgSeq = oDF.getLastImageSeqDB(_currScanProj, _appNum, _setNum, _collaNum, _batchCode);

                                bValid = true;
                            }
                            else
                                MessageBox.Show(this, "You have not copy or cut an image! Please select an image and copy or cut an image.", "Message");
                        }
                    }
                    else if (nodeInfo.Trim().ToLower() == "page")
                    {
                        if (node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            if (sImgInfoCopy.Trim() != "") //Page treeview node image info.
                            {
                                _docType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                                _setNum = node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();
                                _setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);
                                _collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                                iImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());
                                sFrontBack = "F";

                                bValid = true;
                            }
                            else
                                MessageBox.Show(this, "You have not copy or cut an image! Please select an image and copy or cut an image.", "Message");
                        }
                    }

                    if (bValid)
                    {
                        //if (_docType.Trim() != "" || _noSeparator.Trim() == "0")
                        if ((_noSeparator.Trim() == "2" && _docType.Trim() == "") == false)
                        {
                            string sMsg = pasteImageFile();

                            if (sMsg.Trim() == "")
                            {
                                reloadForEdit(); //Refresh.
                                //reloadForCurrRow(_iCurrBatchRowid, _batchCode);

                                TreeNode[] oNodes = tvwBatch.Nodes.Find(selectedNode.Name, true);
                                if (oNodes.Length > 0)
                                {
                                    tvwBatch.SelectedNode = oNodes[0];
                                    tvwBatch.SelectedNode.EnsureVisible();
                                }

                                //tvwBatch.Refresh();
                            }
                            else
                                MessageBox.Show(this, "Paste image error! " + Environment.NewLine + sMsg, "Message");
                        }
                        else
                            MessageBox.Show(this, "Paste image failed for image paste to document type is blank!", "Message");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Paste image error! " + ex.Message, "Message");
            }
        }

        private string cutImageFile()
        {
            bool bOK = true;
            string sMsg = "";
            try
            {
                string sDBTable = "TDocuScan";

                if (_fromProc.Trim().ToLower() == "verify")
                {
                    sDBTable = "TDocuIndex";
                }

                string sSrcDir = "";
                string sSrcFilename = sImgInfoCopy.Split('|').GetValue(1).ToString();

                if (bImgCut) //Cut and Paste event.
                {
                    try
                    {
                        File.Delete(sSrcFilename);

                        sSrcDir = mGlobal.getDirectoriesFromPath(sSrcFilename);
                        if (Directory.Exists(sSrcDir) && Directory.GetFiles(sSrcDir).Length == 0)
                            Directory.Delete(sSrcDir);

                        int rowid = Convert.ToInt32(sImgInfoCopy.Split('|').GetValue(0).ToString());
                        bOK = oDF.deleteRecordDB(rowid, sDBTable);

                        if (bOK)
                        {
                            if (_batchType.Trim().ToLower() == "set")
                                _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                            else
                                _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                            //_totPageCnt = _totPageCnt - 1;

                            if (_batchType.Trim().ToLower() == "set")
                            {
                                _docType = sCurrSetNum;
                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "", sCurrSetNum);
                                //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "");
                            }
                            else
                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt);

                            //if (_fromProc.Trim().ToLower() == "verify")
                            //{
                            //    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sCurrSetNum, _collaNum, _batchCode, iImgSeq);
                            //}
                            //else
                            //{
                            //    oDF.reorderScanImageSeq(_currScanProj, _appNum, sCurrSetNum, _collaNum, _batchCode, iImgSeq);
                            //}
                        }
                    }
                    catch (DirectoryNotFoundException dex) { }
                    catch (Exception ex)
                    {
                        bOK = false;
                        sMsg = ex.Message;
                    }
                }

            }
            catch (Exception ex)
            {
                bOK = false;
                sMsg = ex.Message;
            }
            return sMsg;
        }

        private string replaceImageFile(string pExistRowid, string pExistFilename)
        {
            bool bOK = true;
            string sMsg = "";
            //string actn = "";
            //int iImgSeq = 1;
            try
            {
                //if (bImgCut) actn = "c"; else actn = "a";
                string sDBTable = "TDocuScan";

                if (_fromProc.Trim().ToLower() == "verify")
                {
                    sDBTable = "TDocuIndex";
                }

                string sSrcFilename = sImgInfoCopy.Split('|').GetValue(1).ToString();

                _imgFilename = pExistFilename;

                //iImgSeq = oDF.getValidImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, iCurrImgSeq);

                if (bImgCut)
                {
                    if (oImgCut != null)
                    {
                        //ImageCore oImgCutCore = new ImageCore();
                        //oImgCutCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;
                        //oImgCutCore.IO.LoadImage(oImgCut);                        
                        List<short> lIdx = new List<short>(1);
                        lIdx.Add(0);
                        oImgCut.IO.SaveAsTIFF(_imgFilename, lIdx);
                        //oImgCut.Dispose(); //Remarks for allow multiple cut and paste.

                        if (File.Exists(_imgFilename))
                        {
                            oImgCut.IO.LoadImage(_imgFilename);

                            short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                            _oImgCore.IO.LoadImage(_imgFilename);
                            short iLastImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                            _oImgCore.ImageBuffer.SwitchImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                            _oImgCore.ImageBuffer.RemoveImage(iLastImgIdx);

                            if (lScanImg.Count <= _oImgCore.ImageBuffer.HowManyImagesInBuffer)
                                lScanImg[iCurrImgIdx] = _imgFilename;
                        }

                        if (_batchType.Trim().ToLower() == "set")
                        {
                            _docType = sCurrSetNum; //doctype = setnum for set and required for reorderDocTypeScanImageSeq function below.
                            if (_noSeparator.Trim() == "0") //setnum follows scanning setnum for zero separator, thus do not need to update otherwise affect image sequence below.
                                bOK = updateDocImageDB(pExistRowid, _imgFilename, sDBTable, "", sFrontBack);
                            else
                                bOK = updateDocImageDB(pExistRowid, _imgFilename, sDBTable, sCurrSetNum, sFrontBack, _docType);
                        }
                        else
                        {
                            if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                bOK = updateDocImageDB(pExistRowid, _imgFilename, sDBTable, sCurrSetNum, sFrontBack, _docType);
                            else
                                bOK = updateDocImageDB(pExistRowid, _imgFilename, sDBTable, "", sFrontBack);
                        }

                        string sSetNo = sCurrSetNum;
                        if (_noSeparator.Trim() == "0") sSetNo = 1.ToString(sSetNumFmt);

                        if (bOK)
                        {
                            if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq, _docType);
                                }
                                else
                                {
                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq, _docType);
                                }
                            }
                            else
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq);
                                }
                                else
                                {
                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq);
                                }
                            }
                        }
                    }
                    else
                        sMsg = "Source image does not exists!";
                }
                else
                {
                    if (File.Exists(sSrcFilename))
                    {
                        try
                        {
                            if (!File.Exists(_imgFilename))
                            {
                                Directory.CreateDirectory(mGlobal.getDirectoriesFromPath(_imgFilename));

                                File.Copy(sSrcFilename, _imgFilename, true);
                            }
                            else
                                File.Copy(sSrcFilename, _imgFilename, true);

                            if (File.Exists(_imgFilename))
                            {
                                short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                _oImgCore.IO.LoadImage(_imgFilename);
                                short iLastImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                _oImgCore.ImageBuffer.SwitchImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                                _oImgCore.ImageBuffer.RemoveImage(iLastImgIdx);
                            }
                        }
                        catch (Exception ex)
                        {
                            bOK = false;
                            sMsg = ex.Message;
                        }

                        if (bOK)
                        {
                            bOK = false;
                            if (_batchType.Trim().ToLower() == "set")
                            {
                                _docType = sCurrSetNum; //doctype = setnum for set and required for reorderDocTypeScanImageSeq function below.
                                if (_noSeparator.Trim() == "0") //setnum follows scanning setnum for zero separator, thus do not need to update otherwise affect image sequence below.
                                    bOK = updateDocImageDB(pExistRowid, _imgFilename, sDBTable, "", sFrontBack);
                                else
                                    bOK = updateDocImageDB(pExistRowid, _imgFilename, sDBTable, sCurrSetNum, sFrontBack, _docType);
                            }
                            else
                            {
                                if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                    bOK = updateDocImageDB(pExistRowid, _imgFilename, sDBTable, sCurrSetNum, sFrontBack, _docType);
                                else
                                    bOK = updateDocImageDB(pExistRowid, _imgFilename, sDBTable, "", sFrontBack);
                            }

                            string sSetNo = sCurrSetNum;
                            if (_noSeparator.Trim() == "0") sSetNo = 1.ToString(sSetNumFmt);

                            if (bOK)
                            {
                                if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq, _docType);
                                    }
                                    else
                                    {
                                        oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq, _docType);
                                    }
                                }
                                else
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq);
                                    }
                                    else
                                    {
                                        oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq);
                                    }
                                }
                            }
                        }

                    }
                    else
                        sMsg = "Source image does not exists!";
                }

            }
            catch (Exception ex)
            {
                sMsg = ex.Message;
                throw new Exception(ex.Message);
            }

            return sMsg;
        }

        private string pasteImageFile()
        {
            bool bOK = true;
            string sMsg = "";
            string actn = "";
            int iImgSeq = 1;
            try
            {
                if (bImgCut) actn = "c"; else actn = "a";

                string sBatchPath = mGlobal.replaceValidChars(_batchCode, "_");
                string sDir = "";
                string sSrcFilename = sImgInfoCopy.Split('|').GetValue(1).ToString();
                string sNewFilename = sBatchPath.Trim() + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + actn + ".tiff";

                if (_batchType.Trim().ToLower() == "set")
                {
                    if (_fromProc.Trim().ToLower() == "verify")
                    {
                        sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //if (_docType.Trim() == "")
                        //{
                        //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //}
                        //else
                        //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                    }
                    else
                    {
                        sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //if (_docType.Trim() == "")
                        //{
                        //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //}
                        //else
                        //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                    }
                }
                else //Batch.
                {
                    if (_fromProc.Trim().ToLower() == "verify")
                    {
                        if (_docType.Trim() == "")
                        {
                            sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        }
                        else
                            sDir = mGlobal.addDirSep(staMain.stcProjCfg.IndexDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                    }
                    else
                    {
                        if (_docType.Trim() == "")
                        {
                            sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        }
                        else
                            sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                    }
                }

                _imgFilename = mGlobal.addDirSep(sDir) + sNewFilename;

                //iImgSeq = oDF.getValidImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, iCurrImgSeq);

                if (bImgCut)
                {
                    if (oImgCut != null)
                    {
                        //ImageCore oImgCutCore = new ImageCore();
                        //oImgCutCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;
                        //oImgCutCore.IO.LoadImage(oImgCut);

                        if (!Directory.Exists(sDir))
                            Directory.CreateDirectory(sDir);

                        List<short> lIdx = new List<short>(1);
                        lIdx.Add(0);
                        oImgCut.IO.SaveAsTIFF(_imgFilename, lIdx);
                        //oImgCut.Dispose(); //Remarks for allow multiple cut and paste.

                        if (File.Exists(_imgFilename))
                        {
                            oImgCut.IO.LoadImage(_imgFilename);

                            short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                            _oImgCore.IO.LoadImage(_imgFilename);

                            if (sDrag.ToLower() == "move") //Drag move, the source treeview node is remained, e.g reorder image.
                            {
                                short sNewIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                _oImgCore.ImageBuffer.SwitchImage(sNewIdx, iCurrImgIdx);

                                if (lScanImg.Count < _oImgCore.ImageBuffer.HowManyImagesInBuffer)
                                {
                                    lScanImg.Insert(iCurrImgIdx, _imgFilename);
                                    //lScanImg.RemoveAt(iCurrImgIdx);
                                }
                            }
                            else
                            {
                                //_oImgCore.ImageBuffer.SwitchImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                                _oImgCore.ImageBuffer.MoveImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);

                                lScanImg.Insert(iCurrImgIdx, _imgFilename);
                            }
                        }

                        if (saveDocScanDB("R", _imgFilename))
                        {
                            if (_batchType.Trim().ToLower() == "set")
                                _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                            else
                                _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                            //_totPageCnt += 1;

                            if (_batchType.Trim().ToLower() == "set")
                            {
                                _docType = sCurrSetNum;
                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "", sCurrSetNum);
                                //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "");
                            }
                            else
                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt);

                            string sSetNo = _setNum;
                            if (_noSeparator.Trim() == "0") sSetNo = 1.ToString(sSetNumFmt);

                            if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq, _docType);
                                }
                                else
                                {
                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq, _docType);
                                }
                            }
                            else
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq);
                                }
                                else
                                {
                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq);
                                }
                            }
                        }
                    }
                    else
                        sMsg = "Source image does not exists!";
                }
                else
                {
                    if (File.Exists(sSrcFilename))
                    {
                        if (!Directory.Exists(sDir))
                            Directory.CreateDirectory(sDir);

                        try
                        {
                            File.Copy(sSrcFilename, _imgFilename);

                            if (File.Exists(_imgFilename))
                            {
                                short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                _oImgCore.IO.LoadImage(_imgFilename);

                                _oImgCore.ImageBuffer.MoveImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);

                                lScanImg.Insert(iCurrImgIdx, _imgFilename);
                            }
                        }
                        catch (Exception ex)
                        {
                            bOK = false;
                            sMsg = ex.Message;
                        }

                        if (bOK)
                        {
                            if (saveDocScanDB("R", _imgFilename))
                            {
                                if (!bImgCut) //Copy and Paste event.
                                {
                                    if (_batchType.Trim().ToLower() == "set")
                                        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                    else
                                        _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                    //_totPageCnt += 1;

                                    if (_batchType.Trim().ToLower() == "set")
                                    {
                                        _docType = sCurrSetNum;
                                        bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "", sCurrSetNum);
                                        //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt, "");
                                    }
                                    else
                                        bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPageCnt);
                                }

                                string sSetNo = _setNum;
                                if (_noSeparator.Trim() == "0") sSetNo = 1.ToString(sSetNumFmt);

                                if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq, _docType);
                                    }
                                    else
                                    {
                                        oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq, _docType);
                                    }
                                }
                                else
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq);
                                    }
                                    else
                                    {
                                        oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNo, _collaNum, _batchCode, iImgSeq);
                                    }
                                }
                            }
                        }
                        
                    }
                    else sMsg = "Source image does not exists!";
                }

            }
            catch (Exception ex)
            {
                sMsg = ex.Message;
                throw new Exception(ex.Message);
            }

            return sMsg;
        }

        //public bool updateDocImageDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pDocType, string pDocFilename, string pDBTableName)
        public bool updateDocImageDB(string pRowid, string pDocFilename, string pDBTableName, string pSetNum = "", string pFrontBack = "", string pDocType = "")
        {
            bool bUpdated = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + pDBTableName.Trim().Replace("'", "") + " ";
                sSQL += "SET docimage='" + pDocFilename.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += ",setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                if (pFrontBack.Trim() != "")
                    sSQL += ",page='" + pFrontBack.Trim().Replace("'", "") + "' ";

                if (pDocType.Trim() != "")
                    sSQL += ",doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                sSQL += "WHERE rowid='" + pRowid.Trim().Replace("'", "") + "' ";
                //sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                //sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                //sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                bUpdated = mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                bUpdated = false;
                //throw new Exception(ex.Message);                            
            }

            return bUpdated;
        }             
        
        private void mnuImpStrip_Click(object sender, EventArgs e)
        {
            try
            {
                string nodeInfo = "";
                string sep = "";

                if (tvwBatch.SelectedNode != null)
                {
                    TreeNode node = tvwBatch.SelectedNode;
                    bImgCut = false;

                    nodeInfo = node.Text.Trim().Substring(0, 4);
                    sep = node.Tag.ToString().Trim().Substring(0, 4);

                    string rowid = "";
                    sImgInfoCopy = "";

                    if (sep.Trim().ToLower() == "sep1")
                    {
                        if (_batchType.Trim().ToLower() == "set" && node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            rowid = "0";
                            sImgInfoCopy = rowid + "|" + ofdImportFile.FileName;
                        }
                    }
                    else if (sep.Trim().ToLower() == "sep2")
                    {
                        if ((_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box") && node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            rowid = "0";
                            sImgInfoCopy = rowid + "|" + ofdImportFile.FileName;
                        }
                    }
                    else if (nodeInfo.Trim().ToLower() == "page")
                    {
                        if (node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                            sImgInfoCopy = rowid + "|" + ofdImportFile.FileName;
                        }
                    }

                    if (sImgInfoCopy.Trim() != "")
                    {
                        ofdImportFile.InitialDirectory = "C:\\";
                        ofdImportFile.Filter = "(*.tiff)|*.tiff";

                        if (ofdImportFile.ShowDialog(this) == DialogResult.Cancel)
                            return;

                        sImgInfoCopy = rowid + "|" + ofdImportFile.FileName;
                        ofdImportFile.Dispose();

                        sFrontBack = "F";
                        mnuPasteStrip_Click(this, e);

                        tvwBatch.SelectedNode = node;
                        tvwBatch.Focus();
                    }
                    else
                        MessageBox.Show(this, "Please select either document type or page for import an image.", "Message");
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuDeleteSetStrip_Click(object sender, EventArgs e)
        {
            bool bOK = true;
            string sSetNum = "";
            bool bValid = false;
            try
            {
                if ((_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box") && tvwBatch.SelectedNode.Level == 1)
                {
                    bValid = true;
                }
                else if (_batchType.Trim().ToLower() == "set")
                {
                    if (_noSeparator.Trim() == "2" && tvwBatch.SelectedNode.Level == 0)
                        bValid = true;
                    else if (_noSeparator.Trim() == "1" && tvwBatch.SelectedNode.Level == 1)
                        bValid = true;
                }

                if (tvwBatch.SelectedNode != null)
                {
                    if (bValid)
                    {
                        int iNodes = 0;
                        if (tvwBatch.Nodes.Count > 0)
                        {
                            iNodes = tvwBatch.Nodes[0].Nodes.Count;
                        }

                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 || iNodes > 0)
                        {
                            int i = 0;
                            bool bConfirm = false;

                            sSetNum = tvwBatch.SelectedNode.Tag.ToString().Trim().Split('|').GetValue(1).ToString();

                            if (MessageBox.Show(this, "Confirm to delete selected set " + sSetNum + " and all its images?", "Delete Set Images Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                bConfirm = true;

                            if (bConfirm)
                            {
                                string sDBTable = "TDocuScan";

                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    sDBTable = "TDocuIndex";
                                }

                                string sBatchPath = mGlobal.replaceValidChars(_batchCode, "_");
                                string sDirSet = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + sSetNum + @"\";

                                ScanStatusBar1.Text = "Deleting set";
                                //btnScan.Enabled = false;
                                enableScan(false);
                                btnDeleteStrip.Enabled = false;
                                btnDeleteAllStrip.Enabled = false;

                                int j = 0;
                                if (Directory.Exists(sDirSet))
                                {
                                    DirectoryInfo dirInfo = new DirectoryInfo(sDirSet);

                                    FileInfo[] files = dirInfo.GetFiles();
                                    while (i < files.Length)
                                    {
                                        files[i].Delete();

                                        i += 1;
                                    }

                                    foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                                    {
                                        dir.Delete(true);
                                    }

                                    try
                                    {
                                        Directory.Delete(sDirSet);
                                    }
                                    catch (DirectoryNotFoundException dnfex)
                                    { }
                                }

                                if (i > 0 || j > 0 || iNodes > 0)
                                {
                                    if (bOK)
                                    {
                                        bOK = deleteSetImagesRecordDB(_batchCode, sSetNum, sDBTable);

                                        if (bOK) //Reload all.
                                        {
                                            oDF.deleteSetIndexedRecordDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete set indexed value if any.

                                            if (_noSeparator.Trim() == "C")
                                            {
                                                oDF.deleteAllIDPKeyFieldValuesDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete indexed key field value if any.
                                                oDF.deleteAllIDPKeyValuesDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete indexed key value if any.
                                                oDF.deleteAllIDPTableValuesDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete table value if any.
                                                oDF.deleteAllIDPTableColsDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete table columns structure if any.
                                                oDF.deleteAllIDPModelDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete IDP Model if any.
                                            }

                                            _totPageCnt = 0;
                                            string sStatus = "4";
                                            string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                            //iNodes = getSetTotPage();

                                            if (_fromProc.Trim().ToLower() == "verify")
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                    _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sSetNum);
                                                else
                                                    _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                            }
                                            else
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                    _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sSetNum);
                                                else
                                                    _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                            }

                                            //_totPageCnt = _totPageCnt - iNodes;
                                            if (_totPageCnt < 0) _totPageCnt = 0;

                                            if (_batchType.Trim().ToLower() == "set")
                                            {
                                                sStatus = "3";
                                                _docType = sSetNum;
                                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "4", _totPageCnt, sStatus, "", "", sModifiedDate, sSetNum);
                                                //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "4", _totPageCnt, sStatus, "", "", sModifiedDate);
                                            }
                                            else
                                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "4", _totPageCnt, sStatus, "", "", sModifiedDate);

                                            //if (bOK) //deleteBatchRecordDB(_batchCode);
                                            bOK = deleteBatchSetRecordDB(_batchCode, sSetNum);

                                            if (bOK)
                                                MessageBox.Show("All image are deleted successfully!");
                                            else
                                                MessageBox.Show("All image delete unsuccessful!");

                                            initImageAllInfo();

                                            if (sIsAddOn.Trim() != "")
                                            {
                                                this.MinimizeBox = true;
                                                ScanStatusBar1.Text = "Addon";
                                                this.Text = ScanStatusBar1.Text + " " + this.Text;

                                                if (staMain.stcImgInfoBySet.ImgIdx.Length > 0 || staMain.stcImgInfoBySet.BatchCode.Trim() != "")
                                                {
                                                    _batchCode = staMain.stcImgInfoBySet.BatchCode;
                                                    //_setNum = staMain.stcImgInfoBySet.SetNum;
                                                    if (staMain.stcImgInfoBySet.SetNum.Length > 0)
                                                        _setNum = staMain.stcImgInfoBySet.SetNum[staMain.stcImgInfoBySet.CurrImgIdx];  //Current image index.
                                                    else
                                                        _setNum = staMain.stcImgInfoBySet.SetNum[0];

                                                    _setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);

                                                    if (staMain.stcImgInfoBySet.CollaNum.Length > 0)
                                                        _collaNum = staMain.stcImgInfoBySet.CollaNum[staMain.stcImgInfoBySet.CurrImgIdx];  //Current image index.
                                                    else
                                                        _collaNum = staMain.stcImgInfoBySet.CollaNum[0];

                                                    _scanStatus = "A";

                                                    _iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, _setNum, "'1'");

                                                    loadRejectedImagesFrmDisk(_iCurrBatchRowid);

                                                    CheckImageCount();

                                                    if (sIsAddOn.Trim().ToLower() == "rejected")
                                                    {
                                                        ScanStatusBar1.Text = "Reverted Addon";
                                                        loadRejectedBatchSetTreeview(); //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                                                    }
                                                    else
                                                        loadBatchSetRescanTreeview();
                                                    

                                                    if (tvwBatch.Nodes.Count > 0)
                                                        tvwBatch.ExpandAll();
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Current Document set info is not available!", "Message");
                                                    this.Close();
                                                }
                                            }
                                            else
                                            {
                                                _iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, sCurrSetNum, "'1'");

                                                loadRejectedImagesFrmDisk(_iCurrBatchRowid);

                                                CheckImageCount();

                                                loadBatchSetRescanTreeview();
                                                if (tvwBatch.Nodes.Count > 0)
                                                    tvwBatch.ExpandAll();
                                            }

                                            //setImgInfo(0);
                                        }
                                    }
                                    else
                                        MessageBox.Show("All images delete unsuccessful!");
                                }
                            }
                        }
                    }
                    else
                        MessageBox.Show(this, "Please select a document set to delete the document set!", "Message");
                }
                else
                    MessageBox.Show(this, "Please select a document set to delete the document set!", "Message");
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ScanStatusBar1.Text = "Deleting set";
                //btnScan.Enabled = true;
                enableScan(true);
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;
            }
        }

        private int getSetTotPage()
        {
            int iCnt = 0;

            if (tvwBatch.SelectedNode != null)
            {
                if ((_batchType.Trim().ToLower() == "batch" && tvwBatch.SelectedNode.Level == 1)
                    || (_batchType.Trim().ToLower() == "box" && tvwBatch.SelectedNode.Level == 1)
                    || (_batchType.Trim().ToLower() == "set" && tvwBatch.SelectedNode.Level == 0))
                {
                    int i = 0;
                    while (i < tvwBatch.SelectedNode.Nodes.Count) //doc. types.
                    {
                        iCnt += tvwBatch.SelectedNode.Nodes[i].Nodes.Count;
                        i += 1;
                    }
                }
            }
            return iCnt;
        }

        private bool deleteSetImagesRecordDB(string pBatchCode, string pSetNum, string pDBTableName)
        {
            bool ret = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + pDBTableName + " ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum + "' ";

                ret = mGlobal.objDB.UpdateRows(sSQL, false);
            }
            catch (Exception ex)
            {
            }
            return ret;
        }

        private bool deleteBatchSetRecordDB(string pBatchCode, string pSetNum)
        {
            bool ret = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                ret = mGlobal.objDB.UpdateRows(sSQL, false);
            }
            catch (Exception ex)
            {
            }
            return ret;
        }

        private void tvwBatch_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && tvwBatch.Nodes.Count > 0)
            {
                cmnuTvwStrip.Visible = true;
                cmnuTvwStrip.Show(tvwBatch, new System.Drawing.Point(e.X, e.Y));
            }
        }

        private void tvwBatch_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                tvwBatch.SelectedNode = e.Node;
        }

        private void mnuDeleteImgStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (tvwBatch.SelectedNode != null)
                {
                    btnDeleteStrip_Click(this, e);
                }
                else
                    MessageBox.Show("Please select an image to delete.", "Message");
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuImpRepStrip_Click(object sender, EventArgs e)
        {
            try
            {
                string nodeInfo = "";
                string sep = "";

                if (tvwBatch.SelectedNode != null)
                {
                    TreeNode node = tvwBatch.SelectedNode;
                    bImgCut = false;

                    nodeInfo = node.Text.Trim().Substring(0, 4);
                    sep = node.Tag.ToString().Trim().Substring(0, 4);

                    string rowid = "";
                    sImgInfoCopy = "";

                    if (sep.Trim().ToLower() == "sep1")
                    {
                        if (_batchType.Trim().ToLower() == "set" && node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            rowid = "0";
                            sImgInfoCopy = rowid + "|" + ofdImportFile.FileName;
                        }
                    }
                    else if (sep.Trim().ToLower() == "sep2")
                    {
                        if ((_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box") && node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            rowid = "0";
                            sImgInfoCopy = rowid + "|" + ofdImportFile.FileName;
                        }
                    }
                    else if (nodeInfo.Trim().ToLower() == "page")
                    {
                        if (node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                            sImgInfoCopy = rowid + "|" + ofdImportFile.FileName;
                        }
                    }

                    if (sImgInfoCopy.Trim() != "")
                    {
                        ofdImportFile.InitialDirectory = "C:\\";
                        ofdImportFile.Filter = "(*.tiff)|*.tiff";
                        ofdImportFile.RestoreDirectory = false;

                        if (ofdImportFile.ShowDialog(this) == DialogResult.Cancel)
                            return;

                        sImgInfoCopy = rowid + "|" + ofdImportFile.FileName;
                        ofdImportFile.Dispose();

                        mnuReplaceStrip_Click(this, e);

                        //tvwBatch.Focus();
                        tvwBatch.SelectedNode = node;
                        //tvwBatch.SelectedNode.EnsureVisible();                        

                    }
                    else
                        MessageBox.Show(this, "Please select a page for import and replace an image.", "Message");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void mnuScanRepStrip_Click(object sender, EventArgs e)
        {
            TreeNode node = null;
            try
            {
                string nodeInfo = "";
                string sep = "";
                string sDir = "";
                string sFileName = "";
                string sFileExt = "tiff";

                if (tvwBatch.SelectedNode != null)
                {
                    node = tvwBatch.SelectedNode;

                    //btnScan.Enabled = false;
                    enableScan(false);
                    //btnSend.Enabled = false;
                    SendStripBtn.Enabled = false;
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;

                    BoxNoStripTxt.ReadOnly = true;
                    boxLabelStripTxt.ReadOnly = true;

                    this.AcquireImageReplace();

                    if (oImgCut != null)
                    {

                        bImgCut = true;

                        nodeInfo = node.Text.Trim().Substring(0, 4);
                        sep = node.Tag.ToString().Trim().Substring(0, 4);

                        string rowid = "";
                        sImgInfoCopy = "";

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");

                        if (_batchType.Trim().ToLower() == "set")
                        {
                            sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            //if (_docType.Trim() == "")
                            //{
                            //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            //}
                            //else
                            //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }
                        else //Batch.
                        {
                            if (_docType.Trim() == "")
                            {
                                sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            }
                            else
                                sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }

                        sFileName = sBatchPath.Trim() + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;

                        if (sep.Trim().ToLower() == "sep1")
                        {
                            if (_batchType.Trim().ToLower() == "set" && node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = "0";
                                sImgInfoCopy = rowid + "|" + sFileName;
                            }
                        }
                        else if (sep.Trim().ToLower() == "sep2")
                        {
                            if ((_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box") && node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = "0";
                                sImgInfoCopy = rowid + "|" + sFileName;
                            }
                        }
                        else if (nodeInfo.Trim().ToLower() == "page")
                        {
                            if (node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                sImgInfoCopy = rowid + "|" + sFileName;
                            }
                        }

                        if (sImgInfoCopy.Trim() != "")
                        {
                            mnuReplaceStrip_Click(this, e);
                        }
                    }
                }
                else
                    MessageBox.Show(this, "Please select a page for scan and replace an image.", "Message");
            }
            catch (Exception ex)
            {
            }
            finally
            {
                BoxNoStripTxt.ReadOnly = false;
                boxLabelStripTxt.ReadOnly = false;

                //btnScan.Enabled = true;
                enableScan(true);
                //btnSend.Enabled = true;
                SendStripBtn.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                //tvwSet.Focus();
                if (node != null) tvwBatch.SelectedNode = node;
                //tvwSet.SelectedNode.EnsureVisible();
            }
        }

        private bool AcquireImageReplace(bool pBarcodeOnly = false)
        {
            bool bRet = false;
            Dynamsoft.TWAIN.TwainManager oTM = MDIMain.oTMgr; //new Dynamsoft.TWAIN.TwainManager(MDIMain.strProductKey);
            clsTMScan oTMScan = new clsTMScan(this);
            try
            {
                if (oTM.SourceCount == 0)
                {
                    MessageBox.Show("Scanner source does not exists!");
                    //btnScan.Enabled = true;
                    enableScan(true);
                    //btnSend.Enabled = true;
                    SendStripBtn.Enabled = true;
                    btnDeleteStrip.Enabled = true;
                    btnDeleteAllStrip.Enabled = true;

                    BoxNoStripTxt.ReadOnly = false;
                    boxLabelStripTxt.ReadOnly = false;

                    return false;
                }

                // Select the source for TWAIN
                var srcIndex = -1;

                if (oTM.SourceCount < 0)
                {
                    MessageBox.Show(this, "There is no scanner detected!\n " +
                        "Please select scanner source at menu File>Scanner Source.", "Information");

                    return false;
                }

                // Select the source for TWAIN
                for (short i = 0; i < oTM.SourceCount; i++)
                {
                    if (mbCustom && oTM.SourceNameItems(i).Trim() == sCustSrc.Trim())
                    {
                        srcIndex = i;
                        break;
                    }
                    else if (oTM.SourceNameItems(i) != "") continue;

                    srcIndex = i;
                    break;
                }

                bIsDuplexEnabled = false; //Set to False due to this function only used by Image handling, do not need duplex scanning.

                oTM.CloseSource();
                oTM.CloseSourceManager();
                oTM.SelectSourceByIndex(srcIndex == -1 ? 0 : srcIndex);
                oTM.OpenSource();
                oTM.IfDisableSourceAfterAcquire = true;
                oTM.XferCount = 1;
                oTM.IfShowUI = false;
                oTM.IfDuplexEnabled = bIsDuplexEnabled;

                if (mbCustomScan)
                {
                    if (sCustPixelType.Trim() == "Color")
                    {
                        oTM.PixelType = TWICapPixelType.TWPT_RGB;
                        oTM.BitDepth = 24;
                    }
                    else if (sCustPixelType.Trim() == "Gray")
                    {
                        oTM.PixelType = TWICapPixelType.TWPT_GRAY;
                        oTM.BitDepth = 8;
                    }
                    else //Black & White
                    {
                        oTM.PixelType = TWICapPixelType.TWPT_BW;
                        oTM.BitDepth = 1;
                    }

                    oTM.Resolution = iCustResolutn;
                }
                else
                {
                    oTM.PixelType = TWICapPixelType.TWPT_BW;
                    oTM.BitDepth = 1;
                    oTM.Resolution = 300;
                }

                //btnScan.Enabled = false;
                enableScan(false);
                //btnSend.Enabled = false;
                SendStripBtn.Enabled = false;
                btnDeleteStrip.Enabled = false;
                btnDeleteAllStrip.Enabled = false;

                if (pBarcodeOnly)
                {
                    oTMScan.ScanBarcodeOnly = true;
                }

                bRet = oTM.AcquireImage(oTMScan as Dynamsoft.TWAIN.Interface.IAcquireCallback);

                if (!bRet)
                {
                    MessageBox.Show("An error occurred while scanning.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (oTMScan.ScanBarcodeOnly)
                {
                    if (oTMScan.sErrMsg.Trim() != String.Empty)
                    {
                        _docType = "";
                        sErrMsg = oTMScan.sErrMsg.Trim();
                        MessageBox.Show("An error occurred " + oTMScan.sErrMsg.Trim(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    else
                    {
                        _docType = oTMScan.DocType.Trim();
                        oSepBmp = oTMScan.ScanBmp;
                    }
                }

                oImgCut = oTMScan.ImageScan;
                sFrontBack = oTMScan.FrontBack;
            }
            catch (Dynamsoft.TWAIN.TwainException ex)
            {
                MessageBox.Show("An exception occurs: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //btnScan.Enabled = true;
                enableScan(true);
                //btnSend.Enabled = true;
                SendStripBtn.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                if (oTM != null) oTM.Dispose();
                //if (oTMScan != null) oTMScan.Dispose();
                MDIMain.bIsComplete = true;
            }
            return bRet;
        }

        private void btnMirrorStrip_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
            {
                int iImgWidth = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer).Width;
                int iImgHeight = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer).Height;
                List<AnnotationData> tempListAnnotn = (List<AnnotationData>)_oImgCore.ImageBuffer.GetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation);

                int x = 0; System.Drawing.Point startPoint, endPoint;
                foreach (AnnotationData tempAnno in tempListAnnotn)
                {
                    switch (tempAnno.AnnotationType)
                    {
                        case AnnotationType.enumRectangle:
                        case AnnotationType.enumEllipse:
                        case AnnotationType.enumText:
                            x = iImgWidth - tempAnno.Location.X - tempAnno.Size.Width;
                            startPoint = new System.Drawing.Point(x, tempAnno.StartPoint.Y);
                            endPoint = new System.Drawing.Point((startPoint.X + tempAnno.Size.Width), (startPoint.Y + tempAnno.Size.Height));
                            tempAnno.StartPoint = startPoint;
                            tempAnno.EndPoint = endPoint;
                            break;
                        case AnnotationType.enumLine:
                            x = iImgWidth - tempAnno.Location.X - tempAnno.Size.Width;
                            startPoint = tempAnno.StartPoint;
                            x = iImgWidth - startPoint.X;
                            tempAnno.StartPoint = new System.Drawing.Point(x, startPoint.Y);
                            endPoint = tempAnno.EndPoint;
                            x = iImgWidth - endPoint.X;
                            tempAnno.EndPoint = new System.Drawing.Point(x, endPoint.Y); break;
                    }
                }

                _oImgCore.ImageProcesser.Mirror(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);

                updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            }
        }

        private void btnFlipStrip_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
            {
                int iImageWidth = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer).Width;
                int iImageHeight = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer).Height;
                List<AnnotationData> tempListAnnotn = (List<AnnotationData>)_oImgCore.ImageBuffer.GetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation);

                if (tempListAnnotn != null && tempListAnnotn.Count != 0)
                {
                    foreach (AnnotationData tempAnno in tempListAnnotn)
                    {
                        int y = 0;
                        switch (tempAnno.AnnotationType)
                        {
                            case AnnotationType.enumRectangle:
                            case AnnotationType.enumEllipse:
                            case AnnotationType.enumText:
                                y = iImageHeight - (tempAnno.StartPoint.Y + tempAnno.Size.Height);
                                tempAnno.StartPoint = new System.Drawing.Point(tempAnno.StartPoint.X, y);
                                tempAnno.EndPoint = new System.Drawing.Point((tempAnno.StartPoint.X + tempAnno.Size.Width), (tempAnno.StartPoint.Y + tempAnno.Size.Height));
                                break;
                            case AnnotationType.enumLine:
                                y = iImageHeight - tempAnno.Location.Y - tempAnno.Size.Height;

                                System.Drawing.Point startPoint = tempAnno.StartPoint;
                                y = iImageHeight - startPoint.Y;
                                tempAnno.StartPoint = new System.Drawing.Point(startPoint.X, y);
                                System.Drawing.Point endPoint = tempAnno.EndPoint;
                                y = iImageHeight - endPoint.Y;
                                tempAnno.EndPoint = new System.Drawing.Point(endPoint.X, y);
                                break;
                        }
                    }
                }
                _oImgCore.ImageProcesser.Flip(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);

                updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            }
        }

        private void btnExpaAllStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (tvwBatch.Nodes.Count > 0)
                    tvwBatch.ExpandAll();
            }
            catch (Exception ex)
            {
            }
        }

        private void btnCollAllStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (tvwBatch.Nodes.Count > 0)
                    tvwBatch.CollapseAll();
            }
            catch (Exception ex)
            {
            }
        }

        private void btnDeskewStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                {
                    double dAngle;
                    dAngle = _oImgCore.ImageProcesser.GetSkewAngle(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);

                    _oImgCore.ImageProcesser.Rotate(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, -dAngle, true, Dynamsoft.Core.Enums.EnumInterpolationMethod.BestQuality);

                    updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                }
            }
            catch (Exception ex)
            {
            }
        }

        #region treeview drag and drop events
        private void tvwBatch_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used.  
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
            // Copy the dragged node when the right mouse button is used.  
            else if (e.Button == MouseButtons.Right)
            {
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }

        private void tvwBatch_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void tvwBatch_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.  
            System.Drawing.Point targetPoint = tvwBatch.PointToClient(new System.Drawing.Point(e.X, e.Y));

            TreeNode oNode = tvwBatch.GetNodeAt(targetPoint);

            if (oNode != null)
            {
                if (_batchType.Trim().ToLower() == "set")
                {
                    if (oNode.Level == 1 || oNode.Level == 2)
                    {
                        // Select the node at the mouse position.  
                        tvwBatch.SelectedNode = oNode;
                    }
                }
                else
                {
                    if (oNode.Level == 2 || oNode.Level == 3)
                    {
                        // Select the node at the mouse position.  
                        tvwBatch.SelectedNode = oNode;
                    }
                    else if (oNode.Level != 0)
                    {
                        tvwBatch.SelectedNode = oNode;
                    }
                }
            }
        }

        private void tvwBatch_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.  
            System.Drawing.Point targetPoint = tvwBatch.PointToClient(new System.Drawing.Point(e.X, e.Y));

            // Retrieve the node at the drop location.  
            TreeNode targetNode = tvwBatch.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.  
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Confirm that the node at the drop location is not   
            // the dragged node or a descendant of the dragged node.  
            if (!draggedNode.Equals(targetNode) && !containsNode(draggedNode, targetNode) 
                && (targetNode.Level == 2 || targetNode.Level == 3 || (_noSeparator.Trim() == "0" && targetNode.Level == 1)))
            {
                // If it is a move operation, remove the node from its current   
                // location and add it to the node at the drop location.  
                if (e.Effect == DragDropEffects.Move)
                {
                    sDrag = "move";
                    tvwBatch.SelectedNode = draggedNode;
                    bool bRet = cutImageNode(tvwBatch.SelectedNode, false);

                    if (bRet)
                    {
                        tvwBatch.SelectedNode = targetNode;
                        sFrontBack = "F";
                        mnuPasteStrip_Click(sender, e);
                    }
                    sDrag = string.Empty;
                    //draggedNode.Remove();
                    //if (targetNode.Level == 2) //Doc. Type.
                    //{
                    //    targetNode.Nodes.Add(draggedNode);
                    //}
                    //else
                    //    targetNode.Parent.Nodes.Insert(targetNode.Index, draggedNode);
                }
                // If it is a copy operation, clone the dragged node   
                // and add it to the node at the drop location.  
                else if (e.Effect == DragDropEffects.Copy)
                {
                    tvwBatch.SelectedNode = draggedNode;
                    bool bRet = copyImageNode(tvwBatch.SelectedNode);

                    if (bRet)
                    {
                        tvwBatch.SelectedNode = targetNode;
                        sFrontBack = "F";
                        mnuPasteStrip_Click(sender, e);
                    }
                    //if (targetNode.Level == 2) //Doc. Type.
                    //{
                    //    targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                    //}
                    //else
                    //    targetNode.Parent.Nodes.Insert(targetNode.Index, (TreeNode)draggedNode.Clone());
                }

                //mGlobal.Write2Log(draggedNode.Tag.ToString());
                tvwBatch.Refresh();
                // Expand the node at the location   
                // to show the dropped node.  
                targetNode.Expand();
            }

        }

        private bool containsNode(TreeNode node1, TreeNode node2)
        {
            // Check the parent node of the second node.  
            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            // If the parent node is not null or equal to the first node,   
            // call the ContainsNode method recursively using the parent of   
            // the second node.  
            return containsNode(node1, node2.Parent);
        }

        private bool cutImageNode(TreeNode oSelectedNode, bool bReload = true)
        {
            try
            {
                string msg = "";
                string nodeInfo = "";
                string sep = "";

                if (oSelectedNode != null)
                {
                    TreeNode node = oSelectedNode;
                    selectedNode = node;

                    nodeInfo = node.Text.Trim().Substring(0, 4);
                    sep = node.Tag.ToString().Trim().Substring(0, 4);

                    if (nodeInfo.Trim().ToLower() == "page")
                    {
                        if (sDrag.ToLower() != "move")
                        {
                            if (MessageBox.Show(this, "Cut an image cannot undo. Confirm to cut the selected image?", "Confirmation", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                                return false;
                        }
                    }
                    else
                        return false;

                    bImgCut = true;

                    if (sep.Trim().ToLower() == "sep1")
                    {
                        return false;
                    }
                    else if (sep.Trim().ToLower() == "sep2")
                    {
                        return false;
                    }
                    else if (nodeInfo.Trim().ToLower() == "page")
                    {
                        if (node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            string rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                            string setNum = node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();
                            string docType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                            string collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                            string srcFilename = oDF.getDocFilenameDB(rowid, "TDocuScan");
                            if (srcFilename.Trim() == string.Empty)
                                srcFilename = oDF.getDocFilenameDB(rowid, "TDocuIndex");

                            sImgInfoCopy = rowid + "|" + srcFilename + "|" + docType + "|" + collaNum;

                            if (File.Exists(srcFilename))
                            {
                                oImgCut = new ImageCore();
                                oImgCut.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                                oImgCut.IO.LoadImage(srcFilename);

                                lScanImg.RemoveAt(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                                //_oImgCore.IO.CopyToClipborad(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                                _oImgCore.ImageBuffer.RemoveImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);                                
                            }
                            else
                            {
                                msg = "Cut image failed for image source does not exists!";
                                MessageBox.Show(this, msg, "Message");
                                return false;
                            }

                            msg = cutImageFile();

                            if (msg.Trim() == "")
                            {
                                if (node != null)
                                {
                                    iImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());
                                    if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                    {
                                        if (_fromProc.Trim().ToLower() == "verify")
                                        {
                                            oDF.reorderIndexImageSeq(_currScanProj, _appNum, setNum, collaNum, _batchCode, iImgSeq, docType);
                                        }
                                        else
                                        {
                                            oDF.reorderScanImageSeq(_currScanProj, _appNum, setNum, collaNum, _batchCode, iImgSeq, docType);
                                        }
                                    }
                                    else
                                    {
                                        if (_fromProc.Trim().ToLower() == "verify")
                                        {
                                            oDF.reorderIndexImageSeq(_currScanProj, _appNum, setNum, collaNum, _batchCode, iImgSeq);
                                        }
                                        else
                                            oDF.reorderScanImageSeq(_currScanProj, _appNum, setNum, collaNum, _batchCode, iImgSeq);
                                    }
                                }

                                bScanStart = true;
                            }

                            if (msg.Trim() == "" && bReload)
                            {
                                //reloadForEdit();
                                reloadForCurrRow(_iCurrBatchRowid, _batchCode);

                                TreeNode[] oNodes = tvwBatch.Nodes.Find(selectedNode.Name, true);
                                if (oNodes.Length > 0)
                                {
                                    tvwBatch.SelectedNode = oNodes[0];
                                    tvwBatch.SelectedNode.EnsureVisible();
                                }
                            }
                            else
                                mGlobal.Write2Log("Cut image.." + msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private bool copyImageNode(TreeNode oSelectedNode)
        {
            try
            {
                string nodeInfo = "";
                string sep = "";

                if (oSelectedNode != null)
                {
                    TreeNode node = oSelectedNode;
                    bImgCut = false;

                    nodeInfo = node.Text.Trim().Substring(0, 4);
                    sep = node.Tag.ToString().Trim().Substring(0, 4);

                    if (sep.Trim().ToLower() == "sep1")
                    {
                        return false;
                    }
                    else if (sep.Trim().ToLower() == "sep2")
                    {
                        return false;
                    }
                    else if (nodeInfo.Trim().ToLower() == "page")
                    {
                        if (node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            string rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                            string docType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                            string collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                            string file = oDF.getDocFilenameDB(rowid, "TDocuScan");
                            if (file.Trim() == string.Empty)
                                file = oDF.getDocFilenameDB(rowid, "TDocuIndex");

                            sImgInfoCopy = rowid + "|" + file + "|" + docType + "|" + collaNum;
                        }

                    }
                }
                else return false;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
            return true;
        }
        #endregion

        private string validateScan()
        {
            string sMsg = "";
            try
            {
                if (_noSeparator.Trim() == "2")
                {
                    if (tvwBatch.Nodes.Count == 0 && _docType == "")
                    {
                        sMsg = "First set scanning is not allowed without document set separator and document type separator.";
                    }
                }
                else if (_noSeparator.Trim() == "1")
                {
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        if (tvwBatch.Nodes.Count == 0 && (_docType == "" || _docType == sSetNumFmt))
                        {
                            sMsg = "First set scanning is not allowed without document set separator.";
                        }
                    }
                    else
                    {
                        if (tvwBatch.Nodes.Count == 0 && _docType == "")
                        {
                            sMsg = "First set scanning is not allowed without document set separator.";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                sMsg = ex.Message;
            }
            return sMsg;
        }

        private Dictionary<string, string> ocrImageFile(string sFilename, bool pOCRBarcode = false)
        {
            Dictionary<string, string> dResults = new Dictionary<string, string>();
            //Bitmap bmp = null;
            bool bBarcode = false;
            try
            {
                if (oDF.stcOCRAreaList == null) return null;

                if (oDF.stcOCRAreaList.Count == 0) return null;

                var Ocr = new IronTesseract();
                Ocr.Language = OcrLanguage.EnglishBest;
                //Ocr.AddSecondaryLanguage(OcrLanguage.Thai);
                //Ocr.UseCustomTesseractLanguageFile(@"C:\Development\Solution1\WindowsFormsApp1\bin\x86\Debug\runtimes\win-x86\native\tessdata\tessdata_best\eng.best.traineddata");
                Ocr.Configuration = new TesseractConfiguration()
                {
                    //EngineMode = TesseractEngineMode.TesseractOnly,
                    ReadBarCodes = pOCRBarcode,
                    //RenderSearchablePdfsAndHocr = false,
                    RenderHocr = false,
                    RenderSearchablePdf = false,
                    PageSegmentationMode = TesseractPageSegmentationMode.SparseText
                };

                string sIsBarcode = string.Empty;

                int i = 0; int iIdx = 0;
                //var ContentArea = new IronSoftware.Drawing.CropRectangle(x: 0, y: 0, width: 0, height: 0);
                var ContentArea = new IronSoftware.Drawing.Rectangle(x: 0, y: 0, width: 0, height: 0);

                while (i < oDF.stcOCRAreaList.Count)
                {
                    iIdx = staMain.stcIndexSetting.FieldName.FindIndex(a => a.Contains(oDF.stcOCRAreaList[i].name));
                    if (iIdx != -1)
                    {
                        if (staMain.stcIndexSetting.FieldDataType[iIdx].ToString().ToLower() == "barcode")
                        {
                            bBarcode = true;
                        }
                    }

                    if (bBarcode)
                    {
                        using (var ocrInput = new OcrInput())
                        {
                            ocrInput.DeNoise();
                            ocrInput.Deskew();
                            ocrInput.Sharpen();
                            ocrInput.TargetDPI = 300;

                            ContentArea = new IronSoftware.Drawing.Rectangle(x: oDF.stcOCRAreaList[i].left, y: oDF.stcOCRAreaList[i].top, width: oDF.stcOCRAreaList[i].width, height: oDF.stcOCRAreaList[i].height);

                            //ocrInput.AddImage(sFilename, ContentArea);
                            ocrInput.LoadImage(sFilename, ContentArea);
                            var barcResult = Ocr.Read(ocrInput);

                            if (barcResult.Barcodes.Length > 0)
                            {
                                sIsBarcode = barcResult.Barcodes[0].Value;
                            }
                        }
                    }

                    if (sIsBarcode != string.Empty) //Barcode result.
                    {
                        dResults.Add(oDF.stcOCRAreaList[i].name, sIsBarcode);
                    }
                    else
                    {
                        using (var Input = new OcrInput())
                        {
                            Input.DeNoise();
                            Input.Deskew();
                            Input.Sharpen();
                            Input.TargetDPI = 300;

                            //bmp = (Bitmap)Bitmap.FromFile(sFilename);

                            ContentArea = new IronSoftware.Drawing.Rectangle(x: oDF.stcOCRAreaList[i].left, y: oDF.stcOCRAreaList[i].top, width: oDF.stcOCRAreaList[i].width, height: oDF.stcOCRAreaList[i].height);
                            //Input.AddImage(bmp, ContentArea);
                            //Input.AddImage(sFilename, ContentArea);
                            Input.LoadImage(sFilename, ContentArea);

                            dResults.Add(oDF.stcOCRAreaList[i].name, Ocr.Read(Input).Text);
                        }
                    }

                    bBarcode = false;
                    sIsBarcode = "";
                    i += 1;
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
            finally
            {
                //if (bmp != null) bmp.Dispose();
            }
            return dResults;
        }

        private bool saveDocIndexFieldDB(string pSetNum, string pFieldName, string pResult)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            try
            {
                int idx = staMain.stcIndexSetting.FieldName.FindIndex(a => a.Contains(pFieldName));

                if (idx < 0)
                    return false;

                bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, pSetNum, _docDefId, idx.ToString());

                mGlobal.LoadAppDBCfg();

                if (bExist)
                {
                    bSaved = true;
                    //int iId = 0;
                    //iId = staMain.stcIndexSetting.RowId[idx];

                    //sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                    //sSQL += "SET [fldvalue]=N'" + pResult.Trim().Replace("'", "''") + "'";
                    //sSQL += ",[modifieddate]='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                    //sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND fieldid=" + iId + " ";

                    //if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    //{
                    //    bSaved = false;
                    //    throw new Exception("Update document index data failed!");
                    //}
                }
                else
                {
                    int iId = 0;
                    iId = staMain.stcIndexSetting.RowId[idx];

                    sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                    sSQL += "([scanpjcode]";
                    sSQL += ",[sysappnum]";
                    sSQL += ",[docdefid]";
                    sSQL += ",[setnum]";
                    sSQL += ",[batchcode]";
                    sSQL += ",[fieldid]";
                    sSQL += ",[fldvalue]";
                    sSQL += ") VALUES (";
                    sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                    sSQL += "," + iId + "";
                    sSQL += ",N'" + pResult.Trim().Replace("'", "''") + "')";

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                        throw new Exception("Saving document index data failed!");
                    }
                }

            }
            catch (Exception ex)
            {
                bSaved = false;
                throw new Exception(ex.Message);
            }

            return bSaved;
        }

        private void OCRAndSave(string pSetNum, int pImgSeq)
        {
            try
            {
                var oResults = ocrImageFile(_imgFilename, true);
                //Thread ocrThread = new Thread(new ThreadStart(() => ocrImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer)));
                //ocrThread.IsBackground = true;
                //ocrThread.Start();

                if (oResults != null)
                {
                    string sIndexNum = pSetNum;
                    int i = 0;
                    while (i < oResults.Count)
                    {
                        if (_noSeparator.Trim() == "0") //Set number = Page number selected in TIndexFieldValue table.
                            sIndexNum = pImgSeq.ToString(sSetNumFmt);

                        saveDocIndexFieldDB(sIndexNum, oResults.ElementAt(i).Key, oResults.ElementAt(i).Value);

                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Box OCR.." + ex.Message);
            }
        }

        private Dictionary<string, string> ReadAutoBarcode(string pFilename, bool pMultiple)
        {
            Dictionary<string, string> dResults = new Dictionary<string, string>();
            try
            {
                //MessageBox.Show(_oImgCore.ImageBuffer.HowManyImagesInBuffer.ToString());
                //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(pFilename);         

                var oOptions = new BarcodeReaderOptions
                {
                    // Choose a speed from: Faster, Balanced, Detailed, ExtremeDetail
                    // There is a tradeoff in performance as more Detail is set
                    Speed = ReadingSpeed.Balanced,

                    // Reader will stop scanning once a barcode is found, unless set to true
                    ExpectMultipleBarcodes = pMultiple,

                    // By default, all barcode formats are scanned for.
                    // Specifying one or more, performance will increase.
                    ExpectBarcodeTypes = BarcodeEncoding.AllOneDimensional, //BarcodeEncoding.AllOneDimensional, //BarcodeEncoding.Code128,

                    // Utilizes multiple threads to reads barcodes from multiple images in parallel.
                    Multithreaded = true,

                    // Maximum threads for parallel. Default is 4
                    MaxParallelThreads = 4,

                    // The area of each image frame in which to scan for barcodes.
                    // Will improve performance significantly and avoid unwanted results and avoid noisy parts of the image.
                    CropArea = new System.Drawing.Rectangle(),

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

                if (textResults == null) return null;
                if (textResults.Result == null) return null;
                if (textResults.Result.Values().Length == 0) return null;

                BarcodeResult[] results = textResults.Result.ToArray();

                int i = 0; int iIdx = 0;
                while (i < lAutoBarcode.Count)
                {
                    iIdx = staMain.stcIndexSetting.FieldName.FindIndex(a => a.Contains(lAutoBarcode[i]));
                    if (iIdx != -1)
                    {
                        if (i < results.Length)
                            dResults.Add(lAutoBarcode[i], results[i].Text);
                        else
                            break;
                    }

                    i += 1;
                }

                //textResults = null;
                results = null;
                textResults.Dispose();

                //this.ShowResultOnImage(bmp, textResults);
                //bmp.Dispose();

                return dResults;
            }
            catch (Exception exp)
            {
                mGlobal.Write2Log("Decode auto barcode.." + exp.Message);
                mGlobal.Write2Log(mGlobal.getFileNameFromPath(pFilename));
                mGlobal.Write2Log(exp.StackTrace.ToString());
                //MessageBox.Show(exp.Message, "Decoding error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dResults;
        }

        private void AutoReadBarcodeAndSave(string pSetNum, int pImgSeq, string pFilename)
        {
            try
            {
                bool bMultiple = bMultipleBarcodeOCR; //false;

                var oResults = ReadAutoBarcode(pFilename, bMultiple);
                //Thread ocrThread = new Thread(new ThreadStart(() => ocrImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer)));
                //ocrThread.IsBackground = true;
                //ocrThread.Start();

                if (oResults != null)
                {
                    string sIndexNum = pSetNum;
                    int i = 0;
                    while (i < oResults.Count)
                    {
                        if (_noSeparator.Trim() == "0") //Set number = Page number selected in TIndexFieldValue table.
                            sIndexNum = pImgSeq.ToString(sSetNumFmt);

                        //mGlobal.Write2Log(i.ToString() + ":" + oResults.ElementAt(i).Value);
                        bool bOK = saveDocIndexFieldDB(sIndexNum, oResults.ElementAt(i).Key, oResults.ElementAt(i).Value);

                        if (bOK) iAutoBarCnt += 1;

                        if (bMultiple == false && i == 0) break;

                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Auto barcode.." + ex.Message);
            }
        }

        private void mnuInsStrip_Click(object sender, EventArgs e)
        {
            TreeNode node = null;
            try
            {
                string nodeInfo = "";
                string sep = "";
                //string sDir = "";
                string sFileName = "";
                string sFileExt = "tiff";

                if (tvwBatch.SelectedNode != null)
                {
                    node = tvwBatch.SelectedNode;
                    selectedNode = node;

                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;

                    this.AcquireImageReplace();

                    if (oImgCut != null)
                    {
                        bImgCut = true; //for insert.

                        nodeInfo = node.Text.Trim().Substring(0, 4);
                        sep = node.Tag.ToString().Trim().Substring(0, 4);

                        string rowid = "";
                        sImgInfoCopy = "";

                        //string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        //if (_batchType.Trim().ToLower() == "set")
                        //{
                        //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //    //if (_docType.Trim() == "")
                        //    //{
                        //    //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //    //}
                        //    //else
                        //    //    sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        //}
                        //else //Batch.
                        //{
                        //    if (_docType.Trim() == "")
                        //    {
                        //        sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //    }
                        //    else
                        //        sDir = mGlobal.addDirSep(staMain.stcProjCfg.WorkDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        //}

                        if (staMain.stcProjCfg.BatchAuto.ToUpper() == "Y")
                            sFileName = _batchCode.Trim() + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;
                        else
                            sFileName = mGlobal.replaceValidChars(_batchCode.Trim(), "_") + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;

                        if (sep.Trim().ToLower() == "sep1")
                        {
                            if (_batchType.Trim().ToLower() == "set" && node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = "0";
                                sImgInfoCopy = rowid + "|" + sFileName;
                            }
                        }
                        else if (sep.Trim().ToLower() == "sep2")
                        {
                            if ((_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box") && node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = "0";
                                sImgInfoCopy = rowid + "|" + sFileName;
                            }
                        }
                        else if (nodeInfo.Trim().ToLower() == "page")
                        {
                            if (node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                sImgInfoCopy = rowid + "|" + sFileName;
                            }
                        }

                        if (sImgInfoCopy.Trim() != "")
                        {
                            mnuPasteStrip_Click(this, e);
                            selectedNode = node;
                        }
                        else
                            MessageBox.Show(this, "Please select either document type or page for insert an image.", "Message");
                    }

                }
                else
                    MessageBox.Show(this, "Please select a page or document type for scan and insert an image.", "Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Insert Image.." + ex.Message);
            }
            finally
            {
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                if (selectedNode != null)
                {
                    TreeNode[] oNodes = tvwBatch.Nodes.Find(selectedNode.Name, true);
                    if (oNodes.Length > 0)
                    {
                        tvwBatch.SelectedNode = oNodes[0];
                        tvwBatch.SelectedNode.EnsureVisible();
                    }
                }
                //tvwBatch.Refresh();
            }
        }

        private void ShowResultOnImage(Bitmap bitmap, System.Drawing.Rectangle[] textResults)
        {
            try
            {
                _oImgCore.ImageBuffer.SetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation, null, true);
                if (textResults != null)
                {
                    List<AnnotationData> tempListAnnotation = new List<AnnotationData>();
                    int nTextResultIndex = 0;
                    for (var i = 0; i < textResults.Length; i++)
                    {
                        var penColor = System.Drawing.Color.Red;
                        System.Drawing.Rectangle result = textResults[i];

                        var rectAnnotation = new AnnotationData();
                        rectAnnotation.AnnotationType = AnnotationType.enumRectangle;
                        System.Drawing.Rectangle boundingrect = textResults[i];
                        rectAnnotation.StartPoint = new System.Drawing.Point(boundingrect.Left, boundingrect.Top);
                        rectAnnotation.EndPoint = new System.Drawing.Point((boundingrect.Left + boundingrect.Size.Width), (boundingrect.Top + boundingrect.Size.Height));
                        rectAnnotation.FillColor = System.Drawing.Color.Transparent.ToArgb();
                        rectAnnotation.PenColor = penColor.ToArgb();
                        rectAnnotation.PenWidth = 3;
                        rectAnnotation.GUID = Guid.NewGuid();

                        float fsize = bitmap.Width / 48.0f;
                        if (fsize < 25)
                            fsize = 25;

                        System.Drawing.Font textFont = new System.Drawing.Font("Times New Roman", fsize, System.Drawing.FontStyle.Bold);

                        string strNo = (result != null) ? "[" + (nTextResultIndex++ + 1) + "]" : "";
                        System.Drawing.SizeF textSize = Graphics.FromHwnd(IntPtr.Zero).MeasureString(strNo, textFont);

                        var textAnnotation = new AnnotationData();
                        textAnnotation.AnnotationType = AnnotationType.enumText;
                        textAnnotation.StartPoint = new System.Drawing.Point(boundingrect.Left, (int)(boundingrect.Top - textSize.Height * 1.25f));
                        textAnnotation.EndPoint = new System.Drawing.Point((textAnnotation.StartPoint.X + (int)textSize.Width * 2), (int)(textAnnotation.StartPoint.Y + textSize.Height * 1.25f));
                        if (textAnnotation.StartPoint.X < 0)
                        {
                            textAnnotation.EndPoint = new System.Drawing.Point((textAnnotation.EndPoint.X + textAnnotation.StartPoint.X), textAnnotation.EndPoint.Y);
                            textAnnotation.StartPoint = new System.Drawing.Point(0, textAnnotation.StartPoint.Y);
                        }
                        if (textAnnotation.StartPoint.Y < 0)
                        {
                            textAnnotation.EndPoint = new System.Drawing.Point(textAnnotation.EndPoint.X, (textAnnotation.EndPoint.Y - textAnnotation.StartPoint.Y));
                            textAnnotation.StartPoint = new System.Drawing.Point(textAnnotation.StartPoint.X, 0);
                        }

                        textAnnotation.TextContent = strNo;
                        AnnoTextFont tempFont = new AnnoTextFont();
                        tempFont.TextColor = System.Drawing.Color.Red.ToArgb();
                        tempFont.Size = (int)fsize;
                        tempFont.Name = "Times New Roman";
                        textAnnotation.FontType = tempFont;
                        textAnnotation.GUID = Guid.NewGuid();

                        tempListAnnotation.Add(rectAnnotation);
                        tempListAnnotation.Add(textAnnotation);
                    }
                    _oImgCore.ImageBuffer.SetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation, tempListAnnotation, true);
                }
            }
            catch (ImageCoreException cex)
            {
                throw new Exception(cex.Message);
            }
        }

        private void btnLoadImgStrip_Click(object sender, EventArgs e)
        {
            try
            {
                bool bValid = true;

                ofdLoadImg.Filter = "(*.tif;*.tiff)| *.tif; *.tiff";
                ofdLoadImg.InitialDirectory = @"C:\";

                if (ofdLoadImg.ShowDialog(this) == DialogResult.OK)
                {
                    if (BoxNoStripTxt.Text.Trim() == "")
                    {
                        MessageBox.Show("Box number is required! Please fill in box number.");
                        bValid = false;
                        return;
                    }
                    else
                    {
                        if (!mGlobal.IsInteger(BoxNoStripTxt.Text.Trim()))
                        {
                            MessageBox.Show("Box number must be number! Please fill in box number with number only.");
                            bValid = false;
                            return;
                        }
                    }
                    //if (staMain.stcProjCfg.BatchAuto.Trim().ToUpper() == "N" && _batchCode.Trim() == "")
                    //{
                    //    MessageBox.Show("Batch code is required! Please fill in batch code.");
                    //    bValid = false;
                    //    return;
                    //}

                    btnLoadImgStrip.Enabled = false;
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;
                    //btnScan.Enabled = false;
                    enableScan(false);

                    this._currScanProj = MDIMain.strScanProject;

                    if (_batchType.Trim().ToLower() == "set")
                    {
                        this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum);
                        sCurrDocType = iSetNum.ToString(sSetNumFmt);
                    }
                    else //batch.
                    {
                        _batchNum = oDF.getBatchCodeNum(_currScanProj, _appNum, _batchCode);

                        sCurrDocType = oDF.getLastDocTypeDB(_currScanProj, _appNum, _setNum, _batchCode);

                        this.iSetNum = oDF.getLastSetNum(_currScanProj, _appNum, _batchCode);
                    }

                    this._setNum = this.iSetNum.ToString(sSetNumFmt);

                    if (oDF.checkDocuScanExist(_currScanProj, _appNum, _batchCode, _setNum, sCurrDocType))
                    {
                        this.iCollaNum = oDF.getCurrCollaNum(_currScanProj, _appNum, _batchCode, _setNum, sCurrDocType);
                        _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                    }
                    else
                    {
                        if (iSetNum == 0 && _docType == staMain.stcProjCfg.SeparatorText) //First time load image from files.
                            iCollaNum = 0;
                        else
                        {
                            this.iCollaNum = oDF.getLastCollaNum(_currScanProj, _appNum, _batchCode, _setNum);
                            this.iCollaNum += 1;
                        }

                        _collaNum = this.iCollaNum.ToString(sCollaNumFmt);
                    }

                    this._scanStatus = "R";

                    this.iImgSeq = 0;

                    if (this.sIsAddOn.Trim() != "")
                    {
                        tvwBatch.Nodes.Clear();
                        setInfoForAddOn();
                    }

                    //Init.>>
                    this._isDocSep = true;
                    this._sepCnt = 0;
                    this._docType = "";
                    this._batchStatus = "1";
                    //>>

                    this.ScanToolStrip.Items["btnCloseStrip"].Enabled = false;

                    _scanStart = DateTime.Now;
                    bScanStart = true;

                    dsvImg.IfFitWindow = true;
                    //dsvImg.Zoom = 1;

                    List<string> sFiles = new List<string>();
                    sFiles = ofdLoadImg.FileNames.ToList();

                    if (sFiles.Count > 0)
                    {
                        int h = 0; FileInfo[] createddate = new FileInfo[sFiles.Count];
                        while (h < sFiles.Count)
                        {
                            createddate[h] = new FileInfo(sFiles[h].ToString());

                            h += 1;
                        }

                        clsDateCompareFileInfo oComparer = new clsDateCompareFileInfo();
                        Array.Sort(createddate, oComparer);
                        //Array.Sort(modifiedtime, sFiles.ToArray(), oComparer);
                        h = 0; sFiles.Clear();
                        while (h < createddate.Length)
                        {
                            sFiles.Add(createddate[h].FullName);

                            h += 1;
                        }

                        ImageCore oImg = new ImageCore();
                        oImg.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                        //PreAllTransfer Init.>>
                        if (_fromProc.Trim().ToLower() == "verify")
                        {
                            if (_batchType.Trim().ToLower() == "set")
                                _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, _setNum);
                            else
                                _totPageCnt = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                        }
                        else
                        {
                            if (_batchType.Trim().ToLower() == "set")
                                _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, _setNum);
                            else
                                _totPageCnt = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                        }
                        _isDocSep = true;
                        //>>

                        int i = 0; Bitmap oImgMap; bool bOk = true;
                        while (i < sFiles.Count)
                        {
                            oImg.IO.LoadImage(sFiles[i].ToString());
                            oImgMap = oImg.ImageBuffer.GetBitmap((short)i);

                            ScanStatusBar.Text = "Loading..";
                            _scanDStart = DateTime.Now;

                            clsImgInfo oImgInfo = new clsImgInfo();
                            ImageInfoJson.Others oOthers = new ImageInfoJson.Others();
                            oOthers.TweiPageside = 1;
                            ImageInfoJson.ExtendedImageInfo oExt = new ImageInfoJson.ExtendedImageInfo();
                            oExt.Others = oOthers;
                            oImgInfo.ExtendedImageInfo = oExt;
                            ImageInfoJson.ImageInfo oInfo = new ImageInfoJson.ImageInfo();
                            oImgInfo.ImageInfo = oInfo;
                            bIsDuplexEnabled = false;

                            //bOk = OnPostTransfer(oImgMap, oImg.ImageBuffer.GetMetaData((short)i, EnumMetaDataType.enumAnnotation).ToString());
                            bOk = OnPostTransfer(oImgMap, oImgInfo.ToJson());
                            if (bOk == false || sErrMsg.Trim() != string.Empty)
                                break;

                            i += 1;
                        }

                        if (i > 0 && bOk && sErrMsg.Trim() == string.Empty)
                        {
                            OnPostAllTransfers();
                            reloadForCurrRow(_iCurrBatchRowid, _batchCode);
                        }

                        oImg.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ScanStatusBar.Text = "Ready";
                btnLoadImgStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;
                //btnScan.Enabled = true;
                enableScan(true);

                this.ScanToolStrip.Items["btnCloseStrip"].Enabled = true;

                //Enable Scanner Source menu item.
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], true, "mnuScanSettingStrip");
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], true, "mnuScannerStrip");

                if (tvwBatch.Nodes.Count > 0)
                {
                    btnDeleteStrip.Enabled = true;
                    btnDeleteAllStrip.Enabled = true;
                    //tvwBatch.ExpandAll();
                }
                else
                {
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;
                }

                tvwBatch.Focus();
            }
        }

        private void setInfoForAddOn(bool initImgCore = true) //always go to last set and last doctype for addon.
        {
            try
            {

                if (_stcImgInfo.ImgIdx.Length > 0 || _stcImgInfo.BatchCode.Trim() != "")
                {
                    _batchCode = _stcImgInfo.BatchCode;
                    _batchNum = oDF.getBatchCodeNum(_currScanProj, _appNum, _batchCode);

                    if (_batchType.Trim().ToLower() == "set")
                        _setNum = oDF.getLastSetNum(_currScanProj, _appNum, _batchCode).ToString(sSetNumFmt);
                    else
                        _setNum = oDF.getLastSetNum(_currScanProj, _appNum, _batchCode).ToString(sSetNumFmt);

                    iSetNum = Convert.ToInt32(_setNum); //oDF.getLastSetNum(_currScanProj, _appNum, _batchNum);                    

                    sCurrDocType = oDF.getLastDocTypeDB(_currScanProj, _appNum, _setNum, _batchCode);

                    iCollaNum = oDF.getCurrCollaNum(_currScanProj, _appNum, _batchCode, _setNum, sCurrDocType);
                    _collaNum = iCollaNum.ToString(sCollaNumFmt);

                    if (_fromProc.Trim().ToLower() == "verify" || _fromProc.Trim().ToLower() == "index")
                    {
                        _iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, _setNum, "'4'");
                    }
                    else
                        _iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, _setNum, "'1'");

                    this._scanStatus = "A";

                    this.iImgSeq = oDF.getLastImageSeqDB(_currScanProj, _appNum, _setNum, _collaNum, _batchCode);

                    if (initImgCore)
                        initImageAllInfo();

                    loadRejectedImagesFrmDisk(_iCurrBatchRowid);
                    //if load image is false then clear all images in image core buffer to prevent invalid index error during addon scanning.
                    //_oImgCore.ImageBuffer.RemoveAllImages();
                    //dsvImg.Bind(_oImgCore); //Rebind.

                    CheckImageCount();

                    //tvwBatch.Nodes.Clear();
                    if (sIsAddOn.Trim().ToLower() == "rejected")
                    {
                        ScanStatusBar1.Text = "Reverted Addon";
                        //loadRejectedBatchSetTreeview(DateTime.Now, DateTime.Now);
                        loadRejectedBatchSetTreeview();
                    }
                    else //loadBatchSetRescanTreeview();
                        loadBatchSetRescanTreeview(_fromProc);

                    if (tvwBatch.Nodes.Count > 0)
                        tvwBatch.ExpandAll();
                }
                else
                {
                    MessageBox.Show("Current Document set info is not available!", "Message");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void txtBrighttoolStrip_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (Convert.ToInt32(txtBrighttoolStrip.Text) < 1)
                    {
                        txtBrighttoolStrip.Text = "1";
                    }
                    else if (Convert.ToInt32(txtBrighttoolStrip.Text) > 100)
                    {
                        txtBrighttoolStrip.Text = "100";
                    }

                    if (Convert.ToInt32(txtBrighttoolStrip.Text) > 0 && Convert.ToInt32(txtBrighttoolStrip.Text) < 101)
                    {
                        fltBrightness = Convert.ToInt32(txtBrighttoolStrip.Text) * 1.00f;
                        fltContrast = Convert.ToInt32(txtContrtoolStrip.Text);

                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 && oImgBytes != null)
                        {
                            short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                            //byte[] oBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, Dynamsoft.Core.Enums.EnumImageFileFormat.WEBTW_TIF);

                            System.Drawing.Image oImg = null;
                            MemoryStream memStream = new MemoryStream(oImgBytes);
                            MemoryStream outStream = new MemoryStream();

                            using (var image = SixLabors.ImageSharp.Image.Load(memStream))
                            {
                                image.Mutate(c => c.Brightness(fltBrightness));
                                //image.Mutate(c => c.Contrast(fltContrast));
                                //image.SaveAsTiff(@"C:\temp\temp.tiff");
                                image.SaveAsTiff(outStream);
                                outStream.Position = 0;
                                oImg = System.Drawing.Image.FromStream(outStream);
                            }
                            memStream.Close();
                            memStream.Dispose();

                            _oImgCore.ImageBuffer.RemoveImage(iCurrImgIdx);
                            //_oImgCore.IO.LoadImage(@"C:\temp\temp.tiff", Dynamsoft.Core.Enums.EnumImageFileFormat.WEBTW_TIF);
                            _oImgCore.IO.LoadImage(oImg);
                            _oImgCore.ImageBuffer.MoveImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);

                            oImg.Dispose();
                            outStream.Close();
                            outStream.Dispose();

                            updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }

        private void txtContrtoolStrip_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (Convert.ToInt32(txtContrtoolStrip.Text) < 1)
                    {
                        txtContrtoolStrip.Text = "1";
                    }
                    else if (Convert.ToInt32(txtContrtoolStrip.Text) > 255)
                    {
                        txtContrtoolStrip.Text = "255";
                    }

                    if (Convert.ToInt32(txtContrtoolStrip.Text) > 0 && Convert.ToInt32(txtContrtoolStrip.Text) < 256)
                    {
                        fltBrightness = Convert.ToInt32(txtBrighttoolStrip.Text) * 1.00f;
                        fltContrast = Convert.ToInt32(txtContrtoolStrip.Text);

                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 && oImgBytes != null)
                        {
                            short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                            //byte[] oBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, Dynamsoft.Core.Enums.EnumImageFileFormat.WEBTW_TIF);

                            System.Drawing.Image oImg = null;
                            MemoryStream memStream = new MemoryStream(oImgBytes);
                            MemoryStream outStream = new MemoryStream();

                            using (var image = SixLabors.ImageSharp.Image.Load(memStream))
                            {
                                //image.Mutate(c => c.Brightness(fltBrightness));
                                image.Mutate(c => c.Contrast(fltContrast));
                                //image.SaveAsTiff(@"C:\temp\temp.tiff");
                                image.SaveAsTiff(outStream);
                                outStream.Position = 0;
                                oImg = System.Drawing.Image.FromStream(outStream);
                            }
                            memStream.Close();
                            memStream.Dispose();

                            _oImgCore.ImageBuffer.RemoveImage(iCurrImgIdx);
                            //_oImgCore.IO.LoadImage(@"C:\temp\temp.tiff", Dynamsoft.Core.Enums.EnumImageFileFormat.WEBTW_TIF);
                            _oImgCore.IO.LoadImage(oImg);
                            _oImgCore.ImageBuffer.MoveImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);

                            oImg.Dispose();
                            outStream.Close();
                            outStream.Dispose();

                            updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }

        private string RemoveDuplexHeaderBackPage()
        {
            short iHIdx = 0;
            try
            {
                if (bIsDuplexEnabled)
                {                    
                    //int i = 0;
                    //while (i < lHeaderIdx.Count)
                    //{
                    //    iHIdx = lHeaderIdx[i];
                    //    mGlobal.Write2Log("Removing backpage.." + iHIdx);
                    //    //_oImgCore.ImageBuffer.RemoveImage((short)iHIdx);
                    //    i += 1;
                    //}
                    if (lHeaderIdx.Count > 0)
                    {
                        var task = Task.Delay(300);
                        task.Wait();

                        _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = lHeaderIdx[0];
                        _oImgCore.ImageBuffer.RemoveImages(lHeaderIdx);
                        lHeaderIdx.Clear();
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(iHIdx + " Removing backpage.." + ex.Message);
                mGlobal.Write2Log(".." + ex.StackTrace.ToString());
                return ex.Message;
            }
        }

        private bool CheckBlankPage(short pImgIdx)
        {
            try
            {
                if (sRmvBlank > 0) //Remove blank page option enabled.
                {
                    _oImgCore.ImageProcesser.BlankImageMaxStdDev = sRmvBlank;
                    if (_oImgCore.ImageProcesser.IsBlankImage(pImgIdx))
                    {
                        return true;
                    }
                    GC.Collect();
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool RemoveBlankPage(short pImgIdx)
        {
            try
            {
                if (sRmvBlank > 0) //Remove blank page option enabled.
                {
                    _oImgCore.ImageProcesser.BlankImageMaxStdDev = sRmvBlank;
                    if (_oImgCore.ImageProcesser.IsBlankImage(pImgIdx))
                    {
                        _oImgCore.ImageBuffer.RemoveImage(pImgIdx);
                        return true;
                    }
                    GC.Collect();
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void reloadForEdit()
        {
            //_oImgCore = new ImageCore();
            _stcImgInfo = new staMain._stcImgInfo();
            
            if (this.sIsAddOn.Trim() != "")
            {
                setInfoForAddOn();
            }
            else
            {
                loadRejectedImagesFrmDisk(_iCurrBatchRowid, false);
                loadBatchSetRescanTreeview(_fromProc);
                if (tvwBatch.Nodes.Count > 0)
                    tvwBatch.ExpandAll();

                //setImgInfo(0);
            }

            CheckImageCount();
        }

        private void mnuInsSepStrip_Click(object sender, EventArgs e)
        {
            TreeNode node = null;
            try
            {
                if (_batchType.Trim().ToLower() == "set")
                {
                    MessageBox.Show(this, "Batch Set Separator is not supported!", "Message");
                    return;
                }

                string nodeInfo = "";
                string sSrcFileName = "";
                string sNewFileName = "";

                if (tvwBatch.SelectedNode != null && _noSeparator.Trim() != "0") //Only allow 1 or 2 separator for insert doctype.
                {
                    node = tvwBatch.SelectedNode;
                    selectedNode = node;

                    bool bOK = this.AcquireImageReplace(true);

                    if (_noSeparator.Trim() == "1" && _docType.Trim().ToUpper() != staMain.stcProjCfg.SeparatorText)
                    {
                        MessageBox.Show(this, "Batch Set Separator is expected only!", "Message");
                        return;
                    }
                    else if (_noSeparator.Trim() == "2" && _docType.Trim().ToUpper() == staMain.stcProjCfg.SeparatorText)
                    {
                        MessageBox.Show(this, "Batch Set Separator is not allowed. Document Separator is expected only!", "Message");
                        return;
                    }
                    else if (_noSeparator.Trim() == "2" && _docType.Trim().ToUpper() == "")
                    {
                        MessageBox.Show(this, "Document Separator is expected only!", "Message");
                        return;
                    }

                    //btnScan.Enabled = false;
                    enableScan(false);
                    //btnSend.Enabled = false;
                    SendStripBtn.Enabled = false;
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;

                    if (oImgCut != null && bOK)
                    {
                        bImgCut = true; //for insert.

                        nodeInfo = node.Text.Trim().Substring(0, 4);
                        //sep = node.Tag.ToString().Trim().Substring(0, 4);                        

                        string rowid = "";
                        sImgInfoCopy = "";

                        sCurrDocType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        sCurrSetNum = node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();
                        _setNum = Convert.ToInt32(sCurrSetNum).ToString(sSetNumFmt);
                        _collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                        iImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        string sDir = "";
                        string sRoot = staMain.stcProjCfg.WorkDir;
                        if (_fromProc.Trim().ToLower() == "verify")
                            sRoot = staMain.stcProjCfg.IndexDir;

                        if (_batchType.Trim().ToLower() == "set")
                        {
                            sDir = mGlobal.addDirSep(sRoot) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";
                        }
                        else //Batch.
                        {
                            if (_docType.Trim() == "")
                            {
                                sDir = mGlobal.addDirSep(sRoot) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            }
                            else
                                sDir = mGlobal.addDirSep(sRoot) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }

                        if (nodeInfo.Trim().ToLower() == "page")
                        {
                            if (node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                if (_fromProc.Trim().ToLower() == "verify")
                                    sSrcFileName = oDF.getDocFilenameDB(rowid, "TDocuIndex");
                                else
                                    sSrcFileName = oDF.getDocFilenameDB(rowid, "TDocuScan");

                                FileInfo oFI = new FileInfo(sSrcFileName);
                                sNewFileName = mGlobal.addDirSep(sDir) + oFI.Name;
                                sImgInfoCopy = rowid + "|" + sNewFileName;
                            }
                        }

                        if (sImgInfoCopy.Trim() != "")
                        {
                            if (nodeInfo.Trim().ToLower() == "page")
                            {
                                if ((_docType.Trim() != "" && _docType.Trim() != sCurrDocType.Trim()))
                                {
                                    string sCollaNumFmt = "000";
                                    int iCollaNum = 0;
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        if (oDF.checkDocuIndexExist(_currScanProj, _appNum, _batchCode, _setNum, _docType)) //New DocType
                                        {
                                            iCollaNum = oDF.getCurrCollaNumIndex(_currScanProj, _appNum, _batchCode, _setNum, _docType);
                                            if (iCollaNum < 1) iCollaNum = 1;
                                        }
                                        else
                                            iCollaNum += 1;
                                    }
                                    else
                                    {
                                        if (oDF.checkDocuScanExist(_currScanProj, _appNum, _batchCode, _setNum, _docType)) //New DocType
                                        {
                                            iCollaNum = oDF.getCurrCollaNum(_currScanProj, _appNum, _batchCode, _setNum, _docType);
                                            if (iCollaNum < 1) iCollaNum = 1;
                                        }
                                        else
                                            iCollaNum += 1;
                                    }

                                    _collaNum = iCollaNum.ToString(sCollaNumFmt);

                                    DataRowCollection oDrs = getDocTypeDocScanDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrDocType, rowid, iImgSeq);

                                    FileInfo oFi = null;
                                    int i = 0, s = 0; List<string> lNewFile = new List<string>();
                                    while (i < oDrs.Count)
                                    {
                                        rowid = oDrs[i]["rowid"].ToString();
                                        sSrcFileName = oDrs[i]["docimage"].ToString();

                                        oFi = new FileInfo(sSrcFileName);
                                        sNewFileName = mGlobal.addDirSep(sDir) + oFi.Name;

                                        try
                                        {
                                            if (!Directory.Exists(sDir)) Directory.CreateDirectory(sDir);

                                            File.Move(sSrcFileName, sNewFileName);
                                            lNewFile.Add(sNewFileName);
                                        }
                                        catch (Exception ex)
                                        {
                                            bOK = false;
                                            mGlobal.Write2Log("Insert separator:" + ex.Message);
                                        }

                                        if (bOK)
                                        {
                                            if (insertDocTypeDocScanDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrDocType, _docType, rowid, sNewFileName))
                                            {
                                                s += 1;
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show(this, "File(s) move error! Please check with system person in charge.", "Message");
                                            break;
                                        }

                                        i += 1;
                                    }

                                    if (s > 0)
                                    {
                                        if (_fromProc.Trim().ToLower() == "verify")
                                            oDF.reorderIndexImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, 1, _docType);
                                        else
                                            oDF.reorderScanImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, 1, _docType);

                                        short iCurrImgIdx = MDIMain.oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                        try
                                        {
                                            if (oImgCut.ImageBuffer.HowManyImagesInBuffer > 0)
                                            {
                                                Byte[] oBytes = null;

                                                using (MemoryStream oMem = new MemoryStream())
                                                {
                                                    oSepBmp.Save(oMem, System.Drawing.Imaging.ImageFormat.Tiff);
                                                    oBytes = oMem.ToArray();
                                                }

                                                MDIMain.oImgCore.IO.LoadImageFromBytes(oBytes, EnumImageFileFormat.WEBTW_TIF);
                                                //MDIMain.oImgCore.ImageBuffer.SwitchImage(MDIMain.oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                                                MDIMain.oImgCore.ImageBuffer.MoveImage(MDIMain.oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                                            }

                                            lScanImg.Insert(iCurrImgIdx, _docType);

                                            if (lNewFile.Count > 0)
                                            {
                                                int j = 0; int k = iCurrImgIdx + 1;
                                                while (j < lNewFile.Count)
                                                {
                                                    lScanImg[k] = lNewFile[j];

                                                    j += 1;
                                                    k += 1;
                                                }
                                            }

                                        }
                                        catch (ImageCoreException icex)
                                        {
                                            mGlobal.Write2Log("Reorder viewer: " + icex.Message);
                                        }
                                        catch (Exception ex)
                                        {
                                            mGlobal.Write2Log("Reorder viewer:" + ex.Message);
                                        }

                                        if (oImgCut != null) oImgCut.Dispose();

                                        reloadForEdit(); //Refresh.
                                    }
                                    selectedNode = node;
                                }
                                else
                                    MessageBox.Show(this, "Insert the same document separator for the same document type is not allowed!" + Environment.NewLine
                                        + "Please use another different document separator.", "Message");
                            }
                        }
                        else
                            MessageBox.Show(this, "Please select either document type or page for insert a document separator.", "Message");
                    }
                    else
                        MessageBox.Show(this, "Error! " + sErrMsg, "Message");
                }
                else
                    MessageBox.Show(this, "Please select a page or document type for scan and insert a document separator.", "Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Insert separator.." + ex.Message);
            }
            finally
            {
                if (oSepBmp != null) oSepBmp.Dispose();

                //btnScan.Enabled = true;
                enableScan(true);
                //btnSend.Enabled = true;
                SendStripBtn.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                TreeNode[] oNodes = tvwBatch.Nodes.Find(selectedNode.Name, true);
                if (oNodes.Length > 0)
                {
                    tvwBatch.SelectedNode = oNodes[0];
                    tvwBatch.SelectedNode.EnsureVisible();
                }

                //tvwBatch.Refresh();
            }
        }

        private bool insertDocTypeDocScanDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocTypeOld, string pDocTypeNew, string pRowId, string pNewFilename)
        {
            bool bSaved = true;
            string sSQL;
            string sTable = "TDocuScan";
            try
            {
                if (_fromProc.Trim().ToLower() == "verify")
                    sTable = "TDocuIndex";

                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + sTable + " ";
                sSQL += "SET collanum='" + _collaNum.Trim().Replace("'", "") + "' ";
                sSQL += ",doctype='" + pDocTypeNew.Trim().Replace("'", "") + "' ";
                sSQL += ",docimage='" + pNewFilename.Trim().Replace("'", "") + "' ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocTypeOld.Trim().Replace("'", "") + "' ";
                sSQL += "AND rowid='" + pRowId.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                {
                    bSaved = false;
                    throw new Exception("Saving data failure!");
                }

            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private DataRowCollection getDocTypeDocScanDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocTypeOld, string pRowId, int pCurrImgSeq)
        {
            string sSQL;
            string sTable = "TDocuScan";
            try
            {
                if (_fromProc.Trim().ToLower() == "verify")
                    sTable = "TDocuIndex";

                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT DISTINCT rowid,docimage,imageseq FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + sTable + " ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocTypeOld.Trim().Replace("'", "") + "' ";
                sSQL += "AND rowid>='" + pRowId.Trim().Replace("'", "") + "' ";
                sSQL += "AND imageseq>='" + pCurrImgSeq + "' ";
                sSQL += "ORDER BY imageseq ";
                return mGlobal.objDB.ReturnRows(sSQL);
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        private void mnuImpSepStrip_Click(object sender, EventArgs e)
        {
            TreeNode node = null;
            try
            {
                if (_batchType.Trim().ToLower() == "set")
                {
                    MessageBox.Show(this, "Batch Set Separator is not supported!", "Message");
                    return;
                }

                string nodeInfo = "";
                string sSrcFileName = "";
                string sNewFileName = "";

                string rowid = "";
                sImgInfoCopy = "";

                if (tvwBatch.SelectedNode != null && _noSeparator.Trim() != "0") //Only allow 1 or 2 separator for insert doctype.
                {
                    node = tvwBatch.SelectedNode;
                    selectedNode = node;

                    ofdImportFile.InitialDirectory = "C:\\";
                    ofdImportFile.Filter = "(*.tiff)|*.tiff|(*.pdf)|*.pdf";
                    ofdImportFile.RestoreDirectory = false;

                    if (ofdImportFile.ShowDialog(this) == DialogResult.Cancel)
                        return;

                    ofdImportFile.Dispose();

                    clsTMScan oTMScan = new clsTMScan();
                    oTMScan.ScanBarcodeOnly = true;

                    bool bOK = oTMScan.ReadFromImageBarcode(ofdImportFile.FileName);
                    _docType = oTMScan.DocType.Trim();
                    sErrMsg = oTMScan.sErrMsg;
                    oImgCut = oTMScan.ImageScan;
                    oSepBmp = oTMScan.ScanBmp;

                    if (sErrMsg.Trim() == string.Empty)
                    {
                        if (_noSeparator.Trim() == "1" && _docType.Trim().ToUpper() != staMain.stcProjCfg.SeparatorText)
                        {
                            MessageBox.Show(this, "Batch Set Separator is expected only!", "Message");
                            return;
                        }
                        else if (_noSeparator.Trim() == "2" && _docType.Trim().ToUpper() == staMain.stcProjCfg.SeparatorText)
                        {
                            MessageBox.Show(this, "Batch Set Separator is not allowed. Document Separator is expected only!", "Message");
                            return;
                        }
                        else if (_noSeparator.Trim() == "2" && _docType.Trim().ToUpper() == "")
                        {
                            MessageBox.Show(this, "Document Separator is expected only!", "Message");
                            return;
                        }
                    }

                    //btnScan.Enabled = false;
                    enableScan(false);
                    //btnSend.Enabled = false;
                    SendStripBtn.Enabled = false;
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;

                    if (bOK)
                    {
                        bImgCut = true; //for insert.

                        nodeInfo = node.Text.Trim().Substring(0, 4);
                        //sep = node.Tag.ToString().Trim().Substring(0, 4);                      

                        sCurrDocType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        sCurrSetNum = node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();
                        _setNum = Convert.ToInt32(sCurrSetNum).ToString(sSetNumFmt);
                        _collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                        iImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        string sDir = "";
                        string sRoot = staMain.stcProjCfg.WorkDir;
                        if (_fromProc.Trim().ToLower() == "verify")
                            sRoot = staMain.stcProjCfg.IndexDir;

                        if (_batchType.Trim().ToLower() == "set")
                        {
                            sDir = mGlobal.addDirSep(sRoot) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";
                        }
                        else //Batch.
                        {
                            if (_docType.Trim() == "")
                            {
                                sDir = mGlobal.addDirSep(sRoot) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            }
                            else
                                sDir = mGlobal.addDirSep(sRoot) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }

                        if (nodeInfo.Trim().ToLower() == "page")
                        {
                            if (node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                if (_fromProc.Trim().ToLower() == "verify")
                                    sSrcFileName = oDF.getDocFilenameDB(rowid, "TDocuIndex");
                                else
                                    sSrcFileName = oDF.getDocFilenameDB(rowid, "TDocuScan");

                                FileInfo oFI = new FileInfo(sSrcFileName);
                                sNewFileName = mGlobal.addDirSep(sDir) + oFI.Name;
                                sImgInfoCopy = rowid + "|" + sNewFileName;
                            }
                        }

                        if (sImgInfoCopy.Trim() != "")
                        {
                            if (nodeInfo.Trim().ToLower() == "page")
                            {
                                if ((_docType.Trim() != "" && _docType.Trim() != sCurrDocType.Trim()))
                                {
                                    string sCollaNumFmt = "000";
                                    int iCollaNum = 0;
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        if (oDF.checkDocuIndexExist(_currScanProj, _appNum, _batchCode, _setNum, _docType)) //New DocType
                                        {
                                            iCollaNum = oDF.getCurrCollaNumIndex(_currScanProj, _appNum, _batchCode, _setNum, _docType);
                                            if (iCollaNum < 1) iCollaNum = 1;
                                        }
                                        else
                                            iCollaNum += 1;
                                    }
                                    else
                                    {
                                        if (oDF.checkDocuScanExist(_currScanProj, _appNum, _batchCode, _setNum, _docType)) //New DocType
                                        {
                                            iCollaNum = oDF.getCurrCollaNum(_currScanProj, _appNum, _batchCode, _setNum, _docType);
                                            if (iCollaNum < 1) iCollaNum = 1;
                                        }
                                        else
                                            iCollaNum += 1;
                                    }

                                    _collaNum = iCollaNum.ToString(sCollaNumFmt);

                                    DataRowCollection oDrs = getDocTypeDocScanDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrDocType, rowid, iImgSeq);

                                    FileInfo oFi = null;
                                    int i = 0, s = 0; List<string> lNewFile = new List<string>();
                                    while (i < oDrs.Count)
                                    {
                                        rowid = oDrs[i]["rowid"].ToString();
                                        sSrcFileName = oDrs[i]["docimage"].ToString();

                                        oFi = new FileInfo(sSrcFileName);
                                        sNewFileName = mGlobal.addDirSep(sDir) + oFi.Name;

                                        try
                                        {
                                            if (!Directory.Exists(sDir)) Directory.CreateDirectory(sDir);

                                            File.Move(sSrcFileName, sNewFileName);
                                            lNewFile.Add(sNewFileName);
                                        }
                                        catch (Exception ex)
                                        {
                                            bOK = false;
                                            mGlobal.Write2Log("Insert separator:" + ex.Message);
                                        }

                                        if (bOK)
                                        {
                                            if (insertDocTypeDocScanDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrDocType, _docType, rowid, sNewFileName))
                                            {
                                                s += 1;
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show(this, "File(s) move error! Please check with system person in charge.", "Message");
                                            break;
                                        }

                                        i += 1;
                                    }

                                    if (s > 0)
                                    {
                                        if (_fromProc.Trim().ToLower() == "verify")
                                            oDF.reorderIndexImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, 1, _docType);
                                        else
                                            oDF.reorderScanImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, 1, _docType);

                                        short iCurrImgIdx = MDIMain.oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                        try
                                        {
                                            if (oImgCut.ImageBuffer.HowManyImagesInBuffer > 0)
                                            {
                                                Byte[] oBytes = null;

                                                using (MemoryStream oMem = new MemoryStream())
                                                {
                                                    oSepBmp.Save(oMem, System.Drawing.Imaging.ImageFormat.Tiff);
                                                    oBytes = oMem.ToArray();
                                                }

                                                MDIMain.oImgCore.IO.LoadImageFromBytes(oBytes, EnumImageFileFormat.WEBTW_TIF);
                                                //MDIMain.oImgCore.ImageBuffer.SwitchImage(MDIMain.oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                                                MDIMain.oImgCore.ImageBuffer.MoveImage(MDIMain.oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                                                iCurrImgIdx = MDIMain.oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                            }

                                            lScanImg.Insert(iCurrImgIdx, _docType);

                                            if (lNewFile.Count > 0)
                                            {
                                                int j = 0; int k = iCurrImgIdx + 1;
                                                while (j < lNewFile.Count)
                                                {
                                                    lScanImg[k] = lNewFile[j];

                                                    j += 1;
                                                    k += 1;
                                                }
                                            }

                                        }
                                        catch (ImageCoreException icex)
                                        {
                                            mGlobal.Write2Log("Reorder viewer: " + icex.Message);
                                        }
                                        catch (Exception ex)
                                        {
                                            mGlobal.Write2Log("Reorder viewer:" + ex.Message);
                                        }

                                        if (oImgCut != null) oImgCut.Dispose();

                                        reloadForEdit(); //Refresh.
                                    }
                                    selectedNode = node;
                                }
                                else
                                    MessageBox.Show(this, "Insert the same document separator for the same document type is not allowed!" + Environment.NewLine
                                        + "Please use another different document separator.", "Message");
                            }
                        }
                        else
                            MessageBox.Show(this, "Please select either document type or page for insert a document separator.", "Message");
                    }
                    else
                        MessageBox.Show(this, "Error! " + sErrMsg, "Message");
                }
                else
                    MessageBox.Show(this, "Please select a page or document type for scan and insert a document separator.", "Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Insert separator.." + ex.Message);
            }
            finally
            {
                if (oSepBmp != null) oSepBmp.Dispose();

                //btnScan.Enabled = true;
                enableScan(true);
                //btnSend.Enabled = true;
                SendStripBtn.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                TreeNode[] oNodes = tvwBatch.Nodes.Find(selectedNode.Name, true);
                if (oNodes.Length > 0)
                {
                    tvwBatch.SelectedNode = oNodes[0];
                    tvwBatch.SelectedNode.EnsureVisible();
                }

                //tvwBatch.Refresh();
            }
        }

        private void frmRescanBox1_Activated(object sender, EventArgs e)
        {
            bool bValid = true;
            try
            {
                if (mGlobal.checkOpenedForms("frmScanBox1"))
                    bValid = false;

                if (!bValid)
                {
                    MessageBox.Show("The Box Scan windows is opened!");
                    enableScan(false);
                }
                else
                    enableScan(true);
            }
            catch (Exception ex)
            {
            }
        }

        private void enableScan(bool pEnable)
        {
            try
            {
                if (this.MdiParent != null)
                {
                    ToolStrip oToolStrip = (ToolStrip)this.MdiParent.Controls["toolStrip"];
                    oToolStrip.Items[0].Visible = true;
                    oToolStrip.Items[0].Enabled = pEnable;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private bool idpFromImageClassify(System.Drawing.Bitmap pBmp)
        {
            bool isDocSep = false;
            //System.Drawing.Bitmap bmp = null;
            try
            {
                //if (MDIMain.oImgCore.ImageBuffer.CurrentImageIndexInBuffer >= 0)
                if (MDIMain.oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                {
                    string sLocalEndPoint = mGlobal.GetAppCfg("IDPBaseUrl").Trim() + "/analyze";
                    clsIDP oIDP = new clsIDP();
                    RequestIDP oReq = new RequestIDP();
                    oReq.id = DateTime.Now.ToString("yyyyMMddhhmmss");
                    oReq.endPoint = clsIDP.stcIDPMapping.EndPoint.Trim(); //mGlobal.GetAppCfg("IDPEndPoint").Trim();
                    oReq.apiKey = clsIDP.stcIDPMapping.IDPKey.Trim(); //mGlobal.GetAppCfg("IDPKey").Trim();
                    byte[] data;
                    using (var oMS = new MemoryStream())
                    {
                        pBmp.Save(oMS, System.Drawing.Imaging.ImageFormat.Bmp);
                        data = oMS.ToArray();
                    }
                    oReq.baseString = Convert.ToBase64String(data);
                    oReq.classify = "Yes";
                    oReq.model = "DocClassDemo";

                    string sMsg = "";
                    Task<dynamic> dResult = Task.Run(async () => oIDP.docClassify(sLocalEndPoint, oReq, ref sMsg));
                    //dResult.Wait();
                    while (!dResult.IsCompleted)
                    {
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0));

                        if (dResult.Result != null) break;
                    }

                    if (dResult.Result != null)
                    {
                        if (dResult?.Result.GetType() == typeof(bool))
                        {
                            if (dResult.Result == false)
                                MessageBox.Show(this, "Failed! " + sMsg);
                        }
                        else
                        {
                            ClassifyResult oClr = ClassifyResult.FromJson(dResult.Result);
                            if (oClr != null)
                            {
                                if (oClr.Documents.Length > 0)
                                    if (oClr.Documents[0].DocumentType != null)
                                    {
                                        _docType = oClr.Documents[0].DocumentType.Trim();
                                        //isDocSep = true;
                                    }
                            }
                        }
                    }

                    if (dResult.IsCompleted) dResult.Dispose();
                }
                else
                {
                    if (!bIsDuplexEnabled)
                        mGlobal.Write2Log("IDP with image buffer index invalid!");
                }

            }
            catch (Exception exp)
            {
                mGlobal.Write2Log("IDP classifying.." + exp.Message + Environment.NewLine + exp.StackTrace.ToString());
                //MessageBox.Show(exp.Message, "Classifying error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //if (bmp != null) bmp.Dispose();
            }

            return isDocSep;
        }

        private void SendStripBtn_Click(object sender, EventArgs e)
        {
            try
            {
                btnSend_Click(sender, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void remarksStripMni_Click(object sender, EventArgs e)
        {
            try
            {
                frmRemarks oRemarks = new frmRemarks();
                oRemarks.bEdit = false;
                oRemarks.sRemarks = sRemarks;

                oRemarks.Show(this);
            }
            catch (Exception ex)
            {
            }
        }


    }
}
