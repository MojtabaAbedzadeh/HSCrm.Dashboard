// ================================
// Helpers
// ================================

const ApiAddress = (window.AppContext && window.AppContext.apiAddress) || '';
const TId = (window.AppContext && window.AppContext.tenantId) || '';
const UId = (window.AppContext && window.AppContext.userId) || '';
const Token = (window.AppContext && window.AppContext.token) || '';

let currentNumberingMode = 'Hybrid';
let defaultVatPercent = 0;

$(document).ready(function () {
    $.ajaxSetup({
        beforeSend: function (xhr) {
            if (Token) {
                xhr.setRequestHeader("Authorization", `Bearer ${Token}`);
            }
        }
    });

    $('.persian-date').persianDatepicker({
        format: 'YYYY/MM/DD',
        autoClose: true,
        calendarSwitch: {
            "enabled": false,
            "format": "MMMM"
        }
    });

    if ($('#invoiceNumber').length && !$('#InvoiceId').val()) {
        $.ajax({
            url: (ApiAddress.endsWith('/') ? ApiAddress : ApiAddress + '/') + 'PurchaseInvoice/GetNextInvoiceNumber',
            type: 'GET',
            headers: Token ? { 'Authorization': `Bearer ${Token}` } : {},
            success: function (res) {
                const payload = res?.data ?? res;
                const nextNum = payload?.nextInvoiceNumber || payload?.NextInvoiceNumber || '';
                currentNumberingMode = payload?.numberingMode || 'Hybrid';
                defaultVatPercent = parseFloat(payload?.defaultVatPercent || 0) || 0;

                if (currentNumberingMode === 'Auto') {
                    $('#invoiceNumber').prop('readonly', true);
                    if (nextNum) {
                        $('#invoiceNumber').val(nextNum).attr('placeholder', `شماره خودکار: ${nextNum}`);
                    }
                } else if (currentNumberingMode === 'Manual') {
                    $('#invoiceNumber').prop('readonly', false).val('').attr('placeholder', 'شماره فاکتور را به صورت دستی وارد کنید');
                } else if (nextNum) {
                    $('#invoiceNumber').prop('readonly', false).attr('placeholder', `شماره خودکار: ${nextNum}`);
                    if (!$('#invoiceNumber').val()) {
                        $('#invoiceNumber').val(nextNum);
                    }
                }
            },
            error: function () {
                $('#invoiceNumber').attr('placeholder', 'شماره خودکار سیستم');
            }
        });
    }
});

function normalizeNumber(value) {
    if (value === null || value === undefined) {
        return 0;
    }
    let text = String(value)
        .replace(/[۰-۹]/g, digit => String("۰۱۲۳۴۵۶۷۸۹".indexOf(digit)))
        .replace(/[٠-٩]/g, digit => String("٠١٢٣٤٥٦٧٨٩".indexOf(digit)))
        .replace(/,/g, "")
        .replace(/٬/g, "")
        .replace(/\s/g, "");
    const result = parseFloat(text);
    return isNaN(result) ? 0 : result;
}

function formatPrice(value) {
    const number = normalizeNumber(value);
    return number.toLocaleString("en-US", { maximumFractionDigits: 2 });
}
function unFormatPrice(value) {
    if (!value) return 0;
    return parseFloat(value.toString().replace(/,/g, '')) || 0;
}
function clearProductForm() {
    $('#ProductTitle').val('');
    $('#ProductId').val('');
    $('#productCount').val(0);
    $('#buyPrice').val('');
    $('#discount').val(0);
    $('#tax').val(0).data('manualEdit', false);
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
    let tax = unFormatPrice($('#tax').val());
    if (defaultVatPercent > 0 && $('#tax').data('manualEdit') !== true) {
        const taxable = Math.max((qty * price) - discount, 0);
        tax = Math.round(taxable * defaultVatPercent / 100);
        $('#tax').val(formatPrice(tax));
    }
    let sum = (qty * price) - discount + tax;
    if (sum < 0) sum = 0;
    $('#rowSum').val(formatPrice(sum));
}

