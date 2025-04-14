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
    public partial class frmVerify : Form
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

        public frmVerify()
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
                staMain.loadIndexConfigDB(_currScanProj, _appNum, _docDefId);
                loadIndexFieldsConfig();

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

        private void frmVerify_Load(object sender, EventArgs e)
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

                if (staMain.stcProjCfg.RescanEnable.Trim().ToUpper() == "Y")
                {
                    RescanToolStripBtn.Visible = true;
                    ReindexToolStripBtn.Visible = true;
                }
                else
                {
                    RescanToolStripBtn.Visible = false;
                    ReindexToolStripBtn.Visible = false;
                }

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
                dsvThumbnailList.SetViewMode(11, 1);

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

                    if (_batchCode != "")
                        loadIndexFieldValue();
                }
                else
                { 
                    setIndexFieldReadOnly(true);
                }

                if (staMain.stcProjCfg.VerifyEnable.Trim().ToUpper() == "Y")
                {
                    verifyStripBtn.Visible = true;
                    toolStripSeparator5.Visible = true;
                    btnSaveStrip.Visible = false; //Edit and save is not allowed for verify process. 04/03/2024.
                    toolStripSeparator3.Visible = false;
                }                    
                else
                {
                    verifyStripBtn.Visible = false;
                    toolStripSeparator5.Visible = false;
                    btnSaveStrip.Visible = false;
                    toolStripSeparator3.Visible = false;
                }

                if (ReindexToolStripBtn.Visible)
                {
                    toolStripSeparator3.Visible = true;
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

        private void frmVerify_Paint(object sender, PaintEventArgs e)
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

        private void frmVerify_Resize(object sender, EventArgs e)
        {
            try
            {
                formResize();

                System.Drawing.Point oPoint = new System.Drawing.Point();
                if (tvwSet.Width + dsvImg.Width + pnlIdxFld.Width + 65 < this.Width)
                {
                    dsvImg.Width = this.Width - tvwSet.Width - pnlIdxFld.Width - 65;
                    //dsvImg.Height = txtInfo.Location.Y - dsvImg.Location.Y - 5;
                    tvwSet.Height = Height - IndexStatusStrip.Height - VerifyToolStrip.Height - dsvThumbnailList.Height - 23;
                    dsvImg.Height = Height - IndexStatusStrip.Height - VerifyToolStrip.Height - dsvThumbnailList.Height - txtInfo.Height;

                    txtInfo.Width = dsvImg.Width - panel1.Width - 5;
                    oPoint.Y = dsvImg.Location.Y + dsvImg.Height + 5;
                    oPoint.X = dsvImg.Location.X;
                    txtInfo.Location = oPoint;
                    //dsvImg.Height = txtInfo.Location.Y - dsvImg.Location.Y - 5;

                    oPoint.Y = dsvImg.Location.Y + dsvImg.Height;
                    oPoint.X = txtInfo.Location.X + txtInfo.Width + 5;
                    panel1.Location = oPoint;
                    oPoint.Y = pnlIdxFld.Location.Y;
                    oPoint.X = dsvImg.Location.X + dsvImg.Width + 5;
                    pnlIdxFld.Location = oPoint;

                    dsvThumbnailList.Width = pnlIdxFld.Location.X + pnlIdxFld.Width - 5;
                    oPoint.Y = tvwSet.Location.Y + tvwSet.Height + 5;
                    oPoint.X = tvwSet.Location.X;
                    dsvThumbnailList.Location = oPoint;

                    //pnlIdxFld.Height = dsvThumbnailList.Location.Y - pnlIdxFld.Location.Y - 5;
                    pnlIdxFld.Height = Height - IndexStatusStrip.Height - VerifyToolStrip.Height - dsvThumbnailList.Height - 23 + panel1.Height;
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

        private void frmVerify_FormClosed(object sender, FormClosedEventArgs e)
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

        private void setIndexFieldReadOnly(bool pEnable)
        {
            try
            {
                int i = 0; TextBox oBox = null;
                while (i < pnlIdxFld.Controls.Count)
                {
                    if (pnlIdxFld.Controls[i] is TextBox)
                    {
                        oBox = ((TextBox)pnlIdxFld.Controls[i]);
                        oBox.ReadOnly = pEnable;
                        if (pEnable) oBox.BackColor = SystemColors.ControlLight; else oBox.BackColor = Color.White;
                    }                       

                    i += 1;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void loadIndexFieldsConfig()
        {
            try
            {
                string[] drs = staMain.stcIndexSetting.FieldName.ToArray();

                if (drs != null)
                {
                    Label oLblFld = null; TextBox oTxtFld = null; Label oLblExist = null;
                    System.Drawing.Point currLoc1, currLoc2, currLoc3;
                    int iDefHeight = txtField.Height; int iDefWidth = txtField.Width; List<int[]> lPosition = new List<int[]>(); //Position list.

                    int i = 0;
                    while (i < drs.Length)
                    {
                        if (i == 0)
                        {
                            lblField.Text = drs[i].ToString();
                            txtField.MaxLength = Convert.ToInt32(staMain.stcIndexSetting.FieldSize[i].ToString());

                            txtField.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                            if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                            {
                                txtField.Enabled = false;
                                txtField.BackColor = SystemColors.ControlLight;
                            }
                            else
                            {
                                txtField.Enabled = true;
                                if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                    txtField.BackColor = Color.LightYellow;
                                else
                                    txtField.BackColor = Color.White;
                            }

                            if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline")
                            {
                                txtField.Multiline = true;
                                txtField.Height = txtField.Height * 3;
                            }

                            lPosition.Add(new int[] { txtField.Location.Y, txtField.Height });

                            lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();
                        }
                        else
                        {
                            //currLoc1 = lblField.Location;
                            //currLoc1.Y = lblField.Location.Y + (30 * i);
                            //currLoc2 = txtField.Location;
                            //currLoc2.Y = txtField.Location.Y + (30 * i);
                            //currLoc3 = lblFieldExist.Location;
                            //currLoc3.Y = lblFieldExist.Location.Y + (30 * i);
                            currLoc1 = lblField.Location;
                            currLoc1.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;
                            currLoc2 = txtField.Location;
                            currLoc2.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;
                            currLoc3 = lblFieldExist.Location;
                            currLoc3.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;

                            oLblFld = new Label();
                            oLblFld.AutoSize = true;
                            oLblFld.Name = "lblField" + (i).ToString();
                            oLblFld.Location = currLoc1;
                            oLblFld.Text = drs[i].ToString().Trim();
                            oLblFld.SendToBack();
                            oLblFld.Visible = true;

                            oTxtFld = new TextBox();
                            oTxtFld.Name = "txtField" + (i).ToString();
                            oTxtFld.Location = currLoc2;
                            oTxtFld.Size = txtField.Size;
                            oTxtFld.BringToFront();

                            if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline")
                            {
                                oTxtFld.Multiline = true;
                                oTxtFld.Height = iDefHeight * 3;
                            }

                            oTxtFld.MaxLength = Convert.ToInt32(staMain.stcIndexSetting.FieldSize[i].ToString());

                            oTxtFld.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                            if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                            {
                                oTxtFld.Enabled = false;
                                oTxtFld.BackColor = SystemColors.ControlLight;
                            }
                            else
                            {
                                oTxtFld.Enabled = true;
                                if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                    oTxtFld.BackColor = Color.LightYellow;
                                else
                                    oTxtFld.BackColor = Color.White;
                            }

                            oLblExist = new Label();
                            oLblExist.Name = "lblFieldExist" + (i).ToString();
                            oLblExist.Location = currLoc3;
                            oLblExist.Visible = false;
                            oLblExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                            lPosition.Add(new int[] { oTxtFld.Location.Y, oTxtFld.Height });

                            pnlIdxFld.Controls.Add(oLblFld);
                            pnlIdxFld.Controls.Add(oTxtFld);
                            pnlIdxFld.Controls.Add(oLblExist);
                        }

                        i += 1;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void reloadForAddOn()
        {
            this._oImgCore = new ImageCore();
            _stcImgInfo = new staMain._stcImgInfo();

            this.frmVerify_Load(this, new EventArgs());
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
                    _stcImgInfo.SetNum[i] = "";
                    _stcImgInfo.Page[i] = "";

                    i += 1;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void loadImagesFrmDisk(long pCurrBatchRowId, bool pViewOnly = false)
        {
            string sBatchStatusIn = "'2','4','6'";
            string sIndexStatusIn = "'I','R','A'";
            try
            {
                if (pViewOnly)
                {
                    sBatchStatusIn = "'5','6'"; //View Verified only.
                    sIndexStatusIn = "'V'";
                }
                //DataRow bdr = oDF.getDocBatchDB(_currScanProj, _appNum, _batchType, sBatchStatusIn, _station, _userId, pCurrBatchRowId);
                DataRow bdr = oDF.getProcessDocBatchDB(_currScanProj, _appNum, _batchType, sBatchStatusIn, _station, "", pCurrBatchRowId);
                initImgInfoCollection(1);

                if (bdr != null)
                {
                    _iCurrBatchRowid = Convert.ToInt32(bdr["rowid"].ToString());

                    if (bdr["batchstatus"].ToString().ToUpper() == "5")
                    {
                        verifyStripBtn.Enabled = false;
                        RescanToolStripBtn.Enabled = false;
                        ReindexToolStripBtn.Enabled = false;
                        btnSaveStrip.Enabled = true;
                    }
                    else
                    {
                        verifyStripBtn.Enabled = true;
                        RescanToolStripBtn.Enabled = true;
                        ReindexToolStripBtn.Enabled = true;
                        btnSaveStrip.Enabled = true;
                    }

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

                    DataRowCollection drs = oDF.getIndexedDocFileByBatchCodeDB(_currScanProj, _appNum, _batchCode, sIndexStatusIn);

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
                            //txtField.Text = drs[0]["doctype"].ToString().Trim();
                            //txtField1.Text = drs[0]["cappnum"].ToString().Trim();
                            //txtField2.Text = drs[0]["choldername"].ToString().Trim();
                            //lblField1Exist.Text = drs[0]["cappnum"].ToString().Trim();
                            //lblField2Exist.Text = drs[0]["choldername"].ToString().Trim();
                            sCurrSetNum = _stcImgInfo.SetNum[0].ToString();
                            loadIndexFieldValue();
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

        private void loadIndexFieldValue(string pSetNum = "", string pPageId = "")
        {
            try
            {
                DataRowCollection drs = oDF.getIndexedFieldValueByBatchCodeDB(_currScanProj, _appNum, _batchCode, pSetNum, _docDefId, pPageId);

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        int iCnt = staMain.stcIndexSetting.RowId.Count;
                        int i = 0; Control oCtrl = null; Label oLblExist = null; string sCtrlType = "";
                        while (i < iCnt)
                        {
                            if (i == 0)
                            {
                                oCtrl = pnlIdxFld.Controls["txtField"];
                                oLblExist = (Label)pnlIdxFld.Controls["lblFieldExist"];
                            }
                            else
                            {
                                oCtrl = pnlIdxFld.Controls["txtField" + i];
                                oLblExist = (Label)pnlIdxFld.Controls["lblFieldExist" + i];
                            }

                            if (oCtrl == null || oCtrl.Visible == false)
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlIdxFld.Controls["dtpField"];
                                }
                                else
                                {
                                    oCtrl = pnlIdxFld.Controls["dtpField" + i];
                                }
                            }

                            if (oCtrl == null || oCtrl.Visible == false)
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlIdxFld.Controls["cbxField"];
                                }
                                else
                                {
                                    oCtrl = pnlIdxFld.Controls["cbxField" + i];
                                }
                                sCtrlType = "ComboBox";
                            }

                            if (oLblExist != null) oLblExist.Visible = false;

                            if (oCtrl != null && oLblExist != null && i < drs.Count)
                            {
                                oCtrl.Text = ""; //Init.
                                oLblExist.Text = ""; //Init.                                

                                int j = 0;
                                while (j < drs.Count)
                                {
                                    if (j < iCnt)
                                    {
                                        if (drs[j][1].ToString().Trim() == staMain.stcIndexSetting.RowId[i].ToString().Trim())
                                        {
                                            if (staMain.stcIndexSetting.FieldDataType[i].ToLower() == "valuelist")
                                            {
                                                if (staMain.stcIndexSetting.ValueListType[i].ToLower() == "data source")
                                                {
                                                    oCtrl.Text = drs[j][0].ToString().Trim();
                                                    oLblExist.Text = drs[j][0].ToString().Trim();
                                                }
                                                else
                                                {
                                                    string sValue = oDF.getIndexedFieldValueListByValueDB(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, staMain.stcIndexSetting.FieldName[i], drs[j][0].ToString().Trim());
                                                    oCtrl.Text = sValue;
                                                    oLblExist.Text = sValue;
                                                }
                                            }
                                            else
                                            {
                                                oCtrl.Text = drs[j][0].ToString().Trim();
                                                oLblExist.Text = drs[j][0].ToString().Trim();
                                            }                                            
                                            break;
                                        }
                                    }
                                    
                                    j += 1;
                                }
                            }

                            i += 1;
                        }
                    }
                    else
                    {
                        int iCnt = staMain.stcIndexSetting.FieldName.Count;
                        int i = 0; Control oCtrl = null; Label oLblExist = null;
                        while (i < iCnt)
                        {
                            if (i == 0)
                            {
                                oCtrl = pnlIdxFld.Controls["txtField"];
                                oLblExist = (Label)pnlIdxFld.Controls["lblFieldExist"];
                            }
                            else
                            {
                                oCtrl = pnlIdxFld.Controls["txtField" + i];
                                oLblExist = (Label)pnlIdxFld.Controls["lblFieldExist" + i];
                            }

                            if (oCtrl == null)
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlIdxFld.Controls["dtpField"];
                                }
                                else
                                {
                                    oCtrl = pnlIdxFld.Controls["dtpField" + i];
                                }
                            }

                            if (oLblExist != null) oLblExist.Visible = false;

                            if (oCtrl != null && oLblExist != null)
                            {
                                oCtrl.Text = ""; //Init.
                                oLblExist.Text = ""; //Init.                                
                            }

                            i += 1;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void setImgInfo(short sImageIndex)
        {
            if (_stcImgInfo.ImgIdx.Length > 0 && _stcImgInfo.ImgIdx.Length > sImageIndex)
            {
                if (_stcImgInfo.BatchCode != null)
                {
                    _stcImgInfo.CurrImgIdx = sImageIndex;
                    //txtField.Text = _stcImgInfo.DocType[sImageIndex].Trim();
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

        private void loadBatchSetTreeview(bool pViewOnly = false)
        {
            string sBatchStatusIn = "'2','4','6'";
            string sIndexStatusIn = "'I','R','A'";

            if (pViewOnly) //View Verify only.
            {
                sBatchStatusIn = "'5','6'";
                sIndexStatusIn = "'V'";
            }

            staMain._ScanProj = _currScanProj;
            staMain._AppNum = _appNum;

            if (_batchType.Trim().ToLower() == "set")
            {
                if (_noSeparator.Trim() == "0")
                {
                    staMain.loadIndexSetTreeviewNoSep(this.tvwSet, _batchType, _batchCode, sBatchStatusIn, sIndexStatusIn);
                }
                else
                    staMain.loadIndexSetTreeview(this.tvwSet, _batchType, _batchCode, sBatchStatusIn, sIndexStatusIn);
            }
            else //Batch process type.
            {
				if (_noSeparator.Trim() == "0")
                    staMain.loadIndexBatchTreeviewNoSep(this.tvwSet, _batchType, _batchCode, sBatchStatusIn, sIndexStatusIn);
                else
                    staMain.loadIndexBatchTreeview(this.tvwSet, _batchType, _batchCode, sBatchStatusIn, sIndexStatusIn);
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

        private string validateFields()
        {
            string strMsg = "";

            int iCnt = staMain.stcIndexSetting.FieldName.Count;
            int i = 0; Label oLbl = null; TextBox oTxt = null;
            while (i < iCnt)
            {
                if (i == 0)
                {
                    oLbl = (Label)pnlIdxFld.Controls["lblField"];
                    oTxt = (TextBox)pnlIdxFld.Controls["txtField"];
                }
                else
                {
                    oLbl = (Label)pnlIdxFld.Controls["lblField" + i];
                    oTxt = (TextBox)pnlIdxFld.Controls["txtField" + i];
                }

                if (oLbl != null && oTxt != null)
                {
                    if (staMain.stcIndexSetting.FieldMand[i].ToString().Trim() == "Y" && oTxt.Text.Trim() == "")
                    {
                        strMsg = oLbl.Text + " is required!";
                        break;
                    }

                    if (staMain.stcIndexSetting.FieldDataType[i].ToString().Trim().ToLower() == "integer" && oTxt.Text.Trim() != "")
                    {
                        if (!mGlobal.IsInteger(oTxt.Text.Trim()))
                        {
                            strMsg = oLbl.Text + " must be numbers!";
                            break;
                        }
                    }
                }

                i += 1;
            }

            return strMsg;
        }

        private bool checkDocuSetIndexExist(string pScanProj, string pAppNum, string pBatchCode, string pDocType, string pStatusIn, string pFilename)
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus IN (" + pStatusIn.Trim() + ") ";

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

        private bool checkIndexFieldValueChange()
        {
            bool bRet = false;

            try
            {
                int i = 0; TextBox oTxtBox = null; Label oLbl = null;
                while (i < staMain.stcIndexSetting.RowId.Count)
                {
                    if (i == 0)
                    {
                        oTxtBox = (TextBox)pnlIdxFld.Controls["txtField"];
                        oLbl = (Label)pnlIdxFld.Controls["lblFieldExist"];
                    }
                    else
                    {
                        oTxtBox = (TextBox)pnlIdxFld.Controls["txtField" + i];
                        oLbl = (Label)pnlIdxFld.Controls["lblFieldExist" + i];
                    }

                    if (oTxtBox != null && oLbl != null)
                    {
                        if (oTxtBox.Text.Trim() != oLbl.Text.Trim())
                        {
                            bRet = true;
                            break;
                        }
                    }

                    i += 1;
                }
            }
            catch (Exception ex)
            {
                bRet = true;
            }

            return bRet;
        }

        private bool updateDocIndexDB(string pNewStatus, string pModifiedDate = "")
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            try
            {
                mGlobal.LoadAppDBCfg();

                bExist = checkDocuSetIndexExist(_currScanProj, _appNum, _batchCode, _docType, "'I','R','V'", _filename);

                if (bExist)
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "SET [indexstatus]='" + pNewStatus + "'";
                    sSQL += ",[verifystation]='" + MDIMain.strStation.Trim().Replace("'", "") + "'";
                    sSQL += ",[verifyuser]='" + MDIMain.strUserID.Trim().Replace("'", "") + "'";
                    sSQL += ",[verifystart]='" + _verifyStart.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    sSQL += ",[verifyend]='" + _verifyEnd.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    sSQL += ",[remarks]='" + txtRemarks.Text.Trim().Replace("'","''") + "' ";

                    if (pModifiedDate.Trim() != "")
                        sSQL += ",modifieddate='" + pModifiedDate.Trim().Replace("'", "''") + "' ";

                    sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND indexstatus IN ('I','R') ";

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                        throw new Exception("Update document index data failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool saveDocIndexFieldDB(int pPageId, string pModifiedDate = "")
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            int i = 0;
            TextBox oTxtBox = null;
            try
            {
                mGlobal.LoadAppDBCfg();

                bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, _setNum, _docDefId);

                if (bExist)
                {
                    int iId = 0;
                    while (i < staMain.stcIndexSetting.RowId.Count)
                    {
                        iId = staMain.stcIndexSetting.RowId[i];

                        if (i == 0)
                            oTxtBox = (TextBox)pnlIdxFld.Controls["txtField"];
                        else
                            oTxtBox = (TextBox)pnlIdxFld.Controls["txtField" + i];

                        sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                        //sSQL += "SET [fldvalue]=N'" + oTxtBox.Text.Trim() + "' "; //2023/12/28 Since at Verify stage, edit field value is not allowed thus update not needed. Bug fixed for index field number i is incorrect.
                        //sSQL += ",pageid='" + pPageId + "' ";
                        sSQL += "SET pageid='" + pPageId + "' ";

                        if (pModifiedDate.Trim() != "")
                            sSQL += ",modifieddate='" + pModifiedDate.Trim().Replace("'", "''") + "' ";

                        sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                        sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND setnum='" + _setNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                        sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                        sSQL += "AND fieldid=" + iId + " ";

                        if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                        {
                            bSaved = false;
                            throw new Exception("Update document index data failed!");
                        }

                        i += 1;
                    }

                }
                else
                {
                    int iId = 0;
                    while (i < staMain.stcIndexSetting.RowId.Count)
                    {
                        iId = staMain.stcIndexSetting.RowId[i];

                        if (i == 0)
                            oTxtBox = (TextBox)pnlIdxFld.Controls["txtField"];
                        else
                            oTxtBox = (TextBox)pnlIdxFld.Controls["txtField" + i];

                        sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                        sSQL += "([scanpjcode]";
                        sSQL += ",[sysappnum]";
                        sSQL += ",[docdefid]";
                        sSQL += ",[setnum]";
                        sSQL += ",[batchcode]";
                        sSQL += ",[fieldid]";
                        sSQL += ",[fldvalue]";
                        sSQL += ",[pageid]";
                        sSQL += ",[modifieddate]";
                        sSQL += ") VALUES (";
                        sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _setNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                        sSQL += "," + iId + "";
                        sSQL += ",N'" + oTxtBox.Text.Trim().Replace("'", "") + "'";
                        sSQL += "," + pPageId + "";
                        sSQL += ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                        if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                        {
                            bSaved = false;
                            throw new Exception("Saving document index data failed!");
                        }

                        i += 1;
                    }
                }

            }
            catch (Exception ex)
            {
                bSaved = false;
                mGlobal.Write2Log(ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
                //throw new Exception(ex.Message);                
            }

            return bSaved;
        }

        private bool updateDocIndexFieldDB(string pModifiedDate, int pPageId)
        {
            bool bSaved = true;
            string sSQL = "";
            int i = 0;
            TextBox oTxtBox = null;
            try
            {
                mGlobal.LoadAppDBCfg();

                bool bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, sCurrSetNum, _docDefId);

                if (bExist)
                {
                    int iId = 0;
                    while (i < staMain.stcIndexSetting.RowId.Count)
                    {
                        iId = staMain.stcIndexSetting.RowId[i];

                        if (i == 0)
                            oTxtBox = (TextBox)pnlIdxFld.Controls["txtField"];
                        else
                            oTxtBox = (TextBox)pnlIdxFld.Controls["txtField" + i];

                        sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                        //sSQL += "SET [fldvalue]=N'" + oTxtBox.Text.Trim() + "' "; //2023/12/28 Since at Verify stage, edit field value is not allowed thus update not needed. Bug fixed for index field number i is incorrect.
                        sSQL += "SET pageid='" + pPageId + "' ";
                        sSQL += ",modifieddate='" + pModifiedDate.Trim().Replace("'", "''") + "' ";
                        sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                        sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND setnum='" + sCurrSetNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                        sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                        sSQL += "AND fieldid=" + iId + " ";

                        bSaved = mGlobal.objDB.UpdateRows(sSQL, true);

                        i += 1;
                    }
                }
                else
                {
                    int iId = 0;
                    while (i < staMain.stcIndexSetting.RowId.Count)
                    {
                        iId = staMain.stcIndexSetting.RowId[i];

                        if (i == 0)
                            oTxtBox = (TextBox)pnlIdxFld.Controls["txtField"];
                        else
                            oTxtBox = (TextBox)pnlIdxFld.Controls["txtField" + i];

                        sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                        sSQL += "([scanpjcode]";
                        sSQL += ",[sysappnum]";
                        sSQL += ",[docdefid]";
                        sSQL += ",[setnum]";
                        sSQL += ",[batchcode]";
                        sSQL += ",[fieldid]";
                        sSQL += ",[fldvalue]";
                        sSQL += ",[pageid]";
                        sSQL += ") VALUES (";
                        sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + sCurrSetNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                        sSQL += "," + iId + "";
                        sSQL += ",N'" + oTxtBox.Text.Trim().Replace("'", "") + "'";
                        sSQL += "," + pPageId + ")";

                        bSaved = mGlobal.objDB.UpdateRows(sSQL, true);

                        i += 1;
                    }
                }                      

            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
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

        private void btnApprove_Click(object sender, EventArgs e)
        {
            string strMsg = "";
            try
            {
                btnPrevSetStrip.Enabled = false;
                btnNextSetStrip.Enabled = false;

                _isSaved = false;
                strMsg = validateFields();

                if (strMsg.Trim() == "")
                {
                    if (sCurrSetNum.Trim() == "")
                        strMsg = "The document set number is blank! Please select a document set.";
                }                    

                if (strMsg.Trim() == "")
                {
                    verifyStripBtn.Enabled = false;
                    RescanToolStripBtn.Enabled = false;
                    ReindexToolStripBtn.Enabled = false;

                    _batchCode = _stcImgInfo.BatchCode;
                    _setNum = sCurrSetNum;                  
                    
                    _verifyEnd = DateTime.Now;

                    VerifyingStatusBar.Text = "Verifying";

                    string dModified = "";
                    if (checkIndexFieldValueChange())
                        dModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (updateDocIndexDB("V", dModified))
                    {
                        bool bOK = true;

                        string sSetNum = _setNum;
                        int i = 0, iPgId = 0;
                        while (i < _stcImgInfo.Rowid.Length)
                        {
                            if (_noSeparator.Trim() == "0")
                                _setNum = (i + 1).ToString(sSetNumFmt);
                            else
                                _setNum = _stcImgInfo.SetNum[i];

                            iPgId = _stcImgInfo.Rowid[i];
                            saveDocIndexFieldDB(iPgId, dModified);

                            i += 1;
                        }
                        _setNum = sSetNum;

                        if (bOK)
                        {                            
                            bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "2", "5", "", "", "", "", "", "", _userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            //for status exported.
                            bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "6", "5", "", "", "", "", "", "", _userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            if (bOK)
                            {
                                _isSaved = true;
                                MessageBox.Show("Document Set " + _batchCode + " verified successfully!", "Verify Indexed Document");

                                goNextSet(false);
                            }
                        }                      
                    }
                    else
                        MessageBox.Show("Document Set " + _batchCode + " verify unsuccessful!", "Verify Indexed Document");
                                        
                }
                else
                    MessageBox.Show(strMsg, "Validation Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
            finally
            {
                VerifyingStatusBar.Text = "Verifying";
                verifyStripBtn.Enabled = true;
                RescanToolStripBtn.Enabled = true;
                ReindexToolStripBtn.Enabled = true;

                btnPrevSetStrip.Enabled = true;
                btnNextSetStrip.Enabled = true;
            }
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            string strMsg = "";

            try
            {
                btnPrevSetStrip.Enabled = false;
                btnNextSetStrip.Enabled = false;

                _isSaved = false;
                strMsg = ""; //validateFields();
                //if (strMsg.Trim() == "")
                //{
                //    if (sCurrSetNum.Trim() == "")
                //        strMsg = "The document set number is blank! Please select a document set.";
                //}

                if (strMsg.Trim() == "")
                {
                    frmRemarks oRemarks = new frmRemarks();
                    oRemarks.bEdit = true;
                    oRemarks.sRemarks = txtRemarks.Text.Trim();
                    oRemarks.ShowDialog(this);
                    txtRemarks.Text = oRemarks.sRemarks.Trim();
                    oRemarks.Dispose();

                    verifyStripBtn.Enabled = false;
                    RescanToolStripBtn.Enabled = false;
                    ReindexToolStripBtn.Enabled = false;

                    _setNum = sCurrSetNum;
                    _batchCode = _stcImgInfo.BatchCode;
                    _verifyEnd = DateTime.Now;

                    VerifyingStatusBar.Text = "Reverting";

                    if (updateDocIndexDB("R"))
                    {
                        string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        string sSetNum = sCurrSetNum;
                        int i = 0, iPgId = 0;
                        while (i < _stcImgInfo.Rowid.Length)
                        {
                            if (_noSeparator.Trim() == "0")
                                sCurrSetNum = (i + 1).ToString(sSetNumFmt);
                            else
                                sCurrSetNum = _stcImgInfo.SetNum[i];

                            iPgId = _stcImgInfo.Rowid[i];
                            updateDocIndexFieldDB(sModifiedDate, iPgId);

                            i += 1;
                        }
                        sCurrSetNum = sSetNum;

                        //Document verified still can revert.                        
                        bool bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "2", "4", "", "", txtRemarks.Text.Trim(), "Verify");
                        //for status exported.
                        oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "6", "4", "", "", txtRemarks.Text.Trim(), "Verify");

                        if (bOK)
                        {
                            _isSaved = true;
                            MessageBox.Show("Document Set " + _batchCode + " reverted successfully!", "Revert Indexed Document");

                            this.Close();
                            //goNextSet(false);
                        }                       
                    }
                    else
                        MessageBox.Show("Document Set " + _batchCode + " revert unsuccessful!", "Revert Indexed Document");

                }
                else
                    MessageBox.Show(strMsg, "Validation Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
            finally
            {
                VerifyingStatusBar.Text = "Verifying";
                verifyStripBtn.Enabled = true;
                RescanToolStripBtn.Enabled = true;
                ReindexToolStripBtn.Enabled = true;

                btnPrevSetStrip.Enabled = true;
                btnNextSetStrip.Enabled = true;
            }
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
                    verifyStripBtn.Enabled = false;
                    RescanToolStripBtn.Enabled = false;
                    ReindexToolStripBtn.Enabled = false;
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
					
                    loadIndexFieldsConfig();
                    loadIndexFieldValue();
					
					if (bSingleView)
                        setIndexFieldReadOnly(true);			
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
                verifyStripBtn.Enabled = true;
                RescanToolStripBtn.Enabled = true;
                ReindexToolStripBtn.Enabled = true;
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
                    verifyStripBtn.Enabled = false;
                    RescanToolStripBtn.Enabled = false;
                    ReindexToolStripBtn.Enabled = false;
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

                    loadIndexFieldsConfig();
                    loadIndexFieldValue();

                    if (bSingleView)
                        setIndexFieldReadOnly(true);
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
                verifyStripBtn.Enabled = true;
                RescanToolStripBtn.Enabled = true;
                ReindexToolStripBtn.Enabled = true;
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
                    loadIndexFieldValue(sCurrSetNum);
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

                        loadIndexFieldValue(sCurrSetNum);
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

                        if (_noSeparator.Trim() == "0")
                        {
                            loadIndexFieldValue(sCurrSetNum, rowid);
                        }
                        else
                        {
                            loadIndexFieldValue(sCurrSetNum);
                        }
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

        private void btnSaveStrip_Click(object sender, EventArgs e)
        {
            string strMsg = "";

            try
            {
                _isSaved = false;
                strMsg = validateFields();

                if (strMsg.Trim() == "")
                {
                    bool bOK = true;                   

                    btnSaveStrip.Enabled = false;
                    verifyStripBtn.Enabled = false;
                    RescanToolStripBtn.Enabled = false;
                    ReindexToolStripBtn.Enabled = false;

                    VerifyingStatusBar.Text = "Saving";

                    string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    string sSetNum = sCurrSetNum;
                    int i = 0, iPgId = 0;
                    while (i < _stcImgInfo.Rowid.Length)
                    {
                        if (_noSeparator.Trim() == "0")
                            sCurrSetNum = (i + 1).ToString(sSetNumFmt);
                        else
                            sCurrSetNum = _stcImgInfo.SetNum[i];

                        iPgId = _stcImgInfo.Rowid[i];
                        updateDocIndexFieldDB(sModifiedDate, iPgId);

                        i += 1;
                    }
                    sCurrSetNum = sSetNum;

                    if (i == 0) bOK = false;

                    if (bOK)
                    {                       
                        bOK = updateBatchScanDB(_batchCode, sModifiedDate);

                        if (bOK)
                        {
                            _isSaved = true;
                            MessageBox.Show("Document Set Indexes " + _batchCode + " saved successfully!", "Save Verified Document");
                        }
                        else
                            MessageBox.Show("Document Set Indexes " + _batchCode + " save unsuccessful!", "Save Verified Document");

                    }
                    else
                        MessageBox.Show("Document Set Indexes " + _batchCode + " save unsuccessful!", "Save Verified Document");

                }
                else
                    MessageBox.Show(strMsg, "Validation Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
            finally
            {
                VerifyingStatusBar.Text = "Verifying";
                btnSaveStrip.Enabled = true;
                verifyStripBtn.Enabled = false;
                RescanToolStripBtn.Enabled = true;
                ReindexToolStripBtn.Enabled = true;
            }
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

        private void btnReindex_Click(object sender, EventArgs e)
        {
            string strMsg = "";

            try
            {
                btnPrevSetStrip.Enabled = false;
                btnNextSetStrip.Enabled = false;

                _isSaved = false;
                strMsg = ""; //validateFields();
                //if (strMsg.Trim() == "")
                //{
                //    if (sCurrSetNum.Trim() == "")
                //        strMsg = "The document set number is blank! Please select a document set.";
                //}

                if (strMsg.Trim() == "")
                {
                    frmRemarks oRemarks = new frmRemarks();
                    oRemarks.bEdit = true;
                    oRemarks.sRemarks = txtRemarks.Text.Trim();
                    oRemarks.ShowDialog(this);
                    txtRemarks.Text = oRemarks.sRemarks.Trim();
                    oRemarks.Dispose();

                    verifyStripBtn.Enabled = false;
                    RescanToolStripBtn.Enabled = false;
                    ReindexToolStripBtn.Enabled = false;

                    _setNum = sCurrSetNum;
                    _batchCode = _stcImgInfo.BatchCode;
                    _verifyEnd = DateTime.Now;

                    VerifyingStatusBar.Text = "Reindexing";

                    if (updateDocIndexDB("G"))
                    {
                        string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        string sSetNum = sCurrSetNum;
                        int i = 0, iPgId = 0;
                        while (i < _stcImgInfo.Rowid.Length)
                        {
                            if (_noSeparator.Trim() == "0")
                                sCurrSetNum = (i + 1).ToString(sSetNumFmt);
                            else
                                sCurrSetNum = _stcImgInfo.SetNum[i];

                            iPgId = _stcImgInfo.Rowid[i];
                            updateDocIndexFieldDB(sModifiedDate, iPgId);

                            i += 1;
                        }
                        sCurrSetNum = sSetNum;

                        //Document verified cannot revert.                        
                        bool bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "2", "8", "", "", txtRemarks.Text.Trim(), "Verify");
                        //for status exported.
                        oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "6", "8", "", "", txtRemarks.Text.Trim(), "Verify");

                        if (bOK)
                        {
                            _isSaved = true;
                            MessageBox.Show("Document Set " + _batchCode + " reindex request sent successfully!", "Reindex Indexed Document");

                            this.Close();
                            //goNextSet(false);
                        }
                    }
                    else
                        MessageBox.Show("Document Set " + _batchCode + " reindex request sent unsuccessful!", "Reindex Indexed Document");

                }
                else
                    MessageBox.Show(strMsg, "Validation Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
            finally
            {
                VerifyingStatusBar.Text = "Verifying";
                verifyStripBtn.Enabled = true;
                RescanToolStripBtn.Enabled = true;
                ReindexToolStripBtn.Enabled = true;

                btnPrevSetStrip.Enabled = true;
                btnNextSetStrip.Enabled = true;
            }
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

                            txtField.Focus(); //First index field set focus.
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

                            txtField.Focus(); //First index field set focus.
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
                        btnSaveStrip.Enabled = false;
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

                btnSaveStrip.Enabled = true;
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

        private void RescanToolStripBtn_Click(object sender, EventArgs e)
        {
            try
            {
                btnReject_Click(sender, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void ReindexToolStripBtn_Click(object sender, EventArgs e)
        {
            try
            {
                btnReindex_Click(sender, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void verifyStripBtn_Click(object sender, EventArgs e)
        {
            try
            {
                btnApprove_Click(sender, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void frmVerify_Activated(object sender, EventArgs e)
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
