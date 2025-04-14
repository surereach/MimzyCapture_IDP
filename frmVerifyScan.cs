using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Dynamsoft;
using Dynamsoft.DBR;
using Dynamsoft.TWAIN;
using Dynamsoft.TWAIN.Interface;
using Dynamsoft.Core;
using Dynamsoft.PDF;
using Dynamsoft.Core.Enums;
using Dynamsoft.Core.Annotation;


namespace SRDocScanIDP
{
    public partial class frmVerifyScan : Form
    {
        private ImageCore _oImgCore;

        private string _currScanProj;
        private string _appNum;
        private string _batchType;
        private string _batchCode;
        //private string _collaNum;
        private string _docType;
        private string _setNum;
        private string _filename;
        private int _totPage;
        private string _docDefId;

        private string _station;
        private string _userId;

        private string sSetNumFmt;
		private string _noSeparator;							

        private clsDocuFile oDF = new clsDocuFile();

        private int _currentImageIndex = -1;
        private bool _isSaved;
        private DateTime _verifyStart;
        private DateTime _verifyEnd;

        private staMain._stcImgInfo _stcImgInfo;
        private long _iCurrBatchRowid;
        private string sCurrSetNum;

        public bool bSingleView;

        private const int FORM_MIN_WIDTH = 1280;
        private const int FORM_MIN_HEIGHT = 720;
        private int iFormOffset = 65;

        public frmVerifyScan()
        {
            InitializeComponent();

            _oImgCore = new ImageCore();
            _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;
            dsvImg.Bind(_oImgCore);
            dsvThumbnailList.Bind(_oImgCore);

            dsvImg.IfFitWindow = true; //Windows Size if true else Actual Size set to false.
            //dsvImg.Zoom = 1; //Actual Size.

            dsvImg.MouseShape = true;
            dsvImg.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;

            dsvThumbnailList.MouseShape = true;
            dsvThumbnailList.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;

            _currScanProj = MDIMain.strScanProject; //this.MdiParent.Controls["toolStrip"].Controls[0].Text;
            _appNum = MDIMain.intAppNum.ToString("000");
            _docDefId = staMain.stcProjCfg.DocDefId;
            _noSeparator = staMain.stcProjCfg.NoSeparator;

            try
            {
                bSingleView = false;

                if (staMain.stcProjCfg.BatchType.Trim().ToLower() != "box")
                {
                    Application.AddMessageFilter(new clsMouseFilterMess(this));
                }
                else
                {
                    Application.AddMessageFilter(new clsMouseKbdFilterMess(this));                    
                }
                staMain.sessionRestart();
            }
            catch (Exception)
            {
            }
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

        private void frmVerifyScan_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = this.Text + ": " + MDIMain.strScanProject;
                this.timer1.Enabled = true;

                this.ShowIcon = false;
                //this.MaximizeBox = false;
                //this.MinimizeBox = false;
                this.WindowState = FormWindowState.Maximized;
                this.IndexStatusStrip.Width = this.Width;
                //this.ScanStatusBar1.Width = this.ScanStatusStrip.Width - this.ScanStatusBar.Width - this.ScanStatusBar2.Width - this.CurrDateTime.Width;

                _currScanProj = MDIMain.strScanProject; //this.MdiParent.Controls["toolStrip"].Controls[0].Text;
                _appNum = MDIMain.intAppNum.ToString("000");
                _setNum = "";                
                //_collaNum = "";
                _batchType = MDIMain.sBatchType;
                _batchCode = "";
                _docType = "";
                _filename = "";
                _verifyStart = DateTime.Now;
                _verifyEnd = DateTime.Now;

                _totPage = 0; //Init.
                _isSaved = false;

                _station = MDIMain.strStation;
                _userId = MDIMain.strUserID;

                VerifyingStatusBar.Text = "Verifying"; //1 - Scan, 2 - Indexing, 3 - Deleted, 4 - Rescan, 5 - Verify, 6 - Export, 7 - Transfer, 8 - Reindex.

                dsvImg.IfFitWindow = true;
                dsvImg.SetViewMode(-1, -1);

                dsvThumbnailList.IfFitWindow = true;
                dsvThumbnailList.SetViewMode(1, 10);

                _stcImgInfo = new staMain._stcImgInfo();
                _iCurrBatchRowid = 0;

                if (_batchType.Trim().ToLower() == "set")
                    sSetNumFmt = "000000";
                else
                    sSetNumFmt = "000";

                if (bSingleView == false)
                {
                    loadImagesFrmDisk(0);

                    setImgInfo(0);

                    CheckImageCount();

                    //loadBatchSetTreeview(_verifyStart, _verifyEnd);
                    loadBatchSetTreeview();
                    if (tvwSet.Nodes.Count > 0)
                        tvwSet.ExpandAll();
                }

                if (staMain.stcProjCfg.BatchType.Trim() != "Box")
                {
                    boxStripLbl.Visible = false;
                    boxNoStripTxt.Visible = false;

                    boxLabelStripLbl.Visible = false;
                    boxLabelStripTxt.Visible = false;
                }
                staMain.sessionRestart();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }            
        }

