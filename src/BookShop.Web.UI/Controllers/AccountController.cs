﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookShop.Core.Users;
using BookShop.Web.UI.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Web.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // 1- gelen modeli dogrula
            if (ModelState.IsValid)
            {
                // 1.1- Bu kullanici adina kayitli kullanici var mi bak
                var existUser = await _userManager.FindByEmailAsync(model.Username);

                // 1.2- Yoksa hata don
                if (existUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Boyle bir kullanici adi sistmemize kayitli degil!");
                    return View(model);
                }
                // 1.3- Kullanici adi ve sifre eslesmesi | eslestiyse giris yap
                var login = await _signInManager.PasswordSignInAsync(model.Username, model.Password, true, lockoutOnFailure: false);
                // 1.4- Eslesmediyse hata don
                if (!login.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Şifreniz yanlış!");
                    return View(model);
                }
                // 1.5- Giriş başarılı ise iletişime gönder
                return RedirectToAction("Contact", "Home");
            }
            // 2- Model hataliysa view'a gonder
            return View(model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        // Veritabanına veri ekleyeceğimiz için HttpPost yazıyoruz.
        // Veriyi eş zamanlı olarak (async) getirmesini sağlıyoruz.
        // ValidateAntiForgeryToken; kendisine gönderilen AntiForgeryToken’ı decrypt işleminden geçirerek, validation işlemlerini gerçekleştirmektedir.Eğer validation işlemlerinden geçemezse bu noktada exception’a düşmektedir.
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // kayıt etme işlemleri
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    

                    EmailConfirmed = true,
                    TwoFactorEnabled = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Home");
                }
                AddErrors(result);
            }

            return View(model);
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
        }

    }
}