// ================================
// Helpers
// ================================
const ApiAddress = AppContext.apiAddress;
const TId = AppContext.tenantId;
const UId = AppContext.userId;

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
$('#ProductTitle').on('keyup', function () {
    const text = $(this).val().trim();

    if (text.length < 2) {
        $('#productAutoComplete').hide();
        return;
    }

    $.get(ApiAddress + 'Product/Search', { term: text }, function (res) {
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

            const newSum =
                (newQty * price) - discount + tax;

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

    const model = {
        supplierId: parseInt($('#SupplierId').val()),
        warehouseId: parseInt($('#WarehouseId').val()),
        tenantId: TId,
        invoiceNumber: $('#invoiceNumber').val(),
        issueDate: $('#invoiceDate').val(),
        status: parseInt($('#InvoiceStatus').val()),
        invoiceDiscount: unFormatPrice($('#invoiceDiscount').val()),
        invoiceTax: unFormatPrice($('#invoiceTax').val()),
        PurchaserId: UId,
        items: []
    };


    $('#tblBody tr').each(function () {
        model.items.push({
            productId: parseInt($(this).find('td:eq(1)').text()),
            quantity: parseFloat($(this).find('td:eq(3)').text()),
            unitPrice: unFormatPrice($(this).find('td:eq(4)').text()),
            discount: unFormatPrice($(this).find('td:eq(5)').text()),
            tax: unFormatPrice($(this).find('td:eq(6)').text())
        });
    });

    $.ajax({
        url: ApiAddress + 'PurchaseInvoice/CreateInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(model),

        success: function (status) {
            // ✅ اگر عملیات موفق بوده
            if (status.status === true) {
                swal({
                    title: "موفق",
                    text: "فاکتور با موفقیت ثبت شد",
                    type: "success",
                    confirmButtonColor: "green",
                    confirmButtonText: "باشه",
                    timer: 1500
                });

                // اختیاری: ریدایرکت یا ریست فرم
                // location.href = '/PurchaseInvoice/List';
                // resetForm();

                return;
            }

            // ❌ اگر Status=false ولی HTTP 200 بوده
            switch (status.statusCode) {

                case 204:

                    swal({
                        title: "اطلاعات ناقص",
                        text: "آیتمی برای ثبت وجود ندارد",
                        type: "warning",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;

                case 400:

                    swal({
                        title: "درخواست نامعتبر",
                        text: "اطلاعات ارسالی صحیح نیست",
                        type: "warning",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;

                case 403:

                    swal({
                        title: "عدم دسترسی",
                        text: "شما مجاز به انجام این عملیات نیستید",
                        type: "error",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;

                case 404:

                    swal({
                        title: "یافت نشد",
                        text: "رکورد مورد نظر یافت نشد",
                        type: "error",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;

                case 406:

                    swal({
                        title: "کمبود موجودی",
                        text: "موجودی محصول کافی نمی‌باشد!",
                        type: "warning",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;

                default:

                    swal({
                        title: "خطای سرور",
                        text: "خطایی در سرور رخ داده است",
                        type: "error",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;
            }
        },

        error: function (xhr) {
            // ❌ خطاهای واقعی HTTP (500، 502، Timeout و ...)
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
