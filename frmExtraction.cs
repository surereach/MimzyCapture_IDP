using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

//using Dynamsoft;
//using Dynamsoft.DBR;
//using Dynamsoft.TWAIN;
using Dynamsoft.TWAIN.Interface;
using Dynamsoft.Core;
//using Dynamsoft.PDF;
using Dynamsoft.Core.Enums;
using Dynamsoft.Core.Annotation;

using IronOcr;
using IronSoftware.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using Dynamsoft.TWAIN.Enums;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Collections;
using Grpc.Net.Client.Balancer;
using System.Data.Odbc;
using System.Runtime.ConstrainedExecution;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Collections;
using WinRT.Interop;

namespace SRDocScanIDP
{
    public partial class frmExtraction : Form, IAcquireCallback
    {
        private ImageCore _oImgCore;

        private string _currScanProj;
        private string _appNum;
        private string _batchType;
        public string _batchCode;
        private string _collaNum;
        private string _docType;
        private string _setNum;
        private string _filename;
        private int _totPage;
        private string _docDefId;
        private string _boxNum;
        private string sSetNumFmt;
        private string _noSeparator;

        private string _station;
        private string _userId;

        private clsDocuFile oDF = new clsDocuFile();

        private int _currentImageIndex = -1;
        private bool _isSaved;
        private DateTime _indexStart;
        private DateTime _indexEnd;

        private long _iCurrBatchRowid;
        private List<long> _lIndexedRowid;
        private int _prevCnt;

        private string sImgInfoCopy;
        private bool bImgCut;
        private ImageCore oImgCut;
        private string sDrag;

        private int iCurrImgSeq;
        private string sCurrSetNum;
        private string sCurrDocType;

        public bool bSingleView;

        private string sErrMsg;

        private delegate void CrossThreadOperationControl();
        private delegate void CrossThreadOperationLoadImg();

        Dictionary<string, int> dSelArea; //= new Dictionary<string, int>();
        Dictionary<string, string> dOCRPrevValues; //= new Dictionary<string, string>();
        private int _indexFocusId;

        private TreeNode selectedNode;

        private bool mbCustomScan = false;
        private bool bIsDuplexEnabled;
        private string sCustSrc;
        private string sCustPixelType;
        private int iCustResolutn;

        private bool bDocAutoOCR;

        private float fltBrightness;
        private float fltContrast;
        private byte[] oImgBytes;

        private string sFrontBack;
        private string _fromProc;
        private string _batchStatus;

        private const int FORM_MIN_WIDTH = 1280;
        private const int FORM_MIN_HEIGHT = 720;
        private int iFormOffset = 65;
        List<int[]> lPosition; //Position list.

        //private AnalysisResult? _result;
        private dynamic _result;
        private DateTime _sendStart;
        private DateTime _sendEnd;
        private int _pageId;
        private int _tableNum;
        private bool _tableEdit;
        private int _selRow;
        private int _selCol;
        private int _keyFocusId;
        private int _fldCnt;
        private int _keyFldCnt;
        private const int CHKBOX_MAX_WIDTH = 417;

        public frmExtraction()
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
            dsvImg.FitWindowType = Dynamsoft.Forms.Enums.EnumFitWindowType.enumFitWindowWidth;

            dsvThumbnailList.MouseShape = true;
            dsvThumbnailList.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;

            _currScanProj = MDIMain.strScanProject; //this.MdiParent.Controls["toolStrip"].Controls[0].Text;
            _appNum = MDIMain.intAppNum.ToString("000");
            _docDefId = staMain.stcProjCfg.DocDefId;
            _noSeparator = staMain.stcProjCfg.NoSeparator;

            if (_noSeparator.Trim() == "1")
                mnuInsDocSepStrip.Visible = true;
            else
                mnuInsDocSepStrip.Visible = false;

            ImageCore oImgCut = new ImageCore();
            oImgCut.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;
            lPosition = new List<int[]>();

            try
            {
                clsIDP.loadIDPMappingDB(_currScanProj, _appNum, _docDefId);
                staMain.loadIndexConfigDB(_currScanProj, _appNum, _docDefId);
                loadIndexFieldsConfig();

                bSingleView = false;

                selectedNode = null;
                bDocAutoOCR = false;

                sFrontBack = "F";
                Application.AddMessageFilter(new clsMouseKbdFilterMess(this));
                staMain.sessionRestart();
            }
            catch (Exception ex)
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

        private void frmExtraction_Load(object sender, EventArgs e)
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

                if (staMain.stcProjCfg.RescanEnable.Trim().ToUpper() == "Y")
                {
                    btnRescanToolStrip.Visible = true;
                }
                else
                {
                    btnRescanToolStrip.Visible = false;
                }

                _currScanProj = MDIMain.strScanProject; //this.MdiParent.Controls["toolStrip"].Controls[0].Text;
                _appNum = MDIMain.intAppNum.ToString("000");
                _setNum = "";
                _collaNum = "";
                _batchType = MDIMain.sBatchType;
                _batchCode = "";
                _docType = "";
                _filename = "";
                _indexStart = DateTime.Now;
                _indexEnd = DateTime.Now;
                _docDefId = staMain.stcProjCfg.DocDefId;
                _boxNum = "0";

                if (_batchType.Trim().ToLower() == "set")
                    sSetNumFmt = "000000";
                else
                    sSetNumFmt = "000";

                _totPage = 0; //Init.
                _isSaved = false;

                _station = MDIMain.strStation;
                _userId = MDIMain.strUserID;

                _lIndexedRowid = new List<long>();
                _prevCnt = 0;

                sImgInfoCopy = "";
                bImgCut = false;
                oImgCut = null;
                iCurrImgSeq = 1;
                sCurrSetNum = "";

                sErrMsg = "";
                _fromProc = "";
                _batchStatus = "";

                _result = null;
                _sendStart = DateTime.Now;
                _sendEnd = DateTime.Now;
                _pageId = 0;
                _tableNum = 0;
                _tableEdit = false;
                _selRow = -1;
                _selCol = -1;
                _keyFocusId = 0;
                _fldCnt = 0;
                _keyFldCnt = 0;
                tabFields.AutoScroll = true;
                tabKeyValues.AutoScroll = true;

                IndexingStatusBar.Text = "Indexing"; //1 - Scan, 2 - Indexing, 3 - Deleted, 4 - Reject, 5 - Verify, 6 - Export, 7 - Transfer, 8 - Reindex.

                dsvImg.SetViewMode(-1, -1);

                _indexFocusId = 0;
                btnZoneStrip_Click(sender, e); //set region OCR as default.

                dsvThumbnailList.IfFitWindow = true;
                dsvThumbnailList.SetViewMode(11, 1);

                if (bSingleView == false)
                {
                    CrossThreadOperationLoadImg oLoadImgOpr = delegate ()
                    {
                        loadImagesFrmDisk(_iCurrBatchRowid, "'1','8','6'", "'S','R','A','G'");

                        setImgInfo(0);

                        CheckImageCount();
                    };
                    Invoke(oLoadImgOpr);

                    loadBatchSetTreeview("'1','8','6'", "'S','R','A','G'");
                    if (tvwSet.Nodes.Count > 0)
                    {
                        tvwSet.ExpandAll();
                        if (tvwSet.Nodes[0].Nodes.Count > 0)
                            tvwSet.SelectedNode = tvwSet.Nodes[0].Nodes[0];
                        else
                            tvwSet.SelectedNode = tvwSet.Nodes[0];
                    }

                    if (_fromProc.Trim().ToLower() == "verify")
                        syncPageIndexFieldDB(_batchCode, "TDocuIndex");
                    else
                        syncPageIndexFieldDB(_batchCode, "TDocuScan");

                }
                //if (_batchCode != "")
                //    loadIndexFieldValue();
                sDrag = string.Empty;
                tvwSet.AllowDrop = true;
                tvwSet.Dock = DockStyle.None;

                tvwSet.ItemDrag += new ItemDragEventHandler(tvwSet_ItemDrag);
                tvwSet.DragEnter += new DragEventHandler(tvwSet_DragEnter);
                tvwSet.DragOver += new DragEventHandler(tvwSet_DragOver);
                tvwSet.DragDrop += new DragEventHandler(tvwSet_DragDrop);

                if (staMain.stcProjCfg.BatchType.Trim() != "Box")
                {
                    boxStripLbl.Visible = false;
                    boxNoStripTxt.Visible = false;

                    boxLabelStripLbl.Visible = false;
                    boxLabelStripTxt.Visible = false;
                }

                int iMainId = oDF.getOCRAreaMainDB(_currScanProj, _appNum);
                oDF.getOCRAreaDB(iMainId);

                if (oDF.stcOCRAreaList.Count > 0 && staMain.checkIndexAutoOCREnabled())
                    bDocAutoOCR = true;

                oImgBytes = null;
                fltBrightness = 1;
                fltContrast = 1;
                txtBrighttoolStrip.Text = fltBrightness.ToString();
                txtContrtoolStrip.Text = fltContrast.ToString();

                //No AddOn button for indexing, use Rescan button.
                btnAddOnStrip.Visible = false;

                if (_batchStatus.Trim() == "8")
                {
                    btnSendStrip.Enabled = true;
                    btnSendStrip.Visible = true;
                    IndexingStatusBar.Text = "Reindexing";
                }
                else
                {
                    btnSendStrip.Enabled = false;
                    btnSendStrip.Visible = false;
                }

                if (!btnSendStrip.Visible && !btnRescanToolStrip.Visible)
                {
                    toolStripSeparator16.Visible = true;
                    toolStripSeparator1.Visible = false;
                }
                else
                {
                    toolStripSeparator16.Visible = true;
                    toolStripSeparator1.Visible = true;
                }

                staMain.sessionStop(false);
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
        }

        private void frmExtraction_Paint(object sender, PaintEventArgs e)
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

        private void frmExtraction_Resize(object sender, EventArgs e)
        {
            try
            {
                formResize();

                System.Drawing.Point oPoint = new System.Drawing.Point();
                //if (tvwSet.Width + dsvImg.Width + pnlIdxFld.Width + 65 < this.Width)
                if (tvwSet.Width + dsvImg.Width + tabMainFields.Width + 65 < this.Width)
                {
                    //dsvImg.Width = this.Width - tvwSet.Width - pnlIdxFld.Width - 65;
                    dsvImg.Width = this.Width - tvwSet.Width - tabMainFields.Width - 65;

                    dgdTable.Width = dsvImg.Width;

                    //dsvImg.Height = txtInfo.Location.Y - dsvImg.Location.Y - 5;
                    tvwSet.Height = Height - IndexStatusStrip.Height - IndexingToolStrip.Height - dsvThumbnailList.Height - 23;
                    dsvImg.Height = Height - IndexStatusStrip.Height - IndexingToolStrip.Height - dsvThumbnailList.Height - txtInfo.Height;
                    dsvImg.Height = dsvImg.Height - dgdTable.Height - 15;

                    txtInfo.Width = dsvImg.Width - cbxTables.Width - panel1.Width - 10;
                    oPoint.Y = dsvImg.Location.Y + dsvImg.Height + dgdTable.Height + 10;
                    oPoint.X = dsvImg.Location.X;
                    txtInfo.Location = oPoint;
                    //dsvImg.Height = txtInfo.Location.Y - dsvImg.Location.Y - 5;

                    oPoint.Y = txtInfo.Location.Y;
                    oPoint.X = txtInfo.Location.X + txtInfo.Width + 5;
                    cbxTables.Location = oPoint;

                    oPoint.Y = tabMainFields.Location.Y;
                    oPoint.X = dsvImg.Location.X + dsvImg.Width + 5;
                    tabMainFields.Location = oPoint;

                    oPoint.Y = dsvImg.Location.Y + dsvImg.Height + dgdTable.Height + 5;
                    oPoint.X = txtInfo.Location.X + cbxTables.Width + txtInfo.Width + 5;
                    panel1.Location = oPoint;
                    //oPoint.Y = pnlIdxFld.Location.Y;
                    //oPoint.X = dsvImg.Location.X + dsvImg.Width + 5;
                    //pnlIdxFld.Location = oPoint;

                    //oPoint.Y = pnlKeyFld.Location.Y;
                    //oPoint.X = dsvImg.Location.X + dsvImg.Width + 5;
                    //pnlKeyFld.Location = oPoint;

                    oPoint.Y = btnSave.Location.Y;
                    //oPoint.X = pnlIdxFld.Location.X;
                    oPoint.X = tabMainFields.Location.X;
                    btnSave.Location = oPoint;
                    oPoint.Y = btnNext.Location.Y;
                    oPoint.X = btnSave.Location.X + btnSave.Width + 12;
                    btnNext.Location = oPoint;

                    //dsvThumbnailList.Width = pnlIdxFld.Location.X + pnlIdxFld.Width - 5;
                    dsvThumbnailList.Width = tabMainFields.Location.X + tabMainFields.Width - 5;
                    oPoint.Y = tvwSet.Location.Y + tvwSet.Height + 5;
                    oPoint.X = tvwSet.Location.X;
                    dsvThumbnailList.Location = oPoint;

                    //pnlIdxFld.Height = dsvThumbnailList.Location.Y - pnlIdxFld.Location.Y - 5;
                    tabMainFields.Height = Height - IndexStatusStrip.Height - IndexingToolStrip.Height - dsvThumbnailList.Height - 27;
                    pnlIdxFld.Height = tabMainFields.Height - 5;
                    pnlKeyFld.Height = tabMainFields.Height - 5;
                    pnlKeyFld.Width = tabKeyValues.Width - 5;
                    //pnlIdxFld.Height = Height - IndexStatusStrip.Height - IndexingToolStrip.Height - dsvThumbnailList.Height - 23;
                    //pnlKeyFld.Height = Height - IndexStatusStrip.Height - IndexingToolStrip.Height - dsvThumbnailList.Height - 23;
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

                this.IndexingStatusBar1.Width = this.Width - this.IndexingStatusBar.Width - this.IndexingStatusBar2.Width - this.CurrDateTime.Width - iFormOffset;

                if (this.IndexingStatusBar.Width > 268)
                    this.IndexingStatusBar.Width = 268;
                if (this.IndexingStatusBar1.Width < 658)
                    this.IndexingStatusBar1.Width = 658;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CurrDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
        }

        private void frmExtraction_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (this.MdiParent != null && mGlobal.checkAnyOpenedForms() == false)
                    this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = true;

                if (_oImgCore != null)
                    _oImgCore.Dispose();

                if (oImgCut != null)
                    oImgCut.Dispose();

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
            try
            {
                oImgBytes = _oImgCore.IO.SaveImageToBytes(sImageIndex, EnumImageFileFormat.WEBTW_TIF);
                setImgInfo(sImageIndex);

                CheckImageCount();
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
                    ComboBox oCbxFld = null; DateTimePicker oDtpFld = null; Button oLookBtn = null;
                    System.Drawing.Point currLoc1, currLoc2, currLoc3, currLoc4;
                    int iDefHeight = txtField.Height; int iDefWidth = txtField.Width; //List<int[]> lPosition = new List<int[]>(); //Position list.

                    int i = 0;
                    while (i < drs.Length)
                    {
                        if (i == 0)
                        {
                            if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "valuelist")
                            {
                                lblField.Text = drs[i].ToString();
                                txtField.Visible = false;
                                dtpField.Visible = false;
                                cbxField.Visible = true;

                                cbxField.MaxLength = Convert.ToInt32(staMain.stcIndexSetting.FieldSize[i].ToString());

                                cbxField.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                {
                                    cbxField.Enabled = false;
                                    cbxField.BackColor = SystemColors.ControlLight;
                                }
                                else
                                {
                                    cbxField.Enabled = true;
                                    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                        cbxField.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        cbxField.BackColor = System.Drawing.Color.White;
                                }

                                lPosition.Add(new int[] { cbxField.Location.Y, cbxField.Height });

                                cbxField.Tag = i.ToString();
                                if (staMain.stcIndexSetting.ValueListType[i].Trim().ToLower() == "listing")
                                    loadValueList(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, "Value List", lblField.Text.Trim(), cbxField);
                                else if (staMain.stcIndexSetting.ValueListType[i].Trim().ToLower() == "data source")
                                    loadValueListDS(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, "Value List", lblField.Text.Trim(), "Data Source", cbxField);

                                cbxField.GotFocus += indexField_Focus;

                                lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();
                            }
                            else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "string"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "integer"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "lookup"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "barcode") //Or MultiLine
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
                                        txtField.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        txtField.BackColor = System.Drawing.Color.White;
                                }

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline")
                                {
                                    txtField.Multiline = true;
                                    txtField.Height = txtField.Height * 8;
                                    txtField.ScrollBars = ScrollBars.Both;
                                }
                                else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "lookup")
                                {
                                    mGlobal.Write2Log("Lookup metadata cannot be first index field, dependency is required on first field. Please check with system administrator.");
                                    //txtField.Width = txtField.Width - btnLook.Width - 2;
                                    //btnLook.Tag = i.ToString();
                                    //btnLook.BringToFront();
                                    //currLoc4 = btnLook.Location;
                                    //currLoc4.Y = oTxtFld.Location.Y - 2;
                                    //btnLook.Location = currLoc4;

                                    //btnLook.Visible = true;
                                    //btnLook.Click += new EventHandler(btnLook_Click);
                                }

                                lPosition.Add(new int[] { txtField.Location.Y, txtField.Height });

                                txtField.Tag = i.ToString();
                                txtField.KeyDown += txtField_KeyDown;
                                txtField.GotFocus += indexField_Focus;

                                lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();
                            }
                            else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date" || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "datetime")
                            {
                                lblField.Text = drs[i].ToString();
                                txtField.Visible = false;
                                dtpField.Visible = true;
                                cbxField.Visible = false;

                                if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                {
                                    dtpField.Enabled = false;
                                    dtpField.BackColor = SystemColors.ControlLight;
                                }
                                else
                                {
                                    dtpField.Enabled = true;
                                    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                        dtpField.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        dtpField.BackColor = System.Drawing.Color.White;
                                }

                                dtpField.Tag = i.ToString();
                                dtpField.Format = DateTimePickerFormat.Custom;

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                {
                                    dtpField.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    dtpField.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                lPosition.Add(new int[] { dtpField.Location.Y, dtpField.Height });

                                dtpField.KeyDown += dtpField_KeyDown;
                                dtpField.GotFocus += indexField_Focus;
                                lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();
                            }
                        }
                        else
                        {
                            if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "valuelist")
                            {
                                currLoc1 = lblField.Location;
                                currLoc1.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;
                                currLoc2 = cbxField.Location;
                                currLoc2.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;
                                currLoc3 = lblFieldExist.Location;
                                currLoc3.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;

                                oLblFld = new Label();
                                oLblFld.AutoSize = true;
                                oLblFld.Name = "lblField" + (i).ToString();
                                oLblFld.Location = currLoc1;
                                oLblFld.Text = drs[i].ToString().Trim();
                                oLblFld.SendToBack();

                                oCbxFld = new ComboBox();
                                oCbxFld.Name = "cbxField" + (i).ToString();
                                oCbxFld.Location = currLoc2;
                                oCbxFld.Size = cbxField.Size;
                                oCbxFld.Tag = (i).ToString();
                                oCbxFld.BringToFront();
                                oCbxFld.Width = cbxField.Width;

                                oLblExist = new Label();
                                oLblExist.Name = "lblFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                oCbxFld.MaxLength = Convert.ToInt32(staMain.stcIndexSetting.FieldSize[i].ToString());

                                oCbxFld.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                {
                                    oCbxFld.Enabled = false;
                                    oCbxFld.BackColor = SystemColors.ControlLight;
                                }
                                else
                                {
                                    oCbxFld.Enabled = true;
                                    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                        oCbxFld.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        oCbxFld.BackColor = System.Drawing.Color.White;
                                }

                                lPosition.Add(new int[] { oCbxFld.Location.Y, oCbxFld.Height });

                                oCbxFld.Tag = i.ToString();
                                if (staMain.stcIndexSetting.ValueListType[i].Trim().ToLower() == "listing")
                                    loadValueList(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, "Value List", oLblFld.Text.Trim(), oCbxFld);
                                else if (staMain.stcIndexSetting.ValueListType[i].Trim().ToLower() == "data source")
                                    loadValueListDS(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, "Value List", oLblFld.Text.Trim(), "Data Source", oCbxFld);

                                oCbxFld.GotFocus += indexField_Focus;

                                lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                pnlIdxFld.Controls.Add(oLblFld);
                                pnlIdxFld.Controls.Add(oCbxFld);
                                pnlIdxFld.Controls.Add(oLblExist);
                            }
                            else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "string"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "integer"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "lookup"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "barcode")
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

                                oTxtFld = new TextBox();
                                oTxtFld.Name = "txtField" + (i).ToString();
                                oTxtFld.Location = currLoc2;
                                oTxtFld.Size = txtField.Size;
                                oTxtFld.Tag = (i).ToString();
                                oTxtFld.BringToFront();
                                oTxtFld.Width = iDefWidth;

                                oLblExist = new Label();
                                oLblExist.Name = "lblFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline")
                                {
                                    oTxtFld.Multiline = true;
                                    oTxtFld.Height = iDefHeight * 8;
                                    oTxtFld.ScrollBars = ScrollBars.Both;
                                    //currLoc1.Y = (currLoc1.Y) + oTxtFld.Height;
                                    //currLoc2.Y = (currLoc2.Y) + oTxtFld.Height;
                                    //currLoc3.Y = (currLoc3.Y) + oTxtFld.Height;
                                    //oLblFld.Location = currLoc1;
                                    //oTxtFld.Location = currLoc2;
                                    //oLblExist.Location = currLoc3;
                                }
                                else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "lookup")
                                {
                                    //oLookBtn = new Button();
                                    //oLookBtn.Name = "btnLook" + (i).ToString();
                                    //oLookBtn.Text = "Search";
                                    //oLookBtn.Width = btnLook.Width;
                                    //oLookBtn.Height = btnLook.Height;
                                    //oLookBtn.Tag = i.ToString();
                                    //oLookBtn.BringToFront();

                                    //currLoc4 = btnLook.Location;
                                    //currLoc4.Y = oTxtFld.Location.Y - 2;
                                    //oLookBtn.Location = currLoc4;

                                    //oTxtFld.Width = oTxtFld.Width - oLookBtn.Width - 2;
                                    //oLookBtn.Visible = true;
                                    //oLookBtn.Click += new EventHandler(btnLook_Click);

                                    //pnlIdxFld.Controls.Add(oLookBtn);
                                }
                                //if (currLoc2.Y <= lPosition[i - 1])
                                //{
                                //    currLoc1.Y = lPosition[i - 1] + 5;
                                //    currLoc2.Y = lPosition[i - 1] + 5;
                                //    currLoc3.Y = lPosition[i - 1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oTxtFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}

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
                                        oTxtFld.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        oTxtFld.BackColor = System.Drawing.Color.White;
                                }

                                lPosition.Add(new int[] { oTxtFld.Location.Y, oTxtFld.Height });

                                oTxtFld.KeyDown += txtField_KeyDown;
                                oTxtFld.GotFocus += indexField_Focus;

                                pnlIdxFld.Controls.Add(oLblFld);
                                pnlIdxFld.Controls.Add(oTxtFld);
                                pnlIdxFld.Controls.Add(oLblExist);
                            }
                            else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date" || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "datetime")
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

                                oDtpFld = new DateTimePicker();
                                oDtpFld.Name = "dtpField" + (i).ToString();
                                oDtpFld.Location = currLoc2;
                                oDtpFld.Size = dtpField.Size;
                                oDtpFld.Tag = (i).ToString();
                                oDtpFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                //if (currLoc2.Y <= lPosition[i - 1][0])
                                //{
                                //    currLoc1.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc2.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc3.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oDtpFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}                                

                                oDtpFld.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                {
                                    oDtpFld.Enabled = false;
                                    oDtpFld.BackColor = SystemColors.ControlLight;
                                }
                                else
                                {
                                    oDtpFld.Enabled = true;
                                    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                        oDtpFld.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        oDtpFld.BackColor = System.Drawing.Color.White;
                                }

                                oDtpFld.Tag = i.ToString();
                                oDtpFld.Format = DateTimePickerFormat.Custom;

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy HH:mm:ss";
                                }

                                lPosition.Add(new int[] { oDtpFld.Location.Y, oDtpFld.Height });

                                oDtpFld.KeyDown += dtpField_KeyDown;
                                oDtpFld.GotFocus += indexField_Focus;

                                pnlIdxFld.Controls.Add(oLblFld);
                                pnlIdxFld.Controls.Add(oDtpFld);
                                pnlIdxFld.Controls.Add(oLblExist);
                            }
                        }

                        i += 1;
                    } //End while.

                    i = 0;
                    while (i < drs.Length)
                    {
                        if (staMain.stcIndexSetting.FieldDataType[i].ToString().ToLower() == "lookup")
                        {
                            Control[] oCtrls = null;
                            if ((i - 1) == 0)
                            {
                                oCtrls = pnlIdxFld.Controls.Find("txtField", true); //Previous one only.
                            }
                            else
                                oCtrls = pnlIdxFld.Controls.Find("txtField" + (i - 1), true); //Previous one only.

                            if (oCtrls != null)
                            {
                                if (oCtrls.Length > 0)
                                {
                                    oCtrls[0].KeyDown += lookupField_KeyDown;
                                    oCtrls[0].GotFocus += indexField_Focus;
                                }
                            }
                        }

                        i += 1;
                    } //End while.

                }
                drs = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void setIndexFieldDefaultValue()
        {
            try
            {
                int iCnt = staMain.stcIndexSetting.RowId.Count;
                int i = 0; Control oCtrl = null; Label oLblExist = null; string sCtrlType = "";

                while (i < iCnt)
                {
                    sCtrlType = "";

                    if (i == 0)
                    {
                        oCtrl = (TextBox)pnlIdxFld.Controls["txtField"];
                        oLblExist = (Label)pnlIdxFld.Controls["lblFieldExist"];
                    }
                    else
                    {
                        oCtrl = (TextBox)pnlIdxFld.Controls["txtField" + i];
                        oLblExist = (Label)pnlIdxFld.Controls["lblFieldExist" + i];
                    }

                    if (oCtrl == null || oCtrl.Visible == false)
                    {
                        if (i == 0)
                            oCtrl = pnlIdxFld.Controls["cbxField"];
                        else
                            oCtrl = pnlIdxFld.Controls["cbxField" + i];
                        sCtrlType = "ComboBox";
                    }

                    if (oLblExist != null) oLblExist.Visible = false;

                    if (oCtrl != null && oLblExist != null && i < iCnt)
                    {
                        if (staMain.stcIndexSetting.FieldAutoFill[i].ToString().Trim().ToUpper() == "N")
                        {
                            if (sCtrlType == "ComboBox")
                            {
                                ComboBox oCbx = (ComboBox)oCtrl;
                                oCbx.SelectedValue = staMain.stcIndexSetting.FieldDefault[i].ToString().Trim();
                            }
                            else
                                oCtrl.Text = staMain.stcIndexSetting.FieldDefault[i].ToString().Trim();
                            oLblExist.Text = oCtrl.Text;
                        }
                    }

                    i += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void setIndexFieldAutoValue(string pValue)
        {
            try
            {
                int iCnt = staMain.stcIndexSetting.RowId.Count;
                int i = 0; Control oCtrl = null; Label oLblExist = null; string sCtrlType = "";
                while (i < iCnt)
                {
                    sCtrlType = "";

                    if (i == 0)
                    {
                        oCtrl = (TextBox)pnlIdxFld.Controls["txtField"];
                        oLblExist = (Label)pnlIdxFld.Controls["lblFieldExist"];
                    }
                    else
                    {
                        oCtrl = (TextBox)pnlIdxFld.Controls["txtField" + i];
                        oLblExist = (Label)pnlIdxFld.Controls["lblFieldExist" + i];
                    }

                    if (oCtrl == null || oCtrl.Visible == false)
                    {
                        if (i == 0)
                            oCtrl = pnlIdxFld.Controls["cbxField"];
                        else
                            oCtrl = pnlIdxFld.Controls["cbxField" + i];
                        sCtrlType = "ComboBox";
                    }

                    if (oLblExist != null) oLblExist.Visible = false;

                    if (oCtrl != null && oLblExist != null && i < iCnt)
                    {
                        if (staMain.stcIndexSetting.FieldAutoFill[i].ToString().Trim().ToUpper() == "Y")
                        {
                            if (sCtrlType == "ComboBox")
                            {
                                ComboBox oCbx = (ComboBox)oCtrl;
                                oCbx.SelectedValue = pValue;
                            }
                            else
                                oCtrl.Text = pValue;
                            oLblExist.Text = pValue;
                        }
                    }

                    i += 1;
                }
            }
            catch (Exception ex)
            {
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
                        int i = 0, j = 0; ; Control oCtrl = null; Label oLblExist = null; string sCtrlType = "";
                        ComboBox oCbx; DateTimePicker oDate; DateTime oDateParsed; string sDate = "";

                        while (i < iCnt)
                        {
                            sCtrlType = "";

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
                                sCtrlType = "Date";
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

                            if (sCtrlType == "Date") //Init.
                            {
                                oDate = (DateTimePicker)oCtrl;
                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                {
                                    ShortDatePattern = oDate.CustomFormat
                                };

                                oDate.Value = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                            }

                            if (oCtrl != null && oLblExist != null && i < drs.Count)
                            {
                                if (sCtrlType != "Date")
                                    oCtrl.Text = ""; //Init.
                                oLblExist.Text = ""; //Init.                                

                                j = 0;
                                while (j < drs.Count)
                                {
                                    if (j < iCnt)
                                    {
                                        if (drs[j][1].ToString().Trim() == staMain.stcIndexSetting.RowId[i].ToString().Trim())
                                        {
                                            if (sCtrlType == "ComboBox")
                                            {
                                                oCbx = (ComboBox)oCtrl;
                                                oCbx.SelectedValue = drs[j][0].ToString().Trim();
                                            }
                                            else if (sCtrlType == "Date")
                                            {
                                                oDate = (DateTimePicker)oCtrl;
                                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                                {
                                                    oDate.CustomFormat = "dd/MM/yyyy";
                                                }
                                                else
                                                {
                                                    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                                }

                                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                                {
                                                    ShortDatePattern = oDate.CustomFormat
                                                };

                                                oDateParsed = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                                                try
                                                {
                                                    if (drs[j][0].ToString().Trim() != string.Empty)
                                                    {
                                                        sDate = mGlobal.sanitizeDateValue(drs[j][0].ToString().Trim(), "");
                                                        if (sDate != string.Empty)
                                                        {
                                                            if (!mGlobal.isDate(sDate, new string[] { oDate.CustomFormat }))
                                                                throw new Exception("Date is in invalid format.");

                                                            if (DateTime.TryParse(sDate, out oDateParsed))
                                                            {
                                                                //oDate.Value = Convert.ToDateTime(drs[j][0].ToString().Trim(), formatInfo);
                                                                oDate.Value = oDateParsed;
                                                            }
                                                            else
                                                            {
                                                                oDate.Value = Convert.ToDateTime(sDate, formatInfo);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    mGlobal.Write2Log("load date value.." + sDate + "," + ex.Message);
                                                    //Continue
                                                }
                                            }
                                            else
                                                oCtrl.Text = drs[j][0].ToString().Trim();

                                            oLblExist.Text = drs[j][0].ToString().Trim();
                                            break;
                                        }
                                    }

                                    j += 1;
                                }
                            }

                            i += 1;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("load value.." + ex.Message);
            }
        }

        public void reloadForAddOn()
        {
            this._oImgCore = new ImageCore();
            staMain.stcImgInfoBySet = new staMain._stcImgInfo();
            try
            {
                loadImagesFrmDisk(_iCurrBatchRowid, "'1','8','6'", "'S','R','A','G'");
                loadBatchSetTreeview("'1','8','6'", "'S','R','A','G'");
                if (tvwSet.Nodes.Count > 0)
                {
                    tvwSet.ExpandAll();
                    if (tvwSet.SelectedNode == null)
                        tvwSet.SelectedNode = tvwSet.Nodes[0];
                }

                setImgInfo(0);

                CheckImageCount();
                tvwSet.Focus();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("reload.." + ex.Message);
            }
        }

        private void initImgInfoCollection(int pTot)
        {
            try
            {
                if (pTot == 0) pTot = 1;

                staMain.stcImgInfoBySet.CurrImgIdx = 0;
                staMain.stcImgInfoBySet.SetNum = new string[pTot];
                staMain.stcImgInfoBySet.CollaNum = new string[pTot];
                staMain.stcImgInfoBySet.DocType = new string[pTot];
                staMain.stcImgInfoBySet.ImgIdx = new int[pTot];
                staMain.stcImgInfoBySet.ImgFile = new string[pTot];
                staMain.stcImgInfoBySet.ImgSeq = new int[pTot];
                staMain.stcImgInfoBySet.Rowid = new int[pTot];
                staMain.stcImgInfoBySet.Page = new string[pTot];

                int i = 0;
                while (i < pTot)
                {
                    staMain.stcImgInfoBySet.SetNum[i] = "";
                    staMain.stcImgInfoBySet.CollaNum[i] = "";
                    staMain.stcImgInfoBySet.DocType[i] = "";
                    staMain.stcImgInfoBySet.ImgIdx[i] = 0;
                    staMain.stcImgInfoBySet.ImgFile[i] = "";
                    staMain.stcImgInfoBySet.ImgSeq[i] = 0;
                    staMain.stcImgInfoBySet.Rowid[i] = 0;
                    staMain.stcImgInfoBySet.Page[i] = "";

                    i += 1;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void loadImagesFrmDisk(long lCurrBatchRowId, string pBatchStatusIn, string pDocStatusIn)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                DataRow bdr = oDF.getDocBatchDB(_currScanProj, _appNum, _batchType, pBatchStatusIn, _station, "", lCurrBatchRowId); //'1'
                initImgInfoCollection(1);

                if (bdr != null)
                {
                    _setNum = bdr["setnum"].ToString();
                    _batchCode = bdr["batchcode"].ToString();
                    _totPage = Convert.ToInt32(bdr["totpagecnt"].ToString());
                    _iCurrBatchRowid = Convert.ToInt64(bdr["rowid"].ToString());
                    _fromProc = bdr["fromproc"].ToString();
                    _batchStatus = bdr["batchstatus"].ToString();

                    boxNoStripTxt.Text = bdr["boxnum"].ToString();
                    boxLabelStripTxt.Text = bdr["boxlabel"].ToString();

                    initImgInfoCollection(_totPage);

                    staMain.stcImgInfoBySet.ScanPrj = _currScanProj;
                    staMain.stcImgInfoBySet.AppNum = _appNum;
                    staMain.stcImgInfoBySet.BatchCode = _batchCode;
                    staMain.stcImgInfoBySet.SetNum[0] = _setNum;

                    DataRowCollection drs = null;
                    if (pBatchStatusIn.Trim().IndexOf("2") > 0)
                    {
                        drs = oDF.getIndexedDocFileByBatchCodeDB(_currScanProj, _appNum, _batchCode, "'I'");
                    }
                    else
                        drs = oDF.getDocFileByBatchCodeDB(_currScanProj, _appNum, _batchCode, pDocStatusIn); //"'S','A'"

                    if (drs != null)
                    {
                        if (drs.Count == 0)
                            drs = oDF.getIndexedDocFileByBatchCodeDB(_currScanProj, _appNum, _batchCode, pDocStatusIn);
                    }

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

                            staMain.stcImgInfoBySet.SetNum[i] = drs[i]["setnum"].ToString().Trim();

                            if (i < iTot) //In case index is out of bound of the array.
                            {
                                staMain.stcImgInfoBySet.CollaNum[i] = drs[i]["collanum"].ToString().Trim();
                                staMain.stcImgInfoBySet.DocType[i] = drs[i]["doctype"].ToString().Trim();
                                staMain.stcImgInfoBySet.ImgIdx[i] = i;
                                staMain.stcImgInfoBySet.ImgFile[i] = sFilePath;
                                staMain.stcImgInfoBySet.ImgSeq[i] = Convert.ToInt32(drs[i]["imageseq"].ToString());
                                staMain.stcImgInfoBySet.Rowid[i] = Convert.ToInt32(drs[i]["rowid"].ToString());
                                staMain.stcImgInfoBySet.Page[i] = drs[i]["page"].ToString();
                            }

                            try
                            {
                                if (File.Exists(sFilePath))
                                    _oImgCore.IO.LoadImage(sFilePath);
                            }
                            catch (Exception ex)
                            {
                                mGlobal.Write2Log("Indexing.." + sFilePath + ".." + ex.Message);
                            }

                            i += 1;
                        }

                        if (i > 0)
                        {
                            setIndexFieldAutoValue(staMain.stcImgInfoBySet.DocType[0].ToString());

                            sCurrSetNum = staMain.stcImgInfoBySet.SetNum[0].ToString();
                            loadIndexFieldValue(sCurrSetNum);
                        }
                    }

                    dsvThumbnailList.Bind(_oImgCore);

                    if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                    {
                        _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = 0;
                        staMain.stcImgInfoBySet.CurrImgIdx = 0;
                        dsvImg.Bind(_oImgCore);

                        dsvImg.IfFitWindow = true; //Windows Size if true else Actual Size set to false.
                        //dsvImg.Zoom = 1; //Actual Size.
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Indexing.." + ex.Message);
            }

            Cursor.Current = Cursors.Default;
        }

        private void setImgInfo(short sImageIndex, bool bIndexValue = true)
        {
            try
            {
                if (staMain.stcImgInfoBySet.ImgIdx.Length > 0 && staMain.stcImgInfoBySet.ImgIdx.Length > sImageIndex)
                {
                    if (staMain.stcImgInfoBySet.BatchCode != null)
                    {
                        staMain.stcImgInfoBySet.CurrImgIdx = sImageIndex;
                        //txtField.Text = staMain.stcImgInfoBySet.DocType[sImageIndex].Trim();
                        if (bIndexValue)
                            loadIndexFieldValue(sCurrSetNum);
                        IndexingStatusBar1.Text = "Document Set " + staMain.stcImgInfoBySet.BatchCode + " : " + staMain.stcImgInfoBySet.DocType[sImageIndex];
                        IndexingStatusBar2.Text = "Total Page: " + _totPage;
                        txtInfo.Text = "Batch " + staMain.stcImgInfoBySet.BatchCode.Replace("_", "-") + ""; //Page 1.
                    }
                }
            }
            catch (Exception ex)
            {
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

        private void loadBatchSetTreeview(string pBatchStatusIn, string pDocuStatusIn)
        {
            staMain._ScanProj = _currScanProj;
            staMain._AppNum = _appNum;

            if (pBatchStatusIn.Trim().IndexOf("2") > 0) //Status is Indexing.
            {
                if (_noSeparator.Trim() == "0")
                {
                    if (_batchType.Trim().ToLower() == "set")
                        staMain.loadIndexSetTreeviewNoSep(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                    else
                        staMain.loadIndexBatchTreeviewNoSep(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                }
                else
                {
                    if (_batchType.Trim().ToLower() == "set")
                        staMain.loadIndexSetTreeview(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn); //"'2'", "'I'"
                    else //Batch process type.
                        staMain.loadIndexBatchTreeview(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn); //"'2'", "'I'"
                }
            }
            else
            {
                if (_noSeparator.Trim() == "0")
                {
                    if (_fromProc.Trim().ToLower() == "verify")
                    {
                        if (_batchType.Trim().ToLower() == "set")
                            staMain.loadIndexSetTreeviewNoSep(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                        else
                            staMain.loadIndexBatchTreeviewNoSep(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                    }
                    else
                    {
                        if (_batchType.Trim().ToLower() == "set")
                            staMain.loadBatchTreeviewNoSep(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn); //"'1'", "'S','A'" 
                        else //Batch process type.
                            staMain.loadBatchTreeviewNoSep(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn); //"'1'", "'S','A'"
                    }
                }
                else
                {
                    if (_fromProc.Trim().ToLower() == "verify")
                    {
                        if (_batchType.Trim().ToLower() == "set")
                            staMain.loadIndexSetTreeview(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                        else
                            staMain.loadIndexBatchTreeview(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                    }
                    else
                    {
                        if (_batchType.Trim().ToLower() == "set")
                            staMain.loadDocuSetTreeview(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn); //"'1'", "'S','A'" 
                        else //Batch process type.
                            staMain.loadBatchTreeview(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn); //"'1'", "'S','A'"
                    }
                }

                if (staMain.checkImageNodesAvailable(tvwSet) == false) //try load from TDocuIndex.
                {
                    if (_noSeparator.Trim() == "0")
                    {
                        if (_batchType.Trim().ToLower() == "set")
                            staMain.loadIndexSetTreeviewNoSep(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                        else
                            staMain.loadIndexBatchTreeviewNoSep(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                    }
                    else
                    {
                        if (_batchType.Trim().ToLower() == "set")
                        {
                            staMain.loadIndexSetTreeview(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                        }
                        else //Batch process type.
                        {
                            staMain.loadIndexBatchTreeview(this.tvwSet, _batchType, _batchCode, pBatchStatusIn, pDocuStatusIn);
                        }
                    }
                }

            }
        }

        private void loadPrevIndexFieldValue()
        {
            try
            {
                loadIndexFieldValue(sCurrSetNum);
            }
            catch (Exception ex)
            {
            }
        }

        private string validateFields()
        {
            string strMsg = "";

            int iCnt = staMain.stcIndexSetting.FieldName.Count;
            int i = 0; Label oLbl = null; Control oTxt = null;
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

                if (oTxt == null || oTxt.Visible == false)
                {
                    if (i == 0)
                    {
                        oTxt = (DateTimePicker)pnlIdxFld.Controls["dtpField"];
                    }
                    else
                    {
                        oTxt = (DateTimePicker)pnlIdxFld.Controls["dtpField" + i];
                    }
                }

                if (oTxt == null || oTxt.Visible == false)
                {
                    if (i == 0)
                    {
                        oTxt = (ComboBox)pnlIdxFld.Controls["cbxField"];
                    }
                    else
                    {
                        oTxt = (ComboBox)pnlIdxFld.Controls["cbxField" + i];
                    }
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
                        if (staMain.stcIndexSetting.FieldDecimal[i] != 0)
                        {
                            if (!mGlobal.IsDecimal(oTxt.Text.Trim()))
                            {
                                strMsg = oLbl.Text + " must be numbers with " + staMain.stcIndexSetting.FieldDecimal[i].ToString() + " decimals!";
                                break;
                            }
                            else
                            {
                                if (oTxt.Text.Trim().Substring(oTxt.Text.Trim().IndexOf('.')).Length - 1 > staMain.stcIndexSetting.FieldDecimal[i])
                                {
                                    strMsg = oLbl.Text + " is more than " + staMain.stcIndexSetting.FieldDecimal[i].ToString() + " decimals!";
                                    break;
                                }
                            }
                        }
                        else if (!mGlobal.IsInteger(oTxt.Text.Trim()))
                        {
                            strMsg = oLbl.Text + " must be numbers!";
                            break;
                        }
                    }

                    if (staMain.stcIndexSetting.FieldDataType[i].ToString().Trim().ToLower() == "date" && oTxt.Text.Trim() != "")
                    {
                        string[] dDateFormats = new string[] { "dd/MM/yyyy", "MM/dd/yyyy" };

                        if (!mGlobal.isDate(oTxt.Text.Trim(), dDateFormats))
                        {
                            strMsg = oLbl.Text + " must be date format!";
                            break;
                        }
                    }

                    if (staMain.stcIndexSetting.FieldDataType[i].ToString().Trim().ToLower() == "datetime" && oTxt.Text.Trim() != "")
                    {
                        string[] dDateFormats = new string[] { "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss" };

                        if (!mGlobal.isDate(oTxt.Text.Trim(), dDateFormats))
                        {
                            strMsg = oLbl.Text + " must be date time format!";
                            break;
                        }
                    }
                }

                i += 1;
            }

            return strMsg;
        }

        private string validateKeyFields(SRDocScanIDPForm.KeyValuePair[] pPairs)
        {
            string strMsg = "";
            try
            {
                if (_result == null)
                {
                    return "Key fields result is blank!";
                }
                
                if (pPairs == null)
                {
                    return "Key fields result value are blank!";
                }
                else if (pPairs.Length == 0)
                {
                    return "Key fields result value list is empty!";
                }
                
                int iCnt = pPairs.Length;
                int i = 0; Label oLbl = null; Control oTxt = null; string sKeyDataType = "text";
                while (i < iCnt)
                {
                    if (i == 0)
                    {
                        oLbl = (Label)pnlKeyFld.Controls["lblKeyField"];
                        oTxt = (TextBox)pnlKeyFld.Controls["txtKeyFld"];
                    }
                    else
                    {
                        oLbl = (Label)pnlKeyFld.Controls["lblKeyField" + i];
                        oTxt = (TextBox)pnlKeyFld.Controls["txtKeyFld" + i];
                    }

                    if (oTxt == null || oTxt.Visible == false)
                    {
                        if (i == 0)
                        {
                            oTxt = (DateTimePicker)pnlKeyFld.Controls["dtpKeyField"];
                        }
                        else
                        {
                            oTxt = (DateTimePicker)pnlKeyFld.Controls["dtpKeyField" + i];
                        }
                    }

                    if (oTxt == null || oTxt.Visible == false)
                    {
                        if (i == 0)
                        {
                            oTxt = (ComboBox)pnlKeyFld.Controls["cbxKeyField"];
                        }
                        else
                        {
                            oTxt = (ComboBox)pnlKeyFld.Controls["cbxKeyField" + i];
                        }
                    }

                    if (oLbl != null && oTxt != null)
                    {
                        //if (staMain.stcIndexSetting.FieldMand[i].ToString().Trim() == "Y" && oTxt.Text.Trim() == "")
                        //{
                        //    strMsg = oLbl.Text + " is required!";
                        //    break;
                        //}
                        if (oTxt.Text.ToLower().IndexOf("selected") != -1 || oTxt.Text.ToLower().IndexOf("unselected") != -1)
                        {
                            sKeyDataType = "check";
                        }
                        else if (mGlobal.isDate(oTxt.Text.Trim(), new string[] { "dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss" }))
                        {
                            sKeyDataType = "date";
                        }
                        else
                            sKeyDataType = "text";

                        //if (sKeyDataType.Trim().ToLower() == "integer" && oTxt.Text.Trim() != "")
                        //{
                        //    if (staMain.stcIndexSetting.FieldDecimal[i] != 0)
                        //    {
                        //        if (!mGlobal.IsDecimal(oTxt.Text.Trim()))
                        //        {
                        //            strMsg = oLbl.Text + " must be numbers with " + staMain.stcIndexSetting.FieldDecimal[i].ToString() + " decimals!";
                        //            break;
                        //        }
                        //        else
                        //        {
                        //            if (oTxt.Text.Trim().Substring(oTxt.Text.Trim().IndexOf('.')).Length - 1 > staMain.stcIndexSetting.FieldDecimal[i])
                        //            {
                        //                strMsg = oLbl.Text + " is more than " + staMain.stcIndexSetting.FieldDecimal[i].ToString() + " decimals!";
                        //                break;
                        //            }
                        //        }
                        //    }
                        //    else if (!mGlobal.IsInteger(oTxt.Text.Trim()))
                        //    {
                        //        strMsg = oLbl.Text + " must be numbers!";
                        //        break;
                        //    }
                        //}

                        if (sKeyDataType.Trim().ToLower() == "date" && oTxt.Text.Trim() != "")
                        {
                            string[] dDateFormats = new string[] { "dd/MM/yyyy", "MM/dd/yyyy" };

                            if (!mGlobal.isDate(oTxt.Text.Trim(), dDateFormats))
                            {
                                strMsg = oLbl.Text + " must be date format!";
                                break;
                            }
                        }

                        if (sKeyDataType.Trim().ToLower() == "datetime" && oTxt.Text.Trim() != "")
                        {
                            string[] dDateFormats = new string[] { "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss" };

                            if (!mGlobal.isDate(oTxt.Text.Trim(), dDateFormats))
                            {
                                strMsg = oLbl.Text + " must be date time format!";
                                break;
                            }
                        }
                    }

                    i += 1;
                } //End While.
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Validate keys.." + ex.StackTrace.ToString());
                strMsg = ex.Message;
            }

            return strMsg;
        }

        private bool saveDocIndexDB(string pSetNum, string pNewFilename, string pNewStatus)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            try
            {
                mGlobal.LoadAppDBCfg();

                bExist = oDF.checkDocuSetIndexExistIn(_currScanProj, _appNum, _batchCode, pSetNum, _docType, "'I','G'", _filename);

                if (bExist)
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "SET [indexstatus]='" + pNewStatus.Trim().Replace("'", "") + "'";
                    sSQL += ",[indexstation]='" + MDIMain.strStation.Trim().Replace("'", "") + "'";
                    sSQL += ",[indexuser]='" + MDIMain.strUserID.Trim().Replace("'", "") + "'";
                    sSQL += ",[indexstart]='" + _indexStart.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    sSQL += ",[indexend]='" + _indexEnd.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    sSQL += ",[docimage]='" + pNewFilename.Trim().Replace("'", "") + "' ";
                    sSQL += ",[page]='" + sFrontBack.Trim().Replace("'", "") + "' ";
                    sSQL += ",[exported]='N' ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND doctype='" + _docType.Trim().Replace("'", "") + "' ";
                    sSQL += "AND indexstatus IN ('I','G') ";
                    sSQL += "AND docimage='" + _filename.Trim().Replace("'", "") + "' ";

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
                    sSQL += ",[indexstation]";
                    sSQL += ",[indexuser]";
                    sSQL += ",[indexstart]";
                    sSQL += ",[indexend]";
                    sSQL += ",[imageseq]";
                    sSQL += ",[docimage]";
                    sSQL += ",[branchcode]";
                    sSQL += ",[page]";
                    sSQL += ") VALUES (";
                    sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _collaNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _docType.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pNewStatus.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + MDIMain.strStation.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + MDIMain.strUserID.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _indexStart.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    sSQL += ",'" + _indexEnd.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    sSQL += "," + iCurrImgSeq + "";
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
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool saveDocIndexFieldDB(string pSetNum, int pPageId = 0)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            int i = 0;
            Control oCtrl = null;
            string sValue = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, pSetNum, _docDefId, "", pPageId.ToString());

                if (bExist)
                {
                    int iId = 0;
                    while (i < staMain.stcIndexSetting.RowId.Count)
                    {
                        iId = staMain.stcIndexSetting.RowId[i];
                        if (i == 0)
                            oCtrl = pnlIdxFld.Controls["txtField"];
                        else
                            oCtrl = pnlIdxFld.Controls["txtField" + i];

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlIdxFld.Controls["dtpField"];
                            else
                                oCtrl = pnlIdxFld.Controls["dtpField" + i];
                        }

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlIdxFld.Controls["cbxField"];
                            else
                                oCtrl = pnlIdxFld.Controls["cbxField" + i];

                            ComboBox oCbx = (ComboBox)oCtrl;
                            if (oCbx.SelectedValue != null)
                                sValue = oCbx.SelectedValue.ToString();
                            else
                            {
                                MessageBox.Show(this, "Value is not selected or not matched! Please select from the list.", "Message");
                                throw new Exception("Value is not selected or not matched!");
                            }
                        }
                        else
                            sValue = oCtrl.Text;

                        bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, pSetNum, _docDefId, iId.ToString(), pPageId.ToString());

                        if (bExist)
                        {
                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                            sSQL += "SET [fldvalue]=N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",[modifieddate]='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                            sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                            sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                            sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                            sSQL += "AND fieldid=" + iId + " ";
                            sSQL += "AND pageid='" + pPageId + "' ";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Update document index data failed!");
                            }
                        }
                        else
                        {
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
                            sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                            sSQL += "," + iId + "";
                            sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",'" + pPageId + "')";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Saving document index data failed!");
                            }
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
                            oCtrl = pnlIdxFld.Controls["txtField"];
                        else
                            oCtrl = pnlIdxFld.Controls["txtField" + i];

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlIdxFld.Controls["dtpField"];
                            else
                                oCtrl = pnlIdxFld.Controls["dtpField" + i];
                        }

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlIdxFld.Controls["cbxField"];
                            else
                                oCtrl = pnlIdxFld.Controls["cbxField" + i];

                            ComboBox oCbx = (ComboBox)oCtrl;
                            if (oCbx.SelectedValue != null)
                                sValue = oCbx.SelectedValue.ToString();
                            else
                            {
                                MessageBox.Show(this, "Value is not selected or not matched! Please select from the list.", "Message");
                                throw new Exception("Value is not selected or not matched!");
                            }
                        }
                        else
                            sValue = oCtrl.Text;

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
                        sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                        sSQL += "," + iId + "";
                        sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                        sSQL += ",'" + pPageId + "')";

                        if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                        {
                            bSaved = false;
                            throw new Exception("Saving document index data failed!!");
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

        private void initImageAllInfo()
        {
            try
            {
                txtInfo.Text = "";
                txtCurrImgIdx.Text = "0";
                txtTotImgNum.Text = "0";

                staMain.stcImgInfoBySet = new staMain._stcImgInfo();

                dsvImg.IfFitWindow = true;
                //dsvImg.Zoom = 1;

                _oImgCore = new ImageCore(); //Init.
                _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                dsvImg.Bind(_oImgCore); //Clear.
                dsvThumbnailList.Bind(_oImgCore);

                tvwSet.Nodes.Clear();

                IndexingStatusBar.Text = "";
                IndexingStatusBar1.Text = "";
                IndexingStatusBar2.Text = "";
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fldCnt > 0 && _keyFldCnt == 0)
                {
                    btnSaveIndex_Click(sender, e);
                }
                else if (_fldCnt == 0 && _keyFldCnt > 0)
                {
                    btnSaveKey_Click(sender, e);
                }
                else //Save both Index field data and key field data.
                {
                    btnSaveIndex_Click(sender, e);
                    btnSaveKey_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
            finally
            {
                tabMainFields.Enabled = true;
            }
        }

        private void btnSaveIndex_Click(object sender, EventArgs e)
        {
            string strMsg = "";
            string strType = "set";
            string sIndexSetNum = sCurrSetNum;
            string sIsFullIdx = "P";
            int iPageId = 0;
            bool bReindex = false;
            try
            {
                if (_batchStatus.Trim() == "8")
                    bReindex = true;

                _isSaved = false;
                strMsg = validateFields();

                string sBaseDir = staMain.stcProjCfg.WorkDir;

                if (_fromProc.Trim().ToLower() == "verify")
                    sBaseDir = staMain.stcProjCfg.IndexDir;

                if (staMain.stcImgInfoBySet.CurrImgIdx < staMain.stcImgInfoBySet.Rowid.Length)
                    iPageId = staMain.stcImgInfoBySet.Rowid[staMain.stcImgInfoBySet.CurrImgIdx];

                if (_noSeparator.Trim() == "0")
                {
                    strType = "page";
                }

                if (strMsg.Trim() == "")
                {
                    if (sCurrSetNum.Trim() == "")
                        strMsg = "The document " + strType + " number is blank! Please select a document " + strType + ".";
                }

                if (strMsg.Trim() == "")
                {
                    if (staMain.checkIsSetIndexed(_currScanProj, _appNum, _batchCode, sIndexSetNum, iPageId, false))
                    {
                        //if (MessageBox.Show(this, "This document " + strType + " " + sCurrSetNum + " already saved, would you like to overwrite?",
                        //    "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        //{
                        _setNum = sCurrSetNum;

                        if (saveDocIndexFieldDB(sIndexSetNum, iPageId))
                        {
                            //oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "1", "2", "", "", "", "", _userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            //MessageBox.Show("Document " + strType + " " + sCurrSetNum + " overwrite successfully!", "Save Indexed Document");
                        }
                        //    tvwSet.Focus();
                        //}

                        if (bDocAutoOCR) //Continue to check for whether already full index for Document OCR feature.
                        {
                            sIsFullIdx = "N";
                            strMsg = "";
                        }
                        //else //Commented in case there is only one page.
                        //    return;
                    }
                }

                if (strMsg.Trim() == "")
                {
                    bool bOK = true; bool bIndexed = false;
                    int i = 0; int cnt = 0; int rowid = 0;
                    string sDir = "", sDestBaseDir = ""; string sDestFilename = "";

                    sDestBaseDir = staMain.stcProjCfg.IndexDir;

                    int iTotSet = 0, iTotSetRec = 0;
                    if (tvwSet.Nodes.Count > 0)
                    {
                        if (_batchType.Trim().ToLower() == "set")
                        {
                            if (_noSeparator.Trim() == "0" && !bDocAutoOCR)
                                iTotSet = 1;
                            else
                            {
                                if (tvwSet.Nodes[0].Nodes.Count > 0)
                                {
                                    iTotSet = Convert.ToInt32(tvwSet.Nodes[0].Nodes.Count.ToString());
                                }
                            }
                        }
                        else if (_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box")
                        {
                            if (tvwSet.Nodes[0].Nodes.Count > 0)
                            {
                                iTotSet = Convert.ToInt32(tvwSet.Nodes[0].Nodes.Count.ToString());
                            }
                        }
                    }
                    else
                        return;

                    bOK = staMain.checkIsFullSetIndexed(_currScanProj, _appNum, _batchCode, iTotSet, ref iTotSetRec);
                    bIndexed = staMain.checkIsSetIndexed(_currScanProj, _appNum, _batchCode, sIndexSetNum, false);

                    if (bDocAutoOCR && bOK && (sIsFullIdx == "N" || sIsFullIdx == "P"))
                    {
                        //if (MessageBox.Show(this, "This batch " + _batchCode + " already fully saved, would you like to full index?"
                        //    + Environment.NewLine + "Yes: This batch will proceed to full index or to close. "
                        //    + Environment.NewLine + "No: This batch remain as not full index or not close. "
                        //    + Environment.NewLine + "Notice:[ Batch which is Fully Indexed not able to edit after this. ]",
                        //    "Confirmation", MessageBoxButtons.YesNo) == DialogResult.No)
                        //{
                        //    return;
                        //}
                        //else
                        sIsFullIdx = "I";
                    }

                    btnSaveStrip.Enabled = false;
                    btnNextStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;
                    btnDeleteStrip.Enabled = false;
                    if (bReindex)
                    {
                        btnSendStrip.Enabled = true;
                        btnSendStrip.Visible = true;
                    }
                    else
                    {
                        btnSendStrip.Enabled = false;
                        btnSendStrip.Visible = false;
                    }

                    _batchCode = staMain.stcImgInfoBySet.BatchCode;
                    _indexEnd = DateTime.Now;

                    IndexingStatusBar.Text = "Saving";

                    if (iTotSetRec + 1 == iTotSet && bIndexed == false) //Last record to be full index and not index for edit.
                    {
                        sIsFullIdx = "F";
                    }

                    if (!bOK && Convert.ToInt32(iTotSet) > 1 && iTotSet != (iTotSetRec + 1) && (sIsFullIdx != "F")) //Partial Index but not the last set for indexing.
                    {
                        _setNum = sCurrSetNum;

                        if (saveDocIndexFieldDB(sIndexSetNum, iPageId))
                        {
                            //MessageBox.Show("Document " + strType + " " + sCurrSetNum + " saved successfully!", "Save Indexed Document");                            
                            btnNext_Click(sender, e);
                        }
                        tvwSet.Focus();
                        return;
                    }
                    else if (sIsFullIdx != "F" && Convert.ToInt32(sIndexSetNum) != iTotSet) //Not last set or not full indexed.
                    {
                        saveDocIndexFieldDB(sIndexSetNum, sIndexSetNum, iPageId); //Save or Update Set Index Field Value from the current set.
                        return;
                    }
                    else
                    {
                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        string sNewStatus = "I";

                        while (i < staMain.stcImgInfoBySet.ImgIdx.Length)
                        {
                            _setNum = staMain.stcImgInfoBySet.SetNum[i].ToString();

                            _collaNum = staMain.stcImgInfoBySet.CollaNum[i];
                            _docType = staMain.stcImgInfoBySet.DocType[i];
                            _filename = staMain.stcImgInfoBySet.ImgFile[i];
                            rowid = staMain.stcImgInfoBySet.Rowid[i];
                            iCurrImgSeq = staMain.stcImgInfoBySet.ImgSeq[i];
                            iPageId = rowid;
                            sFrontBack = staMain.stcImgInfoBySet.Page[i];

                            if (_batchType.ToLower() == "set" && _noSeparator.Trim() == "0" && iTotSet == 1 && !bDocAutoOCR)
                            {
                                saveDocIndexFieldDB(1.ToString(sSetNumFmt), iCurrImgSeq.ToString(sSetNumFmt), iPageId); //Save or Update All Set Index Field Value from the first set.
                            }

                            if (_batchType.Trim().ToLower() == "set")
                            {
                                sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                                //if (_docType.Trim() == "")
                                //{
                                //    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                                //}
                                //else
                                //    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                            }
                            else //Batch.
                            {
                                if (_docType.Trim() == "")
                                {
                                    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                                }
                                else
                                    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                            }
                            //if (_docType.Trim() == "")
                            //{
                            //    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";
                            //}
                            //else
                            //    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _docType + @"\";

                            if (!Directory.Exists(sDir))
                                Directory.CreateDirectory(sDir);

                            sDestFilename = mGlobal.addDirSep(sDir) + mGlobal.getFileNameFromPath(_filename);

                            if (_noSeparator.Trim() == "0") //Set number = Page number selected in TIndexFieldValue table.
                            {
                                sIndexSetNum = sCurrSetNum;
                            }
                            else
                                sIndexSetNum = _setNum;

                            if ((_noSeparator.Trim() == "2" && _docType.Trim() != "") || _noSeparator.Trim() != "2")
                            {
                                if (bReindex) sNewStatus = "G";

                                if (saveDocIndexDB(_setNum, sDestFilename, sNewStatus))
                                {
                                    if (staMain.checkIsSetIndexed(_currScanProj, _appNum, _batchCode, sIndexSetNum, false))
                                    {
                                        updatePageIndexFieldDB(sIndexSetNum, "", iPageId.ToString());

                                        bOK = true;
                                        if (bReindex == false)
                                        {
                                            if (_fromProc.Trim().ToLower() == "verify")
                                            {
                                                bOK = oDF.deleteRecordDB(rowid, "TDocuIndex");
                                            }
                                            else
                                                bOK = oDF.deleteRecordDB(rowid, "TDocuScan");
                                        }

                                        if (bOK)
                                        {
                                            //bOK = oDF.moveImgFile(_filename, sDestFilename);
                                            _filename = sDestFilename;

                                            cnt += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (saveDocIndexFieldDB(sIndexSetNum, iPageId))
                                        {
                                            bOK = true;
                                            if (bReindex == false)
                                            {
                                                if (_fromProc.Trim().ToLower() == "verify")
                                                {
                                                    bOK = oDF.deleteRecordDB(rowid, "TDocuIndex");
                                                }
                                                else
                                                    bOK = oDF.deleteRecordDB(rowid, "TDocuScan");
                                            }

                                            if (bOK)
                                            {
                                                //bOK = oDF.moveImgFile(_filename, sDestFilename);
                                                _filename = sDestFilename;

                                                cnt += 1;
                                            }
                                        }
                                    }
                                }
                                else
                                    mGlobal.Write2Log("File move failed with error!");
                            }

                            i += 1;
                        } //End While.

                        if (cnt > 0)
                        {
                            bOK = true;
                            try
                            {
                                sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";
                                string sDestDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";

                                long lDirExist = Directory.GetFiles(sDestDir).Length; //Get exist files.
                                if (sDir.Trim() != "" && lDirExist == 0) //no overwrite.
                                {
                                    String[] sDirs = new String[1];
                                    sDirs[0] = sDir;

                                    oDF.moveDirectories(sDirs, sDestDir); //Move by whole batch for all sets.
                                    //Directory.Move(sDir, sDestDir);
                                    //if (Directory.Exists(sDir))
                                    //    Directory.Delete(sDir, false);
                                }
                            }
                            catch (Exception ex)
                            {
                                mGlobal.Write2Log("Move Dir.." + ex.Message);
                            }
                        }
                        else
                            bOK = false;

                        if (bOK)
                        {
                            if (!bReindex)
                            {
                                if (_batchStatus.Trim() == "1")
                                    bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "1", "2", "", "", "", "", _userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                else //for status exported.
                                    bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "6", "2", "", "", "", "", _userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            }

                            if (bOK)
                            {
                                syncPageIndexFieldDB(_batchCode, "TDocuIndex", string.Empty); //move scan page id to index page id for update.

                                _isSaved = true;
                                if (!bReindex)
                                {
                                    MessageBox.Show("Document " + strType + " " + _batchCode + " saved successfully!", "Save Indexed Document");
                                }

                                //logSummary(_currScanProj, _appNum, _batchCode);                          
                                tvwSet.Focus();

                                _lIndexedRowid.Add(_iCurrBatchRowid);

                                if (!bReindex)
                                {
                                    //btnNext_Click(sender, e);
                                    this.Close();
                                }
                            }
                            else
                                MessageBox.Show("Document " + strType + " " + _batchCode + " save unsuccessful!", "Save Indexed Document");

                        }
                        else
                            MessageBox.Show("Document " + strType + " " + _batchCode + " save unsuccessful!", "Save Indexed Document");
                    }
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
                IndexingStatusBar.Text = "Extraction Indexing";
                if (_batchStatus.Trim() == "8")
                {
                    btnSendStrip.Enabled = true;
                    btnSendStrip.Visible = true;
                    IndexingStatusBar.Text = "Extraction Reindexing";
                }
                else
                {
                    btnSendStrip.Enabled = false;
                    btnSendStrip.Visible = false;
                }

                btnSaveStrip.Enabled = true;
                btnNextStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnDeleteAllStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
            }
        }

        private void btnSaveKey_Click(object sender, EventArgs e)
        {
            string strMsg = "";
            string strType = "set";
            string sIndexSetNum = sCurrSetNum;
            string sIsFullIdx = "P";
            int iPageId = 0;
            bool bReindex = false;
            try
            {
                SRDocScanIDPForm.KeyValuePair[] oPairs = (SRDocScanIDPForm.KeyValuePair[])_result.KeyValuePairs;
                if (_batchStatus.Trim() == "8")
                    bReindex = true;

                _isSaved = false;
                strMsg = validateKeyFields(oPairs);

                string sBaseDir = staMain.stcProjCfg.WorkDir;

                if (_fromProc.Trim().ToLower() == "verify")
                    sBaseDir = staMain.stcProjCfg.IndexDir;

                if (staMain.stcImgInfoBySet.CurrImgIdx < staMain.stcImgInfoBySet.Rowid.Length)
                    iPageId = staMain.stcImgInfoBySet.Rowid[staMain.stcImgInfoBySet.CurrImgIdx];

                if (_noSeparator.Trim() == "0")
                {
                    strType = "page";
                }

                if (strMsg.Trim() == "")
                {
                    if (sCurrSetNum.Trim() == "")
                        strMsg = "The document " + strType + " number is blank! Please select a document " + strType + ".";
                }

                if (strMsg.Trim() == "")
                {
                    if (staMain.checkIsSetPageIndexed(_currScanProj, _appNum, _batchCode, sIndexSetNum, _docType, iPageId, false))
                    {
                        //if (MessageBox.Show(this, "This document " + strType + " " + sCurrSetNum + " already saved, would you like to overwrite?",
                        //    "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        //{
                        _setNum = sCurrSetNum;

                        if (saveDocKeyFieldDB(sIndexSetNum, _docType, iPageId, _keyFldCnt))
                        {
                            //oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "1", "2", "", "", "", "", _userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            //MessageBox.Show("Document " + strType + " " + sCurrSetNum + " overwrite successfully!", "Save Indexed Document");
                        }
                        //    tvwSet.Focus();
                        //}

                        if (bDocAutoOCR) //Continue to check for whether already full index for Document OCR feature.
                        {
                            sIsFullIdx = "N";
                            strMsg = "";
                        }
                        //else //Commented in case there is only one page.
                        //    return;
                    }
                }

                if (strMsg.Trim() == "")
                {
                    bool bOK = true; bool bIndexed = false;
                    int i = 0; int cnt = 0; int rowid = 0;
                    string sDir = "", sDestBaseDir = ""; string sDestFilename = "";
                    sDestBaseDir = staMain.stcProjCfg.IndexDir;

                    int iTotSet = 0, iTotSetRec = 0;
                    if (tvwSet.Nodes.Count > 0)
                    {
                        if (_batchType.Trim().ToLower() == "set")
                        {
                            if (_noSeparator.Trim() == "0" && !bDocAutoOCR)
                                iTotSet = 1;
                            else
                            {
                                if (tvwSet.Nodes[0].Nodes.Count > 0)
                                {
                                    iTotSet = Convert.ToInt32(tvwSet.Nodes[0].Nodes.Count.ToString());
                                }
                            }
                        }
                        else if (_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box")
                        {
                            if (tvwSet.Nodes[0].Nodes.Count > 0)
                            {
                                iTotSet = Convert.ToInt32(tvwSet.Nodes[0].Nodes.Count.ToString());
                            }
                        }
                    }
                    else
                        return;

                    bOK = staMain.checkIsFullSetKeyIndexed(_currScanProj, _appNum, _batchCode, sIndexSetNum, iTotSet, ref iTotSetRec);
                    bIndexed = staMain.checkIsSetKeyIndexed(_currScanProj, _appNum, _batchCode, sIndexSetNum, _docType, false);

                    if (bDocAutoOCR && bOK && (sIsFullIdx == "N" || sIsFullIdx == "P"))
                    {
                        //if (MessageBox.Show(this, "This batch " + _batchCode + " already fully saved, would you like to full index?"
                        //    + Environment.NewLine + "Yes: This batch will proceed to full index or to close. "
                        //    + Environment.NewLine + "No: This batch remain as not full index or not close. "
                        //    + Environment.NewLine + "Notice:[ Batch which is Fully Indexed not able to edit after this. ]",
                        //    "Confirmation", MessageBoxButtons.YesNo) == DialogResult.No)
                        //{
                        //    return;
                        //}
                        //else
                        sIsFullIdx = "I";
                    }

                    btnSaveStrip.Enabled = false;
                    btnNextStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;
                    btnDeleteStrip.Enabled = false;
                    if (bReindex)
                    {
                        btnSendStrip.Enabled = true;
                        btnSendStrip.Visible = true;
                    }
                    else
                    {
                        btnSendStrip.Enabled = false;
                        btnSendStrip.Visible = false;
                    }

                    _batchCode = staMain.stcImgInfoBySet.BatchCode;
                    _indexEnd = DateTime.Now;

                    IndexingStatusBar.Text = "Saving";

                    if (iTotSetRec + 1 == iTotSet && bIndexed == false) //Last record to be full index and not index for edit.
                    {
                        sIsFullIdx = "F";
                    }

                    if (!bOK && Convert.ToInt32(iTotSet) > 1 && iTotSet != (iTotSetRec + 1) && (sIsFullIdx != "F")) //Partial Index but not the last set for indexing.
                    {
                        _setNum = sCurrSetNum;

                        if (saveDocKeyFieldDB(sIndexSetNum, _docType, iPageId, _keyFldCnt))
                        {
                            //MessageBox.Show("Document " + strType + " " + sCurrSetNum + " saved successfully!", "Save Indexed Document");                            
                            btnNext_Click(sender, e);
                        }
                        tvwSet.Focus();
                        return;
                    }
                    else if (sIsFullIdx != "F" && Convert.ToInt32(sIndexSetNum) != iTotSet) //Not last set or not full indexed.
                    {
                        saveDocKeyFieldDB(sIndexSetNum, sIndexSetNum, _docType, iPageId, _keyFldCnt); //Save or Update Set Index Field Value from the current set.
                        return;
                    }
                    else
                    {
                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        string sNewStatus = "I";

                        while (i < staMain.stcImgInfoBySet.ImgIdx.Length)
                        {
                            _setNum = staMain.stcImgInfoBySet.SetNum[i].ToString();
                            _collaNum = staMain.stcImgInfoBySet.CollaNum[i];
                            _docType = staMain.stcImgInfoBySet.DocType[i];
                            _filename = staMain.stcImgInfoBySet.ImgFile[i];
                            rowid = staMain.stcImgInfoBySet.Rowid[i];
                            iCurrImgSeq = staMain.stcImgInfoBySet.ImgSeq[i];
                            iPageId = rowid;
                            sFrontBack = staMain.stcImgInfoBySet.Page[i];

                            if (_batchType.ToLower() == "set" && _noSeparator.Trim() == "0" && iTotSet == 1 && !bDocAutoOCR)
                            {
                                saveDocKeyFieldDB(1.ToString(sSetNumFmt), iCurrImgSeq.ToString(sSetNumFmt), _docType, iPageId, _keyFldCnt); //Save or Update All Set Index Field Value from the first set.
                            }

                            if (_batchType.Trim().ToLower() == "set")
                            {
                                sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                                //if (_docType.Trim() == "")
                                //{
                                //    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                                //}
                                //else
                                //    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                            }
                            else //Batch.
                            {
                                if (_docType.Trim() == "")
                                {
                                    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                                }
                                else
                                    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                            }
                            //if (_docType.Trim() == "")
                            //{
                            //    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";
                            //}
                            //else
                            //    sDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _docType + @"\";

                            if (!Directory.Exists(sDir))
                                Directory.CreateDirectory(sDir);

                            sDestFilename = mGlobal.addDirSep(sDir) + mGlobal.getFileNameFromPath(_filename);

                            if (_noSeparator.Trim() == "0") //Set number = Page number selected in TIndexFieldValue table.
                            {
                                sIndexSetNum = sCurrSetNum;
                            }
                            else
                                sIndexSetNum = _setNum;

                            if ((_noSeparator.Trim() == "2" && _docType.Trim() != "") || _noSeparator.Trim() != "2")
                            {
                                if (bReindex) sNewStatus = "G";

                                if (saveDocIndexDB(_setNum, sDestFilename, sNewStatus))
                                {
                                    if (staMain.checkIsSetKeyIndexed(_currScanProj, _appNum, _batchCode, sIndexSetNum, _docType, false))
                                    {
                                        updatePageKeyFieldDB(sIndexSetNum, _docType, iPageId.ToString(), iPageId.ToString());

                                        bOK = true;
                                        if (bReindex == false)
                                        {
                                            if (_fromProc.Trim().ToLower() == "verify")
                                            {
                                                bOK = oDF.deleteRecordDB(rowid, "TDocuIndex");
                                            }
                                            else
                                                bOK = oDF.deleteRecordDB(rowid, "TDocuScan");
                                        }

                                        if (bOK)
                                        {
                                            //bOK = oDF.moveImgFile(_filename, sDestFilename);
                                            _filename = sDestFilename;

                                            cnt += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (saveDocKeyFieldDB(sIndexSetNum, _docType, iPageId, _keyFldCnt))
                                        {
                                            bOK = true;
                                            if (bReindex == false)
                                            {
                                                if (_fromProc.Trim().ToLower() == "verify")
                                                {
                                                    bOK = oDF.deleteRecordDB(rowid, "TDocuIndex");
                                                }
                                                else
                                                    bOK = oDF.deleteRecordDB(rowid, "TDocuScan");
                                            }

                                            if (bOK)
                                            {
                                                //bOK = oDF.moveImgFile(_filename, sDestFilename);
                                                _filename = sDestFilename;

                                                cnt += 1;
                                            }
                                        }
                                    }
                                }
                                else
                                    mGlobal.Write2Log("File move failed with error!");
                            }

                            i += 1;
                        } //End While.

                        if (cnt > 0)
                        {
                            bOK = true;
                            try
                            {
                                sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";
                                string sDestDir = mGlobal.addDirSep(sDestBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";

                                long lDirExist = Directory.GetFiles(sDestDir).Length; //Get exist files.
                                if (sDir.Trim() != "" && lDirExist == 0) //no overwrite.
                                {
                                    String[] sDirs = new String[1];
                                    sDirs[0] = sDir;

                                    oDF.moveDirectories(sDirs, sDestDir); //Move by whole batch for all sets.
                                    //Directory.Move(sDir, sDestDir);
                                    //if (Directory.Exists(sDir))
                                    //    Directory.Delete(sDir, false);
                                }
                            }
                            catch (Exception ex)
                            {
                                mGlobal.Write2Log("Move Dir.." + ex.Message);
                            }
                        }
                        else
                            bOK = false;

                        if (bOK)
                        {
                            if (!bReindex)
                            {
                                if (_batchStatus.Trim() == "1")
                                    bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "1", "2", "", "", "", "", _userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                else //for status exported.
                                    bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "6", "2", "", "", "", "", _userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            }

                            if (bOK)
                            {
                                syncPageKeyFieldDB(_batchCode, sIndexSetNum, _docType, "TDocuIndex", string.Empty); //move scan page id to index page id for update.

                                _isSaved = true;
                                if (!bReindex)
                                {
                                    MessageBox.Show("Document " + strType + " " + _batchCode + " saved successfully!", "Save Indexed Keys Document");
                                }

                                //logSummary(_currScanProj, _appNum, _batchCode);                          
                                tvwSet.Focus();

                                _lIndexedRowid.Add(_iCurrBatchRowid);

                                if (!bReindex)
                                {
                                    //btnNext_Click(sender, e);
                                    this.Close();
                                }
                            }
                            else
                                MessageBox.Show("Document " + strType + " " + _batchCode + " save unsuccessful!", "Save Indexed Keys Document");

                        }
                        else
                            MessageBox.Show("Document " + strType + " " + _batchCode + " save unsuccessful!", "Save Indexed Keys Document");
                    }
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
                IndexingStatusBar.Text = "Extraction Indexing";
                if (_batchStatus.Trim() == "8")
                {
                    btnSendStrip.Enabled = true;
                    btnSendStrip.Visible = true;
                    IndexingStatusBar.Text = "Extraction Reindexing";
                }
                else
                {
                    btnSendStrip.Enabled = false;
                    btnSendStrip.Visible = false;
                }

                btnSaveStrip.Enabled = true;
                btnNextStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnDeleteAllStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
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

                        _indexStart = DateTime.Now;

                        IndexingStatusBar.Text = "Loading";
                        btnSaveStrip.Enabled = false;
                        btnSave.Enabled = false;
                        btnNext.Enabled = false;

                        _iCurrBatchRowid = getNextBatchRowId(_currScanProj, _appNum, _iCurrBatchRowid);

                        if (_iCurrBatchRowid == -1 || _iCurrBatchRowid == 0)
                            MessageBox.Show(this, "Index have reached last batch documents.", "Message");
                        else
                        {
                            initImageAllInfo();
                            tvwSet.Nodes.Clear();

                            loadImagesFrmDisk(_iCurrBatchRowid, "'1','8','6'", "'S','R','A','G'");
                            setImgInfo(0);

                            CheckImageCount();

                            loadBatchSetTreeview("'1','8','6'", "'S','R','A','G'");
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
                _prevCnt = 0;
                IndexingStatusBar.Text = "Indexing";
                if (_batchStatus.Trim() == "8")
                {
                    btnSendStrip.Enabled = true;
                    btnSendStrip.Visible = true;
                    IndexingStatusBar.Text = "Reindexing";
                }
                else
                {
                    btnSendStrip.Enabled = false;
                    btnSendStrip.Visible = false;
                }

                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
            }
        }

        private void btnSaveStrip_Click(object sender, EventArgs e)
        {
            try
            {
                btnSave_Click(sender, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void btnNextStrip_Click(object sender, EventArgs e)
        {
            try
            {
                btnNext_Click(sender, e);
            }
            catch (Exception ex)
            {
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

                mnuInsSepStrip.Enabled = false;
                mnuImpSepStrip.Enabled = false;

                resetKeyFieldsConfig();
                tabMainFields.SelectedTab = tabFields;
                tabMainFields.Enabled = false;

                nodeInfo = e.Node.Text.Trim().Substring(0, 4);
                sep = e.Node.Tag.ToString().Trim().Substring(0, 4);

                if (sep.Trim().ToLower() == "sep|")
                {
                    sCurrSetNum = 1.ToString(sSetNumFmt); //First set.

                    mnuImpStrip.Enabled = false;
                    mnuCopyStrip.Enabled = false;
                    mnuCutStrip.Enabled = false;
                    mnuDeleteImgStrip.Enabled = false;
                    mnuReplaceStrip.Enabled = false;
                    mnuPasteStrip.Enabled = false;
                    mnuInsStrip.Enabled = false;

                    if (_batchType.ToLower().Trim() == "set")
                    {
                        IndexingStatusBar1.Text = "Document Set " + staMain.stcImgInfoBySet.BatchCode + " : " + e.Node.Text.Trim();
                        //txtField.Text = e.Node.Text.Trim();
                        mnuDeleteSetStrip.Enabled = true;
                    }
                }
                else if (sep.Trim().ToLower() == "sep1")
                {
                    if (_batchType.ToLower().Trim() == "set") //Doc. Type.
                    {
                        IndexingStatusBar1.Text = "Document Set " + staMain.stcImgInfoBySet.BatchCode + " : " + e.Node.Text.Trim();
                        //txtField.Text = e.Node.Text.Trim();
                        sCurrSetNum = e.Node.Text.Trim();

                        //Load the first image for doc set selected.
                        if (e.Node.Nodes.Count > 0)
                        {
                            short sCurrIdx = -1;
                            string rowid = e.Node.Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();

                            short i = 0;
                            while (i < staMain.stcImgInfoBySet.ImgIdx.Length)
                            {
                                if (rowid == staMain.stcImgInfoBySet.Rowid[i].ToString())
                                {
                                    sCurrIdx = (short)staMain.stcImgInfoBySet.ImgIdx[i];
                                    staMain.stcImgInfoBySet.CurrImgIdx = sCurrIdx;
                                    break;
                                }

                                i += 1;
                            }

                            if (sCurrIdx != -1)
                            {
                                if (File.Exists(staMain.stcImgInfoBySet.ImgFile[sCurrIdx].ToString()))
                                {
                                    _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                    //setImgInfo(sCurrIdx);
                                    //txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                    oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
                                }
                                //else
                                //    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                            }
                        }
                        //End.

                        setIndexFieldDefaultValue();

                        loadIndexFieldValue(sCurrSetNum);

                        mnuInsStrip.Enabled = true;
                        mnuImpStrip.Enabled = true;
                        if (sImgInfoCopy.Trim() != "")
                        {
                            mnuPasteStrip.Enabled = true;
                        }

                        setIndexFieldAutoValue(e.Node.Text);
                    }
                    else if (_batchType.ToLower().Trim() == "batch" || _batchType.ToLower().Trim() == "box") //Set.
                    {
                        mnuImpStrip.Enabled = false;
                        mnuDeleteSetStrip.Enabled = true;

                        mnuCopyStrip.Enabled = false;
                        mnuCutStrip.Enabled = false;
                        mnuDeleteImgStrip.Enabled = false;
                        mnuReplaceStrip.Enabled = false;
                        mnuPasteStrip.Enabled = false;
                        mnuInsStrip.Enabled = false;

                        //Load the first image for doc type for doc set selected.
                        if (e.Node.Nodes.Count > 0)
                        {
                            if (_noSeparator.Trim() == "1")
                            {
                                short sCurrIdx = -1;
                                string rowid = e.Node.Nodes[0].Tag.ToString().Trim().Split('|').GetValue(3).ToString();

                                short i = 0;
                                while (i < staMain.stcImgInfoBySet.ImgIdx.Length)
                                {
                                    if (rowid == staMain.stcImgInfoBySet.Rowid[i].ToString())
                                    {
                                        sCurrIdx = (short)staMain.stcImgInfoBySet.ImgIdx[i];
                                        staMain.stcImgInfoBySet.CurrImgIdx = sCurrIdx;
                                        break;
                                    }

                                    i += 1;
                                }

                                if (sCurrIdx != -1)
                                {
                                    if (File.Exists(staMain.stcImgInfoBySet.ImgFile[sCurrIdx].ToString()))
                                    {
                                        _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                        //setImgInfo(sCurrIdx);
                                        //txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                        oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
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
                                    while (i < staMain.stcImgInfoBySet.ImgIdx.Length)
                                    {
                                        if (rowid == staMain.stcImgInfoBySet.Rowid[i].ToString())
                                        {
                                            sCurrIdx = (short)staMain.stcImgInfoBySet.ImgIdx[i];
                                            staMain.stcImgInfoBySet.CurrImgIdx = sCurrIdx;
                                            break;
                                        }

                                        i += 1;
                                    }

                                    if (sCurrIdx != -1)
                                    {
                                        if (File.Exists(staMain.stcImgInfoBySet.ImgFile[sCurrIdx].ToString()))
                                        {
                                            _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                            //setImgInfo(sCurrIdx);
                                            //txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                            oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
                                        }
                                        //else
                                        //    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                                    }
                                }
                            }
                        }
                        //End.
                        if (_noSeparator.Trim() == "C")
                            sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        else
                            sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                        txtInfo.Text = e.Node.Text;

                        setIndexFieldDefaultValue();

                        loadIndexFieldValue(sCurrSetNum);
                        IndexingStatusBar1.Text = "Document Set " + staMain.stcImgInfoBySet.BatchCode + " : " + sCurrSetNum;
                    }
                }
                else if (sep.Trim().ToLower() == "sep2")
                {
                    if (_batchType.ToLower().Trim() == "batch" || _batchType.ToLower().Trim() == "box") //Doc type.
                    {
                        //txtField.Text = e.Node.Text.Trim();
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

                            short i = 0;
                            while (i < staMain.stcImgInfoBySet.ImgIdx.Length)
                            {
                                if (rowid == staMain.stcImgInfoBySet.Rowid[i].ToString())
                                {
                                    sCurrIdx = (short)staMain.stcImgInfoBySet.ImgIdx[i];
                                    staMain.stcImgInfoBySet.CurrImgIdx = sCurrIdx;
                                    break;
                                }

                                i += 1;
                            }

                            if (sCurrIdx != -1)
                            {
                                if (File.Exists(staMain.stcImgInfoBySet.ImgFile[sCurrIdx].ToString()))
                                {
                                    _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                    //setImgInfo(sCurrIdx);
                                    //txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                    oImgBytes = _oImgCore.IO.SaveImageToBytes(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, EnumImageFileFormat.WEBTW_TIF);
                                }
                                //else
                                //    MessageBox.Show(this, "The selected document page does not exists!", "Message");
                            }
                        }
                        //End.

                        sCurrSetNum = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        txtInfo.Text = "Document Set " + sCurrSetNum + " : " + e.Node.Text;

                        loadIndexFieldValue(sCurrSetNum);
                        IndexingStatusBar1.Text = "Document Set " + staMain.stcImgInfoBySet.BatchCode + " : " + sCurrSetNum + " : " + e.Node.Text.Trim();

                        setIndexFieldAutoValue(e.Node.Text);
                    }
                }
                else if (nodeInfo.Trim().ToLower() == "page")
                {
                    mnuImpStrip.Enabled = true;
                    mnuImpRepStrip.Enabled = true;
                    mnuScanRepStrip.Enabled = true;

                    mnuCopyStrip.Enabled = true;
                    mnuCutStrip.Enabled = true;
                    mnuDeleteImgStrip.Enabled = true;
                    mnuReplaceStrip.Enabled = true;
                    mnuPasteStrip.Enabled = true;
                    mnuInsStrip.Enabled = true;

                    bool bIndexed = false;
                    bool bKeyIndexed = false;
                    _pageId = 0;
                    _fldCnt = 0;
                    _keyFldCnt = 0;
                    lPosition = new List<int[]>();

                    tabMainFields.SelectedTab = tabFields;
                    tabMainFields.Enabled = true;

                    if (_noSeparator.Trim() != "0" && _batchType.Trim().ToLower() != "set")
                    {
                        mnuInsSepStrip.Enabled = true;
                        mnuImpSepStrip.Enabled = true;
                    }

                    if (dOCRPrevValues != null) dOCRPrevValues.Clear();

                    if (_noSeparator.Trim() == "C")
                    {
                        if (dgdTable.DataSource != null)
                        {
                            //dgdTable.Rows.Clear();
                            dgdTable.DataSource = null;
                        }
                        if (cbxTables.Items.Count > 0)
                        {
                            cbxTables.DataSource = null;
                            cbxTables.Items.Clear();
                        }

                        resetIndexFieldsConfig();
                        loadIndexFieldsConfig();
                        _fldCnt = lPosition.Count;

                        resetKeyFieldsConfig();
                    }

                    //sTag = "Doc_" + dr["batchcode"].ToString() + "_" + dr["doctype"].ToString() + "_" + dr["rowid"].ToString();
                    if (e.Node.Tag.ToString().Trim().Split('|').Length > 1)
                    {
                        short sCurrIdx = -1;
                        string rowid = e.Node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                        string docType = e.Node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        _docType = docType;
                        _pageId = Convert.ToInt32(rowid);

                        short i = 0;
                        while (i < staMain.stcImgInfoBySet.ImgIdx.Length)
                        {
                            if (rowid == staMain.stcImgInfoBySet.Rowid[i].ToString())
                            {
                                sCurrIdx = (short)staMain.stcImgInfoBySet.ImgIdx[i];
                                staMain.stcImgInfoBySet.CurrImgIdx = sCurrIdx;
                                break;
                            }

                            i += 1;
                        }

                        if (sCurrIdx != -1)
                        {
                            if (File.Exists(staMain.stcImgInfoBySet.ImgFile[sCurrIdx].ToString()))
                            {
                                _oImgCore.ImageBuffer.CurrentImageIndexInBuffer = sCurrIdx;
                                setImgInfo(sCurrIdx, false);
                                txtInfo.Text = txtInfo.Text + " : " + e.Node.Text;
                                if (staMain.stcImgInfoBySet.Page[sCurrIdx] == "F")
                                    txtInfo.Text = txtInfo.Text + " Front";
                                else
                                    txtInfo.Text = txtInfo.Text + " Back";

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

                        txtInfo.Text = "Document Set : " + sCurrSetNum + " : " + docType + " : " + txtInfo.Text;

                        bIndexed = staMain.checkIsSetIndexed(_currScanProj, _appNum, _batchCode, sCurrSetNum, _pageId, false);
                        setIndexFieldDefaultValue();

                        if (_noSeparator.Trim() == "0")
                        {
                            if (sDrag.ToLower() == string.Empty)
                                loadIndexFieldValue("", rowid);
                            else
                                loadIndexFieldValue(sCurrSetNum, rowid);
                        }
                        else
                        {
                            loadIndexFieldValue(sCurrSetNum, rowid);
                        }

                        IndexingStatusBar1.Text = "Document Set " + staMain.stcImgInfoBySet.BatchCode + " : " + sCurrSetNum + " : " + docType;

                        if (_noSeparator.Trim() == "C")
                            setIndexFieldAutoValue(sCurrSetNum);
                        else
                            setIndexFieldAutoValue(e.Node.Parent.Text);

                        bKeyIndexed = staMain.checkIsSetKeyIndexed(_currScanProj, _appNum, _batchCode, sCurrSetNum, _docType, _pageId, false);
                    }

                    if (_noSeparator.Trim() == "C" && (bIndexed == false || bKeyIndexed == false))
                    {
                        //if (oDF.stcOCRAreaList.Count > 0)
                        //{
                        string sFile = staMain.stcImgInfoBySet.ImgFile[_oImgCore.ImageBuffer.CurrentImageIndexInBuffer].ToString();
                        Bitmap bmp = (Bitmap)Bitmap.FromFile(sFile);
                        Cursor.Current = Cursors.WaitCursor;
                        string sModelId = "";
                        int iPageNum = Convert.ToInt32(e.Node.Text.Split(" ", StringSplitOptions.TrimEntries)[1].ToString());

                        if (_docType.ToUpper().Trim() == "FORM")
                            sModelId = "prebuilt-document"; //"BTNForm";
                        else
                            sModelId = clsIDP.stcIDPMapping.ModelId.Trim(); //"prebuilt-invoice"; //"InvoiceSample";

                        _result = null;
                        if (clsIDP.IDPResultExistDB(_currScanProj, _appNum, _batchCode, _setNum, _docType, iPageNum, clsIDP.stcIDPMapping.EndPoint.Trim(), sModelId))
                        {
                            string sResult = clsIDP.getIDPResultDB(_currScanProj, _appNum, _batchCode, _setNum, _docType, iPageNum, clsIDP.stcIDPMapping.EndPoint.Trim(), sModelId);
                            if (sResult.Trim() != string.Empty)
                            {
                                if (_docType.ToUpper().Trim() == "FORM")
                                    _result = SRDocScanIDPForm.FormAnalysisResult.FromJson(sResult);
                                else
                                    _result = AnalysisResult.FromJson(sResult);
                            }
                        }
                        else
                        {
                            _result = idpFromImageExtraction(iPageNum, bmp, sModelId, sFile);
                        }

                        if (_result != null)
                        {
                            if (_docType.ToUpper().Trim() == "FORM")
                            {
                                if (loadIndexFieldsConfig(_result, ref _fldCnt))
                                {
                                    setIndexFieldDefaultValue();
                                    loadIndexFieldValue(_result);
                                }

                                if (_fldCnt == 0)
                                {
                                    resetIndexFieldsConfig();
                                    lblField.Visible = false;
                                    txtField.Visible = false;
                                }

                                if (bKeyIndexed == false)
                                {
                                    if (_result.KeyValuePairs != null)
                                        if (_result.KeyValuePairs.Length > 0)
                                            if (loadKeysValuesConfig(_result.KeyValuePairs, ref _keyFldCnt))
                                            {
                                                if (_keyFldCnt == 0)
                                                    resetKeyFieldsConfig();

                                                loadKeyValues(_result);
                                                if (resultKeysValuesExistDB(sCurrSetNum, _docType, _pageId) == false)
                                                    saveResultKeysValuesDB(sCurrSetNum, _docType, _pageId, _result.KeyValuePairs);
                                            }
                                }
                                else //key field already indexed and saved to DB.
                                {
                                    if (loadIndexKeyFieldConfigDB(sCurrSetNum, _docType, _pageId, ref _keyFldCnt))
                                    {
                                        if (_keyFldCnt == 0)
                                            resetKeyFieldsConfig();

                                        loadIndexKeyFieldValueDB(sCurrSetNum, _docType, _pageId);
                                    }
                                }
                            }
                            else
                            {
                                if (loadIndexFieldsConfig(_result, ref _fldCnt))
                                {
                                    setIndexFieldDefaultValue();
                                    loadIndexFieldValue(_result);
                                }

                                if (_fldCnt == 0)
                                {
                                    lblField.Visible = false;
                                    txtField.Visible = false;
                                }
                                else
                                {
                                    lblField.Visible = true;
                                    txtField.Visible = true;
                                }
                            }

                            loadTableList(_result);
                            populateGridData(_result, 1);
                        }

                        Cursor.Current = Cursors.Default;
                        //List<System.Drawing.Rectangle> cropAreas = new List<System.Drawing.Rectangle>();
                        //System.Drawing.Rectangle oRect = new System.Drawing.Rectangle();

                        //int i = 0;
                        //while (i < oDF.stcOCRAreaList.Count)
                        //{
                        //    oRect = new System.Drawing.Rectangle();
                        //    oRect.X = oDF.stcOCRAreaList[i].left;
                        //    oRect.Y = oDF.stcOCRAreaList[i].top;
                        //    oRect.Width = oDF.stcOCRAreaList[i].width;
                        //    oRect.Height = oDF.stcOCRAreaList[i].height;
                        //    cropAreas.Add(oRect);

                        //    i += 1;
                        //}
                        //try
                        //{
                        //    ShowResultOnImage(bmp, cropAreas.ToArray()); //May have Parameter is invalid exception related to OnPostTransfer..
                        //}
                        //catch (Exception ex)
                        //{
                        //    mGlobal.Write2Log(ex.Message + ".." + ex.StackTrace.ToString());
                        //}

                        bmp.Dispose();
                        //}
                    }
                    else //Already Indexed.
                    {
                        _fldCnt = lPosition.Count;
                        _result = null;
                        string sModelId = "";
                        if (_docType.ToUpper().Trim() == "FORM")
                            sModelId = "prebuilt-document"; //"BTNForm";
                        else
                            sModelId = clsIDP.stcIDPMapping.ModelId.Trim(); //"prebuilt-invoice"; //"InvoiceSample";

                        int iPageNum = Convert.ToInt32(e.Node.Text.Split(" ", StringSplitOptions.TrimEntries)[1].ToString());
                        string sResult = clsIDP.getIDPResultDB(_currScanProj, _appNum, _batchCode, _setNum, _docType, iPageNum, clsIDP.stcIDPMapping.EndPoint.Trim(), sModelId);
                        if (sResult.Trim() != string.Empty)
                        {
                            if (_docType.ToUpper().Trim() == "FORM")
                                _result = SRDocScanIDPForm.FormAnalysisResult.FromJson(sResult);
                            else
                                _result = AnalysisResult.FromJson(sResult);
                        }

                        if (_fldCnt == 0)
                        {
                            lblField.Visible = false;
                            txtField.Visible = false;
                        }
                        else
                        {
                            lblField.Visible = true;
                            txtField.Visible = true;
                        }

                        if (_result != null)
                        {
                            if (bKeyIndexed)
                            {
                                if (loadIndexKeyFieldConfigDB(sCurrSetNum, _docType, _pageId, ref _keyFldCnt))
                                {
                                    if (_keyFldCnt == 0)
                                        resetKeyFieldsConfig();

                                    loadIndexKeyFieldValueDB(sCurrSetNum, _docType, _pageId);
                                }
                            }

                            loadTableList(_result);
                            populateGridDataDB(sCurrSetNum, _docType, _pageId, "1");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("After select.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnDeleteStrip_Click(object sender, EventArgs e)
        {
            bool bOK = true;
            try
            {
                if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                {
                    string sBaseDir = staMain.stcProjCfg.WorkDir;

                    if (_fromProc.Trim().ToLower() == "verify")
                        sBaseDir = staMain.stcProjCfg.IndexDir;

                    int i = 0; int rowid = 0;
                    bool bConfirm = false;
                    string sBatchPath = mGlobal.replaceValidChars(_batchCode, "_");
                    string sDirRoot = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";

                    if (MessageBox.Show(this, "Confirm to delete the selected image?", "Delete Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        bConfirm = true;

                    if (bConfirm)
                    {
                        btnSaveStrip.Enabled = false;
                        btnSave.Enabled = false;
                        btnNext.Enabled = false;

                        i = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;

                        if (i < staMain.stcImgInfoBySet.ImgIdx.Length)
                        {
                            _setNum = staMain.stcImgInfoBySet.SetNum[i];
                            _collaNum = staMain.stcImgInfoBySet.CollaNum[i];
                            _docType = staMain.stcImgInfoBySet.DocType[i];
                            _filename = staMain.stcImgInfoBySet.ImgFile[i];
                            rowid = staMain.stcImgInfoBySet.Rowid[i];

                            try
                            {
                                if (File.Exists(_filename))
                                    File.Delete(_filename);

                                string sDir = mGlobal.addDirSep(sDirRoot) + _docType + @"\";

                                if (Directory.Exists(sDir) && Directory.GetFiles(sDir).Length == 0)
                                    Directory.Delete(sDir);
                            }
                            catch (Exception ex)
                            {
                                bOK = true; //proceed.
                                mGlobal.Write2Log("Image delete exception!" + ex.Message);
                            }

                            if (bOK)
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    bOK = oDF.deleteRecordDB(rowid, "TDocuIndex");
                                }
                                else
                                    bOK = oDF.deleteRecordDB(rowid, "TDocuScan");

                                if (bOK) //Reload all.
                                {
                                    int iTot = 0;
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        if (_batchType.Trim().ToLower() == "set")
                                            iTot = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                        else
                                            iTot = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                    }
                                    else
                                    {
                                        if (_batchType.Trim().ToLower() == "set")
                                            iTot = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                        else
                                            iTot = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                    }
                                    //_totPage = _totPage - 1;
                                    string sStatus = "1";

                                    if (iTot == 0) sStatus = "3";

                                    if (_batchType.Trim().ToLower() == "set")
                                    {
                                        _docType = sCurrSetNum;
                                        bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "1", iTot, sStatus, "", "", "", sCurrSetNum);
                                        //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "1", iTot, sStatus);
                                    }
                                    else
                                        bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "1", iTot, sStatus);

                                    if (bOK)
                                    {
                                        if (iTot == 0)
                                        {
                                            try
                                            {
                                                if (Directory.Exists(sDirRoot) && Directory.GetFiles(sDirRoot).Length == 0)
                                                    Directory.Delete(sDirRoot);
                                            }
                                            catch (Exception ex)
                                            {
                                                mGlobal.Write2Log("Delete an image.." + ex.Message);
                                            }
                                        }

                                        if (tvwSet.SelectedNode != null && iTot != 0)
                                        {
                                            iCurrImgSeq = Convert.ToInt32(tvwSet.SelectedNode.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                                            if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                            {
                                                if (_fromProc.Trim().ToLower() == "verify")
                                                {
                                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                                }
                                                else
                                                {
                                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                                }
                                            }
                                            else
                                            {
                                                if (_fromProc.Trim().ToLower() == "verify")
                                                {
                                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, iCurrImgSeq);
                                                }
                                                else
                                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, iCurrImgSeq);
                                            }

                                            if (_noSeparator.Trim() == "0")
                                            {
                                                oDF.deleteSetIndexedRecordDB(_currScanProj, _appNum, _batchCode, sCurrSetNum);                                                
                                            }

                                            oDF.deleteAllIDPKeyFieldValuesDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, _docType, rowid.ToString()); //delete indexed key field value if any.
                                            oDF.deleteAllIDPKeyValuesDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, _docType, rowid.ToString()); //delete indexed key value if any.
                                            oDF.deleteAllIDPTableValuesDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, _docType, rowid.ToString()); //delete table value if any.
                                            oDF.deleteAllIDPTableColsDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, _docType, rowid.ToString()); //delete table columns structure if any.

                                            selectedNode = tvwSet.SelectedNode;
                                        }
                                        else if (iTot == 0) //Clear all data and IDP data except the batch data.
                                        {
                                            oDF.deleteAllIndexedRecordDB(_currScanProj, _appNum, _batchCode); //delete indexed value if any.
                                            oDF.deleteAllIDPKeyFieldValuesDB(_currScanProj, _appNum, _batchCode); //delete indexed key field value if any.
                                            oDF.deleteAllIDPKeyValuesDB(_currScanProj, _appNum, _batchCode); //delete indexed key value if any.
                                            oDF.deleteAllIDPTableValuesDB(_currScanProj, _appNum, _batchCode); //delete table value if any.
                                            oDF.deleteAllIDPTableColsDB(_currScanProj, _appNum, _batchCode); //delete table columns structure if any.
                                            oDF.deleteAllIDPModelDB(_currScanProj, _appNum, _batchCode); //delete IDP Model if any.
                                        }
                                    }

                                    if (bOK)
                                        MessageBox.Show("Selected image is deleted successfully!");
                                    else
                                        MessageBox.Show("Selected image delete unsuccessful!");

                                    initImageAllInfo();

                                    loadImagesFrmDisk(_iCurrBatchRowid, "'1','8','6'", "'S','R','A','G'");
                                    setImgInfo(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);

                                    CheckImageCount();

                                    loadBatchSetTreeview("'1','8','6'", "'S','R','A','G'");
                                    if (tvwSet.Nodes.Count > 0)
                                    {
                                        tvwSet.ExpandAll();

                                        TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                                        if (oNodes.Length > 0)
                                        {
                                            tvwSet.SelectedNode = oNodes[0];
                                            tvwSet.SelectedNode.EnsureVisible();
                                        }
                                    }
                                    else if (tvwSet.Nodes.Count == 0)
                                        this.Close();
                                }
                                else
                                    MessageBox.Show("Selected image delete unsuccessful for document data!");
                            }
                            else
                                MessageBox.Show("Selected image delete unsuccessful!");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Deleting image.." + ex.Message);
            }
            finally
            {
                IndexingStatusBar.Text = "Indexing";
                if (_batchStatus == "8")
                    IndexingStatusBar.Text = "Reindexing";

                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
            }
        }

        private void btnAddOnStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (staMain.stcImgInfoBySet.BatchCode != "")
                {
                    if (staMain.stcProjCfg.RescanEnable.ToUpper() == "Y")
                    {
                        if (_fromProc.Trim().ToLower() == "verify")
                        {
                            MessageBox.Show(this, _batchCode.Replace("_", "-") + " Batch from verified process is not allow for Add-on. Please perform rescan.", "Message");
                            return;
                        }
                    }
                    else
                    {
                        if (_fromProc.Trim().ToLower() == "verify")
                        {
                            MessageBox.Show(this, _batchCode.Replace("_", "-") + " Batch from verified process is not allow for Add-on. Please consult your operation manager.", "Message");
                            return;
                        }
                    }

                    MDIMain.sIsAddOn = "indexing";

                    if (mGlobal.checkOpenedForms("frmScan1"))
                        MessageBox.Show(this, "The Scan screen already opened!", "Message");
                    else
                    {
                        frmScan1 ofScan = new frmScan1();
                        ofScan.ShowIcon = false;

                        ofScan.MaximizeBox = true;
                        ofScan.MinimizeBox = false;

                        ofScan.MdiParent = this.MdiParent;
                        //this.MdiChildren[0].MaximizeBox = false;
                        //this.MdiChildren[0].MinimizeBox = false;                       

                        this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                        //this.WindowState = FormWindowState.Minimized;
                        //ofScan.WindowState = FormWindowState.Minimized;
                        //ofScan.FormBorderStyle = FormBorderStyle.None;
                        //ofScan.ClientSize = new System.Drawing.Size(ofScan.Width - 600, this.Height);
                        //ofScan.Location = new Point(0, 0);

                        ofScan.Show();
                        ofScan.Focus();
                    }

                }
                else
                    MessageBox.Show(this, "Document set is empty. Rescan is not available.", "Message");

            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            string strMsg = "";

            try
            {
                _batchCode = staMain.stcImgInfoBySet.BatchCode;

                if (MessageBox.Show("Confirm to send this document set " + _batchCode + " for rescan?", "Confirmation",
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }

                frmRemarks oRemarks = new frmRemarks();
                oRemarks.bEdit = true;
                oRemarks.sRemarks = txtRemarks.Text.Trim();
                oRemarks.ShowDialog(this);
                txtRemarks.Text = oRemarks.sRemarks.Trim();
                oRemarks.Dispose();

                _isSaved = false;
                strMsg = ""; //validateFields();

                if (strMsg.Trim() == "")
                {
                    btnSaveStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnRescanToolStrip.Enabled = false;

                    _setNum = staMain.stcImgInfoBySet.SetNum[0];
                    _batchCode = staMain.stcImgInfoBySet.BatchCode;
                    _indexEnd = DateTime.Now;

                    IndexingStatusBar.Text = "Reverting";

                    bool bOK = true;
                    //if (updateDocIndexDB(txtField1.Text.Trim(), txtField2.Text.Trim(), "R"))
                    if (updateDocIndexDB("R"))
                    {

                        if (_batchStatus.Trim() == "8")
                            bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "8", "4", "", "", txtRemarks.Text.Trim(), "Index");
                        else if (_batchStatus.Trim() == "6")
                            bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "6", "4", "", "", txtRemarks.Text.Trim(), "Index");
                        else
                            bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "1", "4", "", "", txtRemarks.Text.Trim(), "Index");

                        if (bOK)
                        {
                            _isSaved = true;
                            MessageBox.Show("Document Set " + _batchCode + " reverted successfully!", "Revert Index Document");

                            //this.Close();
                            btnNext_Click(this, e);
                        }
                    }
                    else
                        MessageBox.Show("Document Set " + _batchCode + " revert unsuccessful!", "Revert Index Document");

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
                IndexingStatusBar.Text = "Indexing";
                if (_batchStatus.Trim() == "8")
                {
                    btnSendStrip.Enabled = true;
                    btnSendStrip.Visible = true;
                    IndexingStatusBar.Text = "Reindexing";
                }
                else
                {
                    btnSendStrip.Enabled = false;
                    btnSendStrip.Visible = false;
                }

                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
            }
        }

        private bool checkDocuSetIndexExist(string pScanProj, string pAppNum, string pBatchCode, string pStatusIn)
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

        private bool updateDocIndexDB(string pStatus, string pModifiedDate = "")
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            try
            {
                mGlobal.LoadAppDBCfg();

                bExist = checkDocuSetIndexExist(_currScanProj, _appNum, _batchCode, "'I','R','S','G'");

                if (bExist)
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "SET [indexstatus]='" + pStatus + "' ";
                    sSQL += ",[remarks]='" + txtRemarks.Text.Trim().Replace("'", "''") + "' ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND indexstatus IN ('S','A','I','R','G') ";

                    mGlobal.objDB.UpdateRows(sSQL, true);
                }
                else //Update TDocuScan
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                    sSQL += "SET [scanstatus]='" + pStatus + "' ";
                    sSQL += ",[remarks]='" + txtRemarks.Text.Trim().Replace("'", "''") + "' ";
                    sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND scanstatus IN ('S','A','R') ";

                    mGlobal.objDB.UpdateRows(sSQL, true);
                }
            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        public void reloadForCurrRow(long pCurrRowId)
        {
            try
            {
                this._oImgCore = new ImageCore();
                _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                staMain.stcImgInfoBySet = new staMain._stcImgInfo();

                _iCurrBatchRowid = pCurrRowId;

                loadImagesFrmDisk(_iCurrBatchRowid, "'1','8','6'", "'S','R','A','G'");
                loadBatchSetTreeview("'1','8','6'", "'S','R','A','G'");
                if (tvwSet.Nodes.Count == 1)
                {
                    if (tvwSet.Nodes[0].Nodes.Count == 0)
                        loadBatchSetTreeview("'2','8','6'", "'S','R','A','G'");

                    if (tvwSet.Nodes.Count > 0)
                    {
                        tvwSet.ExpandAll();
                        if (tvwSet.Nodes[0].Nodes.Count > 0)
                            tvwSet.SelectedNode = tvwSet.Nodes[0].Nodes[0];
                        else
                            tvwSet.SelectedNode = tvwSet.Nodes[0];
                    }
                }

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

                if (_fromProc.Trim().ToLower() == "verify")
                    syncPageIndexFieldDB(_batchCode, "TDocuIndex");
                else
                    syncPageIndexFieldDB(_batchCode, "TDocuScan");

                if (_batchStatus.Trim() == "8")
                {
                    btnSendStrip.Enabled = true;
                    btnSendStrip.Visible = true;
                    IndexingStatusBar.Text = "Reindexing";
                }
                else
                {
                    btnSendStrip.Enabled = false;
                    btnSendStrip.Visible = false;
                }

                //btnNext.Visible = false;
                //btnNextStrip.Visible = false;
                tvwSet.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reload error!\n" + ex.Message, "Error");
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

        private void goPreviousSet(bool bPrev)
        {
            try
            {
                _oImgCore = new ImageCore();
                _oImgCore.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                dsvImg.IfFitWindow = true;

                dsvImg.Bind(_oImgCore); //Clear.
                dsvThumbnailList.Bind(_oImgCore);

                _indexStart = DateTime.Now;

                IndexingStatusBar.Text = "Loading";
                btnSaveStrip.Enabled = false;
                btnSave.Enabled = false;
                btnRescanToolStrip.Enabled = false;

                long lBatchRowid = 0;
                if (_prevCnt < _lIndexedRowid.Count)
                {
                    lBatchRowid = _lIndexedRowid[_prevCnt];
                    _prevCnt += 1;
                }

                //_iCurrBatchRowid = getPrevIndexedRowId(_currScanProj, _appNum, lBatchRowid);

                if (lBatchRowid == -1 || lBatchRowid == 0)
                {
                    MessageBox.Show(this, "Document batch have reached last batch indexed documents.", "Message");
                    //_iCurrBatchRowid = 0; //Init.
                }
                else
                {
                    if (bPrev)
                        loadImagesFrmDisk(lBatchRowid, "'2','8','6'", "'I','A','G'");
                    else
                        loadImagesFrmDisk(0, "'2','8','6'", "'I','A','G'");

                    setImgInfo(0);

                    CheckImageCount();

                    loadBatchSetTreeview("'2','8','6'", "'I','A','G'");
                    if (tvwSet.Nodes.Count > 0)
                        tvwSet.ExpandAll();

                    loadPrevIndexFieldValue();
                }

                _isSaved = true;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnRescanToolStrip.Enabled = true;
            }
            IndexingStatusBar.Text = "Indexing";
            if (_batchStatus.Trim() == "8")
            {
                btnSendStrip.Enabled = true;
                btnSendStrip.Visible = true;
                IndexingStatusBar.Text = "Reindexing";
            }
            else
            {
                btnSendStrip.Enabled = false;
                btnSendStrip.Visible = false;
            }
        }

        public long getPrevIndexedRowId(string pCurrScanProj, string pAppNum, Int64 pCurrRowId)
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
                sSQL += "AND batchstatus='2' ";
                sSQL += "AND rowid<" + pCurrRowId + " ";
                sSQL += "ORDER BY indexdate DESC ";

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
                sSQL += "AND batchstatus='1' ";
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

        private void btnRefreshStrip_Click(object sender, EventArgs e)
        {
            try
            {
                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnRescanToolStrip.Enabled = true;

                _prevCnt = 0;
                initImageAllInfo();
                tvwSet.Nodes.Clear();

                //_iCurrBatchRowid = 0; //Init.
                //loadImagesFrmDisk(0, "'1'", "'S','R','A'");

                loadImagesFrmDisk(_iCurrBatchRowid, "'1','8','6'", "'S','R','A','G'");

                setImgInfo(0);

                CheckImageCount();

                loadBatchSetTreeview("'1','8','6'", "'S','R','A','G'");
                if (tvwSet.Nodes.Count == 1)
                {
                    if (tvwSet.Nodes[0].Nodes.Count == 0)
                        loadBatchSetTreeview("'2','8','6'", "'S','R','A','G'");
                }

                if (tvwSet.Nodes.Count > 0)
                {
                    tvwSet.ExpandAll();
                    if (tvwSet.Nodes[0].Nodes.Count > 0)
                        tvwSet.SelectedNode = tvwSet.Nodes[0].Nodes[0];
                    else
                        tvwSet.SelectedNode = tvwSet.Nodes[0];
                }

                //_isSaved = false;  //Init.
            }
            catch (Exception ex)
            {
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

        private void tvwSet_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                cmnuTvwStrip.Visible = true;
                cmnuTvwStrip.Show(tvwSet, new System.Drawing.Point(e.X, e.Y));
            }
        }

        private void mnuCopyStrip_Click(object sender, EventArgs e)
        {
            try
            {
                copyImageNode(tvwSet.SelectedNode);
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuCutStrip_Click(object sender, EventArgs e)
        {
            try
            {
                cutImageNode(tvwSet.SelectedNode);
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

                if (tvwSet.SelectedNode != null)
                {
                    TreeNode node = tvwSet.SelectedNode;
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
                                iCurrImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                                sExistRowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                sExistFilename = oDF.getDocFilenameDB(sExistRowid, "TDocuScan");
                                if (sExistFilename.Trim() == string.Empty)
                                    sExistFilename = oDF.getDocFilenameDB(sExistRowid, "TDocuIndex");
                                sFrontBack = "F";

                                bValid = true;
                            }
                            else
                                MessageBox.Show(this, "You have not copy or cut or import an image! " + Environment.NewLine
                                    + "Please select a image and copy or cut or import an image.", "Message");
                        }
                    }

                    if (bValid)
                    {
                        //if (_docType.Trim() != "" || _noSeparator.Trim() == "0")
                        if ((_noSeparator.Trim() == "2" && _docType.Trim() == "") == false)
                        {
                            string sMsg = replaceImageFile(sExistRowid, sExistFilename);

                            if (sMsg.Trim() == "")
                            {
                                reloadForAddOn(); //Refresh.

                                TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                                if (oNodes.Length > 0)
                                {
                                    tvwSet.SelectedNode = oNodes[0];
                                    tvwSet.SelectedNode.EnsureVisible();
                                }
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
                //string sOldSetNum = "";
                string sOldPageId = "";
                string sPageId = "";

                if (tvwSet.SelectedNode != null)
                {
                    TreeNode node = tvwSet.SelectedNode;
                    selectedNode = node;

                    nodeInfo = node.Text.Trim().Substring(0, 4);
                    sep = node.Tag.ToString().Trim().Substring(0, 4);

                    if (sep.Trim().ToLower() == "sep1")
                    {
                        if ((_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box") && node.Tag.ToString().Trim().Split('|').Length > 1) //Doc Set.
                        {
                            if (sImgInfoCopy.Trim() != "") //Page treeview node image info.
                            {
                                if (sImgInfoCopy.Split('|').Length == 4)
                                {
                                    _setNum = node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                                    _setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);

                                    _docType = sImgInfoCopy.Split('|').GetValue(2).ToString();
                                    _collaNum = sImgInfoCopy.Split('|').GetValue(3).ToString();
                                    iCurrImgSeq = oDF.getLastImageSeqDB(_currScanProj, _appNum, _setNum, _collaNum, _batchCode);
                                    sOldPageId = sImgInfoCopy.Split('|').GetValue(0).ToString();
                                    //sOldSetNum = sImgInfoCopy.Split('|').GetValue(4).ToString();

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
                        if ((_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box") && node.Tag.ToString().Trim().Split('|').Length > 1)
                        {
                            if (sImgInfoCopy.Trim() != "") //Page treeview node image info.
                            {
                                _docType = node.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                                _setNum = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                                _setNum = Convert.ToInt32(_setNum).ToString(sSetNumFmt);
                                _collaNum = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                iCurrImgSeq = oDF.getLastImageSeqDB(_currScanProj, _appNum, _setNum, _collaNum, _batchCode);
                                iCurrImgSeq += 1; //always paste at the last page of the doc type if doc type node is selected.
                                sOldPageId = sImgInfoCopy.Split('|').GetValue(0).ToString();
                                //sOldSetNum = sImgInfoCopy.Split('|').GetValue(4).ToString();

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
                                iCurrImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());
                                sOldPageId = sImgInfoCopy.Split('|').GetValue(0).ToString();
                                //sOldSetNum = sImgInfoCopy.Split('|').GetValue(4).ToString();
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
                                reloadForAddOn(); //Refresh.

                                //mGlobal.Write2Log("Node Name.." + tvwSet.aNodes[0].Nodes[0].Nodes[0].Nodes[0].Name);
                                TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                                if (oNodes.Length > 0)
                                {
                                    tvwSet.SelectedNode = oNodes[0];
                                    tvwSet.SelectedNode.EnsureVisible();
                                }

                                short iIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                sPageId = staMain.stcImgInfoBySet.Rowid[iIdx].ToString(); //Rowid

                                if (_noSeparator.Trim() == "0")
                                {
                                    if (bImgCut)
                                    {
                                        if (updatePageIndexFieldDB("", sOldPageId, sPageId)) //current set number comes from node selection in tvwSet_AfterSelect.
                                        {
                                            oDF.reorderPageIndexFieldDB(_currScanProj, _appNum, _batchCode, "");

                                            loadIndexFieldValue("", sPageId);
                                        }
                                    }
                                    else
                                    {
                                        syncDocIndexFieldDB();
                                        updatePageIndexFieldDB("", sOldPageId, sPageId);
                                        loadIndexFieldValue(sCurrSetNum, "");
                                    }
                                }
                                else
                                {
                                    if (bImgCut)
                                    {
                                        updatePageIndexFieldDB(sCurrSetNum, sOldPageId, sPageId);
                                        loadIndexFieldValue(sCurrSetNum, sPageId);
                                    }
                                }

                                //tvwSet.Refresh();
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
                        if (_fromProc.Trim().ToLower() == "verify")
                        {
                            bOK = oDF.deleteRecordDB(rowid, "TDocuIndex");
                        }
                        else
                            bOK = oDF.deleteRecordDB(rowid, "TDocuScan");

                        if (bOK)
                        {
                            if (_fromProc.Trim().ToLower() == "verify")
                            {
                                if (_batchType.Trim().ToLower() == "set")
                                    _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                else
                                    _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                            }
                            else
                            {
                                if (_batchType.Trim().ToLower() == "set")
                                    _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                else
                                    _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                            }

                            //_totPage = _totPage - 1;
                            if (_batchType.Trim().ToLower() == "set")
                            {
                                _docType = sCurrSetNum;
                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage, "", sCurrSetNum);
                                //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage, "");
                            }
                            else
                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage);
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
                string sPageId = "";
                string sOldPageId = sImgInfoCopy.Split('|').GetValue(0).ToString();
                string sSrcFilename = sImgInfoCopy.Split('|').GetValue(1).ToString();

                _filename = pExistFilename;

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
                        oImgCut.IO.SaveAsTIFF(_filename, lIdx);
                        //oImgCut.Dispose(); //Remarks for allow multiple cut and paste.

                        if (File.Exists(_filename))
                        {
                            oImgCut.IO.LoadImage(_filename);

                            short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                            _oImgCore.IO.LoadImage(_filename);
                            _oImgCore.ImageBuffer.SwitchImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                        }

                        if (_fromProc.Trim().ToLower() == "verify")
                        {
                            if (_batchType.Trim().ToLower() == "set")
                            {
                                _docType = sCurrSetNum; //doctype = setnum for set and required for reorderDocTypeScanImageSeq function below.
                                if (_noSeparator.Trim() == "0") //setnum follows scanning setnum for zero separator, thus do not need to update otherwise affect image sequence below.
                                    bOK = updateDocImageDB(pExistRowid, _filename, "TDocuIndex", "", sFrontBack);
                                else
                                    bOK = updateDocImageDB(pExistRowid, _filename, "TDocuIndex", sCurrSetNum, sFrontBack, _docType);
                            }
                            else
                            {
                                if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                    bOK = updateDocImageDB(pExistRowid, _filename, "TDocuIndex", sCurrSetNum, sFrontBack, _docType);
                                else
                                    bOK = updateDocImageDB(pExistRowid, _filename, "TDocuIndex", "", sFrontBack);
                            }
                        }
                        else
                        {
                            if (_batchType.Trim().ToLower() == "set")
                            {
                                _docType = sCurrSetNum; //doctype = setnum for set and required for reorderDocTypeScanImageSeq function below.
                                if (_noSeparator.Trim() == "0") //setnum follows scanning setnum for zero separator, thus do not need to update otherwise affect image sequence below.
                                    bOK = updateDocImageDB(pExistRowid, _filename, "TDocuScan", "", sFrontBack);
                                else
                                    bOK = updateDocImageDB(pExistRowid, _filename, "TDocuScan", sCurrSetNum, sFrontBack, _docType);
                            }
                            else
                            {
                                if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                    bOK = updateDocImageDB(pExistRowid, _filename, "TDocuScan", sCurrSetNum, sFrontBack, _docType);
                                else
                                    bOK = updateDocImageDB(pExistRowid, _filename, "TDocuScan", "", sFrontBack);
                            }
                        }

                        if (bOK)
                        {
                            string sSetNum = sCurrSetNum;
                            if (_noSeparator.Trim() == "0") sSetNum = 1.ToString(sSetNumFmt);

                            if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                }
                                else
                                {
                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                }
                            }
                            else
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq);
                                }
                                else
                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq);
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
                            if (!File.Exists(_filename))
                            {
                                Directory.CreateDirectory(mGlobal.getDirectoriesFromPath(_filename));

                                File.Copy(sSrcFilename, _filename, true);
                            }
                            else
                                File.Copy(sSrcFilename, _filename, true);

                            if (File.Exists(_filename))
                            {
                                short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                _oImgCore.IO.LoadImage(_filename);
                                _oImgCore.ImageBuffer.SwitchImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                            }
                        }
                        catch (Exception ex)
                        {
                            bOK = false;
                            sMsg = ex.Message;
                        }

                        if (bOK)
                        {
                            if (_fromProc.Trim().ToLower() == "verify")
                            {
                                if (_batchType.Trim().ToLower() == "set")
                                {
                                    _docType = sCurrSetNum; //doctype = setnum for set and required for reorderDocTypeScanImageSeq function below.
                                    if (_noSeparator.Trim() == "0") //setnum follows scanning setnum for zero separator, thus do not need to update otherwise affect image sequence below.
                                        bOK = updateDocImageDB(pExistRowid, _filename, "TDocuIndex", "", sFrontBack);
                                    else
                                        bOK = updateDocImageDB(pExistRowid, _filename, "TDocuIndex", sCurrSetNum, sFrontBack, _docType);
                                }
                                else
                                {
                                    if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                        bOK = updateDocImageDB(pExistRowid, _filename, "TDocuIndex", sCurrSetNum, sFrontBack, _docType);
                                    else
                                        bOK = updateDocImageDB(pExistRowid, _filename, "TDocuIndex", "", sFrontBack);
                                }
                            }
                            else
                            {
                                if (_batchType.Trim().ToLower() == "set")
                                {
                                    _docType = sCurrSetNum; //doctype = setnum for set and required for reorderDocTypeScanImageSeq function below.
                                    if (_noSeparator.Trim() == "0") //setnum follows scanning setnum for zero separator, thus do not need to update otherwise affect image sequence below.
                                        bOK = updateDocImageDB(pExistRowid, _filename, "TDocuScan", "", sFrontBack);
                                    else
                                        bOK = updateDocImageDB(pExistRowid, _filename, "TDocuScan", sCurrSetNum, sFrontBack, _docType);
                                }
                                else
                                {
                                    if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                        bOK = updateDocImageDB(pExistRowid, _filename, "TDocuScan", sCurrSetNum, sFrontBack, _docType);
                                    else
                                        bOK = updateDocImageDB(pExistRowid, _filename, "TDocuScan", "", sFrontBack);
                                }
                            }

                            if (bOK)
                            {
                                string sSetNum = sCurrSetNum;
                                if (_noSeparator.Trim() == "0") sSetNum = 1.ToString(sSetNumFmt);

                                if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                    }
                                    else
                                    {
                                        oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                    }
                                }
                                else
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq);
                                    }
                                    else
                                        oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq);
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
            //int iImgSeq = 1;
            string sStatus = "S";
            try
            {
                if (bImgCut) actn = "c"; else actn = "a";

                string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                string sDir = "";
                string sSrcFilename = sImgInfoCopy.Split('|').GetValue(1).ToString();
                string sNewFilename = mGlobal.replaceValidChars(_batchCode.Trim(), "_") + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + actn + ".tiff";
                string sBaseDir = staMain.stcProjCfg.WorkDir;

                if (_fromProc.Trim().ToLower() == "verify")
                    sBaseDir = staMain.stcProjCfg.IndexDir;

                if (_batchType.Trim().ToLower() == "set")
                {
                    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";
                    //if (_docType.Trim() == "")
                    //{
                    //    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";
                    //}
                    //else
                    //    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\" + _docType + @"\";
                }
                else //Batch.
                {
                    if (_docType.Trim() == "")
                    {
                        sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                    }
                    else
                        sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                }

                _filename = mGlobal.addDirSep(sDir) + sNewFilename;

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
                        oImgCut.IO.SaveAsTIFF(_filename, lIdx);
                        //oImgCut.Dispose(); //Remarks for allow multiple cut and paste.

                        if (File.Exists(_filename))
                        {
                            oImgCut.IO.LoadImage(_filename);

                            short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                            _oImgCore.IO.LoadImage(_filename);
                            _oImgCore.ImageBuffer.SwitchImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                        }

                        //if (_fromProc.Trim().ToLower() == "verify")
                        //    sStatus = "I";

                        if (saveDocScanDB(sStatus, _filename))
                        {
                            if (_fromProc.Trim().ToLower() == "verify")
                            {
                                if (_batchType.Trim().ToLower() == "set")
                                    _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                else
                                    _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                            }
                            else
                            {
                                if (_batchType.Trim().ToLower() == "set")
                                    _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                else
                                    _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                            }
                            //_totPage += 1;

                            if (_batchType.Trim().ToLower() == "set")
                            {
                                _docType = sCurrSetNum;
                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage, "", sCurrSetNum);
                                //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage, "");
                            }
                            else
                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage);

                            string sSetNum = sCurrSetNum;
                            if (_noSeparator.Trim() == "0") sSetNum = 1.ToString(sSetNumFmt);

                            if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                }
                                else
                                {
                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                }
                            }
                            else
                            {
                                if (_fromProc.Trim().ToLower() == "verify")
                                {
                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq);
                                }
                                else
                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq);
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
                            File.Copy(sSrcFilename, _filename);

                            if (File.Exists(_filename))
                            {
                                short iCurrImgIdx = _oImgCore.ImageBuffer.CurrentImageIndexInBuffer;
                                _oImgCore.IO.LoadImage(_filename);
                                _oImgCore.ImageBuffer.SwitchImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, iCurrImgIdx);
                            }
                        }
                        catch (Exception ex)
                        {
                            bOK = false;
                            sMsg = ex.Message;
                        }

                        if (bOK)
                        {
                            //if (_fromProc.Trim().ToLower() == "verify")
                            //    sStatus = "I";

                            if (saveDocScanDB(sStatus, _filename))
                            {
                                if (!bImgCut) //Copy and Paste event.
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        if (_batchType.Trim().ToLower() == "set")
                                            _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                        else
                                            _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                    }
                                    else
                                    {
                                        if (_batchType.Trim().ToLower() == "set")
                                            _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sCurrSetNum);
                                        else
                                            _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                    }
                                    //_totPage += 1;

                                    if (_batchType.Trim().ToLower() == "set")
                                    {
                                        _docType = sCurrSetNum;
                                        bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage, "", sCurrSetNum);
                                        //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage, "");
                                    }
                                    else
                                        bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, _totPage);
                                }

                                string sSetNum = sCurrSetNum;
                                if (_noSeparator.Trim() == "0") sSetNum = 1.ToString(sSetNumFmt);

                                if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                    }
                                    else
                                    {
                                        oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq, _docType);
                                    }
                                }
                                else
                                {
                                    if (_fromProc.Trim().ToLower() == "verify")
                                    {
                                        oDF.reorderIndexImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq);
                                    }
                                    else
                                        oDF.reorderScanImageSeq(_currScanProj, _appNum, sSetNum, _collaNum, _batchCode, iCurrImgSeq);
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

        private bool saveDocScanDB(string pScanStatus, string pImgFilename)
        {
            try
            {
                if (_fromProc.Trim().ToLower() == "verify")
                {
                    saveDocuIndexDB(pScanStatus, pImgFilename);
                }
                else
                    saveDocuScanDB(pScanStatus, pImgFilename);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private bool saveDocuScanDB(string pScanStatus, string pImgFilename)
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
                sSQL += ",'" + _station.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _userId.Trim().Replace("'", "") + "'";
                sSQL += ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + pScanStatus.Trim().Replace("'", "") + "'";
                sSQL += "," + iCurrImgSeq + "";
                sSQL += ",'" + pImgFilename.Trim().Replace("'", "") + "'";
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
                throw new Exception(ex.Message);
            }

            return bSaved;
        }

        private bool saveDocuIndexDB(string pScanStatus, string pImgFilename)
        {
            bool bSaved = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "([scanproj]";
                sSQL += ",[appnum]";
                sSQL += ",[setnum]";
                sSQL += ",[collanum]";
                sSQL += ",[batchcode]";
                sSQL += ",[docdefid]";
                sSQL += ",[doctype]";
                sSQL += ",[indexstatus]";
                sSQL += ",[indexstation]";
                sSQL += ",[indexuser]";
                sSQL += ",[imageseq]";
                sSQL += ",[docimage]";
                sSQL += ",[page]";
                sSQL += ") VALUES (";
                sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _setNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _collaNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _docType.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pScanStatus.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _station.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _userId.Trim().Replace("'", "") + "'";
                sSQL += "," + iCurrImgSeq + "";
                sSQL += ",'" + pImgFilename.Trim().Replace("'", "") + "'";
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
                throw new Exception(ex.Message);
            }

            return bSaved;
        }

        private void mnuImpStrip_Click(object sender, EventArgs e)
        {
            try
            {
                string nodeInfo = "";
                string sep = "";

                if (tvwSet.SelectedNode != null)
                {
                    TreeNode node = tvwSet.SelectedNode;
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

                        sFrontBack = "F";
                        mnuPasteStrip_Click(this, e);

                        tvwSet.SelectedNode = node;
                        tvwSet.Focus();
                    }
                    else
                        MessageBox.Show(this, "Please select either document type or page for import an image.", "Message");
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void tvwSet_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                tvwSet.SelectedNode = e.Node;
        }

        private void btnDeleteAllStrip_Click(object sender, EventArgs e)
        {
            bool bOK = true;
            try
            {
                int iNodes = 0;
                if (tvwSet.Nodes.Count > 0)
                {
                    iNodes = tvwSet.Nodes[0].Nodes.Count;
                }

                if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 || iNodes > 0)
                {
                    int i = 0;
                    bool bConfirm = false;

                    if (MessageBox.Show(this, "Confirm to delete ALL sets and all the images?", "Delete All Images Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        bConfirm = true;

                    if (bConfirm)
                    {
                        string sBaseDir = staMain.stcProjCfg.WorkDir;

                        if (_fromProc.Trim().ToLower() == "verify")
                            sBaseDir = staMain.stcProjCfg.IndexDir;

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode, "_");
                        string sDirRoot = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\";

                        IndexingStatusBar.Text = "Deleting all images";
                        btnSaveStrip.Enabled = false;
                        btnSave.Enabled = false;
                        btnNext.Enabled = false;
                        btnRescanToolStrip.Enabled = false;
                        btnDeleteStrip.Enabled = false;
                        btnDeleteAllStrip.Enabled = false;

                        int j = 0;
                        if (Directory.Exists(sDirRoot))
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(sDirRoot);

                            FileInfo file;
                            while (i < dirInfo.GetFiles().Length)
                            {
                                file = (FileInfo)dirInfo.GetFiles().GetValue(i);
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
                                //if (_fromProc.Trim().ToLower() == "verify")
                                //{
                                //    bOK = deleteAllImagesRecordDB(_batchCode, "TDocuIndex");
                                //}
                                //else
                                //    bOK = deleteAllImagesRecordDB(_batchCode, "TDocuScan");

                                if (bOK) //Reload all.
                                {
                                    oDF.deleteAllIndexedRecordDB(_currScanProj, _appNum, _batchCode); //delete indexed value if any.

                                    oDF.deleteAllIDPKeyFieldValuesDB(_currScanProj, _appNum, _batchCode); //delete indexed key field value if any.
                                    oDF.deleteAllIDPKeyValuesDB(_currScanProj, _appNum, _batchCode); //delete indexed key value if any.
                                    oDF.deleteAllIDPTableValuesDB(_currScanProj, _appNum, _batchCode); //delete table value if any.
                                    oDF.deleteAllIDPTableColsDB(_currScanProj, _appNum, _batchCode); //delete table columns structure if any.
                                    oDF.deleteAllIDPModelDB(_currScanProj, _appNum, _batchCode); //delete IDP Model if any.

                                    frmNotification oNotify = new frmNotification();
                                    oNotify.deleteAllNotificationDB(_batchCode);
                                    oNotify.Dispose();

                                    _totPage = 0;
                                    //string sStatus = "1";
                                    string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                    //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "1", _totPageCnt, sStatus, "", "", sModifiedDate);

                                    bOK = deleteBatchRecordDB(_batchCode);

                                    if (bOK)
                                        MessageBox.Show("All image are deleted successfully!");
                                    else
                                        MessageBox.Show("All image delete unsuccessful!");

                                    initImageAllInfo();

                                    _iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, _setNum, "'1','3'");

                                    loadImagesFrmDisk(_iCurrBatchRowid, "'1','3','8','6'", "'S','A','R','G'");

                                    CheckImageCount();

                                    loadBatchSetTreeview("'1','3','8','6'", "'S','A','R','G'");
                                    if (tvwSet.Nodes.Count > 0)
                                        tvwSet.ExpandAll();
                                    else
                                    {
                                        this.Close();
                                    }

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
                mGlobal.Write2Log("Delete all images..." + ex.Message);
            }
            finally
            {
                IndexingStatusBar.Text = "Indexing";
                if (_batchStatus == "8")
                    IndexingStatusBar.Text = "Reindexing";

                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;
            }
        }

        private int getSetTotPage()
        {
            int iCnt = 0;

            if (tvwSet.SelectedNode != null)
            {
                if ((_batchType.Trim().ToLower() == "batch" && tvwSet.SelectedNode.Level == 1)
                    || (_batchType.Trim().ToLower() == "box" && tvwSet.SelectedNode.Level == 1)
                    || (_batchType.Trim().ToLower() == "set" && tvwSet.SelectedNode.Level == 0))
                {
                    int i = 0;
                    while (i < tvwSet.SelectedNode.Nodes.Count) //doc. types.
                    {
                        iCnt += tvwSet.SelectedNode.Nodes[i].Nodes.Count;
                        i += 1;
                    }
                }
            }
            return iCnt;
        }

        private bool deleteAllImagesRecordDB(string pBatchCode, string pDBTableName)
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

        private bool deleteBatchRecordDB(string pBatchCode)
        {
            bool bUpdated = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "SET batchstatus='3' "; //Delete.
                sSQL += ",totpagecnt=0 ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                bUpdated = mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                bUpdated = false;
                //throw new Exception(ex.Message);                            
            }

            return bUpdated;
        }

        private void mnuDeleteSetStrip_Click(object sender, EventArgs e)
        {
            bool bOK = true;
            string sSetNum = "";
            bool bValid = false;
            try
            {
                if ((_batchType.Trim().ToLower() == "batch" || _batchType.Trim().ToLower() == "box") && tvwSet.SelectedNode.Level == 1)
                {
                    bValid = true;
                }
                else if (_batchType.Trim().ToLower() == "set")
                {
                    if (_noSeparator.Trim() == "2" && tvwSet.SelectedNode.Level == 0)
                        bValid = true;
                    else if (_noSeparator.Trim() == "1" && tvwSet.SelectedNode.Level == 1)
                        bValid = true;
                }

                if (tvwSet.SelectedNode != null)
                {
                    if (bValid)
                    {
                        int iNodes = 0;
                        if (tvwSet.Nodes.Count > 0)
                        {
                            iNodes = tvwSet.Nodes[0].Nodes.Count;
                        }

                        if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0 || iNodes > 0)
                        {
                            int i = 0;
                            bool bConfirm = false;

                            sSetNum = tvwSet.SelectedNode.Tag.ToString().Trim().Split('|').GetValue(1).ToString();
                            selectedNode = tvwSet.SelectedNode;

                            if (MessageBox.Show(this, "Confirm to delete selected set " + sSetNum + " and all its images?", "Delete Set Images Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                bConfirm = true;

                            if (bConfirm)
                            {
                                string sBaseDir = staMain.stcProjCfg.WorkDir;

                                if (_fromProc.Trim().ToLower() == "verify")
                                    sBaseDir = staMain.stcProjCfg.IndexDir;

                                string sBatchPath = mGlobal.replaceValidChars(_batchCode, "_");
                                string sDirSet = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + sSetNum + @"\";

                                IndexingStatusBar.Text = "Deleting set";
                                btnSaveStrip.Enabled = false;
                                btnSave.Enabled = false;
                                btnNext.Enabled = false;
                                btnRescanToolStrip.Enabled = false;
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
                                        if (_fromProc.Trim().ToLower() == "verify")
                                        {
                                            bOK = deleteSetImagesRecordDB(_batchCode, sSetNum, "TDocuIndex");
                                        }
                                        else
                                            bOK = deleteSetImagesRecordDB(_batchCode, sSetNum, "TDocuScan");

                                        if (bOK) //Reload all.
                                        {
                                            oDF.deleteSetIndexedRecordDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete set indexed value if any.

                                            oDF.deleteAllIDPKeyFieldValuesDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete indexed key field value if any.
                                            oDF.deleteAllIDPKeyValuesDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete indexed key value if any.
                                            oDF.deleteAllIDPTableValuesDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete table value if any.
                                            oDF.deleteAllIDPTableColsDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete table columns structure if any.
                                            oDF.deleteAllIDPModelDB(_currScanProj, _appNum, _batchCode, sSetNum); //delete IDP Model if any.

                                            _totPage = 0;
                                            string sStatus = "1";
                                            string sModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                            //iNodes = getSetTotPage();
                                            if (_fromProc.Trim().ToLower() == "verify")
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                    _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, sSetNum);
                                                else
                                                    _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                            }
                                            else
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                    _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, sSetNum);
                                                else
                                                    _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                            }

                                            //_totPage = _totPage - iNodes;
                                            if (_totPage < 0) _totPage = 0;

                                            if (_batchType.Trim().ToLower() == "set")
                                            {
                                                sStatus = "3";
                                                _docType = sCurrSetNum;
                                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "1", _totPage, sStatus, "", "", sModifiedDate, sSetNum);
                                                //bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "1", _totPage, sStatus, "", "", sModifiedDate);
                                            }
                                            else
                                                bOK = oDF.updateTotPageDB(_currScanProj, _appNum, _batchCode, "1", _totPage, sStatus, "", "", sModifiedDate);

                                            if (bOK)
                                                bOK = deleteBatchSetRecordDB(_batchCode, sSetNum);

                                            if (bOK)
                                                MessageBox.Show("All image are deleted successfully!");
                                            else
                                                MessageBox.Show("All image delete unsuccessful!");

                                            initImageAllInfo();

                                            _iCurrBatchRowid = oDF.getBatchCurrRowID(_currScanProj, _appNum, _batchCode, "'1'");

                                            loadImagesFrmDisk(_iCurrBatchRowid, "'1','8','6'", "'S','A','R','G'");

                                            CheckImageCount();

                                            loadBatchSetTreeview("'1','8','6'", "'S','A','R','G'");
                                            if (tvwSet.Nodes.Count > 0)
                                            {
                                                tvwSet.ExpandAll();

                                                TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                                                if (oNodes.Length > 0)
                                                {
                                                    tvwSet.SelectedNode = oNodes[0];
                                                    tvwSet.SelectedNode.EnsureVisible();
                                                }
                                            }
                                            setImgInfo(0);
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
                IndexingStatusBar.Text = "Indexing";
                if (_batchStatus == "8")
                    IndexingStatusBar.Text = "Reindexing";

                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;
            }
        }

        private void mnuDeleteImgStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (tvwSet.SelectedNode != null)
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

                if (tvwSet.SelectedNode != null)
                {
                    TreeNode node = tvwSet.SelectedNode;
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

                        //tvwSet.Focus();
                        tvwSet.SelectedNode = node;
                        //tvwSet.SelectedNode.EnsureVisible();                        

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

                if (tvwSet.SelectedNode != null)
                {
                    node = tvwSet.SelectedNode;

                    btnSaveStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnRescanToolStrip.Enabled = false;
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;

                    this.AcquireImageReplace();

                    if (oImgCut != null)
                    {

                        bImgCut = true;

                        nodeInfo = node.Text.Trim().Substring(0, 4);
                        sep = node.Tag.ToString().Trim().Substring(0, 4);

                        string rowid = "";
                        sImgInfoCopy = "";

                        //string sBaseDir = staMain.stcProjCfg.WorkDir;

                        //if (_fromProc.Trim().ToLower() == "verify")
                        //    sBaseDir = staMain.stcProjCfg.IndexDir;

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        //if (_batchType.Trim().ToLower() == "set")
                        //{
                        //    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //    //if (_docType.Trim() == "")
                        //    //{
                        //    //    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //    //}
                        //    //else
                        //    //    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        //}
                        //else //Batch.
                        //{
                        //    if (_docType.Trim() == "")
                        //    {
                        //        sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //    }
                        //    else
                        //        sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        //}

                        //if (staMain.stcProjCfg.BatchAuto.ToUpper() == "Y")
                        //    sFileName = sBatchPath + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;
                        //else
                        //    sFileName = sBatchPath + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;
                        sFileName = sBatchPath + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;

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
                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                //tvwSet.Focus();
                if (node != null) tvwSet.SelectedNode = node;
                //tvwSet.SelectedNode.EnsureVisible();
            }
        }

        #region AcquireImage Callback
        public void OnPostAllTransfers()
        {
            CrossThreadOperationControl crossDelegate = delegate ()
            {
                try
                {
                    //mGlobal.Write2Log("Post all.");
                    return;
                }
                catch (Exception ex)
                {
                    mGlobal.Write2Log(ex.Message);
                }
            };

            Invoke(crossDelegate);
        }

        public bool OnPostTransfer(Bitmap bit, string info)
        {
            try
            {
                oImgCut = new ImageCore();

                //mGlobal.Write2Log(info);
                oImgCut.IO.LoadImage(bit);

                sErrMsg = "";
                OnTransferCancelled();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Post an image.." + ex.Message);
                return false;
            }

            return true; //Scan only once at a time.
        }

        public void OnPreAllTransfers()
        {
        }

        public bool OnPreTransfer()
        {
            return true;
        }

        public void OnSourceUIClose()
        {
        }

        public void OnTransferCancelled()
        {
            try
            {
                if (sErrMsg.Trim() != "")
                {
                    MessageBox.Show(this, "Scan process stopped with error! " + Environment.NewLine + sErrMsg, "Process Cancel");
                    sErrMsg = "";
                }

                return;
            }
            catch (Exception ex)
            {
            }
        }

        public void OnTransferError()
        {
            MessageBox.Show(this, "Error happen while in scanning..", "Error");
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

        #region Area functions
        private void btnHandStrip_Click(object sender, EventArgs e)
        {
            dsvImg.MouseShape = true;
            dsvImg.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;
        }

        private void btnPointStrip_Click(object sender, EventArgs e)
        {
            dsvImg.MouseShape = false;
            dsvImg.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;
        }

        private void btnCutAreaStrip_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
            {
                dsvImg.MouseShape = false;
                dsvImg.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;
                System.Drawing.Rectangle rc = dsvImg.GetSelectionRect(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                if (rc.IsEmpty)
                {
                    MessageBox.Show("Please select the rectangle area first!", "Warning Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _oImgCore.ImageProcesser.CutFrameToClipborad(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, rc.Left, rc.Top, rc.Right, rc.Bottom);
                }
            }
        }

        private void btnCropAreaStrip_Click(object sender, EventArgs e)
        {
            if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
            {
                System.Drawing.Rectangle rc = dsvImg.GetSelectionRect(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                if (rc.IsEmpty)
                {
                    MessageBox.Show("Please select the rectangle area first!", "Warning Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    cropPicture(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer, rc);
                }
            }
        }

        private void cropPicture(int imageIndex, System.Drawing.Rectangle rc)
        {
            _oImgCore.ImageProcesser.Crop((short)imageIndex, rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height);
        }
        #endregion Area functions

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

        private bool updateImageFile(short pCurrImgIdx)
        {
            try
            {
                if (pCurrImgIdx < staMain.stcImgInfoBySet.ImgFile.Length)
                {
                    string sFile = staMain.stcImgInfoBySet.ImgFile[pCurrImgIdx];

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

        private void btnExpaAllStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (tvwSet.Nodes.Count > 0)
                    tvwSet.ExpandAll();
            }
            catch (Exception ex)
            {
            }
        }

        private void btnCollAllStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (tvwSet.Nodes.Count > 0)
                    tvwSet.CollapseAll();
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

        private void txtField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;
            try
            {
                var ctrl = (Control)sender;
                if (ctrl is TextBox)
                {
                    e.Handled = true;

                    int iIdx = Convert.ToInt32(ctrl.Tag.ToString());
                    string sDataType = "";

                    if (e.Shift && e.KeyCode == Keys.Enter && ((TextBox)ctrl).Multiline == true)
                    {
                    }
                    else if (e.KeyCode == Keys.Enter && ((TextBox)ctrl).Multiline == true)
                    {
                        //pnlIdxFld.SelectNextControl(ctrl, true, true, true, true);
                        btnSave_Click(sender, e);
                        btnNext_Click(sender, e);
                    }
                    else if (e.KeyCode == Keys.Enter && ((TextBox)ctrl).Multiline == false)
                    {
                        //Look for next field data type.
                        if (iIdx == staMain.stcIndexSetting.FieldDataType.Count)
                        {
                            sDataType = staMain.stcIndexSetting.FieldDataType[iIdx - 1].ToString();
                        }
                        else
                        {
                            if (staMain.stcIndexSetting.FieldDataType.Count < iIdx + 1)
                                sDataType = staMain.stcIndexSetting.FieldDataType[iIdx + 1].ToString();
                            else
                                sDataType = staMain.stcIndexSetting.FieldDataType[iIdx].ToString();
                        }

                        if (sDataType.ToLower() == "lookup")
                        {
                            e.Handled = false;
                            //next should trigger event lookupField_KeyDown.
                        }
                        else
                        {
                            //pnlIdxFld.SelectNextControl(ctrl, true, true, true, true);
                            btnSave_Click(sender, e);
                            btnNext_Click(sender, e);
                        }
                    }
                    else if (e.Control && e.KeyCode == Keys.S)
                    {
                        btnSave_Click(sender, e);
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Keydown.." + ex.Message);
            }
        }

        private void dtpField_KeyDown(object sender, KeyEventArgs e)
        {
            var ctrl = (Control)sender;
            if (ctrl is DateTimePicker)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    //pnlIdxFld.SelectNextControl(ctrl, true, true, true, true);
                    btnNext_Click(sender, e);
                }
            }
        }

        private void tvwSet_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                txtField.Focus();
                //if (txtField.Enabled)
                //if (txtField.Text.Trim() == "")
                //    txtField.Focus();
                //else
                //    btnNext_Click(sender, e);
            }
        }

        private void btnLook_Click(object sender, EventArgs e)
        {
            try
            {
                frmLookup ofLookup = new frmLookup();
                ofLookup.ShowIcon = false;

                ofLookup.StartPosition = FormStartPosition.CenterScreen;
                ofLookup.ShowDialog(this);

                Button oBtn = (Button)sender;
                int iIdx = Convert.ToInt32(oBtn.Tag.ToString().Trim()); //Get Control Index.

                ((TextBox)pnlIdxFld.Controls["txtField" + iIdx]).Text = ofLookup.returnValue;

                //MessageBox.Show(ofLookup.returnValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void logSummary(string pCurrScanProj, string pAppNum, string pBatchCode)
        {
            try
            {
                string sSQL = "SELECT setnum,doctype FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "GROUP BY setnum,doctype ";

                mGlobal.LoadAppDBCfg();

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    int i = 0;
                    while (i < drs.Count)
                    {
                        staMain.postScanSummary("INDEX", _currScanProj, _appNum, drs[i]["setnum"].ToString(), _batchCode, drs[i]["doctype"].ToString(),
                        "I", mGlobal.secDecrypt(mGlobal.GetAppCfg("Branch"), mGlobal.objSecData), _station, _userId, _indexStart.ToString("yyyy-MM-dd HH:mm:ss"), _indexEnd.ToString("yyyy-MM-dd HH:mm:ss"));

                        i += 1;
                    }
                }
                drs = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region treeview drag and drop events
        private void tvwSet_ItemDrag(object sender, ItemDragEventArgs e)
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

        private void tvwSet_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void tvwSet_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.  
            System.Drawing.Point targetPoint = tvwSet.PointToClient(new System.Drawing.Point(e.X, e.Y));

            TreeNode oNode = tvwSet.GetNodeAt(targetPoint);

            if (oNode != null)
            {
                if (oNode.Level == 2 || oNode.Level == 3)
                {
                    // Select the node at the mouse position.  
                    tvwSet.SelectedNode = oNode;
                }
                else if (oNode.Level != 0)
                {
                    tvwSet.SelectedNode = oNode;
                }
            }
        }

        private void tvwSet_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.  
            System.Drawing.Point targetPoint = tvwSet.PointToClient(new System.Drawing.Point(e.X, e.Y));

            // Retrieve the node at the drop location.  
            TreeNode targetNode = tvwSet.GetNodeAt(targetPoint);

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
                    string sDragImgSeq = draggedNode.Tag.ToString().Trim().Split('|').GetValue(6).ToString();
                    int iTargetImgSeq = Convert.ToInt32(targetNode.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                    if (sDragImgSeq.Trim() == (iTargetImgSeq - 1).ToString()) //Move one page down is the same position.
                    {
                        MessageBox.Show(this, "Reorder image one level down is the same position, move image cancelled.", "Message");
                        return;
                    }

                    sDrag = "move";
                    tvwSet.SelectedNode = draggedNode;
                    bool bRet = cutImageNode(tvwSet.SelectedNode, false);
                    if (bRet)
                    {
                        tvwSet.SelectedNode = targetNode;
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
                    tvwSet.SelectedNode = draggedNode;
                    bool bRet = copyImageNode(tvwSet.SelectedNode);

                    if (bRet)
                    {
                        tvwSet.SelectedNode = targetNode;
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
                tvwSet.Refresh();
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
                            iCurrImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());
                            string srcFilename = oDF.getDocFilenameDB(rowid, "TDocuScan");

                            if (srcFilename.Trim() == string.Empty)
                                srcFilename = oDF.getDocFilenameDB(rowid, "TDocuIndex");

                            sImgInfoCopy = rowid + "|" + srcFilename + "|" + docType + "|" + collaNum + "|" + setNum + "|" + iCurrImgSeq;

                            if (File.Exists(srcFilename))
                            {
                                oImgCut = new ImageCore();
                                oImgCut.ImageBuffer.MaxImagesInBuffer = MDIMain.intMaxImgBuff;

                                oImgCut.IO.LoadImage(srcFilename);

                                //_oImgCore.IO.CopyToClipborad(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                                //_oImgCore.ImageBuffer.RemoveImage(_oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
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
                                    if (_noSeparator.Trim() == "2" || _noSeparator.Trim() == "C")
                                    {
                                        if (_fromProc.Trim().ToLower() == "verify")
                                        {
                                            oDF.reorderIndexImageSeq(_currScanProj, _appNum, setNum, collaNum, _batchCode, iCurrImgSeq, docType);
                                        }
                                        else
                                        {
                                            oDF.reorderScanImageSeq(_currScanProj, _appNum, setNum, collaNum, _batchCode, iCurrImgSeq, docType);
                                        }
                                    }
                                    else
                                    {
                                        if (_fromProc.Trim().ToLower() == "verify")
                                        {
                                            oDF.reorderIndexImageSeq(_currScanProj, _appNum, setNum, collaNum, _batchCode, iCurrImgSeq);
                                        }
                                        else
                                            oDF.reorderScanImageSeq(_currScanProj, _appNum, setNum, collaNum, _batchCode, iCurrImgSeq);
                                    }
                                }
                            }

                            if (msg.Trim() == "" && bReload)
                            {
                                reloadForAddOn();

                                TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                                if (oNodes.Length > 0)
                                {
                                    tvwSet.SelectedNode = oNodes[0];
                                    tvwSet.SelectedNode.EnsureVisible();
                                }

                                if (sDrag.ToLower() != "move")
                                {
                                    rowid = tvwSet.SelectedNode.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                    loadIndexFieldValue("", rowid);
                                }
                            }
                            else if (msg.Trim() != string.Empty)
                                mGlobal.Write2Log("Cut image.." + msg);
                            else
                            {
                                if (sDrag.ToLower() != "move")
                                    loadIndexFieldValue("", rowid);
                            }

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
                            string setNum = node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();
                            string docType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                            string collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                            iCurrImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                            string file = oDF.getDocFilenameDB(rowid, "TDocuScan");
                            if (file.Trim() == string.Empty)
                                file = oDF.getDocFilenameDB(rowid, "TDocuIndex");

                            sImgInfoCopy = rowid + "|" + file + "|" + docType + "|" + collaNum + "|" + setNum + "|" + iCurrImgSeq;
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

        private void setIndexFieldOCRValue(int pIndexId, string pValue)
        {
            try
            {
                if (dOCRPrevValues == null) dOCRPrevValues = new Dictionary<string, string>();

                int iCnt = staMain.stcIndexSetting.RowId.Count;
                int i = 0; Control oCtrl = null; Label oLbl = null; string sKey = string.Empty; string sCtrlType = "";
                while (i < iCnt)
                {
                    sCtrlType = "";

                    if (i == 0)
                    {
                        oCtrl = pnlIdxFld.Controls["txtField"];
                        oLbl = (Label)pnlIdxFld.Controls["lblField"];
                    }
                    else
                    {
                        oCtrl = pnlIdxFld.Controls["txtField" + i];
                        oLbl = (Label)pnlIdxFld.Controls["lblField" + i];
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

                    if (oCtrl == null)
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

                    if (oCtrl != null && oLbl != null)
                    {
                        if (iCnt > 1)
                        {
                            int k = 0;
                            while (k < dOCRPrevValues.Count)
                            {
                                sKey = dOCRPrevValues.ElementAt(k).Key;

                                if (oLbl.Text.ToLower().Trim() == staMain.stcIndexSetting.FieldName[i].Trim().ToLower()
                                        && staMain.stcIndexSetting.FieldZoneOCR[i].ToUpper() == "Y"
                                        && i.ToString() == sKey) //Same name, ZoneOCR is Y, Same index field id.
                                {
                                    if (sCtrlType == "ComboBox")
                                    {
                                        ComboBox oCbx = (ComboBox)oCtrl;
                                        oCbx.SelectedValue = dOCRPrevValues[sKey];
                                    }
                                    else
                                        oCtrl.Text = dOCRPrevValues[sKey];
                                }

                                k += 1;
                            }
                        }

                        if (oLbl.Text.ToLower().Trim() == staMain.stcIndexSetting.FieldName[i].Trim().ToLower()
                                && staMain.stcIndexSetting.FieldZoneOCR[i].ToUpper() == "Y"
                                && i == pIndexId) //Same name, ZoneOCR is Y, Same index field id.
                        {
                            if (sCtrlType == "ComboBox")
                            {
                                ComboBox oCbx = (ComboBox)oCtrl;
                                oCbx.SelectedValue = pValue.Trim();
                                oCtrl.Text = pValue.Trim();
                            }
                            else
                                oCtrl.Text = pValue.Trim();

                            if (dOCRPrevValues.ContainsKey(pIndexId.ToString())) //Update the list.
                                dOCRPrevValues[pIndexId.ToString()] = oCtrl.Text;
                            else
                                dOCRPrevValues.Add(pIndexId.ToString(), oCtrl.Text);

                            break;
                        }
                    }

                    i += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void dsvImg_OnImageAreaSelected(short sImageIndex, int left, int top, int right, int bottom)
        {
            try
            {
                if (sImageIndex < 0)
                {
                    MessageBox.Show("Please select an image at the tree view!");
                    return;
                }

                if (dSelArea == null) dSelArea = new Dictionary<string, int>();

                _filename = staMain.stcImgInfoBySet.ImgFile[sImageIndex];
                FileInfo oFI = new FileInfo(_filename);
                if (oFI.Extension.ToLower() == ".tiff")
                {
                    using (var oInput = new OcrInput())
                    {
                        oInput.LoadImage(_filename);
                        if (oInput.PageCount() > 1)
                        {
                            MessageBox.Show(this, "Multiple pages tiff image is not supported for zone OCR.");
                            return;
                        }
                    }
                }

                if (dSelArea.Count == 0)
                {
                    dSelArea.Add("left", left);
                    dSelArea.Add("top", top);
                    dSelArea.Add("right", right);
                    dSelArea.Add("bottom", bottom);
                }

                System.Drawing.Rectangle oRec = new System.Drawing.Rectangle();
                if (sImageIndex > -1)
                {
                    oRec = dsvImg.GetSelectionRect(sImageIndex);
                    //mGlobal.Write2Log("Area.." + oRec.ToString());
                }

                Cursor.Current = Cursors.WaitCursor;

                IronOcr.License.LicenseKey = MDIMain.strIronLic;

                var Ocr = new IronTesseract();
                Ocr.Language = OcrLanguage.EnglishBest;
                //Ocr.AddSecondaryLanguage(OcrLanguage.Thai);                
                //Ocr.UseCustomTesseractLanguageFile(@"C:\Development\SRDocScanIDP - Demo\bin\Debug\3.02\Tessdata\eng.best.traineddata");
                Ocr.Configuration = new TesseractConfiguration()
                {
                    //EngineMode = TesseractEngineMode.TesseractOnly,
                    ReadBarCodes = true,
                    //RenderSearchablePdfsAndHocr = false,
                    RenderHocr = false,
                    RenderSearchablePdf = false,
                    PageSegmentationMode = TesseractPageSegmentationMode.SparseText

                };

                var ContentArea = new IronSoftware.Drawing.Rectangle(x: oRec.X, y: oRec.Y, width: oRec.Width, height: oRec.Height); //oRec; //new System.Drawing.Rectangle() { X = 215, Y = 1250, Height = 280, Width = 1335 };
                //var ContentArea = new IronSoftware.Drawing.CropRectangle(x: 486, y: 71, width: 266, height: 60);

                string sIsBarcode = string.Empty;
                string sConfid = string.Empty;
                using (var ocrInput = new OcrInput())
                {
                    //ocrInput.AddImage(_filename, ContentArea);
                    ocrInput.LoadImage(_filename, ContentArea);
                    var barcResult = Ocr.Read(ocrInput);

                    if (barcResult.Barcodes.Length > 0)
                    {
                        sIsBarcode = barcResult.Barcodes[0].Value;
                        sConfid = barcResult.Confidence.ToString("000.00");
                    }
                    //foreach (var barcode in barResult.Barcodes)
                    //{
                    //    SRDocScanIDP.mGlobal.Write2Log(barcode.Value);
                    //}
                }

                if (sIsBarcode != string.Empty) //Barcode result.
                {
                    setIndexFieldOCRValue(_indexFocusId, sIsBarcode); //Last selected area in the list.

                    if (String.IsNullOrEmpty(IndexingStatusBar2.Text.Trim()))
                        IndexingStatusBar2.Text = "[" + sConfid + "%]";
                    else
                    {
                        IndexingStatusBar2.Text = "Total Page: " + _totPage;
                        IndexingStatusBar2.Text += " [" + sConfid + "%]";
                    }
                }
                else
                {
                    using (var Input = new OcrInput())
                    {
                        Input.DeNoise();
                        Input.Deskew();
                        Input.Sharpen();
                        Input.TargetDPI = 300;
                        SixLabors.ImageSharp.Image imgSharp = null;
                        //SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> imgSharpPx = null;

                        //OpenCvClient oCV = OpenCvClient.Instance;
                        //List<CropRectangle> textRegion = null;
                        //imgSharp = SixLabors.ImageSharp.Image.Load(_filename);
                        //imgSharpPx = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(_filename);
                        //textRegion = oCV.FindTextRegions(imgSharpPx, 2.0, 20, true, true);
                        //oCV.Dispose();

                        //Input.AddImage(strFileName, ContentArea);

                        //Dimensions are in px
                        //Input.AddImage(oBitmap, ContentArea);
                        //Input.AddImage(oBytes, ContentArea);                    
                        //Input.AddImage(imgSharp, ContentArea);
                        //Input.AddImage((IronSoftware.Drawing.AnyBitmap)imgSharpPx, ContentArea);
                        //Input.AddImage(_filename, ContentArea);
                        Input.LoadImage(_filename, ContentArea);

                        //txtField.Text = "";

                        var Result = Ocr.Read(Input);
                        //Console.WriteLine(Result.Text);
                        //mGlobal.Write2Log("Area.." + Result.Text);
                        //txtField.Text = Result.Text;
                        //txtField.Text = Ocr.Read(Input).Text;
                        setIndexFieldOCRValue(_indexFocusId, Result.Text); //Last selected area in the list.

                        if (String.IsNullOrEmpty(IndexingStatusBar2.Text.Trim()))
                            IndexingStatusBar2.Text = "[" + Result.Confidence.ToString("000.00") + "%]";
                        else
                        {
                            IndexingStatusBar2.Text = "Total Page: " + _totPage;
                            IndexingStatusBar2.Text += " [" + Result.Confidence.ToString("000.00") + "%]";
                        }

                        //Input.Dispose();

                        if (imgSharp != null) imgSharp.Dispose();
                        //if (imgSharpPx != null) imgSharpPx.Dispose();
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Area.." + ex.Message);
                mGlobal.Write2Log("Area.." + ex.StackTrace.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void dsvImg_OnImageAreaDeselected(short sImageIndex)
        {
            dSelArea.Clear();
        }

        private void indexField_Focus(object sender, EventArgs e)
        {
            try
            {
                if (sender != null)
                    _indexFocusId = Int32.Parse(((Control)sender).Tag.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        private void keyField_Focus(object sender, EventArgs e)
        {
            try
            {
                if (sender != null)
                    _keyFocusId = Int32.Parse(((Control)sender).Tag.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        private void btnZoneStrip_Click(object sender, EventArgs e)
        {
            dsvImg.MouseShape = false;
            dsvImg.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;
        }

        private void btnHandleStrip_Click(object sender, EventArgs e)
        {
            dsvImg.MouseShape = true;
            dsvImg.Annotation.Type = Dynamsoft.Forms.Enums.EnumAnnotationType.enumNone;
        }

        private void mnuInsStrip_Click(object sender, EventArgs e)
        {
            TreeNode node = null;
            try
            {
                string nodeInfo = "";
                string sep = "";
                string sDir = "";
                string sFileName = "";
                string sFileExt = "tiff";

                if (tvwSet.SelectedNode != null)
                {
                    node = tvwSet.SelectedNode;
                    selectedNode = node;

                    btnSaveStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnRescanToolStrip.Enabled = false;
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

                        //string sBaseDir = staMain.stcProjCfg.WorkDir;

                        //if (_fromProc.Trim().ToLower() == "verify")
                        //    sBaseDir = staMain.stcProjCfg.IndexDir;

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");

                        //if (_batchType.Trim().ToLower() == "set")
                        //{
                        //    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";
                        //    //if (_docType.Trim() == "")
                        //    //{
                        //    //    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //    //}
                        //    //else
                        //    //    sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        //}
                        //else //Batch.
                        //{
                        //    if (_docType.Trim() == "")
                        //    {
                        //        sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                        //    }
                        //    else
                        //        sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        //}

                        //if (staMain.stcProjCfg.BatchAuto.ToUpper() == "Y")
                        //    sFileName = sBatchPath + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;
                        //else
                        //    sFileName = sBatchPath + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;
                        sFileName = sBatchPath + @"_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "." + sFileExt;

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
                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                if (selectedNode != null)
                {
                    TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                    if (oNodes.Length > 0)
                    {
                        tvwSet.SelectedNode = oNodes[0];
                        tvwSet.SelectedNode.EnsureVisible();
                    }
                }

                //tvwSet.Refresh();
            }
        }

        public void getScannerCustomSettings()
        {
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 source,pixeltype,resolution ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TCustomScanner ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    sCustSrc = dr[0].ToString();
                    sCustPixelType = dr[1].ToString();
                    iCustResolutn = Convert.ToInt32(dr[2].ToString());
                    bIsDuplexEnabled = false;

                    mbCustomScan = true;
                }
            }
            catch (Exception ex)
            {
                mbCustomScan = false;
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
                    btnSaveStrip.Enabled = true;
                    btnSave.Enabled = true;
                    btnNext.Enabled = true;
                    btnRescanToolStrip.Enabled = true;
                    btnDeleteStrip.Enabled = true;
                    btnDeleteAllStrip.Enabled = true;

                    return false;
                }

                getScannerCustomSettings();

                // Select the source for TWAIN
                var srcIndex = -1;
                for (short i = 0; i < oTM.SourceCount; i++)
                {

                    if (mbCustomScan && oTM.SourceNameItems(i).Trim() == sCustSrc.Trim())
                    {
                        srcIndex = i;
                        break;
                    }
                    else if (oTM.SourceNameItems(i) != "") continue;

                    srcIndex = i;
                    break;
                }

                if (oTM.SourceCount < 0)
                {
                    MessageBox.Show(this, "There is no scanner detected!\n " +
                        "Please select scanner source at menu File>Scanner Source.", "Information");

                    return false;
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

                oTM.AlwaysOnTop = false;

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

                btnSaveStrip.Enabled = false;
                btnSave.Enabled = false;
                btnNext.Enabled = false;
                btnRescanToolStrip.Enabled = false;
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
                if (!bRet)
                {
                    btnSaveStrip.Enabled = true;
                    btnSave.Enabled = true;
                    btnNext.Enabled = true;
                    btnRescanToolStrip.Enabled = true;
                    btnDeleteStrip.Enabled = true;
                    btnDeleteAllStrip.Enabled = true;
                }

                if (oTM != null) oTM.Dispose();
            }
            return bRet;
        }

        private void btnNoticeStrip_Click(object sender, EventArgs e)
        {
            frmNotification fmNotify = new frmNotification();
            fmNotify.Show(this);
            fmNotify.loadNotification(_batchCode);
            fmNotify.Focus();
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

                if (tvwSet.SelectedNode != null && _noSeparator.Trim() != "0") //Only allow 1 or 2 separator for insert doctype.
                {
                    node = tvwSet.SelectedNode;
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

                    btnSaveStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnRescanToolStrip.Enabled = false;
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
                        iCurrImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                        string sBaseDir = staMain.stcProjCfg.WorkDir;

                        if (_fromProc.Trim().ToLower() == "verify")
                            sBaseDir = staMain.stcProjCfg.IndexDir;

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        string sDir = "";

                        if (_batchType.Trim().ToLower() == "set")
                        {
                            sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";
                        }
                        else //Batch.
                        {
                            if (_docType.Trim() == "")
                            {
                                sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            }
                            else
                                sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }

                        if (nodeInfo.Trim().ToLower() == "page")
                        {
                            if (node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                sSrcFileName = oDF.getDocFilenameDB(rowid, "TDocuScan");
                                if (sSrcFileName.Trim() == string.Empty)
                                    sSrcFileName = oDF.getDocFilenameDB(rowid, "TDocuIndex");

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

                                    DataRowCollection oDrs = getDocTypeDocScanDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrDocType, rowid, iCurrImgSeq);

                                    FileInfo oFi = null;
                                    int i = 0, s = 0;
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
                                        {
                                            oDF.reorderIndexImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, 1, _docType);
                                        }
                                        else
                                            oDF.reorderScanImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, 1, _docType);

                                        //Refresh treeview.
                                        //loadBatchSetTreeview("'1'", "'S','R','A'");
                                        reloadForAddOn(); //Refresh.
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
                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                if (oNodes.Length > 0)
                {
                    tvwSet.SelectedNode = oNodes[0];
                    tvwSet.SelectedNode.EnsureVisible();
                }

                //tvwSet.Refresh();
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

        private bool updateDocTypeDocScanDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocTypeOld, string pSetNumNew, string pDocTypeNew, string pRowId, string pNewFilename)
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
                sSQL += "SET setnum='" + pSetNumNew.Trim().Replace("'", "") + "' ";
                sSQL += ",collanum='" + _collaNum.Trim().Replace("'", "") + "' ";
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
                    //throw new Exception("Saving data failure!");
                }

            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private DataRowCollection getBatchSetByRowIDDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum, string pRowId, int pCurrImgSeq)
        {
            string sSQL;
            string sTable = "TDocuScan";
            try
            {
                if (_fromProc.Trim().ToLower() == "verify")
                    sTable = "TDocuIndex";

                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT DISTINCT rowid,docimage,imageseq,setnum,doctype FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + sTable + " ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND rowid>='" + pRowId.Trim().Replace("'", "") + "' ";
                sSQL += "AND imageseq>='" + pCurrImgSeq + "' ";
                sSQL += "UNION ALL ";
                sSQL += "SELECT DISTINCT rowid,docimage,imageseq,setnum,doctype FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + sTable + " ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum>'" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY setnum,doctype,imageseq,rowid ";

                return mGlobal.objDB.ReturnRows(sSQL);
            }
            catch (Exception ex)
            {
            }

            return null;
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

                sSQL = "SELECT DISTINCT rowid,docimage,imageseq,setnum FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + sTable + " ";
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

                if (tvwSet.SelectedNode != null && _noSeparator.Trim() != "0") //Only allow 1 or 2 separator for insert doctype.
                {
                    node = tvwSet.SelectedNode;
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

                    btnSaveStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnRescanToolStrip.Enabled = false;
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
                        iCurrImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());

                        string sBaseDir = staMain.stcProjCfg.WorkDir;

                        if (_fromProc.Trim().ToLower() == "verify")
                            sBaseDir = staMain.stcProjCfg.IndexDir;

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        string sDir = "";

                        if (_batchType.Trim().ToLower() == "set")
                        {
                            sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum.Trim() + @"\";
                        }
                        else //Batch.
                        {
                            if (_docType.Trim() == "")
                            {
                                sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\";
                            }
                            else
                                sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + _setNum + @"\" + _docType + @"\";
                        }

                        if (nodeInfo.Trim().ToLower() == "page")
                        {
                            if (node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                rowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                sSrcFileName = oDF.getDocFilenameDB(rowid, "TDocuScan");
                                if (sSrcFileName.Trim() == string.Empty)
                                    sSrcFileName = oDF.getDocFilenameDB(rowid, "TDocuIndex");

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

                                    DataRowCollection oDrs = getDocTypeDocScanDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrDocType, rowid, iCurrImgSeq);

                                    FileInfo oFi = null;
                                    int i = 0, s = 0;
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
                                        {
                                            oDF.reorderIndexImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, 1, _docType);
                                        }
                                        else
                                            oDF.reorderScanImageSeq(_currScanProj, _appNum, _setNum, _collaNum, _batchCode, 1, _docType);

                                        //Refresh treeview.
                                        //loadBatchSetTreeview("'1'", "'S','R','A'");
                                        reloadForAddOn(); //Refresh.
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
                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                if (oNodes.Length > 0)
                {
                    tvwSet.SelectedNode = oNodes[0];
                    tvwSet.SelectedNode.EnsureVisible();
                }

                //tvwSet.Refresh();
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
                        //X = left, Y = Top
                        int iLeft = boundingrect.Left;// (int)mGlobal.convert2Points(dsvImg, boundingrect.Left, "X");
                        int iTop = boundingrect.Top;// (int)mGlobal.convert2Points(dsvImg, boundingrect.Top, "Y");

                        rectAnnotation.StartPoint = new System.Drawing.Point(iLeft, iTop);
                        rectAnnotation.EndPoint = new System.Drawing.Point((iLeft + boundingrect.Size.Width), (iTop + boundingrect.Size.Height));
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
                        textAnnotation.StartPoint = new System.Drawing.Point(iLeft, (int)(iTop - textSize.Height * 1.25f));
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

        private void loadValueList(string pPrjCode, string pAppNum, string pDDId, string pListType, string pFieldName, ComboBox pValueList)
        {
            try
            {
                string sSQL = "";
                try
                {
                    mGlobal.LoadAppDBCfg();

                    sSQL = "SELECT [fldname],[valname],[namevalue],[namedesc] ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TValueList ";
                    sSQL += "WHERE scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND docdefid='" + pDDId.Trim().Replace("'", "") + "' ";
                    sSQL += "AND listtype='" + pListType.Trim().Replace("'", "") + "' ";
                    sSQL += "AND fldname='" + pFieldName.Trim().Replace("'", "''") + "' ";

                    DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                    if (drs != null)
                    {
                        pValueList.DataSource = null;
                        pValueList.Items.Clear();
                        int i = 0; Dictionary<string, string> dItem = new Dictionary<string, string>();

                        dItem.Add("", "");

                        while (i < drs.Count)
                        {
                            dItem.Add(drs[i][1].ToString(), drs[i][2].ToString());

                            i += 1;
                        }

                        pValueList.DataSource = new BindingSource(dItem, null);
                        pValueList.DisplayMember = "Key";
                        pValueList.ValueMember = "Value";
                        pValueList.Refresh();
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void lookupField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;
            try
            {
                if (sender != null && e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    Control oCtrl = (Control)sender;

                    if (oCtrl.Text != string.Empty)
                    {
                        int iIdx = Convert.ToInt32(oCtrl.Tag.ToString()); //Dependency field.
                        string sFldName = "";
                        Control[] oCtrls = null;
                        if (iIdx == staMain.stcIndexSetting.FieldName.Count)
                        {
                            sFldName = staMain.stcIndexSetting.FieldName[iIdx - 1].ToString(); //Next field, but last field.
                            oCtrls = pnlIdxFld.Controls.Find("txtField" + (iIdx - 1), true); //Next control.
                        }
                        else
                        {
                            sFldName = staMain.stcIndexSetting.FieldName[iIdx + 1].ToString(); //Next field.
                            oCtrls = pnlIdxFld.Controls.Find("txtField" + (iIdx + 1), true); //Next control.
                        }

                        if (oCtrls != null)
                        {
                            mGlobal.LoadAppDBCfg();

                            string sSQL = "SELECT TOP 1 [fldname],[valname],[namevalue],[namedesc] ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TValueList ";
                            sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                            sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                            sSQL += "AND listtype='Lookup' ";
                            sSQL += "AND fldname='" + sFldName.Trim().Replace("'", "''") + "' ";
                            sSQL += "AND valname='" + oCtrl.Text.Trim().Replace("'", "") + "' ";

                            DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                            if (dr != null)
                            {
                                oCtrls[0].Text = dr[2].ToString();
                            }
                        }
                    }
                }
                else
                    e.Handled = true;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Lookup changed.." + ex.Message);
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

        private bool updatePageIndexFieldDB(string pSetNum, string pOldPageId, string pPageId)
        {
            try
            {
                oDF.updatePageIndexFieldDB(_currScanProj, _appNum, _batchCode, pSetNum, _docDefId, pOldPageId, pPageId);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private bool syncPageIndexFieldDB(string pBatchCode, string pTableName, string pPageId = "NULL")
        {
            try
            {
                mGlobal.LoadAppDBCfg();

                string sSQL = "SELECT IsNULL(COUNT(*),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pPageId.Trim().ToUpper() == "NULL")
                    sSQL += "AND pageid IS NULL ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    if (Convert.ToInt32(dr[0].ToString()) == 0) return false;
                }

                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT rowid,setnum,imageseq ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + pTableName.Trim().Replace("'", "") + " ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY imageseq,createddate DESC ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    mGlobal.LoadAppDBCfg();

                    int i = 0, iImgSeq; string sPageId = "", sSetNo = "";
                    while (i < drs.Count)
                    {
                        sPageId = drs[i][0].ToString();
                        sSetNo = drs[i][1].ToString();
                        iImgSeq = Convert.ToInt32(drs[i][2].ToString());
                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0")
                            sSetNo = iImgSeq.ToString(sSetNumFmt);

                        sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                        sSQL += "SET pageid=" + sPageId + " ";
                        sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                        sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                        sSQL += "AND setnum='" + sSetNo.Trim().Replace("'", "") + "' ";
                        if (pPageId.Trim().ToUpper() == "NULL")
                            sSQL += "AND pageid IS NULL ";

                        mGlobal.objDB.UpdateRows(sSQL);

                        i += 1;
                    }
                }

                dr = null;
                drs = null;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private bool syncDocIndexFieldPageIDDB(string pNewSetNum, string pPageId)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            string sSetNum = "";
            string sFieldId = "";
            string sValue = "";
            string sPageId = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT setnum,fieldid,fldvalue,pageid FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                sSQL += "AND pageid='" + pPageId.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY setnum,fieldid ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    int j = 0;
                    while (j < drs.Count)
                    {
                        sSetNum = drs[j][0].ToString();
                        sFieldId = drs[j][1].ToString();
                        sValue = drs[j][2].ToString();
                        sPageId = drs[j][3].ToString();

                        bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, sSetNum, _docDefId, sFieldId, sPageId);

                        mGlobal.LoadAppDBCfg();

                        if (bExist)
                        {
                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                            sSQL += "SET setnum='" + pNewSetNum.Trim().Replace("'", "") + "' ";
                            sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                            sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND setnum='" + sSetNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                            sSQL += "AND fieldid=" + sFieldId + " ";
                            sSQL += "AND pageid=" + sPageId + " ";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                //throw new Exception("Update document index data failed!");
                            }
                        }
                        //else
                        //{
                        //    sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                        //    sSQL += "([scanpjcode]";
                        //    sSQL += ",[sysappnum]";
                        //    sSQL += ",[docdefid]";
                        //    sSQL += ",[setnum]";
                        //    sSQL += ",[batchcode]";
                        //    sSQL += ",[fieldid]";
                        //    sSQL += ",[fldvalue]";
                        //    sSQL += ",[pageid]";
                        //    sSQL += ") VALUES (";
                        //    sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                        //    sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                        //    sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                        //    sSQL += ",'" + sSetNum.Trim().Replace("'", "") + "'";
                        //    sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                        //    sSQL += "," + sFieldId + "";
                        //    sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                        //    sSQL += ",'" + sPageId + "')";

                        //    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                        //    {
                        //        bSaved = false;
                        //        //throw new Exception("Saving document index data failed!");
                        //    }
                        //}

                        j += 1;
                    }
                }

                drs = null;
            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool syncDocIndexFieldDB()
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            string sSetNum = "";
            string sFieldId = "";
            string sValue = "";
            string sPageId = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT setnum,fieldid,fldvalue,pageid FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY setnum,fieldid ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    int j = 0;
                    while (j < drs.Count)
                    {
                        sSetNum = drs[j][0].ToString();
                        sFieldId = drs[j][1].ToString();
                        sValue = drs[j][2].ToString();
                        sPageId = drs[j][3].ToString();

                        bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, sSetNum, _docDefId, sFieldId, sPageId);

                        mGlobal.LoadAppDBCfg();

                        if (bExist)
                        {
                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                            sSQL += "SET [pageid]='" + sPageId.Trim().Replace("'", "") + "' ";
                            sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                            sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                            sSQL += "AND setnum='" + sSetNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                            sSQL += "AND fieldid=" + sFieldId + " ";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Update document index data failed!");
                            }
                        }
                        else
                        {
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
                            sSQL += ",'" + sSetNum.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                            sSQL += "," + sFieldId + "";
                            sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",'" + sPageId + "')";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Saving document index data failed!");
                            }
                        }

                        j += 1;
                    }
                }

                drs = null;
            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool saveDocIndexFieldDB(string pSetNum, string pNewSetNum, int pPageId = 0)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            int i = 0;
            Control oCtrl = null;
            string sValue = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, pSetNum, _docDefId, "", pPageId.ToString());

                if (bExist)
                {
                    int iId = 0;
                    while (i < staMain.stcIndexSetting.RowId.Count)
                    {
                        iId = staMain.stcIndexSetting.RowId[i];
                        if (i == 0)
                            oCtrl = pnlIdxFld.Controls["txtField"];
                        else
                            oCtrl = pnlIdxFld.Controls["txtField" + i];

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlIdxFld.Controls["dtpField"];
                            else
                                oCtrl = pnlIdxFld.Controls["dtpField" + i];
                        }

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlIdxFld.Controls["cbxField"];
                            else
                                oCtrl = pnlIdxFld.Controls["cbxField" + i];

                            ComboBox oCbx = (ComboBox)oCtrl;
                            if (oCbx.SelectedValue != null)
                                sValue = oCbx.SelectedValue.ToString();
                            else
                            {
                                MessageBox.Show(this, "Value is not selected or not matched! Please select from the list.", "Message");
                                throw new Exception("Value is not selected or not matched!");
                            }
                        }
                        else
                            sValue = oCtrl.Text;

                        bExist = oDF.checkDocuSetIndexFieldExist(_currScanProj, _appNum, _batchCode, pNewSetNum, _docDefId, iId.ToString(), pPageId.ToString());

                        if (bExist)
                        {
                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                            sSQL += "SET [fldvalue]=N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",[modifieddate]='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                            sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                            sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND setnum='" + pNewSetNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                            sSQL += "AND docdefid='" + _docDefId.Trim().Replace("'", "") + "' ";
                            sSQL += "AND fieldid=" + iId + " ";
                            sSQL += "AND pageid='" + pPageId + "' ";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Update document index data failed!");
                            }
                        }
                        else
                        {
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
                            sSQL += ",'" + pNewSetNum.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                            sSQL += "," + iId + "";
                            sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",'" + pPageId + "')";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Saving document index data failed!");
                            }
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
                            oCtrl = pnlIdxFld.Controls["txtField"];
                        else
                            oCtrl = pnlIdxFld.Controls["txtField" + i];

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlIdxFld.Controls["dtpField"];
                            else
                                oCtrl = pnlIdxFld.Controls["dtpField" + i];
                        }

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlIdxFld.Controls["cbxField"];
                            else
                                oCtrl = pnlIdxFld.Controls["cbxField" + i];

                            ComboBox oCbx = (ComboBox)oCtrl;
                            if (oCbx.SelectedValue != null)
                                sValue = oCbx.SelectedValue.ToString();
                            else
                            {
                                MessageBox.Show(this, "Value is not selected or not matched! Please select from the list.", "Message");
                                throw new Exception("Value is not selected or not matched!");
                            }
                        }
                        else
                            sValue = oCtrl.Text;

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
                        sSQL += ",'" + pNewSetNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                        sSQL += "," + iId + "";
                        sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                        sSQL += ",'" + pPageId + "')";

                        if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                        {
                            bSaved = false;
                            throw new Exception("Saving document index data failed!!");
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

        private void btnSendStrip_Click(object sender, EventArgs e)
        {
            string strMsg = "";

            try
            {
                _batchCode = staMain.stcImgInfoBySet.BatchCode;

                if (MessageBox.Show("Confirm to send this document set " + _batchCode + " for verification?", "Confirmation",
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }

                _isSaved = false;
                strMsg = ""; //validateFields();

                if (strMsg.Trim() == "")
                {
                    btnSaveStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnRescanToolStrip.Enabled = false;

                    _setNum = staMain.stcImgInfoBySet.SetNum[0];
                    _batchCode = staMain.stcImgInfoBySet.BatchCode;
                    _indexEnd = DateTime.Now;

                    IndexingStatusBar.Text = "Reindexing";

                    //if (updateDocIndexDB(txtField1.Text.Trim(), txtField2.Text.Trim(), "R"))
                    if (updateDocIndexDB("I"))
                    {
                        bool bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "8", "2", "", "", txtRemarks.Text.Trim(), "Index");

                        if (_batchStatus.Trim() == "6")
                            bOK = oDF.updateBatchStatusDB(_currScanProj, _appNum, _batchCode, "6", "2", "", "", txtRemarks.Text.Trim(), "Index");

                        if (bOK)
                        {
                            _isSaved = true;
                            MessageBox.Show("Document Set " + _batchCode + " sent successfully!", "Send reindex Document");

                            this.Close();
                            //btnNext_Click(this, e);
                        }
                    }
                    else
                        MessageBox.Show("Document Set " + _batchCode + " send unsuccessful!", "Send reindex Document");

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
                IndexingStatusBar.Text = "Indexing";
                if (_batchStatus == "8")
                    IndexingStatusBar.Text = "Reindexing";

                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
            }
        }

        private void mnuInsDocSepStrip_Click(object sender, EventArgs e)
        {
            TreeNode node = null;
            try
            {
                if (_batchType.Trim().ToLower() == "set" && _noSeparator.Trim() != "1")
                {
                    MessageBox.Show(this, "Batch Set Separator is not supported!", "Message");
                    return;
                }
                else if (_noSeparator.Trim() == "0" || _noSeparator.Trim() == "2")
                {
                    MessageBox.Show(this, "Insert document separator is not available for non separator or two separators!", "Message");
                    return;
                }

                int iSetNum = 0;
                string sOldSetnum = "";
                string sOldDocType = "";
                string nodeInfo = "";
                string sSrcFileName = "";
                string sNewFileName = "";

                if (tvwSet.SelectedNode != null && _noSeparator.Trim() != "0") //Only allow 1 separator for insert document separator.
                {
                    node = tvwSet.SelectedNode;
                    selectedNode = node;

                    if (_noSeparator.Trim() == "1" && node.Level != 2)
                    {
                        MessageBox.Show(this, "Please select a page to insert document separator!", "Message");
                        return;
                    }
                    else if (selectedNode.Text.ToLower() == "page 1")
                    {
                        MessageBox.Show(this, "Page One is not allowed to split document set!", "Message");
                        return;
                    }

                    btnSaveStrip.Enabled = false;
                    btnSave.Enabled = false;
                    btnNext.Enabled = false;
                    btnRescanToolStrip.Enabled = false;
                    btnDeleteStrip.Enabled = false;
                    btnDeleteAllStrip.Enabled = false;
                    bool bOK = true;

                    if (selectedNode != null)
                    {
                        nodeInfo = node.Text.Trim().Substring(0, 4);
                        //sep = node.Tag.ToString().Trim().Substring(0, 4);                        

                        string sCurrRowid = "";
                        string rowid = "";
                        sImgInfoCopy = "";

                        sCurrDocType = node.Tag.ToString().Trim().Split('|').GetValue(2).ToString();
                        sCurrSetNum = node.Tag.ToString().Trim().Split('|').GetValue(4).ToString();
                        _setNum = Convert.ToInt32(sCurrSetNum).ToString(sSetNumFmt);
                        _collaNum = node.Tag.ToString().Trim().Split('|').GetValue(5).ToString();
                        iCurrImgSeq = Convert.ToInt32(node.Tag.ToString().Trim().Split('|').GetValue(6).ToString());
                        _docType = staMain.stcProjCfg.SeparatorText;

                        string sBaseDir = staMain.stcProjCfg.WorkDir;

                        if (_fromProc.Trim().ToLower() == "verify")
                            sBaseDir = staMain.stcProjCfg.IndexDir;

                        string sBatchPath = mGlobal.replaceValidChars(_batchCode.Trim(), "_");
                        string sDir = "";

                        if (nodeInfo.Trim().ToLower() == "page")
                        {
                            if (node.Tag.ToString().Trim().Split('|').Length > 1)
                            {
                                sCurrRowid = node.Tag.ToString().Trim().Split('|').GetValue(3).ToString();
                                sSrcFileName = oDF.getDocFilenameDB(sCurrRowid, "TDocuScan");
                                if (sSrcFileName.Trim() == string.Empty)
                                    sSrcFileName = oDF.getDocFilenameDB(sCurrRowid, "TDocuIndex");

                                sImgInfoCopy = sCurrRowid + "|" + sSrcFileName;
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

                                    DataRowCollection oDrs = getBatchSetByRowIDDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrRowid, iCurrImgSeq);

                                    FileInfo oFi = null;
                                    int i = 0, s = 0;
                                    while (i < oDrs.Count)
                                    {
                                        rowid = oDrs[i]["rowid"].ToString();
                                        sSrcFileName = oDrs[i]["docimage"].ToString();
                                        iSetNum = Convert.ToInt32(oDrs[i]["setnum"].ToString()); //setnum
                                        iSetNum += 1;
                                        sOldSetnum = oDrs[i]["setnum"].ToString();
                                        sOldDocType = oDrs[i]["doctype"].ToString();

                                        if (_batchType.Trim().ToLower() == "set")
                                            sOldDocType = sOldSetnum;
                                        else
                                            sOldDocType = "";

                                        if (_fromProc.Trim().ToLower() == "verify")
                                        {
                                            if (oDF.checkDocuIndexExist(_currScanProj, _appNum, _batchCode, iSetNum.ToString(sSetNumFmt), sOldDocType)) //New DocType
                                            {
                                                iCollaNum = oDF.getCurrCollaNumIndex(_currScanProj, _appNum, _batchCode, iSetNum.ToString(sSetNumFmt), sOldDocType);
                                                if (iCollaNum < 1) iCollaNum = 1;
                                            }
                                            else
                                                iCollaNum += 1;
                                        }
                                        else
                                        {
                                            if (oDF.checkDocuScanExist(_currScanProj, _appNum, _batchCode, iSetNum.ToString(sSetNumFmt), sOldDocType)) //New DocType
                                            {
                                                iCollaNum = oDF.getCurrCollaNum(_currScanProj, _appNum, _batchCode, iSetNum.ToString(sSetNumFmt), sOldDocType);
                                                if (iCollaNum < 1) iCollaNum = 1;
                                            }
                                            else
                                                iCollaNum += 1;
                                        }

                                        _collaNum = iCollaNum.ToString(sCollaNumFmt);

                                        if (_batchType.Trim().ToLower() == "set")
                                        {
                                            sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + iSetNum.ToString(sSetNumFmt) + @"\";
                                        }
                                        else //Batch.
                                        {
                                            if (_docType.Trim().ToUpper() == staMain.stcProjCfg.SeparatorText.ToUpper())
                                            {
                                                sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + iSetNum.ToString(sSetNumFmt) + @"\";
                                            }
                                            else
                                                sDir = mGlobal.addDirSep(sBaseDir) + MDIMain.strScanProjectApp.Trim() + @"\" + sBatchPath.Trim() + @"\" + iSetNum.ToString(sSetNumFmt) + @"\" + _docType + @"\";
                                        }

                                        oFi = new FileInfo(sSrcFileName);
                                        sNewFileName = mGlobal.addDirSep(sDir) + oFi.Name;

                                        try
                                        {
                                            bOK = true;
                                            if (!Directory.Exists(sDir)) Directory.CreateDirectory(sDir);

                                            File.Move(sSrcFileName, sNewFileName);
                                        }
                                        catch (Exception ex)
                                        {
                                            bOK = false;
                                            mGlobal.Write2Log("Insert separator:" + ex.Message);
                                        }

                                        if (bOK)
                                        {
                                            if (_batchType.Trim().ToLower() == "set")
                                                bOK = updateDocTypeDocScanDB(_currScanProj, _appNum, _batchCode, sOldSetnum, sOldSetnum, iSetNum.ToString(sSetNumFmt), iSetNum.ToString(sSetNumFmt), rowid, sNewFileName);
                                            else
                                                bOK = updateDocTypeDocScanDB(_currScanProj, _appNum, _batchCode, sOldSetnum, sCurrDocType, iSetNum.ToString(sSetNumFmt), string.Empty, rowid, sNewFileName);

                                            if (bOK)
                                            {
                                                if (_fromProc.Trim().ToLower() == "verify")
                                                {
                                                    if (_batchType.Trim().ToLower() == "set")
                                                        _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, iSetNum.ToString(sSetNumFmt));
                                                    else
                                                        _totPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                                                }
                                                else
                                                {
                                                    if (_batchType.Trim().ToLower() == "set")
                                                        _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, iSetNum.ToString(sSetNumFmt));
                                                    else
                                                        _totPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                                                }

                                                saveBatchDB(_currScanProj, _appNum, _batchCode, sOldSetnum, iSetNum.ToString(sSetNumFmt), _totPage);

                                                s += 1;
                                            }
                                        }
                                        else
                                        {
                                            //"File(s) move error! Please check with system person in charge." 
                                            mGlobal.Write2Log("File(s) move error! " + sSrcFileName);
                                            //break;
                                        }

                                        sCollaNumFmt = "000";
                                        iCollaNum = 0;

                                        i += 1;
                                    } //End While

                                    if (s > 0)
                                    {
                                        //Reorder image sequence after all insert and update records.
                                        oDrs = getBatchSetByRowIDDB(_currScanProj, _appNum, _batchCode, sCurrSetNum, sCurrRowid, 1);
                                        i = 0;
                                        while (i < oDrs.Count)
                                        {
                                            rowid = oDrs[i]["rowid"].ToString();
                                            iSetNum = Convert.ToInt32(oDrs[i]["setnum"].ToString()); //setnum

                                            if (_fromProc.Trim().ToLower() == "verify")
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                {
                                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, iSetNum.ToString(sSetNumFmt), _collaNum, _batchCode, 1);
                                                }
                                                else
                                                    oDF.reorderIndexImageSeq(_currScanProj, _appNum, iSetNum.ToString(sSetNumFmt), _collaNum, _batchCode, 1, string.Empty);
                                            }
                                            else
                                            {
                                                if (_batchType.Trim().ToLower() == "set")
                                                {
                                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, iSetNum.ToString(sSetNumFmt), _collaNum, _batchCode, 1);
                                                }
                                                else
                                                    oDF.reorderScanImageSeq(_currScanProj, _appNum, iSetNum.ToString(sSetNumFmt), _collaNum, _batchCode, 1, string.Empty);
                                            }

                                            syncDocIndexFieldPageIDDB(iSetNum.ToString(sSetNumFmt), rowid); //if any.

                                            i += 1;
                                        }

                                        //Refresh treeview.
                                        //loadBatchSetTreeview("'1'", "'S','R','A'");
                                        reloadForAddOn(); //Refresh.
                                    }
                                    selectedNode = node;
                                }
                                else
                                    MessageBox.Show(this, "Insert the same document separator for the same document type is not allowed!" + Environment.NewLine
                                        + "Please use another different document separator.", "Message");
                            }
                        }
                        else
                            MessageBox.Show(this, "Please select page for insert a document separator.", "Message");
                    }
                    else
                        MessageBox.Show(this, "Error! " + sErrMsg, "Message");
                }
                else
                    MessageBox.Show(this, "Please select a page to insert a document separator.", "Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Insert separator.." + ex.Message);
            }
            finally
            {
                btnSaveStrip.Enabled = true;
                btnSave.Enabled = true;
                btnNext.Enabled = true;
                btnRescanToolStrip.Enabled = true;
                btnDeleteStrip.Enabled = true;
                btnDeleteAllStrip.Enabled = true;

                if (selectedNode != null)
                {
                    TreeNode[] oNodes = tvwSet.Nodes.Find(selectedNode.Name, true);
                    if (oNodes.Length > 0)
                    {
                        tvwSet.SelectedNode = oNodes[0];
                        tvwSet.SelectedNode.EnsureVisible();
                    }
                }

                //tvwSet.Refresh();
            }
        }

        private bool saveBatchDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pOldSetNum, string pSetNum, int pTotPageCnt)
        {
            bool bSaved = true;
            string sSQL = "";
            string sBatchNum = "";
            bool bExist = false;
            int iOldTotPage = 0;
            try
            {
                if (oDF.checkBatchExist(pCurrScanProj, pAppNum, pBatchCode, pSetNum, _batchType)) bExist = true; //Check is duplicate not allowed.

                if (bExist && _batchType.Trim().ToLower() == "batch")
                {
                    if (_fromProc.Trim().ToLower() == "verify")
                    {
                        iOldTotPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                    }
                    else
                    {
                        iOldTotPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                    }

                    mGlobal.LoadAppDBCfg();

                    if (mGlobal.objDB == null)
                    {
                        mGlobal.objDB = new clsDatabase();
                    }

                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "SET totpagecnt=" + iOldTotPage.ToString() + " ";
                    sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND setnum='" + pOldSetNum.Trim().Replace("'", "") + "' ";

                    //mGlobal.Write2Log(sSQL);

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                    }

                    mGlobal.LoadAppDBCfg();

                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "SET totpagecnt=" + pTotPageCnt.ToString() + " ";
                    sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                    //mGlobal.Write2Log(sSQL);

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                    }

                    return bSaved;
                }
                else if (bExist && _batchType.Trim().ToLower() == "set")
                {
                    if (_fromProc.Trim().ToLower() == "verify")
                    {
                        if (_batchType.Trim().ToLower() == "set")
                            iOldTotPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode, pOldSetNum);
                        else
                            iOldTotPage = oDF.getTotPageCntIndex(_currScanProj, _appNum, _batchCode);
                    }
                    else
                    {
                        if (_batchType.Trim().ToLower() == "set")
                            iOldTotPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode, pOldSetNum);
                        else
                            iOldTotPage = oDF.getTotPageCnt(_currScanProj, _appNum, _batchCode);
                    }

                    mGlobal.LoadAppDBCfg();

                    if (mGlobal.objDB == null)
                    {
                        mGlobal.objDB = new clsDatabase();
                    }

                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "SET totpagecnt=" + iOldTotPage.ToString() + " ";
                    sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND setnum='" + pOldSetNum.Trim().Replace("'", "") + "' ";

                    //mGlobal.Write2Log(sSQL);

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                    }

                    mGlobal.LoadAppDBCfg();

                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "SET totpagecnt=" + pTotPageCnt.ToString() + " ";
                    sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                    //mGlobal.Write2Log(sSQL);

                    if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                    {
                        bSaved = false;
                    }

                    return bSaved;
                }

                sBatchNum = oDF.getBatchCodeNum(pCurrScanProj, pAppNum, pBatchCode);

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
                sSQL += ") VALUES (";
                sSQL += "'" + pCurrScanProj.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pAppNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + sBatchNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pBatchCode.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _batchType.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _batchStatus.Trim().Replace("'", "") + "'";
                sSQL += "," + pTotPageCnt + "";
                sSQL += ",'" + _boxNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + string.Empty.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _docDefId.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _docType.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _station.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _userId.Trim().Replace("'", "") + "'";
                sSQL += ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";

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

        private void btnRescanToolStrip_Click(object sender, EventArgs e)
        {
            try
            {
                btnReject_Click(sender, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void frmExtraction_Activated(object sender, EventArgs e)
        {
            try
            {
                enableScan(false);
                if (_isSaved)
                    staMain.sessionStart();
                else
                    staMain.sessionStop(false);
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

        private void loadValueListDS(string pPrjCode, string pAppNum, string pDDId, string pListType, string pFieldName, string pSource, ComboBox pValueList)
        {
            try
            {
                string sSQL = "";
                try
                {
                    mGlobal.LoadAppDBCfg();

                    sSQL = "SELECT [fldname],[source],[sharedfolderfile],[namedesc] ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TValueListDS ";
                    sSQL += "WHERE scanpjcode='" + pPrjCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND docdefid='" + pDDId.Trim().Replace("'", "") + "' ";
                    sSQL += "AND listtype='" + pListType.Trim().Replace("'", "") + "' ";
                    sSQL += "AND fldname='" + pFieldName.Trim().Replace("'", "''") + "' ";
                    sSQL += "AND source='" + pSource.Trim().Replace("'", "''") + "' ";

                    DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                    if (dr != null)
                    {
                        string sFile = dr[2].ToString().Trim();
                        string[] sLines = null;

                        if (sFile == string.Empty)
                        {
                            mGlobal.Write2Log("Value list data source file is empty.");
                            return;
                        }
                        else
                        {
                            FileInfo oFI = new FileInfo(sFile);
                            if (oFI.Exists)
                            {
                                sLines = File.ReadAllLines(sFile);
                            }
                            else
                            {
                                mGlobal.Write2Log("Value list data source file does not exist.");
                                return;
                            }
                        }

                        pValueList.DataSource = null;
                        pValueList.Items.Clear();
                        int i = 0; Dictionary<string, string> dItem = new Dictionary<string, string>();

                        dItem.Add("", "");

                        if (sLines != null)
                        {
                            while (i < sLines.Length)
                            {
                                dItem.Add(sLines[i].Trim(), sLines[i].Trim());

                                i += 1;
                            }
                        }

                        pValueList.DataSource = new BindingSource(dItem, null);
                        pValueList.DisplayMember = "Key";
                        pValueList.ValueMember = "Value";
                        pValueList.Refresh();
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private dynamic idpFromImageExtraction(int pPageNum, System.Drawing.Bitmap pBmp, string pModelId, string pBmpFile)
        {
            //System.Drawing.Bitmap bmp = null;
            try
            {
                if (pBmp == null)
                {
                    MessageBox.Show(this, "Image is blank!");
                    return null;
                }

                //if (_oImgCore.ImageBuffer.CurrentImageIndexInBuffer >= 0)
                if (_oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
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
                    oReq.classify = "No";
                    oReq.model = pModelId;
                    _sendStart = DateTime.Now;

                    string sMsg = "";
                    Task<dynamic> dResult = Task.Run(async () => oIDP.docAnalysis(sLocalEndPoint, oReq, ref sMsg));
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
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show(this, "Failed! " + sMsg);
                            }
                        }
                        else
                        {
                            dynamic oAna;
                            _sendEnd = DateTime.Now;
                            if (_docType.ToUpper().Trim() == "FORM")
                                oAna = SRDocScanIDPForm.FormAnalysisResult.FromJson(dResult.Result);
                            else
                                oAna = AnalysisResult.FromJson(dResult.Result);

                            if (oAna != null)
                            {
                                //if (oAna.Documents.Length > 0)
                                //    if (oAna.Documents[0].DocumentType != null)
                                //    {
                                //        MessageBox.Show(this,oAna.Documents[0].DocumentType.Trim());                                        
                                //    }

                                if (clsIDP.IDPResultExistDB(_currScanProj, _appNum, _batchCode, _setNum, _docType, pPageNum, oReq.endPoint, pModelId) == false)
                                {
                                    oIDP.saveIDPResultDB(_currScanProj, _appNum, _batchCode, _setNum, _docType, oReq.endPoint, pModelId, pPageNum,
                                        _totPage, _station, _userId, _sendStart, _sendEnd, pBmpFile, dResult.Result.ToString());
                                }

                                return oAna;
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
                Cursor.Current = Cursors.Default;

                mGlobal.Write2Log("IDP extracting.." + exp.Message + Environment.NewLine + exp.StackTrace.ToString());
                MessageBox.Show(exp.Message, "Extracting error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                //if (bmp != null) bmp.Dispose();
            }

            return null;
        }

        private bool populateGridDataDB(string pSetNum, string pDocType, int pPageId, string pTableNum)
        {
            try
            {
                DataTable oTabl = new DataTable();
                DataColumn oCol = null;
                string sSQL = "";
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT colname FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableCols ";
                sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND [setnum]='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [doctype]='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND [pageid]='" + pPageId + "' ";
                sSQL += "AND [tableno]='" + pTableNum.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY colorder ";

                DataRowCollection oRows = mGlobal.objDB.ReturnRows(sSQL);

                int i = 0;
                if (oRows != null)
                {
                    while (i < oRows.Count)
                    {
                        oCol = new DataColumn();
                        oCol.ColumnName = oRows[i][0].ToString().Trim();
                        oCol.AutoIncrement = true;
                        oCol.DataType = typeof(string);
                        oTabl.Columns.Add(oCol);

                        i += 1;
                    }
                }

                oRows = null;

                sSQL = "SELECT * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND [setnum]='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [doctype]='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND [pageid]='" + pPageId + "' ";
                sSQL += "AND [tableno]='" + pTableNum.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY rowno,colno ";

                oRows = mGlobal.objDB.ReturnRows(sSQL);

                i = 0; int c = 1; int r = 1; DataRow oNewRow = null;
                if (oRows != null)
                {
                    while (i < oRows.Count)
                    {
                        oNewRow = oTabl.NewRow();
                        while (c <= oTabl.Columns.Count)
                        {
                            if (oRows[i]["rowno"].ToString() == r.ToString() && oRows[c - 1]["colno"].ToString() == c.ToString())
                            {
                                oNewRow[c - 1] = oRows[i + c - 1]["colvalue"].ToString();
                            }

                            c += 1;
                        }

                        if (c > 1)
                        {
                            oTabl.Rows.Add(oNewRow);
                        }

                        r += 1;
                        c = 1;
                        i = i + oTabl.Columns.Count;
                    }
                }

                oRows = null;
                oNewRow = null;

                dgdTable.DataSource = oTabl;
                dgdTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgdTable.EnableHeadersVisualStyles = false;
                i = 0;
                while (i < dgdTable.Columns.Count)
                {
                    dgdTable.Columns[i].HeaderCell.Style.BackColor = System.Drawing.Color.Aqua;

                    i += 1;
                }
                dgdTable.Update();

                dgdTable.Refresh();

                return true;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
        }

        private bool populateGridData(AnalysisResult pResult, int pTableNumber)
        {
            try
            {
                if (pResult.Tables == null)
                {
                    mGlobal.Write2Log("Result table is blank for grid!");
                    return false;
                }
                if (pResult.Tables.Length == 0)
                {
                    mGlobal.Write2Log("Result table is empty for grid!");
                    return false;
                }
                if (pTableNumber == 0)
                {
                    MessageBox.Show(this, "Table number request can not be zero for grid!");
                    return false;
                }
                else if (pTableNumber > pResult.Tables.Length)
                {
                    MessageBox.Show(this, "Result table not exist for table " + pTableNumber.ToString() + " for grid!");
                    return false;
                }
                bool bKeyValueType = false;
                if (pResult.Tables[pTableNumber - 1].ColumnCount == 2)
                    bKeyValueType = true;

                DataTable oTabl = new DataTable();
                DataColumn oCol = null;
                int i = 0, j = 0, k = 0; long cel = 0;
                while (i < pResult.Tables[pTableNumber - 1].ColumnCount)
                {
                    if (bKeyValueType)
                    {
                        while (cel < pResult.Tables[pTableNumber - 1].Cells.LongLength)
                        {
                            if (pResult.Tables[pTableNumber - 1].Cells[cel].ColumnSpan == null) //in case Kind is not exists in the result.
                            {
                                if (pResult.Tables[pTableNumber - 1].Cells[cel].ColumnIndex == 0) //First column is name, Second column is value.
                                {
                                    oCol = new DataColumn();
                                    oCol.ColumnName = pResult.Tables[pTableNumber - 1].Cells[cel].Content.Trim();
                                    oCol.AutoIncrement = true;
                                    oCol.DataType = typeof(string);
                                    oTabl.Columns.Add(oCol);
                                }
                            }
                            else //First few row cell is column name(s).
                            {
                                if (cel + 1 > pResult.Tables[pTableNumber - 1].ColumnCount) break;

                                if (pResult.Tables[pTableNumber - 1].Cells[cel].RowIndex == 0
                                    && pResult.Tables[pTableNumber - 1].Cells[cel].ColumnSpan < 2) //First column is name, Second column is value, exclude table title.
                                {
                                    oCol = new DataColumn();
                                    oCol.ColumnName = pResult.Tables[pTableNumber - 1].Cells[cel].Content.Trim();
                                    oCol.AutoIncrement = true;
                                    oCol.DataType = typeof(string);
                                    oTabl.Columns.Add(oCol);
                                }
                            }

                            cel += 1;
                        }
                        break;
                    }
                    else
                    {
                        if (pResult.Tables[pTableNumber - 1].Cells[i].Kind != null) //in case Kind is not exists in the result.
                        {
                            if (pResult.Tables[pTableNumber - 1].Cells[i].RowIndex == 0)
                            {
                                oCol = new DataColumn();
                                oCol.ColumnName = pResult.Tables[pTableNumber - 1].Cells[i].Content.Trim();
                                oCol.AutoIncrement = true;
                                oCol.DataType = typeof(string);
                                oTabl.Columns.Add(oCol);
                            }

                            if (pResult.Tables[pTableNumber - 1].Cells[i].RowIndex != 0)
                                break;
                        }
                    }

                    i += 1;
                } //End While.

                if (oTabl.Columns.Count > 0)
                {
                    saveResultTableColumnsDB(oTabl, sCurrSetNum, _docType, _pageId, pTableNumber.ToString());
                }

                DataRow oRow = null; bool exit = false; int col = 0;
                if (pTableNumber <= pResult.Tables.LongLength)
                {
                    if (pTableNumber >= 1 || bKeyValueType)
                    {
                        cel = bKeyValueType == true ? 0 : 1; //First (zero index) cell is all the columns header.
                        if (bKeyValueType)
                        {
                            j = pResult.Tables[pTableNumber - 1].Cells.ToList().FindIndex(a => a.ColumnSpan == null);
                        }
                        else
                            j = pResult.Tables[pTableNumber - 1].Cells.ToList().FindIndex(a => a.Kind == null); //First index for Kind is null for column value in the result.
                        if (j != -1)
                        {
                            while (pResult.Tables[pTableNumber - 1].Cells[j].Kind == null) //Kind is null for column value in the result.
                            {
                                if (bKeyValueType == false && pResult.Tables[pTableNumber - 1].Cells[j].ColumnSpan != null)
                                {
                                    if (pResult.Tables[pTableNumber - 1].Cells[j].ColumnSpan >= 2 && pResult.Tables[pTableNumber - 1].Cells[j].RowIndex == 0)
                                    {
                                        j += 1; //Skip a row.
                                        continue;
                                    }
                                }

                                if (pResult.Tables[pTableNumber - 1].Cells[j].RowIndex == cel)
                                {
                                    if (bKeyValueType)
                                    {
                                        oRow = oTabl.NewRow();
                                        k = 0; col = 0;
                                        while (k < pResult.Tables[pTableNumber - 1].Cells.LongLength)
                                        {
                                            if (j + k < pResult.Tables[pTableNumber - 1].Cells.LongLength)
                                            {
                                                if (pResult.Tables[pTableNumber - 1].Cells[j + k].ColumnIndex == 1)
                                                {
                                                    oRow[col] = pResult.Tables[pTableNumber - 1].Cells[j + k].Content.Trim();
                                                    col += 1;
                                                }
                                            }
                                            else
                                            {
                                                exit = true;
                                                break;
                                            }

                                            k += 1;
                                        } //End While.

                                        exit = true;
                                    }
                                    else
                                    {
                                        oRow = oTabl.NewRow();
                                        k = 0;
                                        while (k < oRow.Table.Columns.Count)
                                        {
                                            if (j + k < pResult.Tables[pTableNumber - 1].Cells.LongLength)
                                            {
                                                oRow[k] = pResult.Tables[pTableNumber - 1].Cells[j + k].Content.Trim();
                                            }
                                            else
                                            {
                                                exit = true;
                                                break;
                                            }

                                            k += 1;
                                        } //End While.
                                    }

                                    if (k != 0)
                                    {
                                        oTabl.Rows.Add(oRow);
                                    }

                                    if (exit) break;
                                }

                                if (exit) break;

                                j = j + k;

                                if (j >= pResult.Tables[pTableNumber - 1].Cells.LongLength) break;
                                if (cel >= pResult.Tables[pTableNumber - 1].Cells.LongLength) break;

                                if (bKeyValueType)
                                    cel = pResult.Tables[pTableNumber - 1].Cells[j].RowIndex;
                                else
                                {
                                    if (cel != pResult.Tables[pTableNumber - 1].Cells[j - k].RowIndex)
                                        cel = pResult.Tables[pTableNumber - 1].Cells[j].RowIndex;
                                    else
                                        cel += 1;
                                }
                            } //End While.
                        }
                    }

                    i += 1;
                }

                if (resultTableExistDB(sCurrSetNum, _docType, _pageId, pTableNumber.ToString()) == false)
                {
                    i = 0; j = 0;
                    while (i < oTabl.Rows.Count)
                    {
                        while (j < oTabl.Columns.Count)
                        {
                            saveResultTableDB(sCurrSetNum, _docType, _pageId, pTableNumber.ToString(),
                                i + 1, j + 1, oTabl.Rows[i][j].ToString());
                            j += 1;
                        }

                        j = 0;
                        i += 1;
                    }
                }

                dgdTable.DataSource = oTabl;
                dgdTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgdTable.EnableHeadersVisualStyles = false;
                i = 0;
                while (i < dgdTable.Columns.Count)
                {
                    dgdTable.Columns[i].HeaderCell.Style.BackColor = System.Drawing.Color.Aqua;

                    i += 1;
                }
                dgdTable.Update();

                dgdTable.Refresh();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("IDP populating.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
            return true;
        }

        private bool populateGridData(SRDocScanIDPForm.FormAnalysisResult pResult, int pTableNumber)
        {
            try
            {
                if (pResult.Tables == null)
                {
                    mGlobal.Write2Log("Result table is blank for grid!");
                    return false;
                }
                if (pResult.Tables.Length == 0)
                {
                    mGlobal.Write2Log("Result table is empty for grid!");
                    return false;
                }
                if (pTableNumber == 0)
                {
                    MessageBox.Show(this, "Table number request can not be zero for grid!");
                    return false;
                }
                else if (pTableNumber > pResult.Tables.Length)
                {
                    MessageBox.Show(this, "Result table not exist for table " + pTableNumber.ToString() + " for grid!");
                    return false;
                }
                bool bKeyValueType = false;
                if (pResult.Tables[pTableNumber - 1].ColumnCount == 2)
                    bKeyValueType = true;

                DataTable oTabl = new DataTable();
                DataColumn oCol = null;
                int i = 0, j = 0, k = 0; long cel = 0;
                while (i < pResult.Tables[pTableNumber - 1].ColumnCount)
                {
                    if (bKeyValueType)
                    {
                        while (cel < pResult.Tables[pTableNumber - 1].Cells.LongLength)
                        {
                            if (pResult.Tables[pTableNumber - 1].Cells[cel].ColumnSpan == null) //in case Kind is not exists in the result.
                            {
                                if (pResult.Tables[pTableNumber - 1].Cells[cel].ColumnIndex == 0) //First column is name, Second column is value.
                                {
                                    oCol = new DataColumn();
                                    oCol.ColumnName = pResult.Tables[pTableNumber - 1].Cells[cel].Content.Trim();
                                    oCol.AutoIncrement = true;
                                    oCol.DataType = typeof(string);
                                    oTabl.Columns.Add(oCol);
                                }
                            }
                            else //First few row cell is column name(s).
                            {
                                //if (cel + 1 > pResult.Tables[pTableNumber - 1].ColumnCount) break;
                                if (cel + 1 > pResult.Tables[pTableNumber - 1].Cells.LongLength) break;

                                if (pResult.Tables[pTableNumber - 1].Cells[cel].RowIndex == 0
                                    && pResult.Tables[pTableNumber - 1].Cells[cel].ColumnSpan < 2) //First column is name, Second column is value, exclude table title.
                                //if (pResult.Tables[pTableNumber - 1].Cells[cel].RowIndex != 0
                                //    && pResult.Tables[pTableNumber - 1].Cells[cel].ColumnIndex == 0) //First column is name, Second column is value, exclude table title.
                                {
                                    oCol = new DataColumn();
                                    oCol.ColumnName = pResult.Tables[pTableNumber - 1].Cells[cel].Content.Trim();
                                    oCol.AutoIncrement = true;
                                    oCol.DataType = typeof(string);
                                    oTabl.Columns.Add(oCol);
                                }
                            }

                            cel += 1;
                        }
                        break;
                    }
                    else
                    {
                        if (pResult.Tables[pTableNumber - 1].Cells[i].Kind != null) //in case Kind is not exists in the result.
                        {
                            if (pResult.Tables[pTableNumber - 1].Cells[i].RowIndex == 0)
                            {
                                oCol = new DataColumn();
                                oCol.ColumnName = pResult.Tables[pTableNumber - 1].Cells[i].Content.Trim();
                                oCol.AutoIncrement = true;
                                oCol.DataType = typeof(string);
                                oTabl.Columns.Add(oCol);
                            }

                            if (pResult.Tables[pTableNumber - 1].Cells[i].RowIndex != 0)
                                break;
                        }
                    }

                    i += 1;
                } //End While.

                if (oTabl.Columns.Count > 0)
                {
                    saveResultTableColumnsDB(oTabl, sCurrSetNum, _docType, _pageId, pTableNumber.ToString());
                }

                DataRow oRow = null; bool exit = false; int col = 0;
                if (pTableNumber <= pResult.Tables.LongLength)
                {
                    if (pTableNumber >= 1 || bKeyValueType)
                    {
                        cel = bKeyValueType == true ? 0 : 1; //First (zero index) cell is all the columns header.
                        if (bKeyValueType)
                        {
                            j = pResult.Tables[pTableNumber - 1].Cells.ToList().FindIndex(a => a.ColumnSpan == null || a.ColumnIndex != 0);
                        }
                        else
                            j = pResult.Tables[pTableNumber - 1].Cells.ToList().FindIndex(a => a.Kind == null || a.RowIndex != 0); //First index for Kind is null for column value in the result.

                        if (j != -1)
                        {
                            while (true) //Kind is null for column value in the result.
                            {
                                if (bKeyValueType == false && (pResult.Tables[pTableNumber - 1].Cells[j].Kind == null || pResult.Tables[pTableNumber - 1].Cells[j].RowIndex != 0)
                                == false)
                                    break;

                                if (bKeyValueType == false && pResult.Tables[pTableNumber - 1].Cells[j].ColumnSpan != null)
                                {
                                    if (pResult.Tables[pTableNumber - 1].Cells[j].ColumnSpan >= 2 && pResult.Tables[pTableNumber - 1].Cells[j].RowIndex == 0)
                                    {
                                        j += 1; //Skip a row.
                                        continue;
                                    }
                                }

                                if (pResult.Tables[pTableNumber - 1].Cells[j].RowIndex == cel)
                                {
                                    if (bKeyValueType)
                                    {
                                        oRow = oTabl.NewRow();
                                        k = 0; col = 0;
                                        while (k < pResult.Tables[pTableNumber - 1].Cells.LongLength)
                                        {
                                            if (j + k < pResult.Tables[pTableNumber - 1].Cells.LongLength)
                                            {
                                                if (pResult.Tables[pTableNumber - 1].Cells[j + k].ColumnIndex == 1 
                                                    && pResult.Tables[pTableNumber - 1].Cells[j + k].RowIndex != 0)
                                                {
                                                    if (col < oRow.Table.Columns.Count)
                                                        oRow[col] = pResult.Tables[pTableNumber - 1].Cells[j + k].Content.Trim();
                                                    else
                                                        break;

                                                    col += 1;
                                                }
                                                //else if (col > oRow.Table.Columns.Count - 1)
                                                //{
                                                //    col = 0;
                                                //    oRow = oTabl.NewRow();
                                                //}
                                            }
                                            else
                                            {
                                                exit = true;
                                                break;
                                            }

                                            k += 1;
                                        } //End While.

                                        exit = true;
                                    }
                                    else
                                    {
                                        oRow = oTabl.NewRow();
                                        k = 0;
                                        while (k < oRow.Table.Columns.Count)
                                        {
                                            if (j + k < pResult.Tables[pTableNumber - 1].Cells.LongLength)
                                            {
                                                oRow[k] = pResult.Tables[pTableNumber - 1].Cells[j + k].Content.Trim();
                                            }
                                            else
                                            {
                                                exit = true;
                                                break;
                                            }

                                            k += 1;
                                        } //End While.
                                    }

                                    if (k != 0)
                                    {
                                        oTabl.Rows.Add(oRow);
                                    }

                                    if (exit) break;
                                }

                                if (exit) break;

                                j = j + k;

                                if (j >= pResult.Tables[pTableNumber - 1].Cells.LongLength) break;
                                if (cel >= pResult.Tables[pTableNumber - 1].Cells.LongLength) break;

                                if (bKeyValueType)
                                    cel = pResult.Tables[pTableNumber - 1].Cells[j].RowIndex;
                                else
                                {
                                    if (cel != pResult.Tables[pTableNumber - 1].Cells[j - k].RowIndex)
                                        cel = pResult.Tables[pTableNumber - 1].Cells[j].RowIndex;
                                    else
                                        cel += 1;
                                }
                            } //End While.
                        }
                    }

                    i += 1;
                }

                if (resultTableExistDB(sCurrSetNum, _docType, _pageId, pTableNumber.ToString()) == false)
                {
                    i = 0; j = 0;
                    while (i < oTabl.Rows.Count)
                    {
                        while (j < oTabl.Columns.Count)
                        {
                            saveResultTableDB(sCurrSetNum, _docType, _pageId, pTableNumber.ToString(),
                                i + 1, j + 1, oTabl.Rows[i][j].ToString());
                            j += 1;
                        }

                        j = 0;
                        i += 1;
                    }
                }

                dgdTable.DataSource = oTabl;
                dgdTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgdTable.EnableHeadersVisualStyles = false;
                i = 0;
                while (i < dgdTable.Columns.Count)
                {
                    dgdTable.Columns[i].HeaderCell.Style.BackColor = System.Drawing.Color.Aqua;

                    i += 1;
                }
                dgdTable.Update();

                dgdTable.Refresh();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("IDP populating.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
            return true;
        }

        private void loadIndexFieldsConfig(List<string> lFieldNames)
        {
            try
            {
                string[] drs = lFieldNames.ToArray();

                if (drs != null)
                {
                    Label oLblFld = null; TextBox oTxtFld = null; Label oLblExist = null;
                    ComboBox oCbxFld = null; DateTimePicker oDtpFld = null; Button oLookBtn = null;
                    System.Drawing.Point currLoc1, currLoc2, currLoc3, currLoc4;
                    int iDefHeight = txtField.Height; int iDefWidth = txtField.Width; //List<int[]> lPosition = new List<int[]>(); //Position list.

                    int i = 0;
                    while (i < drs.Length)
                    {
                        if (i == 0)
                        {
                            if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "valuelist")
                            {
                                lblField.Text = mGlobal.getMapName(drs[i].ToString(), clsIDP.stcIDPFieldMap.FieldName.ToArray(), clsIDP.stcIDPFieldMap.FieldDispName.ToArray());
                                txtField.Visible = false;
                                dtpField.Visible = false;
                                cbxField.Visible = true;

                                cbxField.MaxLength = Convert.ToInt32(staMain.stcIndexSetting.FieldSize[i].ToString());

                                cbxField.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                {
                                    cbxField.Enabled = false;
                                    cbxField.BackColor = SystemColors.ControlLight;
                                }
                                else
                                {
                                    cbxField.Enabled = true;
                                    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                        cbxField.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        cbxField.BackColor = System.Drawing.Color.White;
                                }

                                lPosition.Add(new int[] { cbxField.Location.Y, cbxField.Height });

                                cbxField.Tag = i.ToString();
                                if (staMain.stcIndexSetting.ValueListType[i].Trim().ToLower() == "listing")
                                    loadValueList(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, "Value List", lblField.Text.Trim(), cbxField);
                                else if (staMain.stcIndexSetting.ValueListType[i].Trim().ToLower() == "data source")
                                    loadValueListDS(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, "Value List", lblField.Text.Trim(), "Data Source", cbxField);

                                cbxField.GotFocus += indexField_Focus;

                                lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();
                            }
                            else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "string"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "integer"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "lookup"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "barcode") //Or MultiLine
                            {
                                lblField.Text = mGlobal.getMapName(drs[i].ToString(), clsIDP.stcIDPFieldMap.FieldName.ToArray(), clsIDP.stcIDPFieldMap.FieldDispName.ToArray());
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
                                        txtField.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        txtField.BackColor = System.Drawing.Color.White;
                                }

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline")
                                {
                                    txtField.Multiline = true;
                                    txtField.Height = txtField.Height * 8;
                                    txtField.ScrollBars = ScrollBars.Both;
                                }
                                else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "lookup")
                                {
                                    mGlobal.Write2Log("Lookup metadata cannot be first index field, dependency is required on first field. Please check with system administrator.");
                                    //txtField.Width = txtField.Width - btnLook.Width - 2;
                                    //btnLook.Tag = i.ToString();
                                    //btnLook.BringToFront();
                                    //currLoc4 = btnLook.Location;
                                    //currLoc4.Y = oTxtFld.Location.Y - 2;
                                    //btnLook.Location = currLoc4;

                                    //btnLook.Visible = true;
                                    //btnLook.Click += new EventHandler(btnLook_Click);
                                }

                                lPosition.Add(new int[] { txtField.Location.Y, txtField.Height });

                                txtField.Tag = i.ToString();
                                txtField.KeyDown += txtField_KeyDown;
                                txtField.GotFocus += indexField_Focus;

                                lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();
                            }
                            else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date" || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "datetime")
                            {
                                lblField.Text = mGlobal.getMapName(drs[i].ToString(), clsIDP.stcIDPFieldMap.FieldName.ToArray(), clsIDP.stcIDPFieldMap.FieldDispName.ToArray());
                                txtField.Visible = false;
                                dtpField.Visible = true;
                                cbxField.Visible = false;

                                if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                {
                                    dtpField.Enabled = false;
                                    dtpField.BackColor = SystemColors.ControlLight;
                                }
                                else
                                {
                                    dtpField.Enabled = true;
                                    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                        dtpField.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        dtpField.BackColor = System.Drawing.Color.White;
                                }

                                dtpField.Tag = i.ToString();
                                dtpField.Format = DateTimePickerFormat.Custom;

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                {
                                    dtpField.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    dtpField.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                lPosition.Add(new int[] { dtpField.Location.Y, dtpField.Height });

                                dtpField.KeyDown += dtpField_KeyDown;
                                dtpField.GotFocus += indexField_Focus;
                                lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();
                            }
                        }
                        else
                        {
                            if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "valuelist")
                            {
                                currLoc1 = lblField.Location;
                                currLoc1.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;
                                currLoc2 = cbxField.Location;
                                currLoc2.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;
                                currLoc3 = lblFieldExist.Location;
                                currLoc3.Y = lPosition[i - 1][0] + lPosition[i - 1][1] + 5;

                                oLblFld = new Label();
                                oLblFld.AutoSize = true;
                                oLblFld.Name = "lblField" + (i).ToString();
                                oLblFld.Location = currLoc1;
                                oLblFld.Text = mGlobal.getMapName(drs[i].ToString(), clsIDP.stcIDPFieldMap.FieldName.ToArray(), clsIDP.stcIDPFieldMap.FieldDispName.ToArray());
                                oLblFld.SendToBack();

                                oCbxFld = new ComboBox();
                                oCbxFld.Name = "cbxField" + (i).ToString();
                                oCbxFld.Location = currLoc2;
                                oCbxFld.Size = cbxField.Size;
                                oCbxFld.Tag = (i).ToString();
                                oCbxFld.BringToFront();
                                oCbxFld.Width = cbxField.Width;

                                oLblExist = new Label();
                                oLblExist.Name = "lblFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                oCbxFld.MaxLength = Convert.ToInt32(staMain.stcIndexSetting.FieldSize[i].ToString());

                                oCbxFld.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                {
                                    oCbxFld.Enabled = false;
                                    oCbxFld.BackColor = SystemColors.ControlLight;
                                }
                                else
                                {
                                    oCbxFld.Enabled = true;
                                    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                        oCbxFld.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        oCbxFld.BackColor = System.Drawing.Color.White;
                                }

                                lPosition.Add(new int[] { oCbxFld.Location.Y, oCbxFld.Height });

                                oCbxFld.Tag = i.ToString();
                                if (staMain.stcIndexSetting.ValueListType[i].Trim().ToLower() == "listing")
                                    loadValueList(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, "Value List", oLblFld.Text.Trim(), oCbxFld);
                                else if (staMain.stcIndexSetting.ValueListType[i].Trim().ToLower() == "data source")
                                    loadValueListDS(_currScanProj, _appNum, staMain.stcIndexSetting.DocDefId, "Value List", oLblFld.Text.Trim(), "Data Source", oCbxFld);

                                oCbxFld.GotFocus += indexField_Focus;

                                lblFieldExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                pnlIdxFld.Controls.Add(oLblFld);
                                pnlIdxFld.Controls.Add(oCbxFld);
                                pnlIdxFld.Controls.Add(oLblExist);
                            }
                            else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "string"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "integer"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "lookup"
                                || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "barcode")
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
                                oLblFld.Text = mGlobal.getMapName(drs[i].ToString(), clsIDP.stcIDPFieldMap.FieldName.ToArray(), clsIDP.stcIDPFieldMap.FieldDispName.ToArray());
                                oLblFld.SendToBack();

                                oTxtFld = new TextBox();
                                oTxtFld.Name = "txtField" + (i).ToString();
                                oTxtFld.Location = currLoc2;
                                oTxtFld.Size = txtField.Size;
                                oTxtFld.Tag = (i).ToString();
                                oTxtFld.BringToFront();
                                oTxtFld.Width = iDefWidth;

                                oLblExist = new Label();
                                oLblExist.Name = "lblFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "multiline")
                                {
                                    oTxtFld.Multiline = true;
                                    oTxtFld.Height = iDefHeight * 8;
                                    oTxtFld.ScrollBars = ScrollBars.Both;
                                    //currLoc1.Y = (currLoc1.Y) + oTxtFld.Height;
                                    //currLoc2.Y = (currLoc2.Y) + oTxtFld.Height;
                                    //currLoc3.Y = (currLoc3.Y) + oTxtFld.Height;
                                    //oLblFld.Location = currLoc1;
                                    //oTxtFld.Location = currLoc2;
                                    //oLblExist.Location = currLoc3;
                                }
                                else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "lookup")
                                {
                                    //oLookBtn = new Button();
                                    //oLookBtn.Name = "btnLook" + (i).ToString();
                                    //oLookBtn.Text = "Search";
                                    //oLookBtn.Width = btnLook.Width;
                                    //oLookBtn.Height = btnLook.Height;
                                    //oLookBtn.Tag = i.ToString();
                                    //oLookBtn.BringToFront();

                                    //currLoc4 = btnLook.Location;
                                    //currLoc4.Y = oTxtFld.Location.Y - 2;
                                    //oLookBtn.Location = currLoc4;

                                    //oTxtFld.Width = oTxtFld.Width - oLookBtn.Width - 2;
                                    //oLookBtn.Visible = true;
                                    //oLookBtn.Click += new EventHandler(btnLook_Click);

                                    //pnlIdxFld.Controls.Add(oLookBtn);
                                }
                                //if (currLoc2.Y <= lPosition[i - 1])
                                //{
                                //    currLoc1.Y = lPosition[i - 1] + 5;
                                //    currLoc2.Y = lPosition[i - 1] + 5;
                                //    currLoc3.Y = lPosition[i - 1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oTxtFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}

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
                                        oTxtFld.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        oTxtFld.BackColor = System.Drawing.Color.White;
                                }

                                lPosition.Add(new int[] { oTxtFld.Location.Y, oTxtFld.Height });

                                oTxtFld.KeyDown += txtField_KeyDown;
                                oTxtFld.GotFocus += indexField_Focus;

                                pnlIdxFld.Controls.Add(oLblFld);
                                pnlIdxFld.Controls.Add(oTxtFld);
                                pnlIdxFld.Controls.Add(oLblExist);
                            }
                            else if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date" || staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "datetime")
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
                                oLblFld.Text = mGlobal.getMapName(drs[i].ToString(), clsIDP.stcIDPFieldMap.FieldName.ToArray(), clsIDP.stcIDPFieldMap.FieldDispName.ToArray());
                                oLblFld.SendToBack();

                                oDtpFld = new DateTimePicker();
                                oDtpFld.Name = "dtpField" + (i).ToString();
                                oDtpFld.Location = currLoc2;
                                oDtpFld.Size = dtpField.Size;
                                oDtpFld.Tag = (i).ToString();
                                oDtpFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                //if (currLoc2.Y <= lPosition[i - 1][0])
                                //{
                                //    currLoc1.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc2.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc3.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oDtpFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}                                

                                oDtpFld.Text = staMain.stcIndexSetting.FieldDefault[i].Trim();

                                if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                {
                                    oDtpFld.Enabled = false;
                                    oDtpFld.BackColor = SystemColors.ControlLight;
                                }
                                else
                                {
                                    oDtpFld.Enabled = true;
                                    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                        oDtpFld.BackColor = System.Drawing.Color.LightYellow;
                                    else
                                        oDtpFld.BackColor = System.Drawing.Color.White;
                                }

                                oDtpFld.Tag = i.ToString();
                                oDtpFld.Format = DateTimePickerFormat.Custom;

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy HH:mm:ss";
                                }

                                lPosition.Add(new int[] { oDtpFld.Location.Y, oDtpFld.Height });

                                oDtpFld.KeyDown += dtpField_KeyDown;
                                oDtpFld.GotFocus += indexField_Focus;

                                pnlIdxFld.Controls.Add(oLblFld);
                                pnlIdxFld.Controls.Add(oDtpFld);
                                pnlIdxFld.Controls.Add(oLblExist);
                            }
                        }

                        i += 1;
                    } //End while.

                    i = 0;
                    while (i < drs.Length)
                    {
                        if (staMain.stcIndexSetting.FieldDataType[i].ToString().ToLower() == "lookup")
                        {
                            Control[] oCtrls = null;
                            if ((i - 1) == 0)
                            {
                                oCtrls = pnlIdxFld.Controls.Find("txtField", true); //Previous one only.
                            }
                            else
                                oCtrls = pnlIdxFld.Controls.Find("txtField" + (i - 1), true); //Previous one only.

                            if (oCtrls != null)
                            {
                                if (oCtrls.Length > 0)
                                {
                                    oCtrls[0].KeyDown += lookupField_KeyDown;
                                    oCtrls[0].GotFocus += indexField_Focus;
                                }
                            }
                        }

                        i += 1;
                    } //End while.

                }
                drs = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private bool loadIndexFieldsConfig(AnalysisResult oResult, ref int pFldCnt)
        {
            try
            {
                int a = 0; string sKey = ""; List<string> lFieldNames = new List<string>();
                if (oResult.Documents != null)
                {
                    if (oResult.Documents.LongLength > 0)
                    {
                        if (oResult.Documents[0].Fields == null)
                        {
                            mGlobal.Write2Log("Result field is blank for index fields settings!");
                            return false;
                        }
                    }
                    else
                    {
                        mGlobal.Write2Log("Result document is zero for index fields settings!");
                        return false;
                    }
                }
                else
                {
                    mGlobal.Write2Log("Result document field is blank for index fields settings!");
                    return false;
                }

                //staMain.initIndexConfig(oResult.Documents[0].Fields.Count);

                while (a < oResult.Documents[0].Fields.Count)
                {
                    sKey = oResult.Documents[0].Fields.ToArray()[a].Key.Trim();
                    if (oResult.Documents[0].Fields[sKey].Content == null)
                    {
                        mGlobal.Write2Log("Result field is blank for index fields!");
                        return false;
                    }
                    lFieldNames.Add(sKey);

                    //staMain.stcIndexSetting.PrjCode = _currScanProj;
                    //staMain.stcIndexSetting.AppNum = _appNum;
                    //staMain.stcIndexSetting.DocDefId = _docDefId;
                    //staMain.stcIndexSetting.RowId.Add(a);
                    //staMain.stcIndexSetting.FieldName.Add(sKey);
                    //if (sKey.Trim().ToLower().IndexOf("date") != -1)
                    //    staMain.stcIndexSetting.FieldDataType.Add("date");
                    //else
                    //    staMain.stcIndexSetting.FieldDataType.Add("string");
                    //staMain.stcIndexSetting.FieldMand.Add("Y");
                    //staMain.stcIndexSetting.ValueListType.Add("");
                    //staMain.stcIndexSetting.FieldDispSeq.Add(a + 1);
                    //staMain.stcIndexSetting.FieldEdit.Add("Y");
                    //staMain.stcIndexSetting.FieldDefault.Add("");
                    //staMain.stcIndexSetting.FieldSize.Add(oResult.Documents[0].Fields[sKey].Content.Trim().Length);
                    //staMain.stcIndexSetting.FieldAutoFill.Add("N");
                    //staMain.stcIndexSetting.FieldAutoOCR.Add("N");
                    //staMain.stcIndexSetting.FieldZoneOCR.Add("N");
                    //staMain.stcIndexSetting.FieldDecimal.Add(0);

                    a += 1;
                }

                pFldCnt = lFieldNames.Count;
                clsIDP.loadIDPFieldMappingDB(_currScanProj, _appNum, _docDefId);
                loadIndexFieldsConfig(lFieldNames);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return true;
        }

        private bool loadIndexFieldsConfig(SRDocScanIDPForm.FormAnalysisResult oResult, ref int pFldCnt)
        {
            try
            {
                int a = 0; string sKey = ""; List<string> lFieldNames = new List<string>();
                if (oResult.Documents != null)
                {
                    if (oResult.Documents.LongLength > 0)
                    {
                        if (oResult.Documents[0].Fields == null)
                        {
                            mGlobal.Write2Log("Result field is blank for index fields settings!");
                            return false;
                        }
                    }
                    else
                    {
                        mGlobal.Write2Log("Result document is zero for index fields settings!");
                        return false;
                    }
                }
                else
                {
                    mGlobal.Write2Log("Result document field is blank for index fields settings!");
                    return false;
                }

                //staMain.initIndexConfig(oResult.Documents[0].Fields.Count);

                while (a < oResult.Documents[0].Fields.Count)
                {
                    sKey = oResult.Documents[0].Fields.ToArray()[a].Key.Trim();
                    if (oResult.Documents[0].Fields[sKey].Content == null)
                    {
                        mGlobal.Write2Log("Result field is blank for index fields!");
                        return false;
                    }
                    lFieldNames.Add(sKey);

                    //staMain.stcIndexSetting.PrjCode = _currScanProj;
                    //staMain.stcIndexSetting.AppNum = _appNum;
                    //staMain.stcIndexSetting.DocDefId = _docDefId;
                    //staMain.stcIndexSetting.RowId.Add(a);
                    //staMain.stcIndexSetting.FieldName.Add(sKey);
                    //if (sKey.Trim().ToLower().IndexOf("date") != -1)
                    //    staMain.stcIndexSetting.FieldDataType.Add("date");
                    //else
                    //    staMain.stcIndexSetting.FieldDataType.Add("string");
                    //staMain.stcIndexSetting.FieldMand.Add("Y");
                    //staMain.stcIndexSetting.ValueListType.Add("");
                    //staMain.stcIndexSetting.FieldDispSeq.Add(a + 1);
                    //staMain.stcIndexSetting.FieldEdit.Add("Y");
                    //staMain.stcIndexSetting.FieldDefault.Add("");
                    //staMain.stcIndexSetting.FieldSize.Add(oResult.Documents[0].Fields[sKey].Content.Trim().Length);
                    //staMain.stcIndexSetting.FieldAutoFill.Add("N");
                    //staMain.stcIndexSetting.FieldAutoOCR.Add("N");
                    //staMain.stcIndexSetting.FieldZoneOCR.Add("N");
                    //staMain.stcIndexSetting.FieldDecimal.Add(0);

                    a += 1;
                }

                pFldCnt = lFieldNames.Count;
                clsIDP.loadIDPFieldMappingDB(_currScanProj, _appNum, _docDefId);
                loadIndexFieldsConfig(lFieldNames);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return true;
        }

        private void loadIndexFieldValue(AnalysisResult oResult)
        {
            try
            {
                DataTable oTabl = new DataTable();
                oTabl.Columns.Add("fields", typeof(string));
                DataRow oRow = null;
                DataRowCollection drs = oTabl.Rows;
                int a = 0; string sKey = "";

                while (a < oResult.Documents[0].Fields.Count)
                {
                    sKey = oResult.Documents[0].Fields.ToArray()[a].Key.Trim();
                    if (oResult.Documents[0].Fields[sKey].Content == null)
                    {
                        mGlobal.Write2Log("Result field is blank for index fields!");
                        return;
                    }

                    oRow = oTabl.NewRow();
                    oRow[0] = sKey + "|" + oResult.Documents[0].Fields[sKey].Content.Trim();
                    drs.Add(oRow);

                    a += 1;
                }
                oTabl.Dispose();

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        int iCnt = oResult.Documents[0].Fields.Count;//staMain.stcIndexSetting.RowId.Count;
                        int i = 0, j = 0; Control oCtrl = null; Label oLblExist = null; string sCtrlType = "";
                        ComboBox oCbx; DateTimePicker oDate; DateTime oDateParsed; string sDate = "";

                        while (i < iCnt)
                        {
                            sCtrlType = "";

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
                                sCtrlType = "Date";
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

                            if (sCtrlType == "Date") //Init.
                            {
                                oDate = (DateTimePicker)oCtrl;
                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                {
                                    ShortDatePattern = oDate.CustomFormat
                                };

                                oDate.Value = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                            }

                            if (oCtrl != null && oLblExist != null && i < drs.Count)
                            {
                                if (sCtrlType != "Date")
                                    oCtrl.Text = ""; //Init.
                                oLblExist.Text = ""; //Init.                                

                                //j = 0;
                                j = i;
                                while (j < drs.Count)
                                {
                                    if (j < iCnt)
                                    {
                                        if (drs[j][0].ToString().Split("|")[0].Trim() == oResult.Documents[0].Fields.ToArray()[j].Key.Trim())
                                        {
                                            if (sCtrlType == "ComboBox")
                                            {
                                                oCbx = (ComboBox)oCtrl;
                                                oCbx.SelectedValue = drs[j][0].ToString().Split("|")[1].Trim();
                                            }
                                            else if (sCtrlType == "Date")
                                            {
                                                oDate = (DateTimePicker)oCtrl;
                                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                                {
                                                    oDate.CustomFormat = "dd/MM/yyyy";
                                                }
                                                else
                                                {
                                                    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                                }

                                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                                {
                                                    ShortDatePattern = oDate.CustomFormat
                                                };

                                                oDateParsed = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                                                try
                                                {
                                                    if (drs[j][0].ToString().Split("|")[1].Trim() != string.Empty)
                                                    {
                                                        sDate = mGlobal.sanitizeDateValue(drs[j][0].ToString().Split("|")[1].Trim(), "");
                                                        if (sDate != string.Empty)
                                                        {
                                                            if (!mGlobal.isDate(sDate, new string[] { oDate.CustomFormat }))
                                                                throw new Exception("Date is in invalid format.");

                                                            if (DateTime.TryParse(sDate, out oDateParsed))
                                                            {
                                                                //oDate.Value = Convert.ToDateTime(drs[j][0].ToString().Trim(), formatInfo);
                                                                oDate.Value = oDateParsed;
                                                            }
                                                            else
                                                            {
                                                                oDate.Value = Convert.ToDateTime(sDate, formatInfo);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    mGlobal.Write2Log("load date value.." + sDate + "," + ex.Message);
                                                    //Continue
                                                }
                                            }
                                            else
                                                oCtrl.Text = drs[j][0].ToString().Split("|")[1].Trim();

                                            oLblExist.Text = drs[j][0].ToString().Split("|")[1].Trim();
                                            break;
                                        }
                                    }

                                    j += 1;
                                }
                            } //End While.

                            i += 1;
                        } //End While.
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("IDP populating.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private void loadIndexFieldValue(SRDocScanIDPForm.FormAnalysisResult oResult)
        {
            try
            {
                DataTable oTabl = new DataTable();
                oTabl.Columns.Add("fields", typeof(string));
                DataRow oRow = null;
                DataRowCollection drs = oTabl.Rows;
                int a = 0; string sKey = "";

                while (a < oResult.Documents[0].Fields.Count)
                {
                    sKey = oResult.Documents[0].Fields.ToArray()[a].Key.Trim();
                    if (oResult.Documents[0].Fields[sKey].Content == null)
                    {
                        mGlobal.Write2Log("Result field is blank for index fields!");
                        return;
                    }

                    oRow = oTabl.NewRow();
                    oRow[0] = sKey + "|" + oResult.Documents[0].Fields[sKey].Content.Trim();
                    drs.Add(oRow);

                    a += 1;
                }
                oTabl.Dispose();

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        int iCnt = oResult.Documents[0].Fields.Count;//staMain.stcIndexSetting.RowId.Count;
                        int i = 0, j = 0; Control oCtrl = null; Label oLblExist = null; string sCtrlType = "";
                        ComboBox oCbx; DateTimePicker oDate; DateTime oDateParsed; string sDate = "";

                        while (i < iCnt)
                        {
                            sCtrlType = "";

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
                                sCtrlType = "Date";
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

                            if (sCtrlType == "Date") //Init.
                            {
                                oDate = (DateTimePicker)oCtrl;
                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                {
                                    ShortDatePattern = oDate.CustomFormat
                                };

                                oDate.Value = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                            }

                            if (oCtrl != null && oLblExist != null && i < drs.Count)
                            {
                                if (sCtrlType != "Date")
                                    oCtrl.Text = ""; //Init.
                                oLblExist.Text = ""; //Init.                                

                                //j = 0;
                                j = i;
                                while (j < drs.Count)
                                {
                                    if (j < iCnt)
                                    {
                                        if (drs[j][0].ToString().Split("|")[0].Trim() == oResult.Documents[0].Fields.ToArray()[j].Key.Trim())
                                        {
                                            if (sCtrlType == "ComboBox")
                                            {
                                                oCbx = (ComboBox)oCtrl;
                                                oCbx.SelectedValue = drs[j][0].ToString().Split("|")[1].Trim();
                                            }
                                            else if (sCtrlType == "Date")
                                            {
                                                oDate = (DateTimePicker)oCtrl;
                                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                                {
                                                    oDate.CustomFormat = "dd/MM/yyyy";
                                                }
                                                else
                                                {
                                                    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                                }

                                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                                {
                                                    ShortDatePattern = oDate.CustomFormat
                                                };

                                                oDateParsed = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                                                try
                                                {
                                                    if (drs[j][0].ToString().Split("|")[1].Trim() != string.Empty)
                                                    {
                                                        sDate = mGlobal.sanitizeDateValue(drs[j][0].ToString().Split("|")[1].Trim(), "");
                                                        if (sDate != string.Empty)
                                                        {
                                                            if (!mGlobal.isDate(sDate, new string[] { oDate.CustomFormat }))
                                                                throw new Exception("Date is in invalid format.");

                                                            if (DateTime.TryParse(sDate, out oDateParsed))
                                                            {
                                                                //oDate.Value = Convert.ToDateTime(drs[j][0].ToString().Trim(), formatInfo);
                                                                oDate.Value = oDateParsed;
                                                            }
                                                            else
                                                            {
                                                                oDate.Value = Convert.ToDateTime(sDate, formatInfo);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    mGlobal.Write2Log("load date value.." + sDate + "," + ex.Message);
                                                    //Continue
                                                }
                                            }
                                            else
                                                oCtrl.Text = drs[j][0].ToString().Split("|")[1].Trim();

                                            oLblExist.Text = drs[j][0].ToString().Split("|")[1].Trim();
                                            break;
                                        }
                                    }

                                    j += 1;
                                }
                            } //End While.

                            i += 1;
                        } //End While.
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("IDP populating.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private void resetIndexFieldsConfig()
        {
            try
            {
                //Remove current index fields and init index field to default.
                int n = 0; List<string> lDelIdx = new List<string>();
                if (pnlIdxFld.Controls.Count > 7)
                {
                    while (n < pnlIdxFld.Controls.Count)
                    {
                        if (pnlIdxFld.Controls[n].Name == "cbxField" || pnlIdxFld.Controls[n].Name == "dtpField"
                            || pnlIdxFld.Controls[n].Name == "btnLook" || pnlIdxFld.Controls[n].Name == "chkKeyVal")
                            pnlIdxFld.Controls[n].Visible = false;
                        if (pnlIdxFld.Controls[n].Name == "dtpField" || pnlIdxFld.Controls[n].Name == "txtField"
                            || pnlIdxFld.Controls[n].Name == "cbxField")
                        {
                            pnlIdxFld.Controls[n].BackColor = System.Drawing.Color.White;
                            pnlIdxFld.Controls[n].Text = "";
                            if (pnlIdxFld.Controls[n].Name == "txtField")
                                pnlIdxFld.Controls[n].Visible = true;
                        }
                        if (pnlIdxFld.Controls[n].Name == "lblFieldExist")
                            pnlIdxFld.Controls[n].Text = "";
                        else if (pnlIdxFld.Controls[n].Name == "lblField")
                        {
                            pnlIdxFld.Controls[n].Text = "Document Type";
                            pnlIdxFld.Controls[n].Visible = true;
                        }
                        else if (pnlIdxFld.Controls[n].Name == "chkKeyVal")
                            pnlIdxFld.Controls[n].Text = "Selected";

                        if (pnlIdxFld.Controls[n].Name != "lblField" && pnlIdxFld.Controls[n].Name != "lblFieldExist"
                            && pnlIdxFld.Controls[n].Name != "txtField" && pnlIdxFld.Controls[n].Name != "cbxField"
                            && pnlIdxFld.Controls[n].Name != "btnLook" && pnlIdxFld.Controls[n].Name != "dtpField"
                            && pnlIdxFld.Controls[n].Name != "chkKeyVal")
                        {
                            lDelIdx.Add(pnlIdxFld.Controls[n].Name);
                        }

                        n += 1;
                    }
                }

                if (lDelIdx.Count > 0)
                {
                    n = 0;
                    while (n < lDelIdx.Count)
                    {
                        pnlIdxFld.Controls.RemoveByKey(lDelIdx[n]);

                        n += 1;
                    }
                    pnlIdxFld.Refresh();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Reset fields.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private void resetKeyFieldsConfig()
        {
            try
            {
                //Remove current index fields and init index field to default.
                int n = 0; List<string> lDelIdx = new List<string>();
                if (pnlKeyFld.Controls.Count > 7)
                {
                    while (n < pnlKeyFld.Controls.Count)
                    {
                        if (pnlKeyFld.Controls[n].Name == "cbxKeyField" || pnlKeyFld.Controls[n].Name == "dtpKeyField"
                            || pnlKeyFld.Controls[n].Name == "btnKeyLookup" || pnlKeyFld.Controls[n].Name == "chkKeyFldVal")
                            pnlKeyFld.Controls[n].Visible = false;
                        if (pnlKeyFld.Controls[n].Name == "dtpKeyField" || pnlKeyFld.Controls[n].Name == "txtKeyFld"
                            || pnlKeyFld.Controls[n].Name == "cbxKeyField")
                        {
                            pnlKeyFld.Controls[n].BackColor = System.Drawing.Color.White;
                            pnlKeyFld.Controls[n].Text = "";
                        }
                        if (pnlKeyFld.Controls[n].Name == "lblKeyFieldExist")
                            pnlKeyFld.Controls[n].Text = "";
                        else if (pnlKeyFld.Controls[n].Name == "lblKeyField")
                            pnlKeyFld.Controls[n].Text = "Field";
                        else if (pnlKeyFld.Controls[n].Name == "chkKeyFldVal")
                            pnlKeyFld.Controls[n].Text = "Selected";

                        if (pnlKeyFld.Controls[n].Name != "lblKeyField" && pnlKeyFld.Controls[n].Name != "lblKeyFieldExist"
                            && pnlKeyFld.Controls[n].Name != "txtKeyFld" && pnlKeyFld.Controls[n].Name != "cbxKeyField"
                            && pnlKeyFld.Controls[n].Name != "btnKeyLookup" && pnlKeyFld.Controls[n].Name != "dtpKeyField"
                            && pnlKeyFld.Controls[n].Name != "chkKeyFldVal")
                        {
                            lDelIdx.Add(pnlKeyFld.Controls[n].Name);
                        }

                        n += 1;
                    }
                }

                if (lDelIdx.Count > 0)
                {
                    n = 0;
                    while (n < lDelIdx.Count)
                    {
                        pnlKeyFld.Controls.RemoveByKey(lDelIdx[n]);

                        n += 1;
                    }
                    pnlKeyFld.Refresh();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Reset key fields.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private void loadTableList(dynamic pResult)
        {
            try
            {
                cbxTables.DataSource = null;
                cbxTables.Items.Clear();

                if (pResult != null && _oImgCore.ImageBuffer.HowManyImagesInBuffer > 0)
                {
                    if (pResult.Tables != null)
                    {
                        int i = 0; Dictionary<int, string> cItem = new Dictionary<int, string>();
                        while (i < pResult.Tables.Length)
                        {
                            cItem.Add((i + 1), "Table " + (i + 1));

                            i += 1;
                        }

                        if (cItem.Count > 0)
                            cbxTables.DataSource = new BindingSource(cItem, null);
                        cbxTables.DisplayMember = "Value";
                        cbxTables.ValueMember = "Key";
                        cbxTables.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("load tables.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private void cbxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgdTable.Rows.Count > 0)
                {
                    //dgdTable.Rows.Clear();
                    dgdTable.DataSource = null;
                }
                if (cbxTables.SelectedItem != null)
                {
                    KeyValuePair<int, string> oSel = (KeyValuePair<int, string>)cbxTables.SelectedItem;
                    _tableNum = Convert.ToInt32(oSel.Key.ToString());
                }

                if (cbxTables.IsHandleCreated && cbxTables.Focused)
                {
                    if (resultTableExistDB(sCurrSetNum, _docType, _pageId, _tableNum.ToString()) == false)
                    {
                        populateGridData(_result, _tableNum);
                    }
                    else
                    {
                        populateGridDataDB(sCurrSetNum, _docType, _pageId, _tableNum.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Select table.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private bool loadKeysValuesConfig(KeyValuePair[] oKeyValues, ref int pFldCnt)
        {
            try
            {
                KeyValuePair[] drs = oKeyValues;

                if (drs != null)
                {
                    Label oLblFld = null; TextBox oTxtFld = null; Label oLblExist = null;
                    CheckBox oChkFld = null; DateTimePicker oDtpFld = null; Button oLookBtn = null;
                    System.Drawing.Point currLoc1, currLoc2, currLoc3, currLoc4;
                    int iDefHeight = txtField.Height; int iDefWidth = txtField.Width; List<int[]> lPosition = new List<int[]>(); //Position list.
                    string sKey = "", sVal = "";

                    int j = lPosition.Count; //get last control's index in the list.
                    int i = 0;
                    while (i < drs.Length)
                    {
                        sKey = drs[i].Key.Content.Trim();
                        if (drs[i].Value != null)
                            sVal = drs[i].Value.Content.Trim();
                        else
                            sVal = "";

                        if (i == 0 && j == 0)
                        {
                            if (sVal.ToLower().IndexOf("selected") != -1 || sVal.ToLower().IndexOf("unselected") != -1)
                            {
                                chkKeyFldVal.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Text = "Document Type";
                                lblKeyField.Visible = false;
                                txtKeyFld.Text = "";
                                txtKeyFld.Visible = false;
                                dtpKeyField.Visible = false;
                                cbxKeyField.Visible = false;
                                chkKeyFldVal.Visible = true;

                                lPosition.Add(new int[] { chkKeyFldVal.Location.Y, chkKeyFldVal.Height });

                                chkKeyFldVal.Tag = (i + 1).ToString();
                                chkKeyFldVal.Width = pnlKeyFld.Width - 10;
                                if (chkKeyFldVal.Text.Length >= 30)
                                {
                                    chkKeyFldVal.Text = mGlobal.trimTextWithChars(chkKeyFldVal, 500, "...");
                                }
                                chkKeyFldVal.Width = CHKBOX_MAX_WIDTH;

                                lblKeyFieldExist.Text = sKey + "|" + sVal;
                            }
                            else if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss" }))
                            {
                                lblKeyField.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Visible = true;
                                txtKeyFld.Visible = false;
                                dtpKeyField.Visible = true;
                                cbxKeyField.Visible = false;
                                chkKeyFldVal.Visible = false;
                                chkKeyFldVal.Text = "Selected";

                                dtpKeyField.Tag = (i + 1).ToString();
                                dtpKeyField.Format = DateTimePickerFormat.Custom;

                                if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy" }))
                                {
                                    dtpKeyField.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    dtpKeyField.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                lPosition.Add(new int[] { dtpKeyField.Location.Y, dtpKeyField.Height });

                                dtpKeyField.KeyDown += dtpField_KeyDown;
                                dtpKeyField.GotFocus += keyField_Focus;
                                lblKeyFieldExist.Text = sKey + "|" + sVal;
                            }
                            else //Text.
                            {
                                lblKeyField.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Visible = true;
                                chkKeyFldVal.Visible = false;
                                txtKeyFld.MaxLength = 250;
                                txtKeyFld.Text = "";

                                lPosition.Add(new int[] { txtKeyFld.Location.Y, txtKeyFld.Height });

                                txtKeyFld.Tag = (i + 1).ToString();
                                txtKeyFld.KeyDown += txtField_KeyDown;
                                txtKeyFld.GotFocus += keyField_Focus;

                                lblKeyFieldExist.Text = sKey + "|" + sVal;
                            }

                        }
                        else
                        {
                            if (sVal.ToLower().IndexOf("selected") != -1 || sVal.ToLower().IndexOf("unselected") != -1)
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = chkKeyFldVal.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                //oLblFld = new Label();
                                //oLblFld.AutoSize = true;
                                //oLblFld.Name = "lblKeyField" + (i).ToString();
                                //oLblFld.Location = currLoc1;
                                //oLblFld.Text = sKey;
                                //oLblFld.SendToBack();

                                oChkFld = new CheckBox();
                                oChkFld.Name = "chkKeyFldVal" + (i).ToString();
                                oChkFld.Location = currLoc2;
                                oChkFld.Size = chkKeyFldVal.Size;
                                oChkFld.Tag = (i + 1).ToString();
                                oChkFld.Width = pnlKeyFld.Width - 10;
                                oChkFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                if (oChkFld.Text.Length >= 30)
                                {
                                    oChkFld.Text = mGlobal.trimTextWithChars(oChkFld, 500, "...");
                                }
                                oChkFld.Width = CHKBOX_MAX_WIDTH;
                                oChkFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;

                                oChkFld.Text = mGlobal.sanitizeValue(sKey, " ");

                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oChkFld.Enabled = false;
                                //    oChkFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oChkFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oChkFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oChkFld.BackColor = System.Drawing.Color.White;
                                //}

                                lPosition.Add(new int[] { oChkFld.Location.Y, oChkFld.Height });

                                lblFieldExist.Text = sKey + "|" + sVal;
                                oChkFld.GotFocus += keyField_Focus;

                                //pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oChkFld);
                                pnlKeyFld.Controls.Add(oLblExist);
                            }
                            else if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss" }))
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lblKeyField.Location.Y + (30 * i);
                                //currLoc2 = txtKeyFld.Location;
                                //currLoc2.Y = txtKeyFld.Location.Y + (30 * i);
                                //currLoc3 = lblKeyFieldExist.Location;
                                //currLoc3.Y = lblKeyFieldExist.Location.Y + (30 * i);
                                currLoc1 = lblKeyField.Location;
                                currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = txtKeyFld.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                oLblFld = new Label();
                                oLblFld.AutoSize = true;
                                oLblFld.Name = "lblKeyField" + (i).ToString();
                                oLblFld.Location = currLoc1;
                                oLblFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                oLblFld.SendToBack();

                                oDtpFld = new DateTimePicker();
                                oDtpFld.Name = "dtpKeyField" + (i).ToString();
                                oDtpFld.Location = currLoc2;
                                oDtpFld.Size = dtpKeyField.Size;
                                oDtpFld.Tag = (i + 1).ToString();
                                oDtpFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;

                                //if (currLoc2.Y <= lPosition[i - 1][0])
                                //{
                                //    currLoc1.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc2.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc3.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oDtpFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}                                

                                oDtpFld.Text = DateTime.Now.ToString("dd/MM/yyyy");

                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oDtpFld.Enabled = false;
                                //    oDtpFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oDtpFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oDtpFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oDtpFld.BackColor = System.Drawing.Color.White;
                                //}
                                oDtpFld.Format = DateTimePickerFormat.Custom;

                                if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy" }))
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy HH:mm:ss";
                                }

                                lPosition.Add(new int[] { oDtpFld.Location.Y, oDtpFld.Height });

                                oDtpFld.KeyDown += dtpField_KeyDown;
                                oDtpFld.GotFocus += keyField_Focus;

                                pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oDtpFld);
                                pnlKeyFld.Controls.Add(oLblExist);
                            }
                            else //Text
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lblKeyField.Location.Y + (30 * i);
                                //currLoc2 = txtKeyFld.Location;
                                //currLoc2.Y = txtKeyFld.Location.Y + (30 * i);
                                //currLoc3 = lblKeyFieldExist.Location;
                                //currLoc3.Y = lblKeyFieldExist.Location.Y + (30 * i);
                                currLoc1 = lblKeyField.Location;
                                currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = txtKeyFld.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                oLblFld = new Label();
                                oLblFld.AutoSize = true;
                                oLblFld.Name = "lblKeyField" + (i).ToString();
                                oLblFld.Location = currLoc1;
                                oLblFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                oLblFld.SendToBack();

                                oTxtFld = new TextBox();
                                oTxtFld.Name = "txtKeyFld" + (i).ToString();
                                oTxtFld.Location = currLoc2;
                                oTxtFld.Size = txtKeyFld.Size;
                                oTxtFld.Tag = (i + 1).ToString();
                                oTxtFld.Width = iDefWidth;
                                oTxtFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;
                                oLblExist.SendToBack();

                                //if (currLoc2.Y <= lPosition[i - 1])
                                //{
                                //    currLoc1.Y = lPosition[i - 1] + 5;
                                //    currLoc2.Y = lPosition[i - 1] + 5;
                                //    currLoc3.Y = lPosition[i - 1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oTxtFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}

                                oTxtFld.MaxLength = 250;
                                oTxtFld.Text = "";

                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oTxtFld.Enabled = false;
                                //    oTxtFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oTxtFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oTxtFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oTxtFld.BackColor = System.Drawing.Color.White;
                                //}

                                lPosition.Add(new int[] { oTxtFld.Location.Y, oTxtFld.Height });

                                oTxtFld.KeyDown += txtField_KeyDown;
                                oTxtFld.GotFocus += keyField_Focus;

                                pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oTxtFld);
                                pnlKeyFld.Controls.Add(oLblExist);

                                if (oLblFld.Width >= oTxtFld.Location.X)
                                {
                                    oLblFld.Text = mGlobal.trimValueWithChars(oLblFld, oTxtFld.Location, "...");
                                    oLblFld.SendToBack();
                                    oTxtFld.BringToFront();
                                }
                            }
                        }

                        i += 1;
                        j = lPosition.Count;
                    } //End while.

                    //i = 0;
                    //while (i < drs.Length)
                    //{
                    //    if (staMain.stcIndexSetting.FieldDataType[i].ToString().ToLower() == "lookup")
                    //    {
                    //        Control[] oCtrls = null;
                    //        if ((i - 1) == 0)
                    //        {
                    //            oCtrls = pnlKeyFld.Controls.Find("txtKeyFld", true); //Previous one only.
                    //        }
                    //        else
                    //            oCtrls = pnlKeyFld.Controls.Find("txtKeyFld" + (i - 1), true); //Previous one only.

                    //        if (oCtrls != null)
                    //        {
                    //            if (oCtrls.Length > 0)
                    //            {
                    //                oCtrls[0].KeyDown += lookupField_KeyDown;
                    //                oCtrls[0].GotFocus += keyField_Focus;
                    //            }
                    //        }
                    //    }

                    //    i += 1;
                    //} //End while.
                    pFldCnt = lPosition.Count;
                }
                drs = null;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
            return true;
        }

        private bool loadKeysValuesConfig(SRDocScanIDPForm.KeyValuePair[] oKeyValues, ref int pFldCnt)
        {
            try
            {
                SRDocScanIDPForm.KeyValuePair[] drs = oKeyValues;

                if (drs != null)
                {
                    Label oLblFld = null; TextBox oTxtFld = null; Label oLblExist = null;
                    CheckBox oChkFld = null; DateTimePicker oDtpFld = null; Button oLookBtn = null;
                    System.Drawing.Point currLoc1, currLoc2, currLoc3, currLoc4;
                    int iDefHeight = txtField.Height; int iDefWidth = txtField.Width; List<int[]> lPosition = new List<int[]>(); //Position list.
                    string sKey = "", sVal = "";

                    int j = lPosition.Count; //get last control's index in the list.
                    int i = 0;
                    while (i < drs.Length)
                    {
                        sKey = drs[i].Key.Content.Trim();
                        if (drs[i].Value != null)
                            sVal = drs[i].Value.Content.Trim();
                        else
                            sVal = "";

                        if (i == 0 && j == 0)
                        {
                            if (sVal.ToLower().IndexOf("selected") != -1 || sVal.ToLower().IndexOf("unselected") != -1)
                            {
                                chkKeyFldVal.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Text = "Document Type";
                                lblKeyField.Visible = false;
                                txtKeyFld.Text = "";
                                txtKeyFld.Visible = false;
                                dtpKeyField.Visible = false;
                                cbxKeyField.Visible = false;
                                chkKeyFldVal.Visible = true;

                                lPosition.Add(new int[] { chkKeyFldVal.Location.Y, chkKeyFldVal.Height });

                                chkKeyFldVal.Tag = (i + 1).ToString();
                                chkKeyFldVal.Width = pnlKeyFld.Width - 10;
                                if (chkKeyFldVal.Text.Length >= 30)
                                {
                                    chkKeyFldVal.Text = mGlobal.trimTextWithChars(chkKeyFldVal, 500, "...");
                                }
                                chkKeyFldVal.Width = CHKBOX_MAX_WIDTH;

                                chkKeyFldVal.GotFocus += keyField_Focus;

                                lblKeyFieldExist.Text = sKey + "|" + sVal;
                            }
                            else if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss" }))
                            {
                                lblKeyField.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Visible = true;
                                txtKeyFld.Visible = false;
                                dtpKeyField.Visible = true;
                                cbxKeyField.Visible = false;
                                chkKeyFldVal.Visible = false;
                                chkKeyFldVal.Text = "Selected";

                                dtpKeyField.Tag = (i + 1).ToString();
                                dtpKeyField.Format = DateTimePickerFormat.Custom;

                                if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy" }))
                                {
                                    dtpKeyField.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    dtpKeyField.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                lPosition.Add(new int[] { dtpKeyField.Location.Y, dtpKeyField.Height });

                                dtpKeyField.KeyDown += dtpField_KeyDown;
                                dtpKeyField.GotFocus += keyField_Focus;
                                lblKeyFieldExist.Text = sKey + "|" + sVal;
                            }
                            else //Text.
                            {
                                lblKeyField.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Visible = true;
                                chkKeyFldVal.Visible = false;
                                txtKeyFld.MaxLength = 250;
                                txtKeyFld.Text = "";
                                txtKeyFld.Visible = true;
                                dtpField.Visible = false;

                                lPosition.Add(new int[] { txtKeyFld.Location.Y, txtKeyFld.Height });

                                txtKeyFld.Tag = (i + 1).ToString();
                                txtKeyFld.KeyDown += txtField_KeyDown;
                                txtKeyFld.GotFocus += keyField_Focus;

                                lblKeyFieldExist.Text = sKey + "|" + sVal;
                            }
                        }
                        else
                        {
                            if (sVal.ToLower().IndexOf("selected") != -1 || sVal.ToLower().IndexOf("unselected") != -1)
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = chkKeyFldVal.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                //oLblFld = new Label();
                                //oLblFld.AutoSize = true;
                                //oLblFld.Name = "lblKeyField" + (i).ToString();
                                //oLblFld.Location = currLoc1;
                                //oLblFld.Text = sKey;
                                //oLblFld.SendToBack();

                                oChkFld = new CheckBox();
                                oChkFld.Name = "chkKeyFldVal" + (i).ToString();
                                oChkFld.Location = currLoc2;
                                oChkFld.Size = chkKeyFldVal.Size;
                                oChkFld.Tag = (i + 1).ToString();
                                oChkFld.Width = pnlKeyFld.Width - 10;
                                oChkFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                if (oChkFld.Text.Length >= 30)
                                {
                                    oChkFld.Text = mGlobal.trimTextWithChars(oChkFld, 500, "...");
                                }
                                oChkFld.Width = CHKBOX_MAX_WIDTH;
                                oChkFld.Visible = true;
                                oChkFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;

                                oChkFld.Text = mGlobal.sanitizeValue(sKey, " ");

                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oChkFld.Enabled = false;
                                //    oChkFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oChkFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oChkFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oChkFld.BackColor = System.Drawing.Color.White;
                                //}

                                lPosition.Add(new int[] { oChkFld.Location.Y, oChkFld.Height });

                                oChkFld.GotFocus += keyField_Focus;

                                //pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oChkFld);
                                pnlKeyFld.Controls.Add(oLblExist);
                            }
                            else if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss" }))
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lblKeyField.Location.Y + (30 * i);
                                //currLoc2 = txtKeyFld.Location;
                                //currLoc2.Y = txtKeyFld.Location.Y + (30 * i);
                                //currLoc3 = lblKeyFieldExist.Location;
                                //currLoc3.Y = lblKeyFieldExist.Location.Y + (30 * i);
                                currLoc1 = lblKeyField.Location;
                                currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = txtKeyFld.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                oLblFld = new Label();
                                oLblFld.AutoSize = true;
                                oLblFld.Name = "lblKeyField" + (i).ToString();
                                oLblFld.Location = currLoc1;
                                oLblFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                oLblFld.Visible = true;
                                oLblFld.SendToBack();

                                oDtpFld = new DateTimePicker();
                                oDtpFld.Name = "dtpKeyField" + (i).ToString();
                                oDtpFld.Location = currLoc2;
                                oDtpFld.Size = dtpKeyField.Size;
                                oDtpFld.Tag = (i + 1).ToString();
                                oDtpFld.Visible = true;
                                oDtpFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;

                                //if (currLoc2.Y <= lPosition[i - 1][0])
                                //{
                                //    currLoc1.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc2.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc3.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oDtpFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}                                

                                oDtpFld.Text = DateTime.Now.ToString("dd/MM/yyyy");
                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oDtpFld.Enabled = false;
                                //    oDtpFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oDtpFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oDtpFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oDtpFld.BackColor = System.Drawing.Color.White;
                                //}
                                oDtpFld.Format = DateTimePickerFormat.Custom;

                                if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy" }))
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy HH:mm:ss";
                                }

                                lPosition.Add(new int[] { oDtpFld.Location.Y, oDtpFld.Height });

                                oDtpFld.KeyDown += dtpField_KeyDown;
                                oDtpFld.GotFocus += keyField_Focus;

                                pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oDtpFld);
                                pnlKeyFld.Controls.Add(oLblExist);
                            }
                            else //Text
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lblKeyField.Location.Y + (30 * i);
                                //currLoc2 = txtKeyFld.Location;
                                //currLoc2.Y = txtKeyFld.Location.Y + (30 * i);
                                //currLoc3 = lblKeyFieldExist.Location;
                                //currLoc3.Y = lblKeyFieldExist.Location.Y + (30 * i);
                                currLoc1 = lblKeyField.Location;
                                currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = txtKeyFld.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                oLblFld = new Label();
                                oLblFld.AutoSize = true;
                                oLblFld.Name = "lblKeyField" + (i).ToString();
                                oLblFld.Location = currLoc1;
                                oLblFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                if (oLblFld.Width >= 900)
                                {
                                    oLblFld.Text = mGlobal.trimTextWithChars(oLblFld, 900, "...");
                                }
                                oLblFld.Visible = true;
                                oLblFld.SendToBack();

                                oTxtFld = new TextBox();
                                oTxtFld.Name = "txtKeyFld" + (i).ToString();
                                oTxtFld.Location = currLoc2;
                                oTxtFld.Size = txtKeyFld.Size;
                                oTxtFld.Tag = (i + 1).ToString();
                                oTxtFld.Width = iDefWidth;
                                oTxtFld.Visible = true;
                                oTxtFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;
                                oLblExist.SendToBack();

                                //if (currLoc2.Y <= lPosition[i - 1])
                                //{
                                //    currLoc1.Y = lPosition[i - 1] + 5;
                                //    currLoc2.Y = lPosition[i - 1] + 5;
                                //    currLoc3.Y = lPosition[i - 1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oTxtFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}
                                oTxtFld.MaxLength = 250;
                                oTxtFld.Text = "";

                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oTxtFld.Enabled = false;
                                //    oTxtFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oTxtFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oTxtFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oTxtFld.BackColor = System.Drawing.Color.White;
                                //}
                                lPosition.Add(new int[] { oTxtFld.Location.Y, oTxtFld.Height });

                                oTxtFld.KeyDown += txtField_KeyDown;
                                oTxtFld.GotFocus += keyField_Focus;

                                pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oTxtFld);
                                pnlKeyFld.Controls.Add(oLblExist);

                                if (oLblFld.Width >= oTxtFld.Location.X)
                                {
                                    oLblFld.Text = mGlobal.trimValueWithChars(oLblFld, oTxtFld.Location, "...");
                                    oLblFld.SendToBack();
                                    oTxtFld.BringToFront();
                                }
                            }
                        }

                        i += 1;
                        j = lPosition.Count;
                    } //End while.

                    //i = 0;
                    //while (i < drs.Length)
                    //{
                    //    if (staMain.stcIndexSetting.FieldDataType[i].ToString().ToLower() == "lookup")
                    //    {
                    //        Control[] oCtrls = null;
                    //        if ((i - 1) == 0)
                    //        {
                    //            oCtrls = pnlKeyFld.Controls.Find("txtKeyFld", true); //Previous one only.
                    //        }
                    //        else
                    //            oCtrls = pnlKeyFld.Controls.Find("txtKeyFld" + (i - 1), true); //Previous one only.

                    //        if (oCtrls != null)
                    //        {
                    //            if (oCtrls.Length > 0)
                    //            {
                    //                oCtrls[0].KeyDown += lookupField_KeyDown;
                    //                oCtrls[0].GotFocus += keyField_Focus;
                    //            }
                    //        }
                    //    }

                    //    i += 1;
                    //} //End while.
                    pFldCnt = lPosition.Count;
                }
                drs = null;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
            return true;
        }

        private void loadKeyValues(dynamic oResult)
        {
            try
            {
                DataTable oTabl = new DataTable();
                oTabl.Columns.Add("keys", typeof(string));
                DataRow oRow = null;
                DataRowCollection drs = oTabl.Rows;
                int a = 0; string sKey = "", sVal = "";

                while (a < oResult.KeyValuePairs.Length)
                {
                    sKey = oResult.KeyValuePairs[a].Key.Content.Trim();
                    if (oResult.KeyValuePairs[a].Value != null)
                        sVal = oResult.KeyValuePairs[a].Value.Content.Trim();
                    else
                        sVal = "";

                    if (sVal.Trim().ToLower().IndexOf("unselected") != -1)
                        sVal = "unselected";
                    else if (sVal.Trim().ToLower().IndexOf("selected") != -1)
                        sVal = "selected";

                    oRow = oTabl.NewRow();
                    oRow[0] = sKey + "|" + sVal;
                    drs.Add(oRow);

                    a += 1;
                }
                oTabl.Dispose();

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        int iCnt = oResult.KeyValuePairs.Length;//staMain.stcIndexSetting.RowId.Count;
                        int i = 0, j = 0; Control oCtrl = null; Label oLblExist = null; string sCtrlType = "";
                        TextBox oTxtFld; CheckBox oChk; DateTimePicker oDate; DateTime oDateParsed; string sDate = "";

                        while (i < iCnt)
                        {
                            sKey = oResult.KeyValuePairs[i].Key.Content.Trim();
                            if (oResult.KeyValuePairs[i].Value != null)
                                sVal = oResult.KeyValuePairs[i].Value.Content.Trim();
                            else
                                sVal = "";

                            if (sVal.Trim().ToLower().IndexOf("selected") != -1)
                                sCtrlType = "Check";
                            else if (sVal.Trim().ToLower().IndexOf("unselected") != -1)
                                sCtrlType = "Check";
                            else if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss" }))
                                sCtrlType = "Date";
                            else
                                sCtrlType = "Text";

                            if (sCtrlType == "Text")
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlKeyFld.Controls["txtKeyFld"];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist"];
                                }
                                else
                                {
                                    oCtrl = pnlKeyFld.Controls["txtKeyFld" + i];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist" + i];
                                }
                            }
                            else if (sCtrlType == "Date")
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlKeyFld.Controls["dtpKeyField"];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist"];
                                }
                                else
                                {
                                    oCtrl = pnlKeyFld.Controls["dtpKeyField" + i];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist" + i];
                                }
                            }
                            else //checkbox.
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlKeyFld.Controls["chkKeyFldVal"];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist"];
                                }
                                else
                                {
                                    oCtrl = pnlKeyFld.Controls["chkKeyFldVal" + i];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist" + i];
                                }
                            }

                            if (oLblExist != null) oLblExist.Visible = false;

                            if (sCtrlType == "Date") //Init.
                            {
                                oDate = (DateTimePicker)oCtrl;
                                if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy" }))
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                {
                                    ShortDatePattern = oDate.CustomFormat
                                };

                                oDate.Value = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                            }

                            if (oCtrl != null && oLblExist != null && i < drs.Count)
                            {
                                if (sCtrlType != "Date" && sCtrlType != "Check")
                                    oCtrl.Text = ""; //Init.
                                oLblExist.Text = ""; //Init.                                

                                //j = 0;
                                j = i;
                                while (j < drs.Count)
                                {
                                    if (j < iCnt)
                                    {
                                        if (drs[j][0].ToString().Split("|")[0].Trim() == oResult.KeyValuePairs[j].Key.Content.Trim())
                                        {
                                            if (sCtrlType == "Date")
                                            {
                                                oDate = (DateTimePicker)oCtrl;
                                                //if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                                //{
                                                //    oDate.CustomFormat = "dd/MM/yyyy";
                                                //}
                                                //else
                                                //{
                                                //    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                                //}

                                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                                {
                                                    ShortDatePattern = oDate.CustomFormat
                                                };

                                                oDateParsed = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                                                try
                                                {
                                                    if (drs[j][0].ToString().Split("|")[1].Trim() != string.Empty)
                                                    {
                                                        sDate = mGlobal.sanitizeDateValue(drs[j][0].ToString().Split("|")[1].Trim(), "");
                                                        if (sDate != string.Empty)
                                                        {
                                                            if (!mGlobal.isDate(sDate, new string[] { oDate.CustomFormat }))
                                                                throw new Exception("Date is in invalid format.");

                                                            if (DateTime.TryParse(sDate, out oDateParsed))
                                                            {
                                                                //oDate.Value = Convert.ToDateTime(drs[j][0].ToString().Trim(), formatInfo);
                                                                oDate.Value = oDateParsed;
                                                            }
                                                            else
                                                            {
                                                                oDate.Value = Convert.ToDateTime(sDate, formatInfo);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    mGlobal.Write2Log("load date value.." + sDate + "," + ex.Message);
                                                    //Continue
                                                }
                                            }
                                            else if (sCtrlType == "Text")
                                                oCtrl.Text = drs[j][0].ToString().Split("|")[1].Trim();
                                            else //Checkbox.
                                            {
                                                if (drs[j][0].ToString().Split("|")[1].Trim().ToLower() == "selected")
                                                    ((CheckBox)oCtrl).Checked = true;
                                                else
                                                    ((CheckBox)oCtrl).Checked = false;
                                            }

                                            if (sCtrlType == "Check")
                                                oLblExist.Text = drs[j][0].ToString().Trim();
                                            else
                                                oLblExist.Text = drs[j][0].ToString().Split("|")[1].Trim();

                                            break;
                                        }
                                    }

                                    j += 1;
                                }
                            } //End While.

                            i += 1;
                        } //End While.
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("IDP populating.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private bool saveResultTableColumnsDB(DataTable oTabl, string pSetNum, string pDocType, int pPageId, string pTableNum)
        {
            try
            {
                string sSQL = "";

                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableCols ";
                sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND [setnum]='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [doctype]='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND [pageid]='" + pPageId + "' ";
                sSQL += "AND [tableno]='" + pTableNum.Trim().Replace("'", "") + "' ";

                mGlobal.objDB.UpdateRows(sSQL);

                int i = 0;
                while (i < oTabl.Columns.Count)
                {
                    sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableCols ";
                    sSQL += "([scanproj]";
                    sSQL += ",[appnum]";
                    sSQL += ",[batchcode]";
                    sSQL += ",[setnum]";
                    sSQL += ",[doctype]";
                    sSQL += ",[pageid]";
                    sSQL += ",[tableno]";
                    sSQL += ",[colname]";
                    sSQL += ",[colorder]";
                    sSQL += ") VALUES (";
                    sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pPageId + "'";
                    sSQL += ",'" + pTableNum.Trim().Replace("'", "") + "'";
                    sSQL += ",N'" + oTabl.Columns[i].ColumnName.Trim().Replace("'", "''") + "'";
                    sSQL += ",'" + (i + 1).ToString() + "')";

                    mGlobal.objDB.UpdateRows(sSQL);

                    i += 1;
                }

                if (i > 0) return true;

                return false;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
        }

        private bool saveResultTableDB(string pSetNum, string pDocType, int pPageId, string pTableNum,
            int pRow, int pCol, string pColVal)
        {
            try
            {
                bool bExist = false;
                string sSQL = "";
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND [setnum]='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [doctype]='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND [pageid]='" + pPageId + "' ";
                sSQL += "AND [tableno]='" + pTableNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [rowno]='" + pRow.ToString() + "' ";
                sSQL += "AND [colno]='" + pCol.ToString() + "' ";

                DataRow oRow = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (oRow != null)
                {
                    if (Convert.ToInt32(oRow[0].ToString()) > 0) bExist = true;
                }


                if (bExist)
                {
                    sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                    sSQL += "SET colvalue=N'" + pColVal.Trim().Replace("'", "''") + "',";
                    sSQL += "modifieddate=GETDATE() ";
                    sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [setnum]='" + pSetNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [doctype]='" + pDocType.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [pageid]='" + pPageId + "' ";
                    sSQL += "AND [tableno]='" + pTableNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [rowno]='" + pRow.ToString() + "' ";
                    sSQL += "AND [colno]='" + pCol.ToString() + "' ";

                    mGlobal.objDB.UpdateRows(sSQL);
                }
                else //Insert.
                {
                    sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                    sSQL += "([scanproj]";
                    sSQL += ",[appnum]";
                    sSQL += ",[batchcode]";
                    sSQL += ",[setnum]";
                    sSQL += ",[doctype]";
                    sSQL += ",[pageid]";
                    sSQL += ",[tableno]";
                    sSQL += ",[rowno]";
                    sSQL += ",[colno]";
                    sSQL += ",[colvalue]";
                    sSQL += ") VALUES (";
                    sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pPageId + "'";
                    sSQL += ",'" + pTableNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pRow + "'";
                    sSQL += ",'" + pCol + "'";
                    sSQL += ",N'" + pColVal.Trim().Replace("'", "''") + "')";

                    mGlobal.objDB.UpdateRows(sSQL);
                }

                return true;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
        }

        private bool resultTableExistDB(string pSetNum, string pDocType, int pPageId, string pTableNum)
        {
            try
            {
                bool bExist = false;
                string sSQL = "";
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND [setnum]='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [doctype]='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND [pageid]='" + pPageId + "' ";
                sSQL += "AND [tableno]='" + pTableNum.Trim().Replace("'", "") + "' ";

                DataRow oRow = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (oRow != null)
                {
                    if (Convert.ToInt32(oRow[0].ToString()) > 0) bExist = true;
                }

                return bExist;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
        }

        private void dgdTable_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (_tableNum == 0 || _tableEdit == false) return;
                if (_selRow < 0 || _selCol < 0) return;
                bool bDiff = true; //false;
                string sVal = dgdTable.Rows[_selRow].Cells[_selCol].Value.ToString().Trim();

                //string sSQL = "SELECT IsNull(COUNT(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                //sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                //sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                //sSQL += "AND [setnum]='" + sCurrSetNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND [doctype]='" + _docType.Trim().Replace("'", "") + "' ";
                //sSQL += "AND [pageid]='" + _pageId + "' ";
                //sSQL += "AND [tableno]='" + _tableNum + "' ";
                //sSQL += "AND [rowno]='" + (_selRow + 1).ToString() + "' ";
                //sSQL += "AND [colno]='" + (_selCol + 1).ToString() + "' ";
                //sSQL += "AND [colvalue]='" + sVal.Trim().Replace("'", "''") + "' ";

                //DataRow oRow = mGlobal.objDB.ReturnSingleRow(sSQL);

                //if (oRow != null)
                //{
                //    if (Convert.ToInt32(oRow[0].ToString()) == 0) bDiff = true;
                //}

                if (bDiff)
                {
                    string sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                    sSQL += "SET colvalue='" + sVal.Trim().Replace("'", "''") + "',";
                    sSQL += "modifieddate=GETDATE() ";
                    sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [setnum]='" + sCurrSetNum.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [doctype]='" + _docType.Trim().Replace("'", "") + "' ";
                    sSQL += "AND [pageid]='" + _pageId + "' ";
                    sSQL += "AND [tableno]='" + _tableNum + "' ";
                    sSQL += "AND [rowno]='" + (_selRow + 1).ToString() + "' ";
                    sSQL += "AND [colno]='" + (_selCol + 1).ToString() + "' ";

                    bool bOK = mGlobal.objDB.UpdateRows(sSQL);
                    if (bOK == false)
                        mGlobal.Write2Log("Cell update failed! " + e.RowIndex.ToString() + "," + e.ColumnIndex.ToString());

                    dgdTable.DataSource = null; //Init.
                    populateGridDataDB(sCurrSetNum, _docType, _pageId, _tableNum.ToString()); //Refresh.
                }

                _tableEdit = false;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private void dgdTable_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            _tableEdit = false;
            _selRow = -1;
            _selCol = -1;
        }

        private void dgdTable_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.Cancel == false)
            {
                _tableEdit = true;
                _selRow = e.RowIndex;
                _selCol = e.ColumnIndex;
            }
        }

        private void dgdTable_CancelRowEdit(object sender, QuestionEventArgs e)
        {
            _tableEdit = false;
        }

        private void dgdTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            dgdTable_CellLeave(sender, e);
        }

        private bool saveResultKeysValuesDB(string pSetNum, string pDocType, int pPageId, SRDocScanIDPForm.KeyValuePair[] pKeyValues)
        {
            try
            {
                string sKeyDataType = "";
                string sKeyReg = "";
                string sVal = "";
                string sValReg = "";
                string sConfi = "";
                string sSQL = "";
                mGlobal.LoadAppDBCfg();

                int i = 0;
                while (i < pKeyValues.Length)
                {
                    sKeyReg = SRDocScanIDPForm.SerializeRegion.ToJson(pKeyValues[i].Key.BoundingRegions);

                    if (pKeyValues[i].Value == null)
                        sVal = "";
                    else
                        sVal = pKeyValues[i].Value.Content.ToString();

                    if (sVal.ToLower().IndexOf("selected") != -1 || sVal.ToLower().IndexOf("unselected") != -1)
                    {
                        sKeyDataType = "check";
                    }
                    else if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss" }))
                    {
                        sKeyDataType = "date";
                    }
                    else
                        sKeyDataType = "text";

                    if (sVal != string.Empty)
                    {
                        if (pKeyValues[i].Value.BoundingRegions == null)
                            sValReg = "";
                        else
                            sValReg = SRDocScanIDPForm.SerializeRegion.ToJson(pKeyValues[i].Value.BoundingRegions);
                    }
                    else
                        sValReg = "";

                    if (pKeyValues[i].Confidence == 0)
                        sConfi = "0";
                    else
                        sConfi = pKeyValues[i].Confidence.ToString();

                    sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPKeysValues ";
                    sSQL += "([scanproj]";
                    sSQL += ",[appnum]";
                    sSQL += ",[batchcode]";
                    sSQL += ",[setnum]";
                    sSQL += ",[doctype]";
                    sSQL += ",[pageid]";
                    sSQL += ",[keyid]";
                    sSQL += ",[keyname]";
                    sSQL += ",[keydatatype]";
                    sSQL += ",[keyregion]";
                    sSQL += ",[keyvalue]";
                    sSQL += ",[keyvalueregion]";
                    sSQL += ",[confidence]";
                    sSQL += ") VALUES (";
                    sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                    sSQL += ",'" + pPageId + "'";
                    sSQL += ",'" + (i+1).ToString() + "'";
                    sSQL += ",N'" + pKeyValues[i].Key.Content.Trim().Replace("'", "''") + "'";
                    sSQL += ",'" + sKeyDataType.Trim().Replace("'", "''") + "'";
                    sSQL += ",'" + sKeyReg.Trim().Replace("'", "''") + "'";
                    sSQL += ",N'" + sVal.Trim().Replace("'", "''") + "'";
                    sSQL += ",'" + sValReg.Trim().Replace("'", "''") + "'";
                    sSQL += ",'" + sConfi.Trim().Replace("'", "''") + "')";

                    mGlobal.objDB.UpdateRows(sSQL);

                    i += 1;
                }

                return true;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
        }

        private bool resultKeysValuesExistDB(string pSetNum, string pDocType, int pPageId)
        {
            try
            {
                bool bExist = false;
                string sSQL = "";
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPKeysValues ";
                sSQL += "WHERE [scanproj]='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND [appnum]='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [batchcode]='" + _batchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND [setnum]='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND [doctype]='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND [pageid]='" + pPageId + "' ";

                DataRow oRow = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (oRow != null)
                {
                    if (Convert.ToInt32(oRow[0].ToString()) > 0) bExist = true;
                }

                return bExist;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
        }

        private void tabMainFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabMainFields.SelectedTab == tabFields)
                {
                    if (_fldCnt == 0)
                    {
                        lblField.Visible = false;
                        txtField.Visible = false;
                    }
                    else
                    {
                        lblField.Visible = true;
                        txtField.Visible = true;
                    }
                }
                else if (tabMainFields.SelectedTab == tabKeyValues)
                {
                    //if (pnlKeyFld.Height >= tabKeyValues.Height)
                    //{
                    //    tabKeyValues.Height = pnlKeyFld.Height + 35;
                    //    tabKeyValues.Update();
                    //    tabKeyValues.Refresh();
                    //}
                    if (_keyFldCnt == 0)
                    {
                        lblKeyField.Visible = false;
                        txtKeyFld.Visible = false;
                    }
                    else
                    {
                        if (lblKeyField.Text.Trim().ToLower() != "document type")
                        {
                            lblKeyField.Visible = true;
                            txtKeyFld.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
        }

        private bool saveDocKeyFieldDB(string pSetNum, string pDocType, int pPageId, int pKeyFldCnt)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            int i = 0;
            Control oCtrl = null;
            string sKeyName = "";
            string sValue = "";
            string sDataType = "text";
            try
            {
                mGlobal.LoadAppDBCfg();

                bExist = oDF.checkDocuSetKeyFieldExist(_currScanProj, _appNum, _batchCode, pSetNum, pDocType, pPageId.ToString(), "");
                if (bExist)
                {
                    string sFldId = "0";
                    while (i < pKeyFldCnt)
                    {
                        if (i == 0)
                            oCtrl = pnlKeyFld.Controls["lblKeyField"];
                        else
                            oCtrl = pnlKeyFld.Controls["lblKeyField" + i];

                        if (oCtrl != null)
                            sKeyName = oCtrl.Text.Trim();
                        else
                            sKeyName = "";
                        
                        oCtrl = null;
                        sValue = "";

                        if (i == 0)
                            oCtrl = pnlKeyFld.Controls["txtKeyFld"];
                        else
                            oCtrl = pnlKeyFld.Controls["txtKeyFld" + i];

                        if (oCtrl != null) sDataType = "text";

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["dtpKeyField"];
                            else
                                oCtrl = pnlKeyFld.Controls["dtpKeyField" + i];

                            if (oCtrl != null) sDataType = "date";
                        }

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["chkKeyFldVal"];
                            else
                                oCtrl = pnlKeyFld.Controls["chkKeyFldVal" + i];

                            CheckBox oChk = (CheckBox)oCtrl;
                            if (oChk != null && oChk.Visible)
                            {
                                sKeyName = oChk.Text.Trim();
                                sValue = oChk.Checked.ToString();
                                sDataType = "check";
                            }
                        }
                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["cbxKeyField"];
                            else
                                oCtrl = pnlKeyFld.Controls["cbxKeyField" + i];

                            if (oCtrl != null) sDataType = "combo";

                            ComboBox oCbx = (ComboBox)oCtrl;
                            if (oCbx.SelectedValue != null)
                                sValue = oCbx.SelectedValue.ToString();
                            else
                            {
                                MessageBox.Show(this, "Key field value is not selected or not matched! Please select from the list.", "Message");
                                throw new Exception("Key field value is not selected or not matched!");
                            }
                        }
                        else if (sValue.Trim() == string.Empty)
                            sValue = oCtrl.Text;

                        sFldId = oCtrl.Tag.ToString().Trim();
                        bExist = oDF.checkDocuSetKeyFieldExist(_currScanProj, _appNum, _batchCode, pSetNum, pDocType, pPageId.ToString(), sFldId);

                        if (bExist)
                        {
                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                            sSQL += "SET [fldvalue]=N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",[keydatatype]='" + sDataType.Trim().Replace("'", "") + "'";
                            sSQL += ",[modifieddate]='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                            sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                            sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                            sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";                            
                            sSQL += "AND pageid='" + pPageId + "' ";
                            sSQL += "AND fieldid='" + sFldId.Trim().Replace("'", "") + "' ";
                            sSQL += "AND keyname=N'" + sKeyName.Trim().Replace("'", "''") + "' ";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Update document key field data failed!");
                            }
                        }
                        else
                        {
                            sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                            sSQL += "([scanpjcode]";
                            sSQL += ",[sysappnum]";
                            sSQL += ",[batchcode]";
                            sSQL += ",[setnum]";
                            sSQL += ",[doctype]";
                            sSQL += ",[pageid]";
                            sSQL += ",[fieldid]";
                            sSQL += ",[keyname]";
                            sSQL += ",[fldvalue]";
                            sSQL += ",[keydatatype]";
                            sSQL += ") VALUES (";
                            sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + pPageId + "'";
                            sSQL += ",'" + sFldId.Trim().Replace("'", "") + "'";
                            sSQL += ",N'" + sKeyName.Trim().Replace("'", "''") + "'";
                            sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",'" + sDataType.Trim().Replace("'", "") + "')";


                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Saving document key field data failed!");
                            }
                        }

                        i += 1;
                    }

                }
                else
                {
                    string sFldId = "0";
                    while (i < pKeyFldCnt)
                    {
                        sFldId = "0";
                        if (i == 0)
                            oCtrl = pnlKeyFld.Controls["lblKeyField"];
                        else
                            oCtrl = pnlKeyFld.Controls["lblKeyField" + i];

                        if (oCtrl != null)
                            sKeyName = oCtrl.Text.Trim();
                        else
                            sKeyName = "";

                        oCtrl = null;
                        sValue = "";

                        if (i == 0)
                            oCtrl = pnlKeyFld.Controls["txtKeyFld"];
                        else
                            oCtrl = pnlKeyFld.Controls["txtKeyFld" + i];

                        if (oCtrl != null) sDataType = "text";

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["dtpKeyField"];
                            else
                                oCtrl = pnlKeyFld.Controls["dtpKeyField" + i];

                            if (oCtrl != null) sDataType = "date";
                        }

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["chkKeyFldVal"];
                            else
                                oCtrl = pnlKeyFld.Controls["chkKeyFldVal" + i];
                            CheckBox oChk = (CheckBox)oCtrl;
                            if (oChk != null && oChk.Visible)
                            {
                                sKeyName = oChk.Text.Trim();
                                sValue = oChk.Checked.ToString();
                                sDataType = "check";
                            }
                        }
                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["cbxKeyField"];
                            else
                                oCtrl = pnlKeyFld.Controls["cbxKeyField" + i];

                            if (oCtrl != null) sDataType = "combo";

                            ComboBox oCbx = (ComboBox)oCtrl;
                            if (oCbx.SelectedValue != null)
                                sValue = oCbx.SelectedValue.ToString();
                            else
                            {
                                MessageBox.Show(this, "Key field value is not selected or not matched! Please select from the list.", "Message");
                                throw new Exception("Key field value is not selected or not matched!");
                            }
                        }
                        else if (sValue.Trim() == string.Empty)
                            sValue = oCtrl.Text;

                        sFldId = oCtrl.Tag.ToString().Trim();

                        sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                        sSQL += "([scanpjcode]";
                        sSQL += ",[sysappnum]";
                        sSQL += ",[batchcode]";
                        sSQL += ",[setnum]";
                        sSQL += ",[doctype]";
                        sSQL += ",[pageid]";
                        sSQL += ",[fieldid]";
                        sSQL += ",[keyname]";
                        sSQL += ",[fldvalue]";
                        sSQL += ",[keydatatype]";
                        sSQL += ") VALUES (";
                        sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + pPageId + "'";
                        sSQL += ",'" + sFldId.Trim().Replace("'", "") + "'";
                        sSQL += ",N'" + sKeyName.Trim().Replace("'", "''") + "'";
                        sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                        sSQL += ",'" + sDataType.Trim().Replace("'", "") + "')";

                        if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                        {
                            bSaved = false;
                            throw new Exception("Saving document key field data failed!!");
                        }

                        i += 1;
                    }
                }

            }
            catch (Exception ex)
            {
                bSaved = false;
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool saveDocKeyFieldDB(string pSetNum, string pNewSetNum, string pDocType, int pPageId, int pKeyFldCnt)
        {
            bool bSaved = true;
            string sSQL = "";
            bool bExist = false;
            int i = 0;
            Control oCtrl = null;
            string sKeyName = "";
            string sValue = "";
            string sDataType = "text";
            try
            {
                mGlobal.LoadAppDBCfg();

                bExist = oDF.checkDocuSetKeyFieldExist(_currScanProj, _appNum, _batchCode, pSetNum, pDocType, pPageId.ToString(), "");
                if (bExist)
                {
                    string sFldId = "0";
                    while (i < pKeyFldCnt)
                    {
                        if (i == 0)
                            oCtrl = pnlKeyFld.Controls["lblKeyField"];
                        else
                            oCtrl = pnlKeyFld.Controls["lblKeyField" + i];

                        if (oCtrl != null)
                            sKeyName = oCtrl.Text.Trim();
                        else
                            sKeyName = "";

                        oCtrl = null;
                        sValue = "";

                        if (i == 0)
                            oCtrl = pnlKeyFld.Controls["txtKeyFld"];
                        else
                            oCtrl = pnlKeyFld.Controls["txtKeyFld" + i];

                        if (oCtrl != null) sDataType = "text";

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["dtpKeyField"];
                            else
                                oCtrl = pnlKeyFld.Controls["dtpKeyField" + i];

                            if (oCtrl != null) sDataType = "date";
                        }

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["chkKeyFldVal"];
                            else
                                oCtrl = pnlKeyFld.Controls["chkKeyFldVal" + i];

                            CheckBox oChk = (CheckBox)oCtrl;
                            if (oChk != null && oChk.Visible)
                            {
                                sKeyName = oChk.Text.Trim();
                                sValue = oChk.Checked.ToString();
                                sDataType = "check";
                            }
                        }
                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["cbxKeyField"];
                            else
                                oCtrl = pnlKeyFld.Controls["cbxKeyField" + i];

                            if (oCtrl != null) sDataType = "combo";

                            ComboBox oCbx = (ComboBox)oCtrl;
                            if (oCbx.SelectedValue != null)
                                sValue = oCbx.SelectedValue.ToString();
                            else
                            {
                                MessageBox.Show(this, "Key field value is not selected or not matched! Please select from the list.", "Message");
                                throw new Exception("Key field value is not selected or not matched!");
                            }
                        }
                        else if (sValue.Trim() == string.Empty)
                            sValue = oCtrl.Text;

                        sFldId = oCtrl.Tag.ToString().Trim();
                        bExist = oDF.checkDocuSetKeyFieldExist(_currScanProj, _appNum, _batchCode, pNewSetNum, pDocType, pPageId.ToString(), sFldId);

                        if (bExist)
                        {
                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                            sSQL += "SET [fldvalue]=N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",[keydatatype]='" + sDataType.Trim().Replace("'", "") + "'";
                            sSQL += ",[modifieddate]='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                            sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                            sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND batchcode='" + _batchCode.Trim().Replace("'", "") + "' ";
                            sSQL += "AND setnum='" + pNewSetNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                            sSQL += "AND pageid='" + pPageId + "' ";
                            sSQL += "AND fieldid='" + sFldId.Trim().Replace("'", "") + "' ";
                            sSQL += "AND keyname=N'" + sKeyName.Trim().Replace("'", "''") + "' ";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Update document key field data failed!");
                            }
                        }
                        else
                        {
                            sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                            sSQL += "([scanpjcode]";
                            sSQL += ",[sysappnum]";
                            sSQL += ",[batchcode]";
                            sSQL += ",[setnum]";
                            sSQL += ",[doctype]";
                            sSQL += ",[pageid]";
                            sSQL += ",[fieldid]";
                            sSQL += ",[keyname]";
                            sSQL += ",[fldvalue]";
                            sSQL += ",[keydatatype]";
                            sSQL += ") VALUES (";
                            sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + pNewSetNum.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                            sSQL += ",'" + pPageId + "'";
                            sSQL += ",'" + sFldId.Trim().Replace("'", "") + "'";
                            sSQL += ",N'" + sKeyName.Trim().Replace("'", "''") + "'";
                            sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                            sSQL += ",'" + sDataType.Trim().Replace("'", "") + "')";

                            if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                            {
                                bSaved = false;
                                throw new Exception("Saving document key field data failed!");
                            }
                        }

                        i += 1;
                    }

                }
                else
                {
                    string sFldId = "0";
                    while (i < pKeyFldCnt)
                    {
                        sFldId = "0";
                        if (i == 0)
                            oCtrl = pnlKeyFld.Controls["lblKeyField"];
                        else
                            oCtrl = pnlKeyFld.Controls["lblKeyField" + i];

                        if (oCtrl != null)
                            sKeyName = oCtrl.Text.Trim();
                        else
                            sKeyName = "";

                        oCtrl = null;
                        sValue = "";

                        if (i == 0)
                            oCtrl = pnlKeyFld.Controls["txtKeyFld"];
                        else
                            oCtrl = pnlKeyFld.Controls["txtKeyFld" + i];

                        if (oCtrl != null) sDataType = "text";

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["dtpKeyField"];
                            else
                                oCtrl = pnlKeyFld.Controls["dtpKeyField" + i];

                            if (oCtrl != null) sDataType = "date";
                        }

                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["chkKeyFldVal"];
                            else
                                oCtrl = pnlKeyFld.Controls["chkKeyFldVal" + i];
                            CheckBox oChk = (CheckBox)oCtrl;
                            if (oChk != null && oChk.Visible)
                            {
                                sKeyName = oChk.Text.Trim();
                                sValue = oChk.Checked.ToString();
                                sDataType = "check";
                            }
                        }
                        if (oCtrl == null || oCtrl.Visible == false)
                        {
                            if (i == 0)
                                oCtrl = pnlKeyFld.Controls["cbxKeyField"];
                            else
                                oCtrl = pnlKeyFld.Controls["cbxKeyField" + i];

                            if (oCtrl != null) sDataType = "combo";

                            ComboBox oCbx = (ComboBox)oCtrl;
                            if (oCbx.SelectedValue != null)
                                sValue = oCbx.SelectedValue.ToString();
                            else
                            {
                                MessageBox.Show(this, "Key field value is not selected or not matched! Please select from the list.", "Message");
                                throw new Exception("Key field value is not selected or not matched!");
                            }
                        }
                        else if (sValue.Trim() == string.Empty)
                            sValue = oCtrl.Text;

                        sFldId = oCtrl.Tag.ToString().Trim();

                        sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                        sSQL += "([scanpjcode]";
                        sSQL += ",[sysappnum]";
                        sSQL += ",[batchcode]";
                        sSQL += ",[setnum]";
                        sSQL += ",[doctype]";
                        sSQL += ",[pageid]";
                        sSQL += ",[fieldid]";
                        sSQL += ",[keyname]";
                        sSQL += ",[fldvalue]";
                        sSQL += ",[keydatatype]";
                        sSQL += ") VALUES (";
                        sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + _batchCode.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + pNewSetNum.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                        sSQL += ",'" + pPageId + "'";
                        sSQL += ",'" + sFldId.Trim().Replace("'", "") + "'";
                        sSQL += ",N'" + sKeyName.Trim().Replace("'", "''") + "'";
                        sSQL += ",N'" + sValue.Trim().Replace("'", "''") + "'";
                        sSQL += ",'" + sDataType.Trim().Replace("'", "") + "')";

                        if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                        {
                            bSaved = false;
                            throw new Exception("Saving document key field data failed!!");
                        }

                        i += 1;
                    }
                }

            }
            catch (Exception ex)
            {
                bSaved = false;
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        private bool updatePageKeyFieldDB(string pSetNum, string pDocType, string pOldPageId, string pPageId)
        {
            try
            {
                oDF.updatePageKeyFieldDB(_currScanProj, _appNum, _batchCode, pSetNum, pDocType, pOldPageId, pPageId);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private bool syncPageKeyFieldDB(string pBatchCode, string pSetNum, string pDocType, string pTableName, string pPageId = "NULL")
        {
            try
            {
                mGlobal.LoadAppDBCfg();

                string sSQL = "SELECT IsNULL(COUNT(*),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                if (pPageId.Trim().ToUpper() == "NULL")
                    sSQL += "AND pageid IS NULL ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    if (Convert.ToInt32(dr[0].ToString()) == 0) return false;
                }

                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT rowid,setnum,doctype,imageseq ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + pTableName.Trim().Replace("'", "") + " ";
                sSQL += "WHERE scanproj='" + _currScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + _appNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY imageseq,createddate DESC ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    mGlobal.LoadAppDBCfg();

                    int i = 0, iImgSeq; string sPageId = "", sSetNo = "", sDocType = "";
                    while (i < drs.Count)
                    {
                        sPageId = drs[i][0].ToString();
                        sSetNo = drs[i][1].ToString();
                        sDocType = drs[i][2].ToString();
                        iImgSeq = Convert.ToInt32(drs[i][3].ToString());
                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0")
                            sSetNo = iImgSeq.ToString(sSetNumFmt);

                        sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                        sSQL += "SET pageid=" + sPageId + " ";
                        sSQL += "WHERE scanpjcode='" + _currScanProj.Trim().Replace("'", "") + "' ";
                        sSQL += "AND sysappnum='" + _appNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                        sSQL += "AND setnum='" + sSetNo.Trim().Replace("'", "") + "' ";
                        sSQL += "AND doctype='" + sDocType.Trim().Replace("'", "") + "' ";
                        if (pPageId.Trim().ToUpper() == "NULL")
                            sSQL += "AND pageid IS NULL ";

                        mGlobal.objDB.UpdateRows(sSQL);

                        i += 1;
                    }
                }

                dr = null;
                drs = null;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private void loadIndexKeyFieldValueDB(string pSetNum, string pDocType, int pPageId)
        {
            try
            {
                DataRowCollection drs = oDF.getIndexedKeyFieldValueByPageDB(_currScanProj, _appNum, _batchCode, pSetNum, pDocType, pPageId.ToString());

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        int iCnt = drs.Count;//staMain.stcIndexSetting.RowId.Count;
                        int i = 0, j = 0; Control oCtrl = null; Label oLblExist = null; string sCtrlType = "";
                        ComboBox oCbx; DateTimePicker oDate; DateTime oDateParsed; string sDate = "";
                        string sKey = "", sVal = "", sDataType = "text";

                        while (i < iCnt)
                        {
                            sDataType = drs[i]["keydatatype"].ToString().Trim();
                            sKey = drs[i]["keyname"].ToString().Trim();
                            if (drs[i]["fldvalue"] != null)
                                sVal = drs[i]["fldvalue"].ToString().Trim();
                            else
                                sVal = "";

                            if (sDataType.ToLower() == "check")
                                sCtrlType = "Check";
                            else if (sDataType.ToLower() == "date")
                                sCtrlType = "Date";
                            else
                                sCtrlType = "Text";

                            if (sCtrlType == "Text")
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlKeyFld.Controls["txtKeyFld"];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist"];
                                }
                                else
                                {
                                    oCtrl = pnlKeyFld.Controls["txtKeyFld" + i];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist" + i];
                                }
                            }
                            else if (sCtrlType == "Date")
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlKeyFld.Controls["dtpKeyField"];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist"];
                                }
                                else
                                {
                                    oCtrl = pnlKeyFld.Controls["dtpKeyField" + i];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist" + i];
                                }
                            }
                            else //checkbox.
                            {
                                if (i == 0)
                                {
                                    oCtrl = pnlKeyFld.Controls["chkKeyFldVal"];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist"];
                                }
                                else
                                {
                                    oCtrl = pnlKeyFld.Controls["chkKeyFldVal" + i];
                                    oLblExist = (Label)pnlKeyFld.Controls["lblKeyFieldExist" + i];
                                }
                            }

                            if (oLblExist != null) oLblExist.Visible = false;

                            if (sCtrlType == "Date") //Init.
                            {
                                oDate = (DateTimePicker)oCtrl;
                                if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy" }))
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                {
                                    ShortDatePattern = oDate.CustomFormat
                                };

                                oDate.Value = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                            }

                            if (oCtrl != null && oLblExist != null && i < drs.Count)
                            {
                                if (sCtrlType != "Date" && sCtrlType != "Check")
                                    oCtrl.Text = ""; //Init.
                                oLblExist.Text = ""; //Init.                                

                                //j = 0;
                                j = i;
                                while (j < drs.Count)
                                {
                                    if (j < iCnt)
                                    {
                                        if (sCtrlType == "Date")
                                        {
                                            oDate = (DateTimePicker)oCtrl;
                                            //if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                            //{
                                            //    oDate.CustomFormat = "dd/MM/yyyy";
                                            //}
                                            //else
                                            //{
                                            //    oDate.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                            //}

                                            var formatInfo = new System.Globalization.DateTimeFormatInfo()
                                            {
                                                ShortDatePattern = oDate.CustomFormat
                                            };

                                            oDateParsed = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").Trim(), formatInfo);
                                            try
                                            {
                                                if (drs[j]["fldvalue"].ToString().Trim() != string.Empty)
                                                {
                                                    sDate = mGlobal.sanitizeDateValue(drs[j]["fldvalue"].ToString().Trim(), "");
                                                    if (sDate != string.Empty)
                                                    {
                                                        if (!mGlobal.isDate(sDate, new string[] { oDate.CustomFormat }))
                                                            throw new Exception("Date is in invalid format.");

                                                        if (DateTime.TryParse(sDate, out oDateParsed))
                                                        {
                                                            //oDate.Value = Convert.ToDateTime(drs[j][0].ToString().Trim(), formatInfo);
                                                            oDate.Value = oDateParsed;
                                                        }
                                                        else
                                                        {
                                                            oDate.Value = Convert.ToDateTime(sDate, formatInfo);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                mGlobal.Write2Log("load date value.." + sDate + "," + ex.Message);
                                                //Continue
                                            }
                                        }
                                        else if (sCtrlType == "Text")
                                            oCtrl.Text = drs[j]["fldvalue"].ToString().Trim();
                                        else //Checkbox.
                                        {
                                            if (drs[j]["fldvalue"].ToString().Trim().ToLower() == "true")
                                                ((CheckBox)oCtrl).Checked = true;
                                            else
                                                ((CheckBox)oCtrl).Checked = false;
                                        }

                                        oLblExist.Text = drs[j]["fldvalue"].ToString().Trim();

                                        break;
                                    }

                                    j += 1;
                                }
                            } //End While.

                            i += 1;
                        } //End While.
                    }
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("IDP populating keys.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
        }

        private bool loadIndexKeyFieldConfigDB(string pSetNum, string pDocType, int pPageId, ref int pFldCnt)
        {
            try
            {
                DataRowCollection drs = oDF.getIndexedKeyFieldValueByPageDB(_currScanProj, _appNum, _batchCode, pSetNum,
                    pDocType, pPageId.ToString());

                if (drs != null)
                {
                    Label oLblFld = null; TextBox oTxtFld = null; Label oLblExist = null;
                    CheckBox oChkFld = null; DateTimePicker oDtpFld = null; Button oLookBtn = null;
                    System.Drawing.Point currLoc1, currLoc2, currLoc3, currLoc4;
                    int iDefHeight = txtField.Height; int iDefWidth = txtField.Width; List<int[]> lPosition = new List<int[]>(); //Position list.
                    string sKey = "", sVal = "", sDataType = "text";

                    int j = lPosition.Count; //get last control's index in the list.
                    int i = 0;
                    while (i < drs.Count)
                    {
                        sDataType = drs[i]["keydatatype"].ToString().Trim();
                        sKey = drs[i]["keyname"].ToString().Trim();
                        if (drs[i]["fldvalue"] != null)
                            sVal = drs[i]["fldvalue"].ToString().Trim();
                        else
                            sVal = "";

                        if (i == 0 && j == 0)
                        {
                            if (sDataType.Trim().ToLower() == "check")
                            {
                                chkKeyFldVal.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Text = "Document Type";
                                lblKeyField.Visible = false;
                                txtKeyFld.Text = "";
                                txtKeyFld.Visible = false;
                                dtpKeyField.Visible = false;
                                cbxKeyField.Visible = false;
                                chkKeyFldVal.Visible = true;

                                lPosition.Add(new int[] { chkKeyFldVal.Location.Y, chkKeyFldVal.Height });

                                chkKeyFldVal.Tag = (i + 1).ToString();
                                chkKeyFldVal.Width = pnlKeyFld.Width - 10;
                                if (chkKeyFldVal.Text.Length >= 30)
                                {
                                    chkKeyFldVal.Text = mGlobal.trimTextWithChars(chkKeyFldVal, 500, "...");
                                }
                                chkKeyFldVal.Width = CHKBOX_MAX_WIDTH;

                                lblKeyFieldExist.Text = sKey + "|" + sVal;
                            }
                            else if (sDataType.Trim().ToLower() == "date")
                            {
                                lblKeyField.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Visible = true;
                                txtKeyFld.Visible = false;
                                dtpKeyField.Visible = true;
                                cbxKeyField.Visible = false;
                                chkKeyFldVal.Visible = false;
                                chkKeyFldVal.Text = "Selected";

                                dtpKeyField.Tag = (i + 1).ToString();
                                dtpKeyField.Format = DateTimePickerFormat.Custom;

                                if (mGlobal.isDate(sVal, new string[] { "dd/MM/yyyy" }))
                                {
                                    dtpKeyField.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    dtpKeyField.CustomFormat = "dd/MM/yyyy hh:mm:ss";
                                }

                                lPosition.Add(new int[] { dtpKeyField.Location.Y, dtpKeyField.Height });

                                dtpKeyField.KeyDown += dtpField_KeyDown;
                                dtpKeyField.GotFocus += keyField_Focus;
                                lblKeyFieldExist.Text = sKey + "|" + sVal;
                            }
                            else //Text.
                            {
                                lblKeyField.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                lblKeyField.Visible = true;
                                chkKeyFldVal.Visible = false;
                                txtKeyFld.MaxLength = 250;
                                txtKeyFld.Text = "";

                                lPosition.Add(new int[] { txtKeyFld.Location.Y, txtKeyFld.Height });

                                txtKeyFld.Tag = (i + 1).ToString();
                                txtKeyFld.KeyDown += txtField_KeyDown;
                                txtKeyFld.GotFocus += keyField_Focus;

                                lblKeyFieldExist.Text = sKey + "|" + sVal;

                                if (lblKeyField.Width >= txtKeyFld.Location.X)
                                {
                                    lblKeyField.Text = mGlobal.trimValueWithChars(lblKeyField, txtKeyFld.Location, "...");
                                    lblKeyField.SendToBack();
                                    txtKeyFld.BringToFront();
                                }
                            }

                        }
                        else
                        {
                            if (sDataType.Trim().ToLower() == "check")
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = chkKeyFldVal.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                //oLblFld = new Label();
                                //oLblFld.AutoSize = true;
                                //oLblFld.Name = "lblKeyField" + (i).ToString();
                                //oLblFld.Location = currLoc1;
                                //oLblFld.Text = sKey;
                                //oLblFld.SendToBack();

                                oChkFld = new CheckBox();
                                oChkFld.Name = "chkKeyFldVal" + (i).ToString();
                                oChkFld.Location = currLoc2;
                                oChkFld.Size = chkKeyFldVal.Size;
                                oChkFld.Tag = (i + 1).ToString();
                                oChkFld.Width = pnlKeyFld.Width - 10;
                                oChkFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                if (oChkFld.Text.Length >= 30)
                                {
                                    oChkFld.Text = mGlobal.trimTextWithChars(oChkFld, 500, "...");
                                }
                                oChkFld.Width = CHKBOX_MAX_WIDTH;
                                oChkFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;

                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oChkFld.Enabled = false;
                                //    oChkFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oChkFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oChkFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oChkFld.BackColor = System.Drawing.Color.White;
                                //}

                                lPosition.Add(new int[] { oChkFld.Location.Y, oChkFld.Height });

                                lblFieldExist.Text = sKey + "|" + sVal;
                                oChkFld.GotFocus += keyField_Focus;

                                //pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oChkFld);
                                pnlKeyFld.Controls.Add(oLblExist);
                            }
                            else if (sDataType.Trim().ToLower() == "date")
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lblKeyField.Location.Y + (30 * i);
                                //currLoc2 = txtKeyFld.Location;
                                //currLoc2.Y = txtKeyFld.Location.Y + (30 * i);
                                //currLoc3 = lblKeyFieldExist.Location;
                                //currLoc3.Y = lblKeyFieldExist.Location.Y + (30 * i);
                                currLoc1 = lblKeyField.Location;
                                currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = txtKeyFld.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                oLblFld = new Label();
                                oLblFld.AutoSize = true;
                                oLblFld.Name = "lblKeyField" + (i).ToString();
                                oLblFld.Location = currLoc1;
                                oLblFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                oLblFld.SendToBack();

                                oDtpFld = new DateTimePicker();
                                oDtpFld.Name = "dtpKeyField" + (i).ToString();
                                oDtpFld.Location = currLoc2;
                                oDtpFld.Size = dtpKeyField.Size;
                                oDtpFld.Tag = (i + 1).ToString();
                                oDtpFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;

                                //if (currLoc2.Y <= lPosition[i - 1][0])
                                //{
                                //    currLoc1.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc2.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    currLoc3.Y = (currLoc1.Y) + lPosition[i - 1][1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oDtpFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}                                

                                oDtpFld.Text = DateTime.Now.ToString("dd/MM/yyyy");

                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oDtpFld.Enabled = false;
                                //    oDtpFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oDtpFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oDtpFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oDtpFld.BackColor = System.Drawing.Color.White;
                                //}
                                oDtpFld.Format = DateTimePickerFormat.Custom;

                                if (staMain.stcIndexSetting.FieldDataType[i].Trim().ToLower() == "date")
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy";
                                }
                                else
                                {
                                    oDtpFld.CustomFormat = "dd/MM/yyyy HH:mm:ss";
                                }

                                lPosition.Add(new int[] { oDtpFld.Location.Y, oDtpFld.Height });

                                oDtpFld.KeyDown += dtpField_KeyDown;
                                oDtpFld.GotFocus += keyField_Focus;

                                pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oDtpFld);
                                pnlKeyFld.Controls.Add(oLblExist);
                            }
                            else //Text
                            {
                                //currLoc1 = lblKeyField.Location;
                                //currLoc1.Y = lblKeyField.Location.Y + (30 * i);
                                //currLoc2 = txtKeyFld.Location;
                                //currLoc2.Y = txtKeyFld.Location.Y + (30 * i);
                                //currLoc3 = lblKeyFieldExist.Location;
                                //currLoc3.Y = lblKeyFieldExist.Location.Y + (30 * i);
                                currLoc1 = lblKeyField.Location;
                                currLoc1.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc2 = txtKeyFld.Location;
                                currLoc2.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;
                                currLoc3 = lblKeyFieldExist.Location;
                                currLoc3.Y = lPosition[j - 1][0] + lPosition[j - 1][1] + 5;

                                oLblFld = new Label();
                                oLblFld.AutoSize = true;
                                oLblFld.Name = "lblKeyField" + (i).ToString();
                                oLblFld.Location = currLoc1;
                                oLblFld.Text = mGlobal.sanitizeValue(sKey, " ").Trim();
                                oLblFld.SendToBack();

                                oTxtFld = new TextBox();
                                oTxtFld.Name = "txtKeyFld" + (i).ToString();
                                oTxtFld.Location = currLoc2;
                                oTxtFld.Size = txtKeyFld.Size;
                                oTxtFld.Tag = (i + 1).ToString();
                                oTxtFld.Width = iDefWidth;
                                oTxtFld.BringToFront();

                                oLblExist = new Label();
                                oLblExist.Name = "lblKeyFieldExist" + (i).ToString();
                                oLblExist.Location = currLoc3;
                                oLblExist.Visible = false;
                                oLblExist.Text = sKey + "|" + sVal;
                                oLblExist.SendToBack();

                                //if (currLoc2.Y <= lPosition[i - 1])
                                //{
                                //    currLoc1.Y = lPosition[i - 1] + 5;
                                //    currLoc2.Y = lPosition[i - 1] + 5;
                                //    currLoc3.Y = lPosition[i - 1] + 5;
                                //    oLblFld.Location = currLoc1;
                                //    oTxtFld.Location = currLoc2;
                                //    oLblExist.Location = currLoc3;
                                //}

                                oTxtFld.MaxLength = 250;
                                oTxtFld.Text = "";

                                //if (staMain.stcIndexSetting.FieldEdit[i].ToString().ToUpper() == "N")
                                //{
                                //    oTxtFld.Enabled = false;
                                //    oTxtFld.BackColor = SystemColors.ControlLight;
                                //}
                                //else
                                //{
                                //    oTxtFld.Enabled = true;
                                //    if (staMain.stcIndexSetting.FieldMand[i].ToString().ToUpper() == "Y")
                                //        oTxtFld.BackColor = System.Drawing.Color.LightYellow;
                                //    else
                                //        oTxtFld.BackColor = System.Drawing.Color.White;
                                //}

                                lPosition.Add(new int[] { oTxtFld.Location.Y, oTxtFld.Height });

                                oTxtFld.KeyDown += txtField_KeyDown;
                                oTxtFld.GotFocus += keyField_Focus;

                                pnlKeyFld.Controls.Add(oLblFld);
                                pnlKeyFld.Controls.Add(oTxtFld);
                                pnlKeyFld.Controls.Add(oLblExist);

                                if (oLblFld.Width >= oTxtFld.Location.X)
                                {
                                    oLblFld.Text = mGlobal.trimValueWithChars(oLblFld, oTxtFld.Location, "...");
                                    oLblFld.SendToBack();
                                    oTxtFld.BringToFront();
                                }
                            }
                        }

                        i += 1;
                        j = lPosition.Count;
                    } //End while.

                    //i = 0;
                    //while (i < drs.Length)
                    //{
                    //    if (staMain.stcIndexSetting.FieldDataType[i].ToString().ToLower() == "lookup")
                    //    {
                    //        Control[] oCtrls = null;
                    //        if ((i - 1) == 0)
                    //        {
                    //            oCtrls = pnlKeyFld.Controls.Find("txtKeyFld", true); //Previous one only.
                    //        }
                    //        else
                    //            oCtrls = pnlKeyFld.Controls.Find("txtKeyFld" + (i - 1), true); //Previous one only.

                    //        if (oCtrls != null)
                    //        {
                    //            if (oCtrls.Length > 0)
                    //            {
                    //                oCtrls[0].KeyDown += lookupField_KeyDown;
                    //                oCtrls[0].GotFocus += keyField_Focus;
                    //            }
                    //        }
                    //    }

                    //    i += 1;
                    //} //End while.
                    pFldCnt = lPosition.Count;
                }
                drs = null;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
            return true;
        }




    }
}
