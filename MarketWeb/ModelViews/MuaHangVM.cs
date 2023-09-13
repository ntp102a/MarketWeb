using MarketWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

namespace MarketWeb.ModelViews
{
    public class MuaHangVM
    {
        public int CustomerId { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập Họ và Tên")]
        public string FullName { get; set; }
        public string Email { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        public string Phone { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Địa chỉ nhận hàng")]
        public string Address { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành")]
        public int TinhThanh { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn Quận/Huyện")]
        public int QuanHuyen { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn Phường/Xã")]
        public int PhuongXa { get; set; }
        public int PaymentID { get; set; }
        public string Note { get; set; }
    }
}
