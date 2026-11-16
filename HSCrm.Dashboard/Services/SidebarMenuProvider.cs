using HSCrm.Dashboard.Costants;
using HSCrm.Dashboard.Models;
using HSCrm.Dashboard.Services.Interface;
using System.Security.Claims;

namespace HSCrm.Dashboard.Services
{
    public class SidebarMenuProvider : ISidebarMenuProvider
    {
        public List<SidebarMenuItem> GetMenus(ClaimsPrincipal user)
        {
            var allMenus = BuildMenus();
            return FilterMenusByPermission(allMenus, user);
        }

        private List<SidebarMenuItem> FilterMenusByPermission(
            List<SidebarMenuItem> menus,
            ClaimsPrincipal user)
        {
            var filtered = new List<SidebarMenuItem>();

            foreach (var menu in menus)
            {
                if (!string.IsNullOrWhiteSpace(menu.AllowedRoles))
                {
                    var allowedRoles = menu.AllowedRoles
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (!allowedRoles.Any(user.IsInRole))
                        continue;
                }

                bool hasPermission = string.IsNullOrEmpty(menu.PermissionKey)
                                     || menu.PermissionKey
                                         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                         .Any(perm => user.Claims.Any(c =>
                                             string.Equals(c.Type, "Permission", StringComparison.OrdinalIgnoreCase) &&
                                             string.Equals(c.Value, perm, StringComparison.OrdinalIgnoreCase)));
                if (!hasPermission)
                    continue;

                if (menu.Children != null && menu.Children.Any())
                {
                    var visibleChildren = FilterMenusByPermission(menu.Children, user);
                    if (!visibleChildren.Any())
                        continue;

                    menu.Children = visibleChildren;
                    filtered.Add(menu);
                    continue;
                }

                filtered.Add(menu);
            }

            return filtered;
        }

