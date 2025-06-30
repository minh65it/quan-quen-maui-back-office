using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using QuanQuenBackOfficeMobile;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Models;
using System.Collections.Generic;
using System.Collections.Generic;

namespace QuanQuenBackOfficeMobile.Models;

    public class LevelApiResponse
    {
        public PaginationInfo Pagination { get; set; }
        public List<LevelDto> Result { get; set; }
    }

    //public class PaginationInfo
    //{
    //    public int TotalCount { get; set; }
    //    public int TotalPage { get; set; }
    //    public int PageNumber { get; set; }
    //    public int PageSize { get; set; }
    //    public int Skip { get; set; }
    //}

    public class LevelDto
    {
        public int Id { get; set; }
        public string LevelName { get; set; }
        public List<LevelDetailDto> LevelDetails { get; set; }
    }

    public class LevelDetailDto
    {
        public int LevelDetailId { get; set; }
        public int LevelId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
    }
    public enum Status
    {
        [Display(Name = "Khóa")]
        Block = -1,

        [Display(Name = "Đang chờ")]
        Pending,

        [Display(Name = "Đã duyệt")]
        Approve,

        [Display(Name = "Hoạt động")]
        Active,

        [Display(Name = "Không hoạt động")]
        InActive,

        [Display(Name = "Đang tạo")]
        Creating
    }
