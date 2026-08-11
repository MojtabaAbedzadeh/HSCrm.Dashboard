// ================================
// Helpers
// ================================
const ApiAddress = AppContext.apiAddress;
const TId = AppContext.tenantId;
const UId = AppContext.userId;
const Token = AppContext.token;

$(document).ready(function () {
    const apiAddress = window.AppContext.apiAddress;
    const token = window.AppContext.token;

    // ۱. اتوکامپلیت جستجوی محصول
    $("#ProductTitle").on("input", function () {
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
    $('#ProductTitle').on('keyup', function () {
        const text = $(this).val().trim();

        if (text.length < 2) {
            $('#productAutoComplete').hide();
            return;
        }

        $.ajax({
            url: ApiAddress + 'Product/Search',
            type: 'GET',
            data: { term: text },
            headers: {
                'Authorization': 'Bearer ' + Token
            },
            success: function (res) {
                let html = '';
                res.forEach(p => {
                    html += `
                    <li class="list-group-item product-item"
                        data-id="${p.id}"
                        data-title="${p.productTitle}"
                        data-buy="${p.buyPrice}">
                        ${p.productTitle} - ${formatPrice(p.buyPrice)}
                    </li>`;
                });
                $('#productAutoComplete').html(html).show();
            },
            error: function (xhr) {
                console.error("خطا در سرچ محصول:", xhr.status, xhr.responseText);
            }
        });
    });

    $(document).on('click', '.product-item', function () {
        $('#ProductTitle').val($(this).data('title'));
        $('#ProductId').val($(this).data('id'));
        $('#buyPrice').val(formatPrice($(this).data('buy')));
        $('#productAutoComplete').hide();
        $('#productCount').focus();
        $('#productCount').select();
    });

    $(document).on("click", "#productAutoComplete li", function () {
        const id = $(this).data("id");
        const price = $(this).data("price");
        const title = $(this).text();

        $("#ProductTitle").val(title);
        $("#ProductId").val(id);
        $("#buyPrice").val(price);
        $("#productCount").val(1);
        $("#discount").val(0);
        $("#tax").val(0);

        calculateRowSum();
        $("#productAutoComplete").hide().empty();
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
            swal({
                title: "خطا",
                text: "محصول و تعداد را وارد کنید",
                type: "error",
                confirmButtonColor: "green",
                confirmButtonText: "باشه",
                timer: 1500
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
            Status: parseInt($('#InvoiceStatus').val(), 10),
            InvoiceDiscount: unFormatPrice($('#invoiceDiscount').val()) || 0,
            InvoiceTax: unFormatPrice($('#invoiceTax').val()) || 0,
            PurchaserId: UId,
            PaidAmount: payment.paidAmount,
            PaymentMethod: payment.paymentMethod,
            PaymentReferenceNo: $('#paymentReferenceNo').val() || null,
            Items: items
        };

        console.log('payload', model);

        $.ajax({
            url: ApiAddress + 'PurchaseInvoice/CreateInvoice',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            headers: { 'Authorization': 'Bearer ' + Token },
            success: function (status) {
                if (status.status === true) {
                    swal({
                        title: "موفق",
                        text: "فاکتور با موفقیت ثبت شد",
                        type: "success",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });
                    return;
                }

                swal({
                    title: "خطا",
                    text: status.message || "ثبت فاکتور انجام نشد",
                    type: "error",
                    confirmButtonColor: "green",
                    confirmButtonText: "باشه",
                    timer: 1500
                });
            },
            error: function () {
                swal({
                    title: "خطای ارتباط با سرور",
                    text: "امکان ارتباط با سرور وجود ندارد، لطفاً مجدداً تلاش کنید",
                    type: "error",
                    timer: 1500
                });
            }
        });

    });
    function validateInvoiceForm() {

        // نوع فاکتور
        if (!$('#InvoiceStatus').val()) {
            swal({
                title: "خطا",
                text: "نوع فاکتور را انتخاب کنید",
                type: "error",
                timer: 1500
            });
            return false;
        }

        // انبار
        if (!$('#WarehouseId').val() || parseInt($('#WarehouseId').val()) === 0) {
            swal({
                title: "خطا",
                text: "انبار را انتخاب کنید",
                type: "error",
                timer: 1500
            });
            return false;
        }

        // شماره فاکتور
        if (!$('#invoiceNumber').val().trim()) {
            swal({
                title: "خطا",
                text: "شماره فاکتور را وارد کنید",
                type: "error",
                timer: 1500
            });
            return false;
        }

        // تاریخ فاکتور
        if (!$('#invoiceDate').val()) {
            swal({
                title: "خطا",
                text: "تاریخ فاکتور را انتخاب کنید",
                type: "error",
                timer: 1500
            });
            return false;
        }

        // حداقل یک آیتم
        if ($('#tblBody tr').length === 0) {
            swal({
                title: "خطا",
                text: "حداقل یک کالا باید به فاکتور اضافه شود",
                type: "error",
                timer: 1500
            });
            return false;
        }

        return true;
    }

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
                swal({
                    title: "مبلغ فاکتور",
                    text: "مبلغ کل فاکتور باید بیشتر از صفر باشد",
                    type: "warning",
                });

                return {
                    isValid: false
                };
            }

            if (paidAmount <= 0) {
                swal({
                    title: "مبلغ پرداختی",
                    text: "برای پرداخت بخشی، مبلغ پرداختی را وارد کنید",
                    type: "warning",
                });

                return {
                    isValid: false
                };
            }

            if (paidAmount >= invoiceTotal) {
                swal({
                    title: "مبلغ پرداختی",
                    text: "در حالت پرداخت بخشی، مبلغ باید کمتر از مبلغ کل فاکتور باشد",
                    type: "warning",
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

        swal({
            title: "وضعیت پرداخت",
            text: "وضعیت پرداخت انتخاب‌شده معتبر نیست",
            type: "warning",
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

            swal({
                title: "مبلغ نامعتبر",
                text: "مبلغ پرداختی نمی‌تواند بیشتر از مبلغ فاکتور باشد",
                type: "warning",
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