        public List<SidebarMenuItem> BuildMenus()
        {
            return new List<SidebarMenuItem>
            {
                // 1. پیشخوان و کارتابل
                new SidebarMenuItem
                {
                    Category = "پیشخوان و کارتابل",
                    Key = "dashboard",
                    Title = "داشبورد مدیریتی",
                    Icon = "mdi mdi-view-dashboard",
                    Url = "/AdminArea/Home/Index",
                    PermissionKey = Permissions.AccountingReports.View
                },
                new SidebarMenuItem
                {
                    Category = "پیشخوان و کارتابل",
                    Key = "employee_panel",
                    Title = "کارتابل کارمند و کارکرد من",
                    Icon = "mdi mdi-account-clock-outline",
                    Url = "/AdminArea/Employee/Index",
                },

                // 2. اطلاعات پایه، پروژه‌ها و انبار
                new SidebarMenuItem
                {
                    Category = "اطلاعات پایه، پروژه‌ها و انبار",
                    Key = "customers",
                    Title = "مشتریان",
                    Icon = "mdi mdi-account-group",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "customers_list",
                            Title = "لیست مشتری‌ها",
                            Url = "/AdminArea/Customer/Index",
                            PermissionKey = Permissions.Customers.View
                        }
                    }
                },
                new SidebarMenuItem
                {
                    Category = "اطلاعات پایه، پروژه‌ها و انبار",
                    Key = "suppliers",
                    Title = "تأمین‌کنندگان",
                    Icon = "mdi mdi-truck",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "suppliers_list",
                            Title = "لیست تامین‌کنندگان",
                            Url = "/AdminArea/Supplier/Index",
                            PermissionKey = Permissions.Suppliers.View
                        }
                    }
                },
                new SidebarMenuItem
                {
                    Category = "اطلاعات پایه، پروژه‌ها و انبار",
                    Key = "projects",
                    Title = "پروژه‌ها",
                    Icon = "mdi mdi-folder-outline",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "projects_list",
                            Title = "لیست پروژه‌ها",
                            Url = "/AdminArea/Project/Index",
                            PermissionKey = Permissions.Projects.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "project_worklogs",
                            Title = "ساعات کاری و تایم‌شیت",
                            Url = "/AdminArea/ProjectWorkLog/Index",
                            PermissionKey = Permissions.WorkLogs.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "project_progress",
                            Title = "پیشرفت و مستندات پروژه",
                            Url = "/AdminArea/ProjectProgress/Index",
                            PermissionKey = Permissions.ProjectProgress.View
                        }
                    }
                },
                new SidebarMenuItem
                {
                    Category = "اطلاعات پایه، پروژه‌ها و انبار",
                    Key = "products",
                    Title = "محصولات و قیمت‌گذاری",
                    Icon = "mdi mdi-cube-outline",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "products_list",
                            Title = "لیست محصولات",
                            Url = "/AdminArea/Product/Index",
                            PermissionKey = Permissions.Products.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "product_kardex",
                            Title = "کاردکس محصول",
                            Url = "/AdminArea/Inventory/KardexReport",
                            PermissionKey = Permissions.Inventories.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "product_prices",
                            Title = "قیمت‌ها",
                            Url = "/AdminArea/ProductPrice/Index",
                            PermissionKey = Permissions.ProductPrices.View
                        }
                    }
                },
                new SidebarMenuItem
                {
                    Category = "اطلاعات پایه، پروژه‌ها و انبار",
                    Key = "inventories",
                    Title = "انبارها",
                    Icon = "mdi mdi-warehouse",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "warehouses_list",
                            Title = "لیست انبارها",
                            Url = "/AdminArea/Warehouse/Index",
                            PermissionKey = Permissions.Warehouses.View,
                            AllowedRoles = "Owner"
                        }
                    }
                },

                // 3. فروش، خرید و خزانه‌داری
                new SidebarMenuItem
                {
                    Category = "فروش، خرید و خزانه‌داری",
                    Key = "invoices",
                    Title = "فاکتورها و برگشتی‌ها",
                    Icon = "mdi mdi-receipt",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "sales_invoices",
                            Title = "فاکتورهای فروش",
                            Url = "/AdminArea/Invoice/SalesInvoice",
                            PermissionKey = Permissions.SalesInvoices.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "add_sales_invoice",
                            Title = "ثبت فاکتور فروش جدید",
                            Url = "/AdminArea/Invoice/AddSaleInvoice",
                            PermissionKey = Permissions.SalesInvoices.Create
                        },
                        new SidebarMenuItem
                        {
                            Key = "purchase_invoices",
                            Title = "فاکتورهای خرید",
                            Url = "/AdminArea/Invoice/PurchaseInvoice",
                            PermissionKey = Permissions.PurchaseInvoices.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "add_purchase_invoice",
                            Title = "ثبت فاکتور خرید جدید",
                            Url = "/AdminArea/Invoice/AddPurchaseInvoice",
                            PermissionKey = Permissions.PurchaseInvoices.Create
                        },
                        new SidebarMenuItem
                        {
                            Key = "sales_return_invoices",
                            Title = "برگشت از فروش",
                            Url = "/AdminArea/ReturnInvoice/SalesReturn",
                            PermissionKey = Permissions.SalesReturnInvoices.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "purchase_return_invoices",
                            Title = "برگشت از خرید",
                            Url = "/AdminArea/ReturnInvoice/PurchaseReturn",
                            PermissionKey = Permissions.PurchaseReturnInvoices.View
                        }
                    }
                },
                new SidebarMenuItem
                {
                    Category = "فروش، خرید و خزانه‌داری",
                    Key = "treasury",
                    Title = "خزانه‌داری، بانک و چک",
                    Icon = "mdi mdi-bank",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "bank_accounts",
                            Title = "حساب‌های بانکی و صندوق",
                            Url = "/AdminArea/BankAccount/Index",
                            PermissionKey = Permissions.BankAccounts.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "receipts_list",
                            Title = "دریافت‌ها و واریزی‌ها",
                            Url = "/AdminArea/Receipt/Index",
                            PermissionKey = Permissions.Receipt.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "payments",
                            Title = "پرداخت‌ها",
                            Url = "/AdminArea/Payment/Index",
                            PermissionKey = Permissions.Payments.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "cheques_list",
                            Title = "چک‌های صیادی و رأس‌گیری",
                            Url = "/AdminArea/Cheque/Index",
                            PermissionKey = Permissions.Cheques.View
                        }
                    }
                },
                new SidebarMenuItem
                {
                    Category = "فروش، خرید و خزانه‌داری",
                    Key = "expenses",
                    Title = "هزینه‌ها",
                    Icon = "mdi mdi-cash",
                    Url = "/AdminArea/Expense/Index",
                    PermissionKey = $"{Permissions.Expenses.View},{Permissions.Expenses.Create}"
                },

                // 4. حسابداری و گزارشات
                new SidebarMenuItem
                {
                    Category = "حسابداری و گزارشات",
                    Key = "accounting",
                    Title = "حسابداری و گزارشات مالی",
                    Icon = "mdi mdi-calculator-variant",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "accounting_accounts",
                            Title = "درختواره و کدینگ حساب‌ها",
                            Url = "/AdminArea/Accounting/Accounts",
                            PermissionKey = Permissions.AccountingAccounts.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "accounting_vouchers",
                            Title = "اسناد حسابداری (دفتر روزنامه)",
                            Url = "/AdminArea/Accounting/Vouchers",
                            PermissionKey = Permissions.JournalVouchers.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "accounting_reports",
                            Title = "تراز آزمایشی، معین و سود/زیان",
                            Url = "/AdminArea/Accounting/Reports",
                            PermissionKey = Permissions.AccountingReports.View
                        },
                        new SidebarMenuItem
                        {
                            Key = "accounting_aging",
                            Title = "صورت‌حساب ۳۶۰ و تحلیل سنی",
                            Url = "/AdminArea/Accounting/AgingAndStatements",
                            PermissionKey = Permissions.AccountingReports.View
                        }
                    }
                },
                new SidebarMenuItem
                {
                    Category = "حسابداری و گزارشات",
                    Key = "fiscal_years",
                    Title = "سال مالی و بستن حساب‌ها",
                    Icon = "mdi mdi-calendar-check",
                    Url = "/AdminArea/FiscalYear/Index",
                    PermissionKey = Permissions.FiscalYears.View
                },
                new SidebarMenuItem
                {
                    Category = "حسابداری و گزارشات",
                    Key = "audit_trail",
                    Title = "تاریخچه تغییرات (Audit Trail)",
                    Icon = "mdi mdi-history",
                    Url = "/AdminArea/AuditTrail/Index",
                    PermissionKey = Permissions.AuditTrail.View
                },

                // 5. مدیریت و تنظیمات
                new SidebarMenuItem
                {
                    Category = "مدیریت و تنظیمات",
                    Key = "users",
                    Title = "کاربران و نقش‌ها",
                    Icon = "mdi mdi-account",
                    Children = new List<SidebarMenuItem>
                    {
                        new SidebarMenuItem
                        {
                            Key = "users_list",
                            Title = "لیست کاربران",
                            Url = "/AdminArea/User/Index",
                            PermissionKey = Permissions.Users.View,
                            AllowedRoles = "Owner"
                        },
                        new SidebarMenuItem
                        {
                            Key = "roles_list",
                            Title = "نقش‌ها",
                            Url = "/AdminArea/Role/Index",
                            PermissionKey = Permissions.Roles.View,
                            AllowedRoles = "Owner,Admin"
                        }
                    }
                },
                new SidebarMenuItem
                {
                    Category = "مدیریت و تنظیمات",
                    Key = "company_settings",
                    Title = "تنظیمات شرکت",
                    Icon = "mdi mdi-cog-outline",
                    Url = "/AdminArea/Settings/Index",
                    PermissionKey = Permissions.Tenants.View
                }
            };
        }
    }
}
