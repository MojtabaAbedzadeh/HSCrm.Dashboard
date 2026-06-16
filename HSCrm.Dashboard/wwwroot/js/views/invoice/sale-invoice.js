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
            enabled: false,
            format: "MMMM"
        }
    });
});

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
    $('#ProductId').val('');
    $('#productCount').val(0);
    $('#sellPrice').val('');
    $('#discount').val(0);
    $('#tax').val(0);
    $('#rowSum').val(0);
}

// ================================
// Product Search
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
                    data-sell="${p.sellPrice}">
                    ${p.productTitle} - ${formatPrice(p.sellPrice)}
                </li>`;

        });

        $('#productAutoComplete').html(html).show();
    });
});

$(document).on('click', '.product-item', function () {

    $('#ProductTitle').val($(this).data('title'));
    $('#ProductId').val($(this).data('id'));
    $('#sellPrice').val(formatPrice($(this).data('sell')));

    $('#productAutoComplete').hide();

    $('#productCount').focus();
    $('#productCount').select();

});

// ================================
// Row Sum
// ================================
function calculateRowSum() {

    const qty = parseFloat($('#productCount').val()) || 0;
    const price = unFormatPrice($('#sellPrice').val());
    const discount = unFormatPrice($('#discount').val());
    const tax = unFormatPrice($('#tax').val());

    let sum = (qty * price) - discount + tax;

    if (sum < 0) sum = 0;

    $('#rowSum').val(formatPrice(sum));
}

$('#productCount, #sellPrice, #discount, #tax').on('keyup change', calculateRowSum);

// ================================
// Add Product
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

    const price = unFormatPrice($('#sellPrice').val());
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
// Edit Row
// ================================
$(document).on('click', '.btnEdit', function () {

    const row = $(this).closest('tr');

    $('#ProductId').val(row.find('td:eq(1)').text());
    $('#ProductTitle').val(row.find('td:eq(2)').text());
    $('#productCount').val(row.find('td:eq(3)').text());
    $('#sellPrice').val(row.find('td:eq(4)').text());
    $('#discount').val(row.find('td:eq(5)').text());
    $('#tax').val(row.find('td:eq(6)').text());
    $('#rowSum').val(row.find('td:eq(7)').text());

    row.remove();

    calculateInvoiceSum();

});

// ================================
// Remove Row
// ================================
$(document).on('click', '.btnRemove', function () {

    $(this).closest('tr').remove();

    calculateInvoiceSum();

});

// ================================
// Invoice Total
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
// Submit Invoice
// ================================
$('#btnRegInvoice').on('click', function () {

    if (!validateInvoiceForm()) return;

    const model = {
        projectId: parseInt($('#ProjectId').val()),
        warehouseId: parseInt($('#WarehouseId').val()),
        tenantId: TId,
        invoiceNumber: $('#invoiceNumber').val(),
        issueDate: $('#invoiceDate').val(),
        status: parseInt($('#InvoiceStatus').val()),
        invoiceDiscount: unFormatPrice($('#invoiceDiscount').val()),
        invoiceTax: unFormatPrice($('#invoiceTax').val()),
        sellerUserId: UId,
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
        url: ApiAddress + 'SalesInvoice/CreateSalesInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(model),

        success: function (status) {

            if (status.status === true) {

                swal({
                    title: "موفق",
                    text: status.message || "فاکتور با موفقیت ثبت شد",
                    type: "success",
                    confirmButtonColor: "green",
                    confirmButtonText: "باشه",
                    timer: 1500
                });

                return;
            }

            switch (status.statusCode) {

                case 204:

                    swal({
                        title: "اطلاعات ناقص",
                        text: status.message || "آیتمی برای ثبت وجود ندارد",
                        type: "warning",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;

                case 400:

                    swal({
                        title: "درخواست نامعتبر",
                        text: status.message || "اطلاعات ارسالی صحیح نیست",
                        type: "warning",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;

                case 403:

                    swal({
                        title: "عدم دسترسی",
                        text: status.message || "شما مجاز به انجام این عملیات نیستید",
                        type: "error",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;

                case 404:

                    swal({
                        title: "یافت نشد",
                        text: status.message || "رکورد مورد نظر یافت نشد",
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
                        text: status.message || "خطایی در سرور رخ داده است",
                        type: "error",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        timer: 1500
                    });

                    break;
            }
        },

        error: function () {

            swal({
                title: "خطای ارتباط با سرور",
                text: "امکان ارتباط با سرور وجود ندارد، لطفاً مجدداً تلاش کنید",
                type: "error",
                confirmButtonColor: "green",
                confirmButtonText: "باشه"
            });

        }
    });

});

// ================================
// Validation
// ================================
function validateInvoiceForm() {

    if (!$('#InvoiceStatus').val()) {
        swal({ title: "خطا", text: "نوع فاکتور را انتخاب کنید", type: "error", timer: 1500 });
        return false;
    }

    if (!$('#ProjectId').val() || parseInt($('#ProjectId').val()) === 0) {
        swal({ title: "خطا", text: "پروژه را انتخاب کنید", type: "error", timer: 1500 });
        return false;
    }

    if (!$('#WarehouseId').val() || parseInt($('#WarehouseId').val()) === 0) {
        swal({ title: "خطا", text: "انبار را انتخاب کنید", type: "error", timer: 1500 });
        return false;
    }

    if (!$('#invoiceNumber').val().trim()) {
        swal({ title: "خطا", text: "شماره فاکتور را وارد کنید", type: "error", timer: 1500 });
        return false;
    }

    if (!$('#invoiceDate').val()) {
        swal({ title: "خطا", text: "تاریخ فاکتور را انتخاب کنید", type: "error", timer: 1500 });
        return false;
    }

    if ($('#tblBody tr').length === 0) {
        swal({ title: "خطا", text: "حداقل یک کالا باید به فاکتور اضافه شود", type: "error", timer: 1500 });
        return false;
    }

    return true;
}
