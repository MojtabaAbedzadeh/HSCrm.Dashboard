using HSCrm.Dashboard.Costants;
using HSCrm.Dashboard.Models;

public static class SidebarMenuProvider
{
    public static List<SidebarMenuItem> GetMenus()
    {
        return new List<SidebarMenuItem>
        {
            new()
            {
                Key = "dashboard",
                Title = "داشبورد",
                Icon = "mdi mdi-view-dashboard-outline",
                Url = "/AdminArea/Home/Index"
            },

            new()
            {
                Key = "customers",
                Title = "مشتری‌ها",
                Icon = "mdi mdi-account-group-outline",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "customers_list",
                        Title = "لیست مشتری‌ها",
                        Url = "/AdminArea/Customer",
                        PermissionKey = Permissions.Customers.View
                    }
                }
            },

            new()
            {
                Key = "suppliers",
                Title = "تأمین‌کنندگان",
                Icon = "mdi mdi-truck-outline",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "suppliers_list",
                        Title = "لیست تأمین‌کنندگان",
                        Url = "/AdminArea/Supplier",
                        PermissionKey = Permissions.Suppliers.View
                    }
                }
            },

            new()
            {
                Key = "users",
                Title = "کاربران",
                Icon = "mdi mdi-account-key-outline",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "users_list",
                        Title = "لیست کاربران",
                        Url = "/AdminArea/User",
                        PermissionKey = Permissions.Users.View
                    },
                    new SidebarMenuItem
                    {
                        Key = "roles_list",
                        Title = "نقش‌ها",
                        Url = "/AdminArea/Role",
                        PermissionKey = Permissions.Roles.View
                    },
                    new SidebarMenuItem
                    {
                        Key = "role_permissions",
                        Title = "مجوزهای نقش‌ها",
                        Url = "/AdminArea/RolePermission",
                        PermissionKey = Permissions.RolePermissions.View
                    }
                }
            },

            new()
            {
                Key = "products",
                Title = "محصولات",
                Icon = "mdi mdi-package-variant-closed",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "products_list",
                        Title = "لیست محصولات",
                        Url = "/AdminArea/Product",
                        PermissionKey = Permissions.Products.View
                    },
                    new SidebarMenuItem
                    {
                        Key = "product_prices",
                        Title = "قیمت محصولات",
                        Url = "/AdminArea/ProductPrice",
                        PermissionKey = Permissions.ProductPrices.View
                    }
                }
            },

            new()
            {
                Key = "inventories",
                Title = "انبارها",
                Icon = "mdi mdi-warehouse",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "inventories_list",
                        Title = "لیست انبارها",
                        Url = "/AdminArea/Inventory",
                        PermissionKey = Permissions.Inventories.View
                    }
                }
            },

            new()
            {
                Key = "projects",
                Title = "پروژه‌ها",
                Icon = "mdi mdi-briefcase-outline",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "projects_list",
                        Title = "لیست پروژه‌ها",
                        Url = "/AdminArea/Project",
                        PermissionKey = Permissions.Projects.View
                    }
                }
            },

            new()
            {
                Key = "invoices",
                Title = "فاکتورها",
                Icon = "mdi mdi-file-document-multiple-outline",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "sales_invoices",
                        Title = "فاکتور فروش",
                        Url = "/AdminArea/Invoice/SalesInvoice",
                        PermissionKey = Permissions.SalesInvoices.View
                    },
                    new SidebarMenuItem
                    {
                        Key = "purchase_invoices",
                        Title = "فاکتور خرید",
                        Url = "/AdminArea/Invoice/PurchaseInvoice",
                        PermissionKey = Permissions.PurchaseInvoices.View
                    }
                }
            },

            new()
            {
                Key = "expenses",
                Title = "هزینه‌ها",
                Icon = "mdi mdi-cash",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "expenses_list",
                        Title = "لیست هزینه‌ها",
                        Url = "/AdminArea/Expense",
                        PermissionKey = Permissions.Expenses.View
                    }
                }
            },

            new()
            {
                Key = "payments",
                Title = "پرداخت‌ها",
                Icon = "mdi mdi-credit-card-outline",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "payments_list",
                        Title = "لیست پرداخت‌ها",
                        Url = "/AdminArea/Payment",
                        PermissionKey = Permissions.Payments.View
                    }
                }
            },

            new()
            {
                Key = "fiscalyears",
                Title = "سال‌های مالی",
                Icon = "mdi mdi-calendar-range",
                Children =
                {
                    new SidebarMenuItem
                    {
                        Key = "fiscalyears_list",
                        Title = "لیست سال‌های مالی",
                        Url = "/AdminArea/FiscalYear",
                        PermissionKey = Permissions.FiscalYears.View
                    }
                }
            }
        };
    }
}