$('#tax').on('input', function () {
    $(this).data('manualEdit', true);
});
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
$('#invoiceDiscount, #invoiceTax').on('keyup change', function () {
    calculateInvoiceSum();
});
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
            SwalHelper.handleResponse(status, "فاکتور خرید با موفقیت ثبت شد", function () {
                window.location.href = '/AdminArea/Invoice/PurchaseInvoice';
            });
        },
        error: function (xhr) {
            let errorMsg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : "امکان ارتباط با سرور وجود ندارد، لطفاً مجدداً تلاش کنید";
            if (xhr.status === 409) {
                errorMsg = (xhr.responseJSON && xhr.responseJSON.message) || "شماره فاکتور تکراری است!";
            }
            SwalHelper.error(errorMsg, "خطا در ثبت فاکتور خرید");
        }
    });

});
function validateInvoiceForm() {

    // نوع فاکتور
    if ($('#InvoiceType').length && !$('#InvoiceType').val()) {
        SwalHelper.error("نوع فاکتور را انتخاب کنید", "خطا");
        return false;
    }

    // تأمین‌کننده
    if (!$('#SupplierId').val() || parseInt($('#SupplierId').val(), 10) === 0) {
        SwalHelper.error("تامین‌کننده را انتخاب کنید", "خطا");
        return false;
    }

    // انبار
    if (!$('#WarehouseId').val() || parseInt($('#WarehouseId').val(), 10) === 0) {
        SwalHelper.error("انبار را انتخاب کنید", "خطا");
        return false;
    }

    if (currentNumberingMode === 'Manual' && !($('#invoiceNumber').val() || '').trim()) {
        SwalHelper.error("در حالت شماره‌گذاری دستی، وارد کردن شماره فاکتور الزامی است", "خطا");
        return false;
    }

    // تاریخ فاکتور
    if (!$('#invoiceDate').val()) {
        SwalHelper.error("تاریخ فاکتور را انتخاب کنید", "خطا");
        return false;
    }

    // حداقل یک آیتم
    if ($('#tblBody tr').length === 0 || ($('#tblBody tr').length === 1 && $('#tblBody tr#empty-row').length === 1)) {
        SwalHelper.error("حداقل یک کالا باید به فاکتور اضافه شود", "خطا");
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

/* normalizeNumber & formatPrice are defined at top of file */

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

    const rawIssueDate = ($('#invoiceDate').val() || '').trim()
        .replace(/[۰-۹]/g, d => String('۰۱۲۳۴۵۶۷۸۹'.indexOf(d)))
        .replace(/[٠-٩]/g, d => String('٠١٢٣٤٥٦٧٨٩'.indexOf(d)));
    let issueDateValue = new Date().toISOString().split('T')[0];
    if (rawIssueDate) {
        const parts = rawIssueDate.split('/');
        if (parts.length === 3 && parseInt(parts[0], 10) < 1900 && typeof persianDate !== 'undefined') {
            issueDateValue = new persianDate([parseInt(parts[0], 10), parseInt(parts[1], 10), parseInt(parts[2], 10)])
                .toCalendar('gregorian')
                .toLocale('en')
                .format('YYYY-MM-DD');
        } else {
            issueDateValue = rawIssueDate.replaceAll('/', '-');
        }
    }

    // ۳. ساخت آبجکت نهایی (مطابق با مدل Update در بک‌اند)
    const model = {
        InvoiceId: parseInt($('#InvoiceId').val(), 10),
        SupplierId: parseInt($('#SupplierId').val(), 10),
        WarehouseId: parseInt($('#WarehouseId').val(), 10),
        TenantId: TId,
        InvoiceNumber: $('#invoiceNumber').val(),
        IssueDate: issueDateValue,
        Status: parseInt($('#InvoiceStatus').val(), 10) || 0,
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
            if (status.Status === true || status.status === true || status.statusCode === 200) {
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
                    window.location.href = '/AdminArea/Invoice/PurchaseInvoice';
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