        private void frmVerifyScan_Paint(object sender, PaintEventArgs e)
        {
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

        private void frmVerifyScan_Resize(object sender, EventArgs e)
        {
            try
            {
                formResize();

                System.Drawing.Point oPoint = new System.Drawing.Point();
                if (tvwSet.Width + dsvImg.Width + dsvThumbnailList.Width + 65 < this.Width)
                {
                    dsvImg.Width = this.Width - tvwSet.Width - dsvThumbnailList.Width - 65;
                    dsvImg.Height = txtInfo.Location.Y - dsvImg.Location.Y - 5;

                    txtInfo.Width = dsvImg.Width - panel1.Width - 5;
                    oPoint.Y = dsvImg.Location.Y + dsvImg.Height + 5;
                    oPoint.X = dsvImg.Location.X;
                    txtInfo.Location = oPoint;
                    dsvImg.Height = txtInfo.Location.Y - dsvImg.Location.Y - 5;

                    oPoint.Y = panel1.Location.Y;
                    oPoint.X = txtInfo.Location.X + txtInfo.Width + 5;
                    panel1.Location = oPoint;

                    oPoint.Y = dsvThumbnailList.Location.Y;
                    oPoint.X = dsvImg.Location.X + dsvImg.Width + 5;
                    dsvThumbnailList.Location = oPoint;
                    dsvThumbnailList.Height = dsvImg.Height + txtInfo.Height + 3;
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

                this.VerifyingStatusBar1.Width = this.Width - this.VerifyingStatusBar.Width - this.VerifyingStatusBar2.Width - this.CurrDateTime.Width - iFormOffset;

                if (this.VerifyingStatusBar.Width > 268)
                    this.VerifyingStatusBar.Width = 268;
                if (this.VerifyingStatusBar1.Width < 658)
                    this.VerifyingStatusBar1.Width = 658;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CurrDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
        }

        private void frmVerifyScan_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (this.MdiParent != null && mGlobal.checkAnyOpenedForms() == false)
                    this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = true;

                if (_oImgCore != null)
                    _oImgCore.Dispose();

                timer1.Enabled = false;

                staMain.sessionRestart();
                //frmProcSumm oSumm = new frmProcSumm();
                //oSumm.refreshProcSummaryTreeview();
                //oSumm.Dispose();
            }
            catch (Exception ex)
            {
            }
        }

        private void dsvThumbnailList_OnMouseClick(short sImageIndex)
        {
            txtInfo.Text = "";
            //_oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sImageIndex;
            //dsvImg.Bind(_oImgCore);

            setImgInfo(sImageIndex);

            CheckImageCount();
        }

        public void reloadForAddOn()
        {
            this._oImgCore = new ImageCore();
            _stcImgInfo = new staMain._stcImgInfo();

            this.frmVerifyScan_Load(this, new EventArgs());
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

        private void loadImagesFrmDisk(long pCurrBatchRowId, bool pViewOnly = true)
        {
            string sBatchStatusIn = "'2','4','6'";
            string sIndexStatusIn = "'I','R','A'";
            try
            {
                if (pViewOnly)
                {
                    sBatchStatusIn = "'1'"; //View Verified only.
                    sIndexStatusIn = "'S'";
                }
                DataRow bdr = oDF.getDocBatchDB(_currScanProj, _appNum, _batchType, sBatchStatusIn, _station, "", pCurrBatchRowId);
                //DataRow bdr = oDF.getProcessDocBatchDB(_currScanProj, _appNum, _batchType, sBatchStatusIn, _station, "", pCurrBatchRowId);
                initImgInfoCollection(1);

                if (bdr != null)
                {
                    _iCurrBatchRowid = Convert.ToInt32(bdr["rowid"].ToString());

                    boxNoStripTxt.Text = bdr["boxnum"].ToString();
                    boxLabelStripTxt.Text = bdr["boxlabel"].ToString();

                    _setNum = bdr["setnum"].ToString();
                    _batchCode = bdr["batchcode"].ToString();
                    _totPage = Convert.ToInt32(bdr["totpagecnt"].ToString());

                    txtRemarks.Text = bdr["remarks"].ToString();

                    initImgInfoCollection(_totPage);

                    _stcImgInfo.ScanPrj = _currScanProj;
                    _stcImgInfo.AppNum = _appNum;
                    _stcImgInfo.BatchCode = _batchCode;
                    _stcImgInfo.SetNum[0] = _setNum;                    

                    DataRowCollection drs = oDF.getDocFileByBatchCodeDB(_currScanProj, _appNum, _batchCode, sIndexStatusIn);

                    string sFilePath = "";

                    if (drs != null)
                    {
                        if (_totPage != drs.Count)
                            initImgInfoCollection(drs.Count);

                        int iTot = _totPage;
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
                            }
                            try
                            {
                                if (File.Exists(sFilePath))
                                    _oImgCore.IO.LoadImage(sFilePath);
                            }
                            catch (Exception ex)
                            {
                            }

                            i += 1;
                        }

                        if (i > 0)
                        {
                            sCurrSetNum = _stcImgInfo.SetNum[0].ToString();
                        }
                        
                    }

                    dsvThumbnailList.Bind(_oImgCore);

                    if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                    {
                        _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = 0;
                        _stcImgInfo.CurrImgIdx = 0;
                        dsvImg.Bind(_oImgCore);

                        dsvImg.IfFitWindow = true; //Windows Size if true else Actual Size set to false.
                        //dsvImg.Zoom = 1; //Actual Size.
                    }
                        
                }  

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Verifying.." + _batchCode + ".." + ex.Message);
            }
        }

