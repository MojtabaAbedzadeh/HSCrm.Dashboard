// ================================
// Helpers
// ================================

const ApiAddress = AppContext.apiAddress;
const TId = AppContext.tenantId;
const UId = AppContext.userId;
const Token = AppContext.token;

$(document).ready(function () {
    $('.persian-date').persianDatepicker({
        format: 'YYYY/MM/DD',
        autoClose: true,
        calendarSwitch: {
            "enabled": false,
            "format": "MMMM"
        },
    });
})

function formatPrice(value) {
    if (!value) return '0';
    return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
function unFormatPrice(value) {
    if (!value) return 0;
    return parseFloat(value.toString().replace(/,/g, '')) || 0;
}
function clearProductForm() {
    $('#ProductTitle').val('');
    $('#productCount').val(0);
    $('#buyPrice').val('');
    $('#discount').val(0);
    $('#tax').val(0);
    $('#rowSum').val(0);
}
// ================================
// Product Search (Autocomplete)
// ================================

let productSearchRequest = null;
let productSearchSequence = 0;

function getApiValue(object, camelCaseName, pascalCaseName) {
    return object?.[camelCaseName] ?? object?.[pascalCaseName];
}

$('#ProductTitle').on('input', function () {
    const $input = $(this);
    const text = $input.val().trim();

    $('#ProductId').val('');

    if (text.length < 2) {
        $('#productAutoComplete')
            .empty()
            .hide();

        if (productSearchRequest) {
            productSearchRequest.abort();
            productSearchRequest = null;
        }

        return;
    }

    const requestSequence = ++productSearchSequence;

    if (productSearchRequest) {
        productSearchRequest.abort();
    }

    productSearchRequest = $.ajax({
        url: ApiAddress + 'Product/Search',
        type: 'GET',
        dataType: 'json',
        data: {
            term: text
        },
        headers: {
            Authorization: 'Bearer ' + Token
        }
    })
        .done(function (res) {
            // جلوگیری از نمایش نتیجه قدیمی‌تر
            if (requestSequence !== productSearchSequence) {
                return;
            }

            const status =
                getApiValue(res, 'status', 'Status');

            const products =
                getApiValue(res, 'data', 'Data');

            if (status === false) {
                $('#productAutoComplete')
                    .empty()
                    .hide();

                console.error(
                    getApiValue(res, 'message', 'Message') ||
                    'خطا در دریافت محصولات'
                );

                return;
            }

            if (!Array.isArray(products)) {
                $('#productAutoComplete')
                    .empty()
                    .hide();

                console.error(
                    'ساختار پاسخ محصولات معتبر نیست:',
                    res
                );

                return;
            }

            const $list = $('<ul>', {
                class: 'list-group'
            });

            products.forEach(function (product) {
                const productId = getApiValue(
                    product,
                    'id',
                    'Id'
                );

                const productTitle = getApiValue(
                    product,
                    'productTitle',
                    'ProductTitle'
                ) || '';

                const buyPrice = getApiValue(
                    product,
                    'buyPrice',
                    'BuyPrice'
                ) || 0;

                const $item = $('<li>', {
                    class: 'list-group-item product-item',
                    text: productTitle + ' - ' + formatPrice(buyPrice)
                });

                // استفاده از data() از تزریق HTML جلوگیری می‌کند
                $item.data({
                    id: productId,
                    title: productTitle,
                    buy: buyPrice
                });

                $list.append($item);
            });

            $('#productAutoComplete')
                .empty()
                .append($list)
                .toggle(products.length > 0);
        })
        .fail(function (xhr, textStatus) {
            if (textStatus === 'abort') {
                return;
            }

            $('#productAutoComplete')
                .empty()
                .hide();

            console.error(
                'خطا در سرچ محصول:',
                xhr.status,
                xhr.responseText
            );
        })
        .always(function () {
            productSearchRequest = null;
        });
});

$(document).on('click', '.product-item', function () {
    const $item = $(this);

    const productId = $item.data('id');
    const productTitle = $item.data('title');
    const buyPrice = $item.data('buy');

    $('#ProductTitle').val(productTitle);
    $('#ProductId').val(productId);
    $('#buyPrice').val(formatPrice(buyPrice));

    $('#productAutoComplete')
        .empty()
        .hide();

    $('#productCount')
        .focus()
        .select();

    calculateRowSum();
});

// ================================
// Row Sum Calculation
// ================================
function calculateRowSum() {
    const qty = parseFloat($('#productCount').val()) || 0;
    const price = unFormatPrice($('#buyPrice').val());
    const discount = unFormatPrice($('#discount').val());
    const tax = unFormatPrice($('#tax').val());
    let sum = (qty * price) - discount + tax;
    if (sum < 0) sum = 0;
    $('#rowSum').val(formatPrice(sum));
}

$('#productCount, #buyPrice, #discount, #tax').on('keyup change', calculateRowSum);

// ================================
// Add / Merge Product Row
// ================================
$('#btnAddProduct').on('click', function () {

    const productId = $('#ProductId').val();
    const title = $('#ProductTitle').val();
    const qty = parseFloat($('#productCount').val()) || 0;

    if (!productId || qty <= 0) {
        swal.fire({
            title: "خطا",
            text: "محصول و تعداد را وارد کنید",
            icon: "error",
            confirmButtonColor: "green",
            confirmButtonText: "باشه",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });
        return;
    }

    const price = unFormatPrice($('#buyPrice').val());
    const discount = unFormatPrice($('#discount').val());
    const tax = unFormatPrice($('#tax').val());
    const sum = (qty * price) - discount + tax;

    let merged = false;

    $('#tblBody tr').each(function () {
        const rowProductId = $(this).find('td:eq(1)').text();
        if (rowProductId === productId) {
            const oldQty = parseFloat($(this).find('td:eq(3)').text());
            const newQty = oldQty + qty;
            const newSum = (newQty * price) - discount + tax;
            $(this).find('td:eq(3)').text(newQty);
            $(this).find('td:eq(7)').text(formatPrice(newSum));
            merged = true;
        }
    });

    if (!merged) {
        const rowIndex = $('#tblBody tr').length + 1;

        $('#tblBody').append(`
            <tr>
                <td hidden>${rowIndex}</td>
                <td hidden>${productId}</td>
                <td>${title}</td>
                <td style="text-align:center">${qty}</td>
                <td style="text-align:center">${formatPrice(price)}</td>
                <td style="text-align:center">${formatPrice(discount)}</td>
                <td style="text-align:center">${formatPrice(tax)}</td>
                <td style="text-align:center">${formatPrice(sum)}</td>
                <td style="text-align:center">
                    <button class="btn btn-sm btn-warning btnEdit">✎</button>
                    <button class="btn btn-sm btn-danger btnRemove">✖</button>
                </td>
            </tr>
        `);
    }

    clearProductForm();
    calculateInvoiceSum();
});
// ================================
// Edit / Remove Row
// ================================
$(document).on('click', '.btnEdit', function () {
    const row = $(this).closest('tr');

    $('#ProductId').val(row.find('td:eq(1)').text());
    $('#ProductTitle').val(row.find('td:eq(2)').text());
    $('#productCount').val(row.find('td:eq(3)').text());
    $('#buyPrice').val(row.find('td:eq(4)').text());
    $('#discount').val(row.find('td:eq(5)').text());
    $('#tax').val(row.find('td:eq(6)').text());
    $('#rowSum').val(row.find('td:eq(7)').text());

    row.remove();
    calculateInvoiceSum();
});
$(document).on('click', '.btnRemove', function () {
    $(this).closest('tr').remove();
    calculateInvoiceSum();
});
// ================================
// Invoice Total (Front - Display Only)
// ================================
function calculateInvoiceSum() {
    let total = 0;

    $('#tblBody tr').each(function () {
        total += unFormatPrice($(this).find('td:eq(7)').text());
    });

    total -= unFormatPrice($('#invoiceDiscount').val());
    total += unFormatPrice($('#invoiceTax').val());

    if (total < 0) total = 0;

    $('#invoiceSumPrice').val(formatPrice(total));
}
$('#invoiceDiscount, #invoiceTax').on('keyup change', calculateInvoiceSum);
// ================================
// Submit Invoice (Proforma / Final)
// ================================
$('#btnRegInvoice').on('click', function () {

    if (!validateInvoiceForm()) return;

    const items = [];

    $('#tblBody tr').each(function () {
        items.push({
            productId: parseInt($(this).find('td:eq(1)').text()),
            quantity: parseFloat($(this).find('td:eq(3)').text()),
            unitPrice: unFormatPrice($(this).find('td:eq(4)').text()),
            discount: unFormatPrice($(this).find('td:eq(5)').text()),
            tax: unFormatPrice($(this).find('td:eq(6)').text())
        });
    });

    const payment = validatePayment();
    if (!payment.isValid) return;

    const model = {
        SupplierId: parseInt($('#SupplierId').val(), 10),
        WarehouseId: parseInt($('#WarehouseId').val(), 10),
        TenantId: TId,
        InvoiceNumber: $('#invoiceNumber').val(),
        IssueDate: $('#invoiceDate').val(),
        Status: 1,
        InvoiceDiscount: unFormatPrice($('#invoiceDiscount').val()) || 0,
        InvoiceTax: unFormatPrice($('#invoiceTax').val()) || 0,
        PurchaserId: UId,
        PaidAmount: payment.paidAmount,
        PaymentMethod: payment.paymentMethod,
        PaymentReferenceNo: $('#paymentReferenceNo').val() || null,
        Items: items
    };

    $.ajax({
        url: ApiAddress + 'PurchaseInvoice/CreateInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(model),
        headers: { 'Authorization': 'Bearer ' + Token },
        success: function (status) {
            if (status.status === true) {
                swal.fire({
                    title: "موفق",
                    text: "فاکتور با موفقیت ثبت شد",
                    icon: "success",
                    confirmButtonColor: "green",
                    confirmButtonText: "باشه",
                    showConfirmButton: true,
                    timer: 1500,
                    allowOutsideClick: false
                });
                return;
            }

            swal.fire({
                title: "خطا",
                text: status.message || "ثبت فاکتور انجام نشد",
                icon: "error",
                confirmButtonColor: "green",
                confirmButtonText: "باشه",
                showConfirmButton: true,
                timer: 1500,
                allowOutsideClick: false
            });
        },
        error: function () {
            swal.fire({
                title: "خطای ارتباط با سرور",
                text: "امکان ارتباط با سرور وجود ندارد، لطفاً مجدداً تلاش کنید",
                icon: "error",
                confirmButtonColor: "green",
                showConfirmButton: true,
                timer: 1500,
                allowOutsideClick: false
            });
        }
    });

});
function validateInvoiceForm() {

    // نوع فاکتور
    if (!$('#InvoiceType').val()) {
        swal.fire({
            title: "خطا",
            text: "نوع فاکتور را انتخاب کنید",
            icon: "error",
            confirmButtonColor: "green",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });
        return false;
    }

    // انبار
    if (!$('#WarehouseId').val() || parseInt($('#WarehouseId').val()) === 0) {
        swal.fire({
            title: "خطا",
            text: "انبار را انتخاب کنید",
            icon: "error",
            confirmButtonColor: "green",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });
        return false;
    }

    // شماره فاکتور
    if (!$('#invoiceNumber').val().trim()) {
        swal.fire({
            title: "خطا",
            text: "شماره فاکتور را وارد کنید",
            icon: "error",
            confirmButtonColor: "green",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });
        return false;
    }

    // تاریخ فاکتور
    if (!$('#invoiceDate').val()) {
        swal.fire({
            title: "خطا",
            text: "تاریخ فاکتور را انتخاب کنید",
            icon: "error",
            confirmButtonColor: "green",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });
        return false;
    }

    // حداقل یک آیتم
    if ($('#tblBody tr').length === 0) {
        swal.fire({
            title: "خطا",
            text: "حداقل یک کالا باید به فاکتور اضافه شود",
            icon: "error",
            confirmButtonColor: "green",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });
        return false;
    }

    return true;
}

// ذخیره تغییرات با زدن Enter یا خارج شدن از فیلد (Blur)
$(document).on('blur keyup', '.edit-qty', function (e) {
    if (e.type === 'keyup' && e.keyCode !== 13) return;

    const $input = $(this);
    const newVal = $input.val();
    const $td = $input.parent();

    // بازگرداندن مقدار به متن
    $td.text(newVal);

    // به‌روزرسانی جمع همان ردیف (اختیاری: اگر نیاز است قیمت هم محاسبه شود)
    // در اینجا فقط جمع کل را دوباره محاسبه می‌کنیم
    calculateInvoiceSum();
});

"use strict";

/*
|--------------------------------------------------------------------------
| توابع عمومی مبلغ
|--------------------------------------------------------------------------
*/

function normalizeNumber(value) {
    if (value === null || value === undefined) {
        return 0;
    }

    let text = String(value);

    // تبدیل ارقام فارسی و عربی به انگلیسی
    text = text
        .replace(/[۰-۹]/g, function (digit) {
            return String("۰۱۲۳۴۵۶۷۸۹".indexOf(digit));
        })
        .replace(/[٠-٩]/g, function (digit) {
            return String("٠١٢٣٤٥٦٧٨٩".indexOf(digit));
        });

    // حذف جداکننده هزارگان و فاصله
    text = text
        .replace(/,/g, "")
        .replace(/٬/g, "")
        .replace(/\s/g, "");

    const result = parseFloat(text);

    if (isNaN(result)) {
        return 0;
    }

    return result;
}

function formatPrice(value) {
    const number = normalizeNumber(value);

    return number.toLocaleString("en-US", {
        maximumFractionDigits: 2
    });
}

/*
|--------------------------------------------------------------------------
| مبلغ کل فاکتور
|--------------------------------------------------------------------------
*/

function getInvoiceTotal() {
    return normalizeNumber($("#invoiceSumPrice").val());
}

function calculateInvoiceSum() {
    let total = 0;

    $("#tblBody tr").each(function () {
        /*
         * ستون شماره 7 مبلغ نهایی ردیف است.
         */
        const lineTotal = normalizeNumber($(this).find("td:eq(7)").text());

        total += lineTotal;
    });

    const invoiceDiscount =
        normalizeNumber($("#invoiceDiscount").val());

    const invoiceTax =
        normalizeNumber($("#invoiceTax").val());

    total = total - invoiceDiscount + invoiceTax;

    if (total < 0) {
        total = 0;
    }

    $("#invoiceSumPrice").val(formatPrice(total));

    updatePaymentFields();
}

/*
|--------------------------------------------------------------------------
| وضعیت پرداخت
|--------------------------------------------------------------------------
*/

function getPaymentStatus() {
    const value = parseInt($("#paymentStatus").val());

    if (isNaN(value)) {
        return 0;
    }

    return value;
}

function getPaidAmount() {
    return normalizeNumber($("#paidAmount").val());
}

function updateRemainingAmount() {
    const invoiceTotal = getInvoiceTotal();
    const paidAmount = getPaidAmount();

    const remainingAmount = Math.max(
        invoiceTotal - paidAmount,
        0
    );

    $("#remainingAmount").val(formatPrice(remainingAmount));
}

function updatePaymentFields() {
    const paymentStatus = getPaymentStatus();
    const invoiceTotal = getInvoiceTotal();

    const paidAmount = $("#paidAmount");
    const paymentMethod = $("#paymentMethod");
    const remainingAmount = $("#remainingAmount");

    if (paymentStatus === 0) {
        /*
         * بدون پرداخت
         */
        paidAmount
            .val("0")
            .prop("disabled", true);

        paymentMethod
            .val("Cash")
            .prop("disabled", true);

        remainingAmount.val(formatPrice(invoiceTotal));

        return;
    }

    if (paymentStatus === 1) {
        /*
         * پرداخت کامل
         */
        paidAmount
            .val(formatPrice(invoiceTotal))
            .prop("disabled", true);

        paymentMethod.prop("disabled", false);

        remainingAmount.val("0");

        return;
    }

    if (paymentStatus === 2) {
        /*
         * پرداخت بخشی
         */
        paidAmount.prop("disabled", false);

        paymentMethod.prop("disabled", false);

        updateRemainingAmount();

        return;
    }

    /*
     * مقدار نامعتبر؛ بازگشت به بدون پرداخت
     */
    $("#paymentStatus").val("0");

    paidAmount
        .val("0")
        .prop("disabled", true);

    paymentMethod
        .val("Cash")
        .prop("disabled", true);

    remainingAmount.val(formatPrice(invoiceTotal));
}

// در هنگام لود صفحه (document.ready) به این شکل صدا بزنید:
$(document).ready(function () {
    // True به معنی لود اولیه است و مقادیر input را نباید overwrite کند
    updatePaymentFields(true);

    // ... سایر کدها
});

/*
|--------------------------------------------------------------------------
| اعتبارسنجی پرداخت
|--------------------------------------------------------------------------
*/

function validatePayment() {
    const paymentStatus = getPaymentStatus();
    const invoiceTotal = getInvoiceTotal();

    let paidAmount = 0;

    if (paymentStatus === 0) {
        return {
            isValid: true,
            paymentStatus: 0,
            paidAmount: 0,
            paymentMethod: null
        };
    }

    if (paymentStatus === 1) {
        return {
            isValid: true,
            paymentStatus: 1,
            paidAmount: invoiceTotal,
            paymentMethod: $("#paymentMethod").val() || "Cash"
        };
    }

    if (paymentStatus === 2) {
        paidAmount = getPaidAmount();

        if (invoiceTotal <= 0) {
            swal.fire({
                title: "مبلغ فاکتور",
                text: "مبلغ کل فاکتور باید بیشتر از صفر باشد",
                icon: "warning",
                confirmButtonColor: "green",
                showConfirmButton: true,
                timer: 1500,
                allowOutsideClick: false
            });

            return {
                isValid: false
            };
        }

        if (paidAmount <= 0) {
            swal.fire({
                title: "مبلغ پرداختی",
                text: "برای پرداخت بخشی، مبلغ پرداختی را وارد کنید",
                icon: "warning",
                confirmButtonColor: "green",
                showConfirmButton: true,
                timer: 1500,
                allowOutsideClick: false
            });

            return {
                isValid: false
            };
        }

        if (paidAmount >= invoiceTotal) {
            swal.fire({
                title: "مبلغ پرداختی",
                text: "در حالت پرداخت بخشی، مبلغ باید کمتر از مبلغ کل فاکتور باشد",
                icon: "warning",
                confirmButtonColor: "green",
                showConfirmButton: true,
                timer: 1500,
                allowOutsideClick: false
            });

            return {
                isValid: false
            };
        }

        return {
            isValid: true,
            paymentStatus: 2,
            paidAmount: paidAmount,
            paymentMethod: $("#paymentMethod").val() || "Cash"
        };
    }

    swal.fire({
        title: "وضعیت پرداخت",
        text: "وضعیت پرداخت انتخاب‌شده معتبر نیست",
        icon: "warning",
        confirmButtonColor: "green",
        showConfirmButton: true,
        timer: 1500,
        allowOutsideClick: false
    });

    return {
        isValid: false
    };
}

/*
|--------------------------------------------------------------------------
| رویدادهای مربوط به پرداخت
|--------------------------------------------------------------------------
*/

$(document).on("change", "#paymentStatus", function () {
    updatePaymentFields();
});

$(document).on("input change", "#paidAmount", function () {
    const invoiceTotal = getInvoiceTotal();

    let paidAmount = normalizeNumber($(this).val());

    if (paidAmount < 0) {
        paidAmount = 0;
    }

    if (paidAmount > invoiceTotal) {
        paidAmount = invoiceTotal;

        $(this).val(formatPrice(paidAmount));

        swal.fire({
            title: "مبلغ نامعتبر",
            text: "مبلغ پرداختی نمی‌تواند بیشتر از مبلغ فاکتور باشد",
            icon: "warning",
            confirmButtonColor: "green",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });
    }

    updateRemainingAmount();
});

/*
|--------------------------------------------------------------------------
| بارگذاری اولیه
|--------------------------------------------------------------------------
*/

$(document).ready(function () {
    updatePaymentFields();

    /*
     * در صورت وجود رویدادهای محاسبه مبلغ در فایل فعلی،
     * این فیلدها باید پس از تغییر دوباره به‌روزرسانی شوند.
     */
    $(document).on(
        "input change",
        "#invoiceDiscount, #invoiceTax",
        function () {
            setTimeout(function () {
                calculateInvoiceSum();
            }, 0);
        }
    );
});

// ================================
// Update Invoice (Edit Mode)
// ================================
$('#btnUpdateInvoice').on('click', function () {

    // ۱. اعتبارسنجی فرم و لیست کالاها
    if (!validateInvoiceForm()) return;
    
    const items = [];
    $('#tblBody tr').each(function () {
        items.push({
            productId: parseInt($(this).find('td:eq(1)').text()),
            quantity: parseFloat($(this).find('td:eq(3)').text()),
            unitPrice: unFormatPrice($(this).find('td:eq(4)').text()),
            discount: unFormatPrice($(this).find('td:eq(5)').text()),
            tax: unFormatPrice($(this).find('td:eq(6)').text())
        });
    });

    // ۲. اعتبارسنجی وضعیت پرداخت
    const payment = validatePayment();
    if (!payment.isValid) return;

    const issueDateValue = "2026-08-09";

    // ۳. ساخت آبجکت نهایی (مطابق با مدل Update در بک‌اند)
    const model = {
        InvoiceId: parseInt($('#InvoiceId').val(), 10), // حتماً در ویو این فیلد مخفی را داشته باشید
        SupplierId: parseInt($('#SupplierId').val(), 10),
        WarehouseId: parseInt($('#WarehouseId').val(), 10),
        TenantId: TId,
        InvoiceNumber: $('#invoiceNumber').val(),
        IssueDate: issueDateValue,
        Status: $('#InvoiceStatus').val(),
        InvoiceDiscount: unFormatPrice($('#invoiceDiscount').val()) || 0,
        InvoiceTax: unFormatPrice($('#invoiceTax').val()) || 0,
        PurchaserId: UId,
        PaidAmount: payment.paidAmount,
        PaymentMethod: payment.paymentMethod,
        PaymentReferenceNo: $('#paymentReferenceNo').val() || null,
        Items: items
    };

    // ۴. ارسال درخواست PUT
    $.ajax({
        url: ApiAddress + 'PurchaseInvoice/UpdateInvoice',
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify(model),
        headers:
        {
            'Authorization': 'Bearer ' + Token
        },
        success: function (status) {
            if (status.Status === true) {
                swal.fire({
                    title: "موفق",
                    text: "فاکتور با موفقیت ویرایش شد",
                    icon: "success",
                    confirmButtonColor: "green",
                    confirmButtonText: "باشه",
                    showConfirmButton: true,
                    timer: 1500,
                    allowOutsideClick: false
                }).then(function () {
                    // هدایت به لیست فاکتورها پس از موفقیت
                    window.location.href = '/AdminArea/PurchaseInvoice/Index';
                });

                return;
            }

            swal.fire({
                title: "خطا",
                text: status.message || "ویرایش فاکتور انجام نشد",
                icon: "error",
                confirmButtonColor: "red",
                showConfirmButton: true,
                timer: 1500,
                allowOutsideClick: false
            });
        },
        error: function () {
            swal.fire({
                title: "خطا",
                text: "ارتباط با سرور برقرار نشد",
                icon: "error",
                confirmButtonColor: "green",
                showConfirmButton: true,
                timer: 1500,
                allowOutsideClick: false
            });
        }
    });
});