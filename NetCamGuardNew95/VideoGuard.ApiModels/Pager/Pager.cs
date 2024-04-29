using EnumCode;
using System;
using System.Collections.Generic;
using System.Text; 

namespace VideoGuard.Business
{
    public class AttPager:IPager
    {  
        private string _Search; 
        public string Search
        {
            get
            {
                return _Search;
            }
            set
            {
                _Search = value;
            }
        }
        /// <summary>
        /// For X.PagedList.Mvc.Core 
        /// Parameter Name : Page 
        /// </summary>
        public virtual int Page
        {
            get
            {
                return _PageNo;
            }
            set
            {
                _PageNo = value;
            }
        }
        private int _PageNo; 
        public int PageNo
        {
            get
            {
                return _PageNo;
            }
            set
            {
                _PageNo = value;
            }
        }
        private int _PageSize; 
        public int PageSize
        {
            get
            {
                return _PageSize;
            }
            set
            {
                _PageSize = value;
            }
        } 
        private int _TotalCount; 
        public int TotalCount
        {
            get
            {
                return _TotalCount;
            }
            set
            {
                _TotalCount = value;
            }
        }
        private int _TotalPage;
        public int TotalPage
        {
            get
            {
                return _TotalPage;
            }
            set
            {
                _TotalPage = value;
            }
        }

        private SortOrderCode _SortOrder;
        public SortOrderCode SortOrder
        {
            get
            {
                return _SortOrder;
            }
            set
            {
                _SortOrder = value;
            }
        } 
    }


    public interface IPager
    {
        public string Search { get; set; }
        public int PageNo { get; set; } 
        public int PageSize { get; set; } 
        public int TotalCount { get; set; } 
        public int TotalPage { get; set; }
        public SortOrderCode SortOrder { get; set; }
    }

    public class ListPager: IListPager
    {
        private string _Search;
        public string Search
        {
            get
            {
                return _Search;
            }
            set
            {
                _Search = value;
            }
        }

        public int PageTotal { get; set; }
        /// <summary>
        /// PageIndex 第几页
        /// </summary>
        public int Page { get; set; } 
         
        private int _PageSize;
        public int PageSize
        {
            get
            {
                return _PageSize;
            }
            set
            {
                _PageSize = value;
            }
        }
          
        private SortOrderCode _SortOrder;
        public SortOrderCode SortOrder
        {
            get
            {
                return _SortOrder;
            }
            set
            {
                _SortOrder = value;
            }
        }
    }
    public interface IListPager
    {
        public string Search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int PageTotal { get; set; }
        public SortOrderCode SortOrder { get; set; }
    }
}