        private void setImgInfo(short sImageIndex)
        {
            if (_stcImgInfo.ImgIdx.Length > 0 && _stcImgInfo.ImgIdx.Length > sImageIndex)
            {
                if (_stcImgInfo.BatchCode != null)
                {
                    _stcImgInfo.CurrImgIdx = sImageIndex;
                    VerifyingStatusBar1.Text = "Document Set " + _stcImgInfo.BatchCode + " : " + _stcImgInfo.DocType[sImageIndex].ToString();
                    VerifyingStatusBar2.Text = "Total Page: " + _totPage;
                    if (_stcImgInfo.Page[sImageIndex].Trim().ToUpper() == "F")
                        txtInfo.Text = "Front (" + _stcImgInfo.BatchCode.Replace("_", "-") + "-1)"; //Page 1.
                    else
                        txtInfo.Text = "Back (" + _stcImgInfo.BatchCode.Replace("_", "-") + "-1)"; //Page 1.
                }
            }
            else
            {
                VerifyingStatusBar1.Text = "Document Set " + _stcImgInfo.BatchCode + " : ";
                VerifyingStatusBar2.Text = "Total Page: " + _totPage;
                if (_stcImgInfo.Page[sImageIndex].Trim().ToUpper() == "F")
                    txtInfo.Text = "Front (" + _stcImgInfo.BatchCode.Replace("_", "-") + "-1)"; //Page 1.
                else
                    txtInfo.Text = "Back (" + _stcImgInfo.BatchCode.Replace("_", "-") + "-1)"; //Page 1.
            }
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

            ShowSelectedImageArea();
        }

