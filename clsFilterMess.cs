using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; //Necessary

namespace SRDocScanIDP
{
    class clsMouseFilterMess : IMessageFilter //This interface allows an application to capture a message before it is dispatched to a control or form
    {
        private Form FParent;

        public clsMouseFilterMess(Form pParent)
        {
            FParent = pParent;
        }

        public bool PreFilterMessage(ref Message m)
        {
            bool bReturn = true;
            try
            {
                //Check for mouse movements and/or clicks
                bool bMouse = (m.Msg >= 0x200 & m.Msg <= 0x20d) | (m.Msg >= 0xa0 & m.Msg <= 0xad);

                //Check for keyboard button presses
                bool bKbd = false; //(m.Msg >= 0x100 & m.Msg <= 0x109);

                if (mGlobal.blnIsLogin)
                {
                    //if any of these events occur
                    if (bMouse | bKbd)
                    {
                        //mGlobal.Write2Log("Waking up");
                        //wake up
                        staMain.sessionRestart();
                        //bReturn = true;
                        bReturn = false;
                    }
                    else if (MDIMain.bSessTimeout)
                    {
                        //mGlobal.Write2Log("Sleeping");
                        bReturn = false;
                    }
                    else
                        bReturn = false;
                }
                else
                    bReturn = false;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("PreFilter.." + ex.Message);
            }           

            return bReturn;
        }
    }

    class clsMouseKbdFilterMess : IMessageFilter //This interface allows an application to capture a message before it is dispatched to a control or form
    {
        private Form FParent;

        public clsMouseKbdFilterMess(Form pParent)
        {
            FParent = pParent;
        }

        public bool PreFilterMessage(ref Message m)
        {
            bool bReturn = true;
            try
            {
                //Check for mouse movements and/or clicks
                bool bMouse = (m.Msg >= 0x200 & m.Msg <= 0x20d) | (m.Msg >= 0xa0 & m.Msg <= 0xad);

                //Check for keyboard button presses
                bool bKbd = (m.Msg >= 0x100 & m.Msg <= 0x109);

                if (mGlobal.blnIsLogin)
                {
                    //if any of these events occur
                    if (bMouse | bKbd)
                    {
                        //mGlobal.Write2Log("Waking up");
                        //wake up
                        staMain.sessionRestart();
                        //bReturn = true; //Looping or Hanged.
                        bReturn = false; //no looping or no hang.
                    }
                    else if (MDIMain.bSessTimeout)
                    {
                        //mGlobal.Write2Log("Sleeping");
                        bReturn = false;
                    }
                    else
                        bReturn = false;
                }
                else
                    bReturn = false;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("PreFilter.." + ex.Message);
            }

            return bReturn;
        }
    }




}
