﻿using AspNetCoreHero.ToastNotification.Abstractions;
using MarketWeb.Helpper;
using MarketWeb.Extension;
using MarketWeb.Models;
using MarketWeb.ModelViews;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace MarketWeb.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly dbMarketsContext _context;
        public INotyfService _notyfService { get; }
        public AccountsController(dbMarketsContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if (khachhang == null)
                {
                    return Json(data: "Số điện thoại : " + Phone + "Đã được đăng ký");
                }
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang == null)
                {
                    return Json(data: "Email : " + Email + "Đã được đăng ký");
                }
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [Route("tai-khoan-cua-toi.html", Name = "Dashboard")]
        public IActionResult Dashboard()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers
                    .AsNoTracking()
                    .SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    var lsDonhang = _context.Orders
                        .Include(x => x.TransactStatus)
                        .AsNoTracking()
                        .Where(x => x.CustomerId == khachhang.CustomerId)
                        .OrderByDescending(x => x.OrderDate).ToList();
                    ViewBag.Donhang = lsDonhang;
                    return View(khachhang);
                }
            }
            return RedirectToAction("Login");
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public IActionResult DangkyTaiKhoan()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public async Task<IActionResult> DangkyTaiKhoan(RegisterVM taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string salt = Utilities.GetRandomKey();
                    Customer khachhang = new Customer
                    {
                        FullName = taikhoan.Fullname,
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = (taikhoan.Password + salt.Trim()).ToMD5(),
                        CreateDate = DateTime.Now,
                        Active = true,
                        Salt = salt,
                        RoleID = 2
                    };
                    try
                    {
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();

                        //Luu session CustomerID
                        HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("CustomerId");

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, khachhang.FullName),
                            new Claim("CustomerId", khachhang.CustomerId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        _notyfService.Success("Đăng ký thành công");
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                    catch
                    {
                        return RedirectToAction("DangKyTaiKhoan", "Accounts");
                    }
                }
                else
                {
                    return View(taikhoan);
                }
            }
            catch
            {
                return View(taikhoan);
            }
        }

        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public IActionResult Login(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Index", "Home");
            }


            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel customer, string returnUrl = null)
        {
            try
            {
                bool isEmail = Utilities.IsValidEmail(customer.UserName);
                if (!isEmail) return View(customer);

                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);

                if (khachhang == null) return RedirectToAction("DangKyTaiKhoan");

                string pass = (customer.Password + khachhang.Salt.Trim()).ToMD5();

                if (khachhang.Password != pass)
                {
                    _notyfService.Error("Thông tin đăng nhập chưa chính xác");
                    return View(customer);
                }

                if (khachhang.Active == false) return RedirectToAction("ThongBao", "Accounts");

                HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                var taikhoanID = HttpContext.Session.GetString("CustomerId");

                var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, khachhang.FullName),
                        new Claim("CustomerId", khachhang.CustomerId.ToString()),
                        new Claim(ClaimTypes.Role, khachhang.RoleID.ToString())
                    };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                _notyfService.Success("Đăng nhập thành công");

                if (khachhang.RoleID == 1)
                {
                    return RedirectToAction("Index", "AdminOrders", new { Area = "Admin" });
                }
                else
                {
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
                
            }
            catch
            {
                _notyfService.Error("Đăng nhập không thành công");
                return RedirectToAction("DangKyTaiKhoan", "Accounts");
            }
        }

        [HttpGet]
        [Route("dang-xuat.html", Name = "Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                var taikhoan = _context.Customers.Find(Convert.ToInt32(taikhoanID));
                if (taikhoan == null) return RedirectToAction("Login", "Accounts");

                var pass = (model.PasswordNow.Trim() + taikhoan.Salt.Trim()).ToMD5();
                if (pass == taikhoan.Password)
                {
                    string passNew = (model.Password.Trim() + taikhoan.Salt.Trim()).ToMD5();
                    taikhoan.Password = passNew;
                    _context.Update(taikhoan);
                    _context.SaveChanges();
                    _notyfService.Success("Thay đổi mật khẩu thành công");
                    return RedirectToAction("Dashboard", "Accounts");
                }
            }
            catch
            {
                _notyfService.Error("Thay đổi mật khẩu không thành công");
                return RedirectToAction("Dashboard", "Accounts");
            }
            _notyfService.Error("Thay đổi mật khẩu không thành công");
            return RedirectToAction("Dashboard", "Accounts");
        }
    }
}