        private void ShowSelectedImageArea()
        {
            if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer >= 0)
            {
                short imgIdx = (short)(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                var recSelArea = dsvImg.GetSelectionRect(imgIdx);
                var imgCurrent = _oImgCore.ImageBuffer.GetBitmap(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            }
        }

        private void loadBatchSetTreeview(bool pViewOnly = true)
        {
            string sBatchStatusIn = "'2','4','6'";
            string sIndexStatusIn = "'I','R','A'";

            if (pViewOnly) //View Verify only.
            {
                sBatchStatusIn = "'1'";
                sIndexStatusIn = "'S'";
            }

            staMain._ScanProj = _currScanProj;
            staMain._AppNum = _appNum;

            if (_noSeparator.Trim() == "0")
                staMain.loadBatchTreeviewNoSep(this.tvwSet, _batchType, _batchCode, sBatchStatusIn, sIndexStatusIn);
            else
            {
                if (_batchType.Trim().ToLower() == "set")
                {
                    staMain.loadDocuSetTreeview(this.tvwSet, _batchType, _batchCode, sBatchStatusIn, sIndexStatusIn);
                }
                else //Batch process type.
                {
                    staMain.loadBatchTreeview(this.tvwSet, _batchType, _batchCode, sBatchStatusIn, sIndexStatusIn);
                }
            }
        }

        private void loadBatchSetTreeview(DateTime pDateStart, DateTime pDateEnd)
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
                sSQL += "AND batchstatus IN ('2','4','5') ";
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
                        tvwSet.Nodes.Clear();

                        int i = 0;
                        TreeNode node; DataRow dr = null;
                        while (i < dsTVw.Tables[0].Rows.Count)
                        {
                            dr = dsTVw.Tables[0].Rows[i];

                            sTag = "Sep_" + dr["batchcode"].ToString();
                            node = new TreeNode("Document Set " + dr["batchcode"].ToString().Replace("_","-"));
                            node.Tag = new String(sTag.ToCharArray());
                            tvwSet.Nodes.Add(node);

                            i += 1;
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Document Type
                int nc = 0;
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct doctype ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND indexstatus IN ('I','R','V') ";
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
                                tvwSet.Nodes[nc].Nodes.Add(node);

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

                while (nc < tvwSet.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwSet.Nodes[nc].Nodes.Count)
                    {
                        sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);

                        sSQL = "SELECT Distinct rowid,batchcode,doctype,indexstart ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                        sSQL += "WHERE scanproj='" + _currScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText + "' ";
                        sSQL += "AND indexstatus IN ('I','R','V') ";
                        sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                        sSQL += "AND doctype='" + sNodeText1 + "' ";
                        //sSQL += "AND indexstation='" + _station.Trim().Replace("'", "") + "' ";
                        //sSQL += "AND indexuser='" + _userId.Trim().Replace("'", "") + "' ";
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
                                    tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);

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

        private void initImageAllInfo()
        {
            try
            {
                txtInfo.Text = "";
                txtCurrImgIdx.Text = "0";
                txtTotImgNum.Text = "0";

                _stcImgInfo = new staMain._stcImgInfo();

                _oImgCore = new ImageCore(); //Init.

                dsvImg.IfFitWindow = true;

                dsvImg.Bind(_oImgCore); //Clear.
                dsvThumbnailList.Bind(_oImgCore);

                tvwSet.Nodes.Clear();

                VerifyingStatusBar.Text = "";
                VerifyingStatusBar1.Text = "";
                VerifyingStatusBar2.Text = "";
            }
            catch (Exception ex)
            {
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

        private void picboxNext_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 &&
                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer < _oImgCore.ImageBuffer.HowManyImagesInBuffer - 1)
                ++_oImgCore.ImageBuffer.CurrentImageIndexInBuffer;

            setImgInfo(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);

            CheckImageCount();
        }

        private void picboxPrevious_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 && _oImgCore.ImageBuffer.CurrentImageIndexInBuffer > 0)
                --_oImgCore.ImageBuffer.CurrentImageIndexInBuffer;

            setImgInfo(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);

