using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace JobDistributor
{
    class PNInfoListItemSorter : IComparer
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int m_iColumnToSort;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder m_orderOfSort;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public PNInfoListItemSorter()
        {
            // Initialize the column to '0'
            m_iColumnToSort = 0;

            // Initialize the sort order to 'none'
            m_orderOfSort = SortOrder.None;
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int iCompareResult = 0;
            ListViewItem.ListViewSubItem itemX, itemY;

            // Cast the objects to be compared to ListViewItem objects
            itemX = ((ListViewItem)x).SubItems[m_iColumnToSort];
            itemY = ((ListViewItem)y).SubItems[m_iColumnToSort];

            if (m_iColumnToSort == (int)PNItemIndex.InstanceName)
            {
                iCompareResult = itemX.Text.CompareTo(itemY.Text);
                if (m_orderOfSort == SortOrder.Descending)
                {
                    iCompareResult = -itemX.Text.CompareTo(itemY.Text);
                }
            }
            else if (m_iColumnToSort == (int)PNItemIndex.WaitingJobCount)
            {
                iCompareResult = CompareInt(itemX.Text, itemY.Text, m_orderOfSort);
            }
            else if (m_iColumnToSort == (int)PNItemIndex.RunningJobCount)
            {
                iCompareResult = CompareInt(itemX.Text, itemY.Text, m_orderOfSort);
            }
            else if (m_iColumnToSort == (int)PNItemIndex.CPUUsage)
            {
                iCompareResult = CompareDouble(itemX.Text, itemY.Text, m_orderOfSort);
            }
            else if (m_iColumnToSort == (int)PNItemIndex.FreeDiskSpace)
            {
                iCompareResult = CompareDouble(itemX.Text, itemY.Text, m_orderOfSort);
            }
            else if (m_iColumnToSort == (int)PNItemIndex.AvailablePhysicalMemory)
            {
                iCompareResult = CompareDouble(itemX.Text, itemY.Text, m_orderOfSort);
            }
            else if (m_iColumnToSort == (int)PNItemIndex.AvailableVirtualMemory)
            {
                iCompareResult = CompareDouble(itemX.Text, itemY.Text, m_orderOfSort);
            }

            return iCompareResult;
        }

        private int CompareInt(string strX, string strY, SortOrder sortOrder)
        {
            int iX = 0;
            try
            {
                iX = Int32.Parse(strX);
            }
            catch (Exception) { }
            int iY = 0;
            try
            {
                iY = Int32.Parse(strY);
            }
            catch (Exception) { }

            int iRet = 0;
            if (iX > iY)
            {
                iRet = 1;
            }
            else if (iX < iY)
            {
                iRet = -1;
            }

            if (sortOrder == SortOrder.Descending)
            {
                iRet = -iRet;
            }

            return iRet;
        }

        private int CompareDouble(string strX, string strY, SortOrder sortOrder)
        {
            double dblX = 0;
            try
            {
                dblX = double.Parse(strX);
            }
            catch (Exception) { }
            double dblY = 0;
            try
            {
                dblY = double.Parse(strY);
            }
            catch (Exception) { }

            int iRet = 0;
            if (dblX > dblY)
            {
                iRet = 1;
            }
            else if (dblX < dblY)
            {
                iRet = -1;
            }

            if (sortOrder == SortOrder.Descending)
            {
                iRet = -iRet;
            }

            return iRet;
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set
            {
                m_iColumnToSort = value;
            }
            get
            {
                return m_iColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set
            {
                m_orderOfSort = value;
            }
            get
            {
                return m_orderOfSort;
            }
        }
    }

    class JobInfoListItemSorter : IComparer
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int m_iColumnToSort;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder m_orderOfSort;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public JobInfoListItemSorter()
        {
            // Initialize the column to '0'
            m_iColumnToSort = 0;

            // Initialize the sort order to 'none'
            m_orderOfSort = SortOrder.None;
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int iCompareResult = 0;
            ListViewItem.ListViewSubItem itemX, itemY;

            // Cast the objects to be compared to ListViewItem objects
            itemX = ((ListViewItem)x).SubItems[m_iColumnToSort];
            itemY = ((ListViewItem)y).SubItems[m_iColumnToSort];

            if ((m_iColumnToSort == (int)JobItemIndex.PNName) 
                || (m_iColumnToSort == (int)JobItemIndex.Status)
                || (m_iColumnToSort == (int)JobItemIndex.ExeName))
            {
                iCompareResult = itemX.Text.CompareTo(itemY.Text);
                if (m_orderOfSort == SortOrder.Descending)
                {
                    iCompareResult = -itemX.Text.CompareTo(itemY.Text);
                }
            }

            return iCompareResult;
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set
            {
                m_iColumnToSort = value;
            }
            get
            {
                return m_iColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set
            {
                m_orderOfSort = value;
            }
            get
            {
                return m_orderOfSort;
            }
        }
    }
}
