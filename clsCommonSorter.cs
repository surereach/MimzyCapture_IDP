using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SRDocScanIDP
{
    public class clsCommonSorter : IComparer
    {
        private SortOrder OrderOfSort;

        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        public int Compare(Object x, Object y)
        {
            string XVal = x.ToString();
            string YVal = y.ToString();
            //XVal = x.ToString().Substring(6, 4) + x.ToString().Substring(3, 2) + x.ToString().Substring(0, 2) + " " + x.ToString().Substring(11, 2) + x.ToString().Substring(14, 2) + x.ToString().Substring(17, 2);
            //YVal = y.ToString().Substring(6, 4) + y.ToString().Substring(3, 2) + y.ToString().Substring(0, 2) + " " + y.ToString().Substring(11, 2) + y.ToString().Substring(14, 2) + y.ToString().Substring(17, 2);

            int compareResult;
            compareResult = (new CaseInsensitiveComparer()).Compare(XVal, YVal);

            if (OrderOfSort == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }       

        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }
    }

    public class clsDateCompareFileInfo : IComparer<FileInfo>
    {
        /// <summary>
        /// Compare the last dates of the File infos
        /// </summary>
        /// <param name="fi1">First FileInfo to check</param>
        /// <param name="fi2">Second FileInfo to check</param>
        /// <returns></returns>
        public int Compare(FileInfo fi1, FileInfo fi2)
        {
            int result;
            if (fi1.CreationTime == fi2.CreationTime)
            {
                result = 0;
            }
            else if (fi1.CreationTime < fi2.CreationTime)
            {
                result = 1;
            }
            else
            {
                result = -1;
            }

            return result;
        }
    }
}