            CheckImageCount();
        }

        private void picboxFirst_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = 0;

            setImgInfo(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);

            CheckImageCount();
        }

        private void picboxLast_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = (short)(_oImgCore.ImageBuffer.HowManyImagesInBuffer - 1);

            setImgInfo(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);

            CheckImageCount();
        }

        private void goNextSet(bool bNext)
        {
            try
            {
                //if (_isSaved)
                //{               

                long lRowId = getNextVerifyRowId(_currScanProj, _appNum, _batchCode, _iCurrBatchRowid);

                if (lRowId == -1)
                {
                    MessageBox.Show(this, "Document batch have reached last indexed documents.", "Message");
                    //_iCurrBatchRowid = 0;
                }
                else
                {
                    _oImgCore = new ImageCore();

                    //dsvImg.IfFitWindow = true;

                    dsvImg.Bind(_oImgCore); //Clear.
                    dsvThumbnailList.Bind(_oImgCore);

                    _verifyStart = DateTime.Now;

                    VerifyingStatusBar.Text = "Loading";
                    btnEnable(false);

                    _iCurrBatchRowid = lRowId;

                    if (bNext)
                        loadImagesFrmDisk(_iCurrBatchRowid);
                    else
                        loadImagesFrmDisk(0);

                    setImgInfo(0);

                    CheckImageCount();

                    //loadBatchSetTreeview(_verifyStart, _verifyEnd);
                    loadBatchSetTreeview();
                    if (tvwSet.Nodes.Count > 0)
					{
                        tvwSet.ExpandAll();
                        if (tvwSet.Nodes[0].Nodes.Count > 0)
                            tvwSet.SelectedNode = tvwSet.Nodes[0].Nodes[0];
                        else
                            tvwSet.SelectedNode = tvwSet.Nodes[0];
                    }
						
                }
                _isSaved = false;
                //}
                //else
                //    MessageBox.Show("Document set " + _batchCode + " is not save! \nPlease click save button before proceed to next document set.");
            }
            catch (Exception ex)
            {
            }
            finally
            {
                VerifyingStatusBar.Text = "Verifying";
                btnEnable(true);
            }
        }

        private void goPreviousSet(bool bPrev)
        {
            try
            {
                //if (_isSaved)
                //{                

                _iCurrBatchRowid = getPrevVerifyRowId(_currScanProj, _appNum, _batchCode, _iCurrBatchRowid);

                if (_iCurrBatchRowid == -1)
                {
                    MessageBox.Show(this, "Document batch have reached first indexed documents.", "Message");
                    //_iCurrBatchRowid = 0;
                }
                else
                {
                    _oImgCore = new ImageCore();

                    //dsvImg.IfFitWindow = true;

                    dsvImg.Bind(_oImgCore); //Clear.
                    dsvThumbnailList.Bind(_oImgCore);

                    _verifyStart = DateTime.Now;

                    VerifyingStatusBar.Text = "Loading";
                    btnEnable(false);

                    if (bPrev)
                        loadImagesFrmDisk(_iCurrBatchRowid);
                    else
                        loadImagesFrmDisk(0);

                    setImgInfo(0);

                    CheckImageCount();

                    //loadBatchSetTreeview(_verifyStart, _verifyEnd);
                    loadBatchSetTreeview();
                    if (tvwSet.Nodes.Count > 0)
                    {
                        tvwSet.ExpandAll();
                        if (tvwSet.Nodes[0].Nodes.Count > 0)
                            tvwSet.SelectedNode = tvwSet.Nodes[0].Nodes[0];
                        else
                            tvwSet.SelectedNode = tvwSet.Nodes[0];
                    }
                }                

                _isSaved = false;
                //}
                //else
                //    MessageBox.Show("Document set " + _batchCode + " is not save! \nPlease click save button before proceed to next document set.");
            }
            catch (Exception ex)
            {
            }
            finally
            {
                VerifyingStatusBar.Text = "Verifying";
                btnEnable(true);
            }
        }

        private void tvwSet_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string nodeInfo = "";
            string sep = "";
            try
            {
                if (e.Node.Level == 0)
                    txtInfo.Text = e.Node.Text;
                else
                    txtInfo.Text = "";

                nodeInfo = e.Node.Text.Trim().Substring(0, 4);
                sep = e.Node.Tag.ToString().Trim().Substring(0, 4);

                if (sep.Trim().ToLower() == "sep|")
                {
                    sCurrSetNum = 1.ToString(sSetNumFmt); //First set.
                    if (_batchType.ToLower().Trim() == "set")
                    {
                        VerifyingStatusBar1.Text = "Document Set " + _stcImgInfo.BatchCode + " : " + e.Node.Text.Trim();
                        //txtField.Text = e.Node.Text.Trim();
                    }
                }
                else if (sep.Trim().ToLower() == "sep1")
                {
                    if (_batchType.ToLower().Trim() == "set")
                    {
                        sCurrSetNum = e.Node.Text.Trim();

                        VerifyingStatusBar1.Text = "Document Set " + _stcImgInfo.BatchCode + " : " + e.Node.Text.Trim();

                        //Load the first image for doc set selected.
                        if (e.Node.Nodes.Count > 0)
                        {
                            short sCurrIdx = -1;
                            string rowid = e.Node.Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();

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
                                    _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                    //setImgInfo(sCurrIdx);
                                    //txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                }
                                //else
                                //    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                            }
                        }
                        //End.
                        //txtField.Text = e.Node.Text.Trim();
                    }
                    else if (_batchType.ToLower().Trim() == "batch" || _batchType.ToLower().Trim() == "box") //batch.
                    {
                        sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();

                        VerifyingStatusBar1.Text = "Document Set " + _stcImgInfo.BatchCode + " : " + sCurrSetNum;

                        //Load the first image for doc type for doc set selected.
                        if (e.Node.Nodes.Count > 0)
                        {
                            if (_noSeparator.Trim() == "1")
                            {
                                short sCurrIdx = -1;
                                string rowid = e.Node.Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();

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
                                        _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                        //setImgInfo(sCurrIdx);
                                        //txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                    }
                                    //else
                                    //    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                                }
                            }
                            else
                            {
                                if (e.Node.Nodes[0].Nodes.Count > 0)
                                {
                                    short sCurrIdx = -1;
                                    string rowid = e.Node.Nodes[0].Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();

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
                                            _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                            //setImgInfo(sCurrIdx);
                                            //txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                        }
                                        //else
                                        //    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                                    }
                                }
                            }
                        }
                        //End.
                    }

                    sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                    txtInfo.Text = e.Node.Text;
                    VerifyingStatusBar1.Text = "Document Set " + staMain.stcImgInfoBySet.BatchCode + " : " + sCurrSetNum;
                }
                else if (sep.Trim().ToLower() == "sep2")
                {
                    if (_batchType.ToLower().Trim() == "batch" || _batchType.ToLower().Trim() == "box")
                    {
                        sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();

                        //Load the first image for doc type selected.
                        if (e.Node.Nodes.Count > 0)
                        {
                            short sCurrIdx = -1;
                            string rowid = e.Node.Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();

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
                                    _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                    //setImgInfo(sCurrIdx);
                                    //txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                }
                                //else
                                //    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                            }
                        }
                        //End.

                        sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        txtInfo.Text = "Document Set " + sCurrSetNum + " : " + e.Node.Text;

                        VerifyingStatusBar1.Text = "Document Set " + _stcImgInfo.BatchCode + " : " + sCurrSetNum + " : " + e.Node.Text.Trim();
                        //txtField.Text = e.Node.Text.Trim();
                    }
                }
                else if (nodeInfo.Trim().ToLower() == "page")
                {
                    //sTag = "Doc_" + dr["batchcode"].ToString() + "_" + dr["doctype"].ToString() + "_" + dr["rowid"].ToString();
                    if (e.Node.Tag.ToString().Trim().Split('|').Length > 1)
                    {
                        short sCurrIdx = -1;
                        string rowid = e.Node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                        string docType = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        short i = 0;
                        while (i < _stcImgInfo.ImgIdx.Length)                        
                        {
                            if (rowid == _stcImgInfo.Rowid[i].ToString())
                            {
                                sCurrIdx = (short) _stcImgInfo.ImgIdx[i];
                                break;
                            }

                            i += 1;
                        }

                        if (sCurrIdx != -1)
                        {
                            if (File.Exists(_stcImgInfo.ImgFile[sCurrIdx].ToString()))
                            {
                                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                setImgInfo(sCurrIdx);
                            }
                            else
                                MessageBox.Show(this, "The selected document page does not exists!", "Message");
                        }

                        _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                        setImgInfo(sCurrIdx);                        

                        CheckImageCount();
                        if (_noSeparator.Trim() == "0")
                        {
                            sCurrSetNum = (e.Node.Index + 1).ToString(sSetNumFmt); //Set number = Page number
                        }
                        else
							sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();
                        txtInfo.Text = "Document Set : " + sCurrSetNum + " : " + docType + " : " + txtInfo.Text;

                        VerifyingStatusBar1.Text = "Document Set " + _stcImgInfo.BatchCode + " : " + sCurrSetNum + " : " + docType;
                    }
                    
                }                
            }
            catch (Exception ex)
            {
            }
        }

        private void btnNextSetStrip_Click(object sender, EventArgs e)
        {
            try
            {
                goNextSet(true);
            }
            catch (Exception ex)
            {
            }
        }

        private void btnPrevSetStrip_Click(object sender, EventArgs e)
        {
            try
            {
                goPreviousSet(true);
            }
            catch (Exception ex)
            {
            }
        }

        public long getNextVerifyRowId(string pCurrScanProj, string pAppNum, string pBatchCode, Int64 pCurrRowId)
        {
            long iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 IsNull(rowid,0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode<>'" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus IN ('2','4') ";
                sSQL += "AND rowid>" + pCurrRowId + " ";
                sSQL += "GROUP BY rowid ";
                sSQL += "ORDER BY rowid ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    int i = 0;
                    while (i < drs.Count)
                    {
                        iRet = Convert.ToInt64(drs[i][0].ToString());
                        if (iRet > pCurrRowId)
                            break;

                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public long getPrevVerifyRowId(string pCurrScanProj, string pAppNum, string pBatchCode, Int64 pCurrRowId)
        {
            long iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 IsNull(rowid,0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode<>'" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus IN ('2','4') ";
                sSQL += "AND rowid<" + pCurrRowId + " ";
                sSQL += "GROUP BY rowid ";
                sSQL += "ORDER BY rowid DESC ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    int i = 0;
                    while (i < drs.Count)
                    {
                        iRet = Convert.ToInt64(drs[i][0].ToString());
                        if (iRet < pCurrRowId)
                            break;

                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public void reloadForCurrRow(long pCurrRowId, bool bViewOnly)
        {
            try
            {
                this._oImgCore = new ImageCore();
                //_stcImgInfo = new staMain._stcImgInfo();

                _iCurrBatchRowid = pCurrRowId;

                if (bViewOnly)
                {
                    btnPrevSetStrip.Enabled = false;
                    btnNextSetStrip.Enabled = false;
                }
                else
                {
                    btnPrevSetStrip.Enabled = true;
                    btnNextSetStrip.Enabled = true;
                }

                loadImagesFrmDisk(_iCurrBatchRowid, bViewOnly);
                //loadBatchSetTreeview(DateTime.Now, DateTime.Now);
                loadBatchSetTreeview(bViewOnly);
                if (tvwSet.Nodes.Count > 0)
                {
                    tvwSet.ExpandAll();
                    if (tvwSet.Nodes[0].Nodes.Count > 0)
                        tvwSet.SelectedNode = tvwSet.Nodes[0].Nodes[0];
                    else
                        tvwSet.SelectedNode = tvwSet.Nodes[0];
                }
                setImgInfo(0);

                CheckImageCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reload error!\n" + ex.Message, "Error");
            }
            
        }

        private bool updateBatchScanDB(string pBatchCode, string pModifiedDate)
        {
            bool ret = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "SET modifieddate='" + pModifiedDate.Trim().Replace("'", "") + "' ";   
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                ret = mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                ret = false;
            }

            return ret;
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
                        tempAnnotn.StartPoint = new Point(x, y);
                        tempAnnotn.EndPoint = new Point((tempAnnotn.StartPoint.X + iWidth), (tempAnnotn.StartPoint.Y + iHeight));
                        break;
                    case AnnotationType.enumLine:
                        Point startPoint = tempAnnotn.StartPoint;
                        x = iImgHeight - startPoint.Y;
                        y = startPoint.X;
                        tempAnnotn.StartPoint = new Point(x, y);
                        Point endPoint = tempAnnotn.EndPoint;
                        x = iImgHeight - endPoint.Y;
                        y = endPoint.X;
                        tempAnnotn.EndPoint = new Point(x, y);
                        break;
                }
            }
            _oImgCore.ImageProcesser.RotateRight(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
        }

        private void btnRotateLeftStrip_Click(object sender, EventArgs e)
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
                            tempAnnotn.StartPoint = new Point(x, y);
                            tempAnnotn.EndPoint = new Point((tempAnnotn.StartPoint.X + iWidth), (tempAnnotn.StartPoint.Y + iHeight));
                            break;
                        case AnnotationType.enumLine:
                            Point startPoint = tempAnnotn.StartPoint;
                            x = startPoint.Y;
                            y = iImgWidth - startPoint.X;
                            tempAnnotn.StartPoint = new Point(x, y);
                            Point endPoint = tempAnnotn.EndPoint;
                            x = endPoint.Y;
                            y = iImgWidth - endPoint.X;
                            tempAnnotn.EndPoint = new Point(x, y);
                            break;
                    }
                }
            }
            _oImgCore.ImageBuffer.SetMetaData(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumMetaDataType.enumAnnotation, tempListAnnotn, true);
            _oImgCore.ImageProcesser.RotateLeft(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
            updateImageFile(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
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

        private void btnEnable(bool bEnable)
        {
            btnPrevSetStrip.Enabled = bEnable;
            btnNextSetStrip.Enabled = bEnable;
            btnActualStrip.Enabled = bEnable;
            btnFitStrip.Enabled = bEnable;
            btnZoomInStrip.Enabled = bEnable;
            btnZoomOutStrip.Enabled = bEnable;
            btnRotateLeftStrip.Enabled = bEnable;
            btnRotateRightStrip.Enabled = bEnable;
            btnExpaAllStrip.Enabled = bEnable;
            btnCollAllStrip.Enabled = bEnable;
        }

        private void btnExpaAllStrip_Click(object sender, EventArgs e)
        {
            if (tvwSet.Nodes.Count > 0)
                tvwSet.ExpandAll();
        }

        private void btnCollAllStrip_Click(object sender, EventArgs e)
        {
            if (tvwSet.Nodes.Count > 0)
                tvwSet.CollapseAll();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (_noSeparator.Trim() == "0") //No separator.
                {
                    if (tvwSet.Nodes.Count > 0)
                    {
                        TreeNode oRoot = tvwSet.Nodes[0];
                        if (oRoot.Nodes.Count > 0)
                        {
                            if (tvwSet.SelectedNode == null)
                                tvwSet.SelectedNode = oRoot;

                            if (oRoot.IsSelected)
                                tvwSet.SelectedNode = oRoot.Nodes[0];
                            else if (tvwSet.SelectedNode.Index + 1 < oRoot.Nodes.Count)
                                tvwSet.SelectedNode = oRoot.Nodes[tvwSet.SelectedNode.Index + 1];
                            else
                                tvwSet.SelectedNode = oRoot.Nodes[0];

                            if (tvwSet.SelectedNode != null)
                                sCurrSetNum = (tvwSet.SelectedNode.Index + 1).ToString(sSetNumFmt);

                            tvwSet.Focus();
                        }
                    }
                }
                else if (_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box" ||
                    (_batchType.Trim().ToLower() == "set" && staMain.stcProjCfg.BatchAuto.ToUpper() == "N")) //Navigate set only.
                {
                    if (tvwSet.Nodes.Count > 0)
                    {
                        TreeNode oRoot = tvwSet.Nodes[0];
                        if (oRoot.Nodes.Count > 0)
                        {
                            int i = 0; TreeNode oSet = null; int iIdx = i + 1;

                            if (tvwSet.SelectedNode == null)
                                tvwSet.SelectedNode = oSet = oRoot.Nodes[0];
                            else
                            {
                                while (i < oRoot.Nodes.Count)
                                {
                                    oSet = oRoot.Nodes[i];
                                    if (oSet.IsSelected)
                                    {
                                        if ((i + 1) < oRoot.Nodes.Count)
                                        {
                                            oSet = oRoot.Nodes[i + 1];
                                            tvwSet.SelectedNode = oSet;
                                            break;
                                        }
                                        else
                                        {
                                            oSet = oRoot.Nodes[0];
                                            tvwSet.SelectedNode = oSet;
                                            break;
                                        }
                                    }
                                    else if (tvwSet.SelectedNode.Level != 1)
                                    {
                                        iIdx = oSet.Index + 1;
                                        if (tvwSet.SelectedNode.Level == 2)
                                            iIdx = tvwSet.SelectedNode.Parent.Index + 1;
                                        else if (tvwSet.SelectedNode.Level == 3)
                                            iIdx = tvwSet.SelectedNode.Parent.Parent.Index + 1;

                                        if (iIdx == oRoot.Nodes.Count || tvwSet.SelectedNode.Level == 0)
                                            iIdx = 0;

                                        oSet = oRoot.Nodes[iIdx];
                                        tvwSet.SelectedNode = oSet;
                                        break;
                                    }
                                    i += 1;
                                }
                            }
                            tvwSet.Focus();

                            if (oSet != null)
                                sCurrSetNum = oSet.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                        }
                    }
                }
                else if (_batchType.Trim().ToLower() == "set")
                {
                    if (_isSaved || tvwSet.Nodes.Count == 0)
                    {
                        _oImgCore = new ImageCore();
                        _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                        dsvImg.IfFitWindow = true;
                        //dsvImg.Zoom = 1;

                        dsvImg.Bind(_oImgCore); //Clear.
                        dsvThumbnailList.Bind(_oImgCore);

                        VerifyingStatusBar.Text = "Loading";
                        btnNextSetStrip.Enabled = false;
                        btnPrevSetStrip.Enabled = false;

                        _iCurrBatchRowid = getNextBatchRowId(_currScanProj, _appNum, _iCurrBatchRowid);

                        if (_iCurrBatchRowid == -1 || _iCurrBatchRowid == 0)
                            MessageBox.Show(this, "Batch have reached last batch documents.", "Message");
                        else
                        {
                            initImageAllInfo();
                            tvwSet.Nodes.Clear();

                            loadImagesFrmDisk(_iCurrBatchRowid, true);
                            setImgInfo(0);

                            CheckImageCount();

                            loadBatchSetTreeview(true);
                            if (tvwSet.Nodes.Count > 0)
                                tvwSet.ExpandAll();
                        }
                        _isSaved = false;
                    }
                    else
                        MessageBox.Show("Document set " + _batchCode + " is not save! \nPlease click save button before proceed to next document set.");
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                VerifyingStatusBar.Text = "Verifying";

                btnNextSetStrip.Enabled = true;
                btnPrevSetStrip.Enabled = true;
            }
        }

        private void tvwSet_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                if (tvwSet.SelectedNode.Level == 1)
                {
                    btnNext_Click(sender, e);
                }
            }
        }

        public long getNextBatchRowId(string pCurrScanProj, string pAppNum, Int64 pCurrRowId)
        {
            long iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(rowid,0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus IN ('5','6') ";
                sSQL += "AND rowid>" + pCurrRowId + " ";
                sSQL += "ORDER BY rowid ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    int i = 0;
                    while (i < drs.Count)
                    {
                        iRet = Convert.ToInt64(drs[i][0].ToString());
                        if (iRet > pCurrRowId)
                            break;

                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        private void frmVerifyScan_Activated(object sender, EventArgs e)
        {
            try
            {
                enableScan(false);
                staMain.sessionRestart();
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
                    oToolStrip.Items[0].Enabled = pEnable;
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